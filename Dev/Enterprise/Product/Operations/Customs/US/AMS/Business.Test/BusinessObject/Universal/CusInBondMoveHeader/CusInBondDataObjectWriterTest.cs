using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest
	{
		void SetupCusInBondMoveHeaderForAMS(CusInBondMoveHeader moveHeader, ZString sequenceNumber)
		{
			SetupCusInBondMoveHeader(moveHeader, "AMS", "", "", "", "", "", "", "", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", "", "", "", "", "", sequenceNumber);
		}

		void SetupCusInBondMoveHeaderForPTT(CusInBondMoveHeader moveHeader, ZString carrierID, ZString customsStatus)
		{
			SetupCusInBondMoveHeader(moveHeader, "PTT", "", "", customsStatus, "", carrierID, "", "", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", "", "", "", "", "", "");
		}

		void SetupCusInBondMoveHeader(CusInBondMoveHeader moveHeader, ZString inBondNumber, bool isSubsequent = false)
		{
			SetupCusInBondMoveHeader(moveHeader, isSubsequent ? "SIB" : "INB", inBondNumber, "62", "", "Y", "23-234243245", "OTT1", "0401", "1101", new ZDateTime(2018, 3, 21), new ZDateTime(2018, 3, 22),
				new ZDateTime(2018, 3, 23), new ZDateTime(2018, 3, 24), "IAN BOAT", "KD34", "98-23479934", "IAN CITY", "US", CusAgent.GS_Code, "");
		}

		void SetupCusInBondMoveHeader(CusInBondMoveHeader moveHeader, ZString applicationCode, ZString inBondNumber, ZString entryType, ZString customsStatus, ZString btaIndicator, ZString carrierID, ZString carrierSCAC,
			ZString destinationPortSchD, ZString destinationPortSchK, ZDateTime arrivalDate, ZDateTime entryDate, ZDateTime exportDate, ZDateTime tolDate, ZString exportLadenOn, ZString carrierCode, ZString tolCarrierID,
			ZString tolCityName, ZString tolStateCode, ZString cusAgent, ZString seqNumber)
		{
			moveHeader.BM_SubApplicationCode = applicationCode;
			moveHeader.BM_InBondEntryType = entryType;
			moveHeader.BM_CustomsStatus = customsStatus;
			moveHeader.BM_BTAIndicator = btaIndicator;
			moveHeader.BM_InBondCarrierID = carrierID;
			moveHeader.BM_InBondCarrierSCAC = carrierSCAC;
			moveHeader.BM_DestinationPortCode = destinationPortSchD;
			moveHeader.BM_ForeignDestPortKCode = destinationPortSchK;
			moveHeader.BM_ArrivalDate = arrivalDate;
			moveHeader.BM_EntryDate = entryDate;
			moveHeader.BM_ExportDate = exportDate;
			moveHeader.BM_TOLDate = tolDate;
			moveHeader.BM_ExportLadenOn = exportLadenOn;
			moveHeader.BM_TOLCarrierCode = carrierCode;
			moveHeader.BM_TOLCarrierID = tolCarrierID;
			moveHeader.BM_TOLCityName = tolCityName;
			moveHeader.BM_TOLStateCode = tolStateCode;
			moveHeader.BM_GS_NKCusAgent = cusAgent;

			if (!inBondNumber.IsEmpty)
			{
				moveHeader.InBondNumber = inBondNumber;
			}
			if (!seqNumber.IsEmpty)
			{
				moveHeader.BM_ManifestSequenceNumber = seqNumber;
			}
		}

		void AssertInBondMoveHeaderContentsForAMS(InBondMoveHeader moveHeaderData, ZString sequenceNumber)
		{
			AssertInBondMoveHeaderContents(moveHeaderData, "", "", "", "", "", "", "", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", "", "", "", "", "", sequenceNumber);
		}

		void AssertInBondMoveHeaderContentsForPTT(InBondMoveHeader moveHeaderData, ZString carrierID, ZString customsStatus)
		{
			AssertInBondMoveHeaderContents(moveHeaderData, "", "", customsStatus, "", carrierID, "", "", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", "", "", "", "", "", "");
		}

		void AssertInBondMoveHeaderContents(InBondMoveHeader moveHeaderData, ZString inBondNumber)
		{
			AssertInBondMoveHeaderContents(moveHeaderData, inBondNumber, "62", "", "Y", "23-234243245", "OTT1", "0401", "1101", new ZDateTime(2018, 3, 21), new ZDateTime(2018, 3, 22),
				new ZDateTime(2018, 3, 23), new ZDateTime(2018, 3, 24), "IAN BOAT", "KD34", "98-23479934", "IAN CITY", "US", CusAgent.GS_Code, "");
		}

		void AssertInBondMoveHeaderContents(InBondMoveHeader moveHeaderData, ZString inBondNumber, ZString entryType, ZString customsStatus, ZString btaIndicator, ZString carrierID, ZString carrierSCAC,
			ZString destinationPortSchD, ZString destinationPortSchK, ZDateTime arrivalDate, ZDateTime entryDate, ZDateTime exportDate, ZDateTime tolDate, ZString exportLadenOn, ZString tolCarrierCode, ZString tolCarrierID,
			ZString tolCityName, ZString tolStateCode, ZString cusAgent, ZString seqNumber)
		{
			AssertNotNull("Precondition: moveHeaderData", moveHeaderData);

			CombineAssertions(() =>
			{
				if (!inBondNumber.IsEmpty)
				{
					AssertNotNull("moveHeaderData.EntryNumberCollection", moveHeaderData.EntryNumberCollection);
					AssertEquals("moveHeaderData.EntryNumberCollection.Count", 1, moveHeaderData.EntryNumberCollection.Count);
					AssertEquals("moveHeaderData.EntryNumberCollection[0].Type", Enterprise.Customs.US.Business.CusEntryHeaderMessageTypeList.Codes.InBond, moveHeaderData.EntryNumberCollection[0].Type.GetCodeAsUpperCase());
					AssertEquals("moveHeaderData.EntryNumberCollection[0].Number", inBondNumber, moveHeaderData.EntryNumberCollection[0].Number.GetValueOrDefault());
				}
				if (!seqNumber.IsEmpty)
				{
					AssertEquals("moveHeaderData.SequenceNumber", seqNumber, moveHeaderData.SequenceNumber.GetValueOrDefault());
				}

				AssertEquals("moveHeaderData.EntryType", entryType, moveHeaderData.EntryType.GetCodeAsUpperCase());
				AssertEquals("moveHeaderData.CustomsStatus", customsStatus, moveHeaderData.CustomsStatus.GetCodeAsUpperCase());
				AssertEquals("moveHeaderData.BioterrorismActIndicator", btaIndicator, moveHeaderData.BioterrorismActIndicator.GetValueOrDefault());
				AssertEquals("moveHeaderData.InBondCarrierID", carrierID, moveHeaderData.InBondCarrierID.GetValueOrDefault());
				AssertEquals("moveHeaderData.InBondCarrierSCAC", carrierSCAC, moveHeaderData.InBondCarrierSCAC.GetValueOrDefault());
				AssertEquals("moveHeaderData.DestinationPortScheduleD", destinationPortSchD, moveHeaderData.DestinationPortScheduleD.GetCodeAsUpperCase());
				AssertEquals("moveHeaderData.ForeignDestinationPortScheduleK", destinationPortSchK, moveHeaderData.ForeignDestinationPortScheduleK.GetCodeAsUpperCase());
				AssertEquals("moveHeaderData.ExportVesselName", exportLadenOn, moveHeaderData.ExportVesselName.GetValueOrDefault());
				AssertEquals("moveHeaderData.TransferOfLiabilityCarrierCode", tolCarrierCode, moveHeaderData.TransferOfLiabilityCarrierCode.GetValueOrDefault());
				AssertEquals("moveHeaderData.TransferOfLiabilityCarrierID", tolCarrierID, moveHeaderData.TransferOfLiabilityCarrierID.GetValueOrDefault());
				AssertEquals("moveHeaderData.TransferOfLiabilityCityName", tolCityName, moveHeaderData.TransferOfLiabilityCityName.GetValueOrDefault());
				AssertEquals("moveHeaderData.TransferOfLiabilityStateCode", tolStateCode, moveHeaderData.TransferOfLiabilityStateCode.GetCodeAsUpperCase());
				AssertEquals("moveHeaderData.CustomsAgent", cusAgent, moveHeaderData.CustomsAgent.GetCodeAsUpperCase());

				AssertContainDate(moveHeaderData.DateCollection, DateType.EntryDate, ZBool.True, entryDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.Arrival, ZBool.True, arrivalDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.Departure, ZBool.True, exportDate);
				AssertContainDate(moveHeaderData.DateCollection, DateType.TransferOfLiability, ZBool.True, tolDate);
			});
		}
	}
}
