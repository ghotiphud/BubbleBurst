﻿using System;
using System.Windows.Input;
using BubbleBurst.ViewModel.Internal;
using ReactiveUI;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents a bubble in the bubble matrix.
    /// </summary>
    public class BubbleViewModel : ReactiveObject
    {
        readonly BubbleMatrixViewModel _bubbleMatrix;

        static readonly Random _random = new Random(DateTime.Now.Millisecond);

        public BubbleType BubbleType { get; private set; }

        public int Row { get; set; }
        public int Column { get; set; }

        bool _isInBubbleGroup;
        /// <summary>
        /// Returns true if this bubble is a member of the 
        /// currently active bubble group in the user interface.
        /// </summary>
        public bool IsInBubbleGroup
        {
            get { return _isInBubbleGroup; }
            internal set { this.RaiseAndSetIfChanged(ref _isInBubbleGroup, value); }
        }

        /// <summary>
        /// Returns the command used to burst the bubble group in which this bubble exists.
        /// </summary>
        public IReactiveCommand BurstBubbleGroupCommand { get; private set; }

        internal BubbleViewModel(BubbleMatrixViewModel bubbleMatrix, int row, int column)
        {
            if (bubbleMatrix == null)
                throw new ArgumentNullException("bubbleMatrix");

            if (row < 0 || bubbleMatrix.RowCount <= row)
                throw new ArgumentOutOfRangeException("row");

            if (column < 0 || bubbleMatrix.ColumnCount <= column)
                throw new ArgumentOutOfRangeException("column");

            _bubbleMatrix = bubbleMatrix;

            MoveTo(row, column);

            this.BubbleType = GetRandomBubbleType();

            BurstBubbleGroupCommand = new ReactiveCommand();
            BurstBubbleGroupCommand.Subscribe(x => _bubbleMatrix.BurstBubbleGroup());
        }

        /// <summary>
        /// Causes the bubble to evaluate whether or not it is in a bubble group.
        /// </summary>
        /// <param name="isMouseOver">
        /// True if the mouse cursor is currently over this bubble.
        /// </param>
        public void VerifyGroupMembership(bool isMouseOver)
        {
            _bubbleMatrix.VerifyGroupMembership(isMouseOver ? this : null);
        }

        internal void MoveTo(int row, int column)
        {
            Row = row;
            Column = column;
        }

        static BubbleType GetRandomBubbleType()
        {
            var bubbleTypeValues = Enum.GetValues(typeof(BubbleType)) as BubbleType[];
            int highestValue = bubbleTypeValues.Length - 1;
            return (BubbleType)_random.Next(0, highestValue + 1);
        }

        public override string ToString()
        {
            return String.Format("{0}: {1},{2}", BubbleType, Row, Column);
        }
    }
}