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
        internal int RowCount { get; private set; }
        internal int ColumnCount { get; private set; }

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

        public BubblesTaskManager TaskManager { get; private set; }

        public IReactiveCommand UndoCommand { get; private set; }

        /// <summary>
        /// Raised when there are no more bubble groups left to burst.
        /// </summary>
        readonly Subject<bool> _gameEnded = new Subject<bool>();
        public IObservable<bool> GameEnded { get { return _gameEnded.AsObservable(); } }

        internal BubbleMatrixViewModel()
        {
            _bubblesInternal = new ReactiveList<BubbleViewModel>();
            this.Bubbles = _bubblesInternal.CreateDerivedCollection(x => x);

            this.TaskManager = new BubblesTaskManager(this);

            _bubbleGroup = new BubbleGroup(this.Bubbles);

            _bubbleGroupSizeStack = new Stack<int>();

            _isIdle = true;

            var canUndo = this.WhenAny(x => x.IsIdle, x => x.TaskManager.CanUndo, (i, cu) => i.Value && cu.Value);
            UndoCommand = new ReactiveCommand(canUndo);
            UndoCommand.Subscribe(x => Undo());
        }

        /// <summary>
        /// Updates the number of rows and columns that 
        /// the matrix should contain.
        /// </summary>
        public void SetDimensions(int rowCount, int columnCount)
        {
            if (!this.IsIdle)
                throw new InvalidOperationException("Cannot set matrix dimensions is not idle.");

            if (rowCount < 1)
                throw new ArgumentOutOfRangeException("rowCount", rowCount, "Must be greater than zero.");

            if (columnCount < 1)
                throw new ArgumentOutOfRangeException("columnCount", columnCount, "Must be greater than zero.");

            RowCount = rowCount;
            ColumnCount = columnCount;
        }

        public void StartNewGame()
        {
            // Reset game state.
            this.IsIdle = true;
            this.ResetBubbleGroup();
            _bubbleGroupSizeStack.Clear();
            this.TaskManager.Reset();

            InitBubbles();
        }

        void InitBubbles()
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

            this.TaskManager.PublishTasks(bubblesInGroup);
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
            _gameEnded.OnNext(true);
        }
    }
}