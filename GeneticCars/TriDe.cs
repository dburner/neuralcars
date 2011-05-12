#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2006-2008 the OpenTK Team.
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 */
#endregion

#region --- Using Directives ---

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

#endregion

namespace GeneticCars
{
    /// <summary>
    /// Demonstrates immediate mode rendering.
    /// </summary>
    //[Example("Immediate mode", ExampleCategory.OpenGL, "1.x", 1, Documentation = "ImmediateMode")]
    public class T03_Immediate_Mode_Cube : GameWindow
    {
        #region --- Fields ---

        const float rotation_speed = 180.0f;
        float angle;

        Avto igralec;
        bool end = false;

        bool fastForward = false;

        enum Turning { Ne = 0, Levo = -3, Desno = 3 };
        Turning Turn = Turning.Ne;

        enum Accelerating { Ne = 0, Naprej = 1, Nazaj = -1 };
        Accelerating acc = Accelerating.Ne;

        GenetskiAlgoritmi AI;
        ArrayList tekmovalci;

        static readonly int Dolzina = 1100;

        const int DefaultLoopFactor = 10;
        int LoopFactor = DefaultLoopFactor;

        #endregion

        #region --- Constructor ---

        public T03_Immediate_Mode_Cube()
            : base(800, 600, new GraphicsMode(16, 16))
        { }

        #endregion

        #region OnLoad

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            GL.ClearColor(Color.DarkGreen);
            GL.Enable(EnableCap.DepthTest);

            //Inicializiramo form size
            this.Size = new Size(820, 650);

            PlayingGround.ImportFromSCG();

            //avto ki ga vozi igralec
            igralec = new Avto(Color.Blue, true);

            //Inicializiramo genetske algoritme
            AI = new GenetskiAlgoritmi();
            tekmovalci = AI.Inicializiraj();

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
            OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreateOrthographic(820, 650, 1, 30);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);
        }

        #endregion

        #region OnUpdateFrame

        /// <summary>
        /// Prepares the next frame for rendering.
        /// </summary>
        /// <remarks>
        /// Place your control logic here. This is the place to respond to user input,
        /// update object positions etc.
        /// </remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                this.Exit();
                return;
            }
        }

        #endregion

        #region OnRenderFrame

        /// <summary>
        /// Place your rendering code here.
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Matrix4 lookat = Matrix4.LookAt(3, 3, 3, 0, 0, 0, 0, 0, 0);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadMatrix(ref lookat);

            //angle += rotation_speed * (float)e.Time;
            //GL.Rotate(angle, 0.0f, 1.0f, 0.0f);

            DrawCube();

            this.SwapBuffers();
            Thread.Sleep(1);
        }

        #endregion

        #region private void DrawCube()

        private void DrawCube()
        {

            #region Cesta

            //GL.Color3(Color.White);
            //GL.Vertex3(0.0f, 0.0f, -4.0f);
            //GL.Vertex3(1.0f, 0.0f, -4.0f);
            //GL.Vertex3(1.0f, 1.0f, -4.0f);

            //GL.Color3(Color.White);
            //GL.Vertex3(0.0f, 0.0f, 0.0f);
            //GL.Vertex3(0.0f, 0.0f, 6.0f);
            //GL.Vertex3(6.0f, 0.0f, 6.0f);
            //GL.Vertex3(6.0f, 0.0f, 0.0f);

            #region Daljice

            GL.Begin(BeginMode.LineLoop);
            GL.Color3(Color.White);

            foreach (Point p in PlayingGround.Points)
            {
                GL.Vertex3(p.X - 410, -(p.Y) + 325, -4.0);
            }

            GL.End();

            #endregion

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
                //glVertex2f(x0 - px, y0 + py);
                GL.Vertex3(x1 - px - 410, -(y1 + py) + 325, -4.0f);
                //glVertex2f(x1 - px, y1 + py);
                GL.Vertex3(x1 + px - 410, -(y1 - py) + 325, -4.0f);
                //glVertex2f(x1 + px, y1 - py);
                GL.Vertex3(x0 + px - 410, -(y0 - py) + 325, -4.0f);
                //glVertex2f(x0 + px, y0 - py);

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

            #endregion

            //GL.Begin(BeginMode.Quads);

            //GL.Color3(Color.Silver);
            //GL.Vertex3(-1.0f, -1.0f, -1.0f);
            //GL.Vertex3(-1.0f, 1.0f, -1.0f);
            //GL.Vertex3(1.0f, 1.0f, -1.0f);
            //GL.Vertex3(1.0f, -1.0f, -1.0f);

            //GL.Color3(Color.Honeydew);
            //GL.Vertex3(-1.0f, -1.0f, -1.0f);
            //GL.Vertex3(1.0f, -1.0f, -1.0f);
            //GL.Vertex3(1.0f, -1.0f, 1.0f);
            //GL.Vertex3(-1.0f, -1.0f, 1.0f);

            //GL.Color3(Color.Moccasin);

            //GL.Vertex3(-1.0f, -1.0f, -1.0f);
            //GL.Vertex3(-1.0f, -1.0f, 1.0f);
            //GL.Vertex3(-1.0f, 1.0f, 1.0f);
            //GL.Vertex3(-1.0f, 1.0f, -1.0f);

            //GL.Color3(Color.IndianRed);
            //GL.Vertex3(-1.0f, -1.0f, 1.0f);
            //GL.Vertex3(1.0f, -1.0f, 1.0f);
            //GL.Vertex3(1.0f, 1.0f, 1.0f);
            //GL.Vertex3(-1.0f, 1.0f, 1.0f);

            //GL.Color3(Color.PaleVioletRed);
            //GL.Vertex3(-1.0f, 1.0f, -1.0f);
            //GL.Vertex3(-1.0f, 1.0f, 1.0f);
            //GL.Vertex3(1.0f, 1.0f, 1.0f);
            //GL.Vertex3(1.0f, 1.0f, -1.0f);

            //GL.Color3(Color.ForestGreen);
            //GL.Vertex3(1.0f, -1.0f, -1.0f);
            //GL.Vertex3(1.0f, 1.0f, -1.0f);
            //GL.Vertex3(1.0f, 1.0f, 1.0f);
            //GL.Vertex3(1.0f, -1.0f, 1.0f);

            //GL.End();
        }

        #endregion
    }
}
