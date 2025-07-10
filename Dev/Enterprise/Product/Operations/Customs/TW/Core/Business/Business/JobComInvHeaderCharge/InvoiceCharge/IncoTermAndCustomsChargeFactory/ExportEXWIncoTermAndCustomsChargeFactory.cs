using System.Collections.Generic;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business
{
	class ExportExWorksIncoTermAndCustomsChargeFactory : ExportIncoTermAndCustomsChargeFactory
	{
		protected override void SetupExWorksConfiguration()
		{
			base.SetupExWorksConfiguration();

			var incoTerm = Core.Constants.IncoTerms.ExWorks;
			AddChargeConfiguration(incoTerm, ExWorks, new ChargeConfiguration() { IsIncludedInInvoiceAmountFixed = true, IsIncludedInInvoice = false, IsMandatory = false, IsRecommended = true });
		}

		public static CustomsChargeCode ExWorks => TWCustomsChargeCodeProvider.ExWorks;

		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			result.Add(ExWorks);
			return result.ToArray();
		}
	}
}
