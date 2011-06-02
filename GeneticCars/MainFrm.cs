﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace GeneticCars
{
    public class MainFrm : GameWindow
    {
        #region Fields

        #region Igralec

        const float rotation_speed = 180.0f;
        float angle;

        Avto igralec;

        enum Turning { Ne = 0, Levo = -3, Desno = 3 };
        Turning Turn = Turning.Ne;

        enum Accelerating { Ne = 0, Naprej = 1, Nazaj = -1 };
        Accelerating acc = Accelerating.Ne;

        #endregion

        bool PlayerWins = false;

        GenetskiAlgoritmi AI;
        object tekmovalciLocker = new object();
        List<Element> tekmovalci;

        const int Dolzina = 1100;

        const int DefaultLoopFactor = 10;
        int LoopFactor = DefaultLoopFactor;

        volatile bool end = false;
        volatile bool pause = false;

        bool fastForward = false;

        enum FormMode { MainMenu, RaceMenu, LearningMenu, Learning, Race, Winner }
        FormMode Mode;

        MainMenu mainmenu;
        RaceMenu racemenu;
        LearningMenu learningmenu;
        WinnerMenu playerWins, playerLoses;

        const string FileName = @"C:\Users\Bozjak\Documents\File.txt";

        #endregion

        #region Constructor

        public MainFrm()
            : base(800, 600, new GraphicsMode(16, 16))
        {
            this.Title = "NeuralCars3D";

            Mode = FormMode.MainMenu;

            Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
            Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Escape)
            {
                if (Mode == FormMode.MainMenu)
                {
                    Exit();
                    return;
                }
                else if (Mode == FormMode.Learning)
                {
                    Mode = FormMode.LearningMenu;
                    pause = true;
                }
                else if (Mode == FormMode.LearningMenu)
                {
                    Mode = FormMode.Learning;
                    pause = false;

                    SwittchFromMenu();
                }
                else if (Mode == FormMode.Race)
                {
                    Mode = FormMode.RaceMenu;
                    pause = true;
                }
                else if (Mode == FormMode.RaceMenu)
                {
                    Mode = FormMode.Race;
                    pause = false;

                    SwittchFromMenu();
                }
                else if (Mode == FormMode.Winner)
                {
                    Mode = FormMode.MainMenu;
                }

                return;
            }
            else if (e.Key == OpenTK.Input.Key.Up)
            {
                if (Mode == FormMode.MainMenu)
                    mainmenu.MoveUp();
                else if (Mode == FormMode.RaceMenu)
                    racemenu.MoveUp();
                else if (Mode == FormMode.LearningMenu)
                    learningmenu.MoveUp();
                else if (Mode == FormMode.Learning || Mode == FormMode.Race) // Dodal Alex
                    acc = Accelerating.Naprej; // Dodal Alex
            }
            else if (e.Key == OpenTK.Input.Key.Down)
            {
                if (Mode == FormMode.MainMenu)
                    mainmenu.MoveDown();
                else if (Mode == FormMode.RaceMenu)
                    racemenu.MoveDown();
                else if (Mode == FormMode.LearningMenu)
                    learningmenu.MoveDown();
                else if (Mode == FormMode.Learning || Mode == FormMode.Race) // Dodal Alex
                    acc = Accelerating.Nazaj; // Dodal Alex
            }
            else if (e.Key == OpenTK.Input.Key.Enter)
            {
                if (Mode == FormMode.MainMenu)
                    mainmenu.Submit();
                else if (Mode == FormMode.RaceMenu)
                    racemenu.Submit();
                else if (Mode == FormMode.LearningMenu)
                    learningmenu.Submit();
            }
            else if (e.Key == OpenTK.Input.Key.Left)
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race) // Dodal Alex
                    Turn = Turning.Levo; // Dodal Alex
            }
            else if (e.Key == OpenTK.Input.Key.Right)
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race) // Dodal Alex
                    Turn = Turning.Desno; // Dodal Alex
            }
            else if (e.Key == OpenTK.Input.Key.Space)
            {
                fastForward = !fastForward;
            }
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (e.Key == OpenTK.Input.Key.Right)
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race)
                    Turn = Turning.Ne;
            }
            else if (e.Key == OpenTK.Input.Key.Left)
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race)
                    Turn = Turning.Ne;
            }
            else if (e.Key == OpenTK.Input.Key.Up) 
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race)
                    acc = Accelerating.Ne;
            }
            else if (e.Key == OpenTK.Input.Key.Down)
            {
                if (Mode == FormMode.Learning || Mode == FormMode.Race)
                    acc = Accelerating.Ne;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            end = true;
            base.OnClosing(e);
        }

        #endregion

        #region OnLoad

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.DarkGreen);
            GL.Enable(EnableCap.DepthTest);

            //Inicializiramo form size
            this.Size = new Size(820, 650);

            #region MenuActions

            mainmenu = new MainMenu(this.Size);
            mainmenu.SubmitExit = delegate() { end = true; Exit(); };
            mainmenu.SubmitLearningMode = delegate() { SetUpLearning();  SwittchFromMenu(); Mode = FormMode.Learning; };
            mainmenu.SubmitRaceMode = delegate() { SetUpRace(); SwittchFromMenu(); Mode = FormMode.Race; };

            racemenu = new RaceMenu(this.Size);
            racemenu.SubmitExitToMain = delegate() { Mode = FormMode.MainMenu; };
            racemenu.SubmitRestart = delegate() { SetUpRace(); SwittchFromMenu(); Mode = FormMode.Race; };

            learningmenu = new LearningMenu(this.Size);
            learningmenu.SubmitExitToMain = delegate() { Mode = FormMode.MainMenu; };
            learningmenu.SubmitLoad = delegate() { LearningLoad(); pause = false; SwittchFromMenu(); Mode = FormMode.Learning; };
            learningmenu.SubmitSave = delegate() { AI.Write(FileName); Mode = FormMode.MainMenu; };
            learningmenu.SubmitRestart = delegate() { RestartLearning(); pause = false; SwittchFromMenu(); Mode = FormMode.Learning; };

            playerWins = new WinnerMenu(this.Size, true);
            playerLoses = new WinnerMenu(this.Size, false);

            #endregion

            PlayingGround.ImportFromSCG();

            //avto ki ga vozi igralec
            igralec = new Avto(Color.Blue, true);
            igralec.Player = true;

            //Inicializiramo genetske algoritme
            AI = new GenetskiAlgoritmi();
        }

        #endregion

        #region OnResize

        /// <summary>
        /// Called when the user resizes the window.
        /// </summary>
        /// <param name="e">Contains the new width/height of the window.</param>
        /// <remarks>
        /// You want the OpenGL viewport to match the window. This is the place to do it!
        /// </remarks>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            double aspect_ratio = Width / (double)Height;

            //OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 64);
            SwittchFromMenu();
        }

        #endregion

        #region OnRenderFrame

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            switch (Mode)
            {
                case FormMode.MainMenu:
                    mainmenu.Draw();
                    break;
                case FormMode.RaceMenu:
                    racemenu.Draw();
                    break;
                case FormMode.LearningMenu:
                    learningmenu.Draw();
                    break;
                case FormMode.Learning:
                    DrawLearningMode();
                    break;
                case FormMode.Race:
                    DrawRaceMode();
                    break;
                case FormMode.Winner:
                    if (PlayerWins) playerWins.Draw();
                    else playerLoses.Draw();
                    break;
            }

            this.SwapBuffers();
            Thread.Sleep(1);
        }

        #endregion

        #region Draw

        private void DrawLearningMode()
        {
            DrawGround();
            DrawLearningRace();
        }

        private void DrawRaceMode()
        {
            DrawGround();
            DrawRace();
        }

        private void DrawGround()
        {
            GL.Begin(BeginMode.LineLoop);
            GL.Color3(Color.White);

            foreach (Point p in PlayingGround.Points)
            {
                GL.Vertex3(p.X - 410, -(p.Y) + 325, -4.0);
            }

            GL.End();

            for (int i = 0; i < PlayingGround.Points.Length; i++)
            {

                #region Stirikotniki

                GL.Begin(BeginMode.Quads);
                GL.Color3(Color.White);

                Point p = PlayingGround.Points[i];
                Point r = PlayingGround.Points[(i + 1) % PlayingGround.Points.Length];
                float x0 = p.X;
                float y0 = p.Y;
                float x1 = r.X;
                float y1 = r.Y;

                float dx = x1 - x0; //delta x
                float dy = y1 - y0; //delta y
                float linelength = (float)Math.Sqrt(dx * dx + dy * dy);
                dx /= linelength;
                dy /= linelength;
                //Ok, (dx, dy) is now a unit vector pointing in the direction of the line
                //A perpendicular vector is given by (-dy, dx)
                float thickness = 70.0f; //Some number
                float px = 0.5f * thickness * (-dy); //perpendicular vector with lenght thickness * 0.5
                float py = 0.5f * thickness * dx;

                GL.Vertex3(x0 - px - 410, -(y0 + py) + 325, -4.0f);
                GL.Vertex3(x1 - px - 410, -(y1 + py) + 325, -4.0f);
                GL.Vertex3(x1 + px - 410, -(y1 - py) + 325, -4.0f);
                GL.Vertex3(x0 + px - 410, -(y0 - py) + 325, -4.0f);

                GL.End();

                #endregion

                #region Ovalni koti

                GL.Begin(BeginMode.Polygon);

                for (float angle = 0; angle <= Math.PI * 2; angle += 0.01f)
                {
                    GL.Vertex3((x1 + Math.Sin(angle) * 35) - 410, -(y1 + Math.Cos(angle) * 35) + 325, -4.0f);
                }

                GL.End();

                #endregion

            }

            GL.Begin(BeginMode.Points);

            GL.Color3(Color.Red);
            foreach (Point p in PlayingGround.Points)
            {
                GL.Vertex3(p.X - 410, -p.Y + 325, -3.5f);
            }

            GL.End();
        }

        private void DrawLearningRace()
        {
            igralec.PaintOpenGL();

            if (tekmovalci != null)
            {
                double threshold = Avto.CostOfBest - 1590;

                lock (tekmovalciLocker)
                {
                    foreach (Element element in tekmovalci)
                    {
                        if (((element.Cost > threshold) || (element.risiCrte)) && (element != AI.Best))
                        {
                            element.PaintOpenGL();
                        }
                    }
                }
            }

            if (AI.Best != null) AI.Best.PaintOpenGL();
        }

        private void DrawRace()
        {
            igralec.PaintOpenGL();

            foreach (Element element in tekmovalci)
            {
                element.PaintOpenGL();
            }
        }

        private void SwittchFromMenu()
        {
            OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreateOrthographic(820, 650, 1, 30);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
        }

        #endregion

        #region GameLoop

        Thread WorkingThread;

        void ExitWorking()
        {
            end = true;
            if (WorkingThread != null)
            {
                WorkingThread.Join();
                WorkingThread = null;
            }
        }

        #region Learning

        void RestartLearning()
        {
            ExitWorking();

            SetUpLearning();
        }

        void LearningLoad()
        {
            ExitWorking();

            end = pause = false;
            tekmovalci = AI.Load(FileName);
            igralec.Reset();

            WorkingThread = new Thread(new ThreadStart(LearningModeGameLoop));
            WorkingThread.Start();
        }

        void SetUpLearning()
        {
            if (WorkingThread != null)
            {
                ExitWorking();
            }

            end = pause = false;
            tekmovalci = AI.Reset();
            igralec.Reset();

            WorkingThread = new Thread(new ThreadStart(LearningModeGameLoop));
            WorkingThread.Start();
        }

        void LearningModeGameLoop()
        {
            double BestPlayerScore = 0;

            int turn = 0;
            DateTime CompletedTurn = DateTime.Now;
            TimeSpan FrameTime = TimeSpan.FromMilliseconds(30);

            DateTime FPS = DateTime.Now;

            while (!end)
            {
                while (pause)
                {
                    if (end) break;
                    Thread.Sleep(10);
                }

                if ((DateTime.Now - FPS) > TimeSpan.FromSeconds(1))
                {
                    FPS = DateTime.Now;
                }

                igralec.Turn((float)Turn);
                igralec.Accelerate((float)acc);

                igralec.Update();

                if (turn > Dolzina)
                {
                    turn = 0;

                    Avto.CostOfBest = 0;
                    
                    lock (tekmovalciLocker)
                    {
                        tekmovalci = AI.PripraviTekmovalce();
                    }

                    if (igralec.Cost > BestPlayerScore) BestPlayerScore = igralec.Cost;
                    igralec.Reset();

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

                while ((DateTime.Now - CompletedTurn) < FrameTime)
                {
                    Thread.Sleep(1);
                }

                CompletedTurn = DateTime.Now;
            }
        }

        #endregion

        #region Race

        void RestartRace()
        {
            SetUpRace();
        }

        void SetUpRace()
        {
            if (WorkingThread != null)
            {
                ExitWorking();
            }

            end = pause = false;
            tekmovalci = AI.Load(FileName, 5);
            igralec.Reset();

            //Nastimaj pozicije
            igralec.SetStartPoz(-20, 0);
            tekmovalci[0].SetStartPoz(-20, -20);
            tekmovalci[1].SetStartPoz(0, -20);
            tekmovalci[2].SetStartPoz(0, 0);
            tekmovalci[3].SetStartPoz(+20, 0);
            tekmovalci[4].SetStartPoz(+20, -20);

            //Pozeni
            WorkingThread = new Thread(new ThreadStart(RaceModeGameLoop));
            WorkingThread.Start();
        }

        void RaceModeGameLoop()
        {
            const int NumLaps = 2;

            DateTime CompletedTurn = DateTime.Now;
            TimeSpan FrameTime = TimeSpan.FromMilliseconds(30);

            DateTime FPS = DateTime.Now;

            while (!end)
            {
                while (pause)
                {
                    if (end) break;
                    Thread.Sleep(10);
                }

                if ((DateTime.Now - FPS) > TimeSpan.FromSeconds(1))
                {
                    FPS = DateTime.Now;
                }

                igralec.Turn((float)Turn);
                igralec.Accelerate((float)acc);

                igralec.Update(tekmovalci, igralec);

                if (igralec.Lap == NumLaps)
                {
                    PlayerWins = true;
               
                    end = true;
                    break;
                }

                foreach (Element e in tekmovalci)
                {
                    e.Update(tekmovalci, igralec);

                    if (e.Lap == NumLaps)
                    {
                        PlayerWins = false;
                        end = true;
                        break;
                    }
                }

                while ((DateTime.Now - CompletedTurn) < FrameTime)
                {
                    Thread.Sleep(1);
                }

                CompletedTurn = DateTime.Now;
            }

            Mode = FormMode.Winner;
            WorkingThread = null;
        }

        #endregion

        #endregion

    }
}
