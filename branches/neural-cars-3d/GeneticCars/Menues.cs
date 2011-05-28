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

    class RaceMenu : Menu
    {
        public Action SubmitRestart;
        public Action SubmitExitToMain;

        public RaceMenu(Size ClientSize)
            : base(ClientSize)
        {
            AddSelectableLine("Restart", 360, 350, 20);
            AddSelectableLine("Exit to main menu", 300, 380, 20);
        }

        public override void Submit()
        {
            if ((SelectedLine == 0) && (SubmitRestart != null))
                SubmitRestart();
            else if ((SelectedLine == 1) && (SubmitExitToMain != null))
                SubmitExitToMain();
        }
    }

    class LearningMenu : Menu
    {
        public Action SubmitRestart;
        public Action SubmitSave;
        public Action SubmitLoad;
        public Action SubmitExitToMain;

        public LearningMenu(Size ClientSize)
            : base(ClientSize)
        {
            AddSelectableLine("Restart", 360, 350, 20);
            AddSelectableLine("Save model", 340, 380, 20);
            AddSelectableLine("Load model", 341, 410, 20);
            AddSelectableLine("Exit to main menu", 300, 440, 20);
        }

        public override void Submit()
        {
            if ((SelectedLine == 0) && (SubmitRestart != null))
                SubmitRestart();
            else if ((SelectedLine == 1) && (SubmitSave != null))
                SubmitSave();
            else if ((SelectedLine == 2) && (SubmitLoad != null))
                SubmitLoad();
            else if ((SelectedLine == 3) && (SubmitExitToMain != null))
                SubmitExitToMain();
        }
    }
}
