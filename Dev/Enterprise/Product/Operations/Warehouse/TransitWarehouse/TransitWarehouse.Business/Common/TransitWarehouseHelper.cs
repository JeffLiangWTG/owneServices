using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.Definitions.Customs;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Common
{
	public static class TransitWarehouseHelper
	{
		#region GetAttachedPackagesQuery

		public static ZDBOnlyQuery GetAttachedPackagesQuery(string jobNumber, ZGuid warehousePK)
		{
			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, false);
			query.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_UnitType, PackageStateUnitType.Codes.Overpack);
			query.AddToFilter(WhsItemPackageStateSchema.WPS_WW_Warehouse, warehousePK);

			var additionalReference = GetPackageStateAdditionalReferenceQuery(jobNumber);
			query.AddSubQuery(additionalReference, JoinCondition.And);

			return query;
		}

		public static ZDBOnlyQuery GetViewPackagesQuery(string jobNumber, ZGuid jobPK, ZGuid warehousePK)
		{
			var whsItemReceiveConsignmentSubQuery = new ZDBOnlySubQuery(typeof(WhsItemReceiveConsignment), WhsItemPackageStateSchema.WPS_WRC_TransitReceiveConsignment);
			whsItemReceiveConsignmentSubQuery.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_ParentID, jobPK);
			whsItemReceiveConsignmentSubQuery.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			whsItemReceiveConsignmentSubQuery.AddToFilter(WhsItemReceiveConsignmentSchema.WRC_WW_IntendedWarehouse, warehousePK);

			var whsItemDispatchConsignmentSubQuery = new ZDBOnlySubQuery(typeof(WhsItemDispatchConsignment), WhsItemPackageStateSchema.WPS_WDC_TransitDispatchConsignment);
			whsItemDispatchConsignmentSubQuery.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_ParentID, jobPK);
			whsItemDispatchConsignmentSubQuery.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_ParentTableCode, JobShipmentSchema.Constants.Prefix);
			whsItemDispatchConsignmentSubQuery.AddToFilter(WhsItemDispatchConsignmentSchema.WDC_WW_Warehouse, warehousePK);

			var outerSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), WhsItemPackageStateSchema.WPS_KP_Package);
			outerSubQuery.AddToFilter(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, null);

			var innerParentSubQuery = new ZDBOnlySubQuery(typeof(WhsItemPackageState), WhsItemPackageStateSchema.WPS_KP_Package, PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit);
			innerParentSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_UnitType, new[] { PackageStateUnitType.Codes.HandlingUnit, PackageStateUnitType.Codes.AirULDContainer, PackageStateUnitType.Codes.SeaContainer });
			innerParentSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WW_Warehouse, warehousePK);

			var innerSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, WhsItemPackageStateSchema.WPS_KP_Package);
			innerSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);
			innerSubQuery.AddSubQuery(innerParentSubQuery, JoinCondition.And);

			var packageStateSubQuery = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			packageStateSubQuery.AddSubQuery(outerSubQuery, JoinCondition.And);
			packageStateSubQuery.AddSubQuery(innerSubQuery, JoinCondition.Or);

			var packageStateStatusSubQuery = new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, SQLComparisonOperator.NotEqual, PackageStateUnitType.Codes.HandlingUnit);
			packageStateStatusSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_UnitType, SQLComparisonOperator.NotEqual, PackageStateUnitType.Codes.AirULDContainer);
			packageStateStatusSubQuery.AddToFilter(WhsItemPackageStateSchema.WPS_UnitType, SQLComparisonOperator.NotEqual, PackageStateUnitType.Codes.SeaContainer);

			packageStateSubQuery.AddToFilter(packageStateStatusSubQuery);

			var attachedPackagesQuery = GetAttachedPackagesQuery(jobNumber, warehousePK);
			attachedPackagesQuery.AddSubQuery(whsItemReceiveConsignmentSubQuery, JoinCondition.Or);
			attachedPackagesQuery.AddSubQuery(whsItemDispatchConsignmentSubQuery, JoinCondition.Or);
			attachedPackagesQuery.AddToFilter(packageStateSubQuery);

			return attachedPackagesQuery;
		}

		public static ZDBOnlySubQuery GetPackageStateAdditionalReferenceQuery(string jobNumber)
		{
			var additionalReference = new ZDBOnlySubQuery(typeof(ICusEntryNumber), WhsItemPackageStateSchema.PK, CusEntryNumSchema.CE_ParentID);
			additionalReference.AddToFilter(CusEntryNumSchema.CE_EntryType, WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference);
			additionalReference.AddToFilter(CusEntryNumSchema.CE_EntryNum, jobNumber);
			return additionalReference;
		}

		#endregion

		#region GetTransitWarehouse

		public static WhsWarehouse GetTransitWarehouse(BusinessObjectFactory factory, ZGuid addressPK)
		{
			return GetAllTransitWarehousesForAddressPK(factory, addressPK).FirstOrDefault();
		}

		#endregion

		#region GetAllTransitWarehousesForAddressPK

		public static IEnumerable<WhsWarehouse> GetAllTransitWarehousesForAddressPK(BusinessObjectFactory factory, ZGuid addressPK)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, addressPK);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
			return factory.Load<WhsWarehouse>(query).OrderBy(w => w.WW_WarehouseCode);
		}

		#endregion

		#region GetTransitWarehouseInCurrentBranch

		public static WhsWarehouse GetTransitWarehouseInCurrentBranch(BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_GB_RelatedCompanyBranch, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
			query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
			return factory.LoadTop1<WhsWarehouse>(query);
		}

		#endregion

		#region IsTransitWarehouseInCurrentBranch

		public static bool IsTransitWarehouseInCurrentBranch(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			return GetTransitWarehouseInCurrentBranch(factory)?.PK == warehousePK;
		}

		#endregion

		#region GetPremiseIDFortWarehouse

		public static string GetPremiseIDFortWarehouse(WhsWarehouse warehouse)
		{
			var premiseIDToReturn = ZString.Empty;
			if (warehouse != null)
			{
				// It is possible to have more than one premise ID for the same premise address in different countries (but highly unlikely)
				var premiseIDsForWarehouseAddress = warehouse.WarehouseAddress.CustomsCodes?.GetOrgCusCodesForCodeIgnoringCountry(CustomsAdditionalReferenceTypes.EntryType.Codes.ControlledPremiseID);
				premiseIDToReturn = premiseIDsForWarehouseAddress != null && premiseIDsForWarehouseAddress.Any() ? premiseIDsForWarehouseAddress.First().OK_CustomsRegNo : ZString.Empty;
			}

			return premiseIDToReturn;
		}

		#endregion

		#region CalculateTotalWeight

		public static ZDecimal CalculateTotalWeight(PkgPackage[] packages, ZString targetUQ)
		{
			ZDecimal result = 0m;
			if (Constants.Weight.ContainsCode(targetUQ))
			{
				foreach (var package in packages)
				{
					if (Constants.Weight.ContainsCode(package.KP_WeightUQ))
					{
						result += Constants.Weight.Convert(package.KP_Weight, package.KP_WeightUQ, targetUQ);
					}
				}
			}

			return result;
		}

		#endregion

		#region CalculateTotalVolume

		public static ZDecimal CalculateTotalVolume(ZString targetUQ, PkgPackage[] packages)
		{
			ZDecimal result = 0m;
			if (Constants.Volume.ContainsCode(targetUQ))
			{
				foreach (var package in packages)
				{
					if (Constants.Volume.ContainsCode(package.KP_VolumeUQ))
					{
						result += Constants.Volume.Convert(package.KP_Volume, package.KP_VolumeUQ, targetUQ);
					}
				}
			}

			return result;
		}

		#endregion

		#region LocalClientCompanyName

		public static ZString BillingPartyCompanyName(JobHeader jobHeader, JobDocAddress clientRequestedBillToParty)
		{
			var localClientCompanyName = ZString.Empty;
			if (jobHeader != null)
			{
				localClientCompanyName = jobHeader.LocalCharges?.OH_FullName ?? ZString.Empty;
			}
			else if (clientRequestedBillToParty != null)
			{
				localClientCompanyName = clientRequestedBillToParty.E2_AddressOverride
					? clientRequestedBillToParty.CompanyName
					: clientRequestedBillToParty.Address?.CompanyName ?? ZString.Empty;
			}

			return localClientCompanyName;
		}

		#endregion

		#region AdditionalReferenceNumbersSingleLine

		public static ZString AdditionalReferenceNumbersSingleLine(ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers)
		{
			return string.Join(", ", additionalReferenceNumbers.Cast<ICusEntryNumber>().Select(c => FormatCusEntryNumber(c)));
		}

		static string FormatCusEntryNumber(ICusEntryNumber cusEntryNum)
		{
			var typeSeparator = cusEntryNum.CE_EntryNum.IsEmpty && cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: ": ";

			var numberSeparator = cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: "/";

			return FormattableString.Invariant($"{cusEntryNum.CE_EntryType}{typeSeparator}{cusEntryNum.CE_EntryNum}{numberSeparator}{cusEntryNum.CE_RN_NKCountryCode}");
		}

		#endregion

		#region GetOrgHeaderContact

		public static OrgHeaderContact GetOrgHeaderContact(this JobDocAddress jobDocAddress)
		{
			OrgHeaderContact orgHeaderContact = null;
			if (jobDocAddress != null && !jobDocAddress.E2_AddressOverride && jobDocAddress.Address != null)
			{
				orgHeaderContact = new OrgHeaderContact(jobDocAddress.Organisation, jobDocAddress.Address);
			}

			return orgHeaderContact;
		}

		#endregion

		#region GetNowInCurrentWarehouse

		public static ZDateTimeOffset GetNowInCurrentWarehouse(WhsWarehouse warehouse)
		{
			var result = ZDateTimeOffset.Now;
			if (warehouse != null)
			{
				var code = warehouse.RelatedCompanyBranch.HomePort.Code;
				var utc = ZDateTime.UtcNow.ToDateTime();
				var now = Env.Time.GetUnlocoTimeFromUtc(code, utc);
				var offset = Env.Time.GetUtcOffsetBasedOnUtc(code, utc);
				result = new ZDateTimeOffset(now, offset);
			}

			return result;
		}

		#endregion

		#region GetFormattedReferenceString

		public static ZString GetFormattedReferenceString(string description, string code)
		{
			if (!string.IsNullOrEmpty(description))
			{
				if (!string.IsNullOrEmpty(code))
				{
					return $"{description} ({code})"; // This is a format string only contains symbols
				}
				else
				{
					return description;
				}
			}
			else if (!string.IsNullOrEmpty(code))
			{
				return $"({code})"; // This is a format string only contains symbols
			}
			return "";
		}

		#endregion

		#region GetMatchingOrg

		public static IEnumerable<OrgAddress> GetMatchingOrgAddresses(BusinessObject parentBO, DataContextType context)
		{
			if (parentBO != null)
			{
				var query = new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, parentBO.PK);
				query.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, context.ToString());
				var factory = parentBO.Factory;
				var links = factory.Load<IStmUniversalJobLink>(query);
				var orgPks = links.Select(b => ((BusinessObject)b)[StmUniversalJobLinkSchema.UCL_OH_Owner]).Cast<ZGuid>();
				return orgPks.Select(o => o.IsEmpty ? GlbCompany.CurrentCompany.OrgProxy.MainAddress : factory.Load<OrgHeader>(o).MainAddress);
			}

			return Enumerable.Empty<OrgAddress>();
		}

		#endregion

		#region GetFreightModeByTransportMode

		public static FreightMode GetFreightModeByTransportMode(string transportMode, bool isContainer = false)
		{
			var freightMode = FreightMode.UKN;
			switch (transportMode)
			{
				case "AIR":
				case "FAS":
					freightMode = isContainer ? FreightMode.ULD : FreightMode.AIR;
					break;
				case "SEA":
				case "FSA":
					freightMode = isContainer ? FreightMode.FCL : FreightMode.SEA;
					break;
				case "ROA":
					freightMode = isContainer ? FreightMode.FRO : FreightMode.ROA;
					break;
				case "RAI":
					freightMode = isContainer ? FreightMode.FRA : FreightMode.RAI;
					break;
			}

			return freightMode;
		}

		#endregion

		#region GetWeightInKG

		public static ZDecimal GetWeightInKG(TransitPackageForRatingInfo transitPackage)
		{
			return Constants.Weight.Convert(transitPackage.Weight, transitPackage.WeightUQ, Constants.Weight.Kilograms);
		}

		#endregion

		#region GetVolumeInM3

		public static ZDecimal GetVolumeInM3(TransitPackageForRatingInfo transitPackage)
		{
			return Constants.Volume.Convert(transitPackage.Volume, transitPackage.VolumeUQ, Constants.Volume.CubicMetres);
		}

		#endregion

		#region GetTransportCompanyOrganisation

		public static OrgHeader GetTransportCompanyOrganisation(JobDocAddress transportCompanyAddress) => (!transportCompanyAddress?.E2_AddressOverride ?? false) ? transportCompanyAddress.Organisation : null;

		#endregion

		#region GetAttachedJobNumber

		public static ZString GetAttachedJobNumber(PkgPackage package)
		{
			var query = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, package.PK);
			var packageState = package.Factory.LoadTop1<WhsItemPackageState>(query);
			return packageState?.AdditionalReferenceNumbers?.GetFirstReferenceNumberByType(AdditionalReferenceTypes.Codes.BookingPartyReference)?.CE_EntryNum ?? string.Empty;
		}

		#endregion

		#region PopulateAddOnValue

		public static void PopulateAddOnValue(this BusinessObject parent, string name, string type, ZString value)
		{
			if (parent != null)
			{
				var addOnValue = parent.Factory.New<GenCustomAddOnValue>();
				addOnValue.XV_ParentID = parent.PK;
				addOnValue.XV_ParentTableCode = parent.TablePrefix;
				addOnValue.XV_Name = name;
				addOnValue.XV_Type = type;
				addOnValue.XV_Data = value;
			}
		}

		#endregion

		#region GetAddOnValues

		public static List<GenCustomAddOnValue> GetAddOnValues(this BusinessObject parent)
		{
			return parent.Factory.Load<GenCustomAddOnValue>(new ZQuery(GenCustomAddOnValueSchema.XV_ParentID, parent.PK)).ToList();
		}

		public static List<GenCustomAddOnValue> GetAddOnValues(this BusinessObject parent, Func<GenCustomAddOnValue, bool> predicate)
		{
			return parent.GetAddOnValues().Where(predicate).ToList();
		}

		#endregion

		#region CanSendCRESAMessage

		static bool CheckIsCreatedBySystemUser(string createUserCode) => new List<string> { User.ServiceUserCode, User.SupportUserCode, User.UnKnownUserCode, User.InterchangeUserCode, User.WebUserCode }.Contains(createUserCode);

		public static bool CanSendCRESAMessage(BusinessObject consignment)
		{
			var referenceNumbers = consignment.Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, consignment.PK)).ToList();
			return !referenceNumbers.Any(r => (r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority || r.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport) && !CheckIsCreatedBySystemUser(r.CE_SystemCreateUser));
		}

		#endregion
	}
}
