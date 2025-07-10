using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Declaration.Testing;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	public class MessagingFunctionalityManagerTest : TestCaseWithFactory
	{
		public void TestDoNotWarnUserIfTariffIsUpToDate()
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
				var declaration = Factory.New<JobDeclaration>();
				SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
				SetMostRecentExchangeRateEndDateTo(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day);
				ShowTheSubmitToCustomsForm(declaration);
				AssertMultilineASCIIEquals("messagesShown.ToString()", @"Original Entry Message Generated and Ready to be sent by Service Tasks.", GetMessageShownToUser());
			}
		}

		public void TestExceptionThrownWhenConstructedWithNullDeclaration()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), delegate
			{
				new MessagingFunctionalityManager(null);
			}

			);
		}

		[TestDate(2021, 04, 18)]
		public void TestSendingWithDeferredCutoffOkay()
		{
			var jobDec = JobDeclaration.New(Factory);
			var decCreator = new TestFormalEntryCreator(jobDec);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			using (NZCustomsDataRegistry.Instance.BrokerDeferredCutoffDateEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "00009917B"))
			{
				var staff = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
				var usersPin = new CurrentUsersPin(Factory, true);
				var wrapper = staff.GetNZWrapper();
				_ = wrapper.NZBPassword.GP_CurrentPassword;
				wrapper.NZBPassword.GP_UserID = "CUCKOOSQUEAKER";
				usersPin.DecryptedPinCode = "OMGPIN";
				var manager = new MessagingFunctionalityManager(jobDec);
				using (var form = new ZForm())
				{
					form.Show();
					manager.ShowSubmitToCustomsForm(Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
				}

				var cutOffDateCalc = new BrokerDeferredCutOffDateCalculator(jobDec);
				var warningMessage = cutOffDateCalc.GetWarningMessageIfWarningRequired();
				Assert(!warningMessage.IsEmpty);
				AssertMultilineASCIIEquals("Message should match this property", "Question " + warningMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		public void TestSendingWithNoFreightWithApportionedOFTAndNoCharge()
		{
			SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
			JobDeclaration jobDec = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(jobDec);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			ErrorReporter.Clear();
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessagingFunctionalityManager manager = new MessagingFunctionalityManager(jobDec);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.CancelMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", "Error Cannot Send Message - Cannot Cancel when a Clearance Number has never been received From Customs.", UnitTestUserNotification.Instance.LastMessage.ToString());
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", @"Question Not all Entry Lines being sent to Customs have Freight on them. 

This can either be because the value of the line is so low that there is no freight apportioned against this line, or that no Freight has been entered against the Declaration.

Are you sure you want to submit this Declaration without Freight?", UnitTestUserNotification.Instance.LastMessage.ToString());
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			jobDec.ResumeApportionment();
			AssertEquals(0, jobDec.FilteredInvoiceLines[0].Charges.Count);
			AssertEquals(1000m, jobDec.FilteredInvoiceLines[0].CusEntryLine.FreightWholeNZD);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", "Information Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestSendingWithNoFreightForIPI()
		{
			SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
			JobDeclaration jobDec = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(jobDec);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			ErrorReporter.Clear();
			jobDec.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessagingFunctionalityManager manager = new MessagingFunctionalityManager(jobDec);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertEquals("Information Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestSendingWithNoFreight()
		{
			//TSW Active should not impinge on Legacy messaging
			SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
			JobDeclaration jobDec = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(jobDec);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			jobDec.ApportionmentDirty = false;
			ErrorReporter.Clear();
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessagingFunctionalityManager manager = new MessagingFunctionalityManager(jobDec);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.CancelMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", "Error Cannot Send Message - Cannot Cancel when a Clearance Number has never been received From Customs.", UnitTestUserNotification.Instance.LastMessage.ToString());
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", @"Question Not all Entry Lines being sent to Customs have Freight on them. 

This can either be because the value of the line is so low that there is no freight apportioned against this line, or that no Freight has been entered against the Declaration.

Are you sure you want to submit this Declaration without Freight?", UnitTestUserNotification.Instance.LastMessage.ToString());
			jobDec.FilteredInvoiceLines[0].Charges.AddNew(Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 0);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", @"Question Not all Entry Lines being sent to Customs have Freight on them. 

This can either be because the value of the line is so low that there is no freight apportioned against this line, or that no Freight has been entered against the Declaration.

Are you sure you want to submit this Declaration without Freight?", UnitTestUserNotification.Instance.LastMessage.ToString());
			jobDec.FilteredInvoiceLines[0].Charges[0].J7_Amount = 5;
			jobDec.ResumeApportionment();
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", "Information Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		public void TestSendingWithNoFreightAddsALog()
		{
			SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
			JobDeclaration jobDec = JobDeclaration.New(Factory);
			TestFormalEntryCreator decCreator = new TestFormalEntryCreator(jobDec);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(decCreator.Declaration.SaveHandlingSaveExceptions());
			decCreator.MergeDeclaration();
			decCreator.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
			decCreator.Declaration.JE_JS = Factory.New<ForwardingShipment>().PK;
			ErrorReporter.Clear();
			jobDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessagingFunctionalityManager manager = new MessagingFunctionalityManager(jobDec);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", @"Question Not all Entry Lines being sent to Customs have Freight on them. 

This can either be because the value of the line is so low that there is no freight apportioned against this line, or that no Freight has been entered against the Declaration.

Are you sure you want to submit this Declaration without Freight?", UnitTestUserNotification.Instance.LastMessage.ToString());
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertMultilineASCIIEquals("Message should match this property", "Information Original Entry Message Generated and Ready to be sent by Service Tasks.", UnitTestUserNotification.Instance.LastMessage.ToString());
			StmALog[] logs = jobDec.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationHasErrors.Code));
			AssertEquals(1, logs.Length);
			AssertEquals("Entry Line Found With No Freight - User Sent Anyway", logs[0].SL_Reference);
		}

		public void TestSendingCompletion()
		{
			SetupWithValidPINAndBrokerageID();
			var declaration = Factory.New<JobDeclaration>();
			SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryNumber = entryHeader.Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "54923542";
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SetMostRecentExchangeRateEndDateTo(2014, 03, 21);
			ShowTheSubmitToCustomsForm(declaration);
			var msgOnSubmit = GetMessageShownToUser();
			AssertEquals("messagesShown.ToString()", true, msgOnSubmit.StartsWith("'Original Completion Entry' Message Generated and Ready to be sent by Service Tasks."));
		}

		public void TestSendingCompletion_CustomException()
		{
			SetupWithValidPINAndBrokerageID();
			var declaration = Factory.New<JobDeclaration>();
			SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			var entryHeader = declaration.CustomsEntryHeaders[0];
			var entryNumber = entryHeader.Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "54923542";
			entryNumber.CE_ParentID = entryHeader.PK;
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			SetMostRecentExchangeRateEndDateTo(2014, 03, 21);
			AssertExceptionThrown<Exception>(() => { ShowTheSubmitToCustomsForm(declaration, () => { throw new Exception(); }); });
			AssertEquals("Success message is not shown if custom success event handler throws", false, UnitTestUserNotification.Instance.LastMessage.WasInformation);
		}

		public void TestCreateOrReplaceTransactionRecognizesWriteOff_Original()
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
				var declaration = Factory.New<JobDeclaration>();
				SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
				SetMostRecentExchangeRateEndDateTo(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				ShowTheSubmitToCustomsForm(declaration);
				AssertMultilineASCIIEquals("messagesShown.ToString()", @"Original Entry Message Generated and Ready to be sent by Service Tasks.", GetMessageShownToUser());
			}
		}

		public void TestCreateOrReplaceTransactionRecognizesWriteOff_Replace()
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
				var declaration = Factory.New<JobDeclaration>();
				SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
				SetMostRecentExchangeRateEndDateTo(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				entryHeader.EntryNumber = "54923542";
				ShowTheSubmitToCustomsForm(declaration);
				AssertMultilineASCIIEquals("messagesShown.ToString()", @"Replacement Entry Message Generated and Ready to be sent by Service Tasks.", GetMessageShownToUser());
			}
		}

		public void TestFormalDeclarationHasAnErrorWhenSendingWithNoPackagesForAContainer()
		{
			SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
			var declaration = Factory.New<JobDeclaration>();
			SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
			SetMostRecentExchangeRateEndDateTo(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day);
			declaration.JE_MessageType = Common.NZ.NZJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_PaymentMethod = Customs.NZ.Business.PaymentMethodList.Codes.ClientDeferred;
			declaration.JE_TransactionNature = NatureOfTransactionList.Codes.N10;
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HouseBill";
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			var manager = new MessagingFunctionalityManager(declaration);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(MessageManager.OperationType.SubmitMessage, form, false, false);
			}

			AssertHasError(container.CO_ContainerNumberInfo, CusContainerValidation.ErrorMustHaveContainerPackingLine);
		}

		public void TestSendingManifest()
		{
			SetupWithValidPINAndBrokerageID();
			var manifestDec = Factory.New<JobDeclaration>();
			manifestDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			manifestDec.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			manifestDec.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			manifestDec.JE_DeclarationReference = "M00002758-1";
			manifestDec.JE_DateOfArrival = ZDateTime.Today;
			manifestDec.JE_RL_NKOrigin = "AUSYD";
			manifestDec.JE_RL_NKPortOfLoading = "AUSYD";
			manifestDec.JE_RL_NKPortOfArrival = "NZAKL";
			manifestDec.JE_RL_NKFinalDestination = "NZAKL";
			manifestDec.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			manifestDec.SaveHandlingSaveExceptions();
			manifestDec.DoMerge();
			manifestDec.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader = manifestDec.CustomsEntryHeaders.AddNew(typeof(Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader));
			var manager = new MessagingFunctionalityManager(manifestDec);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(MessageManager.OperationType.SubmitMessage, form, false, false, true);
			}

			var msgOnSubmit = GetMessageShownToUser();
			AssertEquals("messagesShown.ToString()", true, msgOnSubmit.StartsWith("ICR Original Manifest Entry Message Generated and Ready to be sent by Service Tasks."));
		}

		public void TestReplaceTransactionRecognizesICRWriteOff()
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
				var declaration = Factory.New<JobDeclaration>();
				SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				entryHeader.EntryNumber = "54923542";
				ShowTheSubmitToCustomsForm(declaration);
				AssertMultilineASCIIEquals("messagesShown.ToString()", @"ICR Replacement Entry Message Generated and Ready to be sent by Service Tasks.", GetMessageShownToUser());
			}
		}

		public void TestCanSendICRReplacementManifest()
		{
			SetupWithValidPINAndBrokerageID();
			var manifestDec1 = Factory.New<JobDeclaration>();
			manifestDec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			manifestDec1.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			manifestDec1.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			manifestDec1.JE_DeclarationReference = "M00002758-1";
			manifestDec1.JE_DateOfArrival = ZDateTime.Today;
			manifestDec1.JE_RL_NKOrigin = "AUSYD";
			manifestDec1.JE_RL_NKPortOfLoading = "AUSYD";
			manifestDec1.JE_RL_NKPortOfArrival = "NZAKL";
			manifestDec1.JE_RL_NKFinalDestination = "NZAKL";
			manifestDec1.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			manifestDec1.SaveHandlingSaveExceptions();
			manifestDec1.DoMerge();
			manifestDec1.CustomsEntryHeaders.RemoveAndDeleteAll();
			var entryHeader = manifestDec1.CustomsEntryHeaders.AddNew(typeof(Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader));
			entryHeader.CH_BGMReference = "M00002758";
			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			entryHeader.EntryNumber = "54923542";
			var manifestDec2 = Factory.New<JobDeclaration>();
			manifestDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			manifestDec2.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			manifestDec2.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			manifestDec2.JE_DeclarationReference = "M00002758-2";
			manifestDec2.JE_DateOfArrival = ZDateTime.Today;
			manifestDec2.JE_RL_NKOrigin = "AUSYD";
			manifestDec2.JE_RL_NKPortOfLoading = "AUSYD";
			manifestDec2.JE_RL_NKPortOfArrival = "NZAKL";
			manifestDec2.JE_RL_NKFinalDestination = "NZAKL";
			manifestDec2.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			manifestDec2.SaveHandlingSaveExceptions();
			manifestDec2.DoMerge();
			var manager = new MessagingFunctionalityManager(manifestDec1);
			using (ZForm form = new ZForm())
			{
				form.Show();
				manager.ShowSubmitToCustomsForm(MessageManager.OperationType.SubmitMessage, form, false, false, true);
			}

			var msgOnSubmit = GetMessageShownToUser();
			AssertEquals("messagesShown.ToString()", true, msgOnSubmit.StartsWith("ICR Replacement Manifest Entry Message Generated and Ready to be sent by Service Tasks."));
			AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, manifestDec1.JE_EntryStatus);
			AssertEquals(LowValueManifestStatusList.Codes.SentToCustoms, manifestDec2.JE_EntryStatus);
		}

		public void TestSendingReplacementWillRecalculateEntryFee()
		{
			var querierMock = new Mock<IServiceManagerQuerier>();
			querierMock.Setup(x => x.CheckStateOfNamedServiceTask(It.IsAny<string>())).Returns(ServiceTaskStatus.AtLeastOneHostIsRunningHealthily);

			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate();
				var declaration = Factory.New<JobDeclaration>();
				SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(declaration);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				var entryHeader = declaration.CustomsEntryHeaders[0];
				entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
				entryHeader.EntryNumber = "54923542";
				entryHeader.CH_EntryChargeWaived = true;  // simulating CH_EntryChargeWaived being set by processing of original response message

				ShowTheSubmitToCustomsForm(declaration);
				AssertMultilineASCIIEquals("messagesShown.ToString()", @"Replacement Entry Message Generated and Ready to be sent by Service Tasks.", GetMessageShownToUser());
				AssertEquals("CH_EntryChargeWaived should be reset if processing a replacment entry", false, entryHeader.CH_EntryChargeWaived);
			}
		}

		#region Implementation
		void SetupWithValidPINAndBrokerageIDWithOneCurrentExchangeRate()
		{
			SetupWithValidPINAndBrokerageID();
			SetMostRecentExchangeRateEndDateTo(ZDateTime.Today.Year, ZDateTime.Today.Month, ZDateTime.Today.Day);
		}

		void SetupWithValidPINAndBrokerageID()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
			currentUser.GetNZWrapper().NZBPassword.GP_UserID = "WTFZOMG";
			CurrentUsersPin usersPin = new CurrentUsersPin(Factory, false);
			usersPin.DecryptedPinCode = "CandyMountainAdventure";
			Factory.Save();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CandyMountainAdventure");
		}

		void ShowTheSubmitToCustomsForm(JobDeclaration declaration, Action actionOnSuccess = null)
		{
			var manager = new MessagingFunctionalityManager(declaration);
			if (actionOnSuccess != null)
			{
				manager.OnSuccessfullyExecuted += actionOnSuccess;
			}
			using (var parentForm = new ZForm())
			{
				parentForm.Show();
				manager.ShowSubmitToCustomsForm(MessageManager.OperationType.SubmitMessage, parentForm, false, false);
			}
		}

		void SetupDeclarationWithOneInvoiceLineOneHouseBillAndOneMergedLine(JobDeclaration declaration)
		{
			var declarationBuilder = new TestFormalEntryCreator(declaration);
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declarationBuilder.SetupTestConsignmentDetails();
			declarationBuilder.SetupTestForAir();
			declarationBuilder.SetupTestForImportFromAU();
			declarationBuilder.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			declarationBuilder.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			declarationBuilder.SetupImportInvoiceLine("0000.00.00.00A", "DESCRIPTION", "NZ", "NZ", "N", 10000m);
			declarationBuilder.AddHouseBillWithPackingDetails("HOUSEBILL", 10, "PK");
			Assert(declarationBuilder.Declaration.SaveHandlingSaveExceptions());
			declarationBuilder.MergeDeclaration();
			declarationBuilder.MergeLineSetDutyAndTax(0, 650.00m, 1457.50m);
		}

		void CreateExchangeRateRow(string currencyCode, ZDateTime endDate, decimal rate)
		{
			NZCCustomsExchangeRate exchangeRate = Factory.New<NZCCustomsExchangeRate>();
			exchangeRate.U7_CurrencyCode = currencyCode;
			exchangeRate.U7_DateActiveFrom = endDate.AddDays(-14);
			exchangeRate.U7_DateActiveTo = endDate;
			exchangeRate.U7_Rate = rate;
		}

		void SetMostRecentExchangeRateEndDateTo(int year, int month, int day)
		{
			TestCaseHelper.ClearTable(NZCCustomsExchangeRate.Schema.TableName);
			CreateExchangeRateRow("USD", new ZDateTime(year, month, day), 1.12m);
			AssertEquals("NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory)", new ZDate(year, month, day), NZCCustomsExchangeRate.GetMostRecentExchangeRateEndDate(Factory));
		}

		static string GetMessageShownToUser()
		{
			var messagesShown = new StringBuilder();
			foreach (UnitTestUserNotification.PreviousMessage message in UnitTestUserNotification.Instance.PreviousMessages)
			{
				messagesShown.AppendLine(message.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			return messagesShown.ToString();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.SetupMessagingEnvironment();
		}
		#endregion
	}
}
