using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class TaxForPivotCollection : CusAddInfoCollection<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
{
	public TaxForPivotCollection(BusinessObject master) : base(master)
	{
	}

	public new PLTaxOnlyForPivot this[int index] => (PLTaxOnlyForPivot)Elements[index];

	public new PLTaxOnlyForPivot AddNew()
	{
		return (PLTaxOnlyForPivot)base.AddNew();
	}

	protected new PLTaxOnlyForPivot AddNew(Type type)
	{
		return (PLTaxOnlyForPivot)base.AddNew(type);
	}

	protected override BusinessObject AddNewCore()
	{
		return AddNew(typeof(PLTaxOnlyForPivot));
	}
}
