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
using System.Windows.Threading;

using System.Xml;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ModbusDeviceEmulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // ==========================================================================================
        IPAddress _address = IPAddress.Loopback;
        int _port = 1502;
        IPEndPoint _localEP;
        Socket _listener;

        IAsyncResult _listen_result;
        List<ModbusSlave> _modbus_slaves = new List<ModbusSlave>();
        Dictionary<byte, IModbusDataStore> _modbus_datastore = new Dictionary<byte, IModbusDataStore>();
        List<IOModuleControl> _modbus_devices = new List<IOModuleControl>();
        DispatcherTimer _timer = null;

        // ==========================================================================================
        static int _FetchIntegerAttribute(XmlTextReader reader, string attribute_name)
        {
            return int.Parse(reader.GetAttribute(attribute_name));
        }

        // ==========================================================================================
        static IOType _TypeOfString(string s)
        {
            if (string.Equals(s, "input", StringComparison.CurrentCultureIgnoreCase))
            {
                return IOType.Input;
            }
            if (string.Equals(s, "output", StringComparison.CurrentCultureIgnoreCase))
            {
                return IOType.Output;
            }
            if (string.Equals(s, "input_register", StringComparison.CurrentCultureIgnoreCase))
            {
                return IOType.InputRegister;
            }
            if (string.Equals(s, "holding_register", StringComparison.CurrentCultureIgnoreCase))
            {
                return IOType.HoldingRegister;
            }
            throw new ApplicationException("Unknown IO type: '" + s + "', expected one of 'input', 'output', 'input_register' or 'holding_register'.");
        }

        // ==========================================================================================
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.IsInDesignMode)
            {
                // Skip the action when only designing.
                return;
            }

            // 1. Parse PLC configuration.
            try
            {
                using (var Input = System.IO.File.OpenRead("setup.xml"))
                {
                    // 1. Parse the input.
                    using (var reader = new XmlTextReader(Input))
                    {
                        IOModuleControl modbus_device = null;
                        while (reader.Read())
                        {
                            // 1. Devices.
                            if (reader.Name == "device")
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        int ios_device_address = _FetchIntegerAttribute(reader, "address");
                                        modbus_device = new IOModuleControl();
                                        modbus_device.Address = ios_device_address;
                                        // YEE!
                                        gridDevices.Children.Add(modbus_device);
                                        _modbus_devices.Add(modbus_device);
                                        _modbus_datastore.Add((byte)modbus_device.Address, modbus_device);
                                        break;
                                    case XmlNodeType.EndElement:
                                        modbus_device = null;
                                        break;
                                }
                            }
                            // 2. Signals.
                            else if (reader.NodeType == XmlNodeType.Element && reader.Name == "signal")
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (modbus_device != null)
                                        {
                                            int ios_id = _FetchIntegerAttribute(reader, "id");
                                            string ios_name = reader.GetAttribute("name") ?? "";
                                            string ios_type_name = reader.GetAttribute("type");
                                            int ios_ioindex = _FetchIntegerAttribute(reader, "ioindex");
                                            string ios_description = reader.GetAttribute("description") ?? "";
                                            string ios_text0 = reader.GetAttribute("text0") ?? "0";
                                            string ios_text1 = reader.GetAttribute("text1") ?? "1";
                                            IOType ios_type = _TypeOfString(ios_type_name);
                                            var pin = new IOPin()
                                            {
                                                IOType = ios_type,
                                                Number = ios_ioindex,
                                                IsInput = ios_type == IOType.Input || ios_type == IOType.InputRegister,
                                                SignalName = ios_name,
                                                SignalDescription = ios_description,
                                                Value1bit = 0,
                                                Value16bit = 0,
                                            };
                                            modbus_device.Pins.Add(pin);
                                        }
                                        // ios_id, ios_name, false, ios_type.Value, ios_device, ios_ioindex, ios_description, ios_text0, ios_text1);
                                        break;
                                    case XmlNodeType.EndElement:
                                        break;
                                }
                            }
                            // 3. End of signals.
                            else if (reader.Name == "signal")
                            {
                            }
                        }
                    }
                }

                if (gridDevices.Children.Count == 1)
                {
                    gridDevices.Rows = 1;
                    gridDevices.Columns = 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                App.Current.Shutdown();
            }


            // Reload saved values, if any.
            try
            {
                FileIO.Load(App.RegisterValuesFilename, _modbus_devices);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            // 3. Start the server.
            try
            {
                // create and start the TCP slave
                _localEP = new IPEndPoint(IPAddress.Any, _port);
                _listener = new Socket(_localEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(_localEP);
                _listener.Listen(10);

                _listen_result = _listener.BeginAccept(_SocketAcceptCallback, this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                App.Current.Shutdown();
            }

            // 4. Start the timer.
            _timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0), DispatcherPriority.Background, _Timer_Tick, this.Dispatcher);
            _timer.IsEnabled = true;
        }

        // ==========================================================================================
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists(App.DataDirectory))
                {
                    System.IO.Directory.CreateDirectory(App.DataDirectory);
                }
                FileIO.Store(App.RegisterValuesFilename, _modbus_devices);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        // ==========================================================================================
        static void _SocketAcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            var self = ar.AsyncState as MainWindow;
            Socket conn = self._listener.EndAccept(ar);


            // Create the state object.
            var ms = new ModbusSlave(conn, self._modbus_datastore, self.log);
            self._modbus_slaves.Add(ms);

            // New stuff...
            self._listen_result = self._listener.BeginAccept(_SocketAcceptCallback, self);
        }

        // ==========================================================================================
        void _Timer_Tick(object sender, EventArgs e)
        {
            foreach (var d in _modbus_devices)
            {
                foreach (var p in d.Pins)
                {
                    if (p.IOType == IOType.InputRegister || p.IOType == IOType.HoldingRegister)
                    {
                        int sum = p.ChangePerMinuteAccumulator + p.ChangePerMinute;
                        int this_round = sum / 60;
                        if (this_round != 0)
                        {
                            p.Value16bit = (p.Value16bit + this_round) & 0xFFFF;
                            p.ChangePerMinuteAccumulator = sum - this_round * 60;
                        }
                        else
                        {
                            p.ChangePerMinuteAccumulator = sum;
                        }
                    }
                }
            }
        }
    }
}
