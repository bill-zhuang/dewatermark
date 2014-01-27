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
            else
            {
                return;
            }
        }
    }
}
