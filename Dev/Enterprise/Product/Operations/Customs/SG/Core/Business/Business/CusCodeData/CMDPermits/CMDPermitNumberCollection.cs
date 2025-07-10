using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CMDPermitNumberCollection : Customs.Business.CusCodeDataCollection<CMDPermitNumber>
	{
		public CMDPermitNumberCollection(ForwardingShipment shipment)
			: base(shipment, CusCodeDataTypeList.Codes.CMD)
		{
			MaxCountValidationEnable(50);
		}
	}
}
