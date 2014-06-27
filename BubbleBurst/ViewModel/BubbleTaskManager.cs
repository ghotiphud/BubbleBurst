using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace BubbleBurst.ViewModel
{
    public class BubbleTaskManager : ReactiveObject
    {
        readonly BubbleMatrixViewModel _bubbleMatrix;
        readonly BubbleTaskFactory _taskFactory;
        readonly Stack<IEnumerable<BubbleTaskGroup>> _undoStack;

        /// <summary>
        /// Returns true if an undo operation can be performed at this time.
        /// </summary>
        internal bool CanUndo { get { return _undoStack.Any(); } }

        Subject<BubbleTaskGroup> _pendingTaskGroups = new Subject<BubbleTaskGroup>();
        public IObservable<BubbleTaskGroup> PendingTaskGroups { get { return _pendingTaskGroups.AsObservable(); } }

        internal BubbleTaskManager(BubbleMatrixViewModel bubbleMatrix)
        {
            _bubbleMatrix = bubbleMatrix;
            _taskFactory = new BubbleTaskFactory(bubbleMatrix);
            _undoStack = new Stack<IEnumerable<BubbleTaskGroup>>();
        }

        internal void BurstBubbleGroup(BubbleViewModel[] bubblesInGroup)
        {
            var burst = _taskFactory.CreateTaskGroup(BubbleTaskType.Burst, bubblesInGroup);

            burst.OnComplete.Subscribe(x =>
                {
                    var moveDown = _taskFactory.CreateTaskGroup(BubbleTaskType.MoveDown, bubblesInGroup);

                    moveDown.OnComplete.Subscribe(y =>
                            {
                                var moveRight = _taskFactory.CreateTaskGroup(BubbleTaskType.MoveRight, bubblesInGroup);

                                moveRight.OnComplete.Subscribe(z => {
                                    _bubbleMatrix.IsIdle = true;
                                    _bubbleMatrix.TryToEndGame();
                                });

                                _pendingTaskGroups.OnNext(moveRight);

                                ArchiveTaskGroups(new List<BubbleTaskGroup> { burst, moveDown, moveRight });
                            });

                    _pendingTaskGroups.OnNext(moveDown);
                });

            _bubbleMatrix.IsIdle = false;
            _pendingTaskGroups.OnNext(burst);
        }

        public void Undo()
        {
            var taskGroups = _undoStack.Pop().ToList();
            this.raisePropertyChanged("CanUndo");

            List<Action> lambdas = new List<Action>();

            for (var i = 0; i < taskGroups.Count(); i++)
            {
                var index = i;
                lambdas.Add(() =>
                {
                    var group = _taskFactory.CreateUndoTaskGroup(taskGroups[index]);

                    if (index != 0)
                    {
                        group.OnComplete.Subscribe(x => lambdas[index - 1]());
                    }

                    _pendingTaskGroups.OnNext(group);
                });
            }

            lambdas.Last()();
        }

        void ArchiveTaskGroups(IEnumerable<BubbleTaskGroup> taskGroups)
        {
            _undoStack.Push(taskGroups);
            this.raisePropertyChanged("CanUndo");
        }

        internal void Reset()
        {
            //_pendingTaskGroups.OnCompleted();
            //_pendingTaskGroups = new Subject<BubbleTaskGroup>();
            _undoStack.Clear();
            this.raisePropertyChanged("CanUndo");
        }
    }
}
