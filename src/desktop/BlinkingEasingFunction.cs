using System.Windows;
using System.Windows.Media.Animation;

namespace deduper.wpf
{
    public class BlinkingEasingFunction : EasingFunctionBase
    {
        protected override double EaseInCore(double normalizedTime)
        {
            if (normalizedTime < 0.5)
                return 0;
            return 1;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BlinkingEasingFunction();
        }
    }
}