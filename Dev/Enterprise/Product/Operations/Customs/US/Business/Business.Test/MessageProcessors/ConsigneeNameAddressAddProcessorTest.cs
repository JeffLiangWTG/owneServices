using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ConsigneeNameAddressAddProcessorTest : ABIProcessorTest<ConsigneeNameAddressAddProcessor, APLA, APLB, APLY>
	{
		[TestDate(2008, 11, 10)]
		public void TestAutoSendCargoRelease_FailedResponse()
		{
			using (RegistrySetupper regSetupper = new RegistrySetupper(AutoSendOption.On))
			{
				PrepareForAutoSendCargoReleaseTest(false, false, false);
				new ABIIncomingMessageProcessor().ExecuteBatch();

				CusEntryHeader loadedEntry = new BusinessObjectFactory().Load<CusEntryHeader>(entryForAutoSendTest.PK);

				Assert("Should have only IJ processed email", (Env.OutgoingCustomsMailManager.EmailsCreated.Count == 1));
				AssertEquals(2, loadedEntry.Messages.Count);
			}
		}

		public void TestIADispositionAction()
		{
			SetTestData();

			string messageText = @"B010712DS9IL                                               DE1JFKJFK_246348     
I300001WALSH, SUSAN                    428 W 23RD ST                            
IBDS9611510910032GCDATA ADDED AS REQUESTED                                      
I300002ALTMAN, JILL                    818 WILMOT RD                            
IBDS9611510910042GCDATA ADDED AS REQUESTED                                      
I300003LANGMANN, SARA ELLEN            46 MAIN ST                               
IBDS9611510910052GCDATA ADDED AS REQUESTED                                      
I300004KEATON, MICHEAL                 890 LAKE SHORE RD                        
IBDS9611510910062GCDATA ADDED AS REQUESTED                                      
I300005DONATO, NANCI                   1140 AVE DES AMERICAS 14TH FL            
IBDS9611510910072GCDATA ADDED AS REQUESTED                                      
I300006PACHMEYER, ALLEN                4204 COLUMBINE WAY NUMBER 4              
IBDS9611510910082GCDATA ADDED AS REQUESTED                                      
I300007DONATO, NANCY                   1140 AVENUE OF THE AMERICAS 14TH         
IBDS9611510910092GCDATA ADDED AS REQUESTED                                      
I300008SCHNEIDER, MARY J               3 RIVER RD                               
IBDS9611510910102GCDATA ADDED AS REQUESTED                                      
I300009HILLMAN, LAWRENCE               1945 SUNNYSIDE AVE                       
IA00009-15S307UCCTLHILLMAN, LAWRENCE               15121610202                  
IBDS9611510910117BGNAME & ADDRESS EXIST ON 5106 FILE                            
IB                 A (CF-5106) IS ON FILE WITH NO BOND                          
IB              524TRANSACTION DATA REJECTED                                    
I300010ROGERS, STEPHANIE               226 E 25TH ST APT 4B                     
IBDS9611510910122GCDATA ADDED AS REQUESTED                                      
I300011D'ORAZIO, ROBERT                1705 HUNTERS PATH LANE                   
IBDS9611510910132GCDATA ADDED AS REQUESTED                                      
Y  0712DS9IL00025                                                               ";

			message2.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);
		}

		[TestDate(2008, 11, 10)]
		public void TestAutoSendCargoRelease_SuccResponse_RegistryOptionOff()
		{
			using (RegistrySetupper regSetupper = new RegistrySetupper(AutoSendOption.Off))
			{
				PrepareForAutoSendCargoReleaseTest(true, false, false);
				new ABIIncomingMessageProcessor().ExecuteBatch();

				CusEntryHeader loadedEntry = new BusinessObjectFactory().Load<CusEntryHeader>(entryForAutoSendTest.PK);

				Assert("Should have only IJ processed email", (Env.OutgoingCustomsMailManager.EmailsCreated.Count == 1));
				AssertEquals(2, loadedEntry.Messages.Count);
			}
		}

		[TestDate(2008, 11, 10)]
		public void TestAutoSendCargoRelease_SuccResponse_HasErrors()
		{
			using (RegistrySetupper regSetupper = new RegistrySetupper(AutoSendOption.On))
			{
				PrepareForAutoSendCargoReleaseTest(true, true, false);
				Factory.Save();
				new ABIIncomingMessageProcessor().ExecuteBatch();

				CusEntryHeader loadedEntry = new BusinessObjectFactory().Load<CusEntryHeader>(entryForAutoSendTest.PK);

				AssertEquals("Should have IJ processed email", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				Assert("Cargo release has not been sent", email.Body.Contains(ConsigneeNameAddressAddProcessor.EmailFailedDescription));
			}
		}

		[TestDate(2008, 11, 10)]
		public void TestAutoSendCargoRelease_SuccResponse_HasMessageErrors()
		{
			using (RegistrySetupper regSetupper = new RegistrySetupper(AutoSendOption.On))
			{
				PrepareForAutoSendCargoReleaseTest(true, false, true);
				Assert("Precondition: Declaration has message errors", decForAutoSendTest.HasMessageErrors);
				new ABIIncomingMessageProcessor().ExecuteBatch();

				CusEntryHeader loadedEntry = new BusinessObjectFactory().Load<CusEntryHeader>(entryForAutoSendTest.PK);

				AssertEquals("Should have IJ processed email", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				Assert("Cargo release has not been sent", email.Body.Contains(ConsigneeNameAddressAddProcessor.EmailFailedDescription));
			}
		}

		[TestDate(2008, 11, 10)]
		public void TestAutoSendCargoRelease_SuccResponse_AllOK()
		{
			using (RegistrySetupper regSetupper = new RegistrySetupper(AutoSendOption.On))
			{
				PrepareForAutoSendCargoReleaseTest(true, false, false);
				new ABIIncomingMessageProcessor().ExecuteBatch();

				CusEntryHeader loadedEntry = new BusinessObjectFactory().Load<CusEntryHeader>(entryForAutoSendTest.PK);

				AssertEquals("Cargo Release has been sent. Awaiting CargoReleaseOriginal", ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal, loadedEntry.CH_Status);
				AssertEquals("Should have 1 IJ processed email and 1 CRL message sent", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				Assert("Cargo release has been sent successfuly", email.Body.Contains(ConsigneeNameAddressAddProcessor.EmailSuccessfulDescription));
			}
		}

		protected override void EndToEndCore()
		{
			SetTestData();

			message2.EM_MessageText = "B018888XJ5IL                                               ~15000               " +
"I3     ABOUT SCALES                    9 PEASE STREET                           " +
"IBXJ500000048   1HESEQUENCE NO: NOT NUMERIC                                     " +
"I4                                CITY                 NT4870                   " +
"IBXJ500000048   008INVALID ZIP CODE                                             " +
"IB              524TRANSACTION DATA REJECTED                                    " +
"Y  8888XJ5IL00005";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		public void TestFailedMessage()
		{
			SetTestData();

			message2.EM_MessageText = "B018888XJ5IL                                               ~15000               " +
"I3     ABOUT SCALES                    9 PEASE STREET                           " +
"IBXJ500000048   1HESEQUENCE NO: NOT NUMERIC                                     " +
"I4                                CITY                 NT4870                   " +
"IBXJ500000048   A08INVALID ZIP CODE                                             " + //more than 999 entry lines.
"IBXJ500000048   7BBDUPLICATE ENTRY & LINE-ITEM SEQ                              " +
"IBXJ500000048   52ZENTRY/LINE SEQUENCE REJECTED                                 " +
"Y  8888XJ5IL00005";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);

			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(entryHeader.ImportUltimateConsignee);
			AssertEquals("KN message shouldn't be sent for Consignee", 0, wrapper.Messages.Count);
		}

		public void TestProcessMessageSuccessfully()
		{
			SetTestData();

			dec.ConsigneeOrgAddress.OH_FullName = "COINWATCH AUSTRALIA PTY LTD";
			dec.ConsigneeOrgAddress.MainAddress.OA_Address1 = "SUITE 9, 6-8 GRICE AVENUE";

			message2.EM_MessageText =
					"B018888XJ5IL                                               ~15000               " +
					"I300001COINWATCH AUSTRALIA PTY LTD     SUITE 9, 6-8 GRICE AVENUE                " +
					"IBXJ5000000550A12GCDATA ADDED AS REQUESTED                                      " + //more than 999 entry lines.
					"Y  8888XJ5IL00002";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			entryHeader.RelatedENSEntry.EntryNumber = "00000055";
			AssertEquals("one message before processing", 2, entryHeader.Messages.Count);

			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertEquals("message is attached to entry", 2, entryHeader.Messages.Count);
		}

		public void TestProcessMultipleLines()
		{
			SetTestData();

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "FAIRFIELD MANUFACTURING COMPANY";
			ultimateConsignee.MainAddress.OA_Address1 = "US 52 SOUTH";

			dec.InvoiceLines[0].JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "SKYLINE SOUTH INC";
			ultimateConsignee.MainAddress.OA_Address1 = "1125 NORTHMEADOW PARKWAY";
			JobComInvoiceLine invoiceLine2 = dec.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			message2.EM_MessageText =
					"B018888XJ5IL                                               ~15000               I300001FAIRFIELD MANUFACTURING COMPANY US 52 SOUTH                              IBXJ5600139230012GCDATA ADDED AS REQUESTED                                      I300002SKYLINE SOUTH INC               1125 NORTHMEADOW PARKWAY                 I4SUITE 100 ROSWELL,  30076 USA   GEORGIA              GA30076                  IBXJ560013923002ACZADDRESS HAS LEADING SPACE                                    IB              524TRANSACTION DATA REJECTED                                    Y  8888XJ5IL00006";

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			CusEntryHeader loadedEntryHeader = loadingFactory.Load<CusEntryHeader>(entryHeader.PK);

			AssertEquals("CusEntryLine.US_IJAccepted for entry line1", true, loadedEntryHeader.MergedLines.FindByLineNumber(1).US_IJAccepted);
			AssertEquals("CusEntryLine.US_IJAccepted for entry line2", false, loadedEntryHeader.MergedLines.FindByLineNumber(2).US_IJAccepted);
			AssertEquals("IsConsigneeNameAddressUsed set to true as there are accepted lines", true, ((ICargoReleaseCusEntryHeader)loadedEntryHeader).IsConsigneeNameAddressUsed);
		}

		public void TestSetUpFlagUseConsigneeNameAddress()
		{
			SetTestData();

			entryHeader.RelatedENSEntry.EntryNumber = "00000071";
			message.EM_SendWithMessageErrors = false;
			message2.EM_MessageText =
				"B018888XJ5IL                                               ~15000               " +
				"I300001A.A.L. SHIPPING AGENCIES P/L    MAIN ADDRESS 1                           " +
				"IBXJ500000071   2GCDATA ADDED AS REQUESTED                                      " +
				"Y  8888XJ5IL00002";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			BusinessObjectFactory loadingFactory = new BusinessObjectFactory();
			CusEntryHeader loadedEntryHeader = loadingFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("IsConsigneeNameAddressUsed set to true", true, ((ICargoReleaseCusEntryHeader)loadedEntryHeader).IsConsigneeNameAddressUsed);

			MQEDIMessage message3 = Factory.New<MQEDIMessage>();
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_MessageNum = "~15000";
			message3.EM_Status = EDIMessage.Status.Queued;
			message3.EM_LinkedObject = entryHeader;
			message3.EM_MessageText = "B018888XJ5IL                                               ~15000               " +
				"I300001A.A.L. SHIPPING AGENCIES P/L    MAIN ADDRESS 1                           " +
				"IBXJ500000071   42FDATA ADDED WITH WARNINGS                                     " +
				"Y  8888XJ5IL00002";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			loadedEntryHeader = loadingFactory.Load<CusEntryHeader>(entryHeader.PK);
			AssertEquals("IsConsigneeNameAddressUsed set to true", true, ((ICargoReleaseCusEntryHeader)loadedEntryHeader).IsConsigneeNameAddressUsed);
		}

		#region implementation

		JobDeclaration dec;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		MQEDIMessage message;
		MQEDIMessage message2;

		protected void SetTestData()
		{
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			dec.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = dec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entryLine = invoiceLine.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.CargoRelease, false);
			entryHeader = entryLine.Header;

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_FullName = "Consignee";
			dec.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = "IJ";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = "B018888XJ5IJ                                               <<MSGNO PLACEHOLDER>>" +
				"I3     ABOUT SCALES                    9 PEASE STREET                           " +
				"I4                                CITY                 NT4870                   " +
				"I78888XJ5 00000048                                                              " +
				"Y  8888XJ5IJ00003";

			message.EM_MessageSubType = ConsigneeNameAddressAddMessageSubTypeList.Codes.Add;
			message.EM_LinkedObject = entryHeader;

			message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageNum = "~15000";
			message2.EM_Status = EDIMessage.Status.Queued;
			message2.EM_MessageText = "";
			message2.EM_LinkedObject = entryHeader;

			Factory.Save();
			message.EM_MessageNum = "~15000";
			Factory.Save();
		}

		JobDeclaration decForAutoSendTest;
		CusEntryHeader entryForAutoSendTest;
		JobComInvoiceHeader invoiceForAutoSendTest;
		JobComInvoiceLine invoiceLineForAutoSendTest;
		MQEDIMessage requestMessageForAutoSendTest;
		MQEDIMessage responseMessageForAutoSendTest;

		void PrepareForAutoSendCargoReleaseTest(bool consigneeNameAddressAddProcessedSuccessfully, bool forceHasErrors, bool forceHasMessageErrors)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A001", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper testHelper = new DeclarationTestHelper(Factory);

			decForAutoSendTest = testHelper.CreateSimpleImportDeclaration(Factory);
			decForAutoSendTest.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			decForAutoSendTest.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			decForAutoSendTest.Invoices.DeleteAll();
			decForAutoSendTest.US_EnableENS = false;
			decForAutoSendTest.US_EnableINB = false;
			decForAutoSendTest.US_EnableCRL = true;
			decForAutoSendTest.IOROrgPK = decForAutoSendTest.Importer.PK;
			decForAutoSendTest.US_EntryDate = ZDateTime.Today.AddDays(1);
			decForAutoSendTest.US_SchDEntry = "8888";
			decForAutoSendTest.US_SchDArrival = "8888";
			decForAutoSendTest.US_US_NKLocationOfGoods = "A001";
			decForAutoSendTest.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			decForAutoSendTest.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			decForAutoSendTest.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			decForAutoSendTest.US_7501Purchased = YesNoDefaultList.Codes.No;
			decForAutoSendTest.US_BondType = BondTypeList.Codes.ContinuousBond;
			decForAutoSendTest.US_SuretyCode = "891";
			decForAutoSendTest.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;

			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "APL EMERALD  TEST";
			vessel.RV_LloydsNumber = "9077123";
			decForAutoSendTest.JE_VesselName = vessel.RV_Code;

			invoiceForAutoSendTest = decForAutoSendTest.Invoices.AddNew();
			invoiceForAutoSendTest.JZ_InvoiceNumber = "1";
			invoiceForAutoSendTest.JZ_InvoiceAmount = 100;
			invoiceForAutoSendTest.US_UC_NKCountryOfExport = "IT";
			invoiceForAutoSendTest.US_TransactionsRelated = "N";

			invoiceLineForAutoSendTest = invoiceForAutoSendTest.JobComInvoiceLines.AddNew();
			invoiceLineForAutoSendTest.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Ukraine;
			invoiceLineForAutoSendTest.JI_Tariff = "0101190010";
			invoiceLineForAutoSendTest.JI_LinePrice = 100m;
			invoiceLineForAutoSendTest.JI_Weight = 100m;
			invoiceLineForAutoSendTest.JI_WeightUQ = "KG";
			invoiceLineForAutoSendTest.US_DestinationState = USStatesList.Codes.California;
			OrgHeader manufacturer = testHelper.CreateConsignor();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUBEREQU6LON");
			invoiceLineForAutoSendTest.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLineForAutoSendTest.JI_LinePrice = 100m;

			decForAutoSendTest.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryForAutoSendTest = decForAutoSendTest.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0];
			AssertNotNull(entryForAutoSendTest);

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			requestMessageForAutoSendTest = mock.Object;
			requestMessageForAutoSendTest.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			requestMessageForAutoSendTest.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessageForAutoSendTest.EM_MessageText = "B018888XJ5IJ                                               <<MSGNO PLACEHOLDER>>" +
				"I3     ABOUT SCALES                    9 PEASE STREET                           " +
				"I4                                CITY                 NT4870                   " +
				"I78888XJ5 00000048                                                              " +
				"Y  8888XJ5IJ00003";

			requestMessageForAutoSendTest.EM_MessageSubType = ConsigneeNameAddressAddMessageSubTypeList.Codes.Add;
			requestMessageForAutoSendTest.EM_LinkedObject = entryForAutoSendTest;
			requestMessageForAutoSendTest.EM_MessageNum = "~15000";

			responseMessageForAutoSendTest = Factory.New<MQEDIMessage>();
			responseMessageForAutoSendTest.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessageForAutoSendTest.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessageForAutoSendTest.EM_MessageNum = "~15000";
			responseMessageForAutoSendTest.EM_Status = EDIMessage.Status.Queued;
			responseMessageForAutoSendTest.EM_MessageText = "";
			responseMessageForAutoSendTest.EM_LinkedObject = entryForAutoSendTest;

			if (consigneeNameAddressAddProcessedSuccessfully)
			{
				responseMessageForAutoSendTest.EM_MessageText =
					"B018888XJ5IL                                               ~15000               " +
					"I300001COINWATCH AUSTRALIA PTY LTD     SUITE 9, 6-8 GRICE AVENUE                " +
					"IBXJ5000000550012GCDATA ADDED AS REQUESTED                                      " +
					"Y  8888XJ5IL00002";
			}
			else
			{
				responseMessageForAutoSendTest.EM_MessageText =
					"B018888XJ5IL                                               ~15000               " +
					"I3     ABOUT SCALES                    9 PEASE STREET                           " +
					"IBXJ500000048   1HESEQUENCE NO: NOT NUMERIC                                     " +
					"I4                                CITY                 NT4870                   " +
					"IBXJ500000048   008INVALID ZIP CODE                                             " +
					"IB              524TRANSACTION DATA REJECTED                                    " +
					"Y  8888XJ5IL00005";
			}

			if (forceHasErrors)
			{
				decForAutoSendTest.US_FixPSD = ZBool.True;
				decForAutoSendTest.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			}
			if (forceHasMessageErrors)
			{
				decForAutoSendTest.US_EntryDate = ZDateTime.Today.AddYears(-5);
				decForAutoSendTest.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddYears(1);
			}

			Factory.Save();
			requestMessageForAutoSendTest.EM_MessageNum = "~15000";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			if (!forceHasMessageErrors)
			{
				invoiceForAutoSendTest.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
				Factory.Save();
			}

			decForAutoSendTest.RunPreSaveValidation();
			AssertEquals("Precondition: HasErrors only if HasErrors forced", forceHasErrors, decForAutoSendTest.HasErrors);

			if (forceHasMessageErrors)
			{
				AssertNotEquals("Messages", "", decForAutoSendTest.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
			}
			else
			{
				decForAutoSendTest.US_EnableENS = false;
				AssertEquals("Messages", "", decForAutoSendTest.NotificationsIncludingChildren.GetMessageErrors().ToUniqueMessageListString());
			}

			Assert("Precondition: entryHeader is CargoRelease", entryForAutoSendTest.IsCargoRelease);
			Assert("Precondition: Cargo Release has not been sent", entryForAutoSendTest.CH_Status.IsEmpty);
		}

		enum AutoSendOption
		{
			On,
			Off
		}

		class RegistrySetupper : IDisposable
		{
			public RegistrySetupper(AutoSendOption autoSendOption)
			{
				bool autoSendOn = false;
				if (autoSendOption == AutoSendOption.On)
				{
					autoSendOn = true;
				}

				USCustomsDataRegistry.Instance.AutoSendCargoReleaseMessageOnSuccessfulIJ.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, autoSendOn);
			}

			#region IDisposable Members

			public void Dispose()
			{
				USCustomsDataRegistry.Instance.AutoSendCargoReleaseMessageOnSuccessfulIJ.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, USCustomsDataRegistry.Instance.AutoSendCargoReleaseMessageOnSuccessfulIJ.DefaultValue);
			}

			#endregion
		}

		#endregion
	}
}
