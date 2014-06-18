using System.Collections.Generic;
using System.Linq;

namespace BubbleBurst.ViewModel.Internal
{
    /// <summary>
    /// Keeps track of a bubble's current location and its location history.
    /// </summary>
    internal class BubbleLocationManager
    {
        BubbleLocation? _currentLocation;
        readonly Stack<BubbleLocation> _previousLocations;

        internal int Row { get { return _currentLocation.HasValue ? _currentLocation.Value.Row : -1; } }
        internal int Column { get { return _currentLocation.HasValue ? _currentLocation.Value.Column : -1; } }

        internal int PreviousRow { get { return _previousLocations.Any() ? _previousLocations.Peek().Row : -1; } }
        internal int PreviousColumn { get { return _previousLocations.Any() ? _previousLocations.Peek().Column : -1; } }

        internal BubbleLocationManager()
        {
            _previousLocations = new Stack<BubbleLocation>();
        }

        internal void MoveTo(int row, int column)
        {
            if (_currentLocation.HasValue)
            {
                _previousLocations.Push(_currentLocation.Value);
            }

            _currentLocation = new BubbleLocation(row, column);
        }

        internal void MoveToPreviousLocation()
        {
            if (_previousLocations.Any())
            {
                _currentLocation = _previousLocations.Pop();
            }
        }

        private struct BubbleLocation
        {
            public BubbleLocation(int row, int column)
            {
                this.Row = row;
                this.Column = column;
            }

            public readonly int Column;
            public readonly int Row;
        }
    }
}