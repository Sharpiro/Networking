using System;
using System.Windows;
using JetBrains.Annotations;

namespace Networking.GuiClient.Tools
{
    public static class Extensions
    {
        [NotNull]
        public static Window GetParentWindow([NotNull]this FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            try
            {
                if (element.Parent == null)
                    throw new NullReferenceException("While climbing the tree, the parent returned null before a window was found");
                if (element.Parent is Window window)
                    return window;
                if (element.Parent is FrameworkElement frameworkElement)
                    return GetParentWindow(frameworkElement);
                throw new InvalidOperationException($"The parent type '{element.Parent.GetType().Name}' of element of type '{element.GetType().Name}' could not be cast to a '{nameof(FrameworkElement)}'");
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to find parent window of element", ex);
            }
        }
    }
}