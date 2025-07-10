using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitConsignmentPackage : EU.ExitControl.Business.CusExitConsignmentPackage,
	Integration.Customs.PLExitControl.ICusExitConsignmentPackage
{
	public CusExitConsignmentPackage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ICusExitReportItemCollection<ExitControlBase.Business.CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

	protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
}
