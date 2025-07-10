using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DGNPackingLineBuilder
	{
		public DGNPackingLine Build(PackLine packLineBO, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres)
		{
			if (packLineBO == null)
			{
				return null;
			}

			var context = new CommonContext(packLineBO.Factory);
			var packingLine = new DGNPackingLine(packLineBO.PK);
			var shipmentBO = packLineBO.Shipment;

			PopulateGeneralInfo(packingLine, packLineBO, shipmentBO, context);
			PopulateGoodsDetails(packingLine, packLineBO, shipmentBO, context);
			PopulateOutturnDetails(packingLine, packLineBO, shipmentBO, context);
			PopulateWeightAndMeasures(packingLine, packLineBO, context, unitOfWeight, unitOfVolume);

			if (shipmentBO.JS_PackingMode == Constants.ContainerModes.RollOnRollOff)
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

		#region Implementation

		void PopulateGeneralInfo(DGNPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
		{
			var container = packLineBO.JL_JC.IsValid
				? packLineBO.Factory.Load<CommonContainer>(packLineBO.JL_JC)
				: null;

			packingLine.ContainerNumber = container != null
				? container.JC_ContainerNum
				: ZString.Empty;

			packingLine.ShipmentID = packLineBO.Shipment?.JS_UniqueConsignRef ?? ZString.Empty;
			packingLine.ImportReferenceNumber = packLineBO.JL_ImportRefNumber;
			packingLine.ExportReferenceNumber = packLineBO.JL_ExportRefNumber;

			packingLine.ItemNumber = packLineBO.JL_ItemNo;
			packingLine.EndItemNumber = packLineBO.JL_EndItemNo;
			packingLine.LoadingMeters = packLineBO.JL_LoadingMeters;
			packingLine.PackingOrder = packLineBO.JL_ContainerPackingOrder;

			var dgBuilder = new DGNDangerousGoodBuilder();
			packingLine.DangerousGoods = packLineBO
				.UNDGs
				.Select(dgBuilder.Build)
				.ToArray();

			packingLine.HarmonizedCodes = packLineBO
				.HarmonisedCodes
				.Select(x => new HarmonizedCode
				{
					Code = x.JLH_Code,
					Country = Country.Create(context, x.Country)
				})
				.ToArray();

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
										&& (firstSeaTransport.JW_RL_NKLoadPort.Substring(0, 2) == Constants.CountryCodes.Brazil
											|| lastSeaTransport.JW_RL_NKDiscPort.Substring(0, 2) == Constants.CountryCodes.Brazil)
					? Constants.CountryCodes.Brazil
					: string.Empty;

			packingLine.HarmonizedCode = new HarmonizedCode()
			{
				Code = packLineBO.JL_HarmonisedCode,
				Country = new Country(packLineBO.Factory, context.Countries)
				{
					Code = harmonisedCodeCountry
				}
			};
		}

		void PopulateGoodsDetails(DGNPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
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
					shipmentBO?.JS_GoodsDescription ?? ZString.Empty
				}.FirstOrDefault(desc => !desc.IsEmpty).SubstringSafe(0, 31981);

			packingLine.Origin = Country.Create(context, packLineBO.Origin);
			packingLine.ReferenceNumber = packLineBO.JL_RefNumber;
		}

		void PopulateWeightAndMeasures(DGNPackingLine packingLine, PackLine packLineBO, CommonContext context, string unitOfWeight, string unitOfVolume)
		{
			packingLine.Quantity = packLineBO.JL_PackageCount;
			packingLine.PackageType = new CodeDescription(packLineBO.Lookups.PackTypes)
			{
				Code = packLineBO.JL_F3_NKPackType
			};

			var unitOfDimensions = Constants.Length.Centimetres;

			packingLine.Weight = new Measurement
			{
				Value = Constants.Weight.Convert(packLineBO.JL_ActualWeight, packLineBO.JL_ActualWeightUQ, unitOfWeight),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};

			packingLine.Volume = new Measurement
			{
				Value = Constants.Volume.Convert(packLineBO.JL_ActualVolume, packLineBO.JL_ActualVolumeUQ, unitOfVolume),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			packingLine.Height = new Measurement
			{
				Value = Constants.Length.Convert(packLineBO.JL_Height, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};

			packingLine.Length = new Measurement
			{
				Value = Constants.Length.Convert(packLineBO.JL_Length, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};

			packingLine.Width = new Measurement
			{
				Value = Constants.Length.Convert(packLineBO.JL_Width, packLineBO.JL_UnitOfDimension, unitOfDimensions),
				Unit = new CodeDescription(context.DimensionUnits)
				{
					Code = unitOfDimensions
				}
			};
		}

		void PopulateOutturnDetails(DGNPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
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

		void PopulateRORODetails(DGNPackingLine packingLine, PackLine packLineBO)
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
