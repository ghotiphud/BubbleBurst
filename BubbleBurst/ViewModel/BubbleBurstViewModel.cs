using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// This is the top-level view model class.
    /// </summary>
    public class BubbleBurstViewModel : ReactiveObject
    {
        public BubbleMatrixViewModel BubbleMatrix { get; private set; }

        GameOverViewModel _gameOver;
        public GameOverViewModel GameOver
        {
            get { return _gameOver; }
            private set { this.RaiseAndSetIfChanged(ref _gameOver, value); }
        }

        public IReactiveCommand RestartCommand { get; private set; }
        /// <summary>
        /// Returns the command that un-bursts the previously burst bubble group.
        /// </summary>
        public IReactiveCommand UndoCommand { get; private set; }

        public BubbleBurstViewModel()
        {
            BubbleMatrix = new BubbleMatrixViewModel();
            BubbleMatrix.GameEnded.Subscribe(x =>
            {
                this.GameOver = new GameOverViewModel(this.BubbleMatrix);
                this.GameOver.RequestClose.Subscribe(rc => this.GameOver = null);
            });

            RestartCommand = new ReactiveCommand();
            RestartCommand.Subscribe(x => BubbleMatrix.StartNewGame());

            var canUndo = Observable.CombineLatest(
                this.WhenAnyObservable(x => x.BubbleMatrix.Undo.CanExecuteObservable),
                this.WhenAnyValue(x => x.GameOver),
                (ce, go) => ce && go != null);

            UndoCommand = new ReactiveCommand(canUndo);
            UndoCommand.Subscribe(x => BubbleMatrix.Undo.Execute(null));
        }
    }
}