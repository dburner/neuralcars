using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace GeneticCars
{
    class TextLine
    {
        public readonly PointF Position;
        public string Text;
        public Brush Color;
        public readonly Font TextFont;

        public TextLine(string text, PointF position, Brush col)
        {
            Text = text;
            Position = position;
            Color = col;
            TextFont = new Font(FontFamily.GenericSansSerif, 8);
        }

        public TextLine(string text, PointF position, Brush col, float size)
            : this (text, position, col)
        {
            TextFont = new Font(FontFamily.GenericSansSerif, size);
        }

        public TextLine(string text, PointF position, Brush col, float size, FontFamily fontfamily)
            : this(text, position, col)
        {
            TextFont = new Font(fontfamily, size);
        }

        public TextLine(string text, PointF position, Brush col, Font f)
            : this(text, position, col)
        {
            TextFont = f;
        }
    }

    class ScreenText :IDisposable
    {
        private readonly Bitmap TextBitmap;

        private int TextureId;
        private Size ClientSize;

        List<TextLine> Lines = new List<TextLine>();

        public int Count { get { return Lines.Count; } } 

        public void Update(int lineId, string newText)
        {
            if (lineId < Lines.Count)
            {
                Lines[lineId].Text = newText;
                UpdateText();
            }
        }

        public void Update(int lineId, Brush brush)
        {
            if (lineId < Lines.Count)
            {
                Lines[lineId].Color = brush;
                UpdateText();
            }
        }

        public ScreenText(Size ClientSize, Size areaSize)
        {
            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            this.ClientSize = ClientSize;
            TextureId = CreateTexture();
        }

        private int CreateTexture()
        {
            int textureId;

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;
        }

        public void Clear()
        {
            Lines.Clear();
        }

        public int AddLine(string s, float x, float y, Brush col, float size = 10)
        {
            Lines.Add(new TextLine(s, new PointF(x, y), col, size));

            UpdateText();

            return Lines.Count - 1;
        }

        private void UpdateText()
        {
            if (Lines.Count > 0)
            {
                using (Graphics gfx = Graphics.FromImage(TextBitmap))
                {
                    gfx.Clear(Color.Black);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    foreach (TextLine line in Lines)
                        gfx.DrawString(line.Text, line.TextFont, line.Color, line.Position);
                }

                System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                TextBitmap.UnlockBits(data);
            }
        }

        public void Draw()
        {
            GL.PushMatrix();
            GL.LoadIdentity();

            Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, ClientSize.Width, ClientSize.Height, 0, -1, 1);
            GL.MatrixMode(MatrixMode.Projection);

            GL.PushMatrix();
            GL.LoadMatrix(ref ortho_projection);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.DstColor);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, TextureId);


            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
            GL.TexCoord2(1, 0); GL.Vertex2(TextBitmap.Width, 0);
            GL.TexCoord2(1, 1); GL.Vertex2(TextBitmap.Width, TextBitmap.Height);
            GL.TexCoord2(0, 1); GL.Vertex2(0, TextBitmap.Height);
            GL.End();
            GL.PopMatrix();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
        }

        public void Dispose()
        {
            if (TextureId > 0)
                GL.DeleteTexture(TextureId);
        }
    }
}
