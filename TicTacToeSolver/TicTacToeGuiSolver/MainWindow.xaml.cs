using System;
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

        private void solve_button_Click(object sender, RoutedEventArgs e)
        {
            State root_state = new State() { ID = 0 };
            root_state.NextStates.Add(new State() { ID = 123 });
            root_state.NextStates[0].NextStates.Add(new State() { ID = 124 });
            root_state.NextStates.Add(new State() { ID = 456 });
            game_tree.Items.Add(root_state);
        }
    }
}
