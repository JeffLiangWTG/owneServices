using System;
using System.Data;
using CargoWise.Data.Utils;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TransitPackageForRatingInfo
	{
		public TransitPackageForRatingInfo(ZGuid pk, ZString unitType, ZInt packageQty, ZString packageType, ZString commodityCode,
			ZDecimal weight, ZString weightUQ, ZDecimal volume, ZString volumeUQ,
			ZBool hasDangerousGoods, ZDateTimeOffset unloadedTime, ZDateTimeOffset unloadCompleteTime, ZDateTimeOffset loadedTime)
		{
			PK = pk;
			UnitType = unitType;
			PackageQty = packageQty;
			PackageType = packageType;
			CommodityCode = commodityCode;
			Weight = weight;
			WeightUQ = weightUQ;
			Volume = volume;
			VolumeUQ = volumeUQ;
			HasDangerousGoods = hasDangerousGoods;
			UnloadedTime = unloadedTime;
			UnloadCompleteTime = unloadCompleteTime;
			LoadedTime = loadedTime;
		}

		public ZGuid PK { get; }

		public ZString UnitType { get; }

		public ZInt PackageQty { get; }

		public ZString PackageType { get; }

		public ZString CommodityCode { get; }

		public ZDecimal Weight { get; }

		public ZString WeightUQ { get; }

		public ZDecimal Volume { get; }

		public ZString VolumeUQ { get; }

		public ZBool HasDangerousGoods { get; }

		public ZDateTimeOffset UnloadedTime { get; }

		public ZDateTimeOffset UnloadCompleteTime { get; }

		public ZDateTimeOffset LoadedTime { get; }

		public static TransitPackageForRatingInfo PopulatePackageForRatingInfo(IDataReader reader)
		{
			var pk = reader.GetGuid(reader.GetOrdinal(WhsItemPackageStateSchema.Constants.PK));
			var unitType = reader.GetString(reader.GetOrdinal(WhsItemPackageStateSchema.Constants.WPS_UnitType));
			var packageQty = reader.GetInt32(reader.GetOrdinal(PkgPackageSchema.Constants.KP_PackageQty));
			var packType = reader.GetString(reader.GetOrdinal(PkgPackageSchema.Constants.KP_F3_NKPackType));
			var weight = reader.GetDecimal(reader.GetOrdinal(PkgPackageSchema.Constants.KP_Weight));
			var weightUQ = reader.GetString(reader.GetOrdinal(PkgPackageSchema.Constants.KP_WeightUQ)).ToUpper();
			var volume = reader.GetDecimal(reader.GetOrdinal(PkgPackageSchema.Constants.KP_Volume));
			var volumeUQ = reader.GetString(reader.GetOrdinal(PkgPackageSchema.Constants.KP_VolumeUQ)).ToUpper();
			var commodityCode = reader.GetString(reader.GetOrdinal(PkgPackageSchema.Constants.KP_RH_NKCommodityCode));
			var hasDG = reader.GetBoolean(reader.GetOrdinal("HasDG"));

			var unloadedTime = ZDateTimeOffset.Empty;
			if (SqlDataReaderExtensions.HasColumn(reader, WhsItemPackageStateSchema.Constants.WPS_UnloadedTime) &&
				reader[WhsItemPackageStateSchema.Constants.WPS_UnloadedTime] != DBNull.Value)
			{
				unloadedTime = (DateTimeOffset)reader[WhsItemPackageStateSchema.Constants.WPS_UnloadedTime];
			}

			var unloadCompleteTime = ZDateTimeOffset.Empty;
			if (SqlDataReaderExtensions.HasColumn(reader, WhsItemReceiveTransportationUnitSchema.Constants.WRH_UnloadCompleteTime) &&
				reader[WhsItemReceiveTransportationUnitSchema.Constants.WRH_UnloadCompleteTime] != DBNull.Value)
			{
				unloadCompleteTime = (DateTimeOffset)reader[WhsItemReceiveTransportationUnitSchema.Constants.WRH_UnloadCompleteTime];
			}

			var loadedTime = ZDateTimeOffset.Empty;
			if (reader[WhsItemPackageStateSchema.Constants.WPS_LoadedTime] != DBNull.Value)
			{
				loadedTime = (DateTimeOffset)reader[WhsItemPackageStateSchema.Constants.WPS_LoadedTime];
			}

			return new TransitPackageForRatingInfo(pk, unitType, packageQty, packType, commodityCode, weight, weightUQ, volume,
				volumeUQ, hasDG, unloadedTime, unloadCompleteTime, loadedTime);
		}
	}
}
