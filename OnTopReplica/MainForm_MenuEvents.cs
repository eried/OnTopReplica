﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using OnTopReplica.Properties;
using WindowsFormsAero.TaskDialog;
using OnTopReplica.SidePanels;

namespace OnTopReplica {
    partial class MainForm {

        private void Menu_opening(object sender, CancelEventArgs e) {
            //Cancel if currently in "fullscreen" mode or a side panel is open
            if (FullscreenManager.IsFullscreen || IsSidePanelOpen) {
                e.Cancel = true;
                return;
            }

            bool showing = _thumbnailPanel.IsShowingThumbnail;

            selectRegionToolStripMenuItem.Enabled = showing;
            switchToWindowToolStripMenuItem.Enabled = showing;
            resizeToolStripMenuItem.Enabled = showing;
            chromeToolStripMenuItem.Checked = IsChromeVisible;
            clickForwardingToolStripMenuItem.Checked = ClickForwardingEnabled;
            chromeToolStripMenuItem.Enabled = showing;
            clickThroughToolStripMenuItem.Enabled = showing;
            clickForwardingToolStripMenuItem.Enabled = showing;
        }

        private void Menu_Switch_click(object sender, EventArgs e) {
            if (CurrentThumbnailWindowHandle == null)
                return;

            Program.Platform.HideForm(this);
            Native.WindowManagerMethods.SetForegroundWindow(CurrentThumbnailWindowHandle.Handle);
        }

        private void Menu_Advanced_opening(object sender, EventArgs e) {
            restoreLastClonedWindowToolStripMenuItem.Checked = Settings.Default.RestoreLastWindow;
        }

        private void Menu_GroupSwitchMode_click(object sender, EventArgs e) {
            SetSidePanel(new SidePanels.GroupSwitchPanel());
        }

        private void Menu_RestoreLastWindow_click(object sender, EventArgs e) {
            Settings.Default.RestoreLastWindow = !Settings.Default.RestoreLastWindow;
        }

        private void Menu_ClickForwarding_click(object sender, EventArgs e) {
            ClickForwardingEnabled = !ClickForwardingEnabled;
        }

        private void Menu_ClickThrough_click(object sender, EventArgs e) {
            ClickThroughEnabled = true;
        }

        private void Menu_Opacity_opening(object sender, CancelEventArgs e) {
            ToolStripMenuItem[] items = {
				toolStripMenuItem1,
				toolStripMenuItem2,
				toolStripMenuItem3,
				toolStripMenuItem4
			};

            foreach (ToolStripMenuItem i in items) {
                if (((double)i.Tag) == this.Opacity)
                    i.Checked = true;
                else
                    i.Checked = false;
            }
        }

        private void Menu_Opacity_click(object sender, EventArgs e) {
            //Get clicked menu item
            ToolStripMenuItem tsi = sender as ToolStripMenuItem;

            if (tsi != null && this.Visible) {
                //Get opacity from the tag
                this.Opacity = (double)tsi.Tag;
                Program.Platform.OnFormStateChange(this);
            }
        }

        private void Menu_Region_click(object sender, EventArgs e) {
            SetSidePanel(new OnTopReplica.SidePanels.RegionPanel());
        }

        private void Menu_Resize_opening(object sender, CancelEventArgs e) {
            if (!_thumbnailPanel.IsShowingThumbnail)
                e.Cancel = true;

            restorePositionAndSizeToolStripMenuItem.Checked = Settings.Default.RestoreSizeAndPosition;
        }

        private void Menu_Resize_Double(object sender, EventArgs e) {
            FitToThumbnail(2.0);
        }

        private void Menu_Resize_FitToWindow(object sender, EventArgs e) {
            FitToThumbnail(1.0);
        }

        private void Menu_Resize_Half(object sender, EventArgs e) {
            FitToThumbnail(0.5);
        }

        private void Menu_Resize_Quarter(object sender, EventArgs e) {
            FitToThumbnail(0.25);
        }

        private void Menu_Resize_Fullscreen(object sender, EventArgs e) {
            FullscreenManager.SwitchFullscreen();
        }

        private void Menu_Resize_RecallPosition_click(object sender, EventArgs e) {
            Settings.Default.RestoreSizeAndPosition = !Settings.Default.RestoreSizeAndPosition;
        }

        private void Menu_Position_Opening(object sender, EventArgs e) {
            disabledToolStripMenuItem.Checked = (PositionLock == null);
            topLeftToolStripMenuItem.Checked = (PositionLock == ScreenPosition.TopLeft);
            topRightToolStripMenuItem.Checked = (PositionLock == ScreenPosition.TopRight);
            centerToolStripMenuItem.Checked = (PositionLock == ScreenPosition.Center);
            bottomLeftToolStripMenuItem.Checked = (PositionLock == ScreenPosition.BottomLeft);
            bottomRightToolStripMenuItem.Checked = (PositionLock == ScreenPosition.BottomRight);
        }

        private void Menu_Position_Disable(object sender, EventArgs e) {
            PositionLock = null;
        }

        private void Menu_Position_TopLeft(object sender, EventArgs e) {
            PositionLock = ScreenPosition.TopLeft;
        }

        private void Menu_Position_TopRight(object sender, EventArgs e) {
            PositionLock = ScreenPosition.TopRight;
        }

        private void Menu_Position_Center(object sender, EventArgs e) {
            PositionLock = ScreenPosition.Center;
        }

        private void Menu_Position_BottomLeft(object sender, EventArgs e) {
            PositionLock = ScreenPosition.BottomLeft;
        }

        private void Menu_Position_BottomRight(object sender, EventArgs e) {
            PositionLock = ScreenPosition.BottomRight;
        }

        private void Menu_Reduce_click(object sender, EventArgs e) {
            //Hide form in a platform specific way
            Program.Platform.HideForm(this);
        }

        private void Menu_Chrome_click(object sender, EventArgs e) {
            IsChromeVisible = !IsChromeVisible;
        }

        private void Menu_Settings_click(object sender, EventArgs e) {
            this.SetSidePanel(new OptionsPanel());
        }

        private void Menu_About_click(object sender, EventArgs e) {
            this.SetSidePanel(new AboutPanel());
        }

        private void Menu_Close_click(object sender, EventArgs e) {
            this.Close();
        }

        private void Menu_Fullscreen_ExitFullscreen_click(object sender, EventArgs e) {
            FullscreenManager.SwitchBack();
        }

        private void Menu_Fullscreen_Mode_opening(object sender, EventArgs e) {
            var mode = Settings.Default.GetFullscreenMode();

            menuModeStandardToolStripMenuItem.Checked = (mode == FullscreenMode.Standard);
            menuModeFullscreenToolStripMenuItem.Checked = (mode == FullscreenMode.Fullscreen);
            menuModeAllScreensToolStripMenuItem.Checked = (mode == FullscreenMode.AllScreens);
        }

        private void Menu_Fullscreen_Mode_Standard_click(object sender, EventArgs e) {
            Settings.Default.SetFullscreenMode(FullscreenMode.Standard);
            FullscreenManager.SwitchFullscreen(FullscreenMode.Standard);
        }

        private void Menu_Fullscreen_Mode_Fullscreen_click(object sender, EventArgs e) {
            Settings.Default.SetFullscreenMode(FullscreenMode.Fullscreen);
            FullscreenManager.SwitchFullscreen(FullscreenMode.Fullscreen);
        }

        private void Menu_Fullscreen_Mode_AllScreens_click(object sender, EventArgs e) {
            Settings.Default.SetFullscreenMode(FullscreenMode.AllScreens);
            FullscreenManager.SwitchFullscreen(FullscreenMode.AllScreens);
        }

    }
}
