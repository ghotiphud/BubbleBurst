using System;
using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>
    /// Locates and stores a set contiguous bubbles of the same color.
    /// Exposes methods to activate and deactivate a group of bubbles,
    /// which is used to highlight them in the UI.
    /// </summary>
    internal class BubbleGroup
    {
        readonly IEnumerable<BubbleViewModel> _allBubbles;

        internal IList<BubbleViewModel> BubblesInGroup { get; private set; }

        internal bool HasBubbles { get { return this.BubblesInGroup.Any(); } }

        internal BubbleGroup(IEnumerable<BubbleViewModel> allBubbles)
        {
            if (allBubbles == null)
                throw new ArgumentNullException("allBubbles");

            _allBubbles = allBubbles;
            this.BubblesInGroup = new List<BubbleViewModel>();
        }

        internal void Activate()
        {
            foreach (BubbleViewModel member in this.BubblesInGroup)
            {
                member.IsInBubbleGroup = true;
            }
        }

        internal void Deactivate()
        {
            foreach (BubbleViewModel member in this.BubblesInGroup)
            {
                member.IsInBubbleGroup = false;
            }
        }

        /// <summary>
        /// Searches for a bubble group in which the specified bubble
        /// is a member.  If a group is found, this object's BubblesInGroup
        /// collection will contain the bubbles in that group afterwards.
        /// </summary>
        internal BubbleGroup FindBubbleGroup(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            bool isBubbleInCurrentGroup = this.BubblesInGroup.Contains(bubble);
            if (!isBubbleInCurrentGroup)
            {
                this.BubblesInGroup.Clear();

                this.SearchForGroup(bubble);

                bool addOriginalBubble =
                    this.HasBubbles &&
                    !this.BubblesInGroup.Contains(bubble);

                if (addOriginalBubble)
                {
                    this.BubblesInGroup.Add(bubble);
                }
            }
            return this;
        }

        internal void Reset()
        {
            this.Deactivate();
            this.BubblesInGroup.Clear();
        }

        void SearchForGroup(BubbleViewModel bubble)
        {
            if (bubble == null)
                throw new ArgumentNullException("bubble");

            foreach (BubbleViewModel groupMember in this.FindMatchingNeighbors(bubble))
            {
                if (!this.BubblesInGroup.Contains(groupMember))
                {
                    this.BubblesInGroup.Add(groupMember);
                    this.SearchForGroup(groupMember);
                }
            }
        }

        IEnumerable<BubbleViewModel> FindMatchingNeighbors(BubbleViewModel bubble)
        {
            var matches = new List<BubbleViewModel>();

            // Check above.
            var match = this.TryFindMatch(bubble.Row - 1, bubble.Column, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check below.
            match = this.TryFindMatch(bubble.Row + 1, bubble.Column, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check left.
            match = this.TryFindMatch(bubble.Row, bubble.Column - 1, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            // Check right.
            match = this.TryFindMatch(bubble.Row, bubble.Column + 1, bubble.BubbleType);
            if (match != null)
                matches.Add(match);

            return matches;
        }

        BubbleViewModel TryFindMatch(int row, int column, BubbleType bubbleType)
        {
            return _allBubbles.SingleOrDefault(b =>
                b.Row == row &&
                b.Column == column &&
                b.BubbleType == bubbleType);
        }
    }
}