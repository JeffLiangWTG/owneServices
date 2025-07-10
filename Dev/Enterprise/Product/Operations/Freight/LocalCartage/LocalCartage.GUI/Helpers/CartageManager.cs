using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public abstract class CartageManager : ICartageExporter
	{
		public void ExportCartageBooking(INotifications notify, CancellationToken token)
		{
			CommonCartageBookingValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			CommonCartageXmlExportToEmailDirector director = new CommonCartageXmlExportToEmailDirector(adapter);
			director.RunExport(this, notify, token);
		}

		public void ExportCartageBookingAsFile(INotifications notify, CancellationToken token)
		{
			CommonCartageBookingValueObjectDataAdapter adapter = new CommonCartageBookingValueObjectDataAdapter();
			CommonCartageXmlExportToFileDirector director = new CommonCartageXmlExportToFileDirector(adapter);
			director.RunExport(this, notify, token);
		}

		public void ExportCartageStatus(INotifications notify, CancellationToken token)
		{
			CommonCartageStatusValueObjectDataAdapter adapter = new CommonCartageStatusValueObjectDataAdapter();
			CommonCartageXmlExportToEmailDirector director = new CommonCartageXmlExportToEmailDirector(adapter);
			director.RunExport(this, notify, token);
		}

		public void ExportCartageStatusAsFile(INotifications notify, CancellationToken token)
		{
			CommonCartageStatusValueObjectDataAdapter adapter = new CommonCartageStatusValueObjectDataAdapter();
			CommonCartageXmlExportToFileDirector director = new CommonCartageXmlExportToFileDirector(adapter);
			director.RunExport(this, notify, token);
		}

		OrgHeader ICartageExporter.SendTo { get { return SendTo; } }
		protected abstract OrgHeader SendTo { get; }

		ZString ICartageExporter.SendToDescription { get { return SendToDescription; } }
		protected abstract ZString SendToDescription { get; }

		ZString ICartageExporter.Description { get { return Description; } }
		protected abstract ZString Description { get; }

		CommonCartage ICartageExporter.GetCartageForExport(NotificationBuffer buffer) { return GetCartageForExport(buffer); }
		protected abstract CommonCartage GetCartageForExport(NotificationBuffer buffer);

		BusinessObjectFactory ICartageExporter.ParentFactory { get { return ParentFactory; } }
		protected abstract BusinessObjectFactory ParentFactory { get; }

		ZString ICartageExporter.ParentJobNumber { get { return ParentJobNumber; } }
		protected abstract ZString ParentJobNumber { get; }

		Logs ICartageExporter.ParentLogs { get { return ParentLogs; } }
		protected abstract Logs ParentLogs { get; }

		Notes ICartageExporter.ParentNotes { get { return ParentNotes; } }
		protected abstract Notes ParentNotes { get; }

		bool ICartageExporter.IsParentSaved { get { return IsParentSaved; } }
		protected abstract bool IsParentSaved { get; }

		CommonCartageType ICartageExporter.CartageJobType { get { return CartageJobType; } }
		protected abstract CommonCartageType CartageJobType { get; }

		void ICartageExporter.CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn) { CartageAdvised(factoryToCartageAdviseIn); }
		protected abstract void CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn);
	}
}
