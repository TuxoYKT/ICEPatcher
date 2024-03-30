using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICEPatcher
{
    public static class Logger
    {
        private static TextBox _textBox;

        public static void SetTextBox(TextBox textBox)
        {
            _textBox = textBox;
        }

        public static void Log(string message)
        {
            // _textBox.Invoke((MethodInvoker)delegate { _textBox.AppendText(message + Environment.NewLine); ; });
        }

        public static void Append(string message)
        { 
            // remove THE last new line before appending
            message = message.Substring(0, message.Length - Environment.NewLine.Length);

            _textBox.AppendText(message); 
        }
        public static void Clear()
        { 
            _textBox.Clear(); 
        }
    }
}
