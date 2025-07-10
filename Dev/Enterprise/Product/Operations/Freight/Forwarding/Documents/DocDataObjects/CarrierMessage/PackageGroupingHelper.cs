using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class PackageGroupingHelper
	{
		public PackageGroupingHelper(ForwardingConsol consol, IContext context)
		{
			this.consol = consol;
			this.context = context;
		}
		protected readonly ForwardingConsol consol;
		protected readonly IContext context;

		public ZString GetTopLevelShipmentPackType(ForwardingShipment shipment)
		{
			if (IsShowInnerPackLines(shipment))
			{
				var hasInnerOuterPackLinePKs = shipment.InnerPackLines.Cast<PackLine>().Select(x => x.JL_JL_OuterPackLine).ToList();

				if (shipment.OuterPackLines.Cast<PackLine>()
					.Any(outerPackLine => !hasInnerOuterPackLinePKs.Contains(outerPackLine.PK)
						&& outerPackLine.JL_F3_NKPackType != shipment.JS_F3_NKTotalCountPackType))
				{
					return Constants.PkgUnit.Package;
				}

				return shipment.JS_F3_NKTotalCountPackType;
			}

			return shipment.JS_F3_NKPackType;
		}

		public void PopulatePackingQuantityAndPackageType(ZString packageGrouping, PackLine packLineBO, PackingLine packingLineDO, ZString topLevelShipmentPackType)
		{
			var shipment = packLineBO.Shipment;
			var innerPackLines = shipment?.InnerPackLines.Where(x => x.JL_JL_OuterPackLine == packLineBO.PK) ?? Array.Empty<PackLine>();
			var isShowInnerPackLines = IsShowInnerPackLines();

			packingLineDO.HasInnerPackLines = innerPackLines.Any();

			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value)
			{
				if (isShowInnerPackLines && innerPackLines.Any())
				{
					packingLineDO.Quantity = innerPackLines.Sum(x => x.JL_PackageCount);
				}

				var packageTypeCode = packingLineDO.PackageType?.Code ?? ZString.Empty;
				if (packageGrouping == Constants.PackageGrouping.Codes.GroupByShipment)
				{
					packageTypeCode = topLevelShipmentPackType;
				}
				else if (isShowInnerPackLines && innerPackLines.Any())
				{
					if (innerPackLines.AllSame(x => x.JL_F3_NKPackType))
					{
						packageTypeCode = innerPackLines.Cast<PackLine>().First().JL_F3_NKPackType;
					}
					else
					{
						packageTypeCode = Constants.PkgUnit.Package;
					}
				}

				if (packingLineDO.PackageType != null)
				{
					packingLineDO.PackageType.Code = packageTypeCode;
				}
				else
				{
					packingLineDO.PackageType = new CodeDescription(packLineBO.Lookups.PackTypes)
					{
						Code = packageTypeCode
					};
				}
			}
		}

		public void PopulateConsolidatedPackingLineDangerousGoodsForDoNotGroup(IReadOnlyCollection<Shipment> shipments, ZString packageGrouping, ZString documentName, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value && packageGrouping == Constants.PackageGrouping.Codes.DoNotGroup)
			{
				shipments.ForEach(shipment => shipment.AllPackingLinesIncludeCoLoad?.ForEach(packingLine => PopulateConsolidatedPackingLineDangerousGoods(packingLine, new List<PackingLine> { packingLine }, unitOfWeight, unitOfVolume, documentName)));
			}
		}

		public void PopulateGroupedAndConsolidatedPackingLines(IReadOnlyCollection<Shipment> shipments, ZString packageGrouping, ZBool mustInContainer, ZString documentName, string unitOfWeight = Constants.Weight.Kilograms, string unitOfVolume = Constants.Volume.CubicMetres)
		{
			if (FreightDataRegistry.Instance.EnablePackageGrouping.Value
				&& (packageGrouping == Constants.PackageGrouping.Codes.GroupByShipment || packageGrouping == Constants.PackageGrouping.Codes.GroupByPackLine))
			{
				foreach (var shipment in shipments)
				{
					GroupAndConsolidatePackingLines(shipment, packageGrouping, mustInContainer, unitOfWeight, unitOfVolume, documentName);
				}
			}
		}

		void GroupAndConsolidatePackingLines(Shipment shipmentDO, ZString packageGrouping, ZBool mustInContainer, ZString unitOfWeight, ZString unitOfVolume, ZString documentName)
		{
			var shipmentBO = consol.Factory.Load<ForwardingShipment>((ZGuid)shipmentDO.Identifier);

			var originalPackingLines = mustInContainer
				? shipmentDO.AllPackingLinesIncludeCoLoad.Where(packingLine => ContainersPackingLinePKs.Contains((ZGuid)packingLine.Identifier)).ToList()
				: shipmentDO.AllPackingLinesIncludeCoLoad.ToList();

			if (!HasUnAllocatedPackLines && shipmentDO.AllPackingLinesIncludeCoLoad.Any(packingLine => !ContainersPackingLinePKs.Contains((ZGuid)packingLine.Identifier)))
			{
				HasUnAllocatedPackLines = true;
			}

			if (!HasPackLinesWithEmptyContainerNumberAndInvalidQuantity && shipmentDO.AllPackingLinesIncludeCoLoad.Any(packingLine => packingLine.ContainerNumber.IsEmpty && packingLine.Quantity <= 0))
			{
				HasPackLinesWithEmptyContainerNumberAndInvalidQuantity = true;
			}

			if (shipmentBO != null && originalPackingLines.Any())
			{
				var groupedPackingLines = new List<PackingLine>();

				if (packageGrouping == Constants.PackageGrouping.Codes.GroupByShipment)
				{
					var processedGroupedPackingLine = BuildGroupedPackingLine(shipmentBO.PK.ToString() + "_GroupedPackingLine", originalPackingLines, unitOfWeight, unitOfVolume, (realPackingLines, groupedPackingLine) =>
					{
						groupedPackingLine.AnyPackCountIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Quantity == 0);
						groupedPackingLine.ShipmentIDWithAnyPackCountIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Quantity == 0).Select(x => x.ShipmentID).Distinct().ToArray());

						groupedPackingLine.AnyPackWeightIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Weight.Value == 0);
						groupedPackingLine.ShipmentIDWithAnyPackWeightIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Weight.Value == 0).Select(x => x.ShipmentID).Distinct().ToArray());

						groupedPackingLine.AnyPackVolumeIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Volume.Value == 0);
						groupedPackingLine.ShipmentIDWithAnyPackVolumeIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Volume.Value == 0).Select(x => x.ShipmentID).Distinct().ToArray());

						groupedPackingLine.PackageType = realPackingLines.FirstOrDefault()?.PackageType;

						groupedPackingLine.GoodsDescription = shipmentBO.DetailedGoodsDescriptionNoteText.IsEmpty
							? shipmentBO.JS_GoodsDescription
							: shipmentBO.DetailedGoodsDescriptionNoteText.SubstringSafe(0, 31981);

						groupedPackingLine.MarksAndNumbers = shipmentBO.JS_MarksAndNumbers.SubstringSafe(0, 31981);

						if (documentName == DataContext.ShippingInstruction)
						{
							PopulateGroupedPackingLineForShippingInstruction(realPackingLines, groupedPackingLine, shipmentDO);
						}
					}, documentName);

					groupedPackingLines.Add(processedGroupedPackingLine);
				}
				else
				{
					Action<List<PackingLine>, PackingLine> populateGroupedPackingLineDetailsForPKL = (realPackingLines, groupedPackingLine) =>
					{
						if (realPackingLines.Any())
						{
							groupedPackingLine.AnyPackCountIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Quantity == 0);
							groupedPackingLine.ShipmentIDWithAnyPackCountIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Quantity == 0).Select(x => x.ShipmentID).Distinct().ToArray());

							groupedPackingLine.AnyPackWeightIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Weight.Value == 0);
							groupedPackingLine.ShipmentIDWithAnyPackWeightIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Weight.Value == 0).Select(x => x.ShipmentID).Distinct().ToArray());

							groupedPackingLine.AnyPackVolumeIsZeroInGroupedSubPackLines = realPackingLines.Any(r => r.Volume.Value == 0);
							groupedPackingLine.ShipmentIDWithAnyPackVolumeIsZeroInGroupedSubPackLines = ZString.Join("\r\n", realPackingLines.Where(r => r.Volume.Value == 0).Select(x => x.ShipmentID).Distinct().ToArray());

							var firstRealPackingLine = realPackingLines.First();
							groupedPackingLine.PackageType = firstRealPackingLine.PackageType;
							groupedPackingLine.GoodsDescription = firstRealPackingLine.GoodsDescription;
							groupedPackingLine.MarksAndNumbers = firstRealPackingLine.MarksAndNumbers;

							if (documentName == DataContext.ShippingInstruction)
							{
								PopulateGroupedPackingLineForShippingInstruction(realPackingLines, groupedPackingLine, shipmentDO);
							}
						}
					};

					originalPackingLines.GroupBy(x => new { GoodsDescription = x.GoodsDescription.Trim(), MarksAndNumbers = x.MarksAndNumbers.Trim(), PackageTypeCode = x.PackageType?.Code ?? ZString.Empty }).ForEach(packingLineGroup =>
					{
						groupedPackingLines.Add(BuildGroupedPackingLine(shipmentBO.PK.ToString() + "_" + packingLineGroup.Key.ToString(), packingLineGroup.ToList(), unitOfWeight, unitOfVolume, populateGroupedPackingLineDetailsForPKL, documentName));
					});
				}

				if (groupedPackingLines.Any() && !shipmentDO.PackingLines.IsNullOrEmpty())
				{
					var defaultPaymentType = shipmentDO.PackingLines.First().HBLPaymentType;

					foreach (var packingLine in groupedPackingLines)
					{
						packingLine.HBLPaymentType = defaultPaymentType;
					}
				}

				shipmentDO.PackingLines = groupedPackingLines;
			}
			else
			{
				shipmentDO.PackingLines = new List<PackingLine>();
			}
		}

		void PopulateGroupedPackingLineForShippingInstruction(List<PackingLine> realPackingLines, PackingLine groupedPackingLine, Shipment shipmentDO)
		{
			var packingLineShipments = realPackingLines.Select(x => CarrierMessageDataExtensions.GetMatchedShipmentForPackLine(shipmentDO, x.ShipmentID)).GroupBy(x => x.ShipmentID).Select(x => x.First());

			groupedPackingLine.GroupITNNumber = string.Join(", ", packingLineShipments.Select(x => x.ITNNumber));
			groupedPackingLine.GroupPOFNumber = string.Join(", ", packingLineShipments.Select(x => x.ExportStatement));
			groupedPackingLine.GroupPOFCode = string.Join(", ", packingLineShipments.Select(x => x.ExportStatementCode));

			if (!groupedPackingLine.GroupPOFNumber.IsEmpty)
			{
				groupedPackingLine.GroupExportStatementField1Type = string.Join(", ", packingLineShipments.Select(x => x.ExportStatementField1Type));
				groupedPackingLine.GroupExportStatementField2Type = string.Join(", ", packingLineShipments.Select(x => x.ExportStatementField2Type));

				var shipmentsWithValidField1Code = packingLineShipments.Where(x => !x.ExportStatementField1Code.IsEmpty);
				groupedPackingLine.GroupExportStatementField1Code = shipmentsWithValidField1Code.Count() == 1
					? shipmentsWithValidField1Code.First().ExportStatementField1Code
					: ZString.Empty;

				var shipmentsWithValidField2Code = packingLineShipments.Where(x => !x.ExportStatementField2Code.IsEmpty);
				groupedPackingLine.GroupExportStatementField2Code = shipmentsWithValidField2Code.Count() == 1
					? shipmentsWithValidField2Code.First().ExportStatementField2Code
					: string.Empty;
			}
			else
			{
				groupedPackingLine.GroupExportStatementField1Type = ZString.Empty;
				groupedPackingLine.GroupExportStatementField1Code = ZString.Empty;
				groupedPackingLine.GroupExportStatementField2Type = ZString.Empty;
				groupedPackingLine.GroupExportStatementField1Type = ZString.Empty;
			}

			groupedPackingLine.GroupDUENumber = string.Join(", ", packingLineShipments.Select(x => x.DUENumber));
			groupedPackingLine.GroupUCRNumber = string.Join(", ", packingLineShipments.Select(x => x.UCRNumber));
			groupedPackingLine.GroupCTKNumber = string.Join(", ", packingLineShipments.Select(x => x.CTKNumber));
			groupedPackingLine.GroupCTNNumber = string.Join(", ", packingLineShipments.Select(x => x.CTNNumber));
		}

		PackingLine BuildGroupedPackingLine(string groupedPackingLineIdentifier, List<PackingLine> packingLines, ZString unitOfWeight, ZString unitOfVolume, Action<List<PackingLine>, PackingLine> populateGroupedPackingLineDetailsDependOnPackageGrouping, ZString documentName)
		{
			var groupedPackingLine = new PackingLine(groupedPackingLineIdentifier, consol.Factory);

			PopulateGroupedOrConsolidatedPackingLineDetails(groupedPackingLine, packingLines, unitOfWeight, unitOfVolume);
			populateGroupedPackingLineDetailsDependOnPackageGrouping(packingLines, groupedPackingLine);
			PopulateCalculatedPackingLineTemperature(groupedPackingLine, packingLines);

			var packingLinesGroupByContainer = new List<PackingLine>();
			var packingLineGroups = packingLines.GroupBy(packingLine => consol.Containers.Cast<ForwardingContainer>().FirstOrDefault(container => container.PackLines.GetPKs().Contains((ZGuid)packingLine.Identifier))?.PK ?? ZGuid.Empty);
			foreach (var packingLineGroup in packingLineGroups)
			{
				var container = consol.Containers.Cast<ForwardingContainer>().FirstOrDefault(c => c.PK == packingLineGroup.Key);

				var consolidatedPackingLine = ConsolidatePackingLines(packingLineGroup.Key.ToString() + "_" + groupedPackingLineIdentifier, packingLineGroup.ToList(), container, groupedPackingLine.PackageType, unitOfWeight, unitOfVolume, documentName);
				if (consolidatedPackingLine != null)
				{
					packingLinesGroupByContainer.Add(consolidatedPackingLine);
				}
			}

			AddNoInnerPackLineValidation(groupedPackingLine, packingLines);

			packingLinesGroupByContainer.Sort((x, y) => x.ContainerNumber.CompareTo(y.ContainerNumber));
			groupedPackingLine.PackingLines = packingLinesGroupByContainer;

			return groupedPackingLine;
		}

		void PopulateGroupedOrConsolidatedPackingLineDetails(PackingLine packingLineDO, List<PackingLine> packingLines, ZString unitOfWeight, ZString unitOfVolume)
		{
			packingLineDO.Quantity = packingLines.Sum(p => p.Quantity);
			packingLineDO.Weight = new Measurement
			{
				Value = packingLines.Where(p => p.Weight?.Unit != null).Sum(p => Constants.Weight.ConvertSafe(p.Weight.Value, p.Weight.Unit.Code, unitOfWeight)),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = unitOfWeight
				}
			};
			packingLineDO.Volume = new Measurement
			{
				Value = packingLines.Where(p => p.Volume?.Unit != null).Sum(p => Constants.Volume.ConvertSafe(p.Volume.Value, p.Volume.Unit.Code, unitOfVolume)),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = unitOfVolume
				}
			};

			packingLineDO.HarmonizedCode = ConsolidateHarmonizedCodes(packingLines.Select(x => x.HarmonizedCode), true);
			packingLineDO.ExportHarmonizedCode = ConsolidateHarmonizedCodes(packingLines.Select(x => x.ExportHarmonizedCode));
			packingLineDO.ImportHarmonizedCode = ConsolidateHarmonizedCodes(packingLines.Select(x => x.ImportHarmonizedCode));
			var selectedOnes = packingLines.Where(packingLine => packingLine.HarmonizedCodes != null).SelectMany(packingLine => packingLine.HarmonizedCodes);
#if NETFRAMEWORK
			packingLineDO.HarmonizedCodes = selectedOnes.DistinctBy(harmonizedCode => new { harmonizedCode.Code, CountryCode = harmonizedCode.Country?.Code ?? ZString.Empty }).ToList();
#else
			packingLineDO.HarmonizedCodes = Enumerable.DistinctBy(selectedOnes, harmonizedCode => new { harmonizedCode.Code, CountryCode = harmonizedCode.Country?.Code ?? ZString.Empty }).ToList();
#endif
			packingLineDO.ShipmentEntryNumbers = string.Join(", ", packingLines.SelectMany(x => x.ShipmentEntryNumbers.Split(", ")).Distinct().Where(x => !x.IsEmpty));
			packingLineDO.ShipmentID = string.Join(", ", packingLines.Select(x => x.ShipmentID).Distinct().Where(x => !x.IsEmpty));
			packingLineDO.ReferenceNumber = string.Join(", ", packingLines.Select(x => x.ReferenceNumber).Distinct().Where(x => !x.IsEmpty));
			packingLineDO.ImportReferenceNumber = string.Join(", ", packingLines.Select(x => x.ImportReferenceNumber).Distinct().Where(x => !x.IsEmpty));
			packingLineDO.ExportReferenceNumber = string.Join(", ", packingLines.Select(x => x.ExportReferenceNumber).Distinct().Where(x => !x.IsEmpty));
			packingLineDO.OutturnComment = string.Join(", ", packingLines.Select(x => x.OutturnComment).Distinct().Where(x => !x.IsEmpty));

			if (packingLines.Where(x => x.Commodity != null && !x.Commodity.Code.IsEmpty).AllSame(x => x.Commodity.Code))
			{
				packingLineDO.Commodity = packingLines.FirstOrDefault(x => x.Commodity != null && !x.Commodity.Code.IsEmpty)?.Commodity;
			}
		}

		public void AddNoInnerPackLineValidation(PackingLine packingLineDO, List<PackingLine> packingLines)
		{
			if (IsShowInnerPackLines() && packingLines.Any(x => !x.HasInnerPackLines))
			{
				packingLineDO.QuantityInfo.AddWarning(() => true
					, Res.GetString("DD1703A1-AB0A-4A79-8494-3E7F5C99858C", "There are pack lines with no inner quantity, outer pack quantity has been taken as inners.\r\nPlease verify in Shipment>Packing>Packs on following Shipments:\r\n{0}.", string.Join("\r\n", packingLines.Where(x => !x.HasInnerPackLines).Select(x => x.ShipmentID).Distinct())));
			}
		}

		HarmonizedCode ConsolidateHarmonizedCodes(IEnumerable<IHarmonizedCode> harmonizedCodes, bool isNormalHarmonizedCode = false)
		{
			harmonizedCodes = harmonizedCodes.Where(x => x != null);

			var harmonizedCode = new HarmonizedCode()
			{
				Country = new Country(consol.Factory, context.Countries)
				{
					Code = harmonizedCodes.FirstOrDefault()?.Country?.Code ?? ZString.Empty
				}
			};

			if (isNormalHarmonizedCode)
			{
				harmonizedCode.Code = string.Join(", ", harmonizedCodes.Select(x => x.Code).Where(x => !x.IsEmpty).Distinct());
			}
			else
			{
				harmonizedCode.Code = string.Join(", ", harmonizedCodes.SelectMany(x => x.Code.Split(", ")).Where(x => !x.IsEmpty).Distinct());
			}

			return harmonizedCode;
		}

		PackingLine ConsolidatePackingLines(ZString consolidatedPackingLineIdentifier, List<PackingLine> packingLines, ForwardingContainer container, ICodeDescription packageType, ZString unitOfWeight, ZString unitOfVolume, ZString documentName)
		{
			if (packingLines.Any())
			{
				var consolidatedPackingLine = new PackingLine(consolidatedPackingLineIdentifier, consol.Factory);

				consolidatedPackingLine.ContainerNumber = packingLines.First().ContainerNumber;
				consolidatedPackingLine.PackageType = packageType;

				PopulateGroupedOrConsolidatedPackingLineDetails(consolidatedPackingLine, packingLines, unitOfWeight, unitOfVolume);
				PopulateConsolidatedPackingLineDangerousGoods(consolidatedPackingLine, packingLines, unitOfWeight, unitOfVolume, documentName);
				PopulateCalculatedPackingLineTemperature(consolidatedPackingLine, packingLines);

				if (!AddDimensionsMandatoryErrorForOOG
					&& container != null && (container.RefContainer?.RC_ISOType ?? ZString.Empty).Length > 2
					&& (container.RefContainer.RC_ISOType[2] == ShippingLineMessagingRequirement.ContainerTypes.OpenTop || container.RefContainer.RC_ISOType[2] == ShippingLineMessagingRequirement.ContainerTypes.FlatRack)
					&& packingLines.Any(x => x.Length == null || x.Length.Value <= 0 || x.Width == null || x.Width.Value <= 0 || x.Height == null || x.Height.Value <= 0))
				{
					AddDimensionsMandatoryErrorForOOG = true;
				}

				return consolidatedPackingLine;
			}

			return null;
		}

		void PopulateConsolidatedPackingLineDangerousGoods(PackingLine consolidatedPackingLine, List<PackingLine> packingLines, ZString unitOfWeight, ZString unitOfVolume, ZString documentName)
		{
			var consolidateDangerousGoods = new List<DangerousGood>();
			var dangerousGoodWithShipmentIDGroups = packingLines.Where(packingLine => packingLine.DangerousGoods != null).SelectMany(packingLine => packingLine.DangerousGoods.Select(dangerousGood => (DangerousGood: dangerousGood, ShipmentID: packingLine.ShipmentID)))
				.GroupBy(x => new
				{
					PackageType = x.DangerousGood.PackageType?.Code ?? ZString.Empty,
					PackedInLimitedQuantity = new ZString(x.DangerousGood.PackedInLimitedQuantity.ToString()),
					x.DangerousGood.IMOClass,
					x.DangerousGood.SubLabel1,
					DGCode = x.DangerousGood.Code,
					FlashPoint = new ZString(x.DangerousGood.FlashPoint?.Value.ToString() ?? string.Empty),
					x.DangerousGood.PackingGroup,
					x.DangerousGood.ProperShippingName,
					TechnicalName = x.DangerousGood.TechnicalName.Trim(),
					MarinePollutant = x.DangerousGood.MarinePollutant?.Code ?? ZString.Empty,
					ContactFullName = x.DangerousGood.Contact?.FullName ?? ZString.Empty,
					ContactPhone = x.DangerousGood.Contact?.Phone ?? ZString.Empty
				});
			var dangerousGoodWithShipmentIDGroupsCount = dangerousGoodWithShipmentIDGroups.Count();

			foreach (var dangerousGoodWithShipmentIDGroup in dangerousGoodWithShipmentIDGroups)
			{
				var consolidatedDangerousGood = ConsolidateDangerousGoods(dangerousGoodWithShipmentIDGroup.Select(x => x.DangerousGood).ToList(), unitOfWeight, unitOfVolume);
				if (consolidatedDangerousGood != null)
				{
					var skipPartialZeroWeightValidation = false;
					if (consolidatedDangerousGood.Weight.Value.IsEmpty && dangerousGoodWithShipmentIDGroupsCount == 1)
					{
						consolidatedDangerousGood.Weight.Value = consolidatedPackingLine.Weight.Value;
						skipPartialZeroWeightValidation = true;
					}

					AddConsolidatedDangerousGoodValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, documentName);
					consolidateDangerousGoods.Add(consolidatedDangerousGood);
				}
			}
			consolidatedPackingLine.DangerousGoods = consolidateDangerousGoods;
		}

		void AddConsolidatedDangerousGoodValidation(DangerousGood consolidatedDangerousGood, IEnumerable<(DangerousGood DangerousGood, ZString ShipmentID)> dangerousGoodWithShipmentIDGroup, ZBool skipPartialZeroWeightValidation, ZString documentName)
		{
			var dgwMessagingRequirement = GetMessagingRequirement(ShippingLineMessagingRequirement.Types.DGNetWeightMandatory);

			switch (documentName)
			{
				case DataContext.BookingRequest:
					{
						AddConsolidateDangerousGoodWeightValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, dgwMessagingRequirement?.RSR_IsBookingRequest ?? false);
						break;
					}
				case DataContext.ShippingInstruction:
					{
						if (dgwMessagingRequirement?.RSR_IsShippingInstruction ?? false)
						{
							AddConsolidateDangerousGoodWeightValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, true);
						}

						break;
					}
				case DataContext.ShippingOrder:
					{
						AddConsolidateDangerousGoodWeightValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, dgwMessagingRequirement?.RSR_IsShippingOrder ?? false);
						break;
					}
				case DataContext.VerifiedGrossMass:
					{
						AddConsolidateDangerousGoodWeightValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, dgwMessagingRequirement?.RSR_IsVerifiedGrossContainerWeight ?? false);
						break;
					}
				case DataContext.EManifest:
					{
						AddConsolidateDangerousGoodWeightValidation(consolidatedDangerousGood, dangerousGoodWithShipmentIDGroup, skipPartialZeroWeightValidation, dgwMessagingRequirement?.RSR_IsEManifest ?? false);
						break;
					}
			}
		}

		void AddConsolidateDangerousGoodWeightValidation(DangerousGood consolidatedDangerousGood, IEnumerable<(DangerousGood DangerousGood, ZString ShipmentID)> dangerousGoodWithShipmentIDGroup, ZBool skipPartialZeroWeightValidation, bool isError)
		{
			var shipmentIDsWithEmptyWeightDangerousGoods = dangerousGoodWithShipmentIDGroup.Where(x => x.DangerousGood.Weight.IsNull || x.DangerousGood.Weight.Value.IsEmpty).Select(x => x.ShipmentID).Distinct();
			var message = Res.GetString("5B777A91-563B-484B-830D-CB0D011E0487", "There are DG records with zero (0) weight.\r\nPlease verify in Shipment>Packing>Pack Line>Dangerous Goods> Weight & Unit on following Shipments:\r\n{0}.", string.Join("\r\n", shipmentIDsWithEmptyWeightDangerousGoods));

			if (isError)
			{
				consolidatedDangerousGood.Weight.ValueInfo.AddMessageErrorIfEmpty(message);
			}
			else
			{
				consolidatedDangerousGood.Weight.ValueInfo.AddWarningIfEmpty(message);
			}

			if (!skipPartialZeroWeightValidation)
			{
				if (isError)
				{
					consolidatedDangerousGood.Weight.ValueInfo.AddMessageError(() => !consolidatedDangerousGood.Weight.Value.IsEmpty && shipmentIDsWithEmptyWeightDangerousGoods.Any(), message);
				}
				else
				{
					consolidatedDangerousGood.Weight.ValueInfo.AddWarning(() => !consolidatedDangerousGood.Weight.Value.IsEmpty && shipmentIDsWithEmptyWeightDangerousGoods.Any(), message);
				}
			}
		}

		DangerousGood ConsolidateDangerousGoods(List<DangerousGood> dangerousGoods, ZString unitOfWeight, ZString unitOfVolume)
		{
			if (dangerousGoods.Any())
			{
				var consolidatedDangerousGood = new DangerousGood();
				var firstDangerousGood = dangerousGoods.First();
				consolidatedDangerousGood.Weight = new Measurement
				{
					Value = dangerousGoods.Where(x => x.Weight?.Unit != null).Sum(x => Constants.Weight.ConvertSafe(x.Weight.Value, x.Weight.Unit.Code, unitOfWeight)),
					Unit = new CodeDescription(context.WeightUnits)
					{
						Code = unitOfWeight
					}
				};
				consolidatedDangerousGood.Volume = new Measurement
				{
					Value = dangerousGoods.Where(x => x.Volume?.Unit != null).Sum(x => Constants.Volume.ConvertSafe(x.Volume.Value, x.Volume.Unit.Code, unitOfVolume)),
					Unit = new CodeDescription(context.VolumeUnits)
					{
						Code = unitOfVolume
					}
				};
				consolidatedDangerousGood.Quantity = dangerousGoods.Sum(x => x.Quantity);
				consolidatedDangerousGood.PackageType = firstDangerousGood.PackageType;
				consolidatedDangerousGood.PackedInLimitedQuantity = firstDangerousGood.PackedInLimitedQuantity;
				consolidatedDangerousGood.IMOClass = firstDangerousGood.IMOClass;
				consolidatedDangerousGood.SubLabel1 = firstDangerousGood.SubLabel1;
				consolidatedDangerousGood.Unno = firstDangerousGood.Unno;
				consolidatedDangerousGood.Variant = firstDangerousGood.Variant;
				consolidatedDangerousGood.Code = firstDangerousGood.Code;
				consolidatedDangerousGood.FlashPoint = firstDangerousGood.FlashPoint;
				consolidatedDangerousGood.PackingGroup = firstDangerousGood.PackingGroup;
				consolidatedDangerousGood.ProperShippingName = firstDangerousGood.ProperShippingName;
				consolidatedDangerousGood.TechnicalName = firstDangerousGood.TechnicalName;
				consolidatedDangerousGood.MarinePollutant = firstDangerousGood.MarinePollutant;
				consolidatedDangerousGood.Contact = firstDangerousGood.Contact;

				consolidatedDangerousGood.SubLabel2 = firstDangerousGood.SubLabel2;
				consolidatedDangerousGood.Standard = firstDangerousGood.Standard;
				if (dangerousGoods.Where(x => !x.State.IsEmpty).AllSame(x => x.State))
				{
					consolidatedDangerousGood.State = dangerousGoods.FirstOrDefault(x => !x.State.IsEmpty)?.State ?? ZString.Empty;
				}
				if (dangerousGoods.Where(x => x.ExceptedQuantityCode != null && !x.ExceptedQuantityCode.Code.IsEmpty).AllSame(x => x.ExceptedQuantityCode.Code))
				{
					consolidatedDangerousGood.ExceptedQuantityCode = dangerousGoods.FirstOrDefault(x => x.ExceptedQuantityCode != null && !x.ExceptedQuantityCode.Code.IsEmpty)?.ExceptedQuantityCode;
				}
				if (dangerousGoods.Where(x => x.EmergencyScheduleFire != null && !x.EmergencyScheduleFire.Code.IsEmpty).AllSame(x => x.EmergencyScheduleFire.Code))
				{
					consolidatedDangerousGood.EmergencyScheduleFire = dangerousGoods.FirstOrDefault(x => x.EmergencyScheduleFire != null && !x.EmergencyScheduleFire.Code.IsEmpty)?.EmergencyScheduleFire;
				}
				if (dangerousGoods.Where(x => x.EmergencyScheduleSpillage != null && !x.EmergencyScheduleSpillage.Code.IsEmpty).AllSame(x => x.EmergencyScheduleSpillage.Code))
				{
					consolidatedDangerousGood.EmergencyScheduleSpillage = dangerousGoods.FirstOrDefault(x => x.EmergencyScheduleSpillage != null && !x.EmergencyScheduleSpillage.Code.IsEmpty)?.EmergencyScheduleSpillage;
				}

				return consolidatedDangerousGood;
			}

			return null;
		}

		void PopulateCalculatedPackingLineTemperature(PackingLine groupedPackingLine, List<PackingLine> realPackingLines)
		{
			var requiresTemperatureControlPackingLines = realPackingLines.Where(x => x.RequiresTemperatureControl && x.TemperatureMaximum?.Unit != null && x.TemperatureMinimum?.Unit != null);
			if (requiresTemperatureControlPackingLines.Any())
			{
				groupedPackingLine.RequiresTemperatureControl = true;

				var isUnitOfTemperatureSame = requiresTemperatureControlPackingLines.AllSame(x => x.TemperatureMinimum.Unit.Code);
				groupedPackingLine.TemperatureMinimum = new Measurement
				{
					Value = isUnitOfTemperatureSame
						? requiresTemperatureControlPackingLines.Select(x => x.TemperatureMinimum.Value).Max()
						: requiresTemperatureControlPackingLines.Select(x => new ZDecimal(Constants.Temperature.Convert(x.TemperatureMinimum.Value, x.TemperatureMinimum.Unit.Code, Constants.Temperature.Centigrade))).Max(),
					Unit = new CodeDescription(context.TemperatureUnits)
					{
						Code = isUnitOfTemperatureSame
							? requiresTemperatureControlPackingLines.First().TemperatureMinimum.Unit.Code
							: new ZString(Constants.Temperature.Centigrade)
					}
				};

				groupedPackingLine.TemperatureMaximum = new Measurement
				{
					Value = isUnitOfTemperatureSame
						? requiresTemperatureControlPackingLines.Select(x => x.TemperatureMaximum.Value).Min()
						: requiresTemperatureControlPackingLines.Select(x => new ZDecimal(Constants.Temperature.Convert(x.TemperatureMaximum.Value, x.TemperatureMaximum.Unit.Code, Constants.Temperature.Centigrade))).Min(),
					Unit = new CodeDescription(context.TemperatureUnits)
					{
						Code = isUnitOfTemperatureSame
							? requiresTemperatureControlPackingLines.First().TemperatureMaximum.Unit.Code
							: new ZString(Constants.Temperature.Centigrade)
					}
				};
			}
			else
			{
				groupedPackingLine.RequiresTemperatureControl = false;
			}
		}

		#region IsShowInnerPackLines

		public ZBool IsShowInnerPackLines(CommonShipment shipment)
		{
			return IsShowInnerPackLines()
				&& (shipment?.InnerPackLines.OfType<PackLine>().Any(x => x.JL_JL_OuterPackLine.IsValid) ?? ZBool.False);
		}

		ZBool IsShowInnerPackLines()
		{
			return FreightDataRegistry.Instance.EnablePackageGrouping.Value
				&& IsRuleAllowToShowInnerPackLines;
		}

		ZBool IsRuleAllowToShowInnerPackLines
		{
			get
			{
				if (!isRuleAllowToShowInnerPackLines.HasValue)
				{
					var rules = (consol.DischargePort?.Country?.Rules.ToArray() ?? Array.Empty<RefCountryRules>())
						.Union(consol.LoadPort?.Country?.Rules.ToArray() ?? Array.Empty<RefCountryRules>());

					var dischargeCountry = consol.JK_RL_NKDischargePort.Left(2);
					var loadCountry = consol.JK_RL_NKLoadPort.Left(2);

					ZBool EqualsOrEmpty(ZString value, ZString euqalsToString)
					{
						return value == euqalsToString || value.IsEmpty;
					}

					var rule = rules.OfType<RefCountryRules>()
						.Where(x => EqualsOrEmpty(x.R7_RN_NKDestination, dischargeCountry) && EqualsOrEmpty(x.R7_RN_NKOrigin, loadCountry)
							&& EqualsOrEmpty(x.R7_TransportMode, consol.JK_TransportMode) && EqualsOrEmpty(x.ServiceLevel?.RS_Code ?? ZString.Empty, consol.JK_AWBServiceLevel))
						.OrderByDescending(x => x.R7_RN_NKDestination == dischargeCountry)
						.ThenByDescending(x => x.R7_RN_NKOrigin == loadCountry)
						.ThenByDescending(x => x.R7_TransportMode == consol.JK_TransportMode)
						.ThenByDescending(x => (x.ServiceLevel?.RS_Code ?? ZString.Empty) == consol.JK_AWBServiceLevel)
						.FirstOrDefault();

					isRuleAllowToShowInnerPackLines = rule?.R7_ShowInner ?? ZBool.False;
				}

				return isRuleAllowToShowInnerPackLines.Value;
			}
		}
		ZBool? isRuleAllowToShowInnerPackLines;

		#endregion

		#region ContainersPackingLinePKs

		List<ZGuid> ContainersPackingLinePKs
		{
			get
			{
				if (containersPackingLinePKs == null)
				{
					containersPackingLinePKs = consol.Containers.Cast<ForwardingContainer>().SelectMany(x => x.PackLines.GetPKs()).ToList();
				}

				return containersPackingLinePKs;
			}
		}
		List<ZGuid> containersPackingLinePKs;

		#endregion

		public ZBool HasUnAllocatedPackLines { get; set; } = false;

		public ZBool AddDimensionsMandatoryErrorForOOG { get; set; } = false;

		public ZBool HasPackLinesWithEmptyContainerNumberAndInvalidQuantity { get; set; } = false;

		RefShippingLineMessagingRequirement GetMessagingRequirement(ZString messageRequirementType)
		{
			if (consol == null)
			{
				return null;
			}

			var orgHeader = consol.IsCoLoad ? consol.Creditor : consol.ShippingLine;

			if (orgHeader == null)
			{
				return null;
			}

			return orgHeader.GetShippingLineMessagingRequirement(messageRequirementType);
		}
	}
}
