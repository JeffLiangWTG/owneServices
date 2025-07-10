using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectWriterTest
	{
		void AssertInBondMoveDetailContents(InBondMoveDetail moveDetailData, ZInt? billLink, ZInt? inBondQty, ZDecimal? monetaryValue, ZString foreignDestPortKCode, ZDateTime? exportDate, ZString? exportLadenOn, ZString? previousITNumber, ZString? customsStatus, ZString? messageStatus, ZString? departureStatus, ZString? arrivalStatus, ZString? exportationStatus, ZString? tolStatus)
		{
			AssertNotNull("Precondition: moveDetailData", moveDetailData);
			CombineAssertions(() =>
			{
				AssertEquals("moveDetailData.AdditionalBillLink", billLink, moveDetailData.AdditionalBillLink);
				AssertEquals("moveDetailData.InBondQuantity", inBondQty, moveDetailData.InBondQuantity);
				AssertEquals("moveDetailData.MonetaryValue", monetaryValue, moveDetailData.MonetaryValue);
				AssertEquals("moveDetailData.ForeignDestPortScheduleK", foreignDestPortKCode, moveDetailData.ForeignDestPortScheduleK);
				AssertEquals("moveDetailData.ExportDate", exportDate, moveDetailData.ExportDate);
				AssertEquals("moveDetailData.ExportVesselName", exportLadenOn, moveDetailData.ExportVesselName);
				AssertEquals("moveDetailData.CustomsStatus", customsStatus.GetValueOrDefault(), moveDetailData.CustomsStatus.GetCodeAsUpperCase());
				AssertEquals("moveDetailData.MessageStatus", messageStatus.GetValueOrDefault(), moveDetailData.MessageStatus.GetCodeAsUpperCase());
				AssertEquals("moveDetailData.DepartureStatus", departureStatus.GetValueOrDefault(), moveDetailData.DepartureStatus.GetCodeAsUpperCase());
				AssertEquals("moveDetailData.ArrivalStatus", arrivalStatus.GetValueOrDefault(), moveDetailData.ArrivalStatus.GetCodeAsUpperCase());
				AssertEquals("moveDetailData.ExportationStatus", exportationStatus.GetValueOrDefault(), moveDetailData.ExportationStatus.GetCodeAsUpperCase());
				AssertEquals("moveDetailData.TransferOfLiabilityStatus", tolStatus.GetValueOrDefault(), moveDetailData.TransferOfLiabilityStatus.GetCodeAsUpperCase());
				if (previousITNumber.HasValue)
				{
					var previousITNumberCollection = moveDetailData.EntryNumberCollection.Where(x => x.Type.GetCodeAsUpperCase() == "PIT").ToArray();
					AssertEquals("previousITNumberCollection", 1, previousITNumberCollection.Length);
					AssertEquals("previousITNumberCollection.Number", previousITNumber, previousITNumberCollection[0].Number);
					AssertEquals("previousITNumberCollection.CountryOfIssue", "US", previousITNumberCollection[0].CountryOfIssue.GetCodeAsUpperCase());
				}
			});
		}

		void SetupCusInBondMoveDetailForPTT(CusInBondMoveDetail moveDetail, ZInt quantity, ZString customsStatus, ZString messageStatus)
		{
			SetupCusInBondMoveDetail(moveDetail, quantity, 0m, "", ZDateTime.Empty, "", "", customsStatus, messageStatus);
		}

		void SetupCusInBondMoveDetail(CusInBondMoveDetail moveDetail, ZInt inBoundQty, ZDecimal monetaryValue, ZString foreignDestPortKCode, ZDateTime exportDate, ZString exportLadenOn, ZString previousITNo, ZString customsStatus, ZString messageStatus)
		{
			moveDetail.B9_InBoundQty = inBoundQty;
			moveDetail.B9_MonetaryValue = monetaryValue;
			moveDetail.B9_ForeignDestPortKCode = foreignDestPortKCode;
			moveDetail.B9_ExportDate = exportDate;
			moveDetail.B9_ExportLadenOn = exportLadenOn;
			moveDetail.B9_PreviousITNumber = previousITNo;
			moveDetail.B9_CustomsStatus = customsStatus;
			moveDetail.B9_MessageStatus = messageStatus;
		}
	}
}
