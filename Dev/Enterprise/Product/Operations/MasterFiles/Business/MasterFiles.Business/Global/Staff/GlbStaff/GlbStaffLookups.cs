using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffLookups : AutoGlbStaffLookups
	{
		public GlbStaffLookups(AutoGlbStaff parent) : base(parent)
		{
		}

		#region Branches

		public GlbBranchCollection Branches
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.Branches",
					() =>
					{
						return new GlbBranchCollection(Factory);
					});
			}
		}

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.Departments",
					() =>
					{
						return new GlbDepartmentCollection(Factory);
					});
			}
		}

		#endregion

		#region Group Lists

		public GlbGroupCollection CompleteGroupList
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.CompleteGroupList",
					() =>
					{
						return new GlbGroupCollection(Factory) { AddScimFilter = true };
					});
			}
		}

		#endregion

		#region Complete Capabilities List

		public GlbCapabilityCollection CompleteCapabilitiesList
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.CompleteCapabilitiesList",
					() =>
					{
						return new GlbCapabilityCollection(Factory);
					});
			}
		}

		#endregion

		#region Complete Staff List

		public GlbStaffCollection CompleteStaffList
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.CompleteStaffList",
					() =>
					{
						return new GlbStaffCollection(Factory);
					});
			}
		}

		#endregion

		#region SalesTeams

		public SalesTeamCollection CompleteSalesTeamList
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.CompleteSalesTeamList",
					() =>
					{
						return new SalesTeamCollection(Factory);
					});
			}
		}

		#endregion

		public CodeDescriptionPairList Gender
		{
			get
			{
				return Factory.GetCachedValue("GendersList", delegate
				{
					var list = new CodeDescriptionPairList();
					list.AddPair(Core.Constants.Genders.Woman, Core.Constants.GenderDescriptions.Woman);
					list.AddPair(Core.Constants.Genders.Man, Core.Constants.GenderDescriptions.Man);
					list.AddPair(Core.Constants.Genders.Agender, Core.Constants.GenderDescriptions.Agender);
					list.AddPair(Core.Constants.Genders.NonBinary, Core.Constants.GenderDescriptions.NonBinary);
					list.AddPair(Core.Constants.Genders.NotSpecified, Core.Constants.GenderDescriptions.NotSpecified);
					list.AddPair(Core.Constants.Genders.Custom, Core.Constants.GenderDescriptions.Custom);
					return list;
				});
			}
		}

		#region Nationality Types

		public IBusinessObjectCollection NationalityTypes
		{
			get { return new RefCountryCollection(Factory); }
		}

		#endregion

		#region Commission Basis Type

		public CodeDescriptionPairList CommissionBasisTypes
		{
			get { return new CommissionBasisType(); }
		}

		#endregion

		#region Employment Types

		public ICodeDescriptionPairList StaffEmploymentTypes
		{
			get { return SystemDataRegistry.Instance.StaffEmploymentTypes.Value; }
		}

		#endregion

		#region Resource Types

		public ReadOnlyCodeDescriptionPairList ResourceTypes
		{
			get { return SystemDataRegistry.Instance.ResourceTypes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region Relationships

		public ReadOnlyCodeDescriptionPairList Relationships
		{
			get { return SystemDataRegistry.Instance.PersonalRelationshipsList.Value; }
		}

		#endregion

		#region WorkingLanguages

		public CodeDescriptionPairList WorkingLanguages
		{
			get
			{
				return Factory.GetCachedValue("GlbStaffLookups.WorkingLanguages",
					() =>
					{
						return new CodeDescriptionPairList(OLookUpEditType.Language);
					});
			}
		}

		#endregion

		#region StateList

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table");
				var country = ((AutoGlbStaff)Parent).Country;
				var stateList = (country != null) ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion

		#region DomainNames

		public ICodeDescriptionPairList DomainNames => ObjectFactory.Get<IADRegistry>().DomainCredentialsCollectionAsCodeDescriptionPairList;

		#endregion

		#region Activity Tracking Status List

		CodeDescriptionPairList activityTrackingStatusList;
		public CodeDescriptionPairList ActivityTrackingStatusList
		{
			get
			{
				if (activityTrackingStatusList == null)
				{
					activityTrackingStatusList = new CodeDescriptionPairList();
					activityTrackingStatusList.AddPair(ActivityTrackingStatus.Yes, Res.GetString("GlbStaff|ActivityTrackingStatus|Yes", "Tracking enabled for all Companies"));
					activityTrackingStatusList.AddPair(ActivityTrackingStatus.No, Res.GetString("GlbStaff|ActivityTrackingStatus|No", "Tracking not enabled for all Companies"));
					activityTrackingStatusList.AddPair(ActivityTrackingStatus.BasedOnCompany, Res.GetString("GlbStaff|ActivityTrackingStatus|BasedOnCompany", "Use logged in Company default setting"));
				}
				return activityTrackingStatusList;
			}
		}

		#endregion
	}
}
