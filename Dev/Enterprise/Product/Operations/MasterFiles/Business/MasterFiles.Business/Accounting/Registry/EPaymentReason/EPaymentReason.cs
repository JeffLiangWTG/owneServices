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
	public class EPaymentReason : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ProviderCode = nameof(ProviderCode);
			public const string ReasonCode = nameof(ReasonCode);
			public const string ReasonDescription = nameof(ReasonDescription);
		}

		#endregion

		#region Bound Properties

		#region ProviderCode

		[List(nameof(PaymentProviderCodes))]
		[ResourceStringData("dc894db5-8c88-4dbe-96c8-a82dcf8650a7", Caption = "Provider")]
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

		#region ReasonCode

		[MaxLength(3)]
		[ResourceStringData("0cf7a828-0f76-4a8b-b6c0-514f8bd8676b", Caption = "Code")]
		public ZString ReasonCode
		{
			get => reasonCode;
			set
			{
				CheckMaximumLength(ReasonCodeInfo, value);
				SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonCode();
				}
			}
		}
		ZString reasonCode;

		public ZPropertyInfo ReasonCodeInfo => GetZPropertyInfo(Schema.ReasonCode);

		#endregion

		#region ReasonDescription

		[MaxLength(80)]
		[ResourceStringData("1e75b62e-217e-453a-88de-65c8498ea378", Caption = "Description")]
		public ZString ReasonDescription
		{
			get => reasonDescription;
			set
			{
				CheckMaximumLength(ReasonDescriptionInfo, value);
				SetNonPersistentPropertyValue(ReasonDescriptionInfo, ref reasonDescription, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReasonDescription();
				}
			}
		}
		ZString reasonDescription;

		public ZPropertyInfo ReasonDescriptionInfo => GetZPropertyInfo(Schema.ReasonDescription);

		#endregion

		#endregion

		public EPaymentReasonValidation Validation => validation ?? (validation = new EPaymentReasonValidation(this));
		EPaymentReasonValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ProviderCode, ProviderCode);
			writer.WriteElementString(Schema.ReasonCode, ReasonCode);
			writer.WriteElementString(Schema.ReasonDescription, ReasonDescription);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ProviderCode = reader.ReadElementString(Schema.ProviderCode);
			ReasonCode = reader.ReadElementString(Schema.ReasonCode);
			ReasonDescription = reader.ReadElementString(Schema.ReasonDescription);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EPaymentReason();
		}
	}
}
