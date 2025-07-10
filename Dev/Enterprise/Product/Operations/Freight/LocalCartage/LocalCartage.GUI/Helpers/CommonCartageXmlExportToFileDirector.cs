using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class CommonCartageXmlExportToFileDirector : CommonCartageXmlExportDirector
	{
		public CommonCartageXmlExportToFileDirector(CommonCartageValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		protected override void ExportToXmlCore(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token)
		{
			Stream fileStream = QueryUserForFileStream(cartageExporter);
			if (fileStream != null)
			{
				using (StreamWriter writer = new StreamWriter(fileStream))
				{
					XmlExporter.Export(cartageExporter, cartage, writer, buffer, token);
				}
			}
			else
			{
				SetWasCancelled(true);
			}
		}

		protected virtual Stream QueryUserForFileStream(ICartageExporter cartageExporter)
		{
			Stream result = null;
			var dialog = new ZSaveFileDialog();
			dialog.CheckPathExists = true;
			dialog.Filter = (NoResString)"Xml Files *.xml|*.xml";
			dialog.DefaultExt = (NoResString)"xml";
			dialog.AddExtension = true;
			dialog.FileName = cartageExporter.ParentJobNumber.ExcludeChars("/").Trim() + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

			if (dialog.ShowDialog() == DialogResult.OK)
			{
				result = dialog.OpenFile();
			}

			return result;
		}
	}
}
