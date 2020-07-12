﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TicTacToeGuiSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private List<State> states;

        private void solve_button_Click(object sender, RoutedEventArgs e)
        {
            // Initialize all states.
            states = new List<State>();
            for (int id = 0; id <= State.max_id; id++)
            {
                states.Add(new State(id));
            }

            // Backfill previous states. Fill next state objects.
            for (int id = 0; id <= State.max_id; id++)
            {
                states[id].NextStates = new List<State>();
                foreach (int next_id in states[id].next_states)
                {
                    states[id].NextStates.Add(states[next_id]);
                    states[next_id].prev_states.Add(id);
                }
            }

            // Solve.
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

            // Display.
            game_tree.Items.Clear();
            game_tree.Items.Add(states[0]);
        }
    }
}
