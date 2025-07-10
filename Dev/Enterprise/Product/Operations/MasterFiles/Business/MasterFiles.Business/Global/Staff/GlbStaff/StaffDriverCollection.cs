using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.TransportCommon.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StaffDriverCollection : GlbStaffCollection
	{
		public StaffDriverCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			SetFilterBusinessObjectDefaults();
		}

		public StaffDriverCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			SetFilterBusinessObjectDefaults();
		}

		#region CreateDriverFilter

		ZQuery CreateDriverFilter()
		{
			return GetDriversQuery(Factory);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public static ZQuery GetDriversQuery(BusinessObjectFactory factory)
		{
			ZQuery result;
			var branchQuery = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			var branches = factory.Load<GlbBranch>(branchQuery);
			if (branches.Any())
			{
				var allDriversQuery = new ZDBOnlyQuery(typeof(GlbStaff));
				foreach (GlbBranch branch in branches)
				{
					var driverQuery = GetDriversQuery(factory, branch);
					if (!driverQuery.IsNoResultQuery)
					{
						allDriversQuery.AddToFilter(driverQuery, JoinCondition.Or);
					}
				}
				result = allDriversQuery;
			}
			else
			{
				result = ZQuery.NoResultQuery;
			}

			return result;
		}

		public static ZQuery GetDriversQuery(BusinessObjectFactory factory, ZString branchCode)
		{
			var branchQuery = new ZQuery(GlbBranchSchema.GB_Code, branchCode);
			var branch = factory.LoadTop1<GlbBranch>(branchQuery);
			return GetDriversQuery(factory, branch);
		}

		static ZQuery GetDriversQuery(BusinessObjectFactory factory, GlbBranch branch)
		{
			ZQuery result;

			var driversGroup = branch != null ? GetDriverGroup(factory, branch) : null;
			if (driversGroup != null)
			{
				var groupSubQuery = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupSchema.PK);
				groupSubQuery.AddToFilter(GlbGroupSchema.PK, driversGroup.PK);

				var groupLinkQuery = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS);
				groupLinkQuery.AddSubQuery(GlbGroupLinkSchema.GK_GG, groupSubQuery, JoinCondition.And);

				var staffQuery = new ZDBOnlyQuery(typeof(GlbStaff));
				staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
				staffQuery.AddSubQuery(groupLinkQuery, JoinCondition.And);

				result = staffQuery;
			}
			else
			{
				result = ZQuery.NoResultQuery;
			}

			return result;
		}

		static GlbGroup GetDriverGroup(BusinessObjectFactory factory, GlbBranch branch)
		{
			var transportRegistry = ObjectFactory.Get<ITransportRegistry>();
			var driversGroupPK = new ZGuid(transportRegistry.TransportDriversGroup.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty));
			return factory.Load<GlbGroup>(driversGroupPK);
		}

		protected void SetFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(DriverBranch, "Property", new ZString(Env.CurrentBranch.Code), isRemovable: true));
			AdditionalFilter = CreateDriverFilter();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name")]
		public const string DriverBranch = "Driver Branch";

		#region Overrides

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			ZQuery isDriverQuery = CreateDriverFilter();
			if (!selectedBusinessObject.MatchesFilter(isDriverQuery))
			{
				errors.Add(Res.GetString("9b963f4a-8cc1-43ff-8d4a-7809f59b15d1", "A Staff member selected from here must be a member of the 'Registry->Port Transport->Port Transport Drivers' group."));
			}
		}

		#endregion

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}
	}
}
