using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace GeneticCars
{
    public partial class Form1 : Form
    {
        Avto igralec;
        bool end = false;

        bool fastForward = false;

        enum Turning { Ne = 0, Levo = -3, Desno = 3 };
        Turning Turn = Turning.Ne;

        enum Accelerating { Ne = 0, Naprej = 1, Nazaj =-1 };
        Accelerating acc = Accelerating.Ne;

        GenetskiAlgoritmi AI;
        List<Element> tekmovalci;

        static readonly int Dolzina = 1100;

        const int DefaultLoopFactor = 10;
        int LoopFactor = DefaultLoopFactor;

        public Form1()
        {
            InitializeComponent();

            //Inicializiramo form size
            this.Size = new Size(820, 650);

            PlayingGround.ImportFromSCG();

            //avto ki ga vozi igralec
            igralec = new Avto(Color.Blue, true);

            //Inicializiramo genetske algoritme
            AI = new GenetskiAlgoritmi();
            tekmovalci = AI.Inicializiraj();

            //Inicializiramo labele
            label1.Text = "0";
            label2.Location = new Point(label1.Right + 20, label2.Location.Y);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(PlayingGround.field, this.ClientRectangle);
#if DEBUG
            PlayingGround.Paint(e.Graphics);
#endif
            if (tekmovalci != null)
            {
                double threshold = Avto.CostOfBest - 1590;

                foreach (Element element in tekmovalci)
                {
                    if (((element.Cost > threshold) || (element.risiCrte)) && (element != AI.Best))
                        element.PaintOpenGL(); // Spremenil Alex
                }
            }

            if (AI.Best != null) AI.Best.PaintOpenGL();  // Spremenil Alex
            igralec.PaintOpenGL();  // Spremenil Alex
        }

        void GameLoop()
        {
            double BestPlayerScore = 0;

            int turn = 0;
            DateTime CompletedTurn = DateTime.Now;
            TimeSpan FrameTime = TimeSpan.FromMilliseconds(30);

            DateTime FPS = DateTime.Now;
            int FPSCounter = 0;

            while (!end)
            {
                FPSCounter++;

                if ((DateTime.Now - FPS) > TimeSpan.FromSeconds(1))
                {
                    label3.Text = string.Format("{0} FPS", FPSCounter);
                    FPSCounter = 0;
                    FPS = DateTime.Now;
                }

                igralec.Turn((float)Turn);
                igralec.Accelerate((float)acc);

                igralec.Update();
                lblPlayerScore.Text = string.Format("Player score: {0:0.00}; Best: {1:0.000}", igralec.Cost, BestPlayerScore);

                if (turn > Dolzina)
                {
                    turn = 0;

                    Avto.CostOfBest = 0;
                    tekmovalci = AI.PripraviTekmovalce();

                    if (igralec.Cost > BestPlayerScore) BestPlayerScore = igralec.Cost;
                    igralec.Reset();

                    label2.Text = string.Format("Generacija: {0}, BestScore: {1:0.000}, Stevilo voznikov: {2}", AI.generacija, AI.BestCost, tekmovalci.Count);

                    if (fastForward && LoopFactor < 200) LoopFactor++;
                }

                //AI tekmovalci:
                int loop = 1;

                if (fastForward)
                {
                    loop *= LoopFactor;
                }

                for (int i = 0; (i < loop) && (turn <= Dolzina); i++)
                {
                    turn++;

                    foreach (Element element in tekmovalci)
                    {
                        element.Update();
                    }
                }

                label1.Text = turn.ToString();

                this.Refresh();
                Application.DoEvents();

                while ((DateTime.Now - CompletedTurn) < FrameTime)
                {
                    Thread.Sleep(1);
                }

                CompletedTurn = DateTime.Now;
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    acc = Accelerating.Naprej;
                    break;
                case Keys.Down:
                    acc = Accelerating.Nazaj;
                    break;
                case Keys.Left:
                    Turn = Turning.Levo;
                    break;
                case Keys.Right:
                    Turn = Turning.Desno;
                    break;
                case Keys.Space:
                    fastForward = !fastForward;
                    if (!fastForward) LoopFactor = DefaultLoopFactor;
                    break;
                case Keys.Escape:
                    end = true;
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.Up) || (e.KeyCode == Keys.Down))
                acc = Accelerating.Ne;
            if ((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right))
                Turn = Turning.Ne;
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            this.Width = 820;
            this.Height = 650;
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            GameLoop();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            end = true;
        }
    }
}
