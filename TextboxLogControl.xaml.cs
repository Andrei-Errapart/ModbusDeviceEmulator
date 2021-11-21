using System;
using System.Collections.Generic;
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

namespace ModbusDeviceEmulator
{
    /// <summary>
    /// Logging textbox with helper functions to append text and clear the log.
    /// </summary>
    public partial class TextboxLogControl : UserControl
    {
        public TextboxLogControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Append line to the textbox.
        /// This can be called from any thread.
        /// </summary>
        /// <param name="line"></param>
        public void AppendLine(string line)
        {
            if (System.Threading.Thread.CurrentThread == Dispatcher.Thread)
            {
                textboxLog.AppendText(line + "\r\n");
            }
            else
            {
                // FIXME: what about EndInvoke?
                Dispatcher.BeginInvoke(new Action(() => { textboxLog.AppendText(line + "\r\n"); }));
            }
        }

        /// <summary>
        /// Clear the log.
        /// This can be called from any thread.
        /// </summary>
        public void Clear()
        {
            if (System.Threading.Thread.CurrentThread == Dispatcher.Thread)
            {
                textboxLog.Clear();
            }
            else
            {
                // FIXME: what about EndInvoke?
                Dispatcher.BeginInvoke(new Action(() => { textboxLog.Clear(); }));
            }
        }

        void Button_Clear_Click(object sender, RoutedEventArgs e)
        {
            textboxLog.Clear();
        }
    }
}
