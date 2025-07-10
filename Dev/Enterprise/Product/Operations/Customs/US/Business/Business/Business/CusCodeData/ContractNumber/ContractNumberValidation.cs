using System.Text.RegularExpressions;

namespace Enterprise.Customs.US.Business
{
	public class ContractNumberValidation : Customs.Business.CusCodeDataValidation
	{
		public ContractNumberValidation(ContractNumber bizObj)
			: base(bizObj)
		{
			declaration = bizObj.Parent as JobDeclaration;
		}
		readonly JobDeclaration declaration;

		protected override void CheckCY_Code()
		{
			//do not want to validate
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (!Parent.CY_Data.IsEmpty && Parent.CY_Data != "PENDING" && !Regex.IsMatch(Parent.CY_Data, @"^[0-9]{2}-[0-9]{5}-[0-9]{3}$"))
			{
				Parent.CY_DataInfo.AddMessageError(InvalidContractNumberMessage);
			}
			if (declaration != null)
			{
				declaration.AddInfoValidation.ValidateUS_EntryType();
			}
		}
		internal const string InvalidContractNumberMessage = "The Contract Number has an invalid format. Contract numbers must be in NN-NNNNN-NNN format (including dashes), or the word 'PENDING'.";
	}
}
