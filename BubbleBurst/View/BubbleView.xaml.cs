using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BubbleBurst.ViewModel;
using ReactiveUI;

namespace BubbleBurst.View
{
    /// <summary>
    /// Displays a bubble.
    /// </summary>
    public partial class BubbleView : Button, IViewFor<BubbleViewModel>
    {
        public BubbleViewModel ViewModel { get; set; }
        object IViewFor.ViewModel { get { return ViewModel; } set { ViewModel = (BubbleViewModel)value; } }

        public BubbleView()
        {
            InitializeComponent();

            base.DataContextChanged += this.HandleDataContextChanged;
            base.MouseEnter += this.HandleMouseEnter;
            base.MouseLeave += this.HandleMouseLeave;
        }

        void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ViewModel = e.NewValue as BubbleViewModel;
        }

        void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.VerifyGroupMembership(true);
            }
        }

        void HandleMouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.VerifyGroupMembership(false);
            }
        }
    }
}