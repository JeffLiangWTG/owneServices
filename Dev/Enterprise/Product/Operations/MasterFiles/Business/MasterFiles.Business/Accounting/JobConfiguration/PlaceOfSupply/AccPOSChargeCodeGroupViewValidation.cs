//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPOSChargeCodeGroupViewValidation
//
//    This class should be used for overriding validation in AutoAccPOSChargeCodeGroupViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;

	public class AccPOSChargeCodeGroupViewValidation : AutoAccPOSChargeCodeGroupViewValidation
	{
		public AccPOSChargeCodeGroupViewValidation(AutoAccPOSChargeCodeGroupView parent) : base(parent)
		{
		}

		protected override void CheckGRO_GroupType()
		{
			base.CheckGRO_GroupType();
			MandatoryValidation.CheckEntered(Parent.GRO_GroupTypeInfo);
			if (!Parent.GRO_GroupType.EqualsIgnoringCase(AccPOSChargeCodeGroup.POSChargeCodeGroupType))
			{
				Parent.GRO_GroupTypeInfo.AddError(Res.GetString("278b45bc-a2a2-40cb-8c76-3249eb9d628a", "Must be '{0}'.", AccPOSChargeCodeGroup.POSChargeCodeGroupType));
			}
		}

		protected override void CheckGRO_GC()
		{
			base.CheckGRO_GC();
			MandatoryValidation.CheckEntered(Parent.GRO_GCInfo);
			ListValidation.ErrorIfInvalidPK(Parent.GRO_GCInfo);
		}

		protected override void CheckGRO_Code()
		{
			base.CheckGRO_Code();
			MandatoryValidation.CheckEntered(Parent.GRO_CodeInfo);
		}

		protected override void CheckGRO_Description()
		{
			base.CheckGRO_Description();
			MandatoryValidation.CheckEntered(Parent.GRO_DescriptionInfo);
		}
	}
}

