using System;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public class LocalCountryCustomsInterface : RegistryBusinessObjectTemplate, IEquatable<LocalCountryCustomsInterface>
	{
		public static class Schema
		{
			public const string RecipientID = nameof(LocalCountryCustomsInterface.RecipientID);
			public const string SubmissionType = nameof(LocalCountryCustomsInterface.SubmissionType);
			public const string InterfaceType = nameof(LocalCountryCustomsInterface.InterfaceType);
		}

		[MaxLength(36)]
		[ReadOnlyMember(nameof(RecipientID_ReadOnly))]
		public ZString RecipientID
		{
			get => recipientID;
			set
			{
				CheckMaximumLength(RecipientIDInfo, value);
				SetNonPersistentPropertyValue(RecipientIDInfo, ref recipientID, value);
				if (!IsValidationSuspended)
				{
					ValidateRecipientID();
				}
			}
		}
		ZString recipientID;

		public ZPropertyInfo RecipientIDInfo => GetZPropertyInfo(Schema.RecipientID);

		ZBool RecipientID_ReadOnly => IsABMInterfaceActivated || SubmissionType == DeclarationApplicationCodeList.Codes.Builtin;

		void ValidateRecipientID()
		{
			RecipientIDInfo.ClearAllNotifications();
			if (CurrentFallbackCompany is GlbCompany company && !IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(company.GC_RN_NKCountryCode, company.PK.ToGuid())
				&& SubmissionType != DeclarationApplicationCodeList.Codes.Builtin)
			{
				if (recipientID.Trim().Length < 3)
				{
					RecipientIDInfo.AddError(Res.GetString("4e9f96d0-a033-42d2-b95d-3684b9335938", "Recipient ID must be at least 3 characters long."));
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(SubmissionTypeList))]
		public ZString SubmissionType
		{
			get => submissionType;
			set
			{
				CheckMaximumLength(SubmissionTypeInfo, value);
				SetNonPersistentPropertyValue(SubmissionTypeInfo, ref submissionType, value);
				if (value == DeclarationApplicationCodeList.Codes.Builtin)
				{
					recipientID = ZString.Empty;
					interfaceType = ZString.Empty;
				}
				else
				{
					interfaceType = LocalCountryCustomsInterfaceTypeCodeList.Codes.WiseTechCustoms;
				}
				if (!IsValidationSuspended)
				{
					ValidateSubmissionType();
				}
			}
		}
		ZString submissionType;

		public ZPropertyInfo SubmissionTypeInfo => GetZPropertyInfo(Schema.SubmissionType);

		ZBool InterfaceTypeReadOnly => SubmissionType == DeclarationApplicationCodeList.Codes.Builtin;

		public CodeDescriptionPairList SubmissionTypeList
		{
			get
			{
				CodeDescriptionPairList result = new DeclarationApplicationCodeListForRegistry();
				var company = CurrentFallbackCompany;

				if (company != null && IntegratedCountryHelper.IsInterfaceOnlySupported(company.GC_RN_NKCountryCode))
				{
					result = DeclarationApplicationCodeListITFOnly;
				}
				return result;
			}
		}

		void ValidateSubmissionType()
		{
			SubmissionTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(SubmissionTypeInfo);
			ListValidation.ErrorIfInvalidCode(SubmissionTypeInfo, SubmissionTypeList);
		}

		[MaxLength(3)]
		[List(nameof(InterfaceTypeList))]
		[ReadOnlyMember(nameof(InterfaceTypeReadOnly))]
		public ZString InterfaceType
		{
			get => interfaceType;
			set
			{
				CheckMaximumLength(InterfaceTypeInfo, value);
				SetNonPersistentPropertyValue(InterfaceTypeInfo, ref interfaceType, value);
				if (!IsValidationSuspended)
				{
					ValidateInterfaceType();
				}
			}
		}

		ZString interfaceType = ZString.Empty;

		public ZPropertyInfo InterfaceTypeInfo => GetZPropertyInfo(Schema.InterfaceType);

		public CodeDescriptionPairList InterfaceTypeList
		{
			get
			{
				return new LocalCountryCustomsInterfaceTypeCodeList();
			}
		}

		void ValidateInterfaceType()
		{
			InterfaceTypeInfo.ClearAllNotifications();
			if (SubmissionType != DeclarationApplicationCodeList.Codes.Builtin)
			{
				MandatoryValidation.CheckEntered(InterfaceTypeInfo);
				ListValidation.ErrorIfInvalidCode(InterfaceTypeInfo, InterfaceTypeList);
			}
		}

		public bool IsABMInterfaceActivated => CurrentFallbackCompany is GlbCompany company && IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(company.GC_RN_NKCountryCode, company.PK.ToGuid());

		internal CodeDescriptionPairList DeclarationApplicationCodeListITFOnly
		{
			get
			{
				if (fDeclarationApplicationCodeListITFOnly == null)
				{
					fDeclarationApplicationCodeListITFOnly = new DeclarationApplicationCodeListForRegistry();
					fDeclarationApplicationCodeListITFOnly.RemoveCode(DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted);
					fDeclarationApplicationCodeListITFOnly.RemoveCode(DeclarationApplicationCodeListForRegistry.Codes.BothInterfaceDefaulted);
					fDeclarationApplicationCodeListITFOnly.RemoveCode(DeclarationApplicationCodeListForRegistry.Codes.Builtin);
				}
				return fDeclarationApplicationCodeListITFOnly;
			}
		}
		CodeDescriptionPairList fDeclarationApplicationCodeListITFOnly;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new LocalCountryCustomsInterface();

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.RecipientID, RecipientID);
			writer.WriteElementString(Schema.SubmissionType, SubmissionType);
			writer.WriteElementString(Schema.InterfaceType, InterfaceType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RecipientID = reader.ReadElementString(Schema.RecipientID);
			SubmissionType = reader.ReadElementString(Schema.SubmissionType);
			InterfaceType = reader.ReadElementString(Schema.InterfaceType);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateRecipientID();
			ValidateSubmissionType();
			ValidateInterfaceType();
		}

		GlbCompany CurrentFallbackCompany
		{
			get
			{
				var companyPK = CurrentFallbackLevel?.CompanyPK(false);
				return CurrentFactory.Load<GlbCompany>(companyPK ?? ZGuid.Empty);
			}
		}

		public bool Equals(LocalCountryCustomsInterface other)
		{
			return !ReferenceEquals(null, other)
					&& (ReferenceEquals(this, other)
						|| (RecipientID.Equals(other.RecipientID)
							&& SubmissionType.Equals(other.SubmissionType)
							&& InterfaceType.Equals(other.InterfaceType)));
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj)
					&& (ReferenceEquals(this, obj)
						|| (obj.GetType() == GetType() && Equals((LocalCountryCustomsInterface)obj)));
		}

		public override int GetHashCode() => RecipientID.GetHashCode() ^ SubmissionType.GetHashCode() ^ InterfaceType.GetHashCode();
	}
}
