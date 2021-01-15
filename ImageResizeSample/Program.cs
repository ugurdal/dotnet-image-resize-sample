using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using PhotoSauce.MagicScaler;

namespace ImageResizeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            const int thumbnailWidth = 156;
            const int thumbnailHeight = 156;
            const int imageWidth = 712;
            const int imageHeight = 650;

            const string path = @"C:\DerinSIS\DerinBilgiGorseller\";
            var thumbPath = Path.Combine(path, "Thumbs");
            var imagePath = Path.Combine(path, "Images");

            // G{0} -> Model klasörü
            // V{0} -> Model klasörü içindeki ürünler
            //U -> Ürün klasörü

            var pictures = new List<PictureInfoModel>();
            var directory = new DirectoryInfo(path);
            // foreach (var item in directory.GetFiles().Where(x => x.LastWriteTime >= DateTime.Now.AddDays(-2)))
            foreach (var folder in directory.GetFileSystemInfos())
            {
                if (folder.Name.StartsWith("G"))
                {
                    //Model klasörü içinden ürünleri ayrıştır
                    if (int.TryParse(folder.Name.Replace("G", ""), out var modelId))
                    {
                        var subDirectory = new DirectoryInfo(Path.Combine(path, folder.FullName));
                        var counter = 0;
                        foreach (var productInModel in subDirectory.GetFiles())
                        {
                            var productName = productInModel.Name.Split('_')[0].ToString().Replace("V", "");
                            if (int.TryParse(productName, out var productId))
                            {
                                pictures.Add(new PictureInfoModel
                                {
                                    FileName = productInModel.FullName,
                                    LastWriteTime = productInModel.LastWriteTime,
                                    ModelId = modelId,
                                    ProductId = productId,
                                    PathName = Path.Combine(@"\", folder.Name, productInModel.Name),
                                    Sequence = counter
                                });
                                counter++;
                            }
                        }
                    }
                }

                if (folder.Name.StartsWith("U"))
                {
                    if (int.TryParse(folder.Name.Replace("U", ""), out var productId))
                    {
                        var subDirectory = new DirectoryInfo(Path.Combine(path, folder.FullName));
                        var counter = 0;
                        foreach (var productInModel in subDirectory.GetFiles())
                        {
                            pictures.Add(new PictureInfoModel
                            {
                                FileName = productInModel.FullName,
                                LastWriteTime = productInModel.LastWriteTime,
                                ModelId = 0,
                                ProductId = productId,
                                PathName = Path.Combine(@"\", folder.Name, productInModel.Name),
                                Sequence = counter
                            });
                            counter++;
                        }
                    }
                }
            }

            pictures = pictures.GroupBy(x => x.ProductId)
            .SelectMany(x => x.Select((y, i) => new { y, i }))
            .Select(x => new PictureInfoModel
            {
                ProductId = x.y.ProductId,
                FileName = x.y.FileName,
                LastWriteTime = x.y.LastWriteTime,
                ModelId = x.y.ModelId,
                PathName = x.y.PathName,
                Sequence = x.i + 1
            }).ToList();


            // foreach (var item in pictures.OrderBy(x => x.ProductId))
            // {
            //     System.Console.WriteLine(item);
            // }

            if (pictures.Any())
            {
                //Thumbnails
                if (!Directory.Exists(thumbPath))
                    Directory.CreateDirectory(thumbPath);

                var imageSettings = new ProcessImageSettings
                {
                    Width = thumbnailWidth,
                    Height = thumbnailHeight,
                    ResizeMode = CropScaleMode.Max,
                    SaveFormat = FileFormat.Jpeg,
                    JpegQuality = 100,
                    JpegSubsampleMode = ChromaSubsampleMode.Subsample420
                };

                foreach (var item in pictures)
                {
                    using var outStream = new FileStream(Path.Combine(thumbPath, item.OutputName), FileMode.Create);
                    MagicImageProcessor.ProcessImage(item.FileName, outStream, imageSettings);
                }

                //Standart Images
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);

                imageSettings.Width = imageWidth;
                imageSettings.Height = imageHeight;

                foreach (var item in pictures)
                {
                    using var outStream = new FileStream(Path.Combine(imagePath, item.OutputName), FileMode.Create);
                    MagicImageProcessor.ProcessImage(item.FileName, outStream, imageSettings);
                }

                //Write last run time to drn2Table -> value 9999;
                //Save image info to Database
                //check existing row
                var dbModel = new
                {
                    ProductId = 1,
                    ModelId = 1,
                    ThumbnailName = Path.Combine(thumbPath, "Image-Name"),
                    ImageName = Path.Combine(imagePath, "Image-Name"),
                    Sequence = 1,
                    OriginalFileName = "",
                    LastWriteTime = DateTime.Now
                };



            }
        }
    }
}