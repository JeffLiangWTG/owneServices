//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccPOSChargeCodeGroupPivotViewValidation
//
//    This class should be used for overriding validation in AutoAccPOSChargeCodeGroupPivotViewValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Schema;

	public class AccPOSChargeCodeGroupPivotViewValidation : AutoAccPOSChargeCodeGroupPivotViewValidation
	{
		public AccPOSChargeCodeGroupPivotViewValidation(AutoAccPOSChargeCodeGroupPivotView parent) : base(parent)
		{
		}

		protected override void CheckGRP_GroupType()
		{
			base.CheckGRP_GroupType();
			MandatoryValidation.CheckEntered(Parent.GRP_GroupTypeInfo);
			if (!Parent.GRP_GroupType.EqualsIgnoringCase(AccPOSChargeCodeGroup.POSChargeCodeGroupType))
			{
				Parent.GRP_GroupTypeInfo.AddError(Res.GetString("f5a6a99d-f8d0-431c-961d-ea7ec0456efb", "Must be '{0}'.", AccPOSChargeCodeGroup.POSChargeCodeGroupType));
			}
		}

		protected override void CheckGRP_GRO_Group()
		{
			base.CheckGRP_GRO_Group();
			MandatoryValidation.CheckEntered(Parent.GRP_GRO_GroupInfo);
			ListValidation.ErrorIfInvalidPK(Parent.GRP_GRO_GroupInfo);
		}

		protected override void CheckGRP_MemberTableCode()
		{
			base.CheckGRP_MemberTableCode();
			MandatoryValidation.CheckEntered(Parent.GRP_MemberTableCodeInfo);
			if (!Parent.GRP_MemberTableCode.EqualsIgnoringCase(AccChargeCodeSchema.Constants.Prefix))
			{
				Parent.GRP_MemberTableCodeInfo.AddError(Res.GetString("278b45bc-a2a2-40cb-8c76-3249eb9d628a", "Must be '{0}'.", AccChargeCodeSchema.Constants.Prefix));
			}
		}

		protected override void CheckGRP_MemberID()
		{
			base.CheckGRP_MemberID();
			MandatoryValidation.CheckEntered(Parent.GRP_MemberIDInfo);
			ListValidation.ErrorIfInvalidPK(Parent.GRP_MemberIDInfo);
		}
	}
}

