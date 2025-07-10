using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>
	{
		protected abstract InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZInt inBondQty);
		protected InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZString previousITNumber, ZDateTime previousITDate, ZString previousITType, ZString previousITPortDCode, ZInt inBondQty)
		{
			var result = new InBondMoveDetail()
			{
				AdditionalBillLink = additionalBillLink,
				PreviousInBondTransitDate = previousITDate,
				PreviousInBondTransitType = new CodeDescriptionPair2Char()
				{ Code = previousITType },
				PreviousInBondTransitPortScheduleD = new CodeDescriptionPair4Char()
				{ Code = previousITPortDCode },
				InBondQuantity = inBondQty,
				EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>(new[] { new UniversalDataBuss.DataObjects.Universal.EntryNumber()
			{ Type = new EntryType()
			{ Code = Constants.CusInBond.MoveDetail.NumberTypes.PreviousInBondNumber }, Number = previousITNumber, CountryOfIssue = new Country()
			{ Code = Core.Constants.CountryCodes.UnitedStates } } })
			};
			return result;
		}
	}
}
