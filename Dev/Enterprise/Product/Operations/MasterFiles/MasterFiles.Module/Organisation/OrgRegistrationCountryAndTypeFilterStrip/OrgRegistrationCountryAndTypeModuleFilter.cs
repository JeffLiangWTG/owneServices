using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public delegate void UpdateDependentList(IList list, ZString property);
	public delegate CodeDescriptionPairList GetDefaultDependentList();

	public class OrgRegistrationCountryAndTypeModuleFilter : ModuleCodeFilter
	{
		#region Construction

		protected OrgRegistrationCountryAndTypeModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection, bool notIn = false)
			: base(category, parentCollection)
		{
			InitializeSubgroup(notIn);
		}

		public OrgRegistrationCountryAndTypeModuleFilter(ZString description, GetCodeQuery queryDelegate, IBusinessObjectCollection findBoxList, IList dropEditList, UpdateDependentList updateDropListDelegate, GetDefaultDependentList getDefaultDropListDelegate, bool notIn)
			: base(description, queryDelegate, findBoxList, dropEditList)
		{
			QueryDelegate = queryDelegate;
			this.updateDropListDelegate = updateDropListDelegate;
			this.getDefaultDropListDelegate = getDefaultDropListDelegate;
			EnsureFindBoxListIsCountryCollection(findBoxList);
			InitializeSubgroup(notIn);
		}

		void InitializeSubgroup(bool notIn)
		{
			if (notIn)
			{
				this.SubGroup = new OK_OHNotInSubGroup();
			}
			else
			{
				this.SubGroup = new OK_OHSubGroup();
			}
			this.notIn = notIn;
		}

		bool notIn;

		void EnsureFindBoxListIsCountryCollection(IBusinessObjectCollection countryList)
		{
			Argument.NotNull(countryList, "countryList");
			if (!countryList.GetType().IsAssignableFrom(typeof(RefCountryCollection)))
			{
				throw new ArgumentException("countryList is not a RefCountryCollection.");
			}
		}

		#endregion

		class OK_OHNotInSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery orgCusCodeSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH, true);
				orgCusCodeSubQuery.AddToFilter(filter);
				result.AddSubQuery(orgCusCodeSubQuery, JoinCondition.And);
				return result;
			}
		}

		class OK_OHSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				subQuery.AddToFilter(filter);

				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		#region GetNewCommonModuleFilter

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new OrgRegistrationCountryAndTypeModuleFilter(category, parentCollection);
		}

		#endregion

		#region Default Category

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Locations; }
		}

		#endregion

		#region Properties

		public override ZString Property1
		{
			get { return base.Property1; }
			set
			{
				base.Property1 = value;
				if (updateDropListDelegate != null)
				{
					updateDropListDelegate(List2, value);
				}
			}
		}

		protected override ModuleFilter ShallowCloneCore()
		{
			return new OrgRegistrationCountryAndTypeModuleFilter(Description, QueryDelegate as GetCodeQuery, (IBusinessObjectCollection)List1, getDefaultDropListDelegate(), updateDropListDelegate, getDefaultDropListDelegate, notIn);
		}

		readonly GetDefaultDependentList getDefaultDropListDelegate;
		readonly UpdateDependentList updateDropListDelegate;

		#endregion
	}
}
