using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class FZUnconcurrenceMessageBuilder : FZMessageBuilder
	{
		public FZUnconcurrenceMessageBuilder(FZEventAction action)
			: base(action.EventHeader)
		{
			this.action = action;
		}
		readonly FZEventAction action;

		protected override IEnumerable<MessageBlock> Build()
		{
			yield return GenerateFZ10WithMandatoryFields(1, Header.FTZAdmissionNumber, FTZActionCodeList.Codes.J);
			yield return GenerateFZ13(action.US_FTZContactName, action.US_FTZContactPhone, action.US_ReasonCode);
			if (!action.US_Reasons.IsEmpty)
			{
				var referenceQualifier = ZString.Empty;
				switch (action.US_ReasonCode)
				{
					case "02":
						referenceQualifier = ReferenceIdentifierCodeList.Codes.ReplacementInBondNumber;
						break;
					case "03":
						referenceQualifier = ReferenceIdentifierCodeList.Codes.ReplacementFTZAdmissionNumber;
						break;
					case "04":
					case "05":
						referenceQualifier = ReferenceIdentifierCodeList.Codes.ReplacementEntryNumber;
						break;
				}
				yield return GenerateFZ14(referenceQualifier, action.US_Reasons);
			}
		}
	}
}
