using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class APHISIdentityValidation : Customs.Business.CusCodeDataValidation
	{
		public APHISIdentityValidation(APHISIdentity regoNumber)
			: base(regoNumber)
		{
		}

		public new APHISIdentity Parent
		{
			get { return (APHISIdentity)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUseMultipleNumbers();
		}

		public void ValidateUseMultipleNumbers()
		{
			ValidateCalculatedProperty(Parent.UseMultipleNumbersInfo);
		}

		protected void CheckUseMultipleNumbers()
		{
			if (Parent.UseMultipleNumbers && Parent.NumberRanges.Count == 0 && IsPGAValidation)
			{
				Parent.UseMultipleNumbersInfo.AddMessageError(ValidationConstants.APHIS.AtLeastOneNumberRangeIsRequired);
			}
		}

		protected override void CheckCY_Code()
		{
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo, Parent.Lookups.CY_CodeList);
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			if (IsPGAValidation)
			{
				if (!Parent.UseMultipleNumbers && Parent.CY_Data.IsEmpty)
				{
					Parent.CY_DataInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Identification Number"));
				}
				else if (Parent.CY_Data.Length > USAPHISIdentityNumberRangeAddInfoSchema.US_StartNumber.MaxLength)
				{
					Parent.CY_DataInfo.AddWarning(ValidationConstants.APHIS.IdentityNumberMaxLengthWarningMessage);
				}
			}
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var product = Parent.Product;
				if (product != null)
				{
					var header = product.Header;
					var invoiceLine = header == null ? null : header.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
