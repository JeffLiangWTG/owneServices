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
	public class GLJournalExchangeRateType : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string BalanceSheetAccountTypeExchangeRateType = nameof(BalanceSheetAccountTypeExchangeRateType);
			public const string ProfitAndLossAccountTypeExchangeRateType = nameof(ProfitAndLossAccountTypeExchangeRateType);
		}

		#endregion

		public GLJournalExchangeRateType() : base()
		{
		}

		public GLJournalExchangeRateType(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		#region Bound Properties

		GlbCompany CurrentLevelCompany
		{
			get
			{
				if (CurrentFallbackLevel != null)
				{
					var currentFallbackCompanyPK = CurrentFallbackLevel.CompanyPK(false);
					if (fCurrentLevelCompany == null || fCurrentLevelCompany.PK != currentFallbackCompanyPK)
					{
						fCurrentLevelCompany = CurrentFactory.Load<GlbCompany>(CurrentFallbackLevel.CompanyPK(false));
					}
				}

				return fCurrentLevelCompany;
			}
		}

		GlbCompany fCurrentLevelCompany;

		public GLJournalExchangeRateTypeLookups Lookups
		{
			get
			{
				var companyPK = CurrentLevelCompany?.PK ?? GlbCompany.CurrentCompany.PK;

				if (lookups == null || lookups.CompanyPK != companyPK)
				{
					lookups = new GLJournalExchangeRateTypeLookups(companyPK);
				}

				return lookups;
			}
		}
		GLJournalExchangeRateTypeLookups lookups;

		#region BalanceSheetAccountTypeExchangeRateType

		[List("Lookups.ExchangeRateTypes")]
		public ZString BalanceSheetAccountTypeExchangeRateType
		{
			get => fBalanceSheetAccountTypeExchangeRateType;
			set
			{
				SetNonPersistentPropertyValue(BalanceSheetAccountTypeExchangeRateTypeInfo, ref fBalanceSheetAccountTypeExchangeRateType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBalanceSheetAccountTypeExchangeRateType();
				}
			}
		}
		ZString fBalanceSheetAccountTypeExchangeRateType;

		public ZPropertyInfo BalanceSheetAccountTypeExchangeRateTypeInfo => GetZPropertyInfo(Schema.BalanceSheetAccountTypeExchangeRateType);

		#endregion

		#region ProfitAndLossAccountTypeExchangeRateType

		[List("Lookups.ExchangeRateTypes")]
		public ZString ProfitAndLossAccountTypeExchangeRateType
		{
			get => fProfitAndLossAccountTypeExchangeRateType;
			set
			{
				SetNonPersistentPropertyValue(ProfitAndLossAccountTypeExchangeRateTypeInfo, ref fProfitAndLossAccountTypeExchangeRateType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateProfitAndLossAccountTypeExchangeRateType();
				}
			}
		}
		ZString fProfitAndLossAccountTypeExchangeRateType;

		public ZPropertyInfo ProfitAndLossAccountTypeExchangeRateTypeInfo => GetZPropertyInfo(Schema.ProfitAndLossAccountTypeExchangeRateType);

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.BalanceSheetAccountTypeExchangeRateType, BalanceSheetAccountTypeExchangeRateType.ToString());
			writer.WriteElementString(Schema.ProfitAndLossAccountTypeExchangeRateType, ProfitAndLossAccountTypeExchangeRateType.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			BalanceSheetAccountTypeExchangeRateType = reader.ReadElementString(Schema.BalanceSheetAccountTypeExchangeRateType);
			ProfitAndLossAccountTypeExchangeRateType = reader.ReadElementString(Schema.ProfitAndLossAccountTypeExchangeRateType);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new GLJournalExchangeRateType(fallbackLevel);
			return result;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var valuesToClone = clone as GLJournalExchangeRateType;
			valuesToClone.ShouldCheckMaximumSettingExceedSystemDefined = ShouldCheckMaximumSettingExceedSystemDefined;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public GLJournalExchangeRateTypeValidation Validation => validation ?? (validation = new GLJournalExchangeRateTypeValidation(this));
		GLJournalExchangeRateTypeValidation validation;

		public bool ShouldCheckMaximumSettingExceedSystemDefined { get; set; }
	}
}
