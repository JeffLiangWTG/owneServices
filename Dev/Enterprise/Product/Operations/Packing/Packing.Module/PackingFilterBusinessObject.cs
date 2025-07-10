using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Module
{
	public class PackingFilterStripBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var packageIdFilter = filters.AddTextFilter("Package ID/Container #", PackageIDQuery);
			packageIdFilter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;
			packageIdFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PackingFilter|PackageIDContainerNo", "Package ID/Container #");
			packageIdFilter.Category = FilterCategories.NumbersAndReferences;

			var parentJobNoFilter = filters.AddNumberFilter("Parent Job Ref #", ViewPkgPackageJobParentsSchema.VI_JobNumber);
			parentJobNoFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PackingFilter|ParentJobNo", "Parent Job Ref #");
			parentJobNoFilter.Category = FilterCategories.NumbersAndReferences;
			parentJobNoFilter.SubGroup = ParentsSubGroup;

			var parentJobStatus = filters.AddTextFilter("Parent Job Status", ParentJobStatusQuery, ParentJobStatuses);
			parentJobStatus.DefaultProperty = ParentJobStatusCodes.Open;
			parentJobStatus.Visibility = FilterVisibility.AlwaysApplied; // show only OPEN jobs by default
			parentJobStatus.MultilingualDescription = ResString.GetMultilingualString("Packing|PackingFilter|ParentJobStatus", "Parent Job Status");
			parentJobStatus.Category = FilterCategories.StatusAndFlags;
			parentJobStatus.SubGroup = ParentsSubGroup;

			var parentJobTypeFilter = filters.AddTextFilter("Parent Job Type", ParentJobTypeQuery, ParentJobTypes);
			parentJobTypeFilter.MultilingualDescription = ResString.GetMultilingualString("Packing|PackingFilter|ParentJobType", "Parent Job Type");
			parentJobTypeFilter.Category = FilterCategories.ModesAndTypes;
			parentJobTypeFilter.SubGroup = ParentsSubGroup;

			return filters;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter("Packing Job ID", PkgPackageJobSchema.KJ_JobID, "P")
			{
				MultilingualDescription = ResString.GetMultilingualString("E23423A9-EB3D-497B-9D76-FCFC571F7A5A", "Packing Job ID")
			};
		}

		#region ParentsSubGroup

		ModuleFilterSubGroup ParentsSubGroup
		{
			get { return parentsSubGroup ?? (parentsSubGroup = new ParentsFilterSubGroup()); }
		}

		ModuleFilterSubGroup parentsSubGroup;

		class ParentsFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var viewFilter = new ZDBOnlySubQuery(typeof(AutoViewPkgPackageJobParents), ViewPkgPackageJobParentsSchema.PK);
				viewFilter.AddToFilter(filter);

				var result = new ZDBOnlyQuery(typeof(PkgPackageJob));
				result.AddSubQuery(PkgPackageJobSchema.KJ_ParentID, viewFilter, JoinCondition.And);

				return result;
			}
		}

		#endregion

		#endregion

		#region Queries

		ZQuery PackageIDQuery(SQLComparisonOperator comparisonOperator, ZString packageID)
		{
			var packageIDSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIDSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, comparisonOperator, packageID);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageSubQuery.AddSubQuery(packageIDSubQuery, JoinCondition.And);

			var outerQuery = new ZDBOnlyQuery(typeof(PkgPackageJob));
			outerQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			return outerQuery;
		}

		ZQuery ParentJobStatusQuery(ZString jobStatus)
		{
			ZQuery result;

			if (jobStatus.EqualsIgnoringCase(ParentJobStatusCodes.Completed))
			{
				result = new ZQuery(ViewPkgPackageJobParentsSchema.VI_IsComplete, true);
			}
			else if (jobStatus.EqualsIgnoringCase(ParentJobStatusCodes.Open))
			{
				result = new ZQuery(ViewPkgPackageJobParentsSchema.VI_IsComplete, false);
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		ZQuery ParentJobTypeQuery(ZString jobType)
		{
			return new ZQuery(ViewPkgPackageJobParentsSchema.VI_JobType, jobType);
		}

		#endregion

		#region Lists

		#region ParentJobStatuses

		static class ParentJobStatusCodes
		{
			public const string Open = "OPN";
			public const string Completed = "CLS";
			public const string All = "ALL";
		}

		public CodeDescriptionPairList ParentJobStatuses
		{
			get
			{
				return Factory.GetCachedValue("6BD485DB-460D-473E-BD30-A017E8670348",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(ParentJobStatusCodes.Open, Res.GetString("f9ad12c1-e935-4384-90b4-cb03e9ab5b42", "Open Jobs"));
						result.AddPair(ParentJobStatusCodes.Completed, Res.GetString("92a3f332-5eb0-4e9c-9a38-b1efbdfc437a", "Completed Jobs"));
						result.AddPair(ParentJobStatusCodes.All, Res.GetString("c04ca9c9-aa13-4ad8-b062-68f6173145dc", "All Jobs"));

						return result;
					});
			}
		}

		#endregion

		#region ParentJobTypes

		public CodeDescriptionPairList ParentJobTypes
		{
			get
			{
				return Factory.GetCachedValue("B824B87C-C059-4634-9A51-62024062B3A5",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair("ASH", Res.GetString("fb47b56f-8e1c-445b-8d38-2170591daf75", "Transport Bookings for Bill of Lading"));
						result.AddPair("SHP", Res.GetString("20192052-21DE-4A9E-8D68-32B66720701B", "Transport Bookings for Shipment"));
						result.AddPair("BKG", Res.GetString("4e83d713-ba09-4209-a7d9-01522c6840f2", "Transport Bookings - Standalone"));
						result.AddPair("CSN", Res.GetString("e8039c95-ed11-411d-9adb-588216adc800", "Transport Consignments"));
						result.AddPair("ORD", Res.GetString("927fb7c8-4e1b-48a2-bb1a-ae2e1f9ef1f2", "Warehouse Order"));
						result.AddPair("WHR", Res.GetString("79665229-5530-4d65-83a3-18f5c504622e", "Warehouse Receipt"));

						return result;
					});
			}
		}

		#endregion

		#endregion

		#region FetchStrategy

		// TODO: Implement module load fetch strategy.
		//public override IBusinessObjectFetchStrategy FetchStrategy
		//{
		//    get { return base.FetchStrategy; }
		//}

		#endregion
	}
}
