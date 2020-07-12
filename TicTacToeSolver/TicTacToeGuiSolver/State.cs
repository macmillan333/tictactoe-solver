using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace TicTacToeGuiSolver
{
    // Refer to Program.cs in the other project
    // for the maths in this class.
    public class State
    {
        public State(int id)
        {
            this.id = id;
            prev_states = new List<int>();
            next_states = new List<int>();
            expected_outcome = Outcome.Undecided;
            Initialize();
        }
        public int id;
        // Inclusive.
        public const int max_board_id = 2 * (6561 + 2187 + 729 + 243 + 81 + 27 + 9 + 3 + 1);
        // Inclusive.
        public const int max_id = max_board_id * 2 + 1;

        public List<int> prev_states;
        public List<int> next_states;
        public List<State> NextStates { get; set; }

        private void Initialize()
        {
            // Decode ID.
            next_player = (NextPlayer)(id % 2);
            int board_state = id / 2;
            cells = new CellContent[9];
            for (int i = 8; i >= 0; i--)
            {
                cells[i] = (CellContent)(board_state % 3);
                board_state /= 3;
            }

            // Is there an outcome?
            outcome = Outcome.Undecided;
            int[] lines = { 12, 345, 678, 36, 147, 258, 48, 246 };
            foreach (int line in lines)
            {
                int pos_0 = line / 100;
                int pos_1 = (line % 100) / 10;
                int pos_2 = line % 10;
                if (cells[pos_0] == CellContent.O && cells[pos_1] == CellContent.O && cells[pos_2] == CellContent.O)
                {
                    outcome = Outcome.OWins;
                    break;
                }
                if (cells[pos_0] == CellContent.X && cells[pos_1] == CellContent.X && cells[pos_2] == CellContent.X)
                {
                    outcome = Outcome.XWins;
                    break;
                }
            }
            if (outcome == Outcome.Undecided)
            {
                bool any_cell_empty = false;
                foreach (CellContent c in cells)
                {
                    if (c == CellContent.Empty)
                    {
                        any_cell_empty = true;
                        break;
                    }
                }
                if (!any_cell_empty)
                {
                    outcome = Outcome.Draw;
                }
            }

            // PS. Some boards are impossible, such as ones with multiple lines of Os and Xs.
            // However, boards with an outcome have no next states, so these impossible
            // states should be isolated from the game tree.
            // Thus there's no need to handle them.

            // If no outcome, find next states.
            // Previous states will be filled by the Solver class.
            if (outcome != Outcome.Undecided)
            {
                expected_outcome = outcome;
                return;
            }
            CellContent next_cell = (next_player == NextPlayer.O) ? CellContent.O : CellContent.X;
            NextPlayer next_next_player = (next_player == NextPlayer.O) ? NextPlayer.X : NextPlayer.O;
            board_state = id / 2;
            int delta_state = 6561;
            for (int i = 0; i < 9; i++)
            {
                if (cells[i] == CellContent.Empty)
                {
                    int next_board = board_state + (int)next_cell * delta_state;
                    next_states.Add(next_board * 2 + (int)next_next_player);
                }
                delta_state /= 3;
            }
        }

        public enum Outcome
        {
            Undecided = -1,
            OWins = 0,
            Draw = 1,
            XWins = 2
        }
        public Outcome outcome;
        // This is the expected outcome from the current state, assuming
        // both players play best moves.
        public Outcome expected_outcome;

        // Below are decoded from ID. We have enough memory to store them so why not.

        public enum CellContent
        {
            Empty = 0,
            O = 1,
            X = 2
        }
        public CellContent[] cells;
        public enum NextPlayer
        {
            O = 0,
            X = 1
        }
        public NextPlayer next_player;

        public string Display
        {
            get
            {
                string display = "";

                // Line 1: row 1, state #
                for (int i = 0; i < 3; i++) display += DisplayCell(i);
                display += $"  #{id}\n";

                // Line 2: row 2, next player
                for (int i = 3; i < 6; i++) display += DisplayCell(i);
                display += $"  Next player: {next_player}\n";

                // Line 3: row 3, expected outcome
                for (int i = 6; i < 9; i++) display += DisplayCell(i);
                display += $"  Expected outcome: {expected_outcome}";

                return display;
            }
        }

        private string DisplayCell(int i)
        {
            switch (cells[i])
            {
                case CellContent.Empty:
                    return "-";
                case CellContent.O:
                    return "O";
                case CellContent.X:
                    return "X";
            }
            return "";
        }
    }
}
