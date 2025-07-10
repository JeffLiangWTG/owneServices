using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public delegate ZQuery GetClientAssignedStaffQuery(ZDBOnlySubQuery orgHeaderFilter);

	public class OrgClientAssignedStaffModuleFilter : ModuleTextFilter
	{
		#region Construction

		public OrgClientAssignedStaffModuleFilter(ZString description, GetClientAssignedStaffQuery queryDelegate = null, IList list = null)
			: base(description, EmptyQuery, list ?? ClientTypes)
		{
			clientAssignedStaffQueryDelegate = queryDelegate ?? (q => q);
		}

		#endregion

		#region Comparison Operator

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				var operators = new List<string>
				{
					string.Empty,
					ComparisonConstants.Exact,
					ComparisonConstants.NotEqual,
					ComparisonConstants.CurrentUser
				};

				if (SupportsBlankComparisonOperators)
				{
					operators.Add(ComparisonConstants.IsBlank);
					operators.Add(ComparisonConstants.IsNotBlank);
				}

				if (SupportsFiltersMatchComparisonOperator)
				{
					operators.Add(ComparisonConstants.FiltersMatch);
				}

				return operators;
			}
		}

		protected override SQLComparisonOperator DefaultSqlComparisonOperator
		{
			get { return SQLComparisonOperator.Equal; }
		}

		public override bool HasComparisonOperator => true;

		protected override void OnComparisonOperatorChanged()
		{
			if (ComparisonOperator == ComparisonConstants.CurrentUser)
			{
				AssignedStaff = new ZString(GlbStaff.CurrentUser.GS_Code);
			}
		}

		#endregion

		#region ModuleId

		protected override ModuleIdentifier GetModuleIdCore()
		{
			return SupportsFiltersMatchComparisonOperator ? ModuleIDs.GlbStaff : null;
		}

		#endregion

		#region Properties

		#region ClientType

		[BusinessObjectTestExclude]
		[List("ClientTypeList")]
		public ZString ClientType
		{
			get { return clientType; }
			set
			{
				clientType = value;
				ClientTypeInfo.RefreshBinding();
			}
		}
		ZString clientType;

		public ZPropertyInfo ClientTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ClientType)); }
		}

		public ClientTypesList ClientTypeList
		{
			get
			{
				if (clientTypeList == null)
				{
					clientTypeList = GetNewClientTypesList();
				}

				return clientTypeList;
			}
		}
		ClientTypesList clientTypeList;

		static CodeDescriptionPairList ClientTypes
		{
			get { return new ClientTypesList(); }
		}

		protected virtual ClientTypesList GetNewClientTypesList()
		{
			return new ClientTypesList();
		}

		#region ClientTypesList

		public class ClientTypesList : CodeDescriptionPairList
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
			public class Codes
			{
				public const string Consignor = "CNR";
				public const string Consignee = "CNE";
				public const string LocalClient = "LOC";
				public const string ControllingCustomer = "CPY";
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
			public class Descriptions
			{
				public static MultilingualString Consignor { get { return ResString.GetMultilingualString("ClientTypesList|Consignor", "Consignor"); } }
				public static MultilingualString Consignee { get { return ResString.GetMultilingualString("ClientTypesList|Consignee", "Consignee"); } }
				public static MultilingualString LocalClient { get { return ResString.GetMultilingualString("ClientTypesList|LocalClient", "Local Client"); } }
				public static MultilingualString ControllingCustomer { get { return ResString.GetMultilingualString("ClientTypesList|ControllingCustomer", "Controlling Customer"); } }
			}

			public ClientTypesList()
			{
				AddClientTypes();
			}

			protected virtual void AddClientTypes()
			{
				AddPair(Codes.Consignor, Descriptions.Consignor);
				AddPair(Codes.Consignee, Descriptions.Consignee);
				AddPair(Codes.LocalClient, Descriptions.LocalClient);
				AddPair(Codes.ControllingCustomer, Descriptions.ControllingCustomer);
			}
		}

		#endregion

		#endregion

		#region Staff Role

		[BusinessObjectTestExclude]
		[List("Lookups.StaffRoles")]
		public ZString StaffRole
		{
			get { return staffRole; }
			set
			{
				staffRole = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientType();
				}

				StaffRoleInfo.RefreshBinding();
			}
		}
		ZString staffRole;

		public ZPropertyInfo StaffRoleInfo
		{
			get { return GetZPropertyInfo(nameof(StaffRole)); }
		}

		#endregion

		#region Assigned Staff

		[BusinessObjectTestExclude]
		[List("Lookups.PersonResponsibles")]
		public ZString AssignedStaff
		{
			get
			{
				if (ComparisonOperator == ComparisonConstants.IsBlank || ComparisonOperator == ComparisonConstants.IsNotBlank)
				{
					return ZString.Empty;
				}
				return assignedStaff;
			}
			set
			{
				assignedStaff = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientType();
				}

				AssignedStaffInfo.RefreshBinding();
			}
		}

		protected bool AssignedStaff_ReadOnly => ShouldComparisonOperatorCauseReadOnly();

		ZString assignedStaff;

		public ZPropertyInfo AssignedStaffInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedStaff)); }
		}

		#endregion

		#region Department

		[BusinessObjectTestExclude]
		[List("Lookups.DepartmentCodes")]
		public ZString Department
		{
			get { return department; }
			set
			{
				department = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientType();
				}

				DepartmentInfo.RefreshBinding();
			}
		}
		ZString department;

		public ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(Department)); }
		}

		#endregion

		#region Controlling Branch

		[BusinessObjectTestExclude]
		[List("Lookups.ControllingBranches")]
		public ZGuid ControllingBranch
		{
			get { return controllingBranch; }
			set
			{
				controllingBranch = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateClientType();
				}

				ControllingBranchInfo.RefreshBinding();
			}
		}
		ZGuid controllingBranch;

		public ZPropertyInfo ControllingBranchInfo
		{
			get { return GetZPropertyInfo(nameof(ControllingBranch)); }
		}

		#endregion

		#endregion

		#region Validation

		public new OrgClientAssignedStaffModuleFilterValidation Validation
		{
			get { return (OrgClientAssignedStaffModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new OrgClientAssignedStaffModuleFilterValidation(this);
		}

		#endregion

		#region Query

		ZDBOnlySubQuery GetFiltersMatchSubQuery()
		{
			if (IsFilterCollectionComparisonOperatorSelected())
			{
				return GetSubModuleSubQuery(SelectedFilters, ((IModuleFilterWithSelectedFilters)this).GetSubFilterQueryIncludingCollectionFilters(), UsesNotInQuery);
			}
			else
			{
				return null;
			}
		}

		static ZQuery EmptyQuery(ZString value)
		{
			return new ZQuery();
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : clientAssignedStaffQueryDelegate(GetClientAssignedStaffFilter());
		}

		protected virtual ZDBOnlySubQuery GetClientAssignedStaffFilter()
		{
			ZDBOnlySubQuery orgQuery;
			var orgAddressQuery = GetOrgAddressQuery();

			if (ClientType == ClientTypesList.Codes.LocalClient)
			{
				orgQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				orgQuery.AddSubQuery(JobHeaderSchema.JH_OA_LocalChargesAddr, orgAddressQuery, JoinCondition.And);
			}
			else
			{
				orgQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				orgQuery.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, GetAddressType());
				orgQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressQuery, JoinCondition.And);
			}

			return orgQuery;
		}

		protected ZDBOnlySubQuery GetOrgAddressQuery()
		{
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);

			if (StaffDetailsProvided || ControllingBranch.IsEmpty)
			{
				var clientAssignedStaffQuery = GetClientAssignedStaffSubQuery();
				orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, clientAssignedStaffQuery, JoinCondition.And);
			}
			AddControllingBranchSubQuery(orgAddressQuery, OrgAddressSchema.OA_OH);

			return orgAddressQuery;
		}

		protected ZDBOnlySubQuery GetClientAssignedStaffSubQuery()
		{
			var clientAssignedStaffQuery = new ZDBOnlySubQuery(typeof(OrgStaffAssignments), OrgStaffAssignmentsSchema.O8_OH);

			if (!AssignedStaff.IsEmpty
				|| ComparisonOperator == ComparisonConstants.IsBlank
				|| ComparisonOperator == ComparisonConstants.IsNotBlank
				|| ComparisonOperator == ComparisonConstants.FiltersMatch)
			{
				if (IsFilterCollectionComparisonOperatorSelected())
				{
					ZDBOnlySubQuery filtersMatchQuery = GetFiltersMatchSubQuery();
					clientAssignedStaffQuery.AddSubQuery(OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, GlbStaffSchema.GS_Code, filtersMatchQuery, JoinCondition.And);
				}
				else
				{
					clientAssignedStaffQuery.AddToFilter(JoinCondition.And, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, SqlComparisonOperator, AssignedStaff);
				}
			}

			if (!StaffRole.IsEmpty)
			{
				clientAssignedStaffQuery.AddToFilter(JoinCondition.And, OrgStaffAssignmentsSchema.O8_Role, StaffRole);
			}

			if (!Department.IsEmpty)
			{
				clientAssignedStaffQuery.AddToFilter(JoinCondition.And, OrgStaffAssignmentsSchema.O8_Department, Department);
			}

			var companyQuery = new ZQuery(OrgStaffAssignmentsSchema.O8_GC, GlbCompany.CurrentCompany.PK);
			companyQuery.AddToFilter(JoinCondition.Or, OrgStaffAssignmentsSchema.O8_GC, null);

			clientAssignedStaffQuery.AddToFilter(companyQuery);

			return clientAssignedStaffQuery;
		}

		protected void AddControllingBranchSubQuery(ZDBOnlyQuery query, SchemaColumn column)
		{
			if (!ControllingBranch.IsEmpty)
			{
				var controllingBranchQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
				controllingBranchQuery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GB_ControllingBranch, ControllingBranch);
				query.AddSubQuery(column, controllingBranchQuery, JoinCondition.And);
			}
		}

		protected bool StaffDetailsProvided
		{
			get { return !StaffRole.IsEmpty || !AssignedStaff.IsEmpty || !Department.IsEmpty; }
		}

		protected virtual ZString GetAddressType()
		{
			ZString result = ZString.Empty;

			switch (ClientType)
			{
				case ClientTypesList.Codes.Consignor:
					result = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
					break;

				case ClientTypesList.Codes.Consignee:
					result = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
					break;

				case ClientTypesList.Codes.LocalClient:
					result = DocAddressTypes.Codes.LocalClient;
					break;

				case ClientTypesList.Codes.ControllingCustomer:
					result = DocAddressTypes.Codes.ControllingCustomer;
					break;
			}

			return result;
		}

		protected readonly GetClientAssignedStaffQuery clientAssignedStaffQueryDelegate;

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);

			writer.WriteElementString("ClientType", ClientType);
			writer.WriteElementString("StaffRole", StaffRole);
			writer.WriteElementString("AssignedStaff", ComparisonOperator != ComparisonConstants.CurrentUser ? AssignedStaff : string.Empty);
			writer.WriteElementString("Department", Department);
			writer.WriteElementString("ControllingBranch", ControllingBranch.ToString());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == "ClientType")
			{
				ClientType = reader.ReadElementString("ClientType");
			}

			if (reader.Name == "StaffRole")
			{
				StaffRole = reader.ReadElementString("StaffRole");
			}

			if (reader.Name == "AssignedStaff")
			{
				var assignedStaff = reader.ReadElementString("AssignedStaff");
				AssignedStaff = (ComparisonOperator == ComparisonConstants.CurrentUser) ? GlbStaff.CurrentUser.GS_Code : assignedStaff;
			}

			if (reader.Name == "Department")
			{
				Department = reader.ReadElementString("Department");
			}

			if (reader.Name == "ControllingBranch")
			{
				ControllingBranch = new Guid(reader.ReadElementString("ControllingBranch"));
			}
		}

		#endregion

		#region Implementation

		new BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}

				return factory;
			}
		}
		BusinessObjectFactory factory;

		protected override bool IsEmptyCore => base.IsEmptyCore && ClientType.IsEmpty;

		protected override void ClearCore()
		{
			base.ClearCore();
			ClientType = ZString.Empty;
			StaffRole = ZString.Empty;
			AssignedStaff = ZString.Empty;
			Department = ZString.Empty;
			ControllingBranch = ZGuid.Empty;
		}

		public OrgStaffAssignmentsLookupsImplementer Lookups
		{
			get { return OrgStaffAssignmentsLookupsImplementer.Get(Factory); }
		}

		#endregion
	}
}
