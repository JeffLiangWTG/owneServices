using System;
using System.Data;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbSecurity : AutoGlbSecurity, Integration.IGlbSecurity
	{
		public GlbSecurity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal static bool IsSecurityModificationOnly(IBusiness businessEntity, GlbSecurityCollection securityPermissions)
		{
			foreach (IBusiness child in businessEntity.Children)
			{
				if (child.HasChanges && (child != securityPermissions))
				{
					StmALogDependentCollection logs = child as StmALogDependentCollection;
					if (logs == null ||
						logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.NotEqual, Events.SecurityModified.Code)).Length > 0)
					{
						return false;
					}
				}
			}
			return true;
		}

		//If you add a new constant here, either introduce it as a real checkpoint with a parent in ZSecurity (For example see FindOrCreateChangeGroupSecurityCheckpoint(Guid securityGroupPK) ) or add its parent to below method.
		public const string ChangeOtherGroupSecurityRightName = "ChangeOtherGroupSecurity";
		public const string ChangeOtherStaffSecurityRightName = "ChangeOtherStaffSecurity";
		public const string GroupOwnerSecurityRightName = "GroupOwner";
		public const string AllowedPrincipalsSecurityRightName = "AllowedPrincipalsSecurityRightName";
		public const string AllowedClientsSecurityRightName = "AllowedClientsSecurityRightName";
		public const string AllowedWarehousesSecurityRightName = "AllowedWarehousesSecurityRightName";

		public string GetParentCheckpointForDisassociatedCheckpoint(string code)
		{
			switch (code)
			{
				case AllowedPrincipalsSecurityRightName:
					return "AgencyPrincipalAccess";
				case AllowedClientsSecurityRightName:
					return "WhsAllowedClients";
				case AllowedWarehousesSecurityRightName:
					return "WhsAllowedWarehouses";
				default:
					return code;
			}
		}

		#region Logging

		StmALog lastCreatedLog;

		BusinessObject Parent
			=> (BusinessObject)Group ?? Staff;

		IGlbSecurityCollectionWithSecurity ParentCollection
		{
			get
			{
				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					IGlbSecurityCollectionWithSecurity securityCollection = parentCollection as IGlbSecurityCollectionWithSecurity;
					if (securityCollection != null)
					{
						return securityCollection;
					}
				}
				return null;
			}
		}

		void ClearLog(bool delete)
		{
			if (delete && (lastCreatedLog != null))
			{
				lastCreatedLog.Delete();
			}
			lastCreatedLog = null;
		}

		void CreateSecurityLog(BusinessObject parent, Event logEvent, ZString securityRight)
		{
			if (parent != null)
			{
				var itemGuid = GU_ItemGUID.IsValid ? GU_ItemGUID.ToGuid() : Guid.Empty;
				var parentCollection = ParentCollection;
				var security = (parentCollection != null) && (parentCollection.Security != null) ? parentCollection.Security : Env.Security;

				string reference = string.Empty;
				if (security.FindCheckPoint(new CheckpointLookupKey(securityRight, itemGuid)) is ISecurityCheckpoint checkpoint)
				{
					reference = GetLogReference(logEvent, checkpoint);
					if (reference.Length > StmALogSchema.SL_Reference.MaxLength)
					{
						reference = reference.Substring(0, StmALogSchema.SL_Reference.MaxLength);
					}
				}

				lastCreatedLog = parent.GetLogs().AddNew(Events.SecurityModified, reference);
			}
		}

		public ISecurityCheckpoint Checkpoint
		{
			get
			{
				if (fCheckpoint == null)
				{
					var itemGuid = GU_ItemGUID.IsValid ? GU_ItemGUID.ToGuid() : Guid.Empty;
					var parentCollection = ParentCollection;
					var security = (parentCollection != null) && (parentCollection.Security != null) ? parentCollection.Security : Env.Security;
					fCheckpoint = security.FindCheckPoint(new CheckpointLookupKey(GU_SecurityRight, itemGuid));
				}
				return fCheckpoint;
			}
		}
		ISecurityCheckpoint fCheckpoint;

		public override void Delete()
		{
			if (IsInDatabase)
			{
				CreateSecurityLog(Parent, Events.DeletedARecordInTheSystem, (ZString)GU_SecurityRightInfo.OriginalValue);
			}
			var success = false;
			try
			{
				base.Delete();
				success = true;
			}
			finally
			{
				if (!success)
				{
					ClearLog(true);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Log Reference")]
		string GetLogReference(Event logEvent, ISecurityCheckpoint checkpoint)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(logEvent.Code);
			builder.Append(" - ");
			builder.Append(checkpoint.HumanReadableName.GetUnresolvedString());
			builder.Append(",");
			builder.Append((NoResString)" Is Allowed: ");

			bool deleting = (logEvent == Events.DeletedARecordInTheSystem);
			builder.Append(deleting ? GU_SecurityItemIsAllowedInfo.OriginalValue.ToString() : GU_SecurityItemIsAllowed.ToString());
			if (!deleting && GU_SecurityItemIsAllowedInfo.HasChanges)
			{
				builder.Append((NoResString)" (was ");
				builder.Append(GU_SecurityItemIsAllowedInfo.OriginalValue.ToString());
				builder.Append(")");
			}
			builder.Append(",");

			builder.Append(" Branch: ");
			builder.Append(Branch == null ? "All" : Branch.GB_Code.ToString());
			builder.Append(",");
			builder.Append(" Dept: ");
			builder.Append(Department == null ? "All" : Department.GE_Code.ToString());
			builder.Append(",");
			builder.Append(" Company: ");
			builder.Append(Company == null ? "All" : Company.GC_Code.ToString());

			return builder.ToString();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			ClearLog(!saveSucceeded);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			Event logEvent = IsInDatabase ? Events.EditedARecord : Events.AddedARecordToTheSystem;
			CreateSecurityLog(Parent, logEvent, GU_SecurityRight);
		}

		#endregion

		#region Properties

		// The rights calculation is currently quite expensive - caching helps improve the user experience.
		bool changeOthersRightsNeedsValidation;
		bool hasRightsToChangeOthers = true;

		#region IsAllowed

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Event Log Reference")]
		public ZString IsAllowed
		{
			get
			{
				return GU_SecurityItemIsAllowed ? "Yes" : "No";
			}
		}

		public ZPropertyInfo IsAllowedInfo
		{
			get { return GetZPropertyInfo(nameof(IsAllowed)); }
		}

		#endregion

		internal bool ChangeOthersRightsNeedsValidation
		{
			get { return changeOthersRightsNeedsValidation; }
			set { changeOthersRightsNeedsValidation = value; }
		}

		internal bool HasRightsToChangeOthers
		{
			get { return hasRightsToChangeOthers; }
			set { hasRightsToChangeOthers = value; }
		}

		public ZString ParentCode
			=> Group?.GG_Code
				?? Staff?.GS_Code
				?? default;

		public ZPropertyInfo ParentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ParentCode)); }
		}

		public ZString ParentDescription
			=> Group?.GG_Desc
				?? Staff?.GS_FullName
				?? default;

		public ZPropertyInfo PParentDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ParentDescription)); }
		}

		public ZString ParentType
		{
			get
			{
				if (Group != null)
				{
					return DataBoundResourceStrings.GetTableDescriptiveName(Group.TableName);
				}
				if (Staff != null)
				{
					return DataBoundResourceStrings.GetTableDescriptiveName(Staff.TableName);
				}
				return "";
			}
		}

		public ZPropertyInfo ParentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ParentType)); }
		}

		#region Item Code

		public ZString ItemCode
		{
			get { return Item == null ? "" : ((ICodeDescription)Item).Code; }
		}

		public ZPropertyInfo ItemCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ItemCode)); }
		}

		#endregion

		#region Item Name

		public ZString ItemName
		{
			get { return Item == null ? "" : ((ICodeDescription)Item).Description; }
		}

		public ZPropertyInfo ItemNameInfo
		{
			get { return GetZPropertyInfo(nameof(ItemName)); }
		}

		#endregion

		#region Item Type

		public ZString ItemType
		{
			get
			{
				return Item == null
				? ""
				: GU_SecurityRight == AllowedWarehousesSecurityRightName
					? Res.GetString("AllowedOrgsAndWarehousesControl|WarehouseItemType", "Warehouse")
					: DataBoundResourceStrings.GetTableDescriptiveName(Item.TableName);
			}
		}

		public ZPropertyInfo ItemTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ItemType)); }
		}

		#endregion

		#region DomainName
		public ZString DomainName
		{
			get
			{
				if (Item is GlbGroup)
				{
					return ((GlbGroup)Item).DomainName;
				}
				return null;
			}
		}

		#endregion

		BusinessObject Item
		{
			get
			{
				switch (GU_SecurityRight)
				{
					case GroupOwnerSecurityRightName:
						return Factory.Load<GlbGroup>(GU_ItemGUID);
					case ChangeOtherGroupSecurityRightName:
						return Factory.Load<GlbGroup>(GU_ItemGUID);
					case ChangeOtherStaffSecurityRightName:
						return Factory.Load<GlbStaff>(GU_ItemGUID);
					case AllowedPrincipalsSecurityRightName:
						return Factory.Load<OrgHeader>(GU_ItemGUID);
					case AllowedClientsSecurityRightName:
						return Factory.Load<OrgHeader>(GU_ItemGUID);
					case AllowedWarehousesSecurityRightName:
						Type whsWarehouseCollectionType = CargoWise.Application.ObjectFactory.GetType<Warehouse.Integration.IWhsWarehouseCollection>();
						BusinessObjectCollection tempCollection = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, Factory);
						return Factory.Load(tempCollection.TypeOfElements, GU_ItemGUID);
					default:
						return null;
				}
			}
		}

		#region Company Code

		[BusinessObjectTestExclude]
		[List("Lookups.Companies")]
		[MaxLength(3)]
		public ZString CompanyCode
		{
			get
			{
				if (Company != null)
				{
					fCompanyCode = Company.GC_Code;
				}

				if (fCompanyCode.IsEmpty)
				{
					return "*";
				}
				else
				{
					return fCompanyCode;
				}
			}
			set
			{
				ChangeOthersRightsNeedsValidation |= value != CompanyCode;

				if (value == "*" || value.IsEmpty)
				{
					GU_GC = ZGuid.Empty;
					fCompanyCode = "*";
				}
				else
				{
					CheckMaximumLength(CompanyCodeInfo, value);
					fCompanyCode = value;
					GlbCompany companyWithCode = (GlbCompany)Factory.LoadFromNaturalKey(typeof(GlbCompany), GlbCompanySchema.GC_Code, value);
					if (companyWithCode != null)
					{
						GU_GC = companyWithCode.PK;
					}
					BranchCode = "*";
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCompanyCode();
				}
				CompanyCodeInfo.RefreshBinding();
			}
		}

		ZString fCompanyCode;

		public ZPropertyInfo CompanyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyCode)); }
		}

		#endregion

		#region Company

		[List("Lookups.Companies")]
		public override GlbCompany Company
		{
			get
			{
				return base.Company;
			}
		}

		#endregion

		#region Department Code

		[BusinessObjectTestExclude]
		[List("Lookups.Departments")]
		[MaxLength(3)]
		public ZString DepartmentCode
		{
			get
			{
				if (Department != null)
				{
					fDepartmentCode = Department.GE_Code;
				}

				if (fDepartmentCode.IsEmpty)
				{
					return "*";
				}
				else
				{
					return fDepartmentCode;
				}
			}
			set
			{
				ChangeOthersRightsNeedsValidation |= value != DepartmentCode;
				if (value == "*" || value.IsEmpty)
				{
					GU_GE = ZGuid.Empty;
					fDepartmentCode = "*";
				}
				else
				{
					CheckMaximumLength(DepartmentCodeInfo, value);
					fDepartmentCode = value;
					GlbDepartment departmentWithCode = (GlbDepartment)Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, value);
					if (departmentWithCode != null)
					{
						GU_GE = departmentWithCode.PK;
					}
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateDepartmentCode();
				}
				DepartmentCodeInfo.RefreshBinding();
			}
		}

		ZString fDepartmentCode;

		public ZPropertyInfo DepartmentCodeInfo
		{
			get { return GetZPropertyInfo(nameof(DepartmentCode)); }
		}

		#endregion

		#region Department

		[List("Lookups.Departments")]
		public override GlbDepartment Department
		{
			get
			{
				return base.Department;
			}
		}

		#endregion

		#region Branch Code

		[BusinessObjectTestExclude]
		[List("Lookups.Branches")]
		[MaxLength(3)]
		public ZString BranchCode
		{
			get
			{
				if (Branch != null)
				{
					fBranchCode = Branch.GB_Code;
				}

				if (fBranchCode.IsEmpty)
				{
					return "*";
				}
				else
				{
					return fBranchCode;
				}
			}
			set
			{
				ChangeOthersRightsNeedsValidation |= value != BranchCode;
				if (value == "*" || value.IsEmpty)
				{
					GU_GB = ZGuid.Empty;
					fBranchCode = "*";
				}
				else
				{
					CheckMaximumLength(BranchCodeInfo, value);
					fBranchCode = value;
					GlbBranch branchWithCode = (GlbBranch)Factory.LoadFromNaturalKey(typeof(GlbBranch), GlbBranchSchema.GB_Code, value);
					if (branchWithCode != null)
					{
						GU_GB = branchWithCode.PK;
					}
					CompanyCode = "*";
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateBranchCode();
				}
				BranchCodeInfo.RefreshBinding();
			}
		}

		ZString fBranchCode;

		public ZPropertyInfo BranchCodeInfo
		{
			get { return GetZPropertyInfo(nameof(BranchCode)); }
		}

		#endregion

		#region Branch

		[List("Lookups.Branches")]
		public override GlbBranch Branch
		{
			get
			{
				return base.Branch;
			}
		}

		#endregion

		public override ZBool GU_SecurityItemIsAllowed
		{
			get { return base.GU_SecurityItemIsAllowed; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_SecurityItemIsAllowed;
				base.GU_SecurityItemIsAllowed = value;
			}
		}

		public override ZString GU_SecurityRight
		{
			get { return base.GU_SecurityRight; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_SecurityRight;
				base.GU_SecurityRight = value;
			}
		}
		[List("Lookups.Groups")]
		public override ZGuid GU_GG
		{
			get { return base.GU_GG; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_GG;
				base.GU_GG = value;
			}
		}

		public override ZGuid GU_GS
		{
			get { return base.GU_GS; }
			set
			{
				ChangeOthersRightsNeedsValidation |= (value != GU_GS);
				base.GU_GS = value;
			}
		}

		public override ZGuid GU_GB
		{
			get { return base.GU_GB; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_GB;
				base.GU_GB = value;
			}
		}

		public override ZGuid GU_GE
		{
			get { return base.GU_GE; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_GE;
				base.GU_GE = value;
			}
		}

		public override ZGuid GU_GC
		{
			get { return base.GU_GC; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_GC;
				base.GU_GC = value;
			}
		}

		public override ZGuid GU_ItemGUID
		{
			get { return base.GU_ItemGUID; }
			set
			{
				ChangeOthersRightsNeedsValidation |= value != GU_ItemGUID;
				base.GU_ItemGUID = value;
			}
		}

		public CheckpointLookupKey GetCheckpointLookupKey()
		{
			CheckpointLookupKey lookupKey = new CheckpointLookupKey(GU_SecurityRight, GU_ItemGUID.IsEmpty ? Guid.Empty : GU_ItemGUID.ToGuid());

			return lookupKey;
		}

		internal CheckpointLookupKey GetCheckpointLookupKey_ForValidation()
		{
			string code = GetParentCheckpointForDisassociatedCheckpoint(GU_SecurityRight);

			if (code == GU_SecurityRight)
			{
				return GetCheckpointLookupKey();
			}
			CheckpointLookupKey lookupKey = new CheckpointLookupKey(code, Guid.Empty);

			return lookupKey;
		}

		#endregion
	}
}
