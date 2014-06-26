using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BubbleBurst.ViewModel.Internal;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents the matrix of bubbles and contains 
    /// logic that drives a game to completion.
    /// </summary>
    public class BubbleMatrixViewModel : ReactiveObject
    {
        readonly BubbleGroup _bubbleGroup;
        readonly Stack<int> _bubbleGroupSizeStack;

        // The Matrix
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }

        // Bubbles
        readonly IReactiveList<BubbleViewModel> _bubblesInternal;
        public IReactiveDerivedList<BubbleViewModel> Bubbles { get; private set; }

        internal int MostBubblesPoppedAtOnce { get { return _bubbleGroupSizeStack.Max(); } }

        // Tasks
        bool _isIdle;
        /// <summary>
        /// Represents whether the application is currently processing something that
        /// requires the user interface to ignore user interactions until it finishes.
        /// </summary>
        public bool IsIdle
        {
            get { return _isIdle; }
            internal set { this.RaiseAndSetIfChanged(ref _isIdle, value); }
        }

        public BubbleTaskManager TaskManager { get; set; }

        public IReactiveCommand UndoCommand { get; private set; }

        /// <summary>
        /// Raised when there are no more bubble groups left to burst.
        /// </summary>
        readonly Subject<Unit> _gameEnded = new Subject<Unit>();
        public IObservable<Unit> GameEnded { get { return _gameEnded.AsObservable(); } }

        internal BubbleMatrixViewModel(int rowCount, int columnCount)
        {
            RowCount = rowCount;
            ColumnCount = columnCount;

            _bubblesInternal = new ReactiveList<BubbleViewModel>();
            this.Bubbles = _bubblesInternal.CreateDerivedCollection(x => x);

            this.TaskManager = new BubbleTaskManager(this);

            _bubbleGroup = new BubbleGroup(this.Bubbles);

            _bubbleGroupSizeStack = new Stack<int>();

            _isIdle = true;

            var canUndo = this.WhenAny(x => x.IsIdle, x => x.TaskManager.CanUndo, (i, cu) => i.Value && cu.Value);
            UndoCommand = new ReactiveCommand(canUndo);
            UndoCommand.Subscribe(x => Undo());

            TaskManager.PendingTaskGroups.Subscribe(tg => ExecuteTaskGroup(tg));
        }

        private void ExecuteTaskGroup(BubbleTaskGroup taskGroup)
        {
            foreach (var task in taskGroup)
            {
                var bubble = task.Bubble;
                var moveDistance = task.MoveDistance;

                switch (taskGroup.TaskType)
                {
                    case BubbleTaskType.Burst:
                        RemoveBubble(bubble);
                        break;
                    case BubbleTaskType.Add:
                        AddBubble(bubble);
                        break;
                    case BubbleTaskType.MoveDown:
                        bubble.MoveTo(bubble.Row + moveDistance, bubble.Column);
                        break;
                    case BubbleTaskType.MoveRight:
                        bubble.MoveTo(bubble.Row, bubble.Column + moveDistance);
                        break;
                }
            }
        }

        public void StartNewGame()
        {
            // Reset game state.
            this.IsIdle = true;
            this.ResetBubbleGroup();
            _bubbleGroupSizeStack.Clear();
            this.TaskManager.Reset();

            InitializeBubbles();
        }

        void InitializeBubbles()
        {
            // Create a new matrix of bubbles.
            this.ClearBubbles();
            ((ReactiveList<BubbleViewModel>)_bubblesInternal).AddRange(
                from row in Enumerable.Range(0, RowCount)
                from col in Enumerable.Range(0, ColumnCount)
                select new BubbleViewModel(this, row, col));
        }

        internal void AddBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Add(bubble);
        }

        public void ClearBubbles()
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot clear bubbles when matrix is not idle.");

            _bubblesInternal.Clear();
        }

        internal void BurstBubbleGroup()
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot burst a bubble group when not idle.");

            var bubblesInGroup = _bubbleGroup.BubblesInGroup.ToArray();
            if (!bubblesInGroup.Any())
                return;

            _bubbleGroupSizeStack.Push(bubblesInGroup.Length);
            
            this.TaskManager.BurstBubbleGroup(bubblesInGroup);
        }

        internal void ResetBubbleGroup()
        {
            _bubbleGroup.Reset();
        }

        internal void RemoveBubble(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            _bubblesInternal.Remove(bubble);
        }

        internal void TryToEndGame()
        {
            bool groupExists = this.Bubbles.Any(b => this.IsInBubbleGroup(b));
            if (!groupExists)
            {
                this.IsIdle = false;
                this.RaiseGameEnded();
            }
        }

        internal void VerifyGroupMembership(BubbleViewModel bubble)
        {
            _bubbleGroup.Deactivate();
            if (bubble != null)
            {
                _bubbleGroup.FindBubbleGroup(bubble).Activate();
            }
        }

        /// <summary>
        /// Reverts the game state to how it was before 
        /// the most recent group of bubbles was burst.
        /// </summary>
        void Undo()
        {
            // Throw away the last bubble group size, 
            // since that burst is about to be undone.
            _bubbleGroupSizeStack.Pop();

            this.TaskManager.Undo();
        }

        bool IsInBubbleGroup(BubbleViewModel bubble)
        {
            return new BubbleGroup(this.Bubbles).FindBubbleGroup(bubble).HasBubbles;
        }

        void RaiseGameEnded()
        {
            _gameEnded.OnNext(Unit.Default);
        }
    }
}