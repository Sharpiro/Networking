using System;
using System.Windows;
using JetBrains.Annotations;

namespace Networking.GuiClient.Tools
{
    public static class Extensions
    {
        [NotNull]
        public static Window GetParentWindowRecurs([NotNull]this FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            try
            {
                if (element.Parent == null)
                    throw new NullReferenceException("While climbing the tree, the parent returned null before a window was found");
                if (element.Parent is Window window)
                    return window;
                return GetParentWindowRecurs((FrameworkElement)element.Parent);
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to find parent window of element", ex);
            }
        }
    }
}