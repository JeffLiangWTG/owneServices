using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.AMS.Messaging.Interface;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeader))]
	sealed class CusInBondMoveHeaderTest : CusInBondMoveHeaderTest<CusInBondMoveHeader>
	{
		public void TestIsNVOCCHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			AssertEquals("moveHeader.IsNVOCCHeader", false, moveHeader.IsNVOCCHeader);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			AssertEquals("moveHeader.IsNVOCCHeader", true, moveHeader.IsNVOCCHeader);
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			AssertEquals("moveHeader.IsNVOCCHeader", false, moveHeader.IsNVOCCHeader);
		}

		public void TestDeleteMoveDetails()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.InBondMovementHeaders.AddNew();
			AssertEquals(true, moveHeader.CanDelete);
			AssertEquals("", moveHeader.ReasonForNotAbleToDelete);
			moveHeader.Messages.AddNew();
			AssertEquals(false, moveHeader.CanDelete);
			AssertEquals(ValidationConstants.MoveHeader.CannotDeleteMovementWhenMessageExists, moveHeader.ReasonForNotAbleToDelete);
		}

		[TestDate(2015, 1, 21, 21, 52, 0)]
		public void TestConcurrency()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			Factory.RefreshEnabled = false;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var moveHeaderInDiffFactory = newFactory.Load<CusInBondMoveHeader>(moveHeader.PK);
			moveHeaderInDiffFactory.BM_CustomsStatus = AMSBillMessageStatusList.Codes.AwaitingDeparture;
			moveHeaderInDiffFactory.BM_ExportDate = new ZDateTime(2015, 1, 24);
			newFactory.Save();
			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveHeader.BM_ExportDate);
			moveHeader.BM_ExportLadenOn = "HELLO";
			var handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(ZString.Empty, moveHeader.BM_CustomsStatus);
			AssertEquals(ZDateTime.Empty, moveHeader.BM_ExportDate);
			AssertEquals("HELLO", moveHeader.BM_ExportLadenOn);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
AMS Movement Header (CargoWise Support @ 21 Jan 2015 21:52:00)
	Export Date
	Customs Status (Critical change)", handler.ReportInformationMessage);
			moveHeader.Delete();
			handler = new Customs.Business.Testing.NotificationHandlerForTest();
			try
			{
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, handler, true);
			}

			AssertEquals(true, moveHeader.IsDeleted);
			AssertEquals(true, moveHeader.HasChanges);
			AssertEquals("ReportInformationCaption", "WARNING", handler.ReportInformationCaption);
			AssertMultilineASCIIEquals("ReportInformationMessage", @"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:
AMS Movement Header (CargoWise Support @ 21 Jan 2015 21:52:00) (pending delete)
	Export Date
	Customs Status (Critical change)", handler.ReportInformationMessage);
		}

		public void TestOceanBillNVOCCMoveDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var pttMoveHeader = header.PTTMovements.AddNew();
			AssertNull(pttMoveHeader.OceanBillNVOCCMoveDetail);
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			AssertNull(inbondMoveHeader.OceanBillNVOCCMoveDetail);
			var amsMoveHeader = header.MovementHeader;
			AssertNull(amsMoveHeader.OceanBillNVOCCMoveDetail);
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var pttMoveDetail = pttMoveHeader.OceanBillNVOCCMoveDetail;
			AssertNotNull(pttMoveDetail);
			var inbondMoveDetail = inbondMoveHeader.OceanBillNVOCCMoveDetail;
			AssertNotNull(inbondMoveDetail);
			AssertNull(amsMoveHeader.OceanBillNVOCCMoveDetail);
			Factory.Save();
			AssertEquals(pttMoveDetail, pttMoveHeader.OceanBillNVOCCMoveDetail);
			AssertEquals("pttMoveDetail.IsDeleted", false, pttMoveDetail.IsDeleted);
			AssertEquals(inbondMoveDetail, inbondMoveHeader.OceanBillNVOCCMoveDetail);
			AssertEquals("inbondMoveDetail.IsDeleted", false, inbondMoveDetail.IsDeleted);
			var newFactory = new BusinessObjectFactory();
			header = newFactory.Load<CusInBondHeader>(header.PK);
			amsMoveHeader = header.MovementHeader;
			AssertNull(amsMoveHeader.OceanBillNVOCCMoveDetail);
			pttMoveHeader = (CusInBondMoveHeader)header.PTTMovements.FindByPK(pttMoveHeader.PK);
			pttMoveDetail = newFactory.Load<CusInBondMoveDetail>(pttMoveDetail.PK);
			AssertEquals(pttMoveDetail, pttMoveHeader.OceanBillNVOCCMoveDetail);
			inbondMoveHeader = newFactory.Load<CusInBondMoveHeader>(inbondMoveHeader.PK);
			inbondMoveDetail = newFactory.Load<CusInBondMoveDetail>(inbondMoveDetail.PK);
			AssertEquals(inbondMoveDetail, inbondMoveHeader.OceanBillNVOCCMoveDetail);
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			AssertEquals("pttMoveDetail.IsDeleted", true, pttMoveDetail.IsDeleted);
			AssertEquals("inbondMoveDetail.IsDeleted", true, inbondMoveDetail.IsDeleted);
			AssertEquals("pttMoveHeader.IsDeleted", true, pttMoveHeader.IsDeleted);
			AssertNull(inbondMoveHeader.OceanBillNVOCCMoveDetail);
		}

		public void TestCarrierCodeForPTT()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "AUC";
			company.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "AUB";
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = branch.PK;
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			header.BH_CarrierSCAC = "OTT1";
			var companyFIRMCode = company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "1913", Core.Constants.CountryCodes.UnitedStates);
			var movementHeader = header.MovementHeader;
			using (FreightDataRegistry.Instance.UseFIRMSCodeAsFilerForPTTMessages.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Use carrier code if registry is false", "OTT1", movementHeader.CarrierCodeForPTT);
			}

			using (FreightDataRegistry.Instance.UseFIRMSCodeAsFilerForPTTMessages.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Use firms code of company org proxy if registry is true", "1913", movementHeader.CarrierCodeForPTT);
			}

			company.OrgProxy.CustomsCodes.RemoveAndDelete(companyFIRMCode);
			var branchFIRMCode = branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FIRMSCode, "1916", Core.Constants.CountryCodes.UnitedStates);
			using (FreightDataRegistry.Instance.UseFIRMSCodeAsFilerForPTTMessages.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("Use firms code of branch org proxy if registry is true", "1916", movementHeader.CarrierCodeForPTT);
			}

			branch.OrgProxy.CustomsCodes.RemoveAndDelete(branchFIRMCode);
			using (FreightDataRegistry.Instance.UseFIRMSCodeAsFilerForPTTMessages.SetTemporaryValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, true))
			{
				AssertEquals("Always use carrier code if there is no firms code", "OTT1", movementHeader.CarrierCodeForPTT);
			}
		}

		public void TestIMessageResponseNotificatorMembers()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "B!1";
			staff1.GS_EmailAddress = "BOB@WHERE.COM";
			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "W!2";
			staff2.GS_EmailAddress = "WENDY@WHERE.COM";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "J!3";
			staff3.GS_EmailAddress = "JOE@WHERE.COM";
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var amsMoveHeader = header.MovementHeader;
			var amsBill = bill.MovementDetail;
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondBill = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			IMessageResponseNotificator amsNotificator = amsMoveHeader;
			IMessageResponseNotificator inBondNotificator = inBondMoveHeader;
			AssertEquals(ZString.Empty, amsNotificator.GetFallbackEmailAddressRecipient());
			AssertEquals(ZString.Empty, inBondNotificator.GetFallbackEmailAddressRecipient());
			var lastAMSMessage1 = amsMoveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			lastAMSMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			lastAMSMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			lastAMSMessage1.EM_MessageNum = "~10001";
			lastAMSMessage1.EM_SystemCreateUser = staff2.GS_Code;
			lastAMSMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			AssertEquals(staff2.GS_EmailAddress, amsNotificator.GetFallbackEmailAddressRecipient());
			AssertEquals(staff2.GS_EmailAddress, inBondNotificator.GetFallbackEmailAddressRecipient());
			var lastAMSMessage2 = amsMoveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			lastAMSMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			lastAMSMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			lastAMSMessage2.EM_MessageNum = "~10002";
			lastAMSMessage2.EM_SystemCreateUser = staff1.GS_Code;
			lastAMSMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			AssertEquals(staff1.GS_EmailAddress, amsNotificator.GetFallbackEmailAddressRecipient());
			AssertEquals(staff1.GS_EmailAddress, inBondNotificator.GetFallbackEmailAddressRecipient());
			var lastInBondMessage1 = inBondMoveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			lastInBondMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			lastInBondMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.SubsequentInBond;
			lastInBondMessage1.EM_MessageNum = "~10003";
			lastInBondMessage1.EM_SystemCreateUser = staff3.GS_Code;
			lastInBondMessage1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-2);
			AssertEquals(staff1.GS_EmailAddress, amsNotificator.GetFallbackEmailAddressRecipient());
			AssertEquals(staff3.GS_EmailAddress, inBondNotificator.GetFallbackEmailAddressRecipient());
			var lastInBondMessage2 = inBondMoveHeader.Messages.AddNew(typeof(AMSEDIMessage));
			lastInBondMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			lastInBondMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.SubsequentInBond;
			lastInBondMessage2.EM_MessageNum = "~10004";
			lastInBondMessage2.EM_SystemCreateUser = staff2.GS_Code;
			lastInBondMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			AssertEquals(staff1.GS_EmailAddress, amsNotificator.GetFallbackEmailAddressRecipient());
			AssertEquals(staff2.GS_EmailAddress, inBondNotificator.GetFallbackEmailAddressRecipient());
		}

		public void TestBooleanFlags()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			AssertEquals("IsAMSMovement", true, moveHeader.IsAMSMovement);
			AssertEquals("IsInBondMovement", false, moveHeader.IsInBondMovement);
			AssertEquals("IsPTTMovement", false, moveHeader.IsPTTMovement);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			AssertEquals("IsAMSMovement", false, moveHeader.IsAMSMovement);
			AssertEquals("IsInBondMovement", true, moveHeader.IsInBondMovement);
			AssertEquals("IsPTTMovement", false, moveHeader.IsPTTMovement);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			AssertEquals("IsAMSMovement", false, moveHeader.IsAMSMovement);
			AssertEquals("IsInBondMovement", false, moveHeader.IsInBondMovement);
			AssertEquals("IsPTTMovement", true, moveHeader.IsPTTMovement);
		}

		public void TestFindRelatedAMSMovementForBM_ManifestSequenceNumber()
		{
			var inbondHeader = Factory.New<CusInBondHeader>();
			AssertEquals("PreCondition", SubApplicationCodeList.Codes.AMS, inbondHeader.MovementHeader.BM_SubApplicationCode);
			var pttMove = inbondHeader.PTTMovements.AddNew();
			pttMove.BM_ManifestSequenceNumber = "123456";
			AssertEquals("123456", pttMove.BM_ManifestSequenceNumber);
			AssertEquals("123456", inbondHeader.MovementHeader.BM_ManifestSequenceNumber);
		}

		public void TestBM_SubApplicationCodeDescription()
		{
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			foreach (ICodeDescription pair in new SubApplicationCodeList())
			{
				moveHeader.BM_SubApplicationCode = pair.Code;
				AssertEquals(pair.Code, pair.Description, moveHeader.BM_SubApplicationCodeDescription);
			}
		}

		public void TestMoveDetailFieldsAreClearedIfSameWithHeader()
		{
			var header = Factory.New<CusInBondHeader>();
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			var moveDetail1 = inbondMoveHeader.MovementDetails.AddNew();
			var moveDetail1Row = ((IBusinessObjectInternals)moveDetail1).Row;
			moveDetail1.B9_ExportDate = new ZDateTime(2012, 4, 1);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			moveDetail1.B9_ExportLadenOn = "BOB BOAT";
			AssertEquals("BOB BOAT", moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			moveDetail1.B9_ForeignDestPortKCode = "12345";
			AssertEquals("12345", moveDetail1Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			var moveDetail2 = inbondMoveHeader.MovementDetails.AddNew();
			var moveDetail2Row = ((IBusinessObjectInternals)moveDetail2).Row;
			moveDetail2.B9_ExportDate = new ZDateTime(2012, 5, 1);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			moveDetail2.B9_ExportLadenOn = "WENDY BOAT";
			AssertEquals("WENDY BOAT", moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			moveDetail2.B9_ForeignDestPortKCode = "67890";
			AssertEquals("67890", moveDetail2Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			inbondMoveHeader.BM_ExportDate = new ZDateTime(2012, 4, 1);
			AssertEquals(new ZDateTime(2012, 4, 1), moveDetail1.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail2.B9_ExportDate);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			inbondMoveHeader.BM_ExportDate = new ZDateTime(2012, 5, 1);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail1.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			inbondMoveHeader.BM_ExportDate = new ZDateTime(2012, 6, 1);
			AssertEquals(new ZDateTime(2012, 6, 1), moveDetail1.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 6, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			moveDetail1.B9_ExportDate = new ZDateTime(2012, 5, 1);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail1.B9_ExportDate);
			AssertEquals(new ZDateTime(2012, 5, 1), moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			AssertEquals(new ZDateTime(2012, 6, 1), moveDetail2.B9_ExportDate);
			AssertEquals(DBNull.Value, moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportDate]);
			inbondMoveHeader.BM_ExportLadenOn = "BOB BOAT";
			AssertEquals("BOB BOAT", moveDetail1.B9_ExportLadenOn);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			AssertEquals("WENDY BOAT", moveDetail2.B9_ExportLadenOn);
			AssertEquals("WENDY BOAT", moveDetail2Row[CusInBondMoveDetail.Schema.B9_ExportLadenOn]);
			inbondMoveHeader.BM_ForeignDestPortKCode = "12345";
			AssertEquals("12345", moveDetail1.B9_ForeignDestPortKCode);
			AssertEquals(ZString.Empty, moveDetail1Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
			AssertEquals("67890", moveDetail2.B9_ForeignDestPortKCode);
			AssertEquals("67890", moveDetail2Row[CusInBondMoveDetail.Schema.B9_ForeignDestPortKCode]);
		}

		public void TestManifestSequenceNumberFormat()
		{
			IManifestMessageAttachee manifest = aMSMoveHeader;
			Header.BH_GB = GlbBranch.CurrentBranch.PK;
			manifest.ManifestSequenceNumber = "ABD";
			AssertEquals("AMSMoveHeader.BM_ManifestSequenceNumber", "", aMSMoveHeader.BM_ManifestSequenceNumber);
			AssertEquals("manifest.ManifestSequenceNumber", "", manifest.ManifestSequenceNumber);
			manifest.ManifestSequenceNumber = "010";
			AssertEquals("AMSMoveHeader.BM_ManifestSequenceNumber", "000010", aMSMoveHeader.BM_ManifestSequenceNumber);
			AssertEquals("manifest.ManifestSequenceNumber", "000010", manifest.ManifestSequenceNumber);
			aMSMoveHeader.BM_ManifestSequenceNumber = "2 33";
			AssertEquals("AMSMoveHeader.BM_ManifestSequenceNumber", "000233", aMSMoveHeader.BM_ManifestSequenceNumber);
			AssertEquals("manifest.ManifestSequenceNumber", "000233", manifest.ManifestSequenceNumber);
		}

		public void TestIControllerIDProviderMembers()
		{
			IControllerIDProvider provider = aMSMoveHeader;
			AssertEquals(ControllerIDs.JobConsol, provider.ControllerID);
			AssertEquals(consol.PK.ToGuid(), provider.BusinessObjectPK);
			Header.BH_ParentID = ZGuid.Empty;
			Header.BH_ParentTableCode = ZString.Empty;
			AssertEquals(ControllerIDs.Customs.US.AMS, provider.ControllerID);
			AssertEquals(Header.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestIManifestMessageAttacheeMembers()
		{
			IManifestMessageAttachee manifest = aMSMoveHeader;
			Header.BH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), manifest.Branch);
			AssertEquals(Factory, manifest.Factory);
			AssertEquals(aMSMoveHeader.Messages, manifest.Messages);
			aMSMoveHeader.BM_CustomsStatus = "BDD";
			AssertEquals("BDD", manifest.MessageStatus);
			manifest.MessageStatus = "KDS";
			AssertEquals("KDS", aMSMoveHeader.BM_CustomsStatus);
			AssertEquals(consol, manifest.TopLevelBusinessObject);
			AssertEquals(ControllerIDs.JobConsol, manifest.ControllerID);
			AssertEquals(consol.PK.ToGuid(), manifest.BusinessObjectPK);
			AssertEquals(consol.Logs, manifest.TopLevelBusinessObjectLogs);
			Header.BH_CarrierSCAC = "SD43";
			AssertEquals("SD43", manifest.CarrierCode);
			Header.BH_ImportTransportMode = TransportTypeList.Codes.Truck;
			AssertEquals(TransportTypeList.Codes.Truck, manifest.ModeOfTransportationCode);
			Header.BH_ImportConveyanceCountry = "DS";
			AssertEquals("DS", manifest.ConveyanceCountryCode);
			Header.BH_ImportConveyanceName = "IMP DUMMY VESSEL";
			AssertEquals("IMP DUMMY VESSEL", manifest.ConveyanceName);
			Header.BH_VoyageNumber = "E343";
			AssertEquals("E343", manifest.VoyageNumber);
			AssertEquals("", manifest.ManifestSequenceNumber);
			aMSMoveHeader.BM_ManifestSequenceNumber = "000123";
			AssertEquals("000123", manifest.ManifestSequenceNumber);
			AssertEquals(ZBool.False, manifest.IsPaperlessMIBParticipant);
			Header.BH_LloydsNumber = "9323423";
			AssertEquals("9323423", manifest.ConveyanceCode);
			Header.BH_ExportFlag = ExportTypeList.Codes.OutboundCargo;
			AssertEquals(ZBool.True, manifest.IsOutboundCargo);
			consol.JK_UniqueConsignRef = "CONS2343";
			AssertEquals("CONS2343", manifest.JobNumber);
			var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition, "IMakeBondCloseDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IMakeBondCloseDisposition6263, "IMakeBondCloseDisposition6263", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INeutralInBondDisposition, "INeutralInBondDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsExamDisposition, "IsExamDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName5 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldDisposition, "IsHoldDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName6 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IsHoldExamRemovedDisposition, "IsHoldExamRemovedDisposition", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName7 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.HoldRemovedExamCompletedMapCode, "HoldRemovedExamCompletedMapCode", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "02", "Entry Advisory", startDate, endDate);
			Factory.Save();
			var bill = Header.Bills.AddNew();
			bill.B0_IssuerCode = "OTT1";
			bill.B0_MasterBillNumber = "MB323423";
			var moveDetail = bill.MovementDetail;
			Factory.Save();
			AssertNoExceptionThrown(delegate
			{
				manifest.UpdateIncomingBillStatus("OTT1", "Z#@#$", AMSMessageSubTypeList.Codes.AmendingAdd, false, null);
			});
			AssertEquals("", moveDetail.B9_MessageStatus);
			AssertEquals("", moveDetail.B9_CustomsStatus);
			manifest.UpdateIncomingBillStatus("OTT1", "MB323423", AMSMessageSubTypeList.Codes.AmendingAdd, false, null);
			AssertEquals(AMSBillMessageStatusList.Codes.Added, moveDetail.B9_MessageStatus);
			AssertEquals(AMSBillCustomsStatusList.Codes.OnFile, moveDetail.B9_CustomsStatus);
			manifest.UpdateIncomingBillStatus("OTT1", "MB323423", AMSMessageSubTypeList.Codes.AmendingDelete, false, null);
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, moveDetail.B9_MessageStatus);
			AssertEquals(AMSBillCustomsStatusList.Codes.NotOnFile, moveDetail.B9_CustomsStatus);
			manifest.UpdateIncomingBillStatus("OTT1", "MB323423", AMSMessageSubTypeList.Codes.AmendingUpdate, false, null);
			AssertEquals(AMSBillMessageStatusList.Codes.Updated, moveDetail.B9_MessageStatus);
			AssertEquals(AMSBillCustomsStatusList.Codes.OnFile, moveDetail.B9_CustomsStatus);
			manifest.UpdateEstimatedDateOfArrival(ZDateTime.Today.AddDays(2));
			AssertEquals(ZDateTime.Today.AddDays(2), Header.BH_ETA);
			Header.DispositionCodes.DeleteAll();
			AssertEquals("Precondition: No Status Change events", 0, Header.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count());
			manifest.UpdatConveyanceEventInformation(ConveyanceEventCodeList.Codes.ArrivalOfVessel, new ZDateTime(2012, 3, 2));
			AssertEquals(1, Header.DispositionCodes.Count);
			AssertDisposition(Header.DispositionCodes[0], Header.DispositionCodes[0].US_Code, Header.DispositionCodes[0].US_DispositionDate);
			var log = Header.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull("A Status Change event should be added", log);
			AssertEquals("Status Change event reference", "AAD - Arrival of vessel", log.SL_Reference);
			manifest.UpdatConveyanceEventInformation(ConveyanceEventCodeList.Codes.ArrivalOfVessel, new ZDateTime(2012, 3, 2));
			AssertEquals("No new as it's the same data", 1, Header.DispositionCodes.Count);
			AssertDisposition(Header.DispositionCodes[0], Header.DispositionCodes[0].US_Code, Header.DispositionCodes[0].US_DispositionDate);
			AssertEquals("No new Status Change events", 1, Header.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count());
			bill.DispositionCodes.DeleteAll();
			AssertEquals("Precondition: No Status Change events", 0, bill.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count());
			manifest.UpdateDispositionInformation("OTT2", "MB323423", "02", new ZDateTime(2012, 4, 2));
			AssertEquals(0, bill.DispositionCodes.Count);
			manifest.UpdateDispositionInformation("OTT1", "MB323423", "02", new ZDateTime(2012, 4, 2));
			AssertEquals(1, bill.DispositionCodes.Count);
			AssertDisposition(bill.DispositionCodes[0], "02", new ZDateTime(2012, 4, 2));
			log = bill.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).FirstOrDefault();
			AssertNotNull("A Status Change event should be added", log);
			AssertEquals("Status Change event reference", "02 - Entry Advisory", log.SL_Reference);
			manifest.UpdateDispositionInformation("OTT1", "MB323423", "02", new ZDateTime(2012, 4, 2));
			AssertEquals("Not added as the data is the same", 1, bill.DispositionCodes.Count);
			AssertDisposition(bill.DispositionCodes[0], "02", new ZDateTime(2012, 4, 2));
			AssertEquals("No new Status Change events", 1, bill.Logs.Find(o => o.SL_SE_NKEvent == Events.StatusChange.Code).Count());
		}

		void AssertDisposition(US.Business.DispositionData disposition, ZString code, ZDateTime date)
		{
			AssertEquals("Code", code, disposition.US_Code);
			AssertEquals("Date", date, disposition.US_DispositionDate);
		}

		public void TestIMessageAttacheeInHeaderMembers()
		{
			consol.JK_UniqueConsignRef = "CONS2343";
			IMessageAttacheeInHeader manifest = aMSMoveHeader;
			AssertEquals("Record Identifier", "CONS2343", manifest.RecordIdentifier);
			AssertEquals("Header PK", Header.PK, manifest.HeaderPK);
			AssertEquals("Record Type", MessageAttacheeRecordType.VesselMovement, manifest.RecordType);
			AssertEquals("Record Type Description", SubApplicationCodeList.Descriptions.AMS, manifest.RecordTypeDescription);
			AssertEquals("PK", aMSMoveHeader.PK, manifest.PK);
			var pttMovement = Header.PTTMovements.AddNew();
			pttMovement.BM_InBondCarrierID = "12-12345678";
			IMessageAttacheeInHeader pttManifest = pttMovement;
			AssertEquals("Record Identifier", "Carrier: " + "12-12345678", pttManifest.RecordIdentifier);
			AssertEquals("Header PK", Header.PK, pttManifest.HeaderPK);
			AssertEquals("Record Type", MessageAttacheeRecordType.PermitToTransferMovement, pttManifest.RecordType);
			AssertEquals("Record Type Description", SubApplicationCodeList.Descriptions.PermitToTransfer, pttManifest.RecordTypeDescription);
			AssertEquals("PK", pttMovement.PK, pttManifest.PK);
			var inBondMovement = Header.InBondMovementHeaders.AddNew();
			inBondMovement.InBondNumber = "INB3234";
			inBondMovement.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			IMessageAttacheeInHeader inbManifest = inBondMovement;
			AssertEquals("Record Identifier", "In-Bond Number: INB3234, Entry Type: 63", inbManifest.RecordIdentifier);
			AssertEquals("Header PK", Header.PK, inbManifest.HeaderPK);
			AssertEquals("Record Type", MessageAttacheeRecordType.InBondMovement, inbManifest.RecordType);
			AssertEquals("Record Type Description", SubApplicationCodeList.Descriptions.MasterInBond, inbManifest.RecordTypeDescription);
			AssertEquals("PK", inBondMovement.PK, inbManifest.PK);
		}

		public override void TestLookups()
		{
			base.TestLookups();
			AssertEquals(typeof(CusInBondMoveHeaderLookups), moveHeader.Lookups.GetType());
		}

		public override void TestValidation()
		{
			base.TestValidation();
			AssertEquals(typeof(CusInBondMoveHeaderValidation), moveHeader.Validation.GetType());
		}

		[ExpectNoExceptions]
		public void TestMajorMarkAsNeedingValidationCore()
		{
			var mockHeader = Factory.NewMoq<CusInBondHeader>();
			var header = mockHeader.Object;
			var mockMoveHeader = Factory.NewMoq<CusInBondMoveHeader>();
			var moveHeader = mockMoveHeader.Object;
			moveHeader.BM_BH = header.PK;
			var mockMoveDetail = Factory.NewMoq<CusInBondMoveDetail>();
			var moveDetail = mockMoveDetail.Object;
			moveHeader.MovementDetails.Add(moveDetail);
			moveHeader.BM_BH = ZGuid.Empty;
			mockMoveHeader.Protected().Setup("MarkAsNeedingValidationCore");
			moveHeader.BM_BH = header.PK;
			mockMoveHeader.VerifyAll();
		}

		public void TestICBPEDIMessageMessageTextNumberPlaceHolderFillerMembers()
		{
			var org = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var scac1 = org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SCA1", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				Header.BH_CarrierSCAC = "SD43";
				var bill = Header.Bills.AddNew();
				bill.B0_IssuerCode = "XXGK";
				bill.B0_MasterBillNumber = "HB32342355";
				bill.B0_MasterInBondIndicator = ZBool.True;
				var inBondNumber = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid()).PeekPreliminaryFormatted(Factory);
				inBondNumber += Enterprise.Customs.US.Business.InBondNumberCheckDigitCalculator.GetCheckDigit(inBondNumber);
				var messageData = "START{0}ENDHELLO WORLD";
				var defaultMessageText = string.Format(messageData, AMSEDIMessage.InBondNumberPlaceHolder);
				var message = Factory.New<AMSEDIMessage>();
				message.EM_MessageText = defaultMessageText;
				var inBondMovement = Header.InBondMovementHeaders.AddNew();
				var inBondMoveDetail = inBondMovement.MovementDetails.AddNew(bill.PK);
				ICBPEDIMessageMessageTextNumberPlaceHolderFiller filler = aMSMoveHeader;
				filler.Fill(message);
				AssertEquals("", aMSMoveHeader.BM_ManifestSequenceNumber);
				AssertNull(aMSMoveHeader.MSNCusEntryNumber);
				AssertEquals("", aMSMoveHeader.InBondNumber);
				AssertEquals(string.Format(messageData, AMSEDIMessage.InBondNumberPlaceHolder), message.EM_MessageText);
				aMSMoveHeader.BM_ManifestSequenceNumber = ZString.Empty;
				message.EM_MessageText = defaultMessageText;
				AssertEquals(ZString.Empty, inBondMovement.BM_ManifestSequenceNumber);
				filler = inBondMovement;
				filler.Fill(message);
				AssertEquals("", aMSMoveHeader.BM_ManifestSequenceNumber);
				AssertEquals("", inBondMovement.BM_ManifestSequenceNumber);
				AssertNull(inBondMovement.MSNCusEntryNumber);
				AssertEquals(inBondNumber, inBondMovement.InBondNumber);
				AssertEquals(string.Format(messageData, inBondNumber.PadRight(AMSEDIMessage.InBondNumberPlaceHolder.Length)), message.EM_MessageText);
				inBondNumber = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid()).PeekPreliminaryFormatted(Factory);
				inBondNumber += Enterprise.Customs.US.Business.InBondNumberCheckDigitCalculator.GetCheckDigit(inBondNumber);
				inBondMovement.InBondNumber = ZString.Empty;
				var sendingObject = new MessageSendingObject(bill.MovementDetail, ActionCode.Creating);
				sendingObject.MB_Send = true;
				var builder = new ACEAMSMessageBuilder(sendingObject, ActionCode.Creating);
				message = builder.PopulateMessage();
				filler = aMSMoveHeader;
				AssertContains(AMSEDIMessage.InBondNumberPlaceHolder, message.EM_MessageText);
				Factory.Save();
				filler.Fill(message);
				AssertNotContains(AMSEDIMessage.InBondNumberPlaceHolder, message.EM_MessageText);
				AssertEquals(message.EM_MessageText.Replace(AMSEDIMessage.InBondNumberPlaceHolder, inBondNumber.PadRight(AMSEDIMessage.InBondNumberPlaceHolder.Length)), message.EM_MessageText);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestIsPTTMovement()
		{
			aMSMoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			AssertEquals("PTT Move Header", true, aMSMoveHeader.IsPTTMovement);
		}

		public override void TestThrowAwayInBondNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill = header.Bills.AddNew();
			var inbondMoveHeader = header.InBondMovementHeaders.AddNew();
			inbondMoveHeader.InBondNumber = "693000140";
			var inbondMoveHeaderPK = inbondMoveHeader.PK;
			var query = new ZQuery(CusEntryNumSchema.CE_EntryNum, "693000140");
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, "INB");
			var inbondNum = Factory.Load<Common.CusEntryNumber>(query);
			AssertEquals("CusEntryNum for inb. moveHeader exists", 1, inbondNum.Length);
			header.InBondMovementHeaders.DeleteAll();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			inbondNum = newFactory.Load<Common.CusEntryNumber>(query);
			AssertEquals("Movement Header was deleted with his InBond number", 0, inbondNum.Length);
		}

		[TestDate(2015, 09, 28)]
		public void TestCS00376010()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			header.BH_CarrierSCAC = "OTT1";
			header.BH_RL_NKPortUnlading = "USLAX";
			header.BH_ETA = ZDateTime.Today;
			header.BH_ImportConveyanceName = "ADELAIDE EXPRESS";
			header.BH_VoyageNumber = "111";
			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.China;
			header.MovementHeader.BM_ManifestSequenceNumber = "000001";
			header.OceanBill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.MasterBill;
			header.OceanBill.B0_IssuerCode = "OTT1";
			header.OceanBill.B0_MasterBillNumber = "08052015";
			header.OceanBill.B0_Firms = "W110";
			header.OceanBillPTTMovement.OceanBillNVOCCMoveDetail.B9_InBoundQty = 100;
			var bill = header.Bills.AddNew();
			bill.B0_IssuerCode = "OTT1";
			bill.B0_MasterBillNumber = "08052015";
			bill.B0_ManifestQty = 100;
			bill.B0_ManifestUQ = "PAL";
			bill.B0_Weight = 5000m;
			bill.B0_WeightUQ = "KG";
			bill.B0_RL_NKPortOfLading = "GBTIL";
			var messageTransmit = Factory.New<AMSEDIMessage>();
			messageTransmit.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			messageTransmit.EM_LinkedObject = header.MovementHeader;
			messageTransmit.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PermitToTransfer;
			messageTransmit.EM_MessageSubType = AMSMessageSubTypeList.Codes.PermitToTransfer;
			messageTransmit.EM_MessageOwner = Constants.ACE;
			messageTransmit.EM_MessageText = "ACR          TI                                                                 M01OTT111CNADELAIDE EXPRESS       111       000001 9143245                      M0208052015_EDIEDIDAT1                                                          P012704092715                                                                   J01OTT1                                                                         T0108052015                  W11034-132086200                                   ZCR                               00000";
			messageTransmit.EM_Status = EDIMessage.Status.Sent;
			messageTransmit.EM_MessageNum = "EDIEDIDAT1";
			var messageReceive = Factory.New<AMSEDIMessage>();
			messageReceive.EM_ApplicationCode = CBPEDIInterchange.ApplicationCodes.AMS;
			messageReceive.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			messageReceive.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse;
			messageReceive.EM_MessageText = "ACR          TR15092721233551750                                                M01OTT111CNADELAIDE EXPRESS       111       000001 9143245                      M0208052015_EDIEDIDAT1                                                          P012704092715                                                                   J01OTT1                                                                         T0108052015                  W11034-132086200                                   W02OTT11509272123340100100001000000000000000000010000000005                     ZCR          TR                   00008";
			messageReceive.EM_MessageNum = messageTransmit.EM_MessageNum;
			var interchange = Factory.New<CBPEDIInterchange>();
			interchange.EI_From = "USC";
			interchange.EI_To = "OTT1";
			interchange.EI_InterchangeNum = "12345";
			messageReceive.EM_EI = interchange.PK;
			Factory.Save();
			new AMSIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			var headerLoaded = new BusinessObjectFactory().Load<CusInBondHeader>(header.PK);
			AssertEquals(1, headerLoaded.MovementHeader.MovementDetails.Count);
			var movementDetail = headerLoaded.MovementHeader.MovementDetails.FirstOrDefault();
			AssertEquals(AMSBillMessageStatusList.Codes.ClearPermitToTransfer, movementDetail.B9_CustomsStatus);
			AssertEquals(AMSBillMessageStatusList.Codes.ClearPermitToTransfer, movementDetail.B9_MessageStatus);
			AssertEquals(ZString.Empty, movementDetail.B9_CustomsStatusDescription);
			AssertEquals(AMSBillMessageStatusList.Descriptions.ClearPermitToTransfer, movementDetail.B9_MessageStatusDescription);
		}

		[TestDate(2016, 8, 8)]
		public void TestUpdateActualArrivalDate()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			((IVesselArrivalMessageAttachee)moveHeader).UpdateActualArrivalDate("1101", ZDateTime.Today, AMSMessageSubTypeList.Codes.VesselArrival);
			AssertEquals(1, header.PortArrivalDetails.Count);
			var portOfArrival = header.PortArrivalDetails[0];
			AssertEquals("1101", portOfArrival.PortCode);
			AssertEquals(ZDateTime.Today, portOfArrival.ActualArrivalDate);
			((IVesselArrivalMessageAttachee)moveHeader).UpdateActualArrivalDate("1102", ZDateTime.Today.AddDays(1), AMSMessageSubTypeList.Codes.VesselArrival);
			AssertEquals(2, header.PortArrivalDetails.Count);
			portOfArrival = header.PortArrivalDetails[1];
			AssertEquals("1102", portOfArrival.PortCode);
			AssertEquals(ZDateTime.Today.AddDays(1), portOfArrival.ActualArrivalDate);
			((IVesselArrivalMessageAttachee)moveHeader).UpdateActualArrivalDate("1101", ZDateTime.Today.AddDays(2), AMSMessageSubTypeList.Codes.VesselArrival);
			AssertEquals(2, header.PortArrivalDetails.Count);
			portOfArrival = header.PortArrivalDetails[0];
			AssertEquals("1101", portOfArrival.PortCode);
			AssertEquals(ZDateTime.Today.AddDays(2), portOfArrival.ActualArrivalDate);
			portOfArrival = header.PortArrivalDetails[1];
			AssertEquals("1102", portOfArrival.PortCode);
			AssertEquals(ZDateTime.Today.AddDays(1), portOfArrival.ActualArrivalDate);
		}

		[TestDate(2020, 07, 29)]
		public void TestPopulateAMSFirstAcceptedTimeFromMessage()
		{
			Header.BH_GB = GlbBranch.CurrentBranch.PK;
			Header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			Header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			var masterBill = Header.OceanBill;
			masterBill.B0_IssuerCode = "EGLV";
			masterBill.B0_MasterBillNumber = "146901158016";
			var houseBill = Header.Bills.AddNew();
			houseBill.B0_IssuerCode = "DISO";
			houseBill.B0_MasterBillNumber = "DMM31903048";
			var moveDetail = moveHeader.MovementDetails.AddNew(houseBill.PK);
			Factory.Save();
			IManifestMessageAttachee manifest = moveHeader;
			var originalMessage0 = moveHeader.Messages.AddNew();
			originalMessage0.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage0.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			originalMessage0.EM_MessageNum = "EDIEDIDAT1";
			originalMessage0.EM_MessageText = "ACR          MI                                                                 M01DISO11PAEVER SALUTE            0247E            9300477                      M02DMM31903048_EDIEDIDAT1                                                       P013002012420                                                                   J01DISO                                                                         B01DMM31903048 570690000000570CTN  0000003209KGP                                B020000000064CMXIAMEN GAOQI INTE            DISOEGLV57069     57069             B04OB EGLV146901158016                                                          B04ULCCAVAN                                                                     N00SH SNAGATR (FUJIAN) ORAL HEALTH TECHNO                                       N02CLOCK AND WATCH INDUSTRIAL PARK,ZONGER ROAD,                                 N02LONGWEN DEVELOPMENT ZONE OF ZHANGZHOU                                        N03ZHANGZHOU                     CN                                             N00CN SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             N00N1 SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             C01EGHU9474528   EMCDUP0739                    HV0                    45G0L     D00           000000000000003209KG                                              D010000000570LICENSED ADIRONDACK CHAIR PPK                              CTN     D02N/A                                                                          ZCR                               00000";
			originalMessage0.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage0.EM_MessageOwner = Constants.ACE;
			originalMessage0.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			originalMessage0.EM_Status = EDIMessage.Status.Sent;
			var responseMessage0 = moveHeader.Messages.AddNew();
			responseMessage0.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse;
			responseMessage0.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			responseMessage0.EM_MessageNum = "EDIEDIDAT1";
			responseMessage0.EM_MessageText = "ACR          MR20010121002090316                                                M01DISO11PAEVER SALUTE            0247E     000001 9300477                      M02DMM31903048_EDIEDIDAT1                                                       P013002012420                                                                   W02DISO2001012100190100100001000000000000000000000000100024000000000000000      ZCR          MR                   00004";
			responseMessage0.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage0.EM_MessageOwner = Constants.ACE;
			responseMessage0.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-9);
			responseMessage0.EM_Status = EDIMessage.Status.Received;
			manifest.UpdateIncomingBillStatus("DISO", "DMM31903048", AMSMessageSubTypeList.Codes.Creating, false, responseMessage0);
			Factory.Save();
			AssertEquals(new ZDateTime(2020, 07, 20), moveDetail.B9_FirstAcceptedTime);
			var originalMessage1 = moveHeader.Messages.AddNew();
			originalMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			originalMessage1.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			originalMessage1.EM_MessageNum = "EDIEDIDAT2";
			originalMessage1.EM_MessageText = "ACR          AI                                                                 M01DISO11PAEVER SALUTE            0247E            9300477                      M02DMM31903048_EDIEDIDAT2                                                       P013002012420                                                                   J01DISO                                                                         A01DISO3002DDMM31903048           03                                            ZCR                               00000";
			originalMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage1.EM_MessageOwner = Constants.ACE;
			originalMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-8);
			originalMessage1.EM_Status = EDIMessage.Status.Sent;
			var responseMessage1 = moveHeader.Messages.AddNew();
			responseMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			responseMessage1.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			responseMessage1.EM_MessageNum = "EDIEDIDAT2";
			responseMessage1.EM_MessageText = "ACR          AR20010604355456683                                                M01DISO11PAEVER SALUTE            0247E     000001 9300477                      M02DMM31903048_EDIEDIDAT2                                                       P013002012420                                                                   W02DISO2001060435530100100000000000000100000000000000000005000000000000000      ZCR          AR                   00004";
			responseMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage1.EM_MessageOwner = Constants.ACE;
			responseMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-7);
			responseMessage1.EM_Status = EDIMessage.Status.Received;
			manifest.UpdateIncomingBillStatus("DISO", "DMM31903048", AMSMessageSubTypeList.Codes.AmendingDelete, false, responseMessage1);
			Factory.Save();
			AssertEquals(ZDateTime.Empty, moveDetail.B9_FirstAcceptedTime);
			var originalMessage2 = moveHeader.Messages.AddNew();
			originalMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			originalMessage2.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingAdd;
			originalMessage2.EM_MessageNum = "EDIEDIDAT3";
			originalMessage2.EM_MessageText = "ACR          AI                                                                 M01DISO11PAEVER SAFETY            0248E     000001 9300465                      M02DMM31903048_EDIEDIDAT3                                                       P013002013120                                                                   J01DISO                                                                         A01DISO3002ADMM31903048           03                                            B01DMM31903048 570690000000570CTN  0000003209KGP                                B020000000064CMXIAMEN GAOQI INTE            DISOEGLV57069     57069             B04OB EGLV146901158016                                                          B04ULCCAVAN                                                                     N00SH SNAGATR (FUJIAN) ORAL HEALTH TECHNO                                       N02CLOCK AND WATCH INDUSTRIAL PARK,ZONGER ROAD,                                 N02LONGWEN DEVELOPMENT ZONE OF ZHANGZHOU                                        N03ZHANGZHOU                     CN                                             N00CN SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             N00N1 SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             C01EGHU9474528   EMCDUP0739                    HV0                    45G0L     D00           000000000000003209KG                                              D010000000570LICENSED ADIRONDACK CHAIR PPK                              CTN     D02N/A                                                                          ZCR                               00000";
			originalMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage2.EM_MessageOwner = Constants.ACE;
			originalMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-6);
			originalMessage2.EM_Status = EDIMessage.Status.Sent;
			var responseMessage2 = moveHeader.Messages.AddNew();
			responseMessage2.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			responseMessage2.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingAdd;
			responseMessage2.EM_MessageNum = "EDIEDIDAT3";
			responseMessage2.EM_MessageText = "ACR          AR20010820234113943                                                M01DISO11PAEVER SAFETY            0248E     000001 9300465                      M02DMM31903048_EDIEDIDAT3                                                       P013002013120                                                                   W02DISO2001082023400100100001000000000100000000000000100025000000000000000      ZCR          AR                   00004";
			responseMessage2.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage2.EM_MessageOwner = Constants.ACE;
			responseMessage2.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-5);
			responseMessage2.EM_Status = EDIMessage.Status.Received;
			manifest.UpdateIncomingBillStatus("DISO", "DMM31903048", AMSMessageSubTypeList.Codes.AmendingAdd, false, responseMessage2);
			Factory.Save();
			AssertEquals(new ZDateTime(2020, 07, 20), moveDetail.B9_FirstAcceptedTime);
			var originalMessage3 = moveHeader.Messages.AddNew();
			originalMessage3.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			originalMessage3.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			originalMessage3.EM_MessageNum = "EDIEDIDAT4";
			originalMessage3.EM_MessageText = "ACR          AI                                                                 M01DISO11PAEVER SALUTE            0247E            9300477                      M02DMM31903048_EDIEDIDAT4                                                       P013002012420                                                                   J01DISO                                                                         A01DISO3002DDMM31903048           03                                            ZCR                               00000";
			originalMessage3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage3.EM_MessageOwner = Constants.ACE;
			originalMessage3.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-4);
			originalMessage3.EM_Status = EDIMessage.Status.Sent;
			var responseMessage3 = moveHeader.Messages.AddNew();
			responseMessage3.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			responseMessage3.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingDelete;
			responseMessage3.EM_MessageNum = "EDIEDIDAT4";
			responseMessage3.EM_MessageText = "ACR          AR20010604355456683                                                M01DISO11PAEVER SALUTE            0247E     000001 9300477                      M02DMM31903048_EDIEDIDAT4                                                       P013002012420                                                                   W02DISO4001060435530100100000000000000100000000000000000005000000000000000      ZCR          AR                   00004";
			responseMessage3.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage3.EM_MessageOwner = Constants.ACE;
			responseMessage3.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-3);
			responseMessage3.EM_Status = EDIMessage.Status.Received;
			manifest.UpdateIncomingBillStatus("DISO", "DMM31903048", AMSMessageSubTypeList.Codes.AmendingDelete, false, responseMessage2);
			Factory.Save();
			AssertEquals(ZDateTime.Empty, moveDetail.B9_FirstAcceptedTime);
			houseBill.B0_MasterBillNumber = "DMM31903049";
			var originalMessage4 = moveHeader.Messages.AddNew();
			originalMessage4.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendment;
			originalMessage4.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingAdd;
			originalMessage4.EM_MessageNum = "EDIEDIDAT5";
			originalMessage4.EM_MessageText = "ACR          AI                                                                 M01DISO11PAEVER SAFETY            0248E     000001 9300465                      M02DMM31903049_EDIEDIDAT5                                                       P013002013120                                                                   J01DISO                                                                         A01DISO5002ADMM31903049           03                                            B01DMM31903049 570690000000570CTN  0000003209KGP                                B020000000064CMXIAMEN GAOQI INTE            DISOEGLV57069     57069             B04OB EGLV146901158016                                                          B04ULCCAVAN                                                                     N00SH SNAGATR (FUJIAN) ORAL HEALTH TECHNO                                       N02CLOCK AND WATCH INDUSTRIAL PARK,ZONGER ROAD,                                 N02LONGWEN DEVELOPMENT ZONE OF ZHANGZHOU                                        N03ZHANGZHOU                     CN                                             N00CN SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             N00N1 SHOPPERS DRUG MART INC.                                                   N02243 CONSUMERS RD                                                             N03TORONTO                       CA                                             N04                       TE+14164931220             FX+14164902309             C01EGHU9474528   EMCDUP0739                    HV0                    45G0L     D00           000000000000003209KG                                              D010000000570LICENSED ADIRONDACK CHAIR PPK                              CTN     D02N/A                                                                          ZCR                               00000";
			originalMessage4.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage4.EM_MessageOwner = Constants.ACE;
			originalMessage4.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-2);
			originalMessage4.EM_Status = EDIMessage.Status.Sent;
			var responseMessage4 = moveHeader.Messages.AddNew();
			responseMessage4.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse;
			responseMessage4.EM_MessageSubType = AMSMessageSubTypeList.Codes.AmendingAdd;
			responseMessage4.EM_MessageNum = "EDIEDIDAT5";
			responseMessage4.EM_MessageText = "ACR          AR20010820234113943                                                M01DISO11PAEVER SAFETY            0248E     000001 9300465                      M02DMM31903049_EDIEDIDAT5                                                       P013002013120                                                                   W02DISO2001082023400100100001000000000100000000000000100025000000000000000      ZCR          AR                   00004";
			responseMessage4.EM_ReceiveTransmit = AMSEDIMessage.Direction.Receive;
			responseMessage4.EM_MessageOwner = Constants.ACE;
			responseMessage4.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			responseMessage4.EM_Status = EDIMessage.Status.Received;
			Factory.Save();
			manifest.UpdateIncomingBillStatus("DISO", "DMM31903049", AMSMessageSubTypeList.Codes.AmendingAdd, false, responseMessage4);
			Factory.Save();
			AssertEquals(new ZDateTime(2020, 07, 28), moveDetail.B9_FirstAcceptedTime);
		}

		public void TestPendingMessagesCollection()
		{
			Header.BH_GB = GlbBranch.CurrentBranch.PK;
			Header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			Header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			var originalMessage0 = moveHeader.Messages.AddNew();
			originalMessage0.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage0.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			originalMessage0.EM_MessageNum = "EDIEDIDAT1";
			originalMessage0.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage0.EM_MessageOwner = Constants.ACE;
			originalMessage0.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			originalMessage0.EM_Status = EDIMessage.Status.Queued;
			var pendingMessageAttache = (IManifestPendingMessagesAttachee)moveHeader;
			AssertEquals("There is no pending messages", 0, pendingMessageAttache.PendingMessages.Count);
			var originalMessage1 = moveHeader.Messages.AddNew();
			originalMessage1.EM_MessageType = AMSApplicationIdentifierCodeList.Codes.ManifestCreate;
			originalMessage1.EM_MessageSubType = AMSMessageSubTypeList.Codes.Creating;
			originalMessage1.EM_MessageNum = "EDIEDIDAT2";
			originalMessage1.EM_ReceiveTransmit = AMSEDIMessage.Direction.Transmit;
			originalMessage1.EM_MessageOwner = Constants.ACE;
			originalMessage1.EM_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-10);
			originalMessage1.EM_Status = EDIMessage.Status.Pending;
			AssertEquals("There is 1 pending message", 1, pendingMessageAttache.PendingMessages.Count);
			var pendingMessage = pendingMessageAttache.PendingMessages[0];
			AssertEquals("pendingMessage.EM_MessageNum", "EDIEDIDAT2", pendingMessage.EM_MessageNum);
		}

		public override void TestHumanReadableName()
		{
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
			AssertEquals("AMS Movement Header", moveHeader.HumanReadableName);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			AssertEquals("Permit Movement Header ", moveHeader.HumanReadableName);
			moveHeader.BM_InBondCarrierID = "PTT1";
			AssertEquals("Permit Movement Header PTT1", moveHeader.HumanReadableName);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			AssertEquals("In-Bond Movement Header ", moveHeader.HumanReadableName);
			moveHeader.InBondNumber = "333210146";
			AssertEquals("In-Bond Movement Header 333210146", moveHeader.HumanReadableName);
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.SubsequentInBond;
			AssertEquals("In-Bond Movement Header 333210146", moveHeader.HumanReadableName);
		}

		ForwardingConsol consol;
		CusInBondMoveHeader aMSMoveHeader;
		protected override void SetUp()
		{
			base.SetUp();
			Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, CBPEDIInterchange.InterchangePartyIDs.AMSMailbox).SetNext(Factory, 1);
			consol = Factory.New<ForwardingConsol>();
			Header.BH_ParentID = consol.PK;
			Header.BH_ParentTableCode = consol.TablePrefix;
			aMSMoveHeader = Header.MovementHeader;
		}

		CusInBondHeader Header => (CusInBondHeader)header;

		protected override CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header)
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

		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			switch (info.Name)
			{
				case CusInBondMoveHeader.Schema.InBondNumber:
					((CusInBondMoveHeader)info.BizObj).BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
					break;
				case CusInBondMoveHeader.Schema.BM_ManifestSequenceNumber:
					((CusInBondMoveHeader)info.BizObj).BM_SubApplicationCode = SubApplicationCodeList.Codes.AMS;
					break;
				default:
					base.SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(info);
					break;
			}
		}
	}
}
