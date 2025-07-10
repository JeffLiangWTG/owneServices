using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(InBondMenuItemMessageSendingObject))]
	sealed class InBondMenuItemMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultCarrierSCACFromOrg()
		{
			var carrierOrg = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "13-1502798000", Core.Constants.CountryCodes.UnitedStates);
			carrierOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			var messageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var messageSendingObject = new InBondMenuItemMessageSendingObject(messageData);
			messageSendingObject.InBondCarrierOrgPK = carrierOrg.PK;
			AssertEquals("Carrier Code should be ABCD", "ABCD", messageSendingObject.InBondCarrierCodeSCAC);
		}

		public void TestProperties()
		{
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject1 = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			AssertEquals("In_Bond Number", ZString.Empty, inBondMenuItemMessageSendingObject1.InBondNumber);
			AssertEquals("JobReference", ZString.Empty, inBondMenuItemMessageSendingObject1.JobReference);
			AssertEquals("Importer PK", ZGuid.Empty, inBondMenuItemMessageSendingObject1.ImporterOrgPK);
			AssertEquals("Importer Name", ZString.Empty, inBondMenuItemMessageSendingObject1.ImporterName);
			AssertEquals("Entry Type", ZString.Empty, inBondMenuItemMessageSendingObject1.EntryType);
			AssertEquals("In_Bond Carrier", ZGuid.Empty, inBondMenuItemMessageSendingObject1.InBondCarrierOrgPK);
			AssertEquals("In_Bond Carrier Name", ZString.Empty, inBondMenuItemMessageSendingObject1.InBondCarrierName);
			AssertEquals("In_Bond Carrier Code SCAC", ZString.Empty, inBondMenuItemMessageSendingObject1.InBondCarrierCodeSCAC);
			AssertEquals("US Destination Port Code", ZString.Empty, inBondMenuItemMessageSendingObject1.USDestinationPortCode);
			AssertEquals("US Destination Port Name", ZString.Empty, inBondMenuItemMessageSendingObject1.USDestinationPortName);
			AssertEquals("Foreign Port Code", ZString.Empty, inBondMenuItemMessageSendingObject1.ForeignDestinationPortCode);
			AssertEquals("Foreign Port Name", ZString.Empty, inBondMenuItemMessageSendingObject1.ForeignDestinationPortName);
			AssertEquals("QP Status", ZString.Empty, inBondMenuItemMessageSendingObject1.QPMessageStatus);
			AssertEquals("QP Status Description", ImportMessageStatusList.Descriptions.NotSent, inBondMenuItemMessageSendingObject1.QPMessageStatusDescription);
			AssertEquals("WP Status", ZString.Empty, inBondMenuItemMessageSendingObject1.WPMessageStatus);
			AssertEquals("WP Status Description", ImportMessageStatusList.Descriptions.NotSent, inBondMenuItemMessageSendingObject1.WPMessageStatusDescription);
			AssertEquals("Arrival Firms Code", ZString.Empty, inBondMenuItemMessageSendingObject1.ArrivalFirmsCode);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.InBondNumberInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.ImporterOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.EntryTypeInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.InBondCarrierOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.InBondCarrierCodeSCACInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.USDestinationPortCodeInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject1.ForeignDestinationPortCodeInfo.ReadOnly);
			inBondMenuItemMessageSendingObject1.InBondNumber = "1";
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.ImporterOrgPKInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.EntryTypeInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.InBondCarrierOrgPKInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.InBondCarrierCodeSCACInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.USDestinationPortCodeInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject1.ForeignDestinationPortCodeInfo.ReadOnly);
			inBondMenuItemMessageData.USDestinationPortCode = "0001";
			AssertEquals("US Destination Port Code", "0001", inBondMenuItemMessageSendingObject1.USDestinationPortCode);
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "Full Name 123";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_FullName = "Full Name 456";
			var carrierCombined = Factory.NewWithValidTestData<USCarrierCombined>();
			carrierCombined.UI_Code = "AAAA";

			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BBBB", "BBBB Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "CCCCC", "CCCCC Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();

			var inBondHeader = Factory.New<CusInBondHeader>();
			inBondHeader.BH_OA_Importer_ZAddress.OrgPK = org1.PK;
			var movementHeader = inBondHeader.MovementHeaders.AddNew();
			movementHeader.InBondNumber = "111111114";
			movementHeader.InBondCarrierOrgPK = org2.PK;
			movementHeader.BM_InBondCarrierSCAC = "AAAA";
			movementHeader.BM_DestinationPortCode = "BBBB";
			movementHeader.BM_ForeignDestPortKCode = "CCCCC";
			movementHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;
			movementHeader.BM_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			movementHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			movementHeader.BM_FIRMS = "1234";
			Factory.Save();
			var inBondMenuItemMessageSendingObject2 = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData, movementHeader);
			AssertEquals("In_Bond Number", "111111114", inBondMenuItemMessageSendingObject2.InBondNumber);
			AssertEquals("Importer PK", org1.PK, inBondMenuItemMessageSendingObject2.ImporterOrgPK);
			AssertEquals("Importer Name", "Full Name 123", inBondMenuItemMessageSendingObject2.ImporterName);
			AssertEquals("In_Bond Entry Type", InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject2.EntryType);
			AssertEquals("In_Bond Carrier", org2.PK, inBondMenuItemMessageSendingObject2.InBondCarrierOrgPK);
			AssertEquals("In_Bond Carrier Name", "Full Name 456", inBondMenuItemMessageSendingObject2.InBondCarrierName);
			AssertEquals("In_Bond Carrier Code SCAC", "AAAA", inBondMenuItemMessageSendingObject2.InBondCarrierCodeSCAC);
			AssertEquals("US Destination Port Code", "BBBB", inBondMenuItemMessageSendingObject2.USDestinationPortCode);
			AssertEquals("US Destination Port Name", "BBBB Name", inBondMenuItemMessageSendingObject2.USDestinationPortName);
			AssertEquals("Foreign Port Code", "CCCCC", inBondMenuItemMessageSendingObject2.ForeignDestinationPortCode);
			AssertEquals("Foreign Port Name", "CCCCC Name", inBondMenuItemMessageSendingObject2.ForeignDestinationPortName);
			AssertEquals("QP Status", "ADA", inBondMenuItemMessageSendingObject2.QPMessageStatus);
			AssertEquals("QP Status Description", ImportMessageStatusList.Descriptions.AwaitingDepartureAmendment, inBondMenuItemMessageSendingObject2.QPMessageStatusDescription);
			AssertEquals("WP Status", "AAV", inBondMenuItemMessageSendingObject2.WPMessageStatus);
			AssertEquals("WP Status Description", ImportMessageStatusList.Descriptions.AwaitingArrival, inBondMenuItemMessageSendingObject2.WPMessageStatusDescription);
			AssertEquals("Arrival Firms Code", "1234", inBondMenuItemMessageSendingObject2.ArrivalFirmsCode);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.InBondNumberInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.ImporterOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.InBondCarrierOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.InBondCarrierCodeSCACInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.USDestinationPortCodeInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject2.ForeignDestinationPortCodeInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject2.EntryTypeInfo.ReadOnly);
			var inBondMenuItemMessageSendingObject3 = new InBondMenuItemMessageSendingObject(inBondMenuItemMessageData);
			inBondMenuItemMessageSendingObject3.InBondNumber = "111111114";
			AssertEquals("In_Bond Number", "111111114", inBondMenuItemMessageSendingObject3.InBondNumber);
			AssertEquals("Importer PK", org1.PK, inBondMenuItemMessageSendingObject3.ImporterOrgPK);
			AssertEquals("Importer Name", "Full Name 123", inBondMenuItemMessageSendingObject3.ImporterName);
			AssertEquals("In_Bond Entry Type", InbondCommonTypeList.Codes._2TransportandExport, inBondMenuItemMessageSendingObject3.EntryType);
			AssertEquals("In_Bond Carrier", org2.PK, inBondMenuItemMessageSendingObject3.InBondCarrierOrgPK);
			AssertEquals("In_Bond Carrier Name", "Full Name 456", inBondMenuItemMessageSendingObject3.InBondCarrierName);
			AssertEquals("In_Bond Carrier Code SCAC", "AAAA", inBondMenuItemMessageSendingObject3.InBondCarrierCodeSCAC);
			AssertEquals("US Destination Port Code", "BBBB", inBondMenuItemMessageSendingObject3.USDestinationPortCode);
			AssertEquals("US Destination Port Name", "BBBB Name", inBondMenuItemMessageSendingObject3.USDestinationPortName);
			AssertEquals("Foreign Port Code", "CCCCC", inBondMenuItemMessageSendingObject3.ForeignDestinationPortCode);
			AssertEquals("Foreign Port Name", "CCCCC Name", inBondMenuItemMessageSendingObject3.ForeignDestinationPortName);
			AssertEquals("QP Status", "ADA", inBondMenuItemMessageSendingObject2.QPMessageStatus);
			AssertEquals("QP Status Description", ImportMessageStatusList.Descriptions.AwaitingDepartureAmendment, inBondMenuItemMessageSendingObject3.QPMessageStatusDescription);
			AssertEquals("WP Status", "AAV", inBondMenuItemMessageSendingObject3.WPMessageStatus);
			AssertEquals("WP Status Description", ImportMessageStatusList.Descriptions.AwaitingArrival, inBondMenuItemMessageSendingObject3.WPMessageStatusDescription);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.InBondNumberInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.ImporterOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.InBondCarrierOrgPKInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.InBondCarrierCodeSCACInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.USDestinationPortCodeInfo.ReadOnly);
			Assert("Should be ReadOnly", inBondMenuItemMessageSendingObject3.ForeignDestinationPortCodeInfo.ReadOnly);
			Assert("Should not be ReadOnly", !inBondMenuItemMessageSendingObject3.EntryTypeInfo.ReadOnly);
		}

		public void TestPedimentoNumber()
		{
			var inBondHeader1 = Factory.New<CusInBondHeader>();
			var bill1 = inBondHeader1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "1";
			var fenNumber1 = bill1.AdditionalReferences.AddNew();
			fenNumber1.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber1.BR_ReferenceNum = "C";
			var fenNumber2 = bill1.AdditionalReferences.AddNew();
			fenNumber2.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber2.BR_ReferenceNum = "B";
			var bill2 = inBondHeader1.Bills.AddNew();
			bill2.B0_MasterBillNumber = "2";
			var fenNumber3 = bill2.AdditionalReferences.AddNew();
			fenNumber3.BR_Qualifier = ReferenceQualifierList.Codes.FEN;
			fenNumber3.BR_ReferenceNum = "A";
			var movementHeader1 = inBondHeader1.MovementHeaders.AddNew();
			var movementDetail1 = movementHeader1.MovementDetails.AddNew();
			movementDetail1.B9_B0 = bill1.PK;
			var movementDetail2 = movementHeader1.MovementDetails.AddNew();
			movementDetail2.B9_B0 = bill2.PK;
			var inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival);
			var inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects[0];
			Assert("Should be readonly", inBondMenuItemMessageSendingObject.PedimentoNumberInfo.ReadOnly);
			AssertEquals("Should be empty", ZString.Empty, inBondMenuItemMessageSendingObject.PedimentoNumber);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Export);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects[0];
			Assert("Should be readonly", inBondMenuItemMessageSendingObject.PedimentoNumberInfo.ReadOnly);
			AssertEquals("Should be empty", ZString.Empty, inBondMenuItemMessageSendingObject.PedimentoNumber);
			inBondMenuItemMessageData = new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>()
			{ movementHeader1 }, InBondMenuItemMessageData.InBondMenuItemMessageTypes.Pedimento);
			inBondMenuItemMessageSendingObject = inBondMenuItemMessageData.InBondMenuItemMessageSendingObjects[0];
			Assert("Should be readonly", inBondMenuItemMessageSendingObject.PedimentoNumberInfo.ReadOnly);
			AssertEquals("Should be A. Order by B0_MasterBillNumber first then order by BR_ReferenceNum.", "B", inBondMenuItemMessageSendingObject.PedimentoNumber);
			inBondMenuItemMessageSendingObject.InBondNumber = "111111111";
			Assert("Should not be readonly", !inBondMenuItemMessageSendingObject.PedimentoNumberInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InBondMenuItemMessageSendingObject(new InBondMenuItemMessageData(Factory, new List<CusInBondMoveHeader>(), InBondMenuItemMessageData.InBondMenuItemMessageTypes.Arrival));
		}
	}
}
