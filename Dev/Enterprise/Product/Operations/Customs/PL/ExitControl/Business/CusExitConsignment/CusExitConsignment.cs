using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CusExitConsignment : EU.ExitControl.Business.CusExitConsignment
		, Integration.Customs.PLExitControl.ICusExitConsignment
{
	public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems => (ICusExitConsignmentItemCollection<CusExitConsignmentItem>)base.CusExitConsignmentItems;

	protected override ICusExitConsignmentItemCollection<ExitControlBase.Business.CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

	protected override bool IsUCC6Core => Header?.IsUCC6 ?? true;
}
