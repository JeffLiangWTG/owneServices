using System;
using System.ComponentModel;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class DefaultEPaymentReason : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ProviderCode = nameof(ProviderCode);
			public const string ReasonCode = nameof(ReasonCode);
			public const string ReasonDescription = nameof(ReasonDescription);
		}

		#endregion

		public DefaultEPaymentReason() : base()
		{
		}

		public DefaultEPaymentReason(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Bound Properties

		#region ProviderCode

		[List(nameof(PaymentProviderCodes))]
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

		#region ReasonCode

		[List(nameof(ReasonCodeForSelectedProvider))]
		[MaxLength(3)]
		[ResourceStringData("fc9ef26d-24a8-4c9d-9b76-e9e6b0e0bb33", Caption = "Code")]
		public ZString ReasonCode
		{
			get => reasonCode;
			set
			{
				CheckMaximumLength(ReasonCodeInfo, value);
				SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value);
				ReasonDescription = ReasonCodeForSelectedProvider.GetDescriptionFromCode(ReasonCode);
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
		[ResourceStringData("d4bea338-cca7-4b0a-974b-3d07ce4e3c35", Caption = "Description")]
		[ReadOnly(true)]
		public ZString ReasonDescription
		{
			get => reasonDescription;
			set
			{
				CheckMaximumLength(ReasonDescriptionInfo, value);
				SetNonPersistentPropertyValue(ReasonDescriptionInfo, ref reasonDescription, value);
			}
		}
		ZString reasonDescription;

		public ZPropertyInfo ReasonDescriptionInfo => GetZPropertyInfo(Schema.ReasonDescription);
		#endregion

		#endregion
		public DefaultEPaymentReasonValidation Validation => validation ?? (validation = new DefaultEPaymentReasonValidation(this));
		DefaultEPaymentReasonValidation validation;

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
			var result = new DefaultEPaymentReason(fallbackLevel);
			return result;
		}

		public ICodeDescriptionPairList ReasonCodeForSelectedProvider
		{
			get
			{
				var reasons = new CodeDescriptionPairList();
				var companyPK = CurrentFallbackLevel?.CompanyPK(false) ?? Guid.Empty;
				var reasonsForAllProviders = AccountingMasterFilesRegistry.Instance.PaymentReasons.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
				var reasonsForSelectedProvider = reasonsForAllProviders.Cast<EPaymentReason>().Where(x => x.ProviderCode == ProviderCode);
				reasonsForSelectedProvider.ForEach(r => reasons.AddPair(r.ReasonCode, r.ReasonDescription));
				return reasons;
			}
		}
	}
}
