using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class VNEAdditionalNumberCollection : Customs.Business.CusCodeDataCollection<VNEAdditionalNumber>
	{
		public VNEAdditionalNumberCollection(BusinessObject parent)
			: base(parent, CusCodeDataTypeList.Codes.VNEAdditionalNumber)
		{
		}
	}
}
