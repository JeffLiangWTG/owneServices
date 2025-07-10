using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest
	{
		public void TestElementsNotSupportedInMoveDetailIfAllowUpdateOfCustomsDeclarationAfterCommencement()
		{
			var header = Factory.New<CusInBondHeader>();
			Factory.SaveForTesting();

			var headerDataObject = SetupInBondHeader();
			headerDataObject.AllowUpdateOfCustomsDeclarationAfterCommencement = true;
			var dataTarget = headerDataObject.DataContext.DataTargetCollection.FirstOrDefault();
			dataTarget.Key = header.BH_JobReference;
			var billDataObject = SetupInBondBill("APLU", "HB1234", 200m, 1);
			billDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				SetupCustomsReferenceData(Constants.AdditionalReference.Type, BillReferenceList.Codes.CUB, "11111111")
			};
			headerDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>() { billDataObject });
			var inbMoveHeaderDataObject = SetupInBondMoveHeader("", "", "", "", "", "", "", "", "", "", "", ZDateTime.Empty, "", "", ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, "", "", null, null);
			var inbMoveHeaderDetail = SetupInBondMoveDetail(1, "1241421413", 100, 50m, "1101", new ZDateTime(2018, 03, 21, 16, 0, 0), "TOL VESSEL 2");
			inbMoveHeaderDetail.CustomsStatus = new CodeDescriptionPair() { Code = "FIL", Description = "" };
			inbMoveHeaderDetail.MessageStatus = new CodeDescriptionPair() { Code = "ADD", Description = "Added" };
			inbMoveHeaderDataObject.InBondMoveDetailCollection = new List<InBondMoveDetail>() { inbMoveHeaderDetail };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { inbMoveHeaderDataObject });

			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw", @"The following elements are not supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true. 
· <CustomsStatus>
· <MessageStatus>
", () => reader.ReadIntoBusinessObject());
		}

		protected override InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZInt inBondQty)
		{
			return SetupInBondMoveDetail(additionalBillLink, ZString.Empty, inBondQty, 200m, "1111", new ZDateTime(2018, 03, 20), "AAAAA");
		}

		InBondMoveDetail SetupInBondMoveDetailForPTT(ZInt additionalBillLink, ZInt inBondQty)
		{
			return SetupInBondMoveDetail(additionalBillLink, ZString.Empty, inBondQty, ZDecimal.Zero, ZString.Empty, ZDateTime.Empty, ZString.Empty);
		}

		InBondMoveDetail SetupInBondMoveDetail(ZInt additionalBillLink, ZString previousITNumber, ZInt inBondQty, ZDecimal monetaryValue, ZString scheduleK, ZDateTime exportDate, ZString vesselName)
		{
			var moveDetail = SetupInBondMoveDetail(additionalBillLink, previousITNumber, ZDateTime.Empty, ZString.Empty, ZString.Empty, inBondQty);
			moveDetail.MonetaryValue = monetaryValue;
			moveDetail.ForeignDestPortScheduleK = scheduleK;
			moveDetail.ExportDate = exportDate;
			moveDetail.ExportVesselName = vesselName;
			return moveDetail;
		}

		void AssertCusInBondMoveDetailContents(CusInBondMoveDetail moveDetailBO, ZString previousITNumber, ZInt inBondQty, ZDecimal monetaryValue, ZString scheduleK, ZDateTime exportDate, ZString vesselName, ZGuid billPK)
		{
			AssertEquals("moveDetailBO.B9_PreviousITNumber", previousITNumber, moveDetailBO.B9_PreviousITNumber);
			AssertEquals("moveDetailBO.B9_InBoundQty", inBondQty, moveDetailBO.B9_InBoundQty);
			AssertEquals("moveDetailBO.B9_MonetaryValue", monetaryValue, moveDetailBO.B9_MonetaryValue);
			AssertEquals("moveDetailBO.B9_ForeignDestPortKCode", scheduleK, moveDetailBO.B9_ForeignDestPortKCode);
			AssertEquals("moveDetailBO.B9_ExportDate", exportDate, moveDetailBO.B9_ExportDate);
			AssertEquals("moveDetailBO.B9_ExportLadenOn", vesselName, moveDetailBO.B9_ExportLadenOn);
			AssertEquals("moveDetailBO.B9_B0", billPK, moveDetailBO.B9_B0);
		}
	}
}
