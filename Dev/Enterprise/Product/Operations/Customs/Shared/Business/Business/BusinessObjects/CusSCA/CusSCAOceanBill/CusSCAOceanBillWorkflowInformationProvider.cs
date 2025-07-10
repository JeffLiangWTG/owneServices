using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;

namespace Enterprise.Customs.Business
{
	public class CusSCAOceanBillWorkflowInformationProvider : IWorkflowInformationProvider
	{
		public CusSCAOceanBillWorkflowInformationProvider(BaseCusSCAOceanBill oceanBill)
		{
			this.oceanBill = oceanBill;
		}

		ZString IWorkflowInformationProvider.Destination => oceanBill.CB_RL_NKPortOfDischarge;

		ZString IWorkflowInformationProvider.Origin => oceanBill.CB_RL_NKPortOfLoading;

		TrackingConstants.BusinessContext IWorkflowInformationProvider.BusinessContext => TrackingConstants.BusinessContext.NoBusinessContext;

		IEnumerable<ZGuid> IWorkflowInformationProvider.Companies => oceanBill != null ? new ZGuid[] { oceanBill.Branch.Company.PK } : Array.Empty<ZGuid>();

		readonly BaseCusSCAOceanBill oceanBill;
	}
}
