﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace OnTopReplica {

    /// <summary>
    /// Extension methods for windows forms.
    /// </summary>
    static class WindowsFormsExtensions {

        /// <summary>
        /// Internationalizes the text of a LinkLabel instance.
        /// </summary>
        /// <param name="text">Main text without link. Contains a '%' character which will be replaced by the link.</param>
        /// <param name="linkText">Linked text.</param>
        public static void Internationalize(this LinkLabel label, string text, string linkText) {
            int linkIndex = text.IndexOf('%');
            if (linkIndex == -1) {
                //Shouldn't happen, but try to fail with meaningful text
                label.Text = text;
                return;
            }

            label.Text = text.Substring(0, linkIndex) + linkText + text.Substring(linkIndex + 1);
            label.LinkArea = new LinkArea(linkIndex, linkText.Length);
        }

        /// <summary>
        /// Makes a safe GUI invoke on a form's GUI thread.
        /// </summary>
        /// <param name="action">The action to be executed on the GUI's thread.</param>
        /// <remarks>
        /// If the form is invalid or disposed, the action is not performed.
        /// </remarks>
        public static void SafeInvoke(this Form form, Action action) {
            if (form == null || form.IsDisposed)
                return;

            if (form.InvokeRequired) {
                form.Invoke(action);
            }
            else {
                action();
            }
        }

    }
}
