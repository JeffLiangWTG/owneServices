using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	sealed class BookingPackingLineBuilder
	{
		public BookingPackingLine Build(PackLine packLineBO, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres, string groupKey = "")
		{
			if (packLineBO == null)
			{
				return null;
			}

			var context = new CommonContext(packLineBO.Factory);
			var packingLine = new BookingPackingLine(packLineBO.PK);
			packingLine.GroupKeyByConsignment = groupKey;
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

		internal BookingPackingLine Build(WhsItemPackageState packageState, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres)
		{
			if (packageState == null)
			{
				return null;
			}

			var context = new CommonContext(packageState.Factory);
			var packingLine = new BookingPackingLine(packageState.PK);

			PopulateGeneralInfo(packingLine, packageState, context);
			PopulateGoodsDetails(packingLine, packageState);
			PopulateWeightAndMeasures(packingLine, packageState, unitOfWeight, unitOfVolume, context);

			return packingLine;
		}

		#region Implementation

		void PopulateGeneralInfo(BookingPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
		{
			var container = packLineBO.JL_JC.IsValid
				? packLineBO.Factory.Load<CommonContainer>(packLineBO.JL_JC)
				: null;

			packingLine.ContainerNumber = container != null
				? container.JC_ContainerNum
				: ZString.Empty;

			packingLine.PackingLineID = packLineBO.JL_PackLineId;
			packingLine.ShipmentID = packLineBO.Shipment?.JS_UniqueConsignRef ?? ZString.Empty;
			packingLine.ImportReferenceNumber = packLineBO.JL_ImportRefNumber.IsEmpty
				? shipmentBO.Numbers.GetFirstReferenceNumberByTypeAndCountry(FranceAdditionalReferenceNumberTypes.Codes.ImportConventional, Core.Constants.CountryCodes.France)?.CE_EntryNum ?? ZString.Empty
				: packLineBO.JL_ImportRefNumber;
			packingLine.ExportReferenceNumber = packLineBO.JL_ExportRefNumber;

			packingLine.ItemNumber = packLineBO.JL_ItemNo;
			packingLine.EndItemNumber = packLineBO.JL_EndItemNo;
			packingLine.LoadingMeters = packLineBO.JL_LoadingMeters;
			packingLine.PackingOrder = packLineBO.JL_ContainerPackingOrder;

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

			packingLine.LastKnownTransitWarehouseStatus = packLineBO.JL_LastKnownTransitWarehouseStatus;
		}

		void PopulateGeneralInfo(BookingPackingLine packingLine, WhsItemPackageState packageState, CommonContext context)
		{
			packingLine.ContainerNumber = packageState.ReceiveTransportationUnit?.ContainerNumber ?? ZString.Empty;
		}

		void PopulateGoodsDetails(BookingPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
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

		void PopulateGoodsDetails(BookingPackingLine packingLine, WhsItemPackageState packageState)
		{
			var package = packageState.Package;
			if (package != null)
			{
				packingLine.Commodity = new CodeDescription(package.Lookups.CommodityCodes)
				{
					Code = package.KP_RH_NKCommodityCode
				};

				packingLine.GoodsDescription = package.KP_GoodsDescription.Trim();
				packingLine.PackingLineID = package.KP_PackageID.IsEmpty ? package.KP_ExternalReference : package.KP_PackageID;
			}
		}

		void PopulateWeightAndMeasures(BookingPackingLine packingLine, PackLine packLineBO, CommonContext context, string unitOfWeight, string unitOfVolume)
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

		void PopulateWeightAndMeasures(BookingPackingLine packingLine, WhsItemPackageState packageState, string unitOfWeight, string unitOfVolume, CommonContext context)
		{
			var package = packageState.Package;
			if (package != null)
			{
				packingLine.Quantity = package.KP_PackageQty;
				packingLine.PackageType = new CodeDescription(package.Lookups.PackTypes)
				{
					Code = package.KP_F3_NKPackType
				};

				var unitOfDimensions = Constants.Length.Centimetres;

				packingLine.Weight = new Measurement
				{
					Value = Constants.Weight.Convert(package.KP_Weight, package.KP_WeightUQ, unitOfWeight),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = unitOfWeight
					}
				};

				packingLine.Volume = new Measurement
				{
					Value = Constants.Volume.Convert(package.KP_Volume, package.KP_VolumeUQ, unitOfVolume),
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = unitOfVolume
					}
				};

				packingLine.Height = new Measurement
				{
					Value = Constants.Length.Convert(package.KP_Height, package.KP_DimensionUQ, unitOfDimensions),
					Unit = new CodeDescription(context.DimensionUnits)
					{
						Code = unitOfDimensions
					}
				};

				packingLine.Length = new Measurement
				{
					Value = Constants.Length.Convert(package.KP_Length, package.KP_DimensionUQ, unitOfDimensions),
					Unit = new CodeDescription(context.DimensionUnits)
					{
						Code = unitOfDimensions
					}
				};

				packingLine.Width = new Measurement
				{
					Value = Constants.Length.Convert(package.KP_Width, package.KP_DimensionUQ, unitOfDimensions),
					Unit = new CodeDescription(context.DimensionUnits)
					{
						Code = unitOfDimensions
					}
				};

				packingLine.MarksAndNumbers = package.KP_MarksAndNumbers.Trim();
			}
		}

		void PopulateOutturnDetails(BookingPackingLine packingLine, PackLine packLineBO, CommonShipment shipmentBO, CommonContext context)
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

		void PopulateRORODetails(BookingPackingLine packingLine, PackLine packLineBO)
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
