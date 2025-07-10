using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class AddInfoCusEntryInstructionValidation : EU.Business.Declaration.AddInfoCusEntryInstructionValidation
	{
		public AddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
		{
		}

		public new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

		protected override void CheckZG_ExportUnionSecretaryCode()
		{
			base.CheckZG_ExportUnionSecretaryCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_ExportUnionSecretaryCodeInfo);
		}

		protected override void CheckZG_ExportUnionCode()
		{
			base.CheckZG_ExportUnionCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_ExportUnionCodeInfo);
		}

		protected override void CheckZG_ExportUnionCountryCode()
		{
			base.CheckZG_ExportUnionCountryCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_ExportUnionCountryCodeInfo);
		}

		protected override void CheckZG_InlandTransportType()
		{
			base.CheckZG_InlandTransportType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_InlandTransportTypeInfo);
		}
	}
}
