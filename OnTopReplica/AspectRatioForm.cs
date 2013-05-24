﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OnTopReplica.Native;
using WindowsFormsAero.Dwm.Helpers;

namespace OnTopReplica {

    /// <summary>
    /// Form that automatically keeps a certain aspect ratio and resizes without flickering.
    /// </summary>
    public class AspectRatioForm : GlassForm {

        bool _keepAspectRatio = true;

        /// <summary>
        /// Gets or sets whether the form should keep its aspect ratio.
        /// </summary>
        /// <remarks>
        /// Refreshes the window's size if set to true.
        /// </remarks>
        [Description("Enables fixed aspect ratio for this form."), Category("Appearance"), DefaultValue(true)]
        public bool KeepAspectRatio {
            get {
                return _keepAspectRatio;
            }
            set {
                _keepAspectRatio = value;
                
                if (value)
                    RefreshAspectRatio();
            }
        }

        double _aspectRatio = 1.0;

        /// <summary>
        /// Gets or sets the form's aspect ratio that will be kept automatically when resizing.
        /// </summary>
        [Description("Determines this form's fixed aspect ratio."), Category("Appearance"), DefaultValue(1.0)]
        public double AspectRatio {
            get {
                return _aspectRatio;
            }
            set {
                if (value <= 0.0 || Double.IsInfinity(value))
                    return;

                _aspectRatio = value;
            }
        }

        Padding _extraPadding;

        /// <summary>
        /// Gets or sets some additional internal padding of the form that is ignored when keeping the aspect ratio.
        /// </summary>
        [Description("Sets some padding inside the form's client area that is ignored when keeping the aspect ratio."),
            Category("Appearance")]
        public Padding ExtraPadding {
            get {
                return _extraPadding;
            }
            set {
                _extraPadding = value;
                
                if(KeepAspectRatio)
                    RefreshAspectRatio();
            }
        }

        /// <summary>
        /// Forces the form to update its height based on the current aspect ratio setting.
        /// </summary>
        public void RefreshAspectRatio() {
            int newWidth = ClientSize.Width;
            int newHeight = (int)((ClientSize.Width - ExtraPadding.Horizontal) / AspectRatio) + ExtraPadding.Vertical;
            
            //Adapt height if it doesn't respect the form's minimum size
            Size clientMinimumSize = FromSizeToClientSize(MinimumSize);
            if (newHeight < clientMinimumSize.Height) {
                newHeight = clientMinimumSize.Height;
                newWidth = (int)((newHeight - ExtraPadding.Vertical) * AspectRatio) + ExtraPadding.Horizontal;
            }

            //Adapt height if it exceeds the screen's height
            var workingArea = Screen.GetWorkingArea(this);
            if (newHeight >= workingArea.Height) {
                newHeight = workingArea.Height;
                newWidth = (int)((newHeight - ExtraPadding.Vertical) * AspectRatio) + ExtraPadding.Horizontal;
            }

            //Update size
            ClientSize = new Size(newWidth, newHeight);

            //Move form vertically to adapt to new size
            //REMOVED: allows the window to correctly be restored slightly off screen
            /*if (Location.Y + Size.Height > workingArea.Y + workingArea.Height) {
                int offsetY = (workingArea.Y + workingArea.Height) - (Location.Y + Size.Height);
                Location = new Point(Location.X, Location.Y - offsetY);
            }*/
        }

        /// <summary>
        /// Adjusts the size of the form by a pixel increment while keeping its aspect ratio.
        /// </summary>
        /// <param name="pixelIncrement">Change of size in pixels.</param>
        public void AdjustSize(int pixelOffset) {
            Size origSize = Size;

            //Resize to new width (clamped to max allowed size and minimum form size)
            int newWidth = Math.Max(Math.Min(origSize.Width + pixelOffset, 
                SystemInformation.MaxWindowTrackSize.Width),
                MinimumSize.Width);

            //Determine new height while keeping aspect ratio
            int newHeight = (int)((newWidth - ExtraPadding.Horizontal - clientSizeConversionWidth) / AspectRatio) + ExtraPadding.Vertical + clientSizeConversionHeight;

            //Apply and move form to recenter
            Size = new Size(newWidth, newHeight);
            int deltaX = Size.Width - origSize.Width;
            int deltaY = Size.Height - origSize.Height;
            Location = new System.Drawing.Point(Location.X - (deltaX / 2), Location.Y - (deltaY / 2));
        }

        /// <summary>
        /// Updates the aspect ratio of the form and optionally forces a refresh.
        /// </summary>
        /// <param name="aspectRatioSource">Size from which aspect ratio should be computed.</param>
        /// <param name="forceRefresh">True if the size of the form should be refreshed to match the new aspect ratio.</param>
        public void SetAspectRatio(Size aspectRatioSource, bool forceRefresh) {
            AspectRatio = ((double)aspectRatioSource.Width / (double)aspectRatioSource.Height);
            _keepAspectRatio = true;

#if DEBUG
            System.Diagnostics.Trace.WriteLine(string.Format("Setting aspect ratio of {0} (for {1}).", AspectRatio, aspectRatioSource));
#endif
            
            if (forceRefresh) {
                RefreshAspectRatio();
            }
        }

        #region Event overriding

        protected override void OnResizeEnd(EventArgs e) {
            base.OnResizeEnd(e);

            //Ensure that the ClientSize of the form is always respected
            //(not ensured by the WM_SIZING message alone because of rounding errors and the chrome space)
            if (KeepAspectRatio) {
                var newHeight = (int)Math.Round(((ClientSize.Width - ExtraPadding.Horizontal) / AspectRatio) + ExtraPadding.Vertical);
                ClientSize = new Size(ClientSize.Width, newHeight);
            }
        }

        /// <summary>
        /// Override WM_SIZING message to restrict resizing.
        /// Taken from: http://www.vcskicks.com/maintain-aspect-ratio.php
        /// Improved with code from: http://stoyanoff.info/blog/2010/06/27/resizing-forms-while-keeping-aspect-ratio/
        /// </summary>
        protected override void WndProc(ref Message m) {
            if (KeepAspectRatio && m.Msg == WM.SIZING) {
                var rc = (Native.NRectangle)Marshal.PtrToStructure(m.LParam, typeof(Native.NRectangle));
                int res = m.WParam.ToInt32();

                int width = (rc.Right - rc.Left) - clientSizeConversionWidth - ExtraPadding.Horizontal;
                int height = (rc.Bottom - rc.Top) - clientSizeConversionHeight - ExtraPadding.Vertical;

                if (res == WMSZ.LEFT || res == WMSZ.RIGHT) {
                    //Left or right resize, adjust top and bottom
                    int targetHeight = (int)(width / AspectRatio);
                    int diffHeight = height - targetHeight;

                    rc.Top += (int)(diffHeight / 2.0);
                    rc.Bottom = rc.Top + targetHeight + ExtraPadding.Vertical + clientSizeConversionHeight;
                }
                else if (res == WMSZ.TOP || res == WMSZ.BOTTOM) {
                    //Up or down resize, adjust left and right
                    int targetWidth = (int)(height * AspectRatio);
                    int diffWidth = width - targetWidth;

                    rc.Left += (int)(diffWidth / 2.0);
                    rc.Right = rc.Left + targetWidth + ExtraPadding.Horizontal + clientSizeConversionWidth;
                }
                else if (res == WMSZ.RIGHT + WMSZ.BOTTOM || res == WMSZ.LEFT + WMSZ.BOTTOM) {
                    //Lower corner resize, adjust bottom
                    rc.Bottom = rc.Top + (int)(width / AspectRatio) + ExtraPadding.Vertical + clientSizeConversionHeight;
                }
                else if (res == WMSZ.LEFT + WMSZ.TOP || res == WMSZ.RIGHT + WMSZ.TOP) {
                    //Upper corner resize, adjust top
                    rc.Top = rc.Bottom - (int)(width / AspectRatio) - ExtraPadding.Vertical - clientSizeConversionHeight;
                }

                Marshal.StructureToPtr(rc, m.LParam, false);
            }

            base.WndProc(ref m);
        }

        #endregion

        #region ClientSize/Size conversion helpers

        int clientSizeConversionWidth, clientSizeConversionHeight;

        /// <summary>
        /// Converts a client size measurement to a window size measurement.
        /// </summary>
        /// <param name="clientSize">Size of the window's client area.</param>
        /// <returns>Size of the whole window.</returns>
        public Size FromClientSizeToSize(Size clientSize) {
            return new Size(clientSize.Width + clientSizeConversionWidth, clientSize.Height + clientSizeConversionHeight);
        }

        /// <summary>
        /// Converts a window size measurement to a client size measurement.
        /// </summary>
        /// <param name="size">Size of the whole window.</param>
        /// <returns>Size of the window's client area.</returns>
        public Size FromSizeToClientSize(Size size) {
            return new Size(size.Width - clientSizeConversionWidth, size.Height - clientSizeConversionHeight);
        }

        protected override void OnShown(EventArgs e) {
            base.OnShown(e);

            clientSizeConversionWidth = Size.Width - ClientSize.Width;
            clientSizeConversionHeight = Size.Height - ClientSize.Height;
        }

        #endregion

    }

}
