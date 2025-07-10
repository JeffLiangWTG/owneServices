using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NO.Business;

public sealed class GoodsRegistrationNumberGeneratorObject(IGoodsRegistrationNumberManager parent, BusinessObjectFactory factory) : NonPersistentBusinessObject(factory)
{
	class Schema
	{
		public const string GoodsRegistrationDate = nameof(GoodsRegistrationDate);
		public const string WarehouseAuthorisationId = nameof(WarehouseAuthorisationId);
	}

	public GoodsRegistrationNumberGeneratorObjectValidation Validation => new (this);

	public GoodsRegistrationNumberGeneratorObjectLookups Lookups
	{
		get
		{
			if (lookups == null || !IsLookupsCachedInBase)
			{
				lookups = new (this);
			}
			return lookups;
		}
	}
	GoodsRegistrationNumberGeneratorObjectLookups lookups;

	[ResourceStringData("17CCFB40-C005-40C2-A322-D8A0DA5D73E8", Caption = "Goods Registration Date", FullDescription = "Goods Registration Date is normally the date of Norwegian border passing for the means of transport.")]
	public ZDate GoodsRegistrationDate
	{
		get => goodsRegistrationDate;
		set
		{
			var oldValue = GoodsRegistrationDate;
			if (oldValue != value)
			{
				SetNonPersistentPropertyValue(GoodsRegistrationDateInfo, ref goodsRegistrationDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGoodsRegistrationDate();
				}
				GoodsRegistrationDateInfo.RefreshBinding(oldValue);
			}
		}
	}
	ZDate goodsRegistrationDate;

	public ZPropertyInfo GoodsRegistrationDateInfo => GetZPropertyInfo(Schema.GoodsRegistrationDate);

	[MaxLength(5)]
	[List(nameof(Lookups) + "." + nameof(GoodsRegistrationNumberGeneratorObjectLookups.AuthorizationsList))]
	[ResourceStringData("BBE61C85-85AB-4886-ADA0-D284DC39DC0E", Caption = "Customs Warehouse Authorization ID", FullDescription = "A 5-digit Authorization is connected to a given Customs Warehouse address. (Code CWP in Authorization module). One company may have several Customs Warehouses (= several CWP IDs).")]
	public ZString WarehouseAuthorisationId
	{
		get => warehouseAuthorisationId;
		set
		{
			var oldValue = WarehouseAuthorisationId;
			if (oldValue != value)
			{
				CheckMaximumLength(WarehouseAuthorisationIdInfo, value);
				SetNonPersistentPropertyValue(WarehouseAuthorisationIdInfo, ref warehouseAuthorisationId, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateWarehouseAuthorisationId();
				}
				WarehouseAuthorisationIdInfo.RefreshBinding(oldValue);
			}
		}
	}
	ZString warehouseAuthorisationId;

	public ZPropertyInfo WarehouseAuthorisationIdInfo => GetZPropertyInfo(Schema.WarehouseAuthorisationId);

	public void SetGoodsRegistrationNumber()
	{
		if (GoodsRegistrationDate.IsEmpty || WarehouseAuthorisationId.IsEmpty)
		{
			return;
		}

		parent.SetNextGoodsRegistrationNumber(GoodsRegistrationDate.ToShortCustomsFormatDateString(yearDigits: 4) +
			WarehouseAuthorisationId.ToString() +
			GoodsNumberStrategy.GetCustomsWarehouseGoodsNumber(GoodsRegistrationDate, WarehouseAuthorisationId));
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		WarehouseAuthorisationId = Lookups.AuthorizationsList.Count == 1 ? Lookups.AuthorizationsList[0].Code : ZString.Empty;
		GoodsRegistrationDate = ZDate.Today;
	}

	readonly IGoodsRegistrationNumberManager parent = Argument.NotNull(parent, nameof(parent));

	NOCustomsWarehouseGoodsNumberStrategy GoodsNumberStrategy => goodsNumberStrategy ??= new(Factory);
	NOCustomsWarehouseGoodsNumberStrategy goodsNumberStrategy;
}
