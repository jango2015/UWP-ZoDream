﻿using ZoDream.Helper;
using ZoDreamToolkit.Common;

namespace ZoDream.Services
{
    /// <summary>
    /// Issues Toast Notifications.
    /// </summary>
    public static class Toast
    {
        /// <summary>
        /// Shows the specified text in a toast.
        /// </summary>
        public static void Show(string text)
        {
            Show(text, null);
        }

        /// <summary>
        /// Shows a toast with an info icon.
        /// </summary>
        public static void ShowInfo(string text)
        {
            Show(text, "Toasts/info.png");
        }

        /// <summary>
        /// Shows a toast with a warning icon.
        /// </summary>
        public static void ShowWarning(string text)
        {
            Show(text, "Toasts/warning.png");
        }

        /// <summary>
        /// Shows a toast with an error icon.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void ShowError(string text)
        {
            Show(text, "Toasts/confused.png");
        }

        /// <summary>
        /// Shows a toast with the specified text and icon.
        /// </summary>
        private static void Show(string text, string imagePath)
        {
            MessageHelper.ShowToastNotification(imagePath, text, NotificationAudioNames.Default);
        }
    }
}
