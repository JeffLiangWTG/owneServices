using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	class DtbLinehaulManifestCostSupporter : LinehaulAndRunSheetCostSupporter<DtbLinehaulManifest>
	{
		public DtbLinehaulManifestCostSupporter(DtbLinehaulManifest manifest)
			: base(manifest)
		{
		}

		protected override ZGuid CreditorPK
		{
			get { return Parent.TransportCompanyPK; }
		}

		public override OrgHeader SendingForwarder
		{
			get { return Parent.OriginDepot != null ? Parent.OriginDepot.Header : null; }
		}

		public override OrgHeader ReceivingForwarder
		{
			get { return Parent.DestinationDepot != null ? Parent.DestinationDepot.Header : null; }
		}

		protected override IEnumerable<IJobInvoicingPlugIn> Consignments
		{
			get { return Parent.Consignments; }
		}
	}
}
