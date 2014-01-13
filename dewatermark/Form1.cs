using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace dewatermark
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnDeWaterMarkByChooseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open Image File";
            ofd.Filter = "All Image Files(*.bmp,*.jpg,*.png,*.gif)|*.bmp;*.jpg;*.png;*.gif";
            ofd.Multiselect = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                int processed_images_count = 0;
                foreach (string filename in ofd.FileNames)
                {
                    try
                    {
                        Bitmap temp = (Bitmap)Bitmap.FromFile(filename);

                        if (Math.Max(temp.Width, temp.Height) == 600)
                        {
                            temp.Dispose();
                            DeWaterMark.DeLarge(filename);
                        }
                        else
                        {
                            temp.Dispose();
                            DeWaterMark.DeSmall(filename);
                        }

                        processed_images_count++;
                        if (processed_images_count == 1000)
                        {
                            processed_images_count = 0;
                            Application.DoEvents();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorlogPath = Path.GetDirectoryName(filename) + "\\errorLog.txt";

                        FileInfo fileInfo = new FileInfo(errorlogPath);
                        using (FileStream fs = new FileStream(errorlogPath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            StreamWriter sw = new StreamWriter(fs);
                            sw.BaseStream.Seek(0, SeekOrigin.End);

                            sw.Write("\r\nTime: {0} {1}\r\n", System.DateTime.Now.ToShortDateString(), System.DateTime.Now.ToLongTimeString());
                            sw.Write("Error message: " + Path.GetFileName(filename) + "   " + ex.Message + "\r\n");
                            sw.Flush();
                            sw.Close();
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }
    }
}
