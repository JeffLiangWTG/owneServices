using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgStaffAssignments : AutoOrgStaffAssignments
		, IOrgStaffAssignments
		, IAddressBookRecipient
		, IWorkflowTriggerEventSource
	{
		public OrgStaffAssignments(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fO8_OH = O8_OH;
		}

		#region Overall Rep Security

		public bool IsOverallRepWithDeniedModifySecurity
		{
			get
			{
				return !Header.SecurityProvider.CanModifyDetailsStaffAssignment(O8_Role) && IsInDatabase;
			}
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			O8_Department = OrgStaffAssignmentsLookups.AllServices;
		}

		#endregion

		#region Properties

		#region O8_Role
		[List("Lookups.StaffRoles")]
		public override ZString O8_Role
		{
			get { return base.O8_Role; }
			set { base.O8_Role = value; }
		}
		#endregion

		#region O8_Department
		[List("Lookups.DepartmentCodes")]
		public override ZString O8_Department
		{
			get
			{
				return base.O8_Department;
			}
			set
			{
				base.O8_Department = value;
			}
		}
		#endregion

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZGuid O8_OH
		{
			get { return IsDeleted ? fO8_OH : base.O8_OH; }
			set
			{
				base.O8_OH = value;
				fO8_OH = value;
			}
		}
		ZGuid fO8_OH;

		#region Responsible Person Name

		public ZString ResponsiblePersonName
		{
			get { return PersonResponsible != null ? PersonResponsible.GS_FullName : ZString.Empty; }
		}

		public ZPropertyInfo ResponsiblePersonNameInfo
		{
			get { return GetZPropertyInfo(nameof(ResponsiblePersonName)); }
		}

		#endregion

		#region Responsible Person Login Name

		public ZString ResponsiblePersonLoginName
		{
			get { return PersonResponsible != null ? PersonResponsible.GS_LoginName : ZString.Empty; }
		}

		public ZPropertyInfo ResponsiblePersonLoginNameInfo
		{
			get { return GetZPropertyInfo(nameof(ResponsiblePersonLoginName)); }
		}

		#endregion

		#region Role Description

		public virtual ZString RoleDescription
		{
			get { return Lookups.StaffRoles.GetDescriptionFromCode(O8_Role); }
		}

		public ZPropertyInfo RoleDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RoleDescription)); }
		}

		#endregion

		#region Company

		[ReadOnlyMember(nameof(CompanySpecific))]
		public override ZGuid O8_GC
		{
			get { return base.O8_GC; }
			set { base.O8_GC = value; }
		}

		#endregion

		#region Product

		[List("ProductList")]
		public override ZString O8_Product
		{
			get => base.O8_Product;
			set => base.O8_Product = value;
		}

		public CodeDescriptionPairList ProductList => new CodeDescriptionPairList(); // We need a list in the base class as per design (used on a ZDropEditColumnStyleInfo component in the EDI client)

		public virtual ZString ProductDescription => ZString.Empty; // We need a this in the base class as per design

		#endregion

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get
			{
				if (!Env.Security.OrgDetailsModifyOtherCompanysStaffAssignments.IsAllowed)
				{
					if ((O8_GC.IsEmpty || O8_GC != GlbCompany.CurrentCompany.PK) && IsInDatabase)
					{
						return true;
					}
				}
				return base.ReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Parent Collection

		OrgStaffAssignmentsCollection StaffAssignmentsCollection
		{
			get { return Header == null ? null : Header.StaffAssignments; }
		}

		public bool CompanySpecific
		{
			get { return StaffAssignmentsCollection == null || StaffAssignmentsCollection.CompanySpecific; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override void OnCreateAutoAdminLog()
		{
			base.OnCreateAutoAdminLog();

			StmALog autoCreatedLog = Logs.AutoCreatedLog;
			if (autoCreatedLog != null)
			{
				using (((IUpdateFieldsLock)autoCreatedLog).LockForUpdatingKeyFields())
				{
					autoCreatedLog.SL_Reference = autoCreatedLog.SL_SE_NKEvent.Equals(Events.AddedARecordToTheSystem.Code)
						? CommentForAddedStaffAssignment
						: CommentForChangedStaffAssignment;
				}
			}
		}

		ZString CommentForAddedStaffAssignment
		{
			get
			{
				ZString logReference = (NoResString)"Staff Assignment -";
				if (PersonResponsible != null)
				{
					logReference = logReference + (NoResString)" Initials " + PersonResponsible.GS_Code;
				}
				logReference = logReference + (NoResString)" Role " + O8_Role;
				logReference = logReference + (NoResString)" Department " + O8_Department;
				return logReference;
			}
		}

		ZString CommentForChangedStaffAssignment
		{
			get
			{
				ZString logReference = ZString.Empty;
				if (O8_GS_NKPersonResponsibleInfo.HasChanges && PersonResponsible != null)
				{
					logReference = logReference + (NoResString)" Initials " + PersonResponsible.GS_Code
						+ (OriginalResponsiblePerson != null ? (NoResString)" (" + OriginalResponsiblePerson.GS_Code + (NoResString)")" : (NoResString)"");
				}
				if (O8_RoleInfo.HasChanges)
				{
					logReference = logReference + (NoResString)" Role " + O8_Role + (NoResString)" (" + O8_RoleInfo.OriginalValue + (NoResString)")";
				}
				if (O8_DepartmentInfo.HasChanges)
				{
					logReference = logReference + (NoResString)" Department " + O8_Department + (NoResString)" (" + O8_DepartmentInfo.OriginalValue + (NoResString)")";
				}

				logReference = logReference.IsEmpty ? (NoResString)"Staff Assignment" : (NoResString)"Staff Assignment -" + logReference;

				return logReference;
			}
		}

		GlbStaff OriginalResponsiblePerson
		{
			get
			{
				return (!O8_GS_NKPersonResponsibleInfo.OriginalValue.IsEmpty) ? Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, (ZString)O8_GS_NKPersonResponsibleInfo.OriginalValue) : null;
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;

			shouldBeReadOnly = IsInDatabase && Header != null && !Header.SecurityProvider.CanModifyDetailsStaffAssignment((ZString)O8_RoleInfo.OriginalValue);

			if (!shouldBeReadOnly && (O8_GC.IsEmpty || O8_GC != GlbCompany.CurrentCompany.PK))
			{
				shouldBeReadOnly = IsInDatabase && Header != null && !Header.SecurityProvider.HasModifyDetailsOtherCompanysStaffAssignmentsSecurity;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region IAddressBookRecipient Members

		OrgHeader IAddressBookRecipient.Organisation
		{
			get { return Header; }
		}

		ZString IAddressBookRecipient.Title
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).Title;
				}
				return ZString.Empty;
			}
		}

		ZString IAddressBookRecipient.Location
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).Location;
				}
				return ZString.Empty;
			}
		}

		ZString IAddressBookRecipient.Phone
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).Phone;
				}
				return ZString.Empty;
			}
		}

		bool IAddressBookRecipient.IsActive
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).IsActive;
				}
				return false;
			}
		}

		ZString IAddressBookRecipient.Role
		{
			get { return O8_Role; }
		}

		public ZString Name
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).Name;
				}
				return ZString.Empty;
			}
		}

		public ZString Email
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return ((IAddressBookRecipient)personResponsible).Email;
				}
				return ZString.Empty;
			}
		}

		ZGuid IAddressBookRecipient.PK
		{
			get
			{
				var personResponsible = PersonResponsible;
				if (personResponsible != null)
				{
					return personResponsible.PK;
				}
				return ZGuid.Empty;
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get
			{
				return (O8_GC.IsValid) ?
					Factory.Load<GlbCompany>(O8_GC) :
					Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var header = Header;
				if (header != null)
				{
					list.Add(Header);
				}
				return list;
			}
		}

		#endregion

		public void ValidateResponsiblePerson()
		{
			OrgStaffAssignmentsValidation validation = Validation;
			validation.IsResponsiblePersonMandatory = true;
			validation.ValidateO8_GS_NKPersonResponsible();
		}

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !IsInDatabase || Header.SecurityProvider.CanModifyDetailsStaffAssignment((ZString)O8_RoleInfo.OriginalValue); }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("56b3423d-8759-4a04-a028-ae5baa22fdfa", "You do not have the required security to remove this record."); }
		}

		#endregion

		#region For Test
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			O8_Product = ZString.Empty;
		}
#endif
		#endregion
	}
}
