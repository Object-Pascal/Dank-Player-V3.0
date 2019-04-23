using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Dank_Player_V3._0
{
    static class Animation
    {
        public static void AnimateGridObjectMargin(UIElement element, Thickness from, Thickness to, TimeSpan duration)
        {
            ThicknessAnimation pa = new ThicknessAnimation();

            pa.From = from;
            pa.To = to;
            pa.Duration = new Duration(duration);

            element.BeginAnimation(Grid.MarginProperty, pa);

        }

        public static void AnimateGridObjectCanvasLeft(UIElement element, double to, TimeSpan duration)
        {
            DoubleAnimation pa = new DoubleAnimation();

            pa.To = to;
            pa.Duration = new Duration(duration);

            element.BeginAnimation(Canvas.LeftProperty, pa);

        }

        public static void AnimateGridObjectCanvasTop(UIElement element, double to, TimeSpan duration)
        {
            DoubleAnimation pa = new DoubleAnimation();

            pa.To = to;
            pa.Duration = new Duration(duration);

            element.BeginAnimation(Canvas.TopProperty, pa);

        }

        public static void AnimateGridObjectMargin(UIElement element, Thickness to, TimeSpan duration)
        {
            ThicknessAnimation pa = new ThicknessAnimation();

            pa.To = to;
            pa.Duration = new Duration(duration);

            element.BeginAnimation(Grid.MarginProperty, pa);
        }

        public static void AnimateGridObjectOpacity(UIElement element, double from, double to, TimeSpan duration, bool fade = false)
        {
            DoubleAnimation pa = new DoubleAnimation();

            pa.From = from;
            pa.To = to;
            pa.Duration = new Duration(duration);

            element.BeginAnimation(Grid.OpacityProperty, pa);

            if (fade)
            {
                DispatcherTimer completed = new DispatcherTimer();
                completed.Interval = duration;
                completed.Start();

                completed.Tick += (s, e) =>
                {
                    AnimateGridObjectOpacity(element, to, from, duration);
                    completed.Stop();
                };
            }
        }

        public static void AnimateGridObjectOpacity(UIElement element, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.OpacityProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateGridObjectOpacity(UIElement element, double to, TimeSpan duration, Action animationCompleted)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.Completed += (s, e) =>
                {
                    animationCompleted.Invoke();
                };

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.OpacityProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateGridObjectOpacitySafeDelete(Grid parent, UIElement element, double from, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.From = from;
                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.OpacityProperty, pa);

                Thread garbageThread = new Thread(() =>
                {
                    Thread.Sleep(duration.Milliseconds);
                    parent.Dispatcher.Invoke(() => {
                        parent.Children.Remove(element);
                    });
                });
                garbageThread.Start();
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateGridObjectHeight(UIElement element, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.HeightProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateGridObjectWidth(UIElement element, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.WidthProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateGridObjectWidth(UIElement element, double from, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.From = from;
                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(Grid.WidthProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateTextBlockObjectFontSize(TextBlock element, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(TextBlock.FontSizeProperty, pa);
            }
            catch (NullReferenceException) { }
        }

        public static void AnimateLabelObjectFontSize(Label element, double to, TimeSpan duration)
        {
            try
            {
                DoubleAnimation pa = new DoubleAnimation();

                pa.To = to;
                pa.Duration = new Duration(duration);

                element.BeginAnimation(TextBlock.FontSizeProperty, pa);
            }
            catch (NullReferenceException) { }
        }
    }
}
