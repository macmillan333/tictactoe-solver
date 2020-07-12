using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace TicTacToeGuiSolver
{
    public class State
    {
        public State()
        {
            NextStates = new List<State>();
        }
        public int ID { get; set; }
        public List<State> NextStates { get; set; }

        public string Display
        {
            get
            {
                return "State #" + ID.ToString() + "\n" + "Another line";
            }
        }
    }
}
