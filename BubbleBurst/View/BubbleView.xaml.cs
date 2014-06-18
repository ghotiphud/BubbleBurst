using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BubbleBurst.ViewModel;

namespace BubbleBurst.View
{
    /// <summary>
    /// Displays a bubble.
    /// </summary>
    public partial class BubbleView : Button
    {
        
        public BubbleView()
        {
            InitializeComponent();

            base.DataContextChanged += this.HandleDataContextChanged;
            base.MouseEnter += this.HandleMouseEnter;
            base.MouseLeave += this.HandleMouseLeave;
        }

        
        
        void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _bubble = e.NewValue as BubbleViewModel;
        }

        void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            if (_bubble != null)
            {
                _bubble.VerifyGroupMembership(true);
            }
        }

        void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            if (_bubble != null)
            {
                _bubble.VerifyGroupMembership(false);
            }
        }

        
        
        BubbleViewModel _bubble;

            }
}