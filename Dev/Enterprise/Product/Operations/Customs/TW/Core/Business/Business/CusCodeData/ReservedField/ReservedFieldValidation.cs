using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class ReservedFieldValidation : Customs.Business.CusCodeDataValidation
	{
		public ReservedFieldValidation(ReservedField parent)
			: base(parent)
		{
		}

		public new ReservedField Parent => (ReservedField)base.Parent;

		protected override void CheckCY_Code()
		{
			var parent = Parent;
			var codeInfo = parent.CY_CodeInfo;
			var code = parent.CY_Code;

			MandatoryValidation.CheckEntered(codeInfo);
			if (!code.IsEmpty)
			{
				if (code.KeepAlphanumericCharacters().ToUpper() != code)
				{
					codeInfo.AddMessageError(ValidationConstants.CusCodeData.CodeIsLettersAndNumbersOnly);
				}
				var supporter = parent?.Parent as IReservedFieldSupporter;
				if (supporter != null)
				{
					CheckDuplicateCode(codeInfo, code, parent.PK, supporter.GetReservedFields());
				}
			}
		}

		void CheckDuplicateCode(ZPropertyInfo codeInfo, ZString code, ZGuid pk, IEnumerable<ReservedField> reservedFields)
		{
			if (reservedFields?.Any(x => x.CY_Code == code && x.PK != pk) ?? false)
			{
				codeInfo.AddMessageError(ValidationConstants.CusCodeData.ReservedFieldDuplicated);
			}
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
		}
	}
}
