using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Country = Enterprise.DocumentVisualizer.DocDataObjects.Country;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public class PackingLineBuilder
	{
		public PackingLine Build(AgencyShipmentPackLine packLineBO, string unitOfWeight = Core.Constants.Weight.Kilograms, string unitOfVolume = Core.Constants.Volume.CubicMetres)
		{
			if (packLineBO == null)
			{
				return null;
			}

			var context = new CommonContext(packLineBO.Factory);
			var packingLine = new PackingLine(packLineBO.PK);
			var shipmentBO = packLineBO.Shipment;

			PopulateGeneralInfo(packingLine, packLineBO, shipmentBO, context);
			PopulateGoodsDetails(packingLine, packLineBO, shipmentBO, context);
			PopulateOutturnDetails(packingLine, packLineBO, shipmentBO, context);
			PopulateWeightAndMeasures(packingLine, packLineBO, context, unitOfWeight, unitOfVolume);

			if (shipmentBO.JS_PackingMode == Core.Constants.ContainerModes.RollOnRollOff)
			{
				PopulateRORODetails(packingLine, packLineBO);
			}

			return packingLine;
		}

		internal static HarmonizedCode GetHarmonizedCodesDesc(BusinessObjectFactory factory, IPackingLine packingLine, ZString port, IContext context)
		{
			var country = port.SubstringSafe(0, 2);
			var harmonizedCode = new HarmonizedCode()
			{
				Country = new Country(factory, context.Countries)
				{
					Code = country
				}
			};

			if (!country.IsEmpty)
			{
				var sb = new ZStringBuilder();
				foreach (var hc in packingLine.HarmonizedCodes?.Where(c => c.Country.Code == country))
				{
					sb.Append(hc.Code);
				}

				if (!sb.IsEmpty)
				{
					harmonizedCode.Code = sb.ToStringWithDelimiterBetweenAppends(", ");
				}
			}

			return harmonizedCode;
		}

		public PackingLine Build(AgencyShipmentContainer containerBO, string unitOfWeight = Core.Constants.Weight.Kilograms, string unitOfVolume = Core.Constants.Volume.CubicMetres)
		{
			if (containerBO == null)
			{
				return null;
			}

			var context = new CommonContext(containerBO.Factory);
			var packingLine = new PackingLine(containerBO.PK);

			packingLine.ReferenceNumber = containerBO.JC_ContainerNum;
			packingLine.PackageType = new CodeDescription(containerBO.Lookups.PackTypes)
			{
				Code = containerBO.JC_F3_NKPackType
			};

			packingLine.VehicleColor = containerBO.JC_VehicleColor;
			packingLine.VehicleMake = containerBO.JC_VehicleMake;
			packingLine.VehicleModel = containerBO.JC_VehicleModel;
			packingLine.VehicleNumberOfDoors = containerBO.JC_VehicleNumberOfDoors;
			packingLine.VehicleTransmission = new CodeDescription(containerBO.JC_VehicleTransmission_List)
			{
				Code = containerBO.JC_VehicleTransmission
			};
			packingLine.VehicleYear = containerBO.JC_VehicleYear;

			packingLine.Quantity = containerBO.JC_ContainerCount;
			packingLine.GoodsDescription = containerBO.JC_Description;
			packingLine.MarksAndNumbers = containerBO.JC_MarksAndNumbers;

			packingLine.Weight = new Measurement
			{
				Value = Core.Constants.Weight.Convert(containerBO.JC_GrossWeight, containerBO.JC_GrossWeightUQ, unitOfWeight),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			packingLine.Volume = new Measurement
			{
				Value = Core.Constants.Volume.Convert(containerBO.JC_GrossVolume, containerBO.JC_GrossVolumeUQ, unitOfVolume),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			packingLine.Height = new Measurement
			{
				Value = containerBO.JC_TotalHeight,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = containerBO.JC_TotalUnitOfMeasure
				}
			};

			packingLine.Length = new Measurement
			{
				Value = containerBO.JC_TotalLength,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = containerBO.JC_TotalUnitOfMeasure
				}
			};

			packingLine.Width = new Measurement
			{
				Value = containerBO.JC_TotalWidth,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = containerBO.JC_TotalUnitOfMeasure
				}
			};

			packingLine.Commodity = new CodeDescription(containerBO.ContainerCommodityCode_List)
			{
				Code = containerBO.JC_RH_NKContainerCommodityCode
			};
			packingLine.HarmonizedCode = new HarmonizedCode()
			{
				Code = containerBO.JC_HarmonisedCode,
				Country = new Country(containerBO.Factory, context.Countries)
				{
					Code = ZString.Empty
				}
			};

			var dgBuilder = new DangerousGoodBuilder();
			packingLine.DangerousGoods = containerBO
				.UNDGs
				.Select(undg => dgBuilder.Build(undg, context))
				.ToArray();

			return packingLine;
		}

		#region Implementation

		void PopulateGeneralInfo(PackingLine packingLine, AgencyShipmentPackLine packLineBO, AgencyShipment shipmentBO, CommonContext context)
		{
			var container = packLineBO.Container;

			packingLine.ContainerNumber = container != null
				? container.JC_ContainerNum
				: ZString.Empty;

			packingLine.ShipmentID = packLineBO.Shipment?.JS_UniqueConsignRef ?? ZString.Empty;
			packingLine.ShippersRef = packLineBO.Shipment?.JS_BookingReference ?? ZString.Empty;
			packingLine.ImportReferenceNumber = packLineBO.JL_ImportRefNumber;
			packingLine.ExportReferenceNumber = packLineBO.JL_ExportRefNumber;

			packingLine.ItemNumber = packLineBO.JL_ItemNo;
			packingLine.EndItemNumber = packLineBO.JL_EndItemNo;
			packingLine.LoadingMeters = packLineBO.JL_LoadingMeters;
			packingLine.PackingOrder = packLineBO.JL_ContainerPackingOrder;
			packingLine.PackingLineID = packLineBO.JL_PackLineId;

			var dgBuilder = new DangerousGoodBuilder();
			packingLine.DangerousGoods = packLineBO
				.UNDGs
				.Select(undg => dgBuilder.Build(undg, context))
				.ToArray();

			packingLine.HarmonizedCodes = packLineBO
				.HarmonisedCodes
				.Select(x => new HarmonizedCode
				{
					Code = x.JLH_Code,
					Country = Country.Create(context, x.Country)
				})
				.ToArray();

			shipmentBO?.TransportsIncludingRelated.Sort(MovementLegComparer.PortsAndDatesBased(shipmentBO?.TransportsIncludingRelated));
			var firstSeaTransport = shipmentBO?.TransportsIncludingRelated.OfType<Freight.Business.Transport>().FirstOrDefault();
			if (firstSeaTransport != null)
			{
				packingLine.ExportHarmonizedCode = GetHarmonizedCodesDesc(packLineBO.Factory, packingLine, firstSeaTransport.JW_RL_NKLoadPort, context);
			}

			var lastSeaTransport = shipmentBO?.TransportsIncludingRelated.OfType<Freight.Business.Transport>().LastOrDefault();
			if (lastSeaTransport != null)
			{
				packingLine.ImportHarmonizedCode = GetHarmonizedCodesDesc(packLineBO.Factory, packingLine, lastSeaTransport.JW_RL_NKDiscPort, context);
			}

			var harmonisedCodeCountry = firstSeaTransport != null
										&& lastSeaTransport != null
										&& (firstSeaTransport.JW_RL_NKLoadPort.Substring(0, 2) == Core.Constants.CountryCodes.Brazil
											|| lastSeaTransport.JW_RL_NKDiscPort.Substring(0, 2) == Core.Constants.CountryCodes.Brazil)
					? Core.Constants.CountryCodes.Brazil
					: string.Empty;

			packingLine.HarmonizedCode = new HarmonizedCode()
			{
				Code = packLineBO.JL_HarmonisedCode,
				Country = new Country(packLineBO.Factory, context.Countries)
				{
					Code = harmonisedCodeCountry
				}
			};

			packingLine.TemperatureMinimum = new Measurement
			{
				Value = packLineBO.JL_RequiredTemperatureMinimum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = packLineBO.JL_RequiredTemperatureUnit
				}
			};

			packingLine.TemperatureMaximum = new Measurement
			{
				Value = packLineBO.JL_RequiredTemperatureMaximum,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = packLineBO.JL_RequiredTemperatureUnit
				}
			};
			packingLine.RequiresTemperatureControl = packLineBO.JL_RequiresTemperatureControl;
		}

		void PopulateGoodsDetails(PackingLine packingLine, AgencyShipmentPackLine packLineBO, AgencyShipment shipmentBO, CommonContext context)
		{
			packingLine.Commodity = new CodeDescription(packLineBO.RefCommodity_List)
			{
				Code = packLineBO.JL_RH_NKCommodityCode
			};

			packingLine.ShortGoodsDescription = packLineBO.JL_Description;
			packingLine.DetailedGoodsDescription = packLineBO.JL_DetailedDescription;
			packingLine.GoodsDescription = new[]
				{
					packLineBO.JL_DetailedDescription,
					packLineBO.JL_Description,
					shipmentBO?.DetailedGoodsDescriptionNoteText ?? ZString.Empty,
					shipmentBO?.JS_GoodsDescription ?? ZString.Empty,
				}.FirstOrDefault(desc => !desc.IsEmpty).SubstringSafe(0, 31981);

			packingLine.Origin = Country.Create(context, packLineBO.Origin);
			packingLine.ReferenceNumber = packLineBO.JL_RefNumber;
		}

		void PopulateWeightAndMeasures(PackingLine packingLine, AgencyShipmentPackLine packLineBO, CommonContext context, string unitOfWeight, string unitOfVolume)
		{
			packingLine.Quantity = packLineBO.JL_PackageCount;
			packingLine.PackageType = new CodeDescription(packLineBO.Lookups.PackTypes)
			{
				Code = packLineBO.JL_F3_NKPackType
			};

			var unitOfDimensions = Core.Constants.Length.Metres;

			packingLine.Weight = new Measurement
			{
				Value = Core.Constants.Weight.Convert(packLineBO.JL_ActualWeight, packLineBO.JL_ActualWeightUQ, unitOfWeight),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			packingLine.Volume = new Measurement
			{
				Value = Core.Constants.Volume.Convert(packLineBO.JL_ActualVolume, packLineBO.JL_ActualVolumeUQ, unitOfVolume),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			packingLine.Height = new Measurement
			{
				Value = Core.Constants.Length.Convert(packLineBO.JL_Height, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};

			packingLine.Length = new Measurement
			{
				Value = Core.Constants.Length.Convert(packLineBO.JL_Length, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};

			packingLine.Width = new Measurement
			{
				Value = Core.Constants.Length.Convert(packLineBO.JL_Width, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};
		}

		void PopulateOutturnDetails(PackingLine packingLine, AgencyShipmentPackLine packLineBO, AgencyShipment shipmentBO, CommonContext context)
		{
			packingLine.Outturn = packLineBO.JL_Outturn;
			packingLine.Damaged = packLineBO.JL_Damaged;
			packingLine.Pillaged = packLineBO.JL_Pillaged;
			packingLine.OutturnComment = packLineBO.JL_OutturnComment;
			packingLine.MarksAndNumbers = new[]
			{
				packLineBO.JL_MarksAndNumbers,
				shipmentBO?.JS_MarksAndNumbers ?? ZString.Empty
			}.FirstOrDefault(marks => !marks.IsEmpty).SubstringSafe(0, 31981);

			packingLine.OutturnLength = new Measurement
			{
				Value = packLineBO.JL_OutturnedLength,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = packLineBO.JL_OutturnUD
				}
			};

			packingLine.OutturnHeight = new Measurement
			{
				Value = packLineBO.JL_OutturnedHeight,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = packLineBO.JL_OutturnUD
				}
			};

			packingLine.OutturnWidth = new Measurement
			{
				Value = packLineBO.JL_OutturnedWidth,
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = packLineBO.JL_OutturnUD
				}
			};

			packingLine.OutturnWeight = new Measurement
			{
				Value = packLineBO.JL_OutturnedWeight,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = packLineBO.JL_OutturnWeightUQ
				}
			};

			packingLine.OutturnVolume = new Measurement
			{
				Value = packLineBO.JL_OutturnedVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = packLineBO.JL_OutturnVolumeUQ
				}
			};
		}

		void PopulateRORODetails(PackingLine packingLine, PackLine packLineBO)
		{
			packingLine.VehicleColor = packLineBO.JL_VehicleColor;
			packingLine.VehicleMake = packLineBO.JL_VehicleMake;
			packingLine.VehicleModel = packLineBO.JL_VehicleModel;
			packingLine.VehicleNumberOfDoors = packLineBO.JL_VehicleNumberOfDoors;
			packingLine.VehicleTransmission = new CodeDescription(packLineBO.JL_VehicleTransmission_List)
			{
				Code = packLineBO.JL_VehicleTransmission
			};
			packingLine.VehicleYear = packLineBO.JL_VehicleYear;
			packingLine.VIN = packLineBO.JL_RefNumber;
		}

		#endregion
	}
}
