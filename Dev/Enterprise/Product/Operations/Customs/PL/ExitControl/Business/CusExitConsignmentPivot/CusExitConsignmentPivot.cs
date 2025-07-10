using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitConsignmentPivot : EU.ExitControl.Business.CusExitConsignmentPivot,
	Integration.Customs.PLExitControl.ICusExitConsignmentPivot
{
	public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusExitConsignmentItem ConsignmentItem => (CusExitConsignmentItem)base.ConsignmentItem;

	public new CusExitConsignmentPackage Package => (CusExitConsignmentPackage)base.Package;

	public new CusExitContainer Container => (CusExitContainer)base.Container;
}
