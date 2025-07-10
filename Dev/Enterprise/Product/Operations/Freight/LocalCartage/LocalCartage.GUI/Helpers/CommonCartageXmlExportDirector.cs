using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public abstract class CommonCartageXmlExportDirector
	{
		protected CommonCartageXmlExportDirector(CommonCartageValueObjectDataAdapter adapter)
		{
			this.Adapter = adapter;
		}

		public void RunExport(ICartageExporter cartageExporter, INotifications notify, CancellationToken token)
		{
			NotificationBuffer buffer = new NotificationBuffer(notify);
			CheckExportConditionsAreMet(cartageExporter, buffer);
			if (!buffer.HasErrors)
			{
				CommonCartage cartage = cartageExporter.GetCartageForExport(buffer);
				if (!buffer.HasErrors)
				{
					ExportToXml(cartageExporter, cartage, buffer, token);
				}
			}
		}

		void ExportToXml(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token)
		{
			if (QueryAdditionalBookingInformation(cartageExporter, cartage, buffer))
			{
				ExportToXmlCore(cartageExporter, cartage, buffer, token);

				if (!WasCanceled && !buffer.HasErrors)
				{
					buffer.Notify(new InfoNotification(Res.GetString("77be9c25-cdbc-41eb-9896-a234ee734a94", "{0} Port Transport Job successfully exported to XML.", cartageExporter.Description)));
				}
			}
		}

		protected void SetWasCancelled(bool value)
		{
			WasCanceled = value;
		}

		bool WasCanceled;
		protected abstract void ExportToXmlCore(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token);

		protected virtual void CheckExportConditionsAreMet(ICartageExporter cartageExporter, NotificationBuffer buffer)
		{
			if (!cartageExporter.IsParentSaved)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("56f9c470-1717-4e82-85d4-7cc0d83e958a", "Please save your changes before you continue.")));
			}
			else if (OrgProxy == null)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("d6b6ff27-ae1f-4b39-8190-9aa4d076284f", "Please set an OrgProxy for your Company or your Branch.")));
			}
			else if (cartageExporter.SendTo == null)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("dfe611e1-e10e-401d-81c1-c7c3a859193c", "Please enter a valid {0}", cartageExporter.SendToDescription)));
			}
		}

		protected OrgHeader OrgProxy
		{
			get { return GlbCompany.CurrentCompany.OrgProxy ?? GlbBranch.CurrentBranch.OrgProxy; }
		}

		protected virtual bool QueryAdditionalBookingInformation(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer)
		{
			bool result = false;

			if (cartage != null)
			{
				var parent = ((BusinessObject)cartage.CartageParent);
				var sendTo = cartageExporter.SendTo;
				var acceptedRejected = ZString.Empty;

				if (HasEDILog(parent, FreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted, sendTo))
				{
					acceptedRejected = Res.GetString("c20f968c-6aa5-4d11-b1ec-54cfeea87ad4", "accepted");
				}
				else if (HasEDILog(parent, FreightConstants.LocalCartageBookingStatus.Codes.BookingRejected, sendTo))
				{
					acceptedRejected = Res.GetString("de50c715-af2d-4452-b491-0cf10571175f", "rejected");
				}

				if (!acceptedRejected.IsEmpty)
				{
					var sendToDescription = CommonCartageValueObjectDataAdapter.GetOrganisationDescription(sendTo);
					buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("d7adb1aa-2a8f-4636-9ecf-e187a9de22ae", "The Port Transport Booking has already been {0} by {1}. No modifications can be made. Please contact them directly.", acceptedRejected, sendToDescription)));
				}
				else if (cartage.BookingInformation != null)
				{
					result = (ZFormModaliser.ShowDialogAndDispose(new CommonBookingInformationForm(cartage.BookingInformation)) == DialogResult.OK);
				}
			}

			return result;
		}

		bool HasEDILog(BusinessObject parent, ZString actionCode, OrgHeader localTransportCo)
		{
			var result = false;

			if (parent != null)
			{
				var ediStatusDescription = CommonCartageValueObjectDataAdapter.GetStatusDescription(actionCode, true);
				var localTransportCoDescription = CommonCartageValueObjectDataAdapter.GetOrganisationDescription(localTransportCo);

				var logFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
				logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, ediStatusDescription);
				logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, localTransportCoDescription);
				result = parent.GetLogs().GetAllLogs().Find(logFilter).Length > 0;
			}

			return result;
		}

		protected CommonCartageXmlExporter XmlExporter
		{
			get { return xmlExporter ?? (xmlExporter = GetNewCommonCartageXmlExporter()); }
		}
		CommonCartageXmlExporter xmlExporter;

		protected virtual CommonCartageXmlExporter GetNewCommonCartageXmlExporter()
		{
			return new CommonCartageXmlExporter(Adapter);
		}

		protected readonly CommonCartageValueObjectDataAdapter Adapter;
	}
}
