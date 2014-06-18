using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>
    /// The modal dialog shown once a game has ended.
    /// </summary>
    public partial class GameOverView : UserControl
    {

        public GameOverView()
        {
            InitializeComponent();

            _outroStoryboard = _contentBorder.Resources["OutroStoryboard"] as Storyboard;

            base.DataContextChanged += this.HandleDataContextChanged;
        }



        void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _gameOver = base.DataContext as GameOverViewModel;
        }

        void HandlePlayAgainHyperlinkClick(object sender, RoutedEventArgs e)
        {
            _gameOver.StartNewGame();
            _outroStoryboard.Begin(this);
        }

        void HandleOutroCompleted(object sender, EventArgs e)
        {
            _gameOver.Close();
        }



        GameOverViewModel _gameOver;
        readonly Storyboard _outroStoryboard;

    }
}