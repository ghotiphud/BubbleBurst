using System;
using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// This is the top-level view model class.
    /// </summary>
    public class BubbleBurstViewModel : ObservableObject
    {
        public BubbleMatrixViewModel BubbleMatrix { get; private set; }
        
        GameOverViewModel _gameOver;
        public GameOverViewModel GameOver
        {
            get { return _gameOver; }
            private set
            {
                if (value == _gameOver)
                    return;

                _gameOver = value;

                base.RaisePropertyChanged("GameOver");
            }
        }

        private bool CanUndo { get { return this.GameOver == null && this.BubbleMatrix.CanUndo; } }

        /// <summary>
        /// Returns the command that starts a new game of BubbleBurst.
        /// </summary>
        public ICommand RestartCommand { get { return new RelayCommand(this.BubbleMatrix.StartNewGame); } }
        /// <summary>
        /// Returns the command that un-bursts the previously burst bubble group.
        /// </summary>
        public ICommand UndoCommand { get { return new RelayCommand(this.BubbleMatrix.Undo, () => this.CanUndo); } }

        public BubbleBurstViewModel()
        {
            this.BubbleMatrix = new BubbleMatrixViewModel();
            this.BubbleMatrix.GameEnded += delegate
            {
                this.GameOver = new GameOverViewModel(this.BubbleMatrix);
                this.GameOver.RequestClose += this.HandleGameOverRequestClose;
            };
        }

        void HandleGameOverRequestClose(object sender, EventArgs e)
        {
            this.GameOver.RequestClose -= this.HandleGameOverRequestClose;
            this.GameOver = null;
        }
    }
}