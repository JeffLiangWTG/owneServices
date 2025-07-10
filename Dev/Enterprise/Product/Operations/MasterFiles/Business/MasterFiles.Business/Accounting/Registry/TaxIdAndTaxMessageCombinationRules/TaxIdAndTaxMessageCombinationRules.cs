using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxIdAndTaxMessageCombinationRules : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string LineType = "LineType";
			public const string TaxRate = "TaxRate";
			public const string TaxMessage = "TaxMessage";
			public const string TaxGroupCode = "TaxGroupCode";
			public const string TaxGroupDescription = "TaxGroupDescription";
			public const string GovernmentCode = "GovernmentCode";
			public const string TaxRateValue = "TaxRateValue";
			public const string TaxRateType = "TaxRateType";
			public const string AuxiliaryType = "AuxiliaryType";
			public const string ExtraTaxRate = "ExtraTaxRate";
		}

		#endregion Schema

		public TaxIdAndTaxMessageCombinationRules() { }

		TaxIdAndTaxMessageCombinationRules(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		GlbCompany Company
		{
			get
			{
				if (CurrentFallbackLevel != null && company == null)
				{
					company = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
				}
				return company;
			}
		}
		GlbCompany company;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxIdAndTaxMessageCombinationRules(fallbackLevel);
		}

		public virtual TaxIdAndTaxMessageCombinationRulesCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (TaxIdAndTaxMessageCombinationRulesCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		[List("Lookups.LineTypes")]
		public ZString LineType
		{
			get => lineType;
			set
			{
				SetNonPersistentPropertyValue(LineTypeInfo, ref lineType, value);
				if (!IsValidationSuspended)
				{
					ValidateLineType();
				}
			}
		}
		ZString lineType = ZString.Empty;

		public ZPropertyInfo LineTypeInfo
		{
			get { return GetZPropertyInfo(Schema.LineType); }
		}

		[List("Lookups.TaxRates")]
		public ZGuid TaxRate
		{
			get => taxRate;
			set
			{
				SetNonPersistentPropertyValue(TaxRateInfo, ref taxRate, value);
				accTaxRate = null;

				if (!IsValidationSuspended)
				{
					ValidateTaxRate();
					ValidateGovernmentCodeHasDifferentTaxRate();
				}
			}
		}
		ZGuid taxRate = ZGuid.Empty;

		public ZPropertyInfo TaxRateInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRate); }
		}

		AccTaxRate AccTaxRate => accTaxRate ?? (accTaxRate = CurrentFactory.Load<AccTaxRate>(TaxRate));
		AccTaxRate accTaxRate;

		[List("Lookups.TaxMessages")]
		public ZGuid TaxMessage
		{
			get => taxMessage;
			set
			{
				SetNonPersistentPropertyValue(TaxMessageInfo, ref taxMessage, value);
				if (!IsValidationSuspended)
				{
					ValidateTaxMessage();
					ValidateGovernmentCodeHasDifferentTaxRate();
				}
				invoiceTaxMessage = null;
				taxMessageGroup = null;
			}
		}
		ZGuid taxMessage = ZGuid.Empty;

		public ZPropertyInfo TaxMessageInfo
		{
			get { return GetZPropertyInfo(Schema.TaxMessage); }
		}

		public ZString TaxGroupCode => InvoiceTaxMessage?.A9_TaxGroupCode ?? ZString.Empty;

		public ZPropertyInfo TaxGroupCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxGroupCode); }
		}

		public ZString TaxGroupDescription => TaxMessageGroup?.Description ?? ZString.Empty;

		public ZPropertyInfo TaxGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.TaxGroupDescription); }
		}

		public ZString GovernmentCode => TaxMessageGroup?.RelatedItemCode ?? ZString.Empty;

		public ZPropertyInfo GovernmentCodeInfo
		{
			get { return GetZPropertyInfo(Schema.GovernmentCode); }
		}

		CodeDescriptionBoolRelatedItem TaxMessageGroup
		{
			get
			{
				if (taxMessageGroup == null)
				{
					var regValues = AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(CurrentFallbackLevel?.CompanyPK(false) ?? GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					taxMessageGroup = regValues?.FindByCode(TaxGroupCode) as CodeDescriptionBoolRelatedItem;
				}
				return taxMessageGroup;
			}
		}
		CodeDescriptionBoolRelatedItem taxMessageGroup;

		public ZString TaxRateType => AccTaxRate != null ? AccTaxRate.AT_Type : ZString.Empty;

		public ZPropertyInfo TaxRateTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRateType); }
		}

		public ZDecimal TaxRateValue => AccTaxRate != null ? AccTaxRate.RateForTodayForUIBinding : 0;

		public ZPropertyInfo TaxRateValueInfo
		{
			get { return GetZPropertyInfo(Schema.TaxRateValue); }
		}

		public ZString AuxiliaryType => AccTaxRate != null ? AccTaxRate.AT_ExtraTaxRateType : ZString.Empty;

		public ZPropertyInfo AuxiliaryTypeInfo
		{
			get { return GetZPropertyInfo(Schema.AuxiliaryType); }
		}

		public ZDecimal ExtraTaxRate => AccTaxRate != null ? AccTaxRate.ExtraRateForTodayForUIBinding : 0;

		public ZPropertyInfo ExtraTaxRateInfo
		{
			get { return GetZPropertyInfo(Schema.ExtraTaxRate); }
		}

		AccInvMsg InvoiceTaxMessage => invoiceTaxMessage ?? (invoiceTaxMessage = CurrentFactory.Load<AccInvMsg>(TaxMessage));
		AccInvMsg invoiceTaxMessage;

		public TaxIdAndTaxMessageCombinationRulesLookups Lookups
		{
			get
			{
				var countryCode = Company?.GC_RN_NKCountryCode ?? ZString.Empty;

				if (lookups == null || lookups.CountryCode != countryCode)
				{
					lookups = new TaxIdAndTaxMessageCombinationRulesLookups(countryCode);
				}

				return lookups;
			}
		}
		TaxIdAndTaxMessageCombinationRulesLookups lookups;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.LineType, LineType);
			writer.WriteElementString(Schema.TaxRate, TaxRate.ToString());
			writer.WriteElementString(Schema.TaxMessage, TaxMessage.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			LineType = reader.ReadElementString(Schema.LineType);
			TaxRate = new ZGuid(reader.ReadElementString(Schema.TaxRate));
			TaxMessage = new ZGuid(reader.ReadElementString(Schema.TaxMessage));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			using (ValidateDuplicatedSuspender.GetSuspender())
			{
				ValidateLineType();
				ValidateTaxRate();
				ValidateTaxMessage();
				ValidateGovernmentCodeHasDifferentTaxRate();
			}
			ValidateDuplicated();
		}

		void ValidateLineType()
		{
			LineTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(LineTypeInfo);

			if (Company != null)
			{
				ListValidation.ErrorIfInvalidCode(LineTypeInfo, Lookups.LineTypes);
			}

			ValidateDuplicated();
		}

		void ValidateTaxRate()
		{
			TaxRateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxRateInfo);

			if (Company != null)
			{
				ListValidation.ErrorIfInvalidPK(TaxRateInfo, Lookups.TaxRates);
			}

			ValidateDuplicated();
		}

		void ValidateTaxMessage()
		{
			TaxMessageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TaxMessageInfo);

			if (Company != null)
			{
				ListValidation.ErrorIfInvalidPK(TaxMessageInfo, Lookups.TaxMessages);
			}

			ValidateDuplicated();
		}

		void ValidateDuplicated()
		{
			if (ValidateDuplicatedSuspender.IsSuspended)
			{
				return;
			}

			var errorMessage = Res.GetString("7BEB3CF8-06B3-4488-9DE1-3ABFD971D451", "This row has been duplicated and must be unique.");
			ClearRowNotificationsContaining(errorMessage);
			using (ValidateDuplicatedSuspender.GetSuspender())
			{
				if (!HasErrors && CheckDuplicated())
				{
					AddRowError(errorMessage);
				}
			}

			bool CheckDuplicated()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<TaxIdAndTaxMessageCombinationRules>()
					.Except(new[] { this })
					.Any(item => item.LineType == LineType && item.TaxRate == TaxRate && item.TaxMessage == TaxMessage);
			}
		}

		void ValidateGovernmentCodeHasDifferentTaxRate()
		{
			var errorMessage = Res.GetString("4BB3E563-C666-4A1D-A2F2-886E5772A03A", "All combinations with the same Government Code must have the same Tax Rate.");
			ClearRowNotificationsContaining(errorMessage);

			if (!HasErrors && CheckGovernmentCodeHasDifferentTaxRate())
			{
				AddRowError(errorMessage);
			}

			bool CheckGovernmentCodeHasDifferentTaxRate()
			{
				if (ParentCollection == null || string.IsNullOrEmpty(GovernmentCode))
				{
					return false;
				}

				return ParentCollection.OfType<TaxIdAndTaxMessageCombinationRules>()
					.Except(new[] { this })
					.Any(item => item.GovernmentCode == GovernmentCode && (item.TaxRateValue != TaxRateValue || item.ExtraTaxRate != ExtraTaxRate));
			}
		}

		public FunctionalitySuspender ValidateDuplicatedSuspender => validateDuplicatedSuspender ?? (validateDuplicatedSuspender = new FunctionalitySuspender());
		FunctionalitySuspender validateDuplicatedSuspender;
	}
}
