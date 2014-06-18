using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel
{
    /// <summary>
    /// Represents some work that BubbleMatrixView must 
    /// process for a given set of BubbleViewModels.
    /// </summary>
    public class BubblesTask
    {
        readonly Func<IEnumerable<BubbleViewModel>> _getBubbles;

        public BubblesTaskType TaskType { get; private set; }

        BubbleViewModel[] _bubbles;
        public IEnumerable<BubbleViewModel> Bubbles
        {
            get
            {
                if (_bubbles == null)
                {
                    // The list of bubbles associated with this task is
                    // retrieved once, on demand, because retrieving the 
                    // list can have side effects.
                    _bubbles = _getBubbles().ToArray();
                }
                return _bubbles;
            }
        }

        /// <summary>
        /// Invoked immediately after the task has been performed.
        /// </summary>
        public Action OnComplete { get; private set; }

        /// <summary>
        /// Returns true if this task is undoing the effects of a previously performed task.
        /// </summary>
        public bool IsUndo { get; private set; }

        internal BubblesTask(BubblesTaskType taskType, bool isUndo, Func<IEnumerable<BubbleViewModel>> getBubbles, Action onComplete)
        {
            this.TaskType = taskType;
            this.IsUndo = isUndo;
            _getBubbles = getBubbles;
            this.OnComplete = onComplete;
        }
    }
}