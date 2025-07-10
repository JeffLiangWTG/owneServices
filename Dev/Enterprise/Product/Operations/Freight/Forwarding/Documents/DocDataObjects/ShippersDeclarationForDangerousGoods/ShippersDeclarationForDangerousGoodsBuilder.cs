using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.DangerousGoods;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;
using ResString = Enterprise.Freight.Forwarding.Documents.DataObjects.ResString;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ShippersDeclarationForDangerousGoodsBuilder
	{
		public ShippersDeclarationForDangerousGoodsBuilder(ForwardingShipment shipment)
		{
			this.shipment = Argument.NotNull(shipment, nameof(shipment));
			this.context = new CommonContext(this.shipment.Factory.GetCachedReadOnlyFactory());
		}

		readonly ForwardingShipment shipment;
		readonly IContext context;

		public ShippersDeclarationForDangerousGoods Build()
		{
			var header = BuildHeader();
			var natureAndQuantityOfGoodsPaged = CreateNatureAndQuantityOfGoods()
				.Where(nqg => nqg.Any(n => n.LineType == NatureAndQuantityOfDangerousGoodsLineType.Detail))
				.Select((nqg, index) => new ShippersDeclarationForDangerousGoodsDetail(index, header, nqg))
				.ToArray();

			var shippersDeclarationForDangerousGoods = new ShippersDeclarationForDangerousGoods();
			shippersDeclarationForDangerousGoods.Pages = new ShippersDeclarationForDangerousGoodsCollection(natureAndQuantityOfGoodsPaged);

			if (shippersDeclarationForDangerousGoods.Pages.Count == 0)
			{
				shippersDeclarationForDangerousGoods.ErrorMessage = GetErrorMessageForEmptyForm();
			}

			return shippersDeclarationForDangerousGoods;
		}

		#region Header

		ShippersDeclarationForDangerousGoodsHeader BuildHeader()
		{
			var res = new ShippersDeclarationForDangerousGoodsHeader();

			var shipper = AddressBuilder.Create(context, shipment.ConsignorDocumentaryAddress);
			shipper.AddressFormattedInfo.AddErrorIfEmpty(Res.GetString("9138f9c6-5c72-4197-84c0-1ef99794015c", "Shipper address is required"));
			res.Shipper = shipper;

			var consignee = AddressBuilder.Create(context, shipment.ConsigneeDocumentaryAddress);
			consignee.AddressFormattedInfo.AddErrorIfEmpty(Res.GetString("672a9fd1-504a-4294-ae5d-7f4929e7589c", "Consignee address is required"));
			res.Consignee = consignee;

			res.Company = AddressBuilder.Create(context, GlbBranch.CurrentBranch.OrgProxy?.MainAddress);

			res.AirWaybillNumber = shipment.DepartureConsol?.JK_MasterBillNum ?? ZString.Empty;
			res.ShippersReferenceNumber = shipment.JS_HouseBill;

			res.AirportOfDeparture = Unloco.Create(context, shipment.Origin);
			res.AirportOfDestination = Unloco.Create(context, shipment.Destination);

			res.IsCargoOnly = IsCargoOnly;

			res.ShipmentType = new CodeDescription(new DangerousGoodsDeclarationShipmentTypes())
			{
				Code = IsRadioactive
					? DangerousGoodsDeclarationShipmentTypes.Codes.Radioactive
					: DangerousGoodsDeclarationShipmentTypes.Codes.NonRadioactive
			};

			res.Signatory = GlbStaff.CurrentUser.GS_FullName;
			res.SignatoryInfo.AddErrorIfEmpty(Res.GetString("3ef581ee-bf67-44e1-9d09-e5d2cb2bf6bb", "Name is required"));

			res.PlaceOfSignature = GlbBranch.CurrentBranch.GB_City.Left(17);
			res.DateOfSignature = ZDateTime.Now;

			res.EmergencyContact = CreateEmergencyContact();

			var undgs = shipment.OuterPackLines.OfType<PackLine>()
				.SelectMany(packLine => packLine.UNDGs.OfType<ForwardingUNDGDataItem>());
			res.PermittedTransportType = GetPermittedTransportType(undgs);

			res.ValidateAllIncludingChildren();

			return res;
		}

		const string radioactiveIMOClass = "7";

		bool IsRadioactive => shipment
			.OuterPackLines
			.Cast<ForwardingPackLine>()
			.SelectMany(p => p.UNDGs)
			.Any(undg => string.Compare(undg.DI_IMOClass, radioactiveIMOClass, StringComparison.OrdinalIgnoreCase) == 0);

		bool IsCargoOnly => shipment
			.DepartureConsol
			?.MostInterestingTransportForBinding
			.Cast<Freight.Business.Transport>()
			.FirstOrDefault()
			?.JW_IsCargoOnly ?? false;

		bool HasDryIceLessThan30Kgs => shipment
			.OuterPackLines
			.Cast<ForwardingPackLine>()
			.SelectMany(p => p.UNDGs).Any(x => x.Substance != null && x.Substance.DG_UNNO.Contains(UNDGConstants.Codes.DryIceUnno) && Core.Constants.Weight.Convert(x.DI_DGWeight, x.DI_UnitOfWeight, Core.Constants.Weight.Kilograms) <= 30);

		bool HasAnyExcludedDangerousGoodsUNCodes => shipment
			.OuterPackLines
			.Cast<ForwardingPackLine>()
			.SelectMany(p => p.UNDGs).Any(x => IsExcludedDangerousGoodsUNCode(x));

		ZString GetErrorMessageForEmptyForm()
		{
			ZString errorMessage = string.Empty;
			var defaultErrorMessage = Res.GetString("36b3c4bd-8e07-4257-89a0-d2f94164b582", "There must be at least one air freight dangerous goods substance entered to issue this form.");

			var undgs = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(p => p.UNDGs);

			var packType = GetUNDGPackType(undgs);

			var undgsWithExceptedQuantities = undgs.Where(undg => !DoesDangerousGoodExceedExceptedQuantity(undg, packType));

			if (!undgs.Any())
			{
				errorMessage = defaultErrorMessage;
			}
			else if (HasAnyExcludedDangerousGoodsUNCodes || HasDryIceLessThan30Kgs)
			{
				errorMessage = Res.GetString("e93838e3-f9f3-4080-b676-d09a1c89be59", "There are no airfreight dangerous goods substances entered that require this form. Lithium battery substances UN 3090, 3091, 3480, and 3481 (all variants) packed under Section II, as well as UN 1845 (when the only DG substance), 2807, 3164, 3245, and 3373 do not require the Shipper's Declaration for Dangerous Goods.");
			}
			else if (undgsWithExceptedQuantities.Any() && HasAnyUNDGsWithoutWeightOrVolume(undgsWithExceptedQuantities))
			{
				errorMessage = Res.GetString("2cf9e569-b3b9-4843-ba26-5136993ec4ff", "There are no air freight dangerous goods substances entered with weight or volume specified. Weight or volume is required to ascertain if the DG is in excepted quantities. Any DG that is in excepted quantities will not be shown in the form.");
			}
			else if (undgsWithExceptedQuantities.Any())
			{
				errorMessage = Res.GetString("3ba9ea9b-f21e-4afe-9015-ca201d69b784", "There are no air freight dangerous goods substances entered that require this form. Any DG that is in excepted quantities will not be shown in the form.");
			}
			else
			{
				errorMessage = defaultErrorMessage;
			}

			return errorMessage;
		}

		static class PermittedTransportTypeDescription
		{
			public static MultilingualString Forbidden = ResString.GetMultilingualString("6a331adc-3958-40f8-867b-d23f847dad60", "FORBIDDEN");
			public static MultilingualString CargoAircraftOnly = ResString.GetMultilingualString("3a127951-3b27-4a93-b269-d361d37eb3ca", "CARGO AIRCRAFT ONLY");
			public static MultilingualString PassengerCargoAircraft = ResString.GetMultilingualString("481100cf-b442-48f3-b52a-83c239ec2611", "PASSENGER AND CARGO AIRCRAFT");
		}

		ZString GetPermittedTransportType(IEnumerable<ForwardingUNDGDataItem> undgs)
		{
			var result = PermittedTransportTypeDescription.PassengerCargoAircraft;

			if (undgs.Any(undg => DoesDGQuantityExceedCargoLimit(undg)))
			{
				result = PermittedTransportTypeDescription.Forbidden;
			}
			else if (undgs.Any(undg => undg.Substance != null
				&& DoesDGQuantityExceedPassengerAndCargoLimit(undg)))
			{
				result = PermittedTransportTypeDescription.CargoAircraftOnly;
			}

			return result;
		}

		ZString GetPackingInstruction(UNDGDataItem undg)
		{
			var substance = undg.Substance;

			if (undg.DI_IsLimitedQuantity)
			{
				return substance.DG_PackIns;
			}

			if (DoesDGQuantityExceedCargoLimit(undg))
			{
				return ZString.Empty;
			}

			if (DoesDGQuantityExceedPassengerAndCargoLimit(undg))
			{
				return substance.DG_CargoPackIns;
			}

			return substance.DG_PaxPackIns;
		}

		bool DoesDGQuantityExceedCargoLimit(UNDGDataItem undg)
		{
			if (undg.Substance == null)
			{
				return false;
			}

			var exceedCargoLimit = ValidUNDGMaximumQuantityChecker.ShouldCheckVolume(undg.Substance) && ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedCargoLimit(undg)
				|| ValidUNDGMaximumQuantityChecker.ShouldCheckWeight(undg.Substance) && ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedCargoLimit(undg);

			return exceedCargoLimit
				|| DoesDGQuantityExceedPassengerAndCargoLimit(undg) && undg.IsForbiddenForCargoAircraft();
		}

		bool DoesDGQuantityExceedPassengerAndCargoLimit(UNDGDataItem undg)
		{
			if (undg.Substance == null)
			{
				return false;
			}

			var exceedPassengerLimit = ValidUNDGMaximumQuantityChecker.ShouldCheckVolume(undg.Substance) && ValidUNDGMaximumQuantityChecker.DoesDGVolumeExceedPassengerAndCargoLimit(undg)
				|| ValidUNDGMaximumQuantityChecker.ShouldCheckWeight(undg.Substance) && ValidUNDGMaximumQuantityChecker.DoesDGWeightExceedPassengerAndCargoLimit(undg);

			return exceedPassengerLimit || undg.IsForbiddenForPassengerAircraft();
		}

		bool HasAnyUNDGsWithoutWeightOrVolume(IEnumerable<UNDGDataItem> undgsWithExceptedQuantities)
		{
			return undgsWithExceptedQuantities.Any(undg => ValidUNDGExceptedQuantityChecker.GetConvertedDangerousGoodsQuantityValue(undg) == 0);
		}

		IContact CreateEmergencyContact()
		{
			var contact = shipment
				.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(p => p.UNDGs)
				.Select(undg => undg.DGContact)
				.FirstOrDefault(dgContact => dgContact != null);

			return new Contact
			{
				FullName = contact?.OC_ContactName ?? ZString.Empty,
				Phone = contact?.OC_Phone ?? ZString.Empty,
				Email = contact?.OC_Email ?? ZString.Empty,
			};
		}

		#endregion

		#region NatureAndQuantityOfGoods

		NatureAndQuantityOfDangerousGoodsLine[][] CreateNatureAndQuantityOfGoods()
		{
			const int natureAndQuantityLinesPerPage = 10;

			var pager = new NatureAndQuantityOfDangerousGoodsLinePager(natureAndQuantityLinesPerPage);

			var packLines = shipment.OuterPackLines.Cast<ForwardingPackLine>()
				.OrderByDescending(x => x.UNDGs.Any(dg => dg.DI_HasOverpack));

			foreach (var packLine in packLines)
			{
				var undgGroups = GetApplicableDangerousGoods(packLine.UNDGs)
					.GroupBy(GetUNDGGroupKey)
					.OrderBy(g => g.Key.Equals(noOverpackGroup))
					.ThenBy(g => g.Key.Equals(hasOverpackGroup));

				var lines = new List<NatureAndQuantityOfDangerousGoodsLine>();

				foreach (var undgGroup in undgGroups)
				{
					var detailLines = CreateNatureAndQtyOfGoodsLines(undgGroup)
						.OrderBy(l => l.UNCode)
						.ThenBy(l => l.Class).ToArray();

					lines.AddRange(detailLines);

					if (!undgGroup.Key.Equals(noOverpackGroup))
					{
						if (undgGroup.Key.Equals(hasOverpackGroup))
						{
							var overpackInfoLine = CreateHasOverpackNatureAndQuantityOfDangerousGoodsLine();
							lines.Add(overpackInfoLine);
						}
						else
						{
							foreach (var line in CreateHasOverpackNatureAndQuantityOfDangerousGoodsLines(undgGroup.ToArray(), detailLines))
							{
								lines.Add(line);
							}
						}
					}
				}

				var detailLinesList = lines.Where(line => line.LineType == NatureAndQuantityOfDangerousGoodsLineType.Detail).ToList();
				if (detailLinesList.Count >= 1)
				{
					if (detailLinesList.Count > 1 && detailLinesList.Select(line => line.UNCode + line.Variant).Distinct().IsCountMoreThan(1))
					{
						lines.AddRange(CreateSummaryLines(packLine, lines));
					}
					detailLinesList.ForEach(detailLine => detailLine.IncludePackInformation = true);
				}

				foreach (var line in lines)
				{
					pager.Add(line);
				}
			}

			return pager.GetLinesPerPage();
		}

		const string hasOverpackGroup = "hasOverpackGroup";
		const string noOverpackGroup = "noOverpackGroup";
		const string overPackIdGroup = "overPackIdGroup";

		ZString GetUNDGGroupKey(UNDGDataItem undg)
		{
			if (!undg.DI_OverpackID.IsEmpty)
			{
				return overPackIdGroup + undg.DI_OverpackID;
			}

			if (undg.DI_HasOverpack)
			{
				return hasOverpackGroup;
			}

			return noOverpackGroup;
		}

		IEnumerable<NatureAndQuantityOfDangerousGoodsLine> CreateNatureAndQtyOfGoodsLines(IEnumerable<UNDGDataItem> undgs)
		{
			foreach (var group in undgs.GroupBy(undg => undg, new UNDGDataItemSubstanceAndPackTypeEqualityComparer()))
			{
				foreach (var undg in group)
				{
					var line = CreateNatureAndQuantityOfDangerousGoodsLine(undg);

					if (line != null)
					{
						yield return line;
					}
				}
			}
		}

		sealed class UNDGDataItemSubstanceAndPackTypeEqualityComparer : IEqualityComparer<UNDGDataItem>
		{
			public bool Equals(UNDGDataItem undg1, UNDGDataItem undg2)
			{
				return undg1.DI_DG == undg2.DI_DG
					&& undg1.DI_F3_NKPackType == undg2.DI_F3_NKPackType
					&& undg1.DI_IsLimitedQuantity == undg2.DI_IsLimitedQuantity
					&& undg1.DI_IsNotOtherwiseSpecified == undg2.DI_IsNotOtherwiseSpecified
					&& GetQuantityIndicator(undg1) == GetQuantityIndicator(undg2)
					&& undg1.DI_TechnicalName == undg2.DI_TechnicalName;
			}

			public int GetHashCode(UNDGDataItem undg)
			{
				unchecked // Overflow is fine, just wrap
				{
					var hash = 17;
					const int prime = 23;
					hash = hash * prime + undg.DI_DG.GetHashCode();
					hash = hash * prime + undg.DI_F3_NKPackType.GetHashCode();
					hash = hash * prime + undg.DI_IsLimitedQuantity.GetHashCode();
					hash = hash * prime + GetQuantityIndicator(undg).GetHashCode();
					hash = hash * prime + undg.DI_IsNotOtherwiseSpecified.GetHashCode();
					hash = hash * prime + undg.DI_TechnicalName.GetHashCode();
					return hash;
				}
			}

			string GetQuantityIndicator(UNDGDataItem undg) => Core.Constants.Weight.ContainsCode(undg.DI_UnitOfWeight) ? "W" : "V";
		}

		IEnumerable<UNDGDataItem> GetApplicableDangerousGoods(UNDGDataItemCollection undgs)
		{
			var packType = GetUNDGPackType(undgs);

			// do not display dry ice if it serves as individual substance. display it when it is used to pack or wrap other substances (less than 30kg)

			var res = undgs
				.Where(undg => (undg.Substance != null && undg.Substance.DG_Standard == UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA
					&& !IsExcludedDangerousGoodsUNCode(undg))
					&& (undg.Substance.DG_UNNO != UNDGConstants.Codes.DryIceUnno || Core.Constants.Weight.Convert(undg.DI_DGWeight, undg.DI_UnitOfWeight, Core.Constants.Weight.Kilograms) <= 30)
					&& DoesDangerousGoodExceedExceptedQuantity(undg, packType)).ToArray();

			return res.Length == 1 && res.First().Substance.DG_UNNO == UNDGConstants.Codes.DryIceUnno ? Array.Empty<UNDGDataItem>() : res;
		}

		bool DoesDangerousGoodExceedExceptedQuantity(UNDGDataItem undg, ExceptedQuantityUtilities.UNDGPackType packType) => !ExceptedQuantityUtilities.IsSubstancePermittedInLimitedQuantities(undg.Substance)
																															|| ValidUNDGExceptedQuantityChecker.DoesDangerousGoodsQuantityExceedMaximumNetAllowedPerPack(undg, packType);

		ExceptedQuantityUtilities.UNDGPackType GetUNDGPackType(IEnumerable<UNDGDataItem> undgs)
		{
			var packType = undgs.Count() == 1
				? ExceptedQuantityUtilities.UNDGPackType.SingleUNDGPack
				: ExceptedQuantityUtilities.UNDGPackType.MultiUNDGPack;

			return packType;
		}

		bool IsExcludedDangerousGoodsUNCode(UNDGDataItem dataItem)
		{
			if (dataItem == null || dataItem.Substance == null)
			{
				return false;
			}

			if (excludedDangerousGoodsUNCodes.Contains(dataItem.Substance.DG_UNNO))
			{
				return true;
			}

			return excludedLithiumBatteryDangerousGoodsUNCodes.Contains(dataItem.Substance.DG_UNNO) && dataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionII;
		}

		const string grossWeightIndicator = "G";

		readonly HashSet<string> excludedDangerousGoodsUNCodes = new HashSet<string>(new[]
		{
			"3164",
			"3373",
			"3245",
			"2807",
		});

		readonly HashSet<string> excludedLithiumBatteryDangerousGoodsUNCodes = new HashSet<string>(new[]
		{
			"3480",
			"3481",
			"3090",
			"3091"
		});

		readonly HashSet<string> displayPISectionLithiumBatteryDangerousGoodsUNCodes = new HashSet<string>(new[]
		{
			"3480",
			"3090"
		});

		int GetIdentifier(UNDGDataItem undg)
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = 17;
				const int prime = 23;

				hash = hash * prime + undg.PK.GetHashCode();

				return hash;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		NatureAndQuantityOfDangerousGoodsLine CreateNatureAndQuantityOfDangerousGoodsLine(UNDGDataItem undgDataItem)
		{
			if (undgDataItem == null)
			{
				return null;
			}

			var id = GetIdentifier(undgDataItem);
			var substance = undgDataItem.Substance;
			var isLimitedQuantity = undgDataItem.DI_IsLimitedQuantity;
			var limitType = isLimitedQuantity ? substance.DG_LQMaxAmtType : (IsCargoOnly ? substance.DG_CargoPackAmtType : substance.DG_LQ2OrPaxMaxAmtType);

			var packingInstruction = displayPISectionLithiumBatteryDangerousGoodsUNCodes.Contains(substance.DG_UNNO) && undgDataItem.DI_PackingInstructionSection == PackingInstructionSectionTypeList.Codes.SectionIB
				? ZString.Format("{0} - {1}", GetPackingInstruction(undgDataItem), undgDataItem.DI_PackingInstructionSection)
				: GetPackingInstruction(undgDataItem);
			var technicalName = undgDataItem.DI_TechnicalName;

			var res = new NatureAndQuantityOfDangerousGoodsLine(id, NatureAndQuantityOfDangerousGoodsLineType.Detail)
			{
				UNCode = substance.DG_UNNO,
				UNCodePrefix = undgDataItem.GetUnnoPrefix(),
				Variant = substance.DG_Variant,
				ProperShippingName = GetProperShippingName(undgDataItem),
				Class = substance.DG_Class + GetSubRisks(substance),
				PackingGroup = substance.DG_PG,
				PackingInstruction = packingInstruction,
				PackCount = undgDataItem.DI_PackageCount,
				PackageType = new CodeDescription(undgDataItem.Lookups.PackTypes)
				{
					Code = undgDataItem.DI_F3_NKPackType
				},
				LimitType = limitType,
				TechnicalName = technicalName,
				SpecialProvisionDescriptor = substance.SpecialProvisionDescriptor
			};

			var undgHasWeightUnit = Core.Constants.Weight.ContainsCode(undgDataItem.DI_UnitOfWeight);
			var undgHasVolumeUnit = Core.Constants.Volume.ContainsCode(undgDataItem.DI_UnitOfVolume);
			var undgHasNECWeightUnit = Core.Constants.Weight.ContainsCode(undgDataItem.DI_NECWeightUQ);

			var maxAmoutUQ = isLimitedQuantity ? substance.DG_LQMaxAmtUQ : (IsCargoOnly ? substance.DG_CargoMaxAmtUQ : substance.DG_LQ2OrPaxMaxAmtUQ);
			var isWeightMaxAmountUQ = Core.Constants.Weight.ContainsCode(maxAmoutUQ.ToUpperInvariant());
			var isVolumeMaxAmountUQ = Core.Constants.Volume.ContainsCode(maxAmoutUQ.ToUpperInvariant());

			var averageQuantityPerPack = 0m;
			var quantity = 0m;

			if (undgHasWeightUnit && (maxAmoutUQ.IsEmpty || isWeightMaxAmountUQ))
			{
				quantity = Core.Constants.Weight.Convert(undgDataItem.DI_DGWeight, undgDataItem.DI_UnitOfWeight, Core.Constants.Weight.Kilograms);
				averageQuantityPerPack = res.PackCount != 0 ? quantity / res.PackCount : 0;
				res.QuantityPerPack = CreateWeightMeasurement(averageQuantityPerPack);
				res.TotalQuantity = CreateWeightMeasurement(quantity);

				if (limitType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode)
				{
					res.QuantityIndicator = grossWeightIndicator;
				}
			}
			else if (undgHasVolumeUnit && (maxAmoutUQ.IsEmpty || isVolumeMaxAmountUQ))
			{
				quantity = Core.Constants.Volume.Convert(undgDataItem.DI_DGVolume, undgDataItem.DI_UnitOfVolume, Core.Constants.Volume.Litre);
				averageQuantityPerPack = res.PackCount != 0 ? quantity / res.PackCount : 0;
				res.QuantityPerPack = CreateVolumeMeasurement(averageQuantityPerPack);
				res.TotalQuantity = CreateVolumeMeasurement(quantity);
			}
			if (undgHasNECWeightUnit && undgDataItem.DI_NECWeight > 0)
			{
				quantity = Core.Constants.Weight.Convert(undgDataItem.DI_NECWeight, undgDataItem.DI_NECWeightUQ, Core.Constants.Weight.Kilograms);
				res.NECWeight = CreateWeightMeasurement(quantity);
			}

			return res;
		}

		ZString GetProperShippingName(UNDGDataItem undg)
		{
			var undgDocObject = new DangerousGoodBuilder().Build(undg, null);
			var components = new string[] {
				undg.TryGetPSNWithAdditionalTextIfSupportForNOS(),
				undgDocObject?.HRCQComponent() ?? string.Empty
			};
			return string.Join(", ", components.Where(str => !string.IsNullOrEmpty(str)));
		}

		ZString GetSubRisks(UNDGSubstance substance)
		{
			var sb = new ZStringBuilder();
			sb.AppendIfNotEmpty(substance.DG_SubLabel1);
			sb.AppendIfNotEmpty(substance.DG_SubLabel2);
			var subRisk = ZString.Empty;

			if (!sb.IsEmpty)
			{
				subRisk = " (" + sb.ToStringWithDelimiterBetweenAppends(",") + ")";
			}

			return subRisk;
		}

		Measurement CreateWeightMeasurement(decimal weight)
		{
			var measurement = new Measurement
			{
				Value = Utilities.Round(weight, quantityRoundingScale),
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};

			return measurement;
		}

		Measurement CreateVolumeMeasurement(decimal volume)
		{
			var measurement = new Measurement
			{
				Value = Utilities.Round(volume, quantityRoundingScale),
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = Core.Constants.Volume.Litre
				}
			};

			return measurement;
		}

		const int quantityRoundingScale = 3;

		#region SuppressResourceStringsCheckRegion

		NatureAndQuantityOfDangerousGoodsLine CreateSummaryNatureAndQuantityOfDangerousGoodsLine(ForwardingPackLine packLine)
		{
			return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
			{
				Description = FormattableString.Invariant($"All Packed in One ({packLine.Lookups.PackTypes.GetDescriptionFromCode(packLine.JL_F3_NKPackType)}(s) x {packLine.JL_PackageCount}).")
			};
		}

		NatureAndQuantityOfDangerousGoodsLine CreateSummaryGrossWeightNatureAndQuantityOfDangerousGoodsLine(IEnumerable<NatureAndQuantityOfDangerousGoodsLine> lines)
		{
			return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
			{
				Description = FormattableString.Invariant($"Total Gross Weight: {lines.Sum(l => l.Quantity?.Value ?? 0)} {Core.Constants.Weight.Kilograms}.")
			};
		}

		IEnumerable<NatureAndQuantityOfDangerousGoodsLine> CreateSummaryLines(ForwardingPackLine packLine, IEnumerable<NatureAndQuantityOfDangerousGoodsLine> lines)
		{
			var qValue = packLine.QValue;
			var hasOverPack = packLine.UNDGs.Any(dg => dg.DI_HasOverpack);

			if (!hasOverPack)
			{
				yield return CreateSummaryNatureAndQuantityOfDangerousGoodsLine(packLine);

				if(qValue != 0)
				{
					yield return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary) { Description = FormattableString.Invariant($"Q = {qValue}") };
				}
			}

			var glmLines = lines.Where(l => l.LimitType == UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode).ToArray();

			if (glmLines.Length > 0)
			{
				yield return CreateSummaryGrossWeightNatureAndQuantityOfDangerousGoodsLine(glmLines);
			}
		}

		NatureAndQuantityOfDangerousGoodsLine CreateHasOverpackNatureAndQuantityOfDangerousGoodsLine()
		{
			return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
			{
				Description = "Overpack Used"
			};
		}

		IEnumerable<NatureAndQuantityOfDangerousGoodsLine> CreateHasOverpackNatureAndQuantityOfDangerousGoodsLines(UNDGDataItem[] undgs, IEnumerable<NatureAndQuantityOfDangerousGoodsLine> detailLines)
		{
			var overpackIds = new HashSet<string>(undgs.Select(undg => undg.DI_OverpackID.ToString()));

			yield return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
			{
				Description = FormattableString.Invariant($"Overpack used x {overpackIds.Count}")
			};

			yield return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
			{
				Description = string.Join(" ", overpackIds.Select(id => FormattableString.Invariant($"Overpack ID# {id}")))
			};

			if (overpackIds.Any())
			{
				var unit = ZString.Empty;
				var totalQuantity = detailLines.Where(l => l.Quantity != null).Sum(l => l.Quantity.Value) / overpackIds.Count;
				if (detailLines.Where(l => l.Quantity?.Unit != null).All(l => l.Quantity.Unit.Code == Core.Constants.Volume.Litre))
				{
					unit = Core.Constants.Volume.Litre;
				}
				else
				{
					unit = detailLines.All(l => l.Quantity != null) && detailLines.Select(l => l.Quantity?.Unit?.Code).Distinct().IsCountEqualTo(1)
						? detailLines.First().Quantity.Unit.Code
						: (ZString)Core.Constants.PkgUnit.Package;
				}

				yield return new NatureAndQuantityOfDangerousGoodsLine(NatureAndQuantityOfDangerousGoodsLineType.Summary)
				{
					Description = FormattableString.Invariant($"Total quantity per overpack {totalQuantity} {unit}")
				};
			}
		}

		#endregion

		#endregion
	}
}
