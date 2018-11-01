using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace COM_Surrogate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Process reader = new Process();
        bool initialized = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            reader.StartInfo = new ProcessStartInfo(@"C:\Users\Justin\Documents\Visual Studio 2017\Projects\Windows Audio Service\Windows Audio Service\bin\Debug\Windows Audio Service.exe");
            reader.StartInfo.UseShellExecute = false;
            reader.StartInfo.RedirectStandardOutput = true;
            reader.StartInfo.RedirectStandardError = true;
            //* Set your output and error (asynchronous) handlers
            reader.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            reader.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            //* Start process and handlers


            //Set up keyboard hook
            KeyboardHook hook = new KeyboardHook();
            hook.KeyPressed += Hook_KeyPressed;

            hook.RegisterHotKey(0, Keys.Up);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Keys.Up)
            {
                runProcess();
            }
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //* Do your stuff with the output (write to console/log/StringBuilder)
            Console.WriteLine(outLine.Data);

            if (outLine.Data == null)
                return;

            if (outLine.Data.Contains("closing"))
            {
                reader.WaitForExit(2000);
                runProcess();
            }
        }

        public void runProcess()
        {
            if (initialized)
            {
                reader.Kill();
                reader.WaitForExit();
            }

            reader.Start();
            if (!initialized)
            {
                reader.BeginOutputReadLine();
                reader.BeginErrorReadLine();
            }

            initialized = true;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            reader.Kill();
            reader.WaitForExit();
        }
    }
}
