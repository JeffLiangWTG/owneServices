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
	public class MaximumAllowedTransactionAmount : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string MaximumAllowedHeaderAmount = nameof(MaximumAllowedHeaderAmount);
			public const string MaximumAllowedLineAmount = nameof(MaximumAllowedLineAmount);
		}

		#endregion

		public MaximumAllowedTransactionAmount() : base()
		{
		}

		public MaximumAllowedTransactionAmount(FallbackLevel fallbackLevel) : base(fallbackLevel)
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

		public int AmountDecimalPlaces => CurrentLevelCompany?.GetLocalDecimals() ?? 2;

		#region MaximumAllowedHeaderAmount

		[DecimalPlaces(nameof(AmountDecimalPlaces))]
		public ZDecimal MaximumAllowedHeaderAmount
		{
			get => fMaximumAllowedHeaderAmount;
			set
			{
				SetNonPersistentPropertyValue(MaximumAllowedHeaderAmountInfo, ref fMaximumAllowedHeaderAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMaximumAllowedHeaderAmount();
				}
			}
		}
		ZDecimal fMaximumAllowedHeaderAmount;

		public ZPropertyInfo MaximumAllowedHeaderAmountInfo => GetZPropertyInfo(Schema.MaximumAllowedHeaderAmount);

		#endregion

		#region MaximumAllowedLineAmount

		[DecimalPlaces(nameof(AmountDecimalPlaces))]
		public ZDecimal MaximumAllowedLineAmount
		{
			get => fMaximumAllowedLineAmount;
			set
			{
				SetNonPersistentPropertyValue(MaximumAllowedLineAmountInfo, ref fMaximumAllowedLineAmount, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMaximumAllowedLineAmount();
				}
			}
		}
		ZDecimal fMaximumAllowedLineAmount;

		public ZPropertyInfo MaximumAllowedLineAmountInfo => GetZPropertyInfo(Schema.MaximumAllowedLineAmount);

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.MaximumAllowedHeaderAmount, MaximumAllowedHeaderAmount.ToString());
			writer.WriteElementString(Schema.MaximumAllowedLineAmount, MaximumAllowedLineAmount.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			MaximumAllowedHeaderAmount = reader.ReadElementStringAsZDecimal(Schema.MaximumAllowedHeaderAmount);
			MaximumAllowedLineAmount = reader.ReadElementStringAsZDecimal(Schema.MaximumAllowedLineAmount);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new MaximumAllowedTransactionAmount(fallbackLevel);
			return result;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			var valuesToClone = clone as MaximumAllowedTransactionAmount;
			valuesToClone.ShouldCheckMaximumSettingExceedSystemDefined = ShouldCheckMaximumSettingExceedSystemDefined;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public MaximumAllowedTransactionAmountValidation Validation => validation ?? (validation = new MaximumAllowedTransactionAmountValidation(this));
		MaximumAllowedTransactionAmountValidation validation;

		public bool ShouldCheckMaximumSettingExceedSystemDefined { get; set; }
	}
}
