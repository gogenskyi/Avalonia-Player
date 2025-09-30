using System;
using Avalonia.Media.Imaging;

namespace PlayerAvalonia.Models 
{
    public class Song
    {
        public string Title{get; set;}
        public string Artist{get; set;}
        public Bitmap AlbumArt{get; set;}
        public int Index{get; set;}
        public string FilePath{get; set;}
    }
}