﻿using System;
using System.Windows.Forms;
using OnTopReplica.Native;
using WindowsFormsAero.Dwm;

namespace OnTopReplica.Platforms {

    class WindowsSeven : WindowsVista {

        public override void PreHandleFormInit() {
            //Set Application ID
            WindowsSevenMethods.SetCurrentProcessExplicitAppUserModelID("LorenzCunoKlopfenstein.OnTopReplica.MainForm");
        }

        public override void PostHandleFormInit(MainForm form) {
            DwmManager.SetWindowFlip3dPolicy(form, Flip3DPolicy.ExcludeAbove);
            DwmManager.SetExcludeFromPeek(form, true);
            DwmManager.SetDisallowPeek(form, true);
        }

        public override void HideForm(MainForm form) {
            form.Opacity = 0;
        }

        public override bool IsHidden(MainForm form) {
            return (form.Opacity == 0.0);
        }

        public override void RestoreForm(MainForm form) {
            if (form.Opacity == 0.0)
                form.Opacity = 1.0;
            
            form.Show();
        }

        /*public override void OnFormStateChange(MainForm form) {
            //SetWindowStyle(form);
        }*/

        /// <summary>
        /// Used to alter the window style. Not used anymore.
        /// </summary>
        private void SetWindowStyle(MainForm form) {
            if (!form.FullscreenManager.IsFullscreen) {
                //This hides the app from ALT+TAB
                //Note that when minimized, it will be shown as an (ugly) minimized tool window
                //thus we do not minimize, but set to transparent when hiding
                long exStyle = WindowMethods.GetWindowLong(form.Handle, WindowMethods.WindowLong.ExStyle).ToInt64();

                exStyle |= (long)(WindowMethods.WindowExStyles.ToolWindow);
                //exStyle &= ~(long)(WindowMethods.WindowExStyles.AppWindow);

                WindowMethods.SetWindowLong(form.Handle, WindowMethods.WindowLong.ExStyle, new IntPtr(exStyle));

                //WindowMethods.SetWindowLong(form.Handle, WindowMethods.WindowLong.HwndParent, WindowManagerMethods.GetDesktopWindow());
            }
        }

    }

}
