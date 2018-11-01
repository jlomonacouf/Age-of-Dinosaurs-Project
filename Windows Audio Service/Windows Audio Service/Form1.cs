using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronOcr;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using System.Threading;

namespace Windows_Audio_Service
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //Signify to user that application has been opened
            BrightnessModifier.SetGamma(5);

            advancedReader = new AdvancedOcr()
            {
                CleanBackgroundNoise = true,
                Strategy = AdvancedOcr.OcrStrategy.Advanced,
                ColorSpace = AdvancedOcr.OcrColorSpace.GrayScale,
                InputImageType = AdvancedOcr.InputTypes.AutoDetect
            };

            //Set up keyboard hook
            KeyboardHook hook = new KeyboardHook();
            hook.KeyPressed += Hook_KeyPressed;

            hook.RegisterHotKey(0, Keys.Down);
        }

        AdvancedOcr advancedReader;
        Bitmap OCRBitmap;
        string question = "";
        string[] answerArray = new string[5];
        Bitmap[] answerBitmaps = new Bitmap[5];
        int correctAnswerIndex = 0;

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void setOCRBitmap(Rectangle rect)
        {
            OCRBitmap = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            List<Bitmap> b = new List<Bitmap>();
            Graphics g = Graphics.FromImage(OCRBitmap);
           
            //Handles unable to capture screen exception
            try
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, OCRBitmap.Size, CopyPixelOperation.SourceCopy);
            }
            catch(Exception)
            {
                this.Close();
            }
        }

        private void setAnswerOCRBitmaps()
        {
            Graphics g;
            Rectangle rect;

            rect = new Rectangle(150, 405, 1100, 70);
            answerBitmaps[0] = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(answerBitmaps[0]);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, answerBitmaps[0].Size, CopyPixelOperation.SourceCopy);

            rect = new Rectangle(150, 554, 1100, 70);
            answerBitmaps[1] = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(answerBitmaps[1]);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, answerBitmaps[1].Size, CopyPixelOperation.SourceCopy);

            rect = new Rectangle(150, 703, 1100, 70);
            answerBitmaps[2] = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(answerBitmaps[2]);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, answerBitmaps[2].Size, CopyPixelOperation.SourceCopy);

            rect = new Rectangle(150, 852, 1100, 70);
            answerBitmaps[3] = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(answerBitmaps[3]);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, answerBitmaps[3].Size, CopyPixelOperation.SourceCopy);

            rect = new Rectangle(150, 1001, 1100, 70);
            answerBitmaps[4] = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(answerBitmaps[4]);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, answerBitmaps[4].Size, CopyPixelOperation.SourceCopy);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            startOCR();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = Cursor.Position.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            scrape();
        }

        private void Hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.Key == Keys.Down /*&& e.Modifier == Windows_Audio_Service.ModifierKeys.Control*/)
            {
                startOCR();
            }
        }

        #region OCR Functions
        
        private void startOCR()
        {
            mousePositionTimer.Stop();
            question = getQuestion();
            answerArray = getAnswers();
            scrape();
        }

        private string getQuestion()
        {
            setOCRBitmap(new Rectangle(108, 214, 1100, 120));
            OcrResult result = advancedReader.Read(OCRBitmap);

            return result.Text;
        }

        private string[] getAnswers()
        {
            OcrResult result;

            setAnswerOCRBitmaps();


            Task<OcrResult> task = Task.Run(() => advancedReader.ReadMultiThreaded(answerBitmaps.AsEnumerable()));
            if (task.Wait(TimeSpan.FromSeconds(15)))
                result = task.Result;
            else
            {
                Console.WriteLine("Thread Hanged");
                //Signify to user that program is closing
                BrightnessModifier.SetGamma(10);

                this.Close();
                return new string[5];
            }
            
            string[] answers;
            //Handles IronOCR License Exception
            try
            {
                answers = result.Text.Split(new string[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch(Exception ee)
            {
                answers = new string[1];
                this.Close();
            }

            for(int i = 0; i < answers.Length; i++)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 ]");
                answers[i] = rgx.Replace(answers[i], "");
            }
        
            return answers;
        }
        #endregion
        
        private void scrape()
        {
            AoDQuestionReader reader = new AoDQuestionReader();
            string[] questionAnswer = reader.findQuestionAnswer(question);
            //MessageBox.Show("Question:\n" + questionAnswer[0] + "\n" + "Answer:\n" + questionAnswer[1]);
            Console.WriteLine("Question:\n" + questionAnswer[0] + "\n" + "Answer:\n" + questionAnswer[1]);
            int bestFitIndex = 0;
            double bestFitRatio = 0;
            for (int i = 0; i < answerArray.Length; i++)
            {
                double ratio = reader.CalculateSimilarity(answerArray[i], questionAnswer[1]);
                if (ratio > bestFitRatio)
                {
                    bestFitIndex = i;
                    bestFitRatio = ratio;
                }
            }
            correctAnswerIndex = bestFitIndex;
            mousePositionTimer.Start();
        }

        private void mousePositionTimer_Tick(object sender, EventArgs e)
        {
            if (Cursor.Position.Y > 194 && Cursor.Position.Y < 246 && correctAnswerIndex == 0)
            {
                BrightnessModifier.SetGamma(10);
            }
            else if (Cursor.Position.Y > 265 && Cursor.Position.Y < 324 && correctAnswerIndex == 1)
            {
                BrightnessModifier.SetGamma(10);
            }
            else if (Cursor.Position.Y > 324 && Cursor.Position.Y < 398 && correctAnswerIndex == 2)
            {
                BrightnessModifier.SetGamma(10);
            }
            else if (Cursor.Position.Y > 415 && Cursor.Position.Y < 470 && correctAnswerIndex == 3)
            {
                BrightnessModifier.SetGamma(10);
            }
            else if (Cursor.Position.Y > 486 && Cursor.Position.Y < 548 && correctAnswerIndex == 4)
            {
                BrightnessModifier.SetGamma(10);
            }
            else
            {
                BrightnessModifier.SetGamma(5);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("closing");
            Application.Exit();
        }
    }
}
