using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Module
{
	public class eManifestShipmentFilterStrip : FilterStripBusinessObject
	{
		#region Descriptions

		internal static class Descriptions
		{
			internal static MultilingualString ShipmentTypeMultilingualDescription
			{
				get { return ResString.GetMultilingualString("164084cd-2386-4f83-94ed-2a8f72170090", "Shipment Type"); }
			}
			internal const string ShipmentTypeFilterId = "Shipment Type";

			internal static MultilingualString ShipmentControlNumberMultilingualDescription
			{
				get { return ResString.GetMultilingualString("bdfc8fac-2d18-49b2-a0c4-753df625930e", "Shipment Control Number"); }
			}
			internal const string ShipmentControlNumberFilterId = "Shipment Control Number";

			internal static MultilingualString PortOfLadingMultilingualDescription
			{
				get { return ResString.GetMultilingualString("20da7c5d-b43d-4a4e-8e8f-5b2a578af37f", "Port of Loading"); }
			}
			internal const string PortOfLadingFilterId = "Port of Loading";

			internal static MultilingualString PortOfLadingKCodeMultilingualDescription
			{
				get { return ResString.GetMultilingualString("fbc37110-b4a9-4871-b9a6-2cffbc9212af", "Port Of Lading (Schedule K)"); }
			}
			internal const string PortOfLadingKCodeFilterId = "Port Of Lading (Schedule K)";

			internal static MultilingualString ShipperConsigneeMultilingualDescription
			{
				get { return ResString.GetMultilingualString("0f043846-ce15-4e29-853d-c2e82717a37e", "Shipper/Consignee"); }
			}

			internal const string ShipperConsigneeFilterId = "Shipper/Consignee";
		}

		#endregion

		#region Overrides of FilterStripBusinessObject

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();

			#region Trip Reference

			collection.AddFilter(
				new NoBlankModuleGuidFilter(
					eManifestFilterStrip.Descriptions.TripReferenceFilterId,
					eManifestFilterStrip.Descriptions.TripReferenceMultilingualDescription,
					FilterCategories.NumbersAndReferences,
					ModuleIDs.Customs.US.eManifest,
					CusInBondBillSchema.B0_BH,
					() => eManifestModule.GetNewGridCollection(Factory)));

			#endregion

			#region Shipment Control Number

			eManifestFilterStrip.AddNumberFilter(
				collection,
				Descriptions.ShipmentControlNumberFilterId,
				Descriptions.ShipmentControlNumberMultilingualDescription,
				CusInBondBillSchema.B0_MasterBillNumber);

			#endregion

			#region Shipment Type / Release Status

			eManifestFilterStrip.AddStatusFilter(
				collection,
				Descriptions.ShipmentTypeFilterId,
				Descriptions.ShipmentTypeMultilingualDescription,
				CusInBondBillSchema.B0_ShipmentType,
				() => Lookups.ShipmentTypes);

			eManifestFilterStrip.AddStatusFilter(
				collection,
				eManifestFilterStrip.Descriptions.ReleaseStatusFilterId,
				eManifestFilterStrip.Descriptions.ReleaseStatusMultilingualDescription,
				CusInBondBillSchema.B0_ReleaseStatus,
				() => Lookups.ReleaseStatusList);

			#endregion

			#region Port Of Lading

			eManifestFilterStrip.AddLocation(
				collection,
				Descriptions.PortOfLadingFilterId,
				Descriptions.PortOfLadingMultilingualDescription,
				CusInBondBillSchema.B0_RL_NKPortOfLading,
				ModuleIDs.RefUNLOCO,
				RefUNLOCOSchema.RL_Code,
				Lookups.PortOfLadings);

			eManifestFilterStrip.AddLocation(
				collection,
				Descriptions.PortOfLadingKCodeFilterId,
				Descriptions.PortOfLadingKCodeMultilingualDescription,
				CusInBondBillSchema.B0_PortOfLadingKCode,
				ModuleIDs.Customs.Universal.ZZRefCusCodeList,
				ZZRefCusCodeListCombinedSchema.ZZD_Code,
				Lookups.ScheduleKPortCodes);

			#endregion

			#region Shipper/Consignee

			var shipperConsigneeFilter = collection.AddGuidFilter(
				Descriptions.ShipperConsigneeFilterId,
				ModuleIDs.Organisation,
				GetShipperConsigneeQuery,
				Lookups.Organizations,
				Lookups.Organizations);

			shipperConsigneeFilter.SetItemDescriptions(
				eManifestShipmentFilterStripControl.Captions.Shipper,
				eManifestShipmentFilterStripControl.Captions.Consignee);

			shipperConsigneeFilter.Category = FilterCategories.Organisations;
			shipperConsigneeFilter.MultilingualDescription = Descriptions.ShipperConsigneeMultilingualDescription;

			#endregion

			return collection;
		}

		#region Implementation

		ZQuery GetShipperConsigneeQuery(ZGuid shipper, ZGuid consignee)
		{
			var query = new ZDBOnlyQuery(typeof(Shipment));
			if (!shipper.IsEmpty)
			{
				query.AddSubQuery(GetPartySubQuery(shipper, PartyTypes.Codes.Shipper), JoinCondition.And);
			}
			if (!consignee.IsEmpty)
			{
				query.AddSubQuery(GetPartySubQuery(consignee, PartyTypes.Codes.Consignee), JoinCondition.And);
			}
			return query;
		}

		static ZDBOnlySubQuery GetPartySubQuery(ZGuid partyPk, string type)
		{
			var partySubQuery = new ZDBOnlySubQuery(typeof(Party), JobDocAddressSchema.E2_ParentID);
			partySubQuery.AddToFilter(JobDocAddressSchema.E2_ParentTableCode, CusInBondBillSchema.Constants.Prefix);
			partySubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, type);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, partyPk);
			partySubQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressSubQuery, JoinCondition.And);
			return partySubQuery;
		}

		ShipmentLookups Lookups
		{
			get { return lookups ?? (lookups = Factory.GetNull<Shipment>().Lookups); }
		}

		ShipmentLookups lookups;

		#endregion

		#region Module Filters

		public override ZQuery Filter
		{
			get
			{
				var result = base.Filter;
				result.AddToFilter(GetCompanyQuery());
				return result;
			}
		}

		ZQuery GetCompanyQuery()
		{
			var releaseStatuses = new[] { string.Empty, ShipmentEntryStatusList.Codes.Error, ShipmentEntryStatusList.Codes.Cancelled, ShipmentEntryStatusList.Codes.Accepted };
			var query = new ZDBOnlyQuery(typeof(Shipment));
			var subQuery = new ZDBOnlySubQuery(typeof(Trip), CusInBondHeaderSchema.PK);

			var refBranch = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			refBranch.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			subQuery.AddSubQuery(CusInBondHeaderSchema.BH_GB, refBranch, JoinCondition.And);
			subQuery.AddToFilter(CusInBondHeaderSchema.BH_ApplicationCode, CusInBondApplicationCodeList.Codes.eManifest);

			query.AddSubQuery(CusInBondBillSchema.B0_BH, subQuery, JoinCondition.And);
			query.AddToFilter(CusInBondBillSchema.B0_ReleaseStatus, releaseStatuses);

			return query;
		}

		internal class NoBlankModuleGuidFilter : ModuleGuidFilter
		{
			internal NoBlankModuleGuidFilter(string filterName, MultilingualString description, FilterCategory category, ModuleIdentifier id, SchemaGuidColumn filterColumn, GetList listDelegate)
				: base(filterName, id, filterColumn, listDelegate)
			{
				MultilingualDescription = description;
				Category = category;
			}

			public override IReadOnlyList<string> AllowedComparisonOperators
			{
				get { return new[] { string.Empty, ComparisonConstants.Exact, ComparisonConstants.NotEqual }; }
			}
		}

		#endregion

		#endregion
	}
}
