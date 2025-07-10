using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Business
{
	public class AddInfoJobDeclarationValidation_TNP : AddInfoCUSDECValidation
	{
		public AddInfoJobDeclarationValidation_TNP(AddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected override void CheckSG_RemovalStartDate()
		{
			base.CheckSG_RemovalStartDate();
			if (Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.REM || Declaration.JE_MessageSubType == DeclarationTypeCodeList.Codes.BRE)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RemovalStartDateInfo, "Start Date Cargo Removal Period");
			}
		}

		protected override void CheckSG_RN_NKFinalDestination()
		{
			base.CheckSG_RN_NKFinalDestination();
			if (!Parent.Declaration.IsSeaStore)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.SG_RN_NKFinalDestinationInfo, "Country/Region of Final Destination. For outward transport, except seastore permits application, specify Country/Region of Final Destination.");
			}
		}
	}
}
