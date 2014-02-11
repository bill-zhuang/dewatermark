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
                removeWaterMark(ofd.FileNames);
            }
            else
            {
                return;
            }
        }

        private void btnDeWaterMarkByChooseFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                //not support folders in folder
                string[] files = Directory.GetFiles(fbd.SelectedPath);

                removeWaterMark(files);
            }
            else
            {
                return;
            }
        }

        private void removeWaterMark(string[] files_path)
        {
            int processed_images_count = 0;
            foreach (string filename in files_path)
            {
                DeWaterMark.RemoveWaterMark(filename);

                processed_images_count++;
                if (processed_images_count == 1000)
                {
                    processed_images_count = 0;
                    Application.DoEvents();
                }
            }

            MessageBox.Show("Finished.");
        }
    }
}
