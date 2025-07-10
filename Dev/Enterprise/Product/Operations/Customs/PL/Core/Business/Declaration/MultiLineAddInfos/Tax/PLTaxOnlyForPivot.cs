using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PLTaxOnlyForPivot : CusAddInfo<EU.Business.Declaration.MultiLineAddInfos.Tax_CusAddInfoOnlyForPIVOT>
{
	public PLTaxOnlyForPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new Tax_OnlyForPivot Data => (Tax_OnlyForPivot)base.Data;

	protected override Type AddInfoType => typeof(Tax_OnlyForPivot);
}
