using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace ModbusDeviceEmulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string DataDirectory = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Errapart Engineering"), "ModbusDeviceEmulator");
        public static string RegisterValuesFilename = "setup.xml.ini"; // Path.Combine(DataDirectory, "RegisterValues.ini");

        internal static bool? _isInDesignMode;
        /// <summary>
        /// Gets a value indicating whether the control is in design mode (running in Blend
        /// or Visual Studio).
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
#if SILVERLIGHT
                    _isInDesignMode = System.ComponentModel.DesignerProperties.IsInDesignTool;
#else
                    var prop = System.ComponentModel.DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool)System.ComponentModel.DependencyPropertyDescriptor
                        .FromProperty(prop, typeof(FrameworkElement))
                        .Metadata.DefaultValue;
#endif
                }

                return _isInDesignMode.Value;
            }
        }
    }
}
