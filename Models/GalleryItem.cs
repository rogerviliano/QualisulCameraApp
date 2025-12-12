using System.Windows.Media;

namespace QualisulCameraApp.Models
{
    public class GalleryItem
    {
        public ImageSource Image { get; set; }
        public string FilePath { get; set; }

        public GalleryItem(ImageSource image, string filePath)
        {
            Image = image;
            FilePath = filePath;
        }
    }
}
