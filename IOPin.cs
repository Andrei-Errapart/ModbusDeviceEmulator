using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace ModbusDeviceEmulator
{
    public enum IOType : int
    {
        /// <summary>1-bit output (coil).</summary>
        Output = 0,
        /// <summary>1-bit input.</summary>
        Input = 1,
        /// <summary>16-bit input register (counter).</summary>
        InputRegister = 2,
        /// <summary>16-bit holding register.</summary>
        HoldingRegister = 3,
    }

    public class IOPin : DependencyObject
    {
        public IOType IOType { get; set; }
        public int Number { get; set; }
        public bool IsInput { get; set; }

        string _PinName = null;
        public string PinName
        {
            get {
                if (_PinName == null)
                {
                    switch (IOType)
                    {
                        case IOType.Output:
                            _PinName = "DO" + Number;
                            break;
                        case IOType.Input:
                            _PinName = "DI" + Number;
                            break;
                        case IOType.InputRegister:
                            _PinName = "IR" + Number;
                            break;
                        case IOType.HoldingRegister:
                            _PinName = "HR" + Number;
                            break;
                    }
                }
                return _PinName;
            }
        }

        public string SignalName { get; set; }

        public string SignalDescription { get; set; }

        // ==============================================================================================================
        #region PROPERTY: Value1bit
        private int _Value1bit;
        private static void _Value1bitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var x = obj as IOPin;
            if (x != null)
            {
                x._Value1bit = (int)args.NewValue;
            }
        }

        public int Value1bit
        {
            get { return DependencyHelper.GetValue<int>(this, Value1bitProperty, _Value1bit); }
            set { DependencyHelper.SetValue<int>(this, Value1bitProperty, ref _Value1bit, value); }
        }
        public static readonly DependencyProperty Value1bitProperty = DependencyProperty.Register("Value1bit", typeof(int), typeof(IOPin), new PropertyMetadata(0, _Value1bitChanged));
        #endregion PROPERTY: Value1bit

        // ==============================================================================================================
        #region PROPERTY: Value16bit
        private int _Value16bit;
        private static void _Value16bitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var x = obj as IOPin;
            if (x != null)
            {
                x._Value16bit = (int)args.NewValue;
            }
        }

        public int Value16bit
        {
            get { return DependencyHelper.GetValue<int>(this, Value16bitProperty, _Value16bit); }
            set { DependencyHelper.SetValue<int>(this, Value16bitProperty, ref _Value16bit, value); }
        }
        public static readonly DependencyProperty Value16bitProperty = DependencyProperty.Register("Value16bit", typeof(int), typeof(IOPin), new PropertyMetadata(0, _Value16bitChanged));
        #endregion PROPERTY: Value16bit

        // ==============================================================================================================
        public int ChangePerMinute
        {
            get { return (int)GetValue(ChangePerMinuteProperty); }
            set { SetValue(ChangePerMinuteProperty, value); }
        }
        public static readonly DependencyProperty ChangePerMinuteProperty = DependencyProperty.Register("ChangePerMinute", typeof(int), typeof(IOPin), new PropertyMetadata(0));

        // ==============================================================================================================
        public int ChangePerMinuteAccumulator = 0;
    }
}
