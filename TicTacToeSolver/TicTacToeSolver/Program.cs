using System;
using System.Collections.Generic;

namespace TicTacToeSolver
{
    // Game states uniquely correspond to IDs, which are encoded from
    // from the board state and next player.
    //
    // Cell state ID: empty -- 0 | O -- 1 | X -- 2
    // Board state ID: (cell 0 state) * 3^8 + (cell 1 state) * 3^7 + ... + (cell 8 state)
    // Next player ID: O -- 0 | X -- 1
    //
    // Game state ID: (board state) *2 + (next player ID)
    class State
    {
        public State(int id)
        {
            this.id = id;
            prev_states = new List<int>();
            next_states = new List<int>();
            expected_outcome = Outcome.Undecided;

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

        }
        public int id;
        // Inclusive.
        public const int max_board_id = 2 * (6561 + 2187 + 729 + 243 + 81 + 27 + 9 + 3 + 1);
        // Inclusive.
        public const int max_id = max_board_id * 2 + 1;

        public List<int> prev_states;
        public List<int> next_states;

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

        public void Print()
        {
            Console.WriteLine($"State #{id}:");
            for (int i = 0; i < 9; i++)
            {
                if (cells[i] == CellContent.Empty)
                {
                    Console.Write(' ');
                }
                else
                {
                    Console.Write(cells[i]);
                }
                if (i % 3 == 2) Console.WriteLine();
            }
            Console.WriteLine($"Outcome: {outcome}");
            Console.WriteLine($"Next player: {next_player}");
            Console.WriteLine();
        }
    }

    class Solver
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Initializing all {State.max_id} states.");
            List<State> states = new List<State>();
            for (int id = 0; id <= State.max_id; id++)
            {
                states.Add(new State(id));
            }

            states[0].Print();
            states[12345].Print();
        }
    }
}
