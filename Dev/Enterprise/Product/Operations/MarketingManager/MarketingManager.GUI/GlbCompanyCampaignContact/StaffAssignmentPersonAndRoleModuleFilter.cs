using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public delegate ZQuery GetStaffAssignmentsQuery(ZQuery orgStaffAssignmentFilter);

	public class StaffAssignmentPersonAndRoleModuleFilter : ModuleTextFilter
	{
		public StaffAssignmentPersonAndRoleModuleFilter(ZString description, GlbCompanyCampaignContactFilterBusinessObject filterStripbusinessObject)
			: base(description, EmptyQuery, StaffRoles)
		{
			this.staffAssignmentQueryDelegate = (q) => q;
		}

		public StaffAssignmentPersonAndRoleModuleFilter(ZString description, GetStaffAssignmentsQuery queryDelegate)
			: base(description, EmptyQuery, StaffRoles)
		{
			if (queryDelegate == null)
			{
				throw new ArgumentNullException(nameof(queryDelegate));
			}

			this.staffAssignmentQueryDelegate = queryDelegate;
		}

		#region ComparisonOperator

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new string[] {
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.IsBlank
				};
			}
		}

		#endregion

		#region Properties

		#region Staff Assignment Person

		[List("StaffMembers")]
		public ZString StaffAssignmentPerson
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank)
				{
					return GetEmptyPropertyValue();
				}
				return staffAssignmentPerson;
			}
			set
			{
				if (SetNonPersistentPropertyValue(StaffAssignmentPersonInfo, ref staffAssignmentPerson, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateStaffAssignmentPerson();
					}
					StaffAssignmentPersonInfo.RefreshBinding();
				}
			}
		}
		ZString staffAssignmentPerson;

		public ZPropertyInfo StaffAssignmentPersonInfo
		{
			get { return GetZPropertyInfo(nameof(StaffAssignmentPerson)); }
		}

		protected bool StaffAssignmentPerson_ReadOnly
		{
			get { return ComparisonOperator == ComparisonConstants.IsBlank; }
		}

		public GlbStaffCollection StaffMembers
		{
			get { return new GlbStaffCollection(new BusinessObjectFactory()); }
		}

		#endregion

		#region Staff Assignment Role

		[BusinessObjectTestExclude]
		[List("StaffRolesList")]
		public ZString StaffAssignmentRole
		{
			get
			{
				return staffAssignmentRole;
			}
			set
			{
				if (SetNonPersistentPropertyValue(StaffAssignmentRoleInfo, ref staffAssignmentRole, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateStaffAssignmentRole();
					}

					StaffAssignmentRoleInfo.RefreshBinding();
				}
			}
		}
		ZString staffAssignmentRole;

		public ZPropertyInfo StaffAssignmentRoleInfo
		{
			get { return GetZPropertyInfo(nameof(StaffAssignmentRole)); }
		}

		public ReadOnlyCodeDescriptionPairList StaffRolesList
		{
			get { return staffRolesList ?? (staffRolesList = CreateStaffRolesList()); }
		}
		ReadOnlyCodeDescriptionPairList staffRolesList;

		public static ReadOnlyCodeDescriptionPairList CreateStaffRolesList()
		{
			return DataRegistry.Instance.OrgStaffMemberAssignmentRoles;
		}

		static ReadOnlyCodeDescriptionPairList StaffRoles
		{
			get { return CreateStaffRolesList(); }
		}

		#endregion

		#endregion

		#region Validation

		public new StaffAssignmentPersonAndRoleModuleFilterValidation Validation
		{
			get { return (StaffAssignmentPersonAndRoleModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new StaffAssignmentPersonAndRoleModuleFilterValidation(this);
		}

		#endregion

		#region Query

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : staffAssignmentQueryDelegate(GetStaffAssignmentPersonAndRoleFilter());
		}

		ZQuery GetStaffAssignmentPersonAndRoleFilter()
		{
			SQLComparisonOperator comparisonOperator = SqlComparisonOperator;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);

			bool allPersonItemsNotIn = false;
			comparisonOperator = ComparisonOperator == ComparisonConstants.IsBlank ? SQLComparisonOperator.NotEqual : comparisonOperator;

			ZDBOnlySubQuery staffAssignmentsSubQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, comparisonOperator.IsNegativeSQLOperator());
			if (!StaffAssignmentPerson.IsEmpty)
			{
				staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), StaffAssignmentPerson);
			}

			if (!StaffAssignmentRole.IsEmpty)
			{
				staffAssignmentsSubQuery.AddToFilter(OrgStaffAssignmentsSchema.O8_Role, comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery(), StaffAssignmentRole);
			}

			if (comparisonOperator != SQLComparisonOperator.NotEqual)
			{
				allPersonItemsNotIn = false;
			}

			orgSubQuery.AddSubQuery(staffAssignmentsSubQuery, JoinCondition.And);

			if (allPersonItemsNotIn)
			{
				ZDBOnlySubQuery notInStaffAssignmentsQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH, true);
				orgSubQuery.AddSubQuery(notInStaffAssignmentsQuery, JoinCondition.Or);
			}

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("StaffAssignmentPerson", StaffAssignmentPerson);
			writer.WriteElementString("StaffAssignmentRole", StaffAssignmentRole);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "StaffAssignmentPerson")
			{
				StaffAssignmentPerson = reader.ReadElementString("StaffAssignmentPerson");
			}

			if (reader.Name == "StaffAssignmentRole")
			{
				StaffAssignmentRole = reader.ReadElementString("StaffAssignmentRole");
			}
		}

		#endregion

		#region Implementation

		protected override bool IsEmptyCore => base.IsEmptyCore && StaffAssignmentRole.IsEmpty && StaffAssignmentPerson.IsEmpty && ComparisonOperator != ComparisonConstants.IsBlank;

		protected override void ClearCore()
		{
			base.ClearCore();
			StaffAssignmentRole = ZString.Empty;
			StaffAssignmentPerson = ZString.Empty;
		}

		readonly GetStaffAssignmentsQuery staffAssignmentQueryDelegate;

		#endregion
	}
}
