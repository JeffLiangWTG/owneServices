using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitContainer : EU.ExitControl.Business.CusExitContainer,
	Integration.Customs.PLExitControl.ICusExitContainer,
	ICusSealTypeSupporter
{
	public CusExitContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override EU.ExitControl.Business.CusExitSealCollection CreateNewCusExitSealCollection() => new CusExitSealCollection(this);

	protected override ICusExitConsignmentPivotCollection<ExitControlBase.Business.CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection()
		=> new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

	protected override Type CusSealTypeCore => typeof(CusExitSeal);

	protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
}
