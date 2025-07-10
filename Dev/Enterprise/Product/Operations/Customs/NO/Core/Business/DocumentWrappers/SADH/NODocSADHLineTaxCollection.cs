using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.NO.Business;

public sealed class NODocSADHLineTaxCollection : DocBaseWrapperCollection<NODocSADHLineTax>
{
	public NODocSADHLineTaxCollection(IBusinessObjectCollection feesCollection, BusinessObjectFactory factory) : base(feesCollection, factory)
	{
	}

	public NODocSADHLineTaxCollection(BusinessObjectFactory factory) : base(factory)
	{
	}

	public ZInt TotalChargeAmount => this.Cast<NODocSADHLineTax>().Sum(lineTax => lineTax.ChargeAmount);
}
