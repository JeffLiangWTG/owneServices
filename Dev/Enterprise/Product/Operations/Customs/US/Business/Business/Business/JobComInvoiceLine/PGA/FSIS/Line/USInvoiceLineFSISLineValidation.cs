using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class USInvoiceLineFSISLineValidation : USFSISLineValidation
	{
		public USInvoiceLineFSISLineValidation(USInvoiceLineFSISLine parent)
			: base(parent)
		{ }

		new USInvoiceLineFSISLine Parent
		{
			get { return (USInvoiceLineFSISLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			if (!Parent.IsElectronicallyCertificated && Parent.Lots.Count == 0)
			{
				Parent.AddRowMessageError(LotsInformationIsMissing);
			}
		}

		internal readonly string LotsInformationIsMissing = ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.USCLeCERT, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today) ? "All certificates issued in countries where eCert is not available require at least one line. eCert is only available in Australia, New Zealand and Chile." : "All certificates issued in countries where eCert is not available require at least one line. eCert is only available in Australia and New Zealand";
	}
}
