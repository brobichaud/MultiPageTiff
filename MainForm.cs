using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Tools;

namespace App
{
	public partial class MainForm : Form, IDropFileTarget
	{
		private DropFileHandler _dropFileHandler;

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			_dropFileHandler = new DropFileHandler(this);
		}

		public ImageCodecInfo GetEncoder(ImageFormat format)
		{
			foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
			{
				if (codec.FormatID == format.Guid)
					return codec;
			}

			return null;
		}

		public void DroppedFiles(Array fileList)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = ".tif";
			dlg.Title = "Select file name for multi-page tiff";
			if (dlg.ShowDialog() != DialogResult.OK)
				return;

			ImageCodecInfo info = GetEncoder(ImageFormat.Tiff);
			EncoderParameters ep = new EncoderParameters(2);
			ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, Convert.ToInt64(EncoderValue.MultiFrame));
			ep.Param[1] = new EncoderParameter(Encoder.Compression, Convert.ToInt64(EncoderValue.CompressionLZW));
			Bitmap pages = null;

			try
			{
				int frame = 1;
				foreach (string file in fileList)
				{
					if (frame == 1)
					{
						// save the first frame
						pages = (Bitmap)Image.FromFile(file);
						pages.Save(dlg.FileName, info, ep);
					}
					else
					{
						// save the intermediate frames
						ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, Convert.ToInt64(EncoderValue.FrameDimensionPage));
						using (Bitmap bm = (Bitmap)Image.FromFile(file))
						{
							pages.SaveAdd(bm, ep);
						}
					}

					if (frame == fileList.Length)
					{
						// flush and close
						ep.Param[0] = new EncoderParameter(Encoder.SaveFlag, Convert.ToInt64(EncoderValue.Flush));
						pages.SaveAdd(ep);
					}

					frame++;
				}
			}
			catch (Exception e)
			{
				string mess = string.Format("Unexpected error: {0}", e.Message);
				MessageBox.Show(mess, "Error saving", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				if (pages != null)
					pages.Dispose();
			}
		}
	}
}
