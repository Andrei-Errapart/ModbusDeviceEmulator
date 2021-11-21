using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ModbusDeviceEmulator
{
    public class SelectTemplateByIOType : DataTemplateSelector
    {
        public DataTemplate OutputTemplate { get; set; }
        public DataTemplate InputTemplate { get; set; }
        public DataTemplate InputRegisterTemplate { get; set; }
        public DataTemplate HoldingRegisterTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item.GetType() == typeof(IOPin))
            {
                var pin = item as IOPin;
                var iot = pin.IOType;
                switch (iot)
                {
                    case IOType.Output:
                        return OutputTemplate;
                    case IOType.Input:
                        return InputTemplate;
                    case IOType.InputRegister:
                        return InputRegisterTemplate;
                    case IOType.HoldingRegister:
                        return HoldingRegisterTemplate;
                }
            }
            return null;
        }
    }
}
