using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace GraphSimulator.Helpers
{
    public static class StoryboardLibrary
    {
        public enum MoveDirection
        {
            UpDown,
            RightLeft
        }

        public static Storyboard MenuAnim(UIElement obj, bool isHide, double size, EasingFunctionBase function, EasingMode mode, MoveDirection moveDirection)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation doubleAnim = new DoubleAnimation();
            obj.RenderTransform = new TranslateTransform();

            if (isHide)
            {
                doubleAnim.From = 0;
                doubleAnim.To = -size;
            }
            else
            {
                doubleAnim.From = -size;
                doubleAnim.To = 0;
            }

            doubleAnim.Duration = new Duration(TimeSpan.FromSeconds(1.5));
            function.EasingMode = mode;
            doubleAnim.EasingFunction = function;

            Storyboard.SetTarget(doubleAnim, obj);
            if (moveDirection == MoveDirection.RightLeft)
                Storyboard.SetTargetProperty(doubleAnim, new PropertyPath("(FrameworkElement.RenderTransform).(TranslateTransform.X)"));
            else if (moveDirection == MoveDirection.UpDown)
                Storyboard.SetTargetProperty(doubleAnim, new PropertyPath("(FrameworkElement.RenderTransform).(TranslateTransform.Y)"));

            storyboard.Children.Add(doubleAnim);

            return storyboard;
        }

    }

}
