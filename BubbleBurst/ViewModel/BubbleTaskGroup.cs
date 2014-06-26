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
    /// <summary>
    /// Groups all of the individual tasks of a single type together
    /// </summary>
    public class BubbleTaskGroup : List<BubbleTask>
    {
        public BubbleTaskType TaskType { get; set; }

        private Subject<Unit> _onComplete = new Subject<Unit>();
        public IObservable<Unit> OnComplete { get { return _onComplete.AsObservable(); } }

        internal void RaiseComplete()
        {
            _onComplete.OnNext(Unit.Default);
            _onComplete.OnCompleted();
        }

        public BubbleTaskGroup Clone()
        {
            var clone = (BubbleTaskGroup)this.MemberwiseClone();

            clone._onComplete = new Subject<Unit>();

            return clone;
        }
    }
}
