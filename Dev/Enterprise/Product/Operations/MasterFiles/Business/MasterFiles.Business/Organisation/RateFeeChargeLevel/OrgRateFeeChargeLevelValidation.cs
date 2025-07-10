//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgRateFeeChargeLevelValidation
//
//    This class should be used for overriding validation in AutoOrgRateFeeChargeLevelValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class OrgRateFeeChargeLevelValidation : AutoOrgRateFeeChargeLevelValidation
	{
		public OrgRateFeeChargeLevelValidation(AutoOrgRateFeeChargeLevel parent)
			: base(parent)
		{
		}

		protected override void CheckORF_ServiceType()
		{
			base.CheckORF_ServiceType();

			MandatoryValidation.CheckEntered(Parent.ORF_ServiceTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ORF_ServiceTypeInfo);

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.ORF_ServiceTypeInfo,
				Parent.Factory.Load<OrgRateFeeChargeLevel>(
				new ZQuery(OrgRateFeeChargeLevelSchema.ORF_OH, Parent.ORF_OH)), Res.GetString("08ba31a0-da0e-4e23-8346-15812b61d584", "The same type can only be configured once."));
		}

		protected override void CheckORF_Level()
		{
			base.CheckORF_Level();

			MandatoryValidation.CheckEntered(Parent.ORF_LevelInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ORF_LevelInfo);
		}

		protected override void CheckORF_Amount1Type()
		{
			base.CheckORF_Amount1Type();

			MandatoryValidation.CheckEntered(Parent.ORF_Amount1TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ORF_Amount1TypeInfo);
		}

		protected override void CheckORF_Amount2Type()
		{
			base.CheckORF_Amount2Type();

			MandatoryValidation.CheckEntered(Parent.ORF_Amount2TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ORF_Amount2TypeInfo);
		}

		protected override void CheckORF_RX_NKAmount1Currency()
		{
			base.CheckORF_RX_NKAmount1Currency();

			if (!Parent.ORF_Amount1Type.Equals(OrgConstants.ServiceLevelAmountTypes.Code.None))
			{
				MandatoryValidation.CheckEntered(Parent.ORF_RX_NKAmount1CurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ORF_RX_NKAmount1CurrencyInfo);
			}
		}

		protected override void CheckORF_RX_NKAmount2Currency()
		{
			base.CheckORF_RX_NKAmount2Currency();

			if (!Parent.ORF_Amount2Type.Equals(OrgConstants.ServiceLevelAmountTypes.Code.None))
			{
				MandatoryValidation.CheckEntered(Parent.ORF_RX_NKAmount2CurrencyInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ORF_RX_NKAmount2CurrencyInfo);
			}
		}
	}
}
