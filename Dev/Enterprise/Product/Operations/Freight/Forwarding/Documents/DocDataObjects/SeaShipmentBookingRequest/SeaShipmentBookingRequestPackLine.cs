using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class SeaShipmentBookingRequestPackLine : DocDataObject
	{
		#region GoodsDescription

		public ZString GoodsDescription
		{
			get => goodsDescription;
			set
			{
				if (SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref goodsDescription, value))
				{
					Validate(GoodsDescriptionInfo);
				}
			}
		}

		ZString goodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(nameof(GoodsDescription));

		#endregion

		#region MarksAndNumbersOnPackages

		public ZString MarksAndNumbersOnPackages
		{
			get => marksAndNumbersOnPackages;
			set
			{
				if (SetNonPersistentPropertyValue(MarksAndNumbersOnPackagesInfo, ref marksAndNumbersOnPackages, value))
				{
					Validate(MarksAndNumbersOnPackagesInfo);
				}
			}
		}

		ZString marksAndNumbersOnPackages;

		public ZPropertyInfo MarksAndNumbersOnPackagesInfo => GetZPropertyInfo(nameof(MarksAndNumbersOnPackages));

		#endregion

		#region Packs

		public ZInt PacksQuantity
		{
			get => packsQuantity;
			set
			{
				if (SetNonPersistentPropertyValue(PacksQuantityInfo, ref packsQuantity, value))
				{
					Validate(PacksQuantityInfo);
				}
			}
		}

		ZInt packsQuantity;

		public ZPropertyInfo PacksQuantityInfo => GetZPropertyInfo(nameof(PacksQuantity));

		public ICodeDescription PackType
		{
			get => packType;
			set => packType = SetChild(packType, value);
		}

		ICodeDescription packType;

		public ZString Packs
		{
			get => packs;
			set
			{
				if (SetNonPersistentPropertyValue(PacksInfo, ref packs, value))
				{
					Validate(PacksInfo);
				}
			}
		}

		ZString packs;

		public ZPropertyInfo PacksInfo => GetZPropertyInfo(nameof(Packs));

		#endregion

		#region CargoWeight

		public Measurement CargoWeight
		{
			get => cargoWeight;
			set => cargoWeight = SetChild(cargoWeight, value);
		}

		Measurement cargoWeight;

		#endregion

		#region Volume

		public Measurement CargoVolume
		{
			get => cargoVolume;
			set => cargoVolume = SetChild(cargoVolume, value);
		}

		Measurement cargoVolume;

		#endregion

		#region DangerousGoods

		public IReadOnlyCollection<DangerousGood> DangerousGoods
		{
			get => dangerousGoods;
			set => dangerousGoods = SetChildCollection(dangerousGoods, value);
		}

		IReadOnlyCollection<DangerousGood> dangerousGoods;

		#endregion

		#region HarmonizedCodes

		public IReadOnlyCollection<HarmonizedCode> HarmonizedCodesCollection
		{
			get => harmonizedCodesCollection;
			set => harmonizedCodesCollection = SetChildCollection(harmonizedCodesCollection, value);
		}

		IReadOnlyCollection<HarmonizedCode> harmonizedCodesCollection;

		public ZString HarmonizedCodes
		{
			get => harmonizedCodes;
			set
			{
				if (SetNonPersistentPropertyValue(HarmonizedCodesInfo, ref harmonizedCodes, value))
				{
					Validate(HarmonizedCodesInfo);
				}
			}
		}

		ZString harmonizedCodes;

		public ZPropertyInfo HarmonizedCodesInfo => GetZPropertyInfo(nameof(HarmonizedCodes));

		#endregion
	}
}
