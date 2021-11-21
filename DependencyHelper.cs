using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Threading;

namespace ModbusDeviceEmulator
{
    /// <summary>
    /// Helper functions for creating Dependency Properties, which can be quickly accessed and updated in other threads.
    /// </summary>
    public static class DependencyHelper
    {
        public static T GetValue<T>(this DependencyObject d, DependencyProperty p, T shadow)
        {
            if (Thread.CurrentThread == d.Dispatcher.Thread)
            {
                return (T)d.GetValue(p);
            }
            else
            {
                return shadow;
            }
        }

        public static void SetValue<T>(this DependencyObject d, DependencyProperty p, ref T shadow, T value) where T : System.IEquatable<T>
        {
            if (!value.Equals(shadow))
            {
                shadow = value;
                if (Thread.CurrentThread == d.Dispatcher.Thread)
                {
                    d.SetValue(p, value);
                }
                else
                {
                    d.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        d.SetValue(p, value);
                    }));
                }
            }
        }
    }
}
