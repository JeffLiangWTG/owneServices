using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationDuplicateValidation : Customs.Business.CusCodeDataValidation
	{
		public DeclarationDuplicateValidation(DeclarationDuplicate parent)
			: base(parent)
		{
		}

		protected new DeclarationDuplicate Parent => (DeclarationDuplicate)base.Parent;

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
			if (!Parent?.CY_Code.IsEmpty ?? false)
			{
				if (Parent.Parent?.DeclarationDuplicates?.Cast<DeclarationDuplicate>().Any(x => x.CY_Code == Parent.CY_Code && x.PK != Parent.PK) ?? false)
				{
					Parent.CY_CodeInfo.AddMessageError(ValidationConstants.EntryInstruction.DeclarationDuplicateDuplicated);
				}
			}
		}

		protected override void CheckCY_Data()
		{
			if (Parent.Copy < 1 || Parent.Copy > 99)
			{
				Parent.CY_DataInfo.AddError(Res.GetString("7D7615D9-09CF-4566-A6DC-F0BE0B85D6C6", "This number is invalid."));
			}
		}
	}
}
