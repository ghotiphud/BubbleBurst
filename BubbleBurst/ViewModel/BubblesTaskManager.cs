﻿using System;
using System.Collections.Generic;
using System.Linq;
using BubbleBurst.ViewModel.Internal;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Responsible for the tasks used to burst and un-burst bubble groups.  
    /// </summary>
    public class BubblesTaskManager : ReactiveObject
    {
        readonly BubblesTaskFactory _bubblesTaskFactory;
        readonly Queue<BubblesTask> _pendingTasks;
        readonly Stack<IEnumerable<BubblesTask>> _undoStack;

        /// <summary>
        /// Returns true if an undo operation can be performed at this time.
        /// </summary>
        internal bool CanUndo { get { return _undoStack.Any(); } }

        /// <summary>
        /// Raised when tasks are available to be performed.
        /// </summary>
        public Subject<Unit> _pendingTasksAvailable = new Subject<Unit>();
        public IObservable<Unit> PendingTasksAvailable { get { return _pendingTasksAvailable.AsObservable(); } }

        internal BubblesTaskManager(BubbleMatrixViewModel bubbleMatrix)
        {
            _bubblesTaskFactory = new BubblesTaskFactory(bubbleMatrix);
            _pendingTasks = new Queue<BubblesTask>();
            _undoStack = new Stack<IEnumerable<BubblesTask>>();
        }

        /// <summary>
        /// Returns the next pending task if one exists, or null.
        /// </summary>
        public BubblesTask GetPendingTask()
        {
            return _pendingTasks.Any() ? _pendingTasks.Dequeue() : null;
        }

        /// <summary>
        /// Publishs a set of tasks that will burst a bubble group.
        /// </summary>
        internal void PublishTasks(BubbleViewModel[] bubblesInGroup)
        {
            var tasks = _bubblesTaskFactory.CreateTasks(bubblesInGroup);
            this.ArchiveTasks(tasks);
            this.PublishTasks(tasks);
        }

        /// <summary>
        /// Publishs a set of tasks that will undo the previous bubble burst.
        /// </summary>
        internal void Undo()
        {
            var originalTasks = _undoStack.Pop();
            this.raisePropertyChanged("CanUndo");

            var undoTasks = _bubblesTaskFactory.CreateUndoTasks(originalTasks);
            this.PublishTasks(undoTasks);
        }

        internal void Reset()
        {
            _pendingTasks.Clear();
            _undoStack.Clear();
            this.raisePropertyChanged("CanUndo");
        }

        void ArchiveTasks(IEnumerable<BubblesTask> tasks)
        {
            _undoStack.Push(tasks);
            this.raisePropertyChanged("CanUndo");
        }

        void PublishTasks(IEnumerable<BubblesTask> tasks)
        {
            foreach (BubblesTask task in tasks)
            {
                _pendingTasks.Enqueue(task);
            }

            this.RaisePendingTasksAvailable();
        }

        void RaisePendingTasksAvailable()
        {
            _pendingTasksAvailable.OnNext(Unit.Default);
        }
    }
}