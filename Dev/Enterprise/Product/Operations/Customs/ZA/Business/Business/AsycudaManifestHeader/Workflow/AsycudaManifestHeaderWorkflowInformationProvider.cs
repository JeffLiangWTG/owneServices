using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaManifestHeaderWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public AsycudaManifestHeaderWorkflowInformationProvider(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies
		{
			get { return new ZGuid[] { header.Branch.Company.PK }; }
		}

		ZString IWorkflowInformationProvider.Destination
		{
			get { return header?.MasterBill?.FinalDestination?.Code ?? ZString.Empty; }
		}

		public ZString Origin
		{
			get { return header?.MasterBill?.Origin?.Code ?? ZString.Empty; }
		}

		MasterFiles.Tracking.TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext
		{
			get { return MasterFiles.Tracking.TrackingConstants.BusinessContext.NoBusinessContext; }
		}

		readonly AsycudaManifestHeader header;
	}
}
