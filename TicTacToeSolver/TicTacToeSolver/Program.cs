using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace TicTacToeSolver
{
    // Game states uniquely correspond to IDs, which are encoded
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

        public void PrintWithIndent(int indent = 0)
        {
            // Line 1: row 1, state #
            for (int i = 0; i < indent; i++) Console.Write(' ');
            for (int i = 0; i < 3; i++) PrintCell(i);
            Console.WriteLine($"  #{id}");

            // Line 2: row 2, next player
            for (int i = 0; i < indent; i++) Console.Write(' ');
            for (int i = 3; i < 6; i++) PrintCell(i);
            Console.WriteLine($"  Next player: {next_player}");

            // Line 3: row 3, expected outcome
            for (int i = 0; i < indent; i++) Console.Write(' ');
            for (int i = 6; i < 9; i++) PrintCell(i);
            Console.WriteLine($"  Expected outcome: {expected_outcome}");

            Console.WriteLine();
        }

        private void PrintCell(int i)
        {
            if (cells[i] == CellContent.Empty)
            {
                Console.Write('-');
            }
            else
            {
                Console.Write(cells[i]);
            }
        }
    }

    class Solver
    {
        private static List<State> states;
        static void Main(string[] args)
        {
            Console.WriteLine($"Initializing all {State.max_id} states.");
            states = new List<State>();
            for (int id = 0; id <= State.max_id; id++)
            {
                states.Add(new State(id));
            }

            Console.WriteLine("Backfilling previous states.");
            for (int id = 0; id <= State.max_id; id++)
            {
                foreach (int next_id in states[id].next_states)
                {
                    states[next_id].prev_states.Add(id);
                }
            }

            Console.WriteLine("Solving.");
            Queue<int> queue = new Queue<int>();
            // Initially, put all terminal states into the queue.
            for (int id = 0; id <= State.max_id; id++)
            {
                if (states[id].outcome != State.Outcome.Undecided)
                {
                    queue.Enqueue(id);
                }
            }
            // Start solving.
            while (queue.Count > 0)
            {
                int id = queue.Dequeue();
                if (states[id].expected_outcome == State.Outcome.Undecided)
                {
                    if (states[id].next_states.Count == 0)
                    {
                        throw new InvalidOperationException($"State #{id} is not terminal, yet it has no next states.");
                    }
                    // Attempt to solve this state.
                    // First, count next states.
                    State.Outcome winning_outcome = (states[id].next_player == State.NextPlayer.O) ?
                        State.Outcome.OWins : State.Outcome.XWins;
                    State.Outcome losing_outcome = (states[id].next_player == State.NextPlayer.O) ?
                        State.Outcome.XWins : State.Outcome.OWins;
                    bool has_undecided_next_state = false;
                    bool has_winning_next_state = false;
                    bool has_draw_next_state = false;
                    foreach (int next_id in states[id].next_states)
                    {
                        switch (states[next_id].expected_outcome)
                        {
                            case State.Outcome.Undecided:
                                has_undecided_next_state = true;
                                break;
                            case State.Outcome.Draw:
                                has_draw_next_state = true;
                                break;
                            case State.Outcome.OWins:
                            case State.Outcome.XWins:
                                if (states[next_id].expected_outcome == winning_outcome)
                                {
                                    has_winning_next_state = true;
                                }
                                break;
                        }
                    }

                    if (has_winning_next_state)
                    {
                        // If there is a winning next state, we can ignore undecided
                        // next states and mark the current state as also winning.
                        states[id].expected_outcome = winning_outcome;
                    }
                    else if (has_undecided_next_state)
                    {
                        // Otherwise, we must solve all next states before solving the
                        // current state.
                        states[id].expected_outcome = State.Outcome.Undecided;
                    }
                    else if (has_draw_next_state)
                    {
                        // If the current state is not winning, at least we want to
                        // force a draw.
                        states[id].expected_outcome = State.Outcome.Draw;
                    }
                    else
                    {
                        // If all else fails, admit defeat.
                        states[id].expected_outcome = losing_outcome;
                    }
                }
                if (states[id].expected_outcome == State.Outcome.Undecided)
                {
                    // If we cannot solve this state, possibly due to unsolved next states,
                    // try again later.
                    queue.Enqueue(id);
                }
                else
                {
                    // If the state is solved, add all unsolved previous states to the queue
                    // so we can try to solve them later.
                    foreach (int prev_id in states[id].prev_states)
                    {
                        if (states[prev_id].expected_outcome == State.Outcome.Undecided)
                        {
                            queue.Enqueue(prev_id);
                        }
                    }
                }
            }

            Console.WriteLine("Solved.");
            // PrintGameTree(root_id: 0, indent: 0);
        }

        static private void PrintGameTree(int root_id, int indent)
        {
            states[root_id].PrintWithIndent(indent);
            foreach (int next_id in states[root_id].next_states)
            {
                PrintGameTree(next_id, indent + 2);
            }
        }
    }
}
