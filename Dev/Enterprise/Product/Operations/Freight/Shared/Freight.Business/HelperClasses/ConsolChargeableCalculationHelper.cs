using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ConsolChargeableCalculationHelper : IConsolChargeableCalculationHelper
	{
		public IReadOnlyDictionary<ZString, ZDecimal> CalculateChargeables(ISet<ZString> uniqueRefs, ZString weightUnit, ZString volumeUnit)
		{
			var result = new Dictionary<ZString, ZDecimal>();
			if (uniqueRefs == null)
			{
				return result;
			}

			const int chunkSize = 500;
			foreach (var chunk in uniqueRefs.Where(uniqueRef => !uniqueRef.IsEmpty).Chunk(chunkSize))
			{
				var chargeables = CalculateChargeablesCore(chunk.ToHashSet(), weightUnit, volumeUnit);
				foreach (var chargeable in chargeables)
				{
					result.Add(chargeable.Key, chargeable.Value);
				}
			}
			return result;
		}

		IReadOnlyDictionary<ZString, ZDecimal> CalculateChargeablesCore(ISet<ZString> uniqueRefs, ZString weightUnit, ZString volumeUnit)
		{
			var result = new Dictionary<ZString, ZDecimal>();
			var sql = $@"
				SELECT {JobConsolSchema.Constants.PK}, {JobConsolSchema.Constants.JK_UniqueConsignRef}
						, {JobConsolSchema.Constants.JK_ConsolChargeable}, {JobConsolSchema.Constants.JK_OverrideConsolChargeable}
						, {JobConsolSchema.Constants.JK_TransportMode}, {JobConsolSchema.Constants.JK_RL_NKLoadPort}
						, {JobConsolSchema.Constants.JK_RL_NKDischargePort}, shp.{JobShipmentSchema.Constants.JS_ActualWeight}
						, shp.{JobShipmentSchema.Constants.JS_UnitOfWeight}, shp.{JobShipmentSchema.Constants.JS_ActualVolume}
						, shp.{JobShipmentSchema.Constants.JS_UnitOfVolume}, shp.{JobShipmentSchema.Constants.JS_LoadingMeters}
				FROM {JobConsolSchema.Constants.SqlSchemaName}.{JobConsolSchema.Constants.TableName} 
				LEFT JOIN (SELECT {JobShipmentSchema.Constants.JS_ActualWeight}, {JobShipmentSchema.Constants.JS_UnitOfWeight}
								, {JobShipmentSchema.Constants.JS_ActualVolume}, {JobShipmentSchema.Constants.JS_UnitOfVolume}
								, {JobShipmentSchema.Constants.JS_LoadingMeters}, {JobConShipLinkSchema.Constants.JN_JK}
							FROM {JobShipmentSchema.Constants.SqlSchemaName}.{JobShipmentSchema.Constants.TableName} 
							JOIN {JobConShipLinkSchema.Constants.SqlSchemaName}.{JobConShipLinkSchema.Constants.TableName} ON {JobShipmentSchema.Constants.PK} = {JobConShipLinkSchema.Constants.JN_JS}
							WHERE {JobShipmentSchema.Constants.JS_IsCancelled} = 0
								AND ({JobShipmentSchema.Constants.JS_JS_ColoadMasterShipment} IS NULL OR {JobShipmentSchema.Constants.JS_IsSplitShipment} = 1)) shp
				ON shp.{JobConShipLinkSchema.Constants.JN_JK}={JobConsolSchema.Constants.PK}
				WHERE {JobConsolSchema.Constants.JK_UniqueConsignRef} IN (SELECT Value FROM @UniqueRefs)";

			var command = Db.Connection.Command(sql); // It is cheaper and quicker to bypass using a BusinessObjectFactory here
			command.AddTableValuedParameter("@UniqueRefs", JobConsolSchema.JK_UniqueConsignRef, uniqueRefs);

			DataView dataView = null;
			using (var reader = command.ExecuteReader())
			{
				var dataTable = new DataTable();
				dataTable.Load(reader);
				dataView = dataTable.DefaultView;
			}

			foreach (var uniqueRef in uniqueRefs)
			{
				result.Add(uniqueRef, new ConsolDummy(uniqueRef, weightUnit, volumeUnit, dataView).GetChargeable());
			}

			return result;
		}

		class ConsolDummy
		{
			public ConsolDummy(ZString consolUniqueRef, ZString weightUnit, ZString volumeUnit, DataView data)
			{
				uniqueRef = consolUniqueRef;
				totalShipmentWeightUnit = weightUnit;
				totalShipmentVolumeUnit = volumeUnit;
				this.data = data;
				data.RowFilter = $"{JobConsolSchema.Constants.JK_UniqueConsignRef} = '{uniqueRef}'";
				if (data.Count > 0)
				{
					PK = (Guid)data[0][JobConsolSchema.Constants.PK];
					Chargeable = (decimal)data[0][JobConsolSchema.Constants.JK_ConsolChargeable];
					OverrideChargeable = (bool)data[0][JobConsolSchema.Constants.JK_OverrideConsolChargeable];
					TransportMode = (string)data[0][JobConsolSchema.Constants.JK_TransportMode];
					LoadPort = (string)data[0][JobConsolSchema.Constants.JK_RL_NKLoadPort];
					DischargePort = (string)data[0][JobConsolSchema.Constants.JK_RL_NKDischargePort];
				}
			}

			readonly DataView data;

			ZGuid PK { get; set; }
			readonly ZString uniqueRef;
			ZDecimal Chargeable { get; set; }
			ZBool OverrideChargeable { get; set; }
			ZString TransportMode { get; set; }
			ZString LoadPort { get; set; }
			ZString DischargePort { get; set; }

			ZDecimal TotalShipmentWeight { get; set; }
			readonly ZString totalShipmentWeightUnit;

			ZDecimal TotalShipmentVolume { get; set; }
			readonly ZString totalShipmentVolumeUnit;

			ZDecimal TotalShipmentLoadingMeters { get; set; }

			ZBool IsDomesticFreight
			{
				get { return ImportExportHelper.IsDomestic(LoadPort, DischargePort); }
			}

			ZString TotalChargeableUnit
			{
				get { return ChargeableAmountCalculator.GetChargeableUnit(TransportMode, totalShipmentWeightUnit, totalShipmentVolumeUnit); }
			}

			public ZDecimal GetChargeable()
			{
				if (OverrideChargeable)
				{
					return Chargeable;
				}
				else
				{
					CalculatesTopLevelShipmentsMeasures();
					CalculateChargeable();

					return Chargeable;
				}
			}

			void CalculatesTopLevelShipmentsMeasures()
			{
				if (PK.IsValid)
				{
					var totalWeight = ZDecimal.Zero;
					var totalVolume = ZDecimal.Zero;
					var totalLoadingMeters = ZDecimal.Zero;
					data.RowFilter = $"{JobConsolSchema.Constants.JK_UniqueConsignRef} = '{uniqueRef}'";
					foreach (DataRowView row in data)
					{
						decimal.TryParse(row[JobShipmentSchema.Constants.JS_ActualWeight].ToString(), out var actualWeight);
						var unitOfWeight = row[JobShipmentSchema.Constants.JS_UnitOfWeight].ToString();

						decimal.TryParse(row[JobShipmentSchema.Constants.JS_ActualVolume].ToString(), out var actualVolume);
						var unitOfVolume = row[JobShipmentSchema.Constants.JS_UnitOfVolume].ToString();

						if (Constants.Weight.ContainsCode(totalShipmentWeightUnit) && Constants.Weight.ContainsCode(unitOfWeight))
						{
							totalWeight += Constants.Weight.Convert(actualWeight, unitOfWeight, totalShipmentWeightUnit.ToString());
						}

						if (Constants.Volume.ContainsCode(totalShipmentVolumeUnit) && Constants.Volume.ContainsCode(unitOfVolume))
						{
							totalVolume += Constants.Volume.Convert(actualVolume, unitOfVolume, totalShipmentVolumeUnit.ToString());
						}
						decimal.TryParse(row[JobShipmentSchema.Constants.JS_LoadingMeters].ToString(), out var loadingMeters);
						totalLoadingMeters += loadingMeters;
					}

					TotalShipmentWeight = totalWeight;
					TotalShipmentVolume = totalVolume;
					TotalShipmentLoadingMeters = totalLoadingMeters;
				}
			}

			void CalculateChargeable()
			{
				Chargeable = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
				{
					Weight = new ZWeight(TotalShipmentWeight, totalShipmentWeightUnit),
					Volume = new ZVolume(TotalShipmentVolume, totalShipmentVolumeUnit),
					LoadingLength = new Quantity(TotalShipmentLoadingMeters, Constants.LoadingLength.LoadingMeters),
					TargetUnit = TotalChargeableUnit,
					ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(IsDomesticFreight, TransportMode, TotalChargeableUnit)
				}).Chargeable.Amount;
			}
		}
	}
}
