using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusMAWBWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public CusMAWBWorkflowInformationProvider(CusMAWB cusMAWB)
		{
			this.cusMAWB = cusMAWB;
		}

		MasterFiles.Tracking.TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext
		{
			get { return Enterprise.MasterFiles.Tracking.TrackingConstants.BusinessContext.NoBusinessContext; }
		}

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies
		{
			get { return new ZGuid[] { cusMAWB.Branch.Company.PK }; }
		}

		ZString IWorkflowInformationProvider.Destination
		{
			get { return cusMAWB.CM_RL_NKDischargePort; }
		}

		ZString IWorkflowInformationProvider.Origin
		{
			get { return cusMAWB.CM_RL_NKLoadPort; }
		}

		readonly CusMAWB cusMAWB;
	}
}
