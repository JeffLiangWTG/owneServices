using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.Customs.Business
{
	public class CusSCAHouseWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public CusSCAHouseWorkflowInformationProvider(BaseCusSCAHouse house)
		{
			this.house = house;
		}

		ZString IWorkflowInformationProvider.Destination => house.CA_RL_NKDischargePort;

		ZString IWorkflowInformationProvider.Origin => house.CA_RL_NKLoadPort;

		TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext => TrackingConstants.BusinessContext.NoBusinessContext;

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies => house.OceanBill != null ? new ZGuid[] { house.OceanBill.Branch.Company.PK } : Array.Empty<ZGuid>();

		readonly BaseCusSCAHouse house;
	}
}
