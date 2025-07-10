using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.Business
{
	public class GovernmentUniformInvoiceDataValidation : Customs.Business.CusCodeDataValidation
	{
		public GovernmentUniformInvoiceDataValidation(GovernmentUniformInvoiceData parent)
			: base(parent)
		{
		}

		protected new GovernmentUniformInvoiceData Parent => (GovernmentUniformInvoiceData)base.Parent;

		protected override void CheckCY_Code()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_CodeInfo);
			if (!Parent?.CY_Code.IsEmpty ?? false)
			{
				if (Parent.Parent?.GovernmentUniformInvoices?.Cast<GovernmentUniformInvoiceData>().Any(x => x.CY_Code == Parent.CY_Code && x.PK != Parent.PK) ?? false)
				{
					Parent.CY_CodeInfo.AddMessageError(Res.GetString("8BC7FD6E-C82E-4FA3-8AF5-126C56CE0920", "This Number is duplicated."));
				}
			}
		}

		protected override void CheckCY_Data()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_DataInfo);
		}
	}
}
