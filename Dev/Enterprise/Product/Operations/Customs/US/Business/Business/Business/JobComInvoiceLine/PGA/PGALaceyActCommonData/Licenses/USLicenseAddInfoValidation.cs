using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USLicenseAddInfoValidation : AutoUSLicenseAddInfoValidation
	{
		public USLicenseAddInfoValidation(AutoUSLicenseAddInfo parent) : base(parent)
		{
		}

		protected override void CheckUS_DateQualifier()
		{
			base.CheckUS_DateQualifier();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_DateQualifierInfo, Parent.Lookups.DateQualifierList);

			if (!Parent.US_Type.IsEmpty && !Parent.US_DateQualifier.IsEmpty)
			{
				if (Parent.US_Type == LaceyActLPCOTypeList.Codes.A01 && Parent.US_DateQualifier != LPCODateQualifierList.Codes.DateIssuedOrSigned)
				{
					Parent.US_DateQualifierInfo.AddMessageError(string.Format(WrongDateQualifier, "3 – Issuance Date", "'A01'"));
				}
				else if (Parent.US_Type != LaceyActLPCOTypeList.Codes.A01 && Parent.US_DateQualifier != LPCODateQualifierList.Codes.ExpirationDate)
				{
					Parent.US_DateQualifierInfo.AddMessageError(string.Format(WrongDateQualifier, "1 – Expiration Date", "other than 'A01'"));
				}
			}
		}
		internal const string WrongDateQualifier = "Date Qualifier should be '{0}' for LPCO Type {1}.";

		protected override void CheckUS_TransType()
		{
			base.CheckUS_TransType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TransTypeInfo, Parent.Lookups.LPCOTransactionTypeList);
		}

		protected override void CheckUS_Type()
		{
			base.CheckUS_Type();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TypeInfo, Parent.Lookups.LPCOTypeList);
		}
	}
}
