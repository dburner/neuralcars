using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GeneticCars
{
    class MainMenu : Menu
    {
        public Action SubmitLearningMode;
        public Action SubmitRaceMode;
        public Action SubmitExit;

        public MainMenu(Size ClientSize)
            : base(ClientSize)
        {
            AddSelectableLine("Learning mode", 300, 350, 20);
            AddSelectableLine("Race mode", 320, 380, 20);
            AddSelectableLine("Exit", 370, 410, 20);

            Text.AddLine("Avotrja: David Božjak, Aleksander Bešir", 580, 630, new SolidBrush(Color.White));
        }

        public override void Submit()
        {
            if ((SelectedLine == 0) && (SubmitLearningMode != null))
                SubmitLearningMode();
            else if ((SelectedLine == 1) && (SubmitRaceMode != null))
                SubmitRaceMode();
            else if ((SelectedLine == 2) && (SubmitExit != null))
                SubmitExit();
        }
    }
}
