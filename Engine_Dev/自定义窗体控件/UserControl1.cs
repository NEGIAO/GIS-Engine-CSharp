using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace 自定义窗体控件
{
    public partial class UserControl1: UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void open_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdiPic = new OpenFileDialog();
            //ofdiPic;
            ofdiPic.Filter = "JPG(*.JPG;*.JPEG);gif文件（*.GIF)|*.jpg;*.jpeg;*.gif";
            ofdiPic.FilterIndex = 1;
            ofdiPic.RestoreDirectory = true;
            ofdiPic.FileName = "";
            if (ofdiPic.ShowDialog() == DialogResult.OK)
            {
                PictureBox picBox = pictureBox;//获取UI中的对象

                string sPicPath = ofdiPic.FileName.ToString();
                FileInfo fiPicInfo = new FileInfo(sPicPath);
                long PicLong = fiPicInfo.Length / 1024;
                string sPicName = fiPicInfo.Name;
                string sPicDirectory = fiPicInfo.Directory.ToString();
                string sPicDirectoryPath = fiPicInfo.DirectoryName;
                //mes.show()
                Bitmap bmPic = new Bitmap(sPicPath);
                if (PicLong > 400)
                {
                    MessageBox.Show("文件大小" + PicLong + "K;已超过最大限制的K范围！");
                }
                else
                {
                    
                    Point ptLocation = new Point(bmPic.Size);
                    if (ptLocation.X > picBox.Size.Width || ptLocation.Y > picBox.Size.Height)
                    {
                        picBox.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                    else
                    {
                        picBox.SizeMode = PictureBoxSizeMode.CenterImage;
                    }
                }
                picBox.LoadAsync(sPicPath);
                labName.Text = sPicName;
                lblLenth.Text = PicLong.ToString() + "KB";
                lblSize.Text = bmPic.Size.Width.ToString() + "x" + bmPic.Size.Height.ToString();
            }
        }
    }
}
