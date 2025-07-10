using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	/// <summary>
	/// Tested by the consumer (WhsOrderCartageValueObjectAdapterTest).
	/// </summary>
	public class WhsOrderCartageHelper
	{
		public WhsOrderCartageHelper(WhsOrder order)
		{
			Order = order;
		}

		readonly WhsOrder Order;

		#region GoodsDescriptionInSingular

		static string PalletDescription => Res.GetString("873cbcca-ee42-4f46-84c8-dc8aca2d917b", "Pallet");

		static string ParcelDescription => Res.GetString("00a591fd-1fac-4511-90ab-9ca44ae542eb", "Parcel");

		static string BoxDescription => Res.GetString("48b4e9d5-808c-4d90-9245-dba78714fdd4", "Box");

		#endregion

		#region GoodsDescriptionInPlural

		static string PalletsDescription => Res.GetString("075fc85f-630a-47ff-8181-fcd20b1ac624", "Pallets");

		static string ParcelsDescription => Res.GetString("87869474-f873-4220-ac89-9f02e3db072c", "Parcels");

		static string BoxesDescription => Res.GetString("58ea67e5-2502-44e9-847f-6a97a0fcfbc9", "Boxes");

		#endregion

		#region SetOrderLinesPackageCount

		public ErrorNotification SetOrderLinesPackageCount(ZInt packageCount, ZString packageType)
		{
			ErrorNotification result = null;

			if (packageCount > 0 && !packageType.IsEmpty)
			{
				if (packageType.EqualsIgnoringCase(PalletDescription))
				{
					if (packageCount.IsInRange(short.MinValue, short.MaxValue))
					{
						Order.WD_PalletsSent = (ZShort)packageCount;
					}
					else
					{
						result = new ErrorNotification(ErrorType.ImportingDataError,
							Res.GetString("3947c92d-7920-44b9-9e6e-7ddc4e6a3475", "Outer Packs could not be imported, the value exceeds the maximum allowed for Pallets Sent."));
					}
				}
				else if (packageType.EqualsIgnoringCase(BoxDescription))
				{
					Order.WD_UnitsSent = (ZDecimal)packageCount;
				}
				else
				{
					Order.WD_PackagesSent = packageCount;
					Order.WD_F3_NKTotalPackType = packageType.SubstringSafe(0, Order.WD_F3_NKTotalPackTypeInfo.MaxLength);
				}
			}

			return result;
		}

		#endregion

		#region OrderLinesPackageCount

		public ZInt OrderLinesPackageCount
		{
			get
			{
				ZInt result;

				if (Order.WD_PalletsSent > 0)
				{
					result = Order.WD_PalletsSent;
				}
				else if (Order.WD_PackagesSent > 0)
				{
					result = Order.WD_PackagesSent;
				}
				else
				{
					result = Order.WD_UnitsSent.ToZInt();
				}

				return result;
			}
		}

		#endregion

		#region OrderLinesGoodsType

		enum GoodsType
		{
			Code,
			Description
		}

		public ZString OrderLinesGoodsTypeDescription
		{
			get { return GetOrderGoodsType(GoodsType.Description); }
		}

		public ZString OrderLinesGoodsTypeCode
		{
			get { return GetOrderGoodsType(GoodsType.Code); }
		}

		// This is tested in the WhsOrderCartageValueDataAdapterIFSTest class

		ZString GetOrderGoodsType(GoodsType goodsType)
		{
			ZString result;

			if (Order.WD_PalletsSent > 0)
			{
				result = (goodsType == GoodsType.Code) ? Core.Constants.PkgUnit.Pallet : PalletDescription;
			}
			else if (Order.WD_PackagesSent > 0)
			{
				RefPackType packType = Order.Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, Order.WD_F3_NKTotalPackType);

				if (packType == null)
				{
					result = (goodsType == GoodsType.Code) ? Core.Constants.PkgUnit.Package : ParcelDescription;
				}
				else
				{
					result = (goodsType == GoodsType.Code) ? packType.F3_Code : packType.F3_DescriptionMultilingual;
				}
			}
			else
			{
				result = (goodsType == GoodsType.Code) ? Core.Constants.PkgUnit.Box : BoxDescription;
			}

			return result;
		}

		ZString GetOrderGoodsDescription()
		{
			ZString result;

			if (Order.WD_PalletsSent > 0)
			{
				result = Order.WD_PalletsSent == 1 ? PalletDescription : PalletsDescription;
			}
			else if (Order.WD_PackagesSent > 0)
			{
				RefPackType packType = Order.Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, Order.WD_F3_NKTotalPackType);

				if (packType == null)
				{
					result = Order.WD_PackagesSent == 1 ? ParcelDescription : ParcelsDescription;
				}
				else
				{
					result = packType.F3_DescriptionMultilingual;
				}
			}
			else
			{
				result = Order.WD_UnitsSent > 1 ? BoxesDescription : BoxDescription;
			}

			return result;
		}

		#endregion

		#region OrderLinesGoodsDescription

		public ZString OrderLinesGoodsDescription => OrderLinesPackageCount + " " + GetOrderGoodsDescription();

		#endregion

		#region OrderLinesTotalWeightInKG / OrderLinesTotalVolumeInM3

		public ZDecimal OrderLinesTotalWeightInKG
		{
			get { return Core.Constants.Weight.Convert(Order.WD_TotalWeight, Order.WD_TotalWeightUnit, Core.Constants.Weight.Kilograms); }
		}

		public ZDecimal OrderLinesTotalVolumeInM3
		{
			get { return Core.Constants.Volume.Convert(Order.WD_TotalCubic, Order.WD_TotalCubicUnit, Core.Constants.Volume.CubicMetres); }
		}

		#endregion
	}
}
