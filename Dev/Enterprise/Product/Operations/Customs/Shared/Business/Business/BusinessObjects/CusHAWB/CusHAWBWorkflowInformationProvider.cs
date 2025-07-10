using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public class CusHAWBWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public CusHAWBWorkflowInformationProvider(CusHAWB cusHAWB)
		{
			this.cusHAWB = cusHAWB;
		}

		MasterFiles.Tracking.TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext
		{
			get { return Enterprise.MasterFiles.Tracking.TrackingConstants.BusinessContext.NoBusinessContext; }
		}

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies
		{
			get { return cusHAWB.MAWB != null ? new ZGuid[] { cusHAWB.MAWB.Branch.Company.PK } : Array.Empty<ZGuid>(); }
		}

		ZString IWorkflowInformationProvider.Destination
		{
			get { return cusHAWB.CS_RL_NKDischargePort; }
		}

		ZString IWorkflowInformationProvider.Origin
		{
			get { return cusHAWB.CS_RL_NKLoadPort; }
		}

		readonly CusHAWB cusHAWB;
	}
}
