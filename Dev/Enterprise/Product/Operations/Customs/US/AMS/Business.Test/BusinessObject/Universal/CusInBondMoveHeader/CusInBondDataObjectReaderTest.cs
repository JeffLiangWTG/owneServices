using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	partial class CusInBondDataObjectReaderTest
	{
		public void TestElementsNotSupportedInMoveHeaderIfAllowUpdateOfCustomsDeclarationAfterCommencement()
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
			inbMoveHeaderDataObject.CustomsStatus = new CodeDescriptionPair() { Code = "ERR" };
			headerDataObject.SetInBondMoveHeaderCollection(() => new List<InBondMoveHeader>() { inbMoveHeaderDataObject });

			var reader = new CusInBondHeaderDataObjectReader(headerDataObject, logger, Factory, null);
			AssertExceptionThrown<DataObjectReadFailureException>("Exception should throw", @"The following elements are not supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true. 
· <CustomsStatus>
", () => reader.ReadIntoBusinessObject());
		}

		protected override InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, bool isDefaultingEnabled = false)
		{
			return SetupInBondMoveHeader(inBondNumber, InbondCommonTypeList.Codes._2TransportandExport, YesNoDefaultList.Codes.Yes, "INB32523", "INB3", "1123", SeaForeignPort2ScheduleK.ZZD_Code, SeaForeignPort2.RL_Code,
				CusAgent.GS_Code, "INC298331", "INC3", new ZDateTime(2018, 03, 21), "INC CITY", USStatesList.Codes.Alabama, new ZDateTime(2018, 03, 22), new ZDateTime(2018, 03, 23), new ZDateTime(2018, 03, 20), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty, GetOrganizationBO_CRAHOLSYD(Factory.BOFactory).MainAddress, GetOrganizationBO_WUFSHIJNB(Factory.BOFactory).MainAddress);
		}

		protected override void AssertCusInBondMoveHeaderContents(US.Business.CusInBondMoveHeader moveHeaderBO)
		{
			AssertCusInBondMoveHeaderContents((CusInBondMoveHeader)moveHeaderBO, ZString.Empty, InbondCommonTypeList.Codes._2TransportandExport, YesNoDefaultList.Codes.Yes, "INB32523", "INB3", "1123", SeaForeignPort2ScheduleK.ZZD_Code, SeaForeignPort2.RL_Code,
				CusAgent.GS_Code, "INC298331", "INC3", new ZDateTime(2018, 03, 21), "INC CITY", USStatesList.Codes.Alabama, new ZDateTime(2018, 03, 22), new ZDateTime(2018, 03, 23), new ZDateTime(2018, 03, 20), SubApplicationCodeList.Codes.MasterInBond, ZString.Empty);
		}

		InBondMoveHeader SetupInBondMoveHeaderForAMS(ZString sequenceNumber)
		{
			return SetupInBondMoveHeader(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, SubApplicationCodeList.Codes.AMS, sequenceNumber, null, null);
		}

		InBondMoveHeader SetupInBondMoveHeaderForPTT(ZString carrierID)
		{
			return SetupInBondMoveHeader(ZString.Empty, ZString.Empty, ZString.Empty, carrierID, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, SubApplicationCodeList.Codes.PermitToTransfer, ZString.Empty, null, null);
		}

		InBondMoveHeader SetupInBondMoveHeader(ZString inBondNumber, ZString entryType, ZString btaIndicator, ZString inBondCarrierID, ZString inBondCarrierSCAC, ZString destinationPortDCode, ZString foreignDestPortKCode,
			ZString exportLadenOn, ZString cusAgent, ZString tolCarrierID, ZString tolCarrierCode, ZDateTime tolDate, ZString tolCityName, ZString tolStateCode, ZDateTime entryDate, ZDateTime arrivalDate,
			ZDateTime exportDate, ZString applicationCode, ZString seqNo, OrgAddress inBondCarrierAddress, OrgAddress tolCarrierAddress)
		{
			var moveHeaderData = SetupInBondMoveHeader(inBondNumber, entryType, btaIndicator, inBondCarrierID, inBondCarrierSCAC, destinationPortDCode, foreignDestPortKCode, exportLadenOn, cusAgent, tolCarrierID, tolCarrierCode, tolDate, tolCityName, tolStateCode, entryDate, arrivalDate, exportDate, inBondCarrierAddress, tolCarrierAddress);
			moveHeaderData.MessagingApplicationCode = new CodeDescriptionPair() { Code = applicationCode };
			moveHeaderData.SequenceNumber = seqNo;
			return moveHeaderData;
		}

		void AssertCusInBondMoveHeaderForAMSContents(CusInBondMoveHeader moveHeaderBO, ZString sequenceNumber)
		{
			AssertCusInBondMoveHeaderContents(moveHeaderBO, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, SubApplicationCodeList.Codes.AMS, sequenceNumber);
		}

		void AssertCusInBondMoveHeaderForPTTContents(CusInBondMoveHeader moveHeaderBO, ZString carrierID)
		{
			AssertCusInBondMoveHeaderContents(moveHeaderBO, ZString.Empty, ZString.Empty, ZString.Empty, carrierID, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZString.Empty, ZString.Empty, ZDateTime.Empty, ZDateTime.Empty, ZDateTime.Empty, SubApplicationCodeList.Codes.PermitToTransfer, ZString.Empty);
		}

		void AssertCusInBondMoveHeaderContents(CusInBondMoveHeader moveHeaderBO, ZString inbondNumber, ZString entryType, ZString btaIndicator, ZString inBondCarrierID, ZString inBondCarrierSCAC, ZString destinationPortDCode, ZString foreignDestPortKCode,
			ZString exportLadenOn, ZString cusAgent, ZString tolCarrierID, ZString tolCarrierCode, ZDateTime tolDate, ZString tolCityName, ZString tolStateCode, ZDateTime entryDate, ZDateTime arrivalDate,
			ZDateTime exportDate, ZString applicationCode, ZString seqNo)
		{
			AssertEquals("moveHeaderBO.BM_InBondEntryType", entryType, moveHeaderBO.BM_InBondEntryType);
			AssertEquals("moveHeaderBO.BM_BTAIndicator", btaIndicator, moveHeaderBO.BM_BTAIndicator);
			AssertEquals("moveHeaderBO.BM_InBondCarrierID", inBondCarrierID, moveHeaderBO.BM_InBondCarrierID);
			AssertEquals("moveHeaderBO.BM_InBondCarrierSCAC", inBondCarrierSCAC, moveHeaderBO.BM_InBondCarrierSCAC);
			AssertEquals("moveHeaderBO.BM_DestinationPortCode", destinationPortDCode, moveHeaderBO.BM_DestinationPortCode);
			AssertEquals("moveHeaderBO.BM_ForeignDestPortKCode", foreignDestPortKCode, moveHeaderBO.BM_ForeignDestPortKCode);
			AssertEquals("moveHeaderBO.BM_ExportLadenOn", exportLadenOn, moveHeaderBO.BM_ExportLadenOn);
			AssertEquals("moveHeaderBO.BM_GS_NKCusAgent", cusAgent, moveHeaderBO.BM_GS_NKCusAgent);
			AssertEquals("moveHeaderBO.BM_TOLCarrierID", tolCarrierID, moveHeaderBO.BM_TOLCarrierID);
			AssertEquals("moveHeaderBO.BM_TOLCarrierCode", tolCarrierCode, moveHeaderBO.BM_TOLCarrierCode);
			AssertEquals("moveHeaderBO.BM_TOLDate", tolDate, moveHeaderBO.BM_TOLDate);
			AssertEquals("moveHeaderBO.BM_TOLStateCode", tolStateCode, moveHeaderBO.BM_TOLStateCode);
			AssertEquals("moveHeaderBO.BM_EntryDate", entryDate, moveHeaderBO.BM_EntryDate);
			AssertEquals("moveHeaderBO.BM_ArrivalDate", arrivalDate, moveHeaderBO.BM_ArrivalDate);
			AssertEquals("moveHeaderBO.BM_ExportDate", exportDate, moveHeaderBO.BM_ExportDate);
			AssertEquals("moveHeaderBO.BM_SubApplicationCode", applicationCode, moveHeaderBO.BM_SubApplicationCode);

			if (!inbondNumber.IsEmpty)
			{
				AssertEquals("moveHeaderBO.InBondNumber", inbondNumber, moveHeaderBO.InBondNumber);
			}
			if (!seqNo.IsEmpty)
			{
				AssertEquals("moveHeaderBO.BM_ManifestSequenceNumber", seqNo, moveHeaderBO.BM_ManifestSequenceNumber);
			}
		}
	}
}
