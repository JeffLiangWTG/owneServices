using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IBillDetails
	{
		ZPropertyInfo AMSBillNumberInfo { get; }
		ZPropertyInfo BKGBillNumberInfo { get; }
		ZPropertyInfo BillNumberInfo { get; }
		IEnumerable<OrgHeader> SCACIssuers { get; }
		ZPropertyInfo[] GetNumberOfPackesInfos(ForwardingShipment shipment);
		ZPropertyInfo[] GetTypeOfPackesInfos(ForwardingShipment shipment);
		ZDateTime BillUssueDate { get; }
	}
}
