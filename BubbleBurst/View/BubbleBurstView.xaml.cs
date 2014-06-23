using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BubbleBurst.ViewModel;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;

namespace BubbleBurst.View
{
    /// <summary>
    /// The top-level View of the game, which contains a bubble matrix,
    /// game-over dialog, and the context menu.
    /// </summary>
    public partial class BubbleBurstView : UserControl, IViewFor<BubbleBurstViewModel>
    {
        // From XAML: BubbleMatrixView _bubbleMatrixView;
        public BubbleBurstViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (BubbleBurstViewModel)value; } }

        public BubbleBurstView()
        {
            LoadBubbleViewResources();

            InitializeComponent();

            ViewModel = base.DataContext as BubbleBurstViewModel;

            _bubbleMatrixView.MatrixDimensionsAvailable.Subscribe(x => this.HandleMatrixDimensionsAvailable());
        }

        static void LoadBubbleViewResources()
        {
            // Insert the BubbleView resources at the App level to avoid resource duplication.
            // If we insert the resources into this control's Resources collection, every time
            // a BubbleView is removed from the UI some ugly debug warning messages are spewed out.
            string path = "pack://application:,,,/BubbleBurst.View;component/BubbleViewResources.xaml";
            var bubbleViewResources = new ResourceDictionary
            {
                Source = new Uri(path)
            };
            Application.Current.Resources.MergedDictionaries.Add(bubbleViewResources);
        }

        void HandleMatrixDimensionsAvailable()
        {
            // Hook the keyboard event on the Window because this
            // control does not receive keystrokes.
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var keyDown = Observable.FromEventPattern<KeyEventHandler, KeyEventArgs>(
                    h => window.PreviewKeyDown += h,
                    h => window.PreviewKeyDown -= h);

                keyDown.Subscribe(ev => this.HandleWindowPreviewKeyDown(ev.EventArgs));
            }

            this.StartNewGame();
        }

        void HandleWindowPreviewKeyDown(KeyEventArgs e)
        {
            bool undo =
                Keyboard.Modifiers == ModifierKeys.Control &&
                e.Key == Key.Z;

            if (undo && ViewModel.UndoCommand.CanExecute(null))
            {
                ViewModel.UndoCommand.Execute(null);
                e.Handled = true;
            }
        }

        void StartNewGame()
        {
            int rows = _bubbleMatrixView.RowCount;
            int cols = _bubbleMatrixView.ColumnCount;
            ViewModel.BubbleMatrix.SetDimensions(rows, cols);
            ViewModel.BubbleMatrix.StartNewGame();
        }
    }
}