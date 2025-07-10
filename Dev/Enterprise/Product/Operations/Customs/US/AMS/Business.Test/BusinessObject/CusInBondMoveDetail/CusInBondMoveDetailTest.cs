using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveDetail))]
	sealed class CusInBondMoveDetailTest : US.Business.Testing.CusInBondMoveDetailTest<CusInBondMoveDetail>
	{
		public void TestCusInBondContainerType()
		{
			var supporter = moveDetail as ICusInBondContainerTypeSupporter;
			AssertEquals(typeof(CusInBondContainer), supporter.ContainerType);
			AssertEquals(typeof(CusInBondContainer), moveDetail.ContainerType);
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals(typeof(CusInBondMoveDetailLookups), moveDetail.Lookups.GetType());
		}

		public void TestIsAcceptedByCustoms()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.InBondMovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			var list = new AMSBillMessageStatusList();
			foreach (var messageStatus in new[] {
				AMSBillMessageStatusList.Codes.ClearArrival,
				AMSBillMessageStatusList.Codes.ClearDeparture,
				AMSBillMessageStatusList.Codes.ClearDiversion,
				AMSBillMessageStatusList.Codes.ClearExportation,
				AMSBillMessageStatusList.Codes.ClearTransferOfLiability,
				AMSBillMessageStatusList.Codes.ClearPermitToTransfer
			})
			{
				list.RemoveCode(messageStatus);
				moveDetail.B9_MessageStatus = messageStatus;
				AssertEquals(messageStatus, true, moveDetail.IsAcceptedByCustoms);
				moveDetail.B9_MessageStatus = ZString.Empty;
				AssertEquals(false, moveDetail.IsAcceptedByCustoms);
			}

			foreach (ICodeDescription pair in list)
			{
				moveDetail.B9_MessageStatus = pair.Code;
				AssertEquals(pair.Code, false, moveDetail.IsAcceptedByCustoms);
			}
		}

		public void TestDeleteMoveDetails()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.InBondMovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			AssertCanDelete(moveDetail, Enterprise.Customs.US.AMS.Business.ValidationConstants.MoveDetail.CannotDeleteMoveDetailBeforeMessageDelete("an In-Bond Cancellation"));

			moveHeader = header.PTTMovements.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew(bill.PK);
			AssertCanDelete(moveDetail, Enterprise.Customs.US.AMS.Business.ValidationConstants.MoveDetail.CannotDeleteMoveDetailBeforeMessageDelete("a Permit To Transfer Cancel"));

			moveDetail = bill.MovementDetail;
			AssertCanDelete(moveDetail, Enterprise.Customs.US.AMS.Business.ValidationConstants.MoveDetail.CannotDeleteMoveDetailBeforeMessageDelete("a Delete"));
		}

		void AssertCanDelete(CusInBondMoveDetail moveDetail, string message)
		{
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals(false, moveDetail.CanDelete);
			AssertEquals(message, moveDetail.ReasonForNotAbleToDelete);

			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			AssertEquals(true, moveDetail.CanDelete);
			AssertEquals("", moveDetail.ReasonForNotAbleToDelete);
			var list = new AMSBillMessageStatusList();
			foreach (var messageStatus in new[] {
				AMSBillMessageStatusList.Codes.ClearArrival,
				AMSBillMessageStatusList.Codes.ClearDeparture,
				AMSBillMessageStatusList.Codes.ClearDiversion,
				AMSBillMessageStatusList.Codes.ClearExportation,
				AMSBillMessageStatusList.Codes.ClearTransferOfLiability,
				AMSBillMessageStatusList.Codes.ClearPermitToTransfer,
				AMSBillMessageStatusList.Codes.Adding,
				AMSBillMessageStatusList.Codes.AwaitingArrival,
				AMSBillMessageStatusList.Codes.Deleting,
				AMSBillMessageStatusList.Codes.AwaitingDeparture,
				AMSBillMessageStatusList.Codes.AwaitingDiversion,
				AMSBillMessageStatusList.Codes.AwaitingExportation,
				AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability,
				AMSBillMessageStatusList.Codes.AwaitingPermitToTransfer,
				AMSBillMessageStatusList.Codes.Updating
			})
			{
				list.RemoveCode(messageStatus);
				moveDetail.B9_MessageStatus = messageStatus;
				AssertEquals(false, moveDetail.CanDelete);
				AssertEquals(message, moveDetail.ReasonForNotAbleToDelete);
			}

			foreach (ICodeDescription pair in list)
			{
				moveDetail.B9_MessageStatus = pair.Code;
				AssertEquals(true, moveDetail.CanDelete);
				AssertEquals("", moveDetail.ReasonForNotAbleToDelete);
			}
		}

		[TestDate(2015, 1, 21, 21, 52, 0)]
		public void TestConcurrency()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var moveDetailInDiffFactory = newFactory.Load<CusInBondMoveDetail>(moveDetail.PK);
			moveDetailInDiffFactory.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			moveDetailInDiffFactory.B9_ExportDate = new ZDateTime(2015, 1, 24);
			newFactory.Save();

			AssertEquals(ZString.Empty, moveDetail.B9_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveDetail.B9_ExportDate);
			moveDetail.B9_ExportLadenOn = "HELLO";
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}
			AssertEquals(ZString.Empty, moveDetail.B9_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveDetail.B9_ExportDate);
			AssertEquals("HELLO", moveDetail.B9_ExportLadenOn);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);

			var lastEdited = ZDataUtils.GetUsernameAndTimeOfLastModification(((INeedRow)moveDetailInDiffFactory).Row, false);
			string getLastEditedInMessage(string input) => string.IsNullOrWhiteSpace(input) ? string.Empty : " (" + input + ")";
			var lastEditedInMessage = getLastEditedInMessage(lastEdited);
			var expectedMessage = $@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
{lastEditedInMessage}
	Export Date
	Customs Status (Critical change)";

			AssertMultilineASCIIEquals("ReportInformationMessage", expectedMessage, handler.ReportInformationMessage);
			moveDetail.Delete();
			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(true, moveDetail.IsDeleted);
			AssertEquals(true, moveDetail.HasChanges);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);

			var moveDetailLastEdited = ZDataUtils.GetUsernameAndTimeOfLastModification(((INeedRow)moveDetail).Row, false);
			var moveDetailLastEditedInMessage = getLastEditedInMessage(moveDetailLastEdited);
			var moveDetailExpectedMessage = $@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
{moveDetailLastEditedInMessage} (pending delete)
	Export Date
	Customs Status (Critical change)";
			AssertMultilineASCIIEquals("ReportInformationMessage", moveDetailExpectedMessage, handler.ReportInformationMessage);

			factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			moveDetail = factory.Load<CusInBondMoveDetail>(moveDetailInDiffFactory.PK);
			AssertEquals(AMSBillCustomsStatusList.Codes.OnFile, moveDetail.B9_CustomsStatus);
			AssertEquals(new ZDateTime(2015, 1, 24), moveDetail.B9_ExportDate);
			AssertEquals("", moveDetail.B9_MessageStatus);
			moveDetail.B9_ExportLadenOn = "HELLO";

			moveDetailInDiffFactory.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;
			newFactory.Save();

			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			var messageStatusLastEdited = ZDataUtils.GetUsernameAndTimeOfLastModification(((INeedRow)moveDetail).Row, false);
			var messageStatusLastEditedInMessage = getLastEditedInMessage(messageStatusLastEdited);
			var messageStatusExpectedMessage = $@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
{messageStatusLastEditedInMessage}
	Message Status (Critical change)";
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", messageStatusExpectedMessage, handler.ReportInformationMessage);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(CusInBondMoveDetailValidation), moveDetail.Validation.GetType());
		}

		public void TestDefaultingDataForInBondMovement()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			bill.B0_ManifestQty = 150;
			var moveDetail = bill.MovementDetail;
			var containers = moveDetail.Containers;
			var container1 = containers.AddNew();
			var commodities1 = container1.Commodities;
			var commodity1 = commodities1.AddNew();
			commodity1.BY_MonetaryValue = 10m;
			var commodity2 = commodities1.AddNew();
			commodity2.BY_MonetaryValue = 25m;
			var container2 = containers.AddNew();
			var commodities2 = container2.Commodities;
			var commodity3 = commodities2.AddNew();
			commodity3.BY_MonetaryValue = 35m;
			var commodity4 = commodities2.AddNew();
			commodity4.BY_MonetaryValue = 30m;

			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inbondMoveDetail = inbondMoveHeader.MovementDetails.AddNew(bill.PK);
			AssertEquals("B9_MonetaryValue", 100m, inbondMoveDetail.B9_MonetaryValue);
			AssertEquals("B9_InBoundQty", 150, inbondMoveDetail.B9_InBoundQty);

			var pttMoveHeader = header.PTTMovements.AddNew();
			var pttMoveDetail = pttMoveHeader.MovementDetails.AddNew(bill.PK);
			AssertEquals("B9_MonetaryValue", ZDecimal.Zero, pttMoveDetail.B9_MonetaryValue);
			AssertEquals("B9_InBoundQty", ZInt.Zero, pttMoveDetail.B9_InBoundQty);
		}

		public void TestIMovemenDetailsMembers()
		{
			var header = Factory.New<CusInBondHeader>();
			var amsMoveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);

			IMovemenDetails details = inBondMoveDetail;
			AssertEquals("PreviousInBondNumber", ZString.Empty, details.PreviousInBondNumber);
			inBondMoveDetail.B9_PreviousITNumber = "PRIT3234";
			AssertEquals("PreviousInBondNumber", "PRIT3234", details.PreviousInBondNumber);

			AssertEquals("InBondQuantity", ZInt.Zero, details.InBondQuantity);
			inBondMoveDetail.B9_InBoundQty = 345;
			AssertEquals("InBondQuantity", 345, details.InBondQuantity);

			AssertEquals("InbondEntryType", ZString.Empty, details.InbondEntryType);
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("InbondEntryType", InbondCommonTypeList.Codes._1ImmediateTransport, details.InbondEntryType);

			AssertEquals("IsBTAFDA", ZBool.False, details.IsBTAFDA);
			inBondMoveHeader.BM_BTAIndicator = "Y";
			AssertEquals("IsBTAFDA", ZBool.True, details.IsBTAFDA);

			AssertEquals("ConventionalInbondNumber", ZString.Empty, details.ConventionalInbondNumber);
			inBondMoveHeader.InBondNumber = "986574532";
			AssertEquals("ConventionalInbondNumber", "986574532", details.ConventionalInbondNumber);

			AssertEquals("InbondCarrierCode", ZString.Empty, details.InbondCarrierCode);
			inBondMoveHeader.BM_InBondCarrierSCAC = "OT23";
			AssertEquals("InbondCarrierCode", "OT23", details.InbondCarrierCode);

			AssertEquals("USPortOfDestination", ZString.Empty, details.USPortOfDestination);
			inBondMoveHeader.BM_DestinationPortCode = "2705";
			AssertEquals("USPortOfDestination", "2705", details.USPortOfDestination);

			AssertEquals("ForeignDestination", ZString.Empty, details.ForeignDestination);
			inBondMoveHeader.BM_ForeignDestPortKCode = "37340";
			AssertEquals("ForeignDestination", "37340", details.ForeignDestination);

			AssertEquals("Value", ZInt.Zero, details.Value);
			inBondMoveDetail.B9_MonetaryValue = 3243;
			AssertEquals("Value", 3243, details.Value);

			AssertEquals("BondedCarrierID", ZString.Empty, details.BondedCarrierID);
			inBondMoveHeader.BM_InBondCarrierID = "23-234243245";
			AssertEquals("BondedCarrierID", "23-234243245", details.BondedCarrierID);

			AssertEquals("PaperlessInbondNumber", ZString.Empty, details.PaperlessInbondNumber);
			inBondMoveHeader.InBondNumber = "V9865745323";
			AssertEquals("PaperlessInbondNumber", "V9865745323", details.PaperlessInbondNumber);

			AssertEquals("ExportVesselName", ZString.Empty, details.ExportVesselName);
			inBondMoveDetail.B9_ExportLadenOn = "WENDY THE DESTROYER";
			AssertEquals("ExportVesselName", "WENDY THE DESTROYER", details.ExportVesselName);

			AssertEquals("ArrivalDateTime", ZDateTime.Empty, details.ArrivalDateTime);
			inBondMoveHeader.BM_ArrivalDate = new ZDateTime(2012, 3, 3, 5, 4, 34);
			AssertEquals("ArrivalDateTime", new ZDateTime(2012, 3, 3, 5, 4, 34), details.ArrivalDateTime);

			AssertEquals("ExportDateTime", ZDateTime.Empty, details.ExportDateTime);
			inBondMoveDetail.B9_ExportDate = new ZDateTime(2012, 4, 5, 6, 3, 34);
			AssertEquals("ExportDateTime", new ZDateTime(2012, 4, 5, 6, 3, 34), details.ExportDateTime);

			AssertEquals("TOLInBondCarrierCode", ZString.Empty, details.TOLInBondCarrierCode);
			inBondMoveHeader.BM_TOLCarrierCode = "KD34";
			AssertEquals("TOLInBondCarrierCode", "KD34", details.TOLInBondCarrierCode);

			AssertEquals("TOLBondedCarrierID", ZString.Empty, details.TOLBondedCarrierID);
			inBondMoveHeader.BM_TOLCarrierID = "98-23479934";
			AssertEquals("TOLBondedCarrierID", "98-23479934", details.TOLBondedCarrierID);

			AssertEquals("TOLDateTime", ZDateTime.Empty, details.TOLDateTime);
			inBondMoveHeader.BM_TOLDate = new ZDateTime(2012, 8, 4, 23, 12, 34);
			AssertEquals("TOLDateTime", new ZDateTime(2012, 8, 4, 23, 12, 34), details.TOLDateTime);

			AssertEquals("TOLCityName", ZString.Empty, details.TOLCityName);
			inBondMoveHeader.BM_TOLCityName = "JOE CITY";
			AssertEquals("TOLCityName", "JOE CITY", details.TOLCityName);

			AssertEquals("TOLStateCode", ZString.Empty, details.TOLStateCode);
			inBondMoveHeader.BM_TOLStateCode = "CA";
			AssertEquals("TOLStateCode", "CA", details.TOLStateCode);
		}

		public void TestEffectiveFields()
		{
			var header = Factory.New<CusInBondHeader>();
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			var moveDetail1 = inbondMoveHeader.MovementDetails.AddNew();
			var moveDetail1Row = ((IBusinessObjectInternals)moveDetail1).Row;
			var moveDetail2 = inbondMoveHeader.MovementDetails.AddNew();
			var moveDetail2Row = ((IBusinessObjectInternals)moveDetail2).Row;

			inbondMoveHeader.BM_ExportDate = new ZDateTime(2012, 4, 1);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail1.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);

			moveDetail1.B9_ExportDate = new ZDateTime(2012, 4, 1);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail1.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);

			moveDetail1.B9_ExportDate = new ZDateTime(2012, 5, 1);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail1.B9_ExportDate);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);

			inbondMoveHeader.BM_ExportLadenOn = "BOB BOAT";
			AssertEquals("BOB BOAT", moveDetail1.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			AssertEquals("BOB BOAT", moveDetail2.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);

			moveDetail2.B9_ExportLadenOn = "BOB BOAT";
			AssertEquals("BOB BOAT", moveDetail1.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			AssertEquals("BOB BOAT", moveDetail2.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);

			moveDetail2.B9_ExportLadenOn = "WENDY BOAT";
			AssertEquals("BOB BOAT", moveDetail1.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			AssertEquals("WENDY BOAT", moveDetail2.B9_ExportLadenOn);
			AssertEquals("WENDY BOAT", moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);

			inbondMoveHeader.BM_ForeignDestPortKCode = "12345";
			AssertEquals("12345", moveDetail1.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			AssertEquals("12345", moveDetail2.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);

			moveDetail1.B9_ForeignDestPortKCode = "12345";
			AssertEquals("12345", moveDetail1.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			AssertEquals("12345", moveDetail2.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);

			moveDetail1.B9_ForeignDestPortKCode = "67890";
			AssertEquals("67890", moveDetail1.B9_ForeignDestPortKCode);
			AssertEquals("67890", moveDetail1Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			AssertEquals("12345", moveDetail2.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
		}

		public void TestICommonBillOfLadingShipmentReferenceDetailsIncludesOceanBill()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var oceanBill = header.OceanBill;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			ICommonBillOfLading billOfLading = moveDetail;
			oceanBill.B0_IssuerCode = "";
			oceanBill.B0_MasterBillNumber = "";
			var details = billOfLading.ShipmentReferenceDetails(ActionCode.Creating);
			var oceanBillReference = details.FirstOrDefault(x => x.Qualifier == BillReferenceList.Codes.OB);
			AssertNull("Should not include if Issuer and Bill Number is empty", oceanBillReference);
			oceanBill.B0_IssuerCode = "OTTA";
			details = billOfLading.ShipmentReferenceDetails(ActionCode.Creating);
			oceanBillReference = details.FirstOrDefault(x => x.Qualifier == BillReferenceList.Codes.OB);
			AssertEquals("oceanBillReference.ReferenceIdentifier", "OTTA", oceanBillReference.ReferenceIdentifier);
			oceanBill.B0_IssuerCode = "";
			oceanBill.B0_MasterBillNumber = "MB123";
			details = billOfLading.ShipmentReferenceDetails(ActionCode.Creating);
			oceanBillReference = details.FirstOrDefault(x => x.Qualifier == BillReferenceList.Codes.OB);
			AssertEquals("oceanBillReference.ReferenceIdentifier", "MB123", oceanBillReference.ReferenceIdentifier);
			oceanBill.B0_IssuerCode = "OTTA";
			details = billOfLading.ShipmentReferenceDetails(ActionCode.Creating);
			oceanBillReference = details.FirstOrDefault(x => x.Qualifier == BillReferenceList.Codes.OB);
			AssertEquals("oceanBillReference.ReferenceIdentifier", "OTTAMB123", oceanBillReference.ReferenceIdentifier);

			oceanBill.B0_BH = ZGuid.Empty;
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			oceanBill.B0_BH = header.PK;
			AssertEquals("header.OceanBill", oceanBill, header.OceanBill);
			AssertNull("Should only get from OceanBill if it is NVOCC", details.FirstOrDefault(x => x.Qualifier == BillReferenceList.Codes.OB));
		}

		public void TestIACEBillOfLadingMembers()
		{
			var helper = new MasterFilesTestHelper(Factory);
			var header = Factory.New<CusInBondHeader>();
			header.MovementHeader.BM_ManifestSequenceNumber = "0101";
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "KD34";
			bill.B0_MasterBillNumber = "SCAD23432342";
			bill.B0_RL_NKPortOfLading = helper.AUMEL.RL_Code;
			bill.B0_ManifestQty = 150;
			bill.B0_ManifestUQ = Core.Constants.PkgUnit.Package;
			bill.B0_Weight = 1.534m;
			bill.B0_WeightUQ = Core.Constants.Weight.Tonnes;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterFROB;
			bill.B0_MasterInBondIndicator = ZBool.True;
			bill.B0_RL_NKInBondPortOfDest = helper.USLAX.RL_Code;
			bill.B0_IssuerSCAC = "SDKD";
			bill.B0_Firms = "SD32";
			bill.B0_Volume = 2510m;
			bill.B0_VolumeUQ = Core.Constants.Volume.CubicDecimetres;
			bill.B0_PlaceOfReceipt = "BOB'S HOUSE";
			var snp1 = bill.SecondaryNotifyParties.AddNewIfNotExist("SN23");
			AssertEquals((short)1, snp1.CY_Order);
			var snp2 = bill.SecondaryNotifyParties.AddNewIfNotExist("SN45");
			AssertEquals((short)2, snp2.CY_Order);
			var snp3 = bill.SecondaryNotifyParties.AddNewIfNotExist("SN67");
			AssertEquals((short)3, snp3.CY_Order);
			var snp4 = bill.SecondaryNotifyParties.AddNewIfNotExist("SN89");
			AssertEquals((short)4, snp4.CY_Order);
			bill.B0_RL_NKLastForeignPort = helper.AUSYD.RL_Code;
			bill.B0_TransportModeToPortOfLading = TransportTypeList.Codes.Rail;
			bill.B0_TransportPaymentMethod = PaymentMethodCodeList.Codes.CAS;
			bill.B0_RL_NKForeignPortOfContract = helper.AUMEL.RL_Code;

			bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.CG, "GB32342342");
			bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.M, "M89213123");

			var orgCustomsBroker = Factory.New<OrgHeader>();
			orgCustomsBroker.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ABIRoutingCode, "5678DEF68", Core.Constants.CountryCodes.UnitedStates);
			bill.CustomsBroker.E2_OA_Address = orgCustomsBroker.MainAddress.PK;

			var foreignShipper = bill.ForeignShipper;
			foreignShipper.E2_AddressOverride = true;
			foreignShipper.E2_CompanyName = "BOB THE BUILDER";
			foreignShipper.E2_Address1 = "BOB'S ADDDRESS 1";
			foreignShipper.E2_Address2 = "BOB'S ADDDRESS 2".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '2');
			foreignShipper.E2_City = "BOB'S CITY";
			foreignShipper.E2_State = "BOB'S STATE";
			foreignShipper.E2_RN_NKCountryCode = "AU";
			foreignShipper.E2_Postcode = "2111";
			foreignShipper.E2_Phone = "2353982343";
			foreignShipper.E2_Contact = "BOB'S CONTACT";
			foreignShipper.E2_Mobile = "6956552243";
			foreignShipper.E2_Fax = "865555343";
			foreignShipper.E2_Email = "BOB@WHERE.COM";

			var consignee = bill.Consignee;
			consignee.E2_AddressOverride = true;
			consignee.E2_CompanyName = "WENDY THE DESTROYER";
			consignee.E2_Address1 = "WENDY'S ADDDRESS 1";
			consignee.E2_Address2 = "WENDY'S ADDDRESS 2".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '2');
			consignee.E2_City = "WENDY'S CITY";
			consignee.E2_State = "WENDY'S STATE";
			consignee.E2_RN_NKCountryCode = "NZ";
			consignee.E2_Postcode = "3695";
			consignee.E2_Phone = "9854963541";
			consignee.E2_Contact = "WENDY'S CONTACT";
			consignee.E2_Fax = "8563114566";

			var notifyParty1 = bill.NotifyParty1;
			notifyParty1.E2_AddressOverride = true;
			notifyParty1.E2_CompanyName = "WHO THE WHAT";
			notifyParty1.E2_Address1 = "WHO'S ADDDRESS 1";
			notifyParty1.E2_Address2 = "WHO'S ADDDRESS 2".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '2');
			notifyParty1.E2_City = "WHO'S CITY";
			notifyParty1.E2_State = "WHO'S STATE";
			notifyParty1.E2_RN_NKCountryCode = "DD";
			notifyParty1.E2_Postcode = "6958";
			notifyParty1.E2_Phone = "632587";
			notifyParty1.E2_Contact = "WHO'S CONTACT";
			notifyParty1.E2_Email = "WHO@WHERE.COM";

			var notifyParty2 = bill.NotifyParty2;
			notifyParty2.E2_AddressOverride = true;
			notifyParty2.E2_CompanyName = "WHERE THE WHAT";
			notifyParty2.E2_City = "WHERE'S CITY";
			notifyParty2.E2_RN_NKCountryCode = "NZ";
			notifyParty2.E2_Postcode = "5689";
			notifyParty2.E2_Phone = "369874";
			notifyParty2.E2_Contact = "WHERE'S CONTACT";

			var shipToParty = bill.ShipToParty;
			shipToParty.E2_AddressOverride = true;
			shipToParty.E2_CompanyName = "SHIP TO PARTY";
			shipToParty.E2_Address1 = "SHIP TO PARTY ADDRESS 1".PadRight(JobDocAddress.Schema.E2_Address1MaxLength, '1');
			shipToParty.E2_Address2 = "SHIP TO PARTY ADDRESS 2".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '2');
			shipToParty.E2_City = "SHIPTO'S CITY";
			shipToParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			shipToParty.E2_State = "SHIPTO'S STATE";
			shipToParty.E2_Postcode = "5689985";
			shipToParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			shipToParty.E2_GovRegNum = "DUNS23432342";
			shipToParty.E2_Contact = "SHIPTO'S CONTACT";
			shipToParty.E2_Mobile = "855369874";
			shipToParty.E2_Email = "SHIP@WHEN.COM";

			var bookingParty = bill.BookingParty;
			bookingParty.E2_AddressOverride = true;
			bookingParty.E2_CompanyName = "BOOKING PARTY";
			bookingParty.E2_Address1 = "BOOKING PARTY ADDRESS 1".PadRight(JobDocAddress.Schema.E2_Address1MaxLength, '1');
			bookingParty.E2_Address2 = "BOOKING PARTY ADDRESS 2".PadRight(JobDocAddress.Schema.E2_Address2MaxLength, '2');
			bookingParty.E2_City = "BOOKING'S CITY";
			bookingParty.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			bookingParty.E2_State = "BOOKING'S STATE";
			bookingParty.E2_Postcode = "896584";
			bookingParty.E2_GovRegNumType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
			bookingParty.E2_GovRegNum = "DUNS963854";
			bookingParty.E2_Contact = "BOOKING'S CONTACT";
			bookingParty.E2_Email = "BOOK@HOW.COM";
			bookingParty.E2_Fax = "3498644656";

			var movementDetail = bill.MovementDetail;
			var container1 = movementDetail.Containers.AddNew();
			var container2 = movementDetail.Containers.AddNew();
			var container3 = movementDetail.Containers.AddNew();
			var inBondHeader = header.InBondMovementHeaders.AddNew();
			var inBondDetail = inBondHeader.MovementDetails.AddNew(bill.PK);
			IACEBillOfLading aCEBillOfLading = inBondDetail;
			var aceContainers = new List<IACEContainer>(aCEBillOfLading.Containers);
			AssertEquals(3, aceContainers.Count);
			AssertContainsExactElementsInAnyOrder(new IACEContainer[] { container1, container2, container3 }, aceContainers);

			var entities = new List<IEntity>(aCEBillOfLading.Entities(ActionCode.Creating));
			AssertEquals(7, entities.Count);
			AssertEntity(entities[0], ACEEntityIDCodeList.Codes.Shipper, "BOB THE BUILDER", "", "", "BOB'S ADDDRESS 1", "", "BOB'S ADDDRESS 22222222222222222222", "222222222222222", "BOB'S CITY", "", "", "AU", "BOB'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "2353982343", CommunicationsNumberQualifierList.Codes.CellularPhone, "6956552243");
			AssertEntity(entities[1], ACEEntityIDCodeList.Codes.Consignee, "WENDY THE DESTROYER", "", "", "WENDY'S ADDDRESS 1", "", "WENDY'S ADDDRESS 222222222222222222", "222222222222222", "WENDY'S CITY", "", "", "NZ", "WENDY'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "9854963541", CommunicationsNumberQualifierList.Codes.Facsimile, "8563114566");
			AssertEntity(entities[2], ACEEntityIDCodeList.Codes.NotifyPartyNo1, "WHO THE WHAT", "", "", "WHO'S ADDDRESS 1", "", "WHO'S ADDDRESS 22222222222222222222", "222222222222222", "WHO'S CITY", "", "", "DD", "WHO'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "632587", CommunicationsNumberQualifierList.Codes.ElectronicMail, "WHO@WHERE.COM");
			AssertEntity(entities[3], ACEEntityIDCodeList.Codes.NotifyPartyNo2, "WHERE THE WHAT", "", "", "", "", "", "", "WHERE'S CITY", "", "", "NZ", "WHERE'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "369874", "", "");
			AssertEntity(entities[4], ACEEntityIDCodeList.Codes.SecondaryNotifyParty, "", CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, "SN67");
			AssertEntity(entities[5], ACEEntityIDCodeList.Codes.SecondaryNotifyParty, "", CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, "SN89");
			AssertEntity(entities[6], ACEEntityIDCodeList.Codes.CustomsBroker, "", CusInBondMoveDetail.ABIRoutingCodeSNPQualifier, "5678DEF68");

			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseFROBAndISF;
			entities = new List<IEntity>(aCEBillOfLading.Entities(ActionCode.Creating));
			AssertEquals(9, entities.Count);
			AssertEntity(entities[0], ACEEntityIDCodeList.Codes.Shipper, "BOB THE BUILDER", "", "", "BOB'S ADDDRESS 1", "", "BOB'S ADDDRESS 22222222222222222222", "222222222222222", "BOB'S CITY", "", "", "AU", "BOB'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "2353982343", CommunicationsNumberQualifierList.Codes.CellularPhone, "6956552243");
			AssertEntity(entities[1], ACEEntityIDCodeList.Codes.Consignee, "WENDY THE DESTROYER", "", "", "WENDY'S ADDDRESS 1", "", "WENDY'S ADDDRESS 222222222222222222", "222222222222222", "WENDY'S CITY", "", "", "NZ", "WENDY'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "9854963541", CommunicationsNumberQualifierList.Codes.Facsimile, "8563114566");
			AssertEntity(entities[2], ACEEntityIDCodeList.Codes.NotifyPartyNo1, "WHO THE WHAT", "", "", "WHO'S ADDDRESS 1", "", "WHO'S ADDDRESS 22222222222222222222", "222222222222222", "WHO'S CITY", "", "", "DD", "WHO'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "632587", CommunicationsNumberQualifierList.Codes.ElectronicMail, "WHO@WHERE.COM");
			AssertEntity(entities[3], ACEEntityIDCodeList.Codes.NotifyPartyNo2, "WHERE THE WHAT", "", "", "", "", "", "", "WHERE'S CITY", "", "", "NZ", "WHERE'S CONTACT", CommunicationsNumberQualifierList.Codes.Telephone, "369874", "", "");
			AssertEntity(entities[4], ACEEntityIDCodeList.Codes.SecondaryNotifyParty, "", CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, "SN67");
			AssertEntity(entities[5], ACEEntityIDCodeList.Codes.SecondaryNotifyParty, "", CusInBondMoveDetail.SCACOrFIRMSOfSNPQualifier, "SN89");
			AssertEntity(entities[6], ACEEntityIDCodeList.Codes.CustomsBroker, "", CusInBondMoveDetail.ABIRoutingCodeSNPQualifier, "5678DEF68");
			AssertEntity(entities[7], ACEEntityIDCodeList.Codes.ShipTo, "SHIP TO PARTY", CusInBondMoveDetail.DUNSQualifier, "DUNS23432342", "SHIP TO PARTY ADDRESS 1111111111111", "111111111111111", "SHIP TO PARTY ADDRESS 2222222222222", "222222222222222", "SHIPTO'S CITY", "SH", "5689985", "US", "SHIPTO'S CONTACT", CommunicationsNumberQualifierList.Codes.CellularPhone, "855369874", CommunicationsNumberQualifierList.Codes.ElectronicMail, "SHIP@WHEN.COM");
			AssertEntity(entities[8], ACEEntityIDCodeList.Codes.BookingParty, "BOOKING PARTY", CusInBondMoveDetail.DUNSQualifier, "DUNS963854", "BOOKING PARTY ADDRESS 1111111111111", "111111111111111", "BOOKING PARTY ADDRESS 2222222222222", "222222222222222", "BOOKING'S CITY", "BO", "896584", "US", "BOOKING'S CONTACT", CommunicationsNumberQualifierList.Codes.ElectronicMail, "BOOK@HOW.COM", CommunicationsNumberQualifierList.Codes.Facsimile, "3498644656");
		}

		void AssertEntity(IEntity entity, ZString entityCode, ZString entityName, ZString codeQualifier, ZString idCode,
			ZString addressLine1, ZString addressLine2, ZString addressLine3, ZString addressLine4, ZString cityName, ZString stateProvince, ZString postalCode, ZString countryCode,
			ZString contactName, ZString commNumberQualifier, ZString communicationsNumber, ZString commNumberQualifier2, ZString communicationsNumber2)
		{
			CombineAssertions(delegate
			{
				AssertEntity(entity, entityCode, entityName, codeQualifier, idCode, addressLine1, addressLine2, addressLine3, addressLine4, cityName, stateProvince, postalCode, countryCode);
				var adminContact = entity.AdminContact;
				AssertNotNull("AdminContact", adminContact);
				AssertNotifyPartyContact(adminContact, contactName, commNumberQualifier, communicationsNumber, commNumberQualifier2, communicationsNumber2);
			});
		}

		void AssertEntity(IEntity entity, ZString entityCode, ZString entityName, ZString codeQualifier, ZString idCode,
			ZString addressLine1, ZString addressLine2, ZString addressLine3, ZString addressLine4, ZString cityName, ZString stateProvince, ZString postalCode, ZString countryCode)
		{
			AssertEntity(entity, entityCode, entityName, codeQualifier, idCode);
			AssertEntityAddress(entity, addressLine1, addressLine2, addressLine3, addressLine4, cityName, stateProvince, postalCode, countryCode);
		}

		void AssertEntity(IEntity entity, ZString entityCode, ZString entityName, ZString codeQualifier, ZString idCode)
		{
			AssertEquals("EntityCode", entityCode, entity.EntityCode);
			AssertEquals("EntityName", entityName, entity.EntityName);
			AssertEquals("CodeQualifier", codeQualifier, entity.CodeQualifier);
			AssertEquals("IDCode", idCode, entity.IDCode);
		}

		void AssertEntityAddress(IEntityAddress entityAddress, ZString addressLine1, ZString addressLine1Part2, ZString addressLine2, ZString addressLine2Part2, ZString cityName, ZString stateProvince, ZString postalCode, ZString countryCode)
		{
			AssertEquals("AddressLine1", addressLine1, entityAddress.AddressLine1);
			AssertEquals("AddressLine1Part2", addressLine1Part2, entityAddress.AddressLine1Part2);
			AssertEquals("AddressLine2", addressLine2, entityAddress.AddressLine2);
			AssertEquals("AddressLine2Part2", addressLine2Part2, entityAddress.AddressLine2Part2);
			AssertEquals("CityName", cityName, entityAddress.CityName);
			AssertEquals("StateProvince", stateProvince, entityAddress.StateProvince);
			AssertEquals("PostalCode", postalCode, entityAddress.PostalCode);
			AssertEquals("CountryCode", countryCode, entityAddress.CountryCode);
		}

		void AssertNotifyPartyContact(INotifyPartyContact notifyPartyContact, ZString contactName, ZString commNumberQualifier, ZString communicationsNumber, ZString commNumberQualifier2, ZString communicationsNumber2)
		{
			AssertEquals("ContactName", contactName, notifyPartyContact.ContactName);
			AssertEquals("CommNumberQualifier", commNumberQualifier, notifyPartyContact.CommNumberQualifier);
			AssertEquals("CommunicationsNumber", communicationsNumber, notifyPartyContact.CommunicationsNumber);
			AssertEquals("CommNumberQualifier2", commNumberQualifier2, notifyPartyContact.CommNumberQualifier2);
			AssertEquals("CommunicationsNumber2", communicationsNumber2, notifyPartyContact.CommunicationsNumber2);
		}

		public void TestMoveDetailHasBillOnFile()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;

			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;

			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.OnFile;
			AssertEquals("One bill is on file", true, moveDetail.HasBillOnFile);
			moveDetail.B9_CustomsStatus = AMSBillCustomsStatusList.Codes.NotOnFile;
			AssertEquals("Nothing is on file", false, moveDetail.HasBillOnFile);
		}

		public void TestOnFactorySavingBeforeTransactionCore()
		{
			Factory.Save();
			AssertEquals("Precondition: No Message Status Change event", 0, Bill.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).Count());
			moveDetail.B9_MessageStatus = AMSBillMessageStatusList.Codes.Adding;
			Factory.Save();
			var messagelog = Bill.Logs.Find(o => o.SL_SE_NKEvent == Events.MessageStatusChange.Code).FirstOrDefault();
			AssertNotNull("A Message Status Change event should be added", messagelog);
			AssertEquals("Message Status Change Event Reference", "ANG - Adding", messagelog.SL_Reference);
		}

		#region Implementation

		CusInBondHeader Header
		{
			get => (CusInBondHeader)header;
			set => header = value;
		}

		CusInBondBill Bill
		{
			get => (CusInBondBill)bill;
			set => bill = value;
		}

		CusInBondMoveHeader MoveHeader
		{
			get => (CusInBondMoveHeader)moveHeader;
			set => moveHeader = value;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			Header = factory.New<CusInBondHeader>();
			Bill = Header.Bills.AddNew();
			MoveHeader = Header.MovementHeader;
			return MoveHeader.MovementDetails.AddNew(Bill.PK);
		}

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header)
		{
			var moveHeader = header.Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			return moveHeader;
		}

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader()
		{
			return Factory.New<CusInBondHeader>();
		}

		protected override Customs.Business.CusInBondBill CreateNewCusInBondBill(Customs.Business.CusInBondHeader header)
		{
			return ((CusInBondHeader)header).Bills.AddNew();
		}

		protected override CusInBondMoveDetail CreateNewCusInBondMoveDetail(US.Business.CusInBondMoveHeader moveHeader)
		{
			return ((CusInBondMoveHeader)moveHeader).MovementDetails.AddNew();
		}

		#endregion
	}
}
