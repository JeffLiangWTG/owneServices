using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	partial class InBondDataObjectReaderTest<THeader, TBill, TMoveHeader, TMoveDetail, TContainer, TCommodity>
	{
		protected abstract void AssertCusInBondMoveHeaderContents(CusInBondMoveHeader moveHeaderBO);

		protected abstract InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, bool isDefaultingEnabled = false);

		protected InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, ZString entryType, ZString btaIndicator, ZString inBondCarrierID, ZString inBondCarrierSCAC, ZString destinationPortDCode, ZString foreignDestPortKCode, ZString exportLadenOn, ZString cusAgent, ZString tolCarrierID, ZString tolCarrierCode, ZDateTime tolDate, ZString tolCityName, ZString tolStateCode, ZDateTime entryDate, ZDateTime arrivalDate, ZDateTime exportDate, OrgAddress inBondCarrierAddress, OrgAddress tolCarrierAddress)
		{
			var result = new InBondMoveHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				EntryType = new CodeDescriptionPair9Char()
				{ Code = entryType },
				BioterrorismActIndicator = btaIndicator,
				DestinationPortScheduleD = new CodeDescriptionPair4Char()
				{ Code = destinationPortDCode },
				ForeignDestinationPortScheduleK = new CodeDescriptionPair5Char()
				{ Code = foreignDestPortKCode },
				ExportVesselName = exportLadenOn,
				TransferOfLiabilityCarrierID = tolCarrierID,
				TransferOfLiabilityCarrierCode = tolCarrierCode,
				TransferOfLiabilityCityName = tolCityName,
				TransferOfLiabilityStateCode = new CodeDescriptionPair2Char()
				{ Code = tolStateCode },
				CustomsAgent = new Staff()
				{ Code = cusAgent },
				DateCollection = new List<Date>(),
				EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.EntryNumber>(new[] { new UniversalDataBuss.DataObjects.Universal.EntryNumber()
			{ Type = new EntryType()
			{ Code = Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond }, Number = inBondNumber } }),
			};
			if (!inBondCarrierID.IsEmpty)
			{
				result.InBondCarrierID = inBondCarrierID;
			}

			if (!inBondCarrierSCAC.IsEmpty)
			{
				result.InBondCarrierSCAC = inBondCarrierSCAC;
			}

			result.DateCollection.Add(DateType.EntryDate, ZBool.True, entryDate);
			result.DateCollection.Add(DateType.Arrival, ZBool.True, arrivalDate);
			result.DateCollection.Add(DateType.Departure, ZBool.True, exportDate);
			result.DateCollection.Add(DateType.TransferOfLiability, ZBool.True, tolDate);
			SetupAddressData(result, inBondCarrierAddress, Constants.CusInBond.AddressTypes.InBondCarrier);
			SetupAddressData(result, tolCarrierAddress, Constants.CusInBond.AddressTypes.TransferOfLiabilityCarrier);
			return result;
		}
	}
}
