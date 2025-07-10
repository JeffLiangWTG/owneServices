using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CusClassPartPivot : EU.Business.MasterFiles.CusClassPartPivot, Integration.Customs.PL.ICusClassPartPivot
{
	public CusClassPartPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	protected override CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT> CreateTaxCollection() => new TaxForPivotCollection(this);

	public new TaxForPivotCollection Taxes => (TaxForPivotCollection)base.Taxes;

	protected override IDictionary<ZString, Type> GetCusAddInfoTypes()
	{
		var result = base.GetCusAddInfoTypes();
		result[CusAddInfoTypeAttribute.Codes.GBTax] = typeof(PLTaxOnlyForPivot);
		return result;
	}
}
