using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class ExchangeRateCurrencyConfiguration : AccJobConfigPivot
	{
		public ExchangeRateCurrencyConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Properties

		#region JCT_JCF_JobConfig

		[RelatedBusinessObject(nameof(AccExRateConfiguration))]
		public override ZGuid JCT_JCF_JobConfig
		{
			get => base.JCT_JCF_JobConfig;
			set => base.JCT_JCF_JobConfig = value;
		}

		public AccExchangeRateConfiguration AccExRateConfiguration => Factory.Load<AccExchangeRateConfiguration>(JCT_JCF_JobConfig);

		#endregion

		#region JCT_Code / Currency

		[ResourceStringData("02D83C12-0C5C-45A9-9C3E-9111314E9F48", Caption = "Currency", FullDescription = "Enter or select the Currencies this configuration applies to.")]
		[MaxLength(3)]
		[List("Lookups.Currencies")]
		[ReadOnlyMember(nameof(IsExRateConfigCurrencyTypeAll))]
		public override ZString JCT_Code
		{
			get => base.JCT_Code;
			set => base.JCT_Code = value;
		}

		#endregion

		#region JCT_ExRateType

		[ResourceStringData("96CE2A77-5262-4556-ACE3-E89AA14E7C45", Caption = "Ex Rate Type")]
		[MaxLength(3)]
		[List("Lookups.ExRateTypeList")]
		public override ZString JCT_ExRateType
		{
			get => base.JCT_ExRateType;
			set => base.JCT_ExRateType = value;
		}

		#endregion

		#region JCT_StartDate

		[ResourceStringData("B5348A37-7E00-4639-AFC1-C86787243F6E", Caption = "Start Date")]
		[ReadOnlyMember(nameof(IsExRateConfigSystemDefaultSavedInDB))]
		public override ZDate JCT_StartDate
		{
			get => base.JCT_StartDate;
			set => base.JCT_StartDate = value;
		}

		#endregion

		#region JCT_ExpiryDate

		[ResourceStringData("653977D7-6670-4C21-84C4-99261E82FA21", Caption = "Expiry Date")]
		[ReadOnlyMember(nameof(IsExRateConfigSystemDefaultSavedInDB))]
		public override ZDate JCT_ExpiryDate
		{
			get => base.JCT_ExpiryDate;
			set => base.JCT_ExpiryDate = value;
		}

		#endregion

		public AccExRateConfigurationLevelEnum Level => AccExRateConfiguration?.Level ?? AccExRateConfigurationLevelEnum.None;

		public ExchangeRateType ExchangeRateType => Enterprise.ZArchitecture.Environment.ExchangeRate.GetExchangeRateType(JCT_ExRateType);

		#endregion

		#region Validation

		protected override AccJobConfigPivotValidation GetNewValidation() => new ExchangeRateCurrencyConfigurationValidation(this);

		#endregion

		#region Overrides

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (!Globals.IsUserInteractive)
				{
					return base.UniqueIndexFailureHandlers;
				}

				var handlers = new List<IUniqueIndexFailureHandler>();
				handlers.Add(new ExchangeRateCurrencyConfigurationUniqueIndexFailureHandler());
				return handlers;
			}
		}

		public override bool ReadOnly => AccExRateConfiguration?.ReadOnly ?? base.ReadOnly;

		protected override ZString HumanReadableNameCore => Res.GetString("F6E3AFE4-32DA-48D1-975B-02C2EAED65FD", "Job Billing Exchange Rate Currency");

		public override bool CanDelete => base.CanDelete && !IsExRateConfigSystemDefaultSavedInDB;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsExRateConfigSystemDefaultSavedInDB)
				{
					return ResString.GetMultilingualString("9aff7f88-6c13-42bc-91d0-3b797c7e0e1c", "A currency configuration for ALL job types is mandatory and cannot be deleted.");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

		public bool IsExRateConfigCurrencyTypeAll => AccExRateConfiguration.IsCurrencyTypeAll;

		bool IsExRateConfigSystemDefaultSavedInDB => AccExRateConfiguration.IsSystemDefaultSavedInDB && JCT_Code.IsEmpty && !HasDateRange && IsInDatabase;

		public bool HasDateRange => !JCT_StartDate.IsEmpty && !JCT_ExpiryDate.IsEmpty;

		#endregion

		#region Implementation

		public bool IsDuplicateOf(ExchangeRateCurrencyConfiguration other)
		{
			if (PK == other.PK)
			{
				return false;
			}

			if (other.JCT_JCF_JobConfig.Equals(JCT_JCF_JobConfig))
			{
				return JCT_Code == other.JCT_Code
					&& JCT_StartDate == other.JCT_StartDate
					&& JCT_ExpiryDate == other.JCT_ExpiryDate;
			}

			return JCT_Code == other.JCT_Code;
		}

		#endregion
	}
}
