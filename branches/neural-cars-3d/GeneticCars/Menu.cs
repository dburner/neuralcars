using System;
using System.Collections.Generic;
using System.Drawing;

namespace GeneticCars
{
    abstract class Menu : IDisposable
    {
        protected int SelectedLine { get; private set; }

        List<int> SelectableLines = new List<int>();

        protected readonly Brush SelectedItemBrush = new SolidBrush(Color.Yellow);
        protected readonly Brush ItemBrush = new SolidBrush(Color.Red);

        protected ScreenText Text;

        public Menu(Size ClientSize)
        {
            Text = new ScreenText(ClientSize, ClientSize);

            Text.AddLine("NeuralCars3D", 240, 150, new SolidBrush(Color.Red), 40);
            Text.AddLine("Avotrja: David Božjak, Aleksander Bešir", 580, 630, new SolidBrush(Color.White));
        }

        public void Draw()
        {
            Text.Draw();
        }

        protected void AddSelectableLine(string s, float x, float y, float size = 10)
        {
            Brush b = SelectableLines.Count == 0 ? SelectedItemBrush : ItemBrush;
            SelectableLines.Add(Text.AddLine(s, x, y, b, size));
        }

        public void MoveUp()
        {
            if (SelectedLine == 0)
                return;

            Text.Update(SelectableLines[SelectedLine], ItemBrush);
            Text.Update(SelectableLines[--SelectedLine], SelectedItemBrush);
        }

        public void MoveDown()
        {
            if (SelectedLine == SelectableLines.Count - 1)
                return;

            Text.Update(SelectableLines[SelectedLine], ItemBrush);
            Text.Update(SelectableLines[++SelectedLine], SelectedItemBrush);
        }

        abstract public void Submit();

        public void Dispose()
        {
            Text.Dispose();
        }
    }
}
