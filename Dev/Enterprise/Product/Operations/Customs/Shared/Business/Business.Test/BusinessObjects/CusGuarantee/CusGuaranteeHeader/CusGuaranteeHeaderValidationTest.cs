using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	class CusGuaranteeHeaderValidationTest : SharedCusPermitHeaderValidationTest<BaseCusGuaranteeHeader>
	{
		public new void TestCheckCPH_UnitOfMeasure()
		{
			CombineAssertions("Check MustBeEntered", () =>
			{
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.VAL;
				PermitHeader.Validation.ValidateCPH_UnitOfMeasure();
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.QTY;
				PermitHeader.Validation.ValidateCPH_UnitOfMeasure();
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
				PermitHeader.CPH_UnitOfMeasure = ZString.Empty;
				AssertHasErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, MandatoryValidation.MustBeEntered);
			});

			CombineAssertions("Check MinLength", () =>
			{
				PermitHeader.CPH_UnitOfMeasure = ZString.Empty;
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
				PermitHeader.CPH_UnitOfMeasure = "K";
				AssertHasErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
				PermitHeader.CPH_UnitOfMeasure = "KG";
				AssertNoErrorContaining(PermitHeader.CPH_UnitOfMeasureInfo, SharedCusPermitHeaderValidation.UnitOfMeasureMinimumLength);
			});
		}

		#region Implementation

		protected override BaseCusGuaranteeHeader GetNewPermitHeader(BusinessObjectFactory factory)
		{
			return Factory.NewWithValidTestData<BaseCusGuaranteeHeader_ForTest>();
		}

		#endregion
	}

	class BaseCusGuaranteeHeader_ForTest : BaseCusGuaranteeHeader
	{
		public BaseCusGuaranteeHeader_ForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override CusPermitHeaderLookups GetNewLookups()
		{
			return new CusGuaranteeHeaderLookups_ForTest(this);
		}

		public override ISharedCountrySpecificInstruction GetCountrySpecificInstruction()
			=> new GuaranteeCountrySpecificInstruction_ForTest(Factory);
	}

	class GuaranteeCountrySpecificInstruction_ForTest : GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction_ForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override AppliesToIndicator GetAppliesToIndicator(ZString permitType, ZString permitSubType)
		{
			switch (permitType)
			{
				case "EPT":
					return AppliesToIndicator.ForceEmpty;
				case "MND":
					return AppliesToIndicator.Mandatory;
				case "OPT":
					return AppliesToIndicator.Optional;
				default:
					return AppliesToIndicator.ForceEmpty;
			}
		}

		public override ZString GetValueFromFieldType(ZString ruleCode)
		{
			return Enum.TryParse(ruleCode, out FieldType fieldType) ? fieldType.ToString() : nameof(FieldType.Text);
		}
	}

	class CusGuaranteeHeaderLookups_ForTest : CusGuaranteeHeaderLookups
	{
		public CusGuaranteeHeaderLookups_ForTest(BaseCusGuaranteeHeader parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList PermitTypes => new CodeDescriptionPairList() {
			new CodeDescriptionPair("IMP", "Basic Import Permit"),
			new CodeDescriptionPair("EXP", "Basic Export Permit")
		};

		public override CodeDescriptionPairList PermitSubTypes => new CodeDescriptionPairList() {
			new CodeDescriptionPair("LVE", "Light Motor Vehicles")
		};
	}
}
