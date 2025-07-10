using System.Linq;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryLineValidation : AutoZACusEntryLineValidation
	{
		public CusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDiamondLevyValueAndAmount();
			ValidateNotMoreThanTenAdditionalInfos();
		}

		protected override void CheckCL_LineNumber()
		{
			base.CheckCL_LineNumber();

			var entryLineNumber = Parent.CL_LineNumber;
			if (entryLineNumber > 9999)
			{
				Parent.CL_LineNumberInfo.AddMessageError(ValidationConstants.Shared.EntryLineNumberExceedMax);
			}
		}

		internal void ValidateDiamondLevyValueAndAmount()
		{
			var entryLine = Parent as CusEntryLine;
			entryLine.RemoveRowMessageError(DiamondLevyValueAndAmountRequiredError);
			if (entryLine.Header.IsExport)
			{
				var hasDLA = entryLine.ProvisionalPayments.OfType<ProvisionalPaymentAmountCodeData>().Any(x => x.CY_Code == LineLevelProvisionalPaymentsForExports.Codes.DLA);
				var hasDLV = entryLine.AdditionalInformationCodes.OfType<AdditionalInformation>().Any(x => x.CY_Code == UniversalReferenceConstants.AdditionalInformation.DiamondLevyValue);

				if (hasDLA ^ hasDLV)
				{
					entryLine.AddRowMessageError(DiamondLevyValueAndAmountRequiredError);
				}
			}
		}

		internal void ValidateNotMoreThanTenAdditionalInfos()
		{
			var entryLine = Parent as CusEntryLine;
			entryLine.RemoveRowMessageError(NoMoreThanTenAdditionalInfos);
			if (entryLine.AdditionalInformationCodes.Count > 10)
			{
				entryLine.AddRowMessageError(NoMoreThanTenAdditionalInfos);
			}
		}

		public static string DiamondLevyValueAndAmountRequiredError => ResString.GetMultilingualString("11C15007-E7D7-4A2B-A7D5-0A24D317EC58", "Diamond levy value and amounts are required");
		public static string NoMoreThanTenAdditionalInfos => ResString.GetMultilingualString("088ADE3E-45DE-4F57-AC29-DF74B2876A03", "Cannot have more than 10 Additional Information elements");
	}
}
