using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	[SingleObjectAroundARow]
	[SystemDefinedValues]
	public abstract class CusInBondPerson : AutoCusInBondPerson, ICertificatesProvider
	{
		protected CusInBondPerson(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondPerson.Schema
		{
			internal const string FallbackToParent = nameof(CusInBondPerson.FallbackToParent);
		}

		public static T LoadOrCreate<T>(CusInBondHeader cusInBondHeader, ZString type)
			where T : CusInBondPerson
		{
			var factory = cusInBondHeader.Factory;
			var cusInBondHeaderPK = cusInBondHeader.PK;

			var query = GetFilter(cusInBondHeaderPK, type);
			query.FetchOnlyFromLocalCache = !cusInBondHeader.IsInDatabase;
			var result = factory.LoadTop1<T>(query);
			if (result == null)
			{
				result = factory.New<T>();
				using (result.SuspendSettingHasChanges())
				{
					result.CP_BH_Header = cusInBondHeaderPK;
					result.CP_Type = type;
				}
			}
			return result;
		}

		public static ZQuery GetFilter(ZGuid nctsHeaderPk, ZString type)
		{
			var query = new ZQuery(CusInBondPersonSchema.CP_BH_Header, nctsHeaderPk);
			query.AddToFilter(CusInBondPersonSchema.CP_Type, type);
			return query;
		}

		#region Properties

		[ReadOnlyMember(Schema.FallbackToParent)]
		public override ZDateTime CP_DateOfBirth
		{
			get { return FallbackToParent ? Parent.DateOfBirth : base.CP_DateOfBirth; }
			set { base.CP_DateOfBirth = value; }
		}

		[ReadOnlyMember(Schema.FallbackToParent)]
		public override ZString CP_FullName
		{
			get { return FallbackToParent ? Parent.FullName : base.CP_FullName; }
			set { base.CP_FullName = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondPersonLookups.Gender))]
		[ReadOnlyMember(Schema.FallbackToParent)]
		public override ZString CP_Gender
		{
			get { return FallbackToParent ? Parent.Gender : base.CP_Gender; }
			set { base.CP_Gender = value; }
		}

		public override ZString CP_GS_NKStaff
		{
			get { return base.CP_GS_NKStaff; }
			set
			{
				var hasChanges = base.CP_GS_NKStaff != value;
				base.CP_GS_NKStaff = value;
				if (hasChanges && !IsCopying)
				{
					OnParentChanged();
				}
			}
		}

		protected bool CP_GS_NKStaff_ReadOnly
		{
			get { return Contact != null; }
		}

		#region Staff Wrapper

		class StaffWrapper : ICusInBondPersonParent
		{
			internal StaffWrapper(GlbStaff staff)
			{
				this.staff = staff;
			}

			#region Implementation of ICusInBondPersonParent

			ZString ICusInBondPersonParent.FullName
			{
				get { return staff.GS_FullName; }
			}

			ZDateTime ICusInBondPersonParent.DateOfBirth
			{
				get { return staff.GS_Birthdate; }
			}

			ZString ICusInBondPersonParent.Gender
			{
				get { return staff.GS_Gender; }
			}

			ZString ICusInBondPersonParent.Nationality
			{
				get { return staff.GS_RN_NKNationalityCode; }
			}

			GenRegCertAccredMaintListCollection ICusInBondPersonParent.Certificates
			{
				get { return staff.Certificates; }
			}

			#endregion

			readonly GlbStaff staff;
		}

		#endregion

		public override ZGuid CP_OC_Contact
		{
			get { return base.CP_OC_Contact; }
			set
			{
				var hasChanges = base.CP_OC_Contact != value;
				base.CP_OC_Contact = value;
				if (hasChanges && !IsCopying)
				{
					OnParentChanged();
				}
			}
		}

		protected bool CP_OC_Contact_ReadOnly
		{
			get { return Staff != null; }
		}

		#region Contact Wrapper

		class ContactWrapper : ICusInBondPersonParent
		{
			internal ContactWrapper(OrgContact contact)
			{
				this.contact = contact;
			}

			#region Implementation of ICusInBondPersonParent

			ZString ICusInBondPersonParent.FullName
			{
				get { return contact.OC_ContactName; }
			}

			ZDateTime ICusInBondPersonParent.DateOfBirth
			{
				get { return contact.OC_Birthday; }
			}

			ZString ICusInBondPersonParent.Gender
			{
				get { return contact.OC_Gender; }
			}

			ZString ICusInBondPersonParent.Nationality
			{
				get { return contact.OC_RN_NKNationality; }
			}

			GenRegCertAccredMaintListCollection ICusInBondPersonParent.Certificates
			{
				get { return contact.Certificates; }
			}

			#endregion

			readonly OrgContact contact;
		}

		#endregion

		[ReadOnlyMember(Schema.FallbackToParent)]
		public override ZString CP_RN_NKNationality
		{
			get { return FallbackToParent ? Parent.Nationality : base.CP_RN_NKNationality; }
			set { base.CP_RN_NKNationality = value; }
		}

		#region Parent

		ICusInBondPersonParent Parent
		{
			get
			{
				if (parent == null)
				{
					var staff = Staff;
					if (staff != null)
					{
						parent = new StaffWrapper(staff);
					}
					else
					{
						var contact = Contact;
						if (contact != null)
						{
							parent = new ContactWrapper(contact);
						}
					}
				}
				return parent;
			}
		}
		ICusInBondPersonParent parent;

		void OnParentChanged()
		{
			parent = null;
			parentCertificates = null;
			MarkAsNeedingValidation();
			RefreshCertificatesFilter();
			RefreshBinding();
		}

		void RefreshCertificatesFilter()
		{
			Certificates.AdditionalFilter = FallbackToParent ? ParentCertificates.CompleteFilter : InternalCertificates.CompleteFilter;
			Certificates.SetReadOnlyIncludingChildren(FallbackToParent);
		}

		protected bool FallbackToParent
		{
			get { return Parent != null; }
		}

		#region ICusInBondPersonParent

		interface ICusInBondPersonParent
		{
			ZString FullName { get; }
			ZDateTime DateOfBirth { get; }
			ZString Gender { get; }
			ZString Nationality { get; }
			GenRegCertAccredMaintListCollection Certificates { get; }
		}

		#endregion

		#endregion

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			if (FallbackToParent)
			{
				ClearPersistentDetails();
			}
		}

		void ClearPersistentDetails()
		{
			CP_FullNameInfo.ClearValue();
			CP_DateOfBirthInfo.ClearValue();
			CP_GenderInfo.ClearValue();
			CP_RN_NKNationalityInfo.ClearValue();
			InternalCertificates.DeleteAll();
		}

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clonedPerson = (CusInBondPerson)base.CloneInternal(args);
			using (clonedPerson.SuspendSettingHasChanges())
			using (clonedPerson.GetValidationSuspender())
			{
				if (!FallbackToParent)
				{
					var certificateCloneArgs = new CertificateCloneArgs();
					clonedPerson.InternalCertificates.AddRange(InternalCertificates.Select(c => c.Clone(certificateCloneArgs)));
				}
			}
			return clonedPerson;
		}

		class CertificateCloneArgs : BusinessObjectCloneArgs
		{
			internal CertificateCloneArgs()
				: base(new[]
				{
					AutoGenRegCertAccredMaintList.Schema.XZ_ParentID,
					AutoGenRegCertAccredMaintList.Schema.XZ_ParentTableCode
				}, performRowCopyWithoutTriggeringValidationAndSetter: true)
			{ }
		}

		#endregion

		#region Implementation of ICertificatesProvider

		#region Certificates

		public GenRegCertAccredMaintListCollection Certificates
		{
			get
			{
				if (certificates == null)
				{
					certificates = new GenRegCertAccredMaintListCollection(Factory, this);
					RefreshCertificatesFilter();
				}
				return certificates;
			}
		}

		GenRegCertAccredMaintListCollection certificates;

		[ChildEditable(true)]
		GenRegCertAccredMaintListCollection InternalCertificates
		{
			get
			{
				if (internalCertificates == null)
				{
					internalCertificates = new GenRegCertAccredMaintListCollection(this);
					RegisterEditableChildObject(internalCertificates);
				}
				return internalCertificates;
			}
		}

		GenRegCertAccredMaintListCollection internalCertificates;

		GenRegCertAccredMaintListCollection ParentCertificates
		{
			get
			{
				if (parentCertificates == null && Parent != null)
				{
					parentCertificates = Parent.Certificates;
					var certificateTypeList = GetCertificateTypeList();
					if (certificateTypeList != null)
					{
						var types = certificateTypeList.Cast<ICodeDescription>().Select(p => p.Code);
						var filter = new ZQuery(GenRegCertAccredMaintListSchema.XZ_Type, types);
						parentCertificates.AdditionalFilter = filter;
					}
				}
				return parentCertificates;
			}
		}

		GenRegCertAccredMaintListCollection parentCertificates;

		#endregion

		public virtual ICodeDescriptionPairList GetCertificateTypeList()
		{
			return null;
		}

		public virtual ICodeDescriptionPairList GetActiveCertificateTypeList()
		{
			return null;
		}

		public virtual ZString GetDefaultDescription(ZString code)
		{
			return ZString.Empty;
		}

		#endregion
	}
}
