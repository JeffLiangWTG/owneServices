using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsBill : EU.NCTS.Business.NctsBill
{
	public NctsBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ChildEditable(true)]
	public new EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> GoodsItems => (EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc>)base.GoodsItems;

	protected override EU.NCTS.Business.INctsDepartureCargoDescCollection<EU.NCTS.Business.NctsDepartureCargoDesc> GetNewGoodsItems() => new EU.NCTS.Business.NctsDepartureCargoDescCollection<NctsDepartureCargoDesc>(this);
}
