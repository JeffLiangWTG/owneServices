using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public abstract class WhsXmlExportDirector<TDocket, TValueObject>
		where TDocket : WhsDocket
		where TValueObject : IValueObject
	{
		protected WhsXmlExportDirector(WhsValueObjectDataAdapter<TDocket, TValueObject> adapter)
		{
			Adapter = adapter;
		}

		#region Run Export

		public void RunExport(TDocket docket)
		{
			if (docket.HasChanges)
			{
				Globals.Message.Show(Res.GetString("da7745af-79f1-4ba0-9c1c-4cae8a80fd5e", "Please save your changes before you continue."));
			}
			else
			{
				RunExportCore(docket);
			}
		}

		#endregion

		#region Implementation

		protected abstract ZString GetDocketTypeDescription();

		void RunExportCore(TDocket docket)
		{
			var exportError = GetCheckExportConditionsAreMet(docket);
			if (exportError.IsEmpty)
			{
				try
				{
					ExportToXml(docket);
				}
				catch (Exception ex)
				{
					if (ex.IsCriticalException())
					{
						throw;
					}
					Globals.Message.ShowError(ex.Message, Res.GetString("4152651e-0dd8-4327-b788-a7e1bf258359", "Error Exporting {0} to XML", GetDocketTypeDescription()));
				}
			}
			else
			{
				Globals.Message.ShowError(exportError);
			}
		}

		void ExportToXml(TDocket docket)
		{
			if (docket != null)
			{
				var notify = new NotificationBuffer();
				ExportToXmlCore(docket, notify);

				if (notify.HasErrors)
				{
					Globals.Message.ShowError(notify.AsString);
				}
				else
				{
					AddSuccessNotification();
				}
			}
		}

		protected virtual void AddSuccessNotification()
		{
			Globals.Message.ShowInformation(Res.GetString("62f7062e-4e47-45ca-9ad7-07383ca0085a", "{0} successfully exported to XML.", GetDocketTypeDescription()), Res.GetString("b990abf4-b12f-464f-b8e4-b9410977253a", "Export {0} to XML", GetDocketTypeDescription()));
		}

		protected abstract bool ExportToXmlCore(TDocket docket, INotifications notify);

		#region Check Export Conditions Are Met

		protected virtual ZString GetCheckExportConditionsAreMet(TDocket docket)
		{
			var result = ZString.Empty;
			if (OrgProxy == null)
			{
				result = new ZString(Res.GetString("16332f33-fc9c-45eb-bd88-932887f3de78", "Please set an OrgProxy for your Company or your Branch."));
			}
			else if (docket.Client == null)
			{
				result = Res.GetString("6f5322f4-409c-485a-b470-b91dbd09cf0c", "Please enter a valid Client");
			}
			return result;
		}

		protected OrgHeader OrgProxy => GlbCompany.CurrentCompany.OrgProxy ?? GlbBranch.CurrentBranch.OrgProxy;

		#endregion

		#region XmlExporter

		public WhsXmlExporter<TDocket, TValueObject> XmlExporter => xmlExporter ?? (xmlExporter = GetNewXmlExporter());

		WhsXmlExporter<TDocket, TValueObject> xmlExporter;

		protected abstract WhsXmlExporter<TDocket, TValueObject> GetNewXmlExporter();

		#endregion

		protected virtual ZString GetFileExtension() => ".xml";

		protected readonly WhsValueObjectDataAdapter<TDocket, TValueObject> Adapter;

		#endregion
	}
}
