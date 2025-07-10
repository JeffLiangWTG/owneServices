using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USExportDEAHeaderAddInfoValidation : USDEAHeaderAddInfoValidation
	{
		public USExportDEAHeaderAddInfoValidation(DEAHeaderAddInfo parent)
			: base(parent)
		{
		}

		new DEAHeaderAddInfo Parent
		{
			get { return (DEAHeaderAddInfo)base.Parent; }
		}

		protected override void CheckUS_PermitNumber()
		{
			base.CheckUS_PermitNumber();

			if (IsDEAIndicatorDeclared)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_PermitNumberInfo);
			}
		}

		protected override void CheckUS_RegistrationNumber()
		{
			base.CheckUS_RegistrationNumber();

			if (IsDEAIndicatorDeclared)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_RegistrationNumberInfo);
			}
		}

		ZBool IsDEAIndicatorDeclared
		{
			get
			{
				var invoiceLine = Parent?.Parent?.InvoiceLine;
				return invoiceLine != null && OGAIndicatorList.IsToBeDeclared(invoiceLine.US_DEAInd);
			}
		}
	}
}
