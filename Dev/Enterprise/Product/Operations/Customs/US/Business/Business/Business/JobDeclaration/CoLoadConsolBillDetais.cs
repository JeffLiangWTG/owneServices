using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	public class CoLoadConsolBillDetais : IBillDetails
	{
		internal CoLoadConsolBillDetais(ForwardingConsol relevantConsol)
		{
			this.relevantConsol = relevantConsol;
		}
		readonly ForwardingConsol relevantConsol;
		public ZPropertyInfo[] GetNumberOfPackesInfos(ForwardingShipment shipment)
		{
			return ((IBillDetails)relevantConsol).GetNumberOfPackesInfos(shipment);
		}

		public ZPropertyInfo[] GetTypeOfPackesInfos(ForwardingShipment shipment)
		{
			return ((IBillDetails)relevantConsol).GetTypeOfPackesInfos(shipment);
		}

		public ZPropertyInfo AMSBillNumberInfo => ((IBillDetails)relevantConsol).AMSBillNumberInfo;

		public ZPropertyInfo BKGBillNumberInfo => ((IBillDetails)relevantConsol).BKGBillNumberInfo;

		public ZPropertyInfo BillNumberInfo => relevantConsol.JK_CoLoadMasterBillInfo;

		public IEnumerable<OrgHeader> SCACIssuers => ((IBillDetails)relevantConsol).SCACIssuers;

		public ZDateTime BillUssueDate => ZDateTime.Empty;
	}
}
