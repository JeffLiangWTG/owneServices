using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using RatesService = WiseRates.Api.Model;
using RefServiceLevel = Enterprise.MasterFiles.Business.RefServiceLevel;

namespace Enterprise.Rating.Business
{
	public static class DescriptionHelpers
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Rate Description creator")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode", Justification = "Rate Description combined from various sources")]
		public static string GetRateLineDescription(IRateLine line, RatingCriteria criteria, string cartageZoneDescription = null, IAutoRatingChargeInfo relatedCharge = null)
		{
			var entryDescription = new ZStringBuilder();

			if (line == null)
			{
				return string.Empty;
			}

			var rateEntry = line.ParentRateEntry;

			entryDescription.AppendLine(line.DescriptionWithInclusiveCharges());
			entryDescription.AppendLine();

			if (rateEntry.IsSpotEntry)
			{
				entryDescription.AppendLine(GetSpotRateChargeIntroduction(line, rateEntry, criteria));
			}
			else
			{
				entryDescription.Append(Res.GetString("b632699b-db0e-4e89-b654-a80cc8dfc2bc", "Charge located in") + " ");
				entryDescription.Append(GetChargeName(line, criteria));
				entryDescription.AppendLine(" " + Res.GetString("7eb8460f-e034-4f9d-8842-4b22c040b3e6", "with the following details:"));
			}
			entryDescription.AppendLine();

			if (!rateEntry.IsCostRate() && !GetPaymentTerm(line, criteria).IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("ba9e5987-ca75-4a0e-a565-53d613df3c79", "Payment Term:"));
				entryDescription.Append((string)GetPaymentTerm(line, criteria));
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_Mode.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a881ab79-c0f5-4f09-bb36-c02eb4429aa5", "Mode:"));
				entryDescription.Append(rateEntry.TI_Mode);
				entryDescription.AppendLine();
			}

			if (!line.ChargeCode.AC_ChargeGroup.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a306ebb6-2211-4ef5-b4c8-538ee414387d", "Charge Code Group:"));
				entryDescription.Append(line.ChargeCode.AC_ChargeGroup);

				if (!line.ChargeCode.AC_ChargeSubGroup.IsEmpty)
				{
					entryDescription.Append(" / " + line.ChargeCode.AC_ChargeSubGroup);
				}

				entryDescription.AppendLine();
			}

			if (!rateEntry.RateProvider.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("f8c0bb70-4713-11e6-9746-fcaa14295823", "Rate Provider:"));
				entryDescription.Append(rateEntry.RateProvider);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_ContractNumber.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription,
					(rateEntry.IsCostRate() ? Res.GetString("4cfcbc9a-2e30-46e8-88bf-a4ba3e366f07", "Carrier") : Res.GetString("3535c92a-c46f-4de1-bfff-c65f4a02d728", "Client"))
					+ " " +
					Res.GetString("8F938AFF-B70B-4D7D-B0D8-7669981E52EA", "Contract Number:")
				);
				entryDescription.Append(rateEntry.TI_ContractNumber);
				entryDescription.AppendLine();
			}

			AppendCGReferenceCustomFieldsDescription(entryDescription, line, rateEntry);

			if (!rateEntry.TI_RateStartDate.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("941f3a37-6445-4076-aef7-e4aab093197a", "Start Date:"));
				entryDescription.Append(rateEntry.TI_RateStartDate.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture));
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_RateEndDate.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("71d0cb55-6288-402a-b3d2-82c61d4e6e42", "End Date:"));
				entryDescription.Append(rateEntry.TI_RateEndDate.ToString("dd MMMM yyyy", CultureInfo.CurrentCulture));
				entryDescription.AppendLine();
			}

			AppendInlandCustomFieldsDescriptions(entryDescription, line, rateEntry);
			AppendOceanCustomFieldsDescriptions(entryDescription, line, rateEntry);
			AppendOriginDescriptions(entryDescription, line, rateEntry);
			AppendDestinationDescriptions(entryDescription, line, rateEntry);
			AppendOutlandCustomFieldsDescriptions(entryDescription, line, rateEntry);
			AppendHandlingOfficeLocation(entryDescription, line, rateEntry);

			if (rateEntry.DestinationZone != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("c9222371-315e-4e24-b353-1eb1fc0587f3", "Destination Zone:"));
				entryDescription.Append(rateEntry.DestinationZone.TZ_ZoneName);
				entryDescription.AppendLine();
			}

			var via = rateEntry.Via();
			if (via != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("7f314823-dee0-47f7-ad32-6b1bd0a4670d", "Via:"));
				entryDescription.Append(via.Code);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_RateOrigin.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("b17afb72-096f-44d5-96bb-7c0045f19cf8", "Rate Origin:"));
				entryDescription.Append(rateEntry.TI_RateOrigin);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_RateDestination.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("d53d8510-ed74-4c5d-a6e7-843babea0333", "Rate Destination:"));
				entryDescription.Append(rateEntry.TI_RateDestination);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_PlannedLoadLRC.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a3851139-9258-4418-ab9d-e58a94fe6b4d", "Planned Load:"));
				entryDescription.Append(rateEntry.TI_PlannedLoadLRC);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_PlannedDischargeLRC.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("24eebb26-e093-48a5-8e5d-2b794899200e", "Planned Discharge:"));
				entryDescription.Append(rateEntry.TI_PlannedDischargeLRC);
				entryDescription.AppendLine();
			}

			AppendMatchingLocations(entryDescription, rateEntry);

			if (!rateEntry.TI_TransitTime.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("4E07C150-26AE-4C6D-BB67-715307363909", "Transit Time:"));
				entryDescription.Append(rateEntry.TI_TransitTime);
				entryDescription.AppendLine();
			}

			if (!line.UniversalChargeCodes.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a41dcb93-a30c-485f-8cce-54e0002fdd2c", "Universal Charge Codes:"));
				entryDescription.Append(line.UniversalChargeCodes);
				entryDescription.AppendLine();
			}

			if (!line.CarrierChargeCode.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("f5f2edea-39ce-4a42-b829-6134fc2e9cf2", "Carrier Charge Code:"));
				entryDescription.Append(line.CarrierChargeCode);
				entryDescription.AppendLine();
			}

			if (rateEntry.TI_IsCrossTrade)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("085f4617-00ed-4570-ad5c-e3419820f954", "Cross-Trade Rate:"));
				entryDescription.Append(Res.GetString("a85e8e4c-8b62-4c1b-bbc6-c28e16f8af87", "Yes"));
				entryDescription.AppendLine();
			}

			if (rateEntry.TransportProvider != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("97589ac1-7bab-456f-9dc5-03e71a9348b5", "Carrier:"));
				entryDescription.Append(rateEntry.TransportProvider.OH_FullNameTruncated);
				entryDescription.AppendLine();
			}

			if (rateEntry.Consignor != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("8693ad97-152b-44a1-985a-ee5e9c10e013", "Consignor:"));
				entryDescription.Append(rateEntry.Consignor.OH_FullNameTruncated);
				entryDescription.AppendLine();
			}

			if (rateEntry.Consignee != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("b444ad64-9731-40e3-b30b-a85403eed5eb", "Consignee:"));
				entryDescription.Append(rateEntry.Consignee.OH_FullNameTruncated);
				entryDescription.AppendLine();
			}

			if (rateEntry.ControllingCustomer != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("8dce169c-7a1b-4e03-b2ea-7cc0d11d8ae4", "Ctrl. Cus.:"));
				entryDescription.Append(rateEntry.ControllingCustomer.OH_FullNameTruncated);
				entryDescription.AppendLine();
			}

			if (rateEntry.Supplier != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("12eeccf8-0b62-4b6a-94fe-2af18485b7b6", "Svc. Provider:"));
				entryDescription.Append(rateEntry.Supplier.OH_FullNameTruncated);
				entryDescription.AppendLine();
			}

			if (rateEntry.Container != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a3c7fc9b-98c1-47e5-bffc-a657bf024088", "Container:"));

				var containerOwnership = string.Empty;
				if (line.TL_WeightVolume == RatingConstants.Units.CN && !line.TL_ContainerOwnership.IsEmpty)
				{
					containerOwnership = RateLinesLookups.GetContainerOwnershipsDescription(line);
				}

				entryDescription.Append((rateEntry.Container.RC_Code + " " + containerOwnership).Trim());
				entryDescription.AppendLine();
			}

			if (rateEntry.IsWHS())
			{
				if (!rateEntry.TI_WW_Warehouse.IsEmpty)
				{
					AppendDescriptionPrefix(entryDescription, Res.GetString("b53131a1-c70a-4d09-b8b8-51a4ee5a4abb", "Warehouse:"));
					entryDescription.Append((ZString)rateEntry.Warehouse()[WhsWarehouseSchema.WW_WarehouseName.Name]);
					entryDescription.AppendLine();
				}

				if (line.ProductNumber() != null)
				{
					AppendDescriptionPrefix(entryDescription, Res.GetString("1e1739c6-420d-4f3b-8f2a-5019145967a2", "Product:"));
					entryDescription.Append(line.ProductNumber().OP_PartNum);
					entryDescription.AppendLine();
				}
			}

			if ((rateEntry.IsTRW() || rateEntry.IsTWU()) && !rateEntry.TI_WW_Warehouse.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("b53131a1-c70a-4d09-b8b8-51a4ee5a4abb", "Warehouse:"));
				entryDescription.Append((ZString)rateEntry.Warehouse()[WhsWarehouseSchema.WW_WarehouseName.Name]);
				entryDescription.AppendLine();
			}

			if ((rateEntry.IsContainerYard() || rateEntry.IsContainerYardTPU()) && !rateEntry.TI_WW_Warehouse.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("edf6d05b-7d3c-49a6-ade6-0eeed685c036", "Yard:"));
				entryDescription.Append((ZString)rateEntry.Warehouse()[WhsWarehouseSchema.WW_WarehouseName.Name]);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_RS_NKServiceLevel_NI.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("ccc1cd1c-06d7-4839-b9cb-5f630c5f19eb", "Service Level:"));
				entryDescription.Append(rateEntry.TI_RS_NKServiceLevel_NI);

				var serviceLevel = rateEntry.Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, rateEntry.TI_RS_NKServiceLevel_NI);
				if (serviceLevel != null && serviceLevel.RS_IsDoorToDoor)
				{
					entryDescription.Append(" " + Res.GetString("fd40c500-08b6-4b33-ac3c-f444fbac8736", "(Door-to-Door)"));
				}
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_PL_NKCarrierServiceLevel.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("da16f69c-1fb9-46c6-b110-dd7b7e77194c", "Carrier Service Level:"));
				entryDescription.Append(rateEntry.TI_PL_NKCarrierServiceLevel);
				entryDescription.AppendLine();
			}

			if (rateEntry.IsClientRate() || rateEntry.IsCompanyTariff())
			{
				if (!rateEntry.TI_HBLDeliveryMode.IsEmpty)
				{
					AppendDescriptionPrefix(entryDescription, Res.GetString("d6fa3051-e6c0-4530-8892-613d9e8355b2", "HBL Delivery Mode:"));
					entryDescription.Append(rateEntry.TI_HBLDeliveryMode);
					entryDescription.AppendLine();
				}
			}

			if (!rateEntry.TI_RH_NKCommodityCode.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("1ef55c34-f091-4e39-bd13-ecedd1133f11", "Commodity Code:"));
				entryDescription.Append(rateEntry.TI_RH_NKCommodityCode);
				entryDescription.AppendLine();
			}

			if (!rateEntry.TI_Frequency.IsEmpty && rateEntry.TI_Frequency > 0)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("1c35bab8-b668-4fda-a36a-e2cf4ed06e20", "Frequency:"));
				entryDescription.Append($"{rateEntry.TI_Frequency}");
				entryDescription.AppendLine();

				AppendDescriptionPrefix(entryDescription, Res.GetString("5ac0e888-1c39-48b0-9a5c-390c6ec29efc", "Frequency Unit:"));
				entryDescription.Append(rateEntry.TI_FrequencyUnit);
				entryDescription.AppendLine();
			}

			DescribeAllRateEntryFields(entryDescription, rateEntry);

			if (!rateEntry.IsCostRate() && !rateEntry.TI_FMCTariffID.IsEmpty)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("5bd76c97-7ce6-4f75-b509-d0446c2b0c11", "FMC Tariff ID:"));
				entryDescription.Append(rateEntry.TI_FMCTariffID);
				entryDescription.AppendLine();
			}

			AppendPriceByCustomFieldDescription(entryDescription, line, rateEntry);

			var rateLine = line as RateLine;
			if (rateLine != null)
			{
				if (!string.IsNullOrEmpty(rateLine.ChargeInformationNoteText))
				{
					AppendDescriptionPrefix(entryDescription, Res.GetString("721781f4-c27e-459b-b3b2-fb23149ec7b6", "Public Note:"));
					entryDescription.Append(rateLine.ChargeInformationNoteText);
					entryDescription.AppendLine();
				}
				if (!string.IsNullOrEmpty(rateLine.ChargeInternalNoteText))
				{
					AppendDescriptionPrefix(entryDescription, Res.GetString("7b749ddf-5fe6-4307-a982-373901391022", "Internal Note:"));
					entryDescription.Append(rateLine.ChargeInternalNoteText);
					entryDescription.AppendLine();
				}
			}

			if (!line.TL_Condition.IsEmpty && !(line.TL_Condition == RateLineConditions.UserDefined && line.TL_ConditionalExpression.IsEmpty))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("704c54c5-a6a6-492b-816d-5cc194914d66", "Rate Line Condition:"));

				if (!line.TL_Condition.IsEmpty)
				{
					if (line.TL_Condition == RateLineConditions.UserDefined)
					{
						entryDescription.Append(line.TL_ConditionalExpression);
					}
					else
					{
						entryDescription.Append(RateLinesLookups.GetRateLineConditionsDescription(line));
					}
				}

				entryDescription.AppendLine();
			}

			if (line.UsesCompanyTariffOrCostBasedCalculator())
			{
				AddInheritanceInfo(entryDescription, criteria, line, new List<IRateLine>(), relatedCharge);
				entryDescription.AppendLine();
			}

			var summarizerWillDoTheJob = criteria.MergeCharges == MergeChargeOptions.CrossAdapter || (criteria.MergeCharges == MergeChargeOptions.HLSMerge);

			if (!summarizerWillDoTheJob && criteria.AutoRatedFor.Count > 0)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("53e0d1a8-4a30-41a2-8b5e-7c679919d0ba", "Autorated for:"));
				entryDescription.Append(criteria.HumanReadableName());
				entryDescription.AppendLine();
			}

			var isCosting = rateEntry.IsCosting() || rateEntry.IsWiseCost();
			var (rateDateType, autoratingDate) = criteria.GetEffectiveDate(line, shouldAcceptInvalidDateWhenFallbackIsEnabled: true, isCosting: isCosting);
			if (rateDateType != null && autoratingDate.IsValid)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("769c91c7-0f16-4fda-a922-c0410c0b7c85", "Autorating Date:"));
				// Let's just print whatever we have, even if it's invalid
				entryDescription.Append(autoratingDate.ToShortDateString());
				if (rateDateType.DateType == JobDateTypes.Codes.CostingAutoratingDateOverride)
				{
					entryDescription.Append(" (");
					entryDescription.Append(Res.GetString("b846a4ab-ddb8-47c9-b83a-539efc597684", "Override"));
					entryDescription.Append(")");
				}
				entryDescription.AppendLine();
			}

			if (!rateEntry.IsWHS() && !rateEntry.IsTRW() && !rateEntry.IsTWU())
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("dbfaecfc-7843-4831-b5da-6c9e2ff012f1", "Leg:"));
				entryDescription.Append(criteria.GetLeg(rateEntry).ToString());
				entryDescription.AppendLine();
			}

			if (RatingDataRegistry.Instance.MultiModalRatingCost.Value)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("bccc721b-81a1-4019-b8cc-8e240550a07f", "Route Set Number:"));
				entryDescription.Append(criteria.RouteSetNumber.ToString());
				entryDescription.AppendLine();
			}

			if (!string.IsNullOrEmpty(cartageZoneDescription))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("9f1ce1d7-e677-434e-9dac-929e798c2859", "Zone:"));
				entryDescription.Append(cartageZoneDescription);
				entryDescription.AppendLine();
			}

			if (line.Uses(CalculatorType.Equalization) && rateEntry.Container != null)
			{
				entryDescription.AppendLine();
				AppendDescriptionPrefix(entryDescription, Res.GetString("4348e29a-414f-4b7a-9d7f-fa26422c3c19", "Volume Equalization Discount details:"));
				entryDescription.Append(_Rating.EqualizationInfo.GetEqualizationDetailsPerRateLine(line.PK));
			}

			if (!line.TL_FeeChargeType.IsEmpty)
			{
				var feeChargeType = line.GetFeeChargeType();

				var chargeLevel = feeChargeType == null
					? null
					: feeChargeType.FeeChargeLevels.Cast<FeeChargeLevel>().FirstOrDefault(x => x.Code == line.TL_FeeChargeLevel);

				AppendDescriptionPrefix(entryDescription, Res.GetString("2f136a3d-6378-4d4e-b7d6-22d3047d195d", "Fee Charge Type:"));
				entryDescription.Append((feeChargeType != null ? (ZString)(line.TL_FeeChargeType + " - " + feeChargeType.Description) : line.TL_FeeChargeType));
				entryDescription.AppendLine();

				AppendDescriptionPrefix(entryDescription, Res.GetString("2f9e0de6-540d-4517-bb48-282e89eb8392", "Fee Charge Level:"));
				entryDescription.Append((chargeLevel != null ? (ZString)(line.TL_FeeChargeLevel + " - " + chargeLevel.Description) : line.TL_FeeChargeLevel));
				entryDescription.AppendLine();
			}

			AddDescriptionForNamedAccounts(entryDescription, rateEntry);

			entryDescription.AppendLine();
			entryDescription.AppendLine();

			if (!summarizerWillDoTheJob)
			{
				entryDescription.AppendLine(Res.GetString("04e3333c-b022-477b-94dd-7a31efab583e", @"User:		{0}", GlbStaff.CurrentUser.GS_FullName));
				entryDescription.AppendLine(Res.GetString("8d6aa377-405a-4341-9ccf-3ee1aa1a1755", @"Time:		{0}", ZDateTime.Now));
				entryDescription.AppendLine();
			}

			return entryDescription.ToString();
		}

		static void AppendDescriptionPrefix(ZStringBuilder entryDescription, string descriptionPrefix) =>
			entryDescription.Append(FormatWithTab(descriptionPrefix));

		/// <summary>
		/// Onscreen ZTextBox formatting uses tabWidth 8.
		/// </summary>
		public static string FormatWithTab(string descriptionPrefix, int tabWidth = 8, int maxLength = 24)
		{
			if (descriptionPrefix.Length > maxLength)
			{
				return descriptionPrefix + System.Environment.NewLine + new string('\t', maxLength / tabWidth);
			}
			
			var tabsRequired = Math.Max(0, (maxLength - descriptionPrefix.Length + tabWidth - 1) / tabWidth);
			return descriptionPrefix + new string('\t', tabsRequired);
		}

		static void AppendHandlingOfficeLocation(ZStringBuilder entryDescription, IRateLine line, IRateEntry rateEntry)
		{
			if (line is WiseLine wiseLine)
			{
				var handlingOffice = wiseLine
					.GetCustomFieldValue(Rate.CustomFields.CargoSphere.HandlingOffice)?
					.ToString();

				if (!string.IsNullOrEmpty(handlingOffice))
				{
					entryDescription.AppendLine($"{FormatWithTab(Res.GetString("cee571be-47dc-4614-8b19-6b6885503d44", "Handling Office:"))}{handlingOffice}");
				}
			}
		}

		static void AppendMatchingLocations(ZStringBuilder entryDescription, IRateEntry rateEntry)
		{
			var locations =
			new[] {
					new { Code = RateEntryLookups.LocationSourceOption.Code.FirstLoad, Location = rateEntry.TI_FirstLoadLRC },
					new { Code = RateEntryLookups.LocationSourceOption.Code.LastDischarge, Location = rateEntry.TI_LastDischargeLRC },
					new { Code = RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad, Location = rateEntry.TI_FirstRouteSetLoadPortLRC },
					new { Code = RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge, Location = rateEntry.TI_LastRouteSetDischargePortLRC },
				}.Where(x => !x.Location.IsEmpty)
				 .Select(location => $"{location.Location} ({location.Code})");
			if (!locations.Any())
			{
				return;
			}
			AppendDescriptionPrefix(entryDescription, Res.GetString("4a1d355b-06a1-415d-a6ef-8fccd593f835", "Matching Locations:"));
			entryDescription.Append(string.Join(", ", locations));
			entryDescription.AppendLine();
		}

		// These columns have already been described manually in GetRateLineDescription or intentionally ignored
		internal static readonly HashSet<string> manuallyDescribedOrIgnoredColumns = new HashSet<string>()
		{
			"TI_Mode",
			"TI_ContractNumber",
			"TI_RateStartDate",
			"TI_RateEndDate",
			"TI_OriginLRC",
			"TI_DestinationLRC",
			"TI_TZ_DestinationZone",
			"TI_ViaLRC",
			"TI_TransitTime",
			"TI_FirstLoadLRC",
			"TI_LastDischargeLRC",
			"TI_FirstRouteSetLoadPortLRC",
			"TI_LastRouteSetDischargePortLRC",
			"TI_IsCrossTrade",
			"TI_WW_Warehouse",
			"TI_RS_NKServiceLevel_NI",
			"TI_PL_NKCarrierServiceLevel",
			"TI_HBLDeliveryMode",
			"TI_RH_NKCommodityCode",
			"TI_Frequency",
			"TI_FrequencyUnit",
			"TI_FMCTariffID",
			"TI_OH_TransportProvider",
			"TI_OH_Supplier",
			"TI_OH_Consignor",
			"TI_OH_Consignee",
			"TI_OH_ControllingCustomer",
			"TI_RateOrigin",
			"TI_RateDestination",
			"TI_PlannedLoadLRC",
			"TI_PlannedDischargeLRC",
			"TI_LineOrder",
			"TI_SystemCreateTimeUtc",
			"TI_SystemCreateUser",
			"TI_SystemLastEditTimeUtc",
			"TI_SystemLastEditUser",
			"TI_RateCategory",
			"TI_DataChecked",
			"TI_ParentID",
			"TI_ParentTableCode",
			"TI_CYC_WW_Facility"
		};

		internal static void DescribeAllRateEntryFields(ZStringBuilder entryDescription, IRateEntry rateEntry)
		{
			if (rateEntry is not RateEntry bo)
			{
				return;
			}
			var rateEntryFields = bo.GetFields();
			var columns = RateEntrySchema.All.Where(c => !manuallyDescribedOrIgnoredColumns.Contains(c.Name) && rateEntryFields.Contains(c.Name));
			foreach (var column in columns)
			{
				var value = bo[column.Name];
				if (value == null
					|| value.ToString().IsNullOrEmpty()
					|| value is ZGuid)
				{
					continue;
				}
				else if (value is ZBool zBool)
				{
					if (zBool == false)
					{
						continue;
					}
					value = Res.GetString("a85e8e4c-8b62-4c1b-bbc6-c28e16f8af87", "Yes");
				}

				var info = bo.FindPropertyInfo(column.Name);
				string description = info?.HumanReadableName ?? column.Name;
				entryDescription.AppendLine($"{FormatWithTab($"{description}:")}{value}");
			}
		}

		internal static string ToStringForCalculationLog(ZDecimal amountToFormat)
		{
			var format = "f4";
			if (amountToFormat.ToString(format, Culture.CurrentCompanyCountryCulture).EndsWith("00", StringComparison.OrdinalIgnoreCase))
			{
				format = "f2";
			}
			else if (amountToFormat.ToString(format, Culture.CurrentCompanyCountryCulture).EndsWith("0", StringComparison.OrdinalIgnoreCase))
			{
				format = "f3";
			}

			return amountToFormat.ToString(format, Culture.CurrentCompanyCountryCulture);
		}
		static string GetSpotRateChargeIntroduction(IRateLine line, IRateEntry rateEntry, RatingCriteria criteria)
		{
			ZString charge;

			var rateLine = line as RateLine;
			if (rateLine != null && !rateLine.SpotRateDescription.IsEmpty)
			{
				charge = rateLine.SpotRateDescription;
			}
			else
			{
				var spotRateInfo = rateEntry.IsCostRate() ? criteria.CostSpotRateInfo : criteria.SellSpotRateInfo;
				charge = spotRateInfo.GetAutoratedValueTypeDescription();
			}

			var jobDescription = GetJobDescriptionFromCriteria(line, criteria);

			if (rateEntry.IsJobServiceSpotEntry)
			{
				var rateType = rateEntry.IsCostRate()
					? Res.GetString("d24fd526-d01e-4639-8e38-ad4ce1e00e12", "Cost")
					: Res.GetString("605b279c-0655-4d6d-94d3-459b34b32197", "Charge");

				return Res.GetString("ad404ce8-0a26-4b1d-ab17-d6888a08e5f8", "{0} {1} is applicable for {2}.", charge, rateType, jobDescription);
			}
			else
			{
				return Res.GetString("f204467a-6204-48dd-a52c-58a12774738e", "{0} is applicable for {1}.", charge, jobDescription);
			}
		}

		static ZString GetJobDescriptionFromCriteria(IRateLine line, RatingCriteria criteria)
		{
			ZString result = criteria.HumanReadableName();

			if (line.IsContainerSpotRate())
			{
				var measures = criteria.JobMeasures;
				result = Res.GetString("af017889-8bfb-4594-a059-970c9277702b", "Container Number");

				if (measures.HasContainerMeasure)
				{
					var containerNumber = measures.GetContainerSpotRates()
						.Where(x => x.ContainerPK == line.ParentRateEntry.ContainerPKForSpotEntry)
						.Select(x => x.ContainerNumber)
						.FirstOrDefault();
					if (!string.IsNullOrEmpty(containerNumber))
					{
						result += " " + containerNumber;
					}
				}
				return result;
			}

			if (line.ChargeCode != null && Equals(criteria.ConsumerType, JobInvoicingConsumerTypes.Shipment) && line.ParentRateEntry.IsJobServiceSpotEntry)
			{
				result += " ";

				if (line.ChargeCode.AC_ChargeGroup == ChargeCodeGroupList.Codes.Destination)
				{
					result += Res.GetString("08aec663-63da-4011-8446-6186feb61c68", "Delivery");
				}
				else
				{
					result += Res.GetString("388e7f9f-7917-44a2-a984-eeaa73e4e2f7", "Pickup");
				}
			}

			if (result.IsEmpty && !criteria.JobNumber.IsEmpty)
			{
				result = criteria.JobNumber;
			}

			if (result.IsEmpty)
			{
				result = Res.GetString("8b395214-64dd-43ba-851d-c070d23a0b80", "this job");
			}

			return result;
		}

		static string AddGlobalPrefixIfNeeded(IRateLine rateLine, string rateTypeString)
		{
			if (rateLine?.ParentRateEntry?.ParentRatingHeader?.Company != null)
			{
				return rateTypeString;
			}

			return rateLine?.ParentRateEntry?.ParentRatingHeader?.IsIntercompanyTariff() ?? false
				? Res.GetString("fe1fd680-ac02-458c-9b5e-11e5b35a5146", "Intercompany Tariff")
				: Res.GetString("e4253a7c-7ee6-43ef-a181-a1c5a3dc06e1", "global {0}", rateTypeString);
		}

		static string GetChargeName(IRateLine rateLine, RatingCriteria criteria)
		{
			var result = new ZStringBuilder();

			var rateEntry = rateLine.ParentRateEntry;
			var orgCode = rateEntry.ParentRatingHeader.Header != null ? rateEntry.ParentRatingHeader.Header.OH_Code : ZString.Empty;

			var clientRateString = AddGlobalPrefixIfNeeded(rateLine, Res.GetString("18cf8783-42ef-406c-b0a2-db663b5ea57a", "client rate"));
			var wiseCostingString = Res.GetString("6F5EC509-A28C-4C0B-A3DA-63AE8DB12BD4", "Wise Costing");
			var costingString = AddGlobalPrefixIfNeeded(rateLine, Res.GetString("F89DA159-0D6C-4EBD-BF59-1F306AF9A8EC", "cost"));
			var standardCostingString = AddGlobalPrefixIfNeeded(rateLine, Res.GetString("F13D9571-C10C-4C96-A082-ABDBF0FFD1ED", "standard cost"));
			var tariffApplicableOrgCodes = GetTariffApplicableOrgCodes(rateLine, criteria);

			if (rateEntry.IsClientRateHavingSubsidiaryRelations() && !string.IsNullOrWhiteSpace(tariffApplicableOrgCodes))
			{
				result.Append(Res.GetString("5f01333a-7cb9-48e1-906f-c0dfd376d803", "{0} group client rate", orgCode));
				result.Append(" " + Res.GetString("814c2352-9352-4efc-bcb5-f734641018f0", "(Linked to {0}: {1})", clientRateString, tariffApplicableOrgCodes));
			}
			else if (rateEntry.IsClientRate() || (_Rating.Sell && rateEntry.IsIntercompanyTariff()))
			{
				result.Append(Res.GetString("c76d2a48-20a3-4fcb-adef-11724fee44a3", "{0} {1}", orgCode, clientRateString).Trim());
			}
			else if (rateEntry.IsCostRate())
			{
				if (rateEntry.ParentRatingHeader.Header != null)
				{
					OrgWithSource orgsource = null;

					if (criteria != null)
					{
						var providers = criteria.Creditors[rateLine.ChargeCode.AC_ChargeGroup];
						orgsource = providers == null ? null : providers.FirstOrDefault(x => x.Org.OH_Code == orgCode);
					}

					var sourcetext = orgsource == null || string.IsNullOrWhiteSpace(orgsource.SourceText) ? null : "(" + orgsource.SourceText + ")";

					var rhCode = rateEntry.IsWiseCost()
						? wiseCostingString
						: costingString;

					result.Append(new ZStringBuilder(new string[] { orgCode, sourcetext, rhCode }).ToStringWithDelimiterBetweenAppends(" ").Trim());
				}
				else
				{
					var rhCode = rateEntry.IsWiseCost()
						? wiseCostingString
						: standardCostingString;

					var rhDesc = rateEntry.ParentRatingHeader != null ? rateEntry.ParentRatingHeader.TH_GlobalRateDescriptionMultilingual : ZString.Empty;
					result.Append(new ZStringBuilder(new string[] { rhCode, rhDesc }).ToStringWithDelimiterBetweenAppends(" - ").Trim());
				}
			}
			else if (rateEntry.IsCompanyTariff())
			{
				var tariffLevel = rateEntry.ParentRatingHeader.TH_GlobalRateLevel.ToString();
				result.Append(AddGlobalPrefixIfNeeded(rateLine, Res.GetString("a2299c63-4a49-4e78-be14-c084fd977e02", "Company Tariff Level {0}", tariffLevel)).Trim());
				if (criteria.TariffLevel.ToString() == tariffLevel)
				{
					result.Append(" " + Res.GetString("8633db89-0dd1-4266-8abf-3e476e650863", "(Overridden in job)", tariffApplicableOrgCodes));
				}
				else
				{
					result.Append(" " + Res.GetString("cd3d1f11-ba12-405b-9900-be80b56d09ad", "(Linked to: {0})", tariffApplicableOrgCodes));
				}
			}

			else if (rateEntry.IsOneOffQuote())
			{
				var quoteNumber = rateEntry.ParentRatingHeader.TH_QuoteNumber;
				result.Append(Res.GetString("19ed44f0-523d-4022-8307-9a47b2c0cc9d", "{0} one off quotation {1}", orgCode, quoteNumber).Trim());
			}
			else if (rateEntry.IsQuote())
			{
				result.Append(Res.GetString("480b4046-ee42-4e5a-86e1-17203762e523", "{0} quotation", orgCode).Trim());
			}
			else
			{
				result.Append(Res.GetString("ded6919e-4630-4d25-92b2-86398e1345b1", "Shipment"));
			}

			return result.ToString();
		}

		static void AppendCGReferenceCustomFieldsDescription(ZStringBuilder entryDescription, IRateLine line, IRateEntry rateEntry)
		{
			var wiseEntry = rateEntry as WiseEntry;
			var cgReference = wiseEntry?.GetCustomFieldValue(Rate.CustomFields.Cargoguide.Reference) as string;

			if (!string.IsNullOrEmpty(cgReference))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("5ae1d005-563f-4494-a0e2-6de2a21bd2f2", "CG Reference:"));
				entryDescription.Append(cgReference);
				entryDescription.AppendLine();
			}
		}

		static void AddDescriptionForNamedAccounts(ZStringBuilder entryDescription, IRateEntry rateEntry)
		{
			if (rateEntry.NamedAccounts.IsNullOrEmpty())
			{
				return;
			}

			AppendDescriptionPrefix(entryDescription, Res.GetString("20677f90-2af4-45e3-aba1-d64df314090c", "Named Accounts:"));
			entryDescription.Append(string.Join(", ", rateEntry.NamedAccounts));
			entryDescription.AppendLine();
		}

		static void AppendPriceByCustomFieldDescription(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var wiseLine = rateLine as WiseLine;
			var wiseRate = rateEntry as WiseEntry;
			if (wiseLine == null || wiseRate == null)
			{
				return;
			}

			var priceBy = wiseLine.GetCustomFieldValue(Rate.CustomFields.CargoSphere.PriceBy)?.ToString();
			var lclUnit = wiseRate.GetCustomFieldValue(Rate.CustomFields.CargoSphere.LclUnit)?.ToString();

			if (!string.IsNullOrEmpty(priceBy) && !string.IsNullOrEmpty(lclUnit))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("1e052dfa-c93f-4143-bd7f-31c537b4a5ab", "CS Price Break:"));
				entryDescription.Append(Res.GetString("9272594b-4f75-4efb-bd84-f21101d6ef6f", "{0} applicable to {1}", priceBy, lclUnit));
				entryDescription.AppendLine();
			}
		}

		static void AppendInlandCustomFieldsDescriptions(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var customCategory = (rateLine as WiseLine)?.CustomCategory;
			var wiseEntry = rateEntry as WiseEntry;
			var customFields = wiseEntry?.CustomFields;

			var inlandRoute = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.InlandRouting);
			if (!string.IsNullOrEmpty(inlandRoute))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("c32630cb-3fc1-431b-b9e6-fe8cc5f12930", "Inland Route:"));
				entryDescription.Append(inlandRoute);
				entryDescription.AppendLine();
			}
		}

		static void AppendOceanCustomFieldsDescriptions(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var customCategory = (rateLine as WiseLine)?.CustomCategory;
			var wiseEntry = rateEntry as WiseEntry;
			var customFields = wiseEntry?.CustomFields;

			var oceanRoute = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.OceanRouting);
			if (!string.IsNullOrEmpty(oceanRoute))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("f1364e6d-ffaa-4483-b179-81b9eb5686f1", "Ocean Route:"));
				entryDescription.Append(oceanRoute);
				entryDescription.AppendLine();
			}
		}

		static void AppendOutlandCustomFieldsDescriptions(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var customCategory = (rateLine as WiseLine)?.CustomCategory;
			var wiseEntry = rateEntry as WiseEntry;
			var customFields = wiseEntry?.CustomFields;

			var outlandRoute = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.OutlandRouting);
			if (!string.IsNullOrEmpty(outlandRoute))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("922a39c4-ffa9-46a9-9d86-ac03626fd168", "Outland Route:"));
				entryDescription.Append(outlandRoute);
				entryDescription.AppendLine();
			}
		}

		static void AppendOriginDescriptions(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var customCategory = (rateLine as WiseLine)?.CustomCategory;
			var wiseEntry = rateEntry as WiseEntry;
			var customFields = wiseEntry?.CustomFields;

			string originLocation;
			if (string.IsNullOrEmpty(customCategory))
			{
				originLocation = rateEntry.Origin()?.Code;
			}
			else
			{
				originLocation = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.Origin);
			}

			if (!string.IsNullOrEmpty(originLocation))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("a3f2628f-1cad-4ad6-bff1-c5701b12f10f", "Origin:"));
				entryDescription.Append(originLocation);
				entryDescription.AppendLine();
			}

			var originTerminal = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.OriginTerminal);
			if (!string.IsNullOrEmpty(originTerminal))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("9f96aa86-eb73-42e7-af02-0389a08ed3a7", "Origin Terminal:"));
				entryDescription.Append(originTerminal);
				entryDescription.AppendLine();
			}

			var originRouting = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.OriginRouting);
			if (!string.IsNullOrEmpty(originRouting))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("248361b1-4e17-4f25-a965-4b43ec802bea", "Origin Routing:"));
				entryDescription.Append(originRouting);
				entryDescription.AppendLine();
			}

			if (rateEntry.OriginZone != null)
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("48540c85-9e90-4272-86ec-b5c89de97e09", "Origin Zone:"));
				entryDescription.Append(rateEntry.OriginZone.TZ_ZoneName);
				entryDescription.AppendLine();
			}
		}

		static void AppendDestinationDescriptions(ZStringBuilder entryDescription, IRateLine rateLine, IRateEntry rateEntry)
		{
			var customCategory = (rateLine as WiseLine)?.CustomCategory;
			var wiseEntry = rateEntry as WiseEntry;
			var customFields = wiseEntry?.CustomFields;

			string destinationLocation;
			if (string.IsNullOrEmpty(customCategory))
			{
				destinationLocation = rateEntry.Destination()?.Code;
			}
			else
			{
				destinationLocation = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.Destination);
			}

			if (!string.IsNullOrEmpty(destinationLocation))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("8a18ed6f-d943-47a7-8f2a-ea0fc558260e", "Destination:"));
				entryDescription.Append(destinationLocation);
				entryDescription.AppendLine();
			}

			var destinationTerminal = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.DestinationTerminal);
			if (!string.IsNullOrEmpty(destinationTerminal))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("decacd3c-93d3-4246-9457-1b4d926ab3c5", "Destination Terminal:"));
				entryDescription.Append(destinationTerminal);
				entryDescription.AppendLine();
			}

			var destinationRouting = GetCargoSphereCustomFieldValue(customFields, customCategory, RatesService.Rate.CustomFields.CargoSphere.DestinationRouting);
			if (!string.IsNullOrEmpty(destinationRouting))
			{
				AppendDescriptionPrefix(entryDescription, Res.GetString("9403525d-13e2-4458-8c52-472e1ae61cb7", "Destination Routing:"));
				entryDescription.Append(destinationRouting);
				entryDescription.AppendLine();
			}
		}

		/// <summary>
		/// Gets a custom field only if it belongs to the provided charge category.
		/// If it does not exist in the provided charge category then it will not
		/// be returned even if it is in the actual CustomFields in the providerCustomFields.
		/// </summary>
		static string GetCargoSphereCustomFieldValue(IEnumerable<CustomField> providerCustomFields, string chargeCategory, string customFieldCode)
		{
			if (string.IsNullOrEmpty(chargeCategory)
				|| !GroupedCustomFieldCodes.TryGetValue(chargeCategory, out var groupedCodes)
				|| !groupedCodes.Contains(customFieldCode))
			{
				return null;
			}

			return (string)providerCustomFields?.FirstOrDefault(x => x.Code == customFieldCode)?.Value;
		}

		static Dictionary<string, List<string>> GroupedCustomFieldCodes => new Dictionary<string, List<string>>
		{
			{
				WRConstants.ChargeCustomCategory.Inland,
				new List<string> { RatesService.Rate.CustomFields.CargoSphere.InlandRouting }
			},
			{
				WRConstants.ChargeCustomCategory.Outland,
				new List<string> { RatesService.Rate.CustomFields.CargoSphere.OutlandRouting }
			},
			{
				WRConstants.ChargeCustomCategory.Ocean,
				new List<string>
				{
					RatesService.Rate.CustomFields.CargoSphere.OceanRouting,
					RatesService.Rate.CustomFields.CargoSphere.Origin, RatesService.Rate.CustomFields.CargoSphere.OriginTerminal, RatesService.Rate.CustomFields.CargoSphere.OriginRouting,
					RatesService.Rate.CustomFields.CargoSphere.Destination, RatesService.Rate.CustomFields.CargoSphere.DestinationTerminal, RatesService.Rate.CustomFields.CargoSphere.DestinationRouting
				}
			},
			{
				WRConstants.ChargeCustomCategory.BOL,
				new List<string>
				{
					RatesService.Rate.CustomFields.CargoSphere.InlandRouting, RatesService.Rate.CustomFields.CargoSphere.OceanRouting, RatesService.Rate.CustomFields.CargoSphere.OutlandRouting,
					RatesService.Rate.CustomFields.CargoSphere.Origin, RatesService.Rate.CustomFields.CargoSphere.OriginTerminal, RatesService.Rate.CustomFields.CargoSphere.OriginRouting,
					RatesService.Rate.CustomFields.CargoSphere.Destination, RatesService.Rate.CustomFields.CargoSphere.DestinationTerminal, RatesService.Rate.CustomFields.CargoSphere.DestinationRouting
				}
			}
		};

		static ZString GetPaymentTerm(IRateLine line, RatingCriteria criteria)
		{
			if (line != null && criteria != null)
			{
				var isCollect = criteria.Cache.IsCollect(line.IsCostRate(), line.ChargeCode.AC_ChargeGroup);
				if (isCollect.HasValue)
				{
					return isCollect.Value ? Res.GetString("b729bf76-7854-4886-aac6-664c1ac1f868", "Collect") : Res.GetString("513bdec5-4d18-4058-84f5-3a08eab6db1a", "Prepaid");
				}
			}

			return ZString.Empty;
		}

		static string GetTariffApplicableOrgCodes(IRateLine rateLine, RatingCriteria criteria)
		{
			var result = new HashSet<ZString>();

			if (rateLine?.ParentRateEntry == null)
			{
				return string.Empty;
			}

			result.UnionWith(
				criteria
					.GetChargedOrgs(rateLine, forCompanyTariff: true)
					.Where(x => rateLine.IsApplicableToOrg(x, criteria) && x.PK != rateLine.ParentRateEntry.ParentRatingHeader.TH_OH)
					.Select(x => x.OH_Code)
			);

			return string.Join(", ", result.OrderBy(x => x).ToArray());
		}

		static void AddInheritanceInfo(ZStringBuilder stringBuilder, RatingCriteria criteria, IRateLine line, ICollection<IRateLine> processedLines, IAutoRatingChargeInfo relatedCharge, string shift = "")
		{
			if (line.UsesCostBasedCalculator() && relatedCharge != null && relatedCharge.IsApportioned)
			{
				var chargeDesc = Res.GetString("e12453d7-5205-4579-afae-11c8b0f4cdc6", "{0}: Apportioned {1} {2}",
					relatedCharge.ChargeCode.AC_Code,
					relatedCharge.CostCurrency.Code,
					relatedCharge.CostAmount.ToString("f2", CultureInfo.CurrentCulture));

				stringBuilder.Append(System.Environment.NewLine);
				stringBuilder.Append(shift);
				stringBuilder.Append(Res.GetString("3af1d25f-0ed8-411c-b342-2abe5f21e12d", "Based On:"));
				stringBuilder.Append(System.Environment.NewLine);
				stringBuilder.Append(shift + "\t" + chargeDesc);
			}
			else
			{
				List<IRateLine> originalLines;
				if (criteria.Cache.CostBasedCalculatorRelatedLines.TryGetValue(RateLine.GetBO(line).PK, out originalLines) && originalLines.Count > 0)
				{
					stringBuilder.Append(System.Environment.NewLine);
					stringBuilder.Append(shift);
					stringBuilder.Append(Res.GetString("3af1d25f-0ed8-411c-b342-2abe5f21e12d", "Based On:"));

					foreach (var originalLine in originalLines)
					{
						if (!processedLines.Contains(originalLine))
						{
							processedLines.Add(originalLine);

							stringBuilder.Append(System.Environment.NewLine);
							stringBuilder.Append(shift + "\t" + GetChargeName(originalLine, criteria));

							if (originalLine.UsesCompanyTariffOrCostBasedCalculator())
							{
								AddInheritanceInfo(stringBuilder, criteria, originalLine, processedLines, relatedCharge, shift + "\t");
							}
						}
					}
				}
			}
		}
	}
}
