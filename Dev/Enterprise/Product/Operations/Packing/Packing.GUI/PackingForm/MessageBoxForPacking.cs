using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Scanning;

namespace Enterprise.Packing.GUI
{
	class MessageBoxForPacking : ZMessageBox
	{
		public MessageBoxForPacking(string message, string caption, MessageBoxIcon icon)
			: base(message, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1)
		{
			InitializeForm();
		}

		public MessageBoxForPacking(MultilingualString message, string caption, MessageBoxIcon icon)
			: base(message, caption, MessageBoxButtons.OK, icon, MessageBoxDefaultButton.Button1)
		{
			InitializeForm();
		}

		void InitializeForm()
		{
			KeyDown += (o, e) =>
			{
				if (ScanningManager.IsStartOrStopScan(e.KeyData))
				{
					IsScanningMode = !IsScanningMode;
					SuppressKey(e);
				}
				else if (IsScanningMode)
				{
					SuppressKey(e);
				}
			};

			KeyPreview = true;
		}

		static void SuppressKey(KeyEventArgs e)
		{
			e.Handled = true;
			e.SuppressKeyPress = true;
		}

		bool IsScanningMode;
	}
}
