using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USExportTTBLineAddInfoValidation : USTTBLineAddInfoValidation
	{
		public USExportTTBLineAddInfoValidation(USTTBLineAddInfo parent)
			: base(parent)
		{
		}

		new USTTBLineAddInfo Parent
		{
			get { return (USTTBLineAddInfo)base.Parent; }
		}

		protected override void CheckUS_Date()
		{
			base.CheckUS_Date();

			if (IsTTBIndicatorDeclaredOrDisclaimed)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_DateInfo);
			}
		}

		protected override void CheckUS_SerialNumber()
		{
			base.CheckUS_SerialNumber();

			if (IsTTBIndicatorDeclaredOrDisclaimed)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_SerialNumberInfo);
			}
		}

		ZBool IsTTBIndicatorDeclaredOrDisclaimed
		{
			get
			{
				var invoiceLine = Parent?.Parent?.InvoiceLine;
				return invoiceLine != null && OGAIndicatorList.IsToBeDeclaredOrDisclaimed(invoiceLine.US_TTBInd);
			}
		}
	}
}
