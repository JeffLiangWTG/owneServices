using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class ApportionedCharge : BaseApportionedCharge, Integration.Customs.ZA.IApportionedCharge
	{
		public ApportionedCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
