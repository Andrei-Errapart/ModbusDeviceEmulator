using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace ModbusDeviceEmulator
{
    /// <summary>
    /// Interaction logic for IOModuleControl.xaml
    /// </summary>
    [ContentProperty("Pins")]
    public partial class IOModuleControl : UserControl, IModbusDataStore
    {
        public int Address { get; set; }

        /// <summary>
        /// Groupbox header.
        /// </summary>
        public string Header { get; set; }


        /// <summary>
        /// Is the module connected? Default is true (connected).
        /// </summary>
        public bool? Connected { get; set; }

        public bool IsConnected { get { return Connected == true; } }

        /// <summary>
        /// List of IO pins.
        /// </summary>
        public ObservableCollection<IOPin> Pins { get; set; }
        
        public IOModuleControl()
        {
            Pins = new ObservableCollection<IOPin>();
            Connected = true;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        private void Button_SetTo1_Click(object sender, RoutedEventArgs e)
        {
            _ActOnSenderPin(sender, (IOPin pin) => { pin.Value1bit = 1; });
        }

        private void Button_SetTo0_Click(object sender, RoutedEventArgs e)
        {
            _ActOnSenderPin(sender, (IOPin pin) => { pin.Value1bit = 0; });
        }

        private void Button_IncCount_Click(object sender, RoutedEventArgs e)
        {
            _ActOnSenderPin(sender, (IOPin pin) => { pin.Value16bit += 1; });
        }

        private void Button_DecCount_Click(object sender, RoutedEventArgs e)
        {
            _ActOnSenderPin(sender, (IOPin pin) => { pin.Value16bit -= 1; });
        }

        private void _ActOnSenderPin(object sender, Action<IOPin> act)
        {
            var button = sender as Button;
            if (button != null)
            {
                var pin = button.Tag as IOPin;
                if (pin != null)
                {
                    act(pin);
                }
            }
        }

        static UInt64 _AlldoneMask(int StartBit, int Count)
        {
            return ((1UL << (StartBit + Count)) - 1UL) - ((1UL << StartBit) - 1UL);
        }

        byte _ReadBits(byte[] dst, int dst_offset, int Address, int Count, IOType ExpectedType)
        {
            UInt64 done_mask = 0;
            UInt64 alldone_mask = _AlldoneMask(0, Count);
            int nbytes = (Count + 7) / 8;

            // 1. Clear bitarea.
            for (int i = 0; i < nbytes; ++i)
            {
                dst[dst_offset + i] = 0;
            }

            // 2. Copy input.
            foreach (var pin in Pins)
            {
                var ofs = pin.Number - Address;
                if (pin.IOType == ExpectedType && ofs >= 0 && ofs < Count)
                {
                    int bitval = pin.Value1bit << (ofs & 7);
                    dst[dst_offset + ofs/8] |= (byte)bitval;
                    done_mask |= (1UL << ofs);
                }
            }
            return done_mask == alldone_mask ? (byte)0 : ModbusSlave.EXCEPTION_ILLEGAL_DATA_ADDRESS;
        }

        byte _ReadRegisters(byte[] dst, int dst_offset, int RegisterAddress, int Count)
        {
            UInt64 done_mask = 0;
            UInt64 alldone_mask = _AlldoneMask(0, Count);
            foreach (var pin in Pins)
            {
                var ofs = pin.Number - RegisterAddress;
                if ((pin.IOType == IOType.InputRegister || pin.IOType==IOType.HoldingRegister) && ofs >= 0 && ofs < Count)
                {
                    ModbusFormat.Bytes_Of_UInt16(dst, dst_offset + 2 * ofs, (ushort)pin.Value16bit);
                    done_mask |= (1UL << ofs);
                }
            }
            return done_mask == alldone_mask ? (byte)0 : ModbusSlave.EXCEPTION_ILLEGAL_DATA_ADDRESS;
        }

        public byte ReadCoils(byte[] dst, int dst_offset, int CoilAddress, int Count)
        {
            var r = _ReadBits(dst, dst_offset, CoilAddress, Count, IOType.Output);
            return r;
        }

        public byte ReadDiscreteInputs(byte[] dst, int dst_offset, int InputAddress, int Count)
        {
            var r = _ReadBits(dst, dst_offset, InputAddress, Count, IOType.Input);
            return r;
        }

        public byte ReadHoldingRegisters(byte[] dst, int dst_offset, int RegisterAddress, int Count)
        {
            var r = _ReadRegisters(dst, dst_offset, RegisterAddress, Count);
            return r;
        }

        public byte ReadInputRegisters(byte[] dst, int dst_offset, int RegisterAddress, int Count)
        {
            var r = _ReadRegisters(dst, dst_offset, RegisterAddress, Count);
            return r;
        }

        public byte WriteSingleCoil(int CoilAddress, bool Value)
        {
            foreach (var pin in Pins)
            {
                if (!pin.IsInput && pin.Number==CoilAddress)
                {
                    pin.Value1bit = Value ? 1 : 0;
                    return 0; // success, it is.
                }
            }
            return ModbusSlave.EXCEPTION_ILLEGAL_DATA_ADDRESS;
        }

        public byte WriteMultipleCoils(int CoilAddress, int Values, int Count)
        {
            UInt64 done_mask = 0;
            UInt64 alldone_mask = _AlldoneMask(CoilAddress, Count);
            foreach (var pin in Pins)
            {
                var ofs = pin.Number - CoilAddress;
                if (!pin.IsInput && ofs >= 0 && ofs < Count)
                {
                    // ModbusFormat.Bytes_Of_UInt16(dst, dst_offset + 2 * ofs, (ushort)pin.Count);
                    pin.Value1bit = (Values >> ofs) & 1;
                    done_mask |= (1UL << ofs);
                }
            }
            return done_mask == alldone_mask ? (byte)0 : ModbusSlave.EXCEPTION_ILLEGAL_DATA_ADDRESS;
        }

        public byte WriteSingleHoldingRegister(int RegisterAddress, int Value)
        {
            foreach (var pin in Pins)
            {
                if (pin.IOType==IOType.HoldingRegister && pin.Number==RegisterAddress)
                {
                        pin.Value16bit = Value;
                        return 0; // success, it is.
                }
            }
            return ModbusSlave.EXCEPTION_ILLEGAL_DATA_ADDRESS;
        }
    }
}
