using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultEPaymentReference : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ProviderCode = "ProviderCode";
			public const string ReferenceType = "ReferenceType";
			public const string Reference = "Reference";
		}

		#endregion

		public DefaultEPaymentReference() : base()
		{
		}

		public DefaultEPaymentReference(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Properties

		#region ProviderCode

		[List("PaymentProviderCodes")]
		[ResourceStringData("7f174dd8-f01b-4285-ae77-8a4a230c465c", Caption = "Provider")]
		public ZString ProviderCode
		{
			get => providerCode;
			set
			{
				SetNonPersistentPropertyValue(ProviderCodeInfo, ref providerCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateProviderCode();
				}
			}
		}
		ZString providerCode;

		public ZPropertyInfo ProviderCodeInfo => GetZPropertyInfo(Schema.ProviderCode);

		public ICodeDescriptionPairList PaymentProviderCodes => EPaymentProviderCodes.CodesList;

		#endregion

		#region ReferenceType

		[List("PaymentReferenceTypeList")]
		[ResourceStringData("24D80E74-E175-4660-8995-A7E25798249C", Caption = "Reference Type")]
		public ZString ReferenceType
		{
			get => referenceType;
			set
			{
				SetNonPersistentPropertyValue(ReferenceTypeInfo, ref referenceType, value);

				if (!IsFreeText)
				{
					Reference = ZString.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateReferenceType();
				}
			}
		}
		ZString referenceType;

		public ZPropertyInfo ReferenceTypeInfo => GetZPropertyInfo(Schema.ReferenceType);

		public ICodeDescriptionPairList PaymentReferenceTypeList => EPaymentReferenceTypes.CodesList;

		#endregion

		#region Reference

		[MaxLength(30)]
		[ResourceStringData("33F003DC-4025-4892-91D4-E1B13B7DFD15", Caption = "Reference")]
		[ReadOnlyMember(nameof(Reference_ReadOnly))]
		public ZString Reference
		{
			get => reference;
			set
			{
				CheckMaximumLength(ReferenceInfo, value);
				SetNonPersistentPropertyValue(ReferenceInfo, ref reference, value);
			}
		}
		ZString reference;

		public ZPropertyInfo ReferenceInfo => GetZPropertyInfo(Schema.Reference);
		bool Reference_ReadOnly => !IsFreeText;

		#endregion

		#endregion

		#region Validation

		public DefaultEPaymentReferenceValidation Validation => validation ?? (validation = new DefaultEPaymentReferenceValidation(this));
		DefaultEPaymentReferenceValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ProviderCode, ProviderCode);
			writer.WriteElementString(Schema.ReferenceType, ReferenceType);
			writer.WriteElementString(Schema.Reference, Reference);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ProviderCode = reader.ReadElementString(Schema.ProviderCode);
			ReferenceType = reader.ReadElementString(Schema.ReferenceType);
			Reference = reader.ReadElementString(Schema.Reference);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DefaultEPaymentReference(fallbackLevel);

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var target = clone as DefaultEPaymentReference;
			target.ProviderCode = ProviderCode;
			target.ReferenceType = ReferenceType;
			target.Reference = Reference;
		}

		bool IsFreeText => ReferenceType == EPaymentReferenceTypes.FreeText;
	}
}
