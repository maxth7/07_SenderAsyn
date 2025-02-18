using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Xml.Linq;



namespace emulator
{
    internal class SerialPortAsyn
    {
        private readonly TextBox _outputTextBox;
        private readonly SerialPort _serialPort;

        public SerialPortAsyn(TextBox outputTextBox, string PortName) 
        {
            _outputTextBox = outputTextBox;

            _serialPort = new SerialPort(PortName)
               {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                
                Encoding = Encoding.UTF8
            };
           
        }

        public bool IsOpen => _serialPort.IsOpen;

        public void Open()
        {
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
        }
        public void Close()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public async Task WriteAsync(string data)
        {
            if (_serialPort.BaseStream != null)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                await _serialPort.BaseStream.WriteAsync(buffer, 0, buffer.Length);
                UpdateTextBox($"Line sent: {data}\r\n");
            }
        }

        public async Task<string> ReadAsync()
        {
            if (_serialPort.BaseStream != null)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, buffer.Length);

                string strRead = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                UpdateTextBox($"Line sent: {strRead}\r\n");
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
            }
            return string.Empty;
        }


        private void UpdateTextBox(string message)
        {
            if (_outputTextBox.InvokeRequired)
            {
                _outputTextBox.Invoke(new Action(() => _outputTextBox.AppendText(message)));
            }
            else
            {
                _outputTextBox.AppendText(message);
            }
        }
        
    }

}
