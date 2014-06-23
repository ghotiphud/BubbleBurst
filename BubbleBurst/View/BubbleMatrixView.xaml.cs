using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BubbleBurst.ViewModel;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace BubbleBurst.View
{
    /// <summary>
    /// Displays a fixed-size grid of bubbles.
    /// </summary>
    public partial class BubbleMatrixView : ItemsControl, IViewFor<BubbleMatrixViewModel>
    {
        public BubbleMatrixViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (BubbleMatrixViewModel)value; } }

        BubbleCanvas _bubbleCanvas;
        BubblesTaskStoryboardFactory _storyboardFactory;

        internal int RowCount { get { return ViewModel.RowCount; } }
        internal int ColumnCount { get { return ViewModel.ColumnCount; } }

        internal Subject<Unit> _matrixDimensionsAvailable = new Subject<Unit>();
        /// <summary>
        /// Raised when the RowCount and ColumnCount properties have meaningful values.
        /// </summary>
        internal IObservable<Unit> MatrixDimensionsAvailable { get { return _matrixDimensionsAvailable.AsObservable(); } }

        public BubbleMatrixView()
        {
            InitializeComponent();

            var dataContextChanged = Observable.FromEventPattern<DependencyPropertyChangedEventHandler, DependencyPropertyChangedEventArgs>(
                h => base.DataContextChanged += h,
                h => base.DataContextChanged -= h);

            dataContextChanged.Subscribe(ev => ViewModel = ev.EventArgs.NewValue as BubbleMatrixViewModel);

            // Hook the event raised after a bubble group bursts and a series
            // of animations need to run to advance the game state.
            var taskAvailable = this.WhenAnyObservable(x => x.ViewModel.TaskManager.PendingTasksAvailable);
            taskAvailable.Subscribe(x => this.ProcessNextTask());
        }

        void HandleBubbleCanvasLoaded(object sender, RoutedEventArgs e)
        {
            // Store a reference to the panel that contains the bubbles.
            _bubbleCanvas = sender as BubbleCanvas;
            _bubbleCanvas.RowCount = this.RowCount;
            _bubbleCanvas.ColumnCount = this.ColumnCount;

            // Create the factory that makes Storyboards used after a bubble group bursts.
            _storyboardFactory = new BubblesTaskStoryboardFactory(_bubbleCanvas);

            // Let the world know that the size of the bubble matrix is known.
            this.RaiseMatrixDimensionsAvailable();
        }

        void RaiseMatrixDimensionsAvailable()
        {
            _matrixDimensionsAvailable.OnNext(Unit.Default);
        }

        void ProcessNextTask()
        {
            var task = ViewModel.TaskManager.GetPendingTask();
            if (task != null)
            {
                var storyboard = _storyboardFactory.CreateStoryboard(task);
                this.PerformTask(task, storyboard);
            }
        }

        void PerformTask(BubblesTask task, Storyboard storyboard)
        {
            if (storyboard != null)
            {
                // There are some bubbles that need to be animated, so we must
                // wait until the Storyboard finishs before completing the task.
                storyboard.Completed += delegate { this.CompleteTask(task); };

                // Freeze the Storyboard to improve perf.
                storyboard.Freeze();

                // Start animating the bubbles associated with the task.
                storyboard.Begin(this);
            }
            else
            {
                // There are no bubbles associated with this task,
                // so immediately move to the task completion phase.
                this.CompleteTask(task);
            }
        }

        void CompleteTask(BubblesTask task)
        {
            task.OnComplete();
            this.ProcessNextTask();
        }
    }
}