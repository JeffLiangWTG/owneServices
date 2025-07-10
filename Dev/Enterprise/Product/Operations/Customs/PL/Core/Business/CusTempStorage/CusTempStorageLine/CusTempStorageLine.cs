using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.PL.Business.CusTempStorage;

public class CusTempStorageLine : EU.Business.CusTempStorage.CusTempStorageLine
	, Integration.Customs.PL.ICusTempStorageLine
{
	public CusTempStorageLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CusTempStorageLineLookups(this);

	public new CusTempStorageLineLookups Lookups => (CusTempStorageLineLookups)base.Lookups;

	public new CusTempStorageDec Dec => (CusTempStorageDec)base.Dec;

	#region CusTempStorageLineItems

	protected override ICusTempStorageLineItemCollection<EU.Business.CusTempStorage.CusTempStorageLineItem> NewCusTempStorageLineItemCollection()
	{
		return new EU.Business.CusTempStorage.CusTempStorageLineItemCollection<CusTempStorageLineItem>(this);
	}

	public new ICusTempStorageLineItemCollection<CusTempStorageLineItem> CusTempStorageLineItems => (ICusTempStorageLineItemCollection<CusTempStorageLineItem>)base.CusTempStorageLineItems;

	#endregion

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.EmptyList))]
	public override ZString TSL_OwnerReferenceType { get => base.TSL_OwnerReferenceType; set => base.TSL_OwnerReferenceType = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.EmptyList))]
	public override ZString TSL_UnionStatus { get => base.TSL_UnionStatus; set => base.TSL_UnionStatus = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.EmptyList))]
	public override ZString TSL_PackageType { get => base.TSL_PackageType; set => base.TSL_PackageType = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.EmptyList))]
	public override ZString TSL_GoodsType { get => base.TSL_GoodsType; set => base.TSL_GoodsType = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.WeightUQList))]
	public override ZString TSL_GrossWeightUQ { get => base.TSL_GrossWeightUQ; set => base.TSL_GrossWeightUQ = value; }

	[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.EmptyList))]
	public override ZString TSL_LocationOfGoods { get => base.TSL_LocationOfGoods; set => base.TSL_LocationOfGoods = value; }
}
