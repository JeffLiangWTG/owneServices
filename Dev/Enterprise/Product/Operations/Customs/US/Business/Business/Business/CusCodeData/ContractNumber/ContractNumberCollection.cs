
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public class ContractNumberCollection : CusCodeDataCollection<ContractNumber>
	{
		public ContractNumberCollection(BusinessObject bizObj)
			: base(bizObj, CusCodeDataTypeList.Codes.ContractNumber)
		{
		}
	}
}
