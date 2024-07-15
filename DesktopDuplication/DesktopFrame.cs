using System.Drawing;

namespace DesktopDuplication
{
    /// <summary>
    /// Provides image data, cursor data, and image metadata about the retrieved desktop frame.
    /// </summary>
    public class DesktopFrame
    {
        /// <summary>
        /// Gets the bitmap representing the last retrieved desktop frame. This image spans the entire bounds of the specified monitor.
        /// </summary>
        public Bitmap DesktopImage { get; internal set; }
    }
}
