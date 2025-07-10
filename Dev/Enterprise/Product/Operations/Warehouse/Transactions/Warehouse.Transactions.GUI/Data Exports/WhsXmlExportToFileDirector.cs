using System.Globalization;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract class WhsXmlExportToFileDirector<TDocket, TValueObject> : WhsXmlExportDirector<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsXmlExportToFileDirector(WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
			: base(adapter)
		{
			RequestFileNameFromUser = true;
		}

		#region Properties

		public bool RequestFileNameFromUser
		{
			get;
			set;
		}

		#endregion

		#region Overrides

		protected override bool ExportToXmlCore(TDocket docket, INotifications notify)
		{
			bool exported = false;
			var stream = GetFileStream(docket);
			if (stream != null)
			{
				using (var writer = new StreamWriter(stream))
				{
					XmlExporter.Export(docket, writer, notify);
					exported = true;
				}
			}
			return exported;
		}

		#endregion

		#region Implementation

		protected virtual Stream GetFileStream(TDocket docket)
		{
			Stream result = null;
			using (var dialog = new ZSaveFileDialog())
			{
				dialog.CheckPathExists = true;
				dialog.Filter = (NoResString)"Xml Files *.xml|*.xml"; // File Extension Filter
				dialog.DefaultExt = (NoResString)"xml"; // File Extension Filter
				dialog.AddExtension = true;
				dialog.FileName = GetFileName(docket);

				if ((ZDialogResult)ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == ZDialogResult.OK)
				{
					result = dialog.OpenFile();
				}

				return result;
			}
		}

		protected ZString GetFileName(TDocket docket)
		{
			return docket.WD_ExternalReference.Trim() + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.CurrentCulture) + GetFileExtension();
		}

		#endregion
	}
}
