using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Security.Policy;
using  System.Windows.Forms;

using Serilog;
using Serilog.Filters.Expressions;
using CustomCheckBoxLibrary;
namespace emulator
{
    public partial class FormEmulator : Form
    {
        private System.Windows.Forms.Timer blinkTimer;
        private readonly Color originalCircleColor; 
        private readonly Color originalInnerCircleColor;

        private SerialPortAsyn _serialPortSender;
     
        string dataToSend = "Hello port 3 from port 4!";
        byte[] byteData = { 0x01, 0x02, 0x03 }; 
        string NameReceivingPort;
        string NameSendingPort="";

        bool enableLogging = true;
        string msg = "";
        public FormEmulator()
        {
            InitializeComponent();
                        
            NameSendingPort = comboBoxNameSendingPort.Text;

            textBoxSender.Enabled = true;

            textBoxMessages.Text = "";
            textBoxSender.Text = "";

            customCheckBoxCD.Checked = true;
            customCheckBoxCD.CircleColor = Color.Green;
            customCheckBoxCD.InnerCircleColor = Color.Green;

            customCheckBoxRI.Checked = true;
            customCheckBoxRI.CircleColor = Color.Green;
            customCheckBoxRI.InnerCircleColor = Color.Green;

            customCheckBoxDSR.Checked = true;
            customCheckBoxDSR.CircleColor = Color.Green;
            customCheckBoxDSR.InnerCircleColor = Color.Green;

            customCheckBoxCTS.Checked = true;
            customCheckBoxCTS.CircleColor = Color.Green;
            customCheckBoxCTS.InnerCircleColor = Color.Green;

            customCheckBoxTI.Checked = true;
            customCheckBoxTI.CircleColor = Color.Black;
            customCheckBoxTI.InnerCircleColor = Color.Black;

            checkBoxDTR.Enabled = true;
            checkBoxRTS.Enabled = true;

            originalCircleColor = customCheckBoxTI.CircleColor;
            originalInnerCircleColor = customCheckBoxTI.InnerCircleColor;

            blinkTimer = new System.Windows.Forms.Timer();
            int intValue = (int)numericUpDownSendInterval.Value;
            blinkTimer.Interval = intValue;
            blinkTimer.Tick += BlinkTimer_Tick;

            string logFilePath = Path.Combine(AppContext.BaseDirectory, "logs", "log.txt");
            enableLogging = false;
            Log.Logger = new LoggerConfiguration()
                .Filter.ByExcluding(_ => !enableLogging)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss}] {Level:u3} {Message:lj}{NewLine}{Exception}")
             
                  .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, 
                  buffered: false, flushToDiskInterval: TimeSpan.Zero)
                .CreateLogger();

            Log.Information("Приложение запущено");
        }

        private async void BlinkTimer_Tick(object sender, EventArgs e)
        {
            customCheckBoxTI.CircleColor = customCheckBoxTI.CircleColor == originalCircleColor
                ? Color.Green
                : originalCircleColor;

            customCheckBoxTI.InnerCircleColor = customCheckBoxTI.InnerCircleColor == originalInnerCircleColor
                    ? Color.Green
                : originalInnerCircleColor;

            if (IsPortAvailable(NameReceivingPort))
               {
                blinkTimer.Stop();

                customCheckBoxTI.CircleColor = Color.Black;
                customCheckBoxTI.InnerCircleColor = Color.Black;

                if (_serialPortSender != null && _serialPortSender.IsOpen)
                {
                    _serialPortSender.Close();
                    buttonStart.Text = "Start";
                }
            }

            if (_serialPortSender.IsOpen)
                {
                    string selectedText = comboBoxDataType.Text;
                    if ("Text string" == selectedText)
                    {          
                        await _serialPortSender.WriteAsync(dataToSend);
                    msg = "The data is sent as a string.";
                        textBoxMessages.Text =msg;
                        Log.Information(msg);
                    }
                
                    else
                    {
                        msg = "The data is sent as bytes.";
                         
                        textBoxMessages.Text =msg ;
                        Log.Information(msg);
                }
                }
                else
                {
                         msg = "The port "
                            + NameReceivingPort
                            + " was closed."
                            + Environment.NewLine
                            + "Open the port before sending data.";
                         
                        textBoxMessages.Text= msg;
                        Log.Warning(msg);
                }

        }

       bool IsPortAvailable(string portName)
        {
          
            try
            {
                using (var testPort = new SerialPort(portName))
                {
                    testPort.Open();
                    testPort.Close();
                }
                msg = "Checking the recipient's port for availability."
                    + Environment.NewLine
                    + "The port " + portName + " is not open."
                    + Environment.NewLine
                    + "Open the port to Receiver."
                    + Environment.NewLine;

                textBoxMessages.ForeColor = Color.Red;
                textBoxMessages.AppendText(msg);
                Log.Error(msg);
                this.Refresh();
                return true;
            }
            catch
            {
                msg = "Checking the recipient's port for availability."
                    + Environment.NewLine
                    + "The port " + portName + " is open."
                    + Environment.NewLine
                    + "Data transfer can be started."
                    + Environment.NewLine;

                textBoxMessages.ForeColor = Color.Blue;
                textBoxMessages.AppendText(msg);
                Log.Warning(msg);
                this.Refresh();
                return false; 
            }
         
        }
       
        private void ButtonStartClick(object sender, EventArgs e)
        {
            this.Refresh();
            if (checkBoxLogging.Checked)
                {
                    enableLogging = true; 
                }
                else
                {
                     enableLogging = false;
                }

            Log.Information("The sender has started work.");


          NameReceivingPort = comboBoxReceivingPort.Text;
            if (buttonStart.Text == "Start")
            {
                if (IsPortAvailable(NameReceivingPort))
                {
                    return;
                }
              
            }
            NameSendingPort = comboBoxNameSendingPort.Text;

            if (buttonStart.Text == "Start")
            {
                if (_serialPortSender != null && _serialPortSender.IsOpen)
                {
                    _serialPortSender.Close();
                }
                _serialPortSender = new SerialPortAsyn(textBoxSender, NameSendingPort);
               

                if (!_serialPortSender.IsOpen)
                {
                    
                    _serialPortSender.Open();

                    if (_serialPortSender.IsOpen)
                    {
                        msg= "The port "
                        + NameSendingPort
                        + " has just been opened"
                         + Environment.NewLine;
                       
                        textBoxMessages.AppendText(msg);
                        Log.Information(msg);

                    }

                    // Меняем текст кнопки
                    buttonStart.Text = "Stop";
                    buttonStart.Refresh();

                    groupBoxSendInterval.Enabled = false;
                    // Запускаем таймер
                    if (!blinkTimer.Enabled)
                    {

                        blinkTimer.Start();
                        groupBoxSendInterval.Enabled = false;
                    }

                }
                else
                {
                    msg = "The port "
                           + NameSendingPort
                           + "is already open."
                           + Environment.NewLine;

                    textBoxMessages.Text = msg;
                    Log.Warning(msg);
                }
            }
            else//  Close
            {
              if (blinkTimer.Enabled)
                {
                    blinkTimer.Stop();
                 
                    customCheckBoxTI.CircleColor = Color.Black;
                    customCheckBoxTI.InnerCircleColor = Color.Black;

                }
                groupBoxSendInterval.Enabled = true;
                try
                {
                    if (_serialPortSender.IsOpen)
                    {
                        _serialPortSender.Close();

                        msg = "The port "
                                            + NameSendingPort
                                            + " is now closed."
                                             + Environment.NewLine;
                        textBoxMessages.Text =msg ;
                        Log.Warning(msg);

                    }
                    else
                    {
                        msg = "The port  "
                                               + NameSendingPort
                                               + "is already closed."
                                                + Environment.NewLine;
                        textBoxMessages.Text = msg;
                        Log.Warning(msg);
                    }
                 
                    buttonStart.Text = "Start";
                    textBoxSender.Text = "";

                    textBoxMessages.Text = "Data transmission stopped"
                                            + Environment.NewLine;

                }
                catch (OperationCanceledException)
                {
                    msg = "Data transmission has been stopped."
                                              + Environment.NewLine;
                    textBoxMessages.AppendText(msg);
                    Log.Warning(msg);
                }
               
            }
        }

        private void FormEmulator_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.CloseAndFlush();
        }
    }
}
