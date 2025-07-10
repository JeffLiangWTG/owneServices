using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbBranchLookups : AutoGlbBranchLookups
	{
		public GlbBranchLookups(AutoGlbBranch parent) : base(parent)
		{
		}

		#region OrgProxies

		public new ForwarderOrBrokerOrCarrierOrServicesCollection OrgProxies
		{
			get { return new ForwarderOrBrokerOrCarrierOrServicesCollection(Factory); }
		}

		#endregion

		public GlbDepartmentCollection AllowedDepartmentsExcludeSelected
		{
			get
			{
				var allowedDepartments = Factory.GetCachedValue("GlbBranchLookups.AllDepartments", () => new GlbDepartmentCollection(Factory));

				var selectedDepartments = (Parent as GlbBranch).AllowedDepartments;
				if (selectedDepartments.Count != 0)
				{
					var selectedDepartmentsPK = selectedDepartments.Select(bizObj => bizObj.AAB_GE_Department).ToArray();
					allowedDepartments.AdditionalFilter = new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, selectedDepartmentsPK);
				}
				else
				{
					allowedDepartments.AdditionalFilter = new ZQuery();
				}

				allowedDepartments.AddNotificationWhenAdditionalFilterNotMetOverride = (errors, bizObj) =>
				{
					errors.Add(Res.GetString("c0be4a01-a31f-4c9f-90f9-7f09f1b1c472", "This department has already been attached."));
				};

				return allowedDepartments;
			}
		}

		public CodeDescriptionPairList AccountingGroupCodes
		{
			get
			{
				var companyPK = ((GlbBranch)Parent).GB_GC.IsValid ? ((GlbBranch)Parent).GB_GC.ToGuid() : Guid.Empty;
				return AccountingMasterFilesRegistry.Instance.BranchManagementCodes.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
			}
		}

		#region StateList

		public CodeDescriptionPairList StateList
		{
			get
			{
				var result = new UntranslatableCodeDescriptionPairList((NoResString)"States from RefCountryState table");
				var country = ((GlbBranch)Parent).BaseCountry;
				var stateList = (country != null) ? new OrgCodeLists().State_List(country) : new CodeDescriptionPairList();
				result.AddRange(stateList);

				return result;
			}
		}

		#endregion
	}
}
