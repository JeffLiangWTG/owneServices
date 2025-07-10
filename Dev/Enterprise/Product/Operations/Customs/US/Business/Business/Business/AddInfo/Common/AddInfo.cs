using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class AddInfo : AutoUSAddInfo
	{
		#region Constructor

		protected AddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			this.Parent = addInfoProperty.BizObj;
			this.AddInfoProperty = addInfoProperty;
			LoadPropertiesFromAddInfoProperty(false);
			isInitialised = true;
		}

		#endregion

		#region US_TransactionsRelated

		public override ZString US_TransactionsRelated
		{
			get { return base.US_TransactionsRelated; }
			set { base.US_TransactionsRelated = value.ToUpper(); }
		}

		#endregion

		#region US_RoutedTransaction
		public override ZString US_RoutedTransaction
		{
			get { return base.US_RoutedTransaction; }
			set { base.US_RoutedTransaction = value.ToUpper(); }
		}
		#endregion

		#region US_HazardousCargo
		public override ZString US_HazardousCargo
		{
			get { return base.US_HazardousCargo; }
			set { base.US_HazardousCargo = value.ToUpper(); }
		}
		#endregion

		public override ZPropertyInfo US_DDTCITARExemptionNoInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCITARExemptionNo, "DDTC ITAR Exemption Number"); }
		}

		public override ZPropertyInfo US_DDTCMilitaryEquipmentIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCMilitaryEquipmentIndicator, "DDTC Significant Military Equipment Indicator"); }
		}

		public override ZPropertyInfo US_DDTCPartyCertificationIndicatorInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCPartyCertificationIndicator, "DDTC Eligible Party Certification Indicator"); }
		}

		public override ZPropertyInfo US_DDTCQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCQuantity, "DDTC Quantity"); }
		}

		public override ZPropertyInfo US_DDTCRegistrationNoInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCRegistrationNo, "DDTC Registration Number"); }
		}

		public override ZPropertyInfo US_DDTCUnitInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCUnit, "DDTC Unit of Measure"); }
		}

		public override ZPropertyInfo US_DDTCUSMLCategoryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.US_DDTCUSMLCategoryCode, "DDTC USML Category Code"); }
		}

		[LightValidationTestExempt]
		public override ZString US_SchDEntry
		{
			get { return base.US_SchDEntry; }
			set { base.US_SchDEntry = value; }
		}

		public bool IsExport
		{
			get { return IsExportCore; }
		}

		protected abstract bool IsExportCore
		{
			get;
		}

		public bool IsDrawback
		{
			get { return Declaration != null && Declaration.IsDrawback; }
		}

		public bool IsACEDrawback
		{
			get { return Declaration != null && Declaration.IsACEDrawback; }
		}

		public JobDeclaration Declaration
		{
			get
			{
				if (Parent is Customs.Business.IDeclarationProvider)
				{
					return (JobDeclaration)((Customs.Business.IDeclarationProvider)Parent).Declaration;
				}
				return null;
			}
		}

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;

				if (HasChanges && !Parent.IsMarkingAsNeedingValidationSuspended)
				{
					Parent.MarkAsNeedingValidation();
				}
			}
		}

		protected abstract ZString GetTransportMode();

		public void ResetToOriginalValue(SchemaColumn column)
		{
			if (HasChangesSinceLastSaving(column))
			{
				this[column] = DbAddInfo[column];
			}
		}
	}
}
