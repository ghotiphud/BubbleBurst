using BubbleBurst.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Thriple.Easing;

namespace BubbleBurst.View
{
    public class BubbleTaskStoryboardFactory
    {
        private BubbleCanvas _bubbleCanvas;

        public BubbleTaskStoryboardFactory(BubbleCanvas _bubbleCanvas)
        {
            // TODO: Complete member initialization
            this._bubbleCanvas = _bubbleCanvas;
        }
        internal Storyboard CreateStoryboard(BubbleTaskGroup taskGroup)
        {
            int millisecondsPerUnit;
            Func<ContentPresenter, double> getTo;
            DependencyProperty animatedProperty;
            IEnumerable<BubbleViewModel> bubbles;

            this.GetStoryboardCreationData(
                taskGroup,
                out millisecondsPerUnit,
                out getTo,
                out animatedProperty,
                out bubbles);

            return this.CreateStoryboard(
                taskGroup,
                millisecondsPerUnit,
                getTo,
                animatedProperty,
                bubbles.ToArray());
        }

        void GetStoryboardCreationData(
          BubbleTaskGroup taskGroup,
          out int millisecondsPerUnit,
          out Func<ContentPresenter, double> getTo,
          out DependencyProperty animatedProperty,
          out IEnumerable<BubbleViewModel> bubbles)
        {
            switch (taskGroup.TaskType)
            {
                case BubbleTaskType.Burst:
                    millisecondsPerUnit = 100;
                    getTo = cp => 0.0;
                    animatedProperty = UIElement.OpacityProperty;
                    bubbles = taskGroup.Select(t => t.Bubble);
                    break;

                case BubbleTaskType.Add:
                    millisecondsPerUnit = 100;
                    getTo = cp => 1.0;
                    animatedProperty = UIElement.OpacityProperty;
                    bubbles = taskGroup.Select(t => t.Bubble);
                    break;

                case BubbleTaskType.MoveDown:
                    millisecondsPerUnit = 50;
                    getTo = _bubbleCanvas.CalculateTop;
                    animatedProperty = Canvas.TopProperty;

                    // Sort the bubbles to ensure that the columns move 
                    // in sync with each other in an appealing way.
                    bubbles =
                        from bubble in taskGroup.Select(t => t.Bubble)
                        orderby bubble.PreviousColumn
                        orderby bubble.PreviousRow descending
                        select bubble;
                    break;

                case BubbleTaskType.MoveRight:
                    millisecondsPerUnit = 50;
                    getTo = _bubbleCanvas.CalculateLeft;
                    animatedProperty = Canvas.LeftProperty;

                    // Sort the bubbles to ensure that the rows move 
                    // in sync with each other in an appealing way.
                    bubbles =
                        from bubble in taskGroup.Select(t => t.Bubble)
                        orderby bubble.PreviousRow descending
                        orderby bubble.PreviousColumn descending
                        select bubble;
                    break;

                default:
                    throw new ArgumentException("Unrecognized BubblesTaskType: " + taskGroup.TaskType);
            }

            //if (taskGroup.IsUndo)
            //{
            //    bubbles = bubbles.Reverse();
            //}
        }

        Storyboard CreateStoryboard(
            BubbleTaskGroup taskGroup,
            int millisecondsPerUnit,
            Func<ContentPresenter, double> getTo,
            DependencyProperty animatedProperty,
            BubbleViewModel[] bubbles)
        {
            if (!bubbles.Any())
                return null;

            var storyboard = new Storyboard();
            var targetProperty = new PropertyPath(animatedProperty);
            var beginTime = TimeSpan.FromMilliseconds(0);
            var beginTimeIncrement = TimeSpan.FromMilliseconds(millisecondsPerUnit / bubbles.Count());

            foreach (ContentPresenter presenter in this.GetBubblePresenters(bubbles))
            {
                var bubble = presenter.DataContext as BubbleViewModel;
                var duration = CalculateDuration(taskGroup.TaskType, bubble, millisecondsPerUnit);
                var to = getTo(presenter);
                var anim = new EasingDoubleAnimation
                {
                    BeginTime = beginTime,
                    Duration = duration,
                    Equation = EasingEquation.CubicEaseIn,
                    To = to,
                };

                Storyboard.SetTarget(anim, presenter);
                Storyboard.SetTargetProperty(anim, targetProperty);

                if (IsTaskStaggered(taskGroup.TaskType))
                {
                    beginTime = beginTime.Add(beginTimeIncrement);
                }

                storyboard.Children.Add(anim);
            }

            return storyboard;
        }

        IEnumerable<ContentPresenter> GetBubblePresenters(IEnumerable<BubbleViewModel> bubbles)
        {
            var bubblePresenters = new List<ContentPresenter>();
            var contentPresenters = _bubbleCanvas.Children.Cast<ContentPresenter>().ToArray();
            foreach (BubbleViewModel bubble in bubbles)
            {
                var bubblePresenter = contentPresenters.FirstOrDefault(cp => cp.DataContext == bubble);
                if (bubblePresenter != null)
                {
                    bubblePresenters.Add(bubblePresenter);
                }
            }
            return bubblePresenters;
        }

        static Duration CalculateDuration(
            BubbleTaskType taskType,
            BubbleViewModel bubble,
            int millisecondsPerUnit)
        {
            int totalMilliseconds;
            switch (taskType)
            {
                case BubbleTaskType.Burst:
                case BubbleTaskType.Add:
                    totalMilliseconds = millisecondsPerUnit;
                    break;

                case BubbleTaskType.MoveDown:
                    totalMilliseconds = millisecondsPerUnit * Math.Abs(bubble.Row - bubble.PreviousRow);
                    break;

                case BubbleTaskType.MoveRight:
                    totalMilliseconds = millisecondsPerUnit * Math.Abs(bubble.Column - bubble.PreviousColumn);
                    break;

                default:
                    throw new ArgumentException("Unrecognized BubblesTaskType value: " + taskType, "taskType");
            }
            return new Duration(TimeSpan.FromMilliseconds(totalMilliseconds));
        }

        static bool IsTaskStaggered(BubbleTaskType taskType)
        {
            return taskType != BubbleTaskType.Burst;
        }
    }
}
