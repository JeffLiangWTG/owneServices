using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.GUI.Protest;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Bill = Enterprise.Customs.US.Business.Bill;
using CusEntryLine = Enterprise.Customs.US.Business.CusEntryLine;
using ISF = Enterprise.Customs.Common.US.ISF;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class JobDeclarationModuleTest : Customs.Module.Testing.JobDeclarationModuleAbstractTest
	{
		[TestDate(2006, 08, 18)]
		[ExpectNoExceptions]
		public void TestActionMenu()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				Menu.MenuItemCollection menus = module.GetNewActionMenuItems().FindByText("Reference Files Request").MenuItems;
				foreach (MenuItem menuItem in menus)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					menuItem.PerformClick();
				}
			}
		}

		public void TestShowNAFTAInIssueList()
		{
			using (var module = new JobDeclarationModule())
			{
				AssertEquals(false, ((JobDeclarationFilterBusinessObject)module.FilterBusinessObject).Lookups.ReconIssueList.ContainsCode(ReconIssueCodeList.Codes.FTA));
			}
		}

		public void TestBIRDImportMenu()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				MenuAssertion.AssertHasMenu("Import from BIRD", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &BIRD");
				module.ImportMenuItems["Import From &BIRD"].Invoke(null, EventArgs.Empty);
				AssertEquals(typeof(DataImporterForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestCorrectFormIsOpened()
		{
			AssertCorrectFormIsOpened<JobDeclarationModule>(AssertPreviousNextControl);
		}

		public void TestGetNewController()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				var reconDeclaration = Factory.New<JobDeclaration>();
				reconDeclaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
				ZController controller = module.GetNewController(ProtestTypeDeclaration);
				Assert(controller is Protest.ProtestController);
				controller = module.GetNewController(reconDeclaration);
				Assert(controller is ReconController);
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				controller = module.GetNewController(declaration);
				Assert(controller is JobDeclarationController);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
				controller = module.GetNewController(declaration);
				Assert(controller is DrawbackController);
				var drawbackDeclaration = Factory.New<JobDeclaration>();
				drawbackDeclaration.SetDefaultValuesForDrawback();
				controller = module.GetNewController(drawbackDeclaration);
				Assert(controller is DrawbackController);
			}
		}

		public void TestCreateAndRunBulkLandedCosting()
		{
			JobDeclaration expDec = Factory.New<JobDeclaration>();
			expDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			expDec.Invoices.AddNew();
			expDec.InvoiceLines.AddNew();
			JobDeclaration impDecWithLine = Factory.New<JobDeclaration>();
			impDecWithLine.JE_MessageType = JobMessageTypeList.Codes.Import;
			impDecWithLine.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			impDecWithLine.Invoices.AddNew();
			impDecWithLine.InvoiceLines.AddNew();
			JobDeclaration impDecWithoutLine = Factory.New<JobDeclaration>();
			impDecWithoutLine.JE_MessageType = JobMessageTypeList.Codes.Import;
			impDecWithoutLine.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			impDecWithoutLine.Invoices.AddNew();
			Factory.Save();
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.SelectedBusinessObjectsForLC = new BusinessObject[] { expDec, impDecWithLine, impDecWithoutLine };
				module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Run Landed Costing").PerformClick();
				string text = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertNotNull(text);
				AssertContains(expDec.JE_DeclarationReference, text);
				AssertContains(impDecWithoutLine.JE_DeclarationReference, text);
				BusinessObject landedCostHeader = Factory.LoadTop1(ObjectFactory.GetType<Integration.LandedCosting.ILandedCostHeader>(), new LandedCostHeaderFilter(impDecWithLine));
				AssertNotNull(landedCostHeader);
			}
		}

		public void TestACEAntiDumpingMenuItemVisibility()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var item = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Reference Files Request").MenuItems.FindByText("Anti-Dumping and Countervailing(ACE)");
				AssertNotNull(item);
			}
		}

		public void TestRequestSpecialistTeamAssignmentFileMenu()
		{
			using (var module = new JobDeclarationModuleForTest())
			{
				var item = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Reference Files Request").MenuItems.FindByText("Request Specialist Team Assignment File");
				AssertNotNull(item);
				UnitTestUserNotification.Instance.ClearMessages();
				item.PerformClick();
				AssertEquals(JobDeclarationModule.NotSupportedByCBPForSpecialistTeamQuery, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCalculateDutySavingsMenuItem()
		{
			var helper = new Business.Testing.EntryDutyCalculatorTestDataHelper(Factory);
			helper.SetupReferenceData();
			var expDec = helper.CreateDeclaration();
			expDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			expDec.JE_DeclarationReference = "T10EXPDEC";
			expDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(expDec);
			var mscDec = helper.CreateDeclaration();
			mscDec.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			mscDec.US_EntryFilerCode = "XJ5";
			mscDec.JE_DeclarationReference = "T10MSCDEC";
			mscDec.US_EnableCRL = true;
			mscDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(mscDec);
			var ensDisableDec = helper.CreateDeclaration();
			ensDisableDec.US_EnableENS = false;
			ensDisableDec.US_EnableCRL = true;
			ensDisableDec.JE_DeclarationReference = "T10ENSDISDEC";
			ensDisableDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(ensDisableDec);
			var noDutyCalcDec = helper.CreateDeclaration();
			noDutyCalcDec.US_NoDutyCalc = true;
			noDutyCalcDec.JE_DeclarationReference = "T10NODUTYDEC";
			noDutyCalcDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(noDutyCalcDec);
			var dec1 = helper.CreateDeclaration();
			dec1.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(dec1);
			var dec2 = helper.CreateDeclaration();
			dec2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(dec2);
			var dec3 = helper.CreateDeclaration();
			dec3.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ResetData(dec3);
			Factory.Save();
			using (var module = new JobDeclarationModuleForTest())
			{
				module.CountryCode = GlbCompany.CurrentCompany.Country.Code;
				var menuItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Calculate Duty Savings Data");
				CombineAssertions(() =>
				{
					Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertEquals("Security not allowed", Env.Security.CustomsDeclarationEnquiryEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					Env.Security.CustomsDeclarationEnquiryEdit.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertEquals("nothing selected", "Please select at least one Customs Declaration to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
					module.SelectedBusinessObjectsForTesting = new BusinessObject[] { expDec, mscDec, ensDisableDec, noDutyCalcDec, dec1, dec3 };
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menuItem.PerformClick();
					AssertMultilineASCIIEquals($@"
Duty Savings calculation finished running.

Calculation was not run on the following selected Customs Declarations.
Duty Savings is only applicable where Shipment Type is IMP(Import) and Enable ENS is ticked.

{expDec.JE_DeclarationReference}, {mscDec.JE_DeclarationReference}, {ensDisableDec.JE_DeclarationReference}", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertDutySavingData(expDec.PK, false);
					AssertDutySavingData(mscDec.PK, false);
					AssertDutySavingData(ensDisableDec.PK, false);
					AssertDutySavingData(noDutyCalcDec.PK, false);
					AssertDutySavingData(dec1.PK, true);
					AssertDutySavingData(dec2.PK, false);
					AssertDutySavingData(dec3.PK, true);
				});
			}
		}

		public void TestGetNewStandardMenuItems()
		{
			using (JobDeclarationModuleForTest module = new JobDeclarationModuleForTest())
			{
				AssertNotNull("Set by AllowNew ocerride in helper module test class", module.GetNewStandardMenuItems().FindByText("&New"));
				AssertNotNull("Registry item is ON, Declaration menu items should be found", module.GetNewStandardMenuItems().FindByText("&New").MenuItems.FindByText("New Declaration"));
				AssertNotNull("Registry item is ON, Protest menu items should be found", module.GetNewStandardMenuItems().FindByText("&New").MenuItems.FindByText("New Protest"));
				AssertNotNull("Registry item is ON, Reconciliation menu items should be found", module.GetNewStandardMenuItems().FindByText("&New").MenuItems.FindByText("New Reconciliation"));
				AssertNotNull("Registry item is ON, Drawback menu items should be found", module.GetNewStandardMenuItems().FindByText("&New").MenuItems.FindByText("New Drawback"));
			}
		}

		public void TestTotalInvoicedDetailsDBHits()
		{
			var accChgCode = Factory.NewWithValidTestData<AccChargeCode>();
			accChgCode.AC_ChargeType = "DSB";
			accChgCode.AC_Code = "TST";
			accChgCode.AC_GC = GlbCompany.CurrentCompany.PK;
			Enterprise.Registry.Business.RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, accChgCode.PK.ToGuid());
			for (int i = 1; i <= 10; i++)
			{
				if (i <= 5)
				{
					var declaration = Factory.New<Customs.Business.BaseJobDeclaration>();
					var jobLinked = CreateJob(declaration.PK, Enterprise.ZArchitecture.Schema.JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
					jobLinked.JH_JobNum = i.ToString();
					var charge2 = AddCharge(jobLinked, accChgCode.PK, "2", i, i.ToString("D8"));
					declaration.RefreshAccounting_ARInvoiceQueryResult();
				}
				else
				{
					var declaration2 = Factory.New<Customs.Business.BaseJobDeclaration>();
					var jobLinkedToDeclaration2 = CreateJob(declaration2.PK, Enterprise.ZArchitecture.Schema.JobDeclarationSchema.Constants.Prefix, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK);
					jobLinkedToDeclaration2.JH_JobNum = i.ToString();
					var charge = AddCharge(jobLinkedToDeclaration2, accChgCode.PK, "1", i + 30, i.ToString("D8"));
					charge.ARLine.TransactionHeader.AH_TransactionCategory = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice;
					charge.ARLine.TransactionHeader.AH_JH = jobLinkedToDeclaration2.PK;
					declaration2.RefreshAccounting_ARInvoiceQueryResult();
				}
			}

			Factory.Save();
			Factory.ExecuteAllFetchHints();
			using (var module = (JobDeclarationModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (var form = new ZChildForm(module.GridCollection))
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					((ZFilterStripControl)module.EmbeddedControl).FilteredGrid.SetColumnVisible(true, new string[] { JobDeclarationSchema.Constants.JE_DeclarationReference, "TotalBilledAmount", "TotalOutstandingAmount", "TotalInvoicedAmount" });
					((ZFilterStripControl)module.EmbeddedControl).FilteredGrid.ReOrderColumns(new string[] { JobDeclarationSchema.Constants.JE_DeclarationReference, "TotalBilledAmount", "TotalOutstandingAmount", "TotalInvoicedAmount" });
					((ZFilterStripControl)module.EmbeddedControl).FirePerformSearch();
					foreach (var job in module.GridCollection.Cast<Customs.Business.BaseJobDeclaration>())
					{
						job.RefreshAccounting_ARInvoiceQueryResult();
					}

					((ZFilterStripControl)module.EmbeddedControl).Refresh();
					IBusinessObjectCollection collection = module.GridCollection;
					AssertNotEquals(0, collection.Count);
					Assert("AccTransactionLines", collection.Factory.GetTableHitCount("AccTransactionLines") <= 1);
					AssertEquals(1, collection.Factory.GetTableHitCount("AccTransactionHeader"));
					AssertEquals(1, collection.Factory.GetTableHitCount("JobDeclaration"));
				}
			}
		}

		protected override List<string> FetchHintIgnoreField
		{
			get
			{
				List<string> result = base.FetchHintIgnoreField;
				result.Add("TotalBilledAmount"); //this is in a dynamic collection - so fetch hints wont work.
				result.Add("TotalOutstandingAmount"); //this is in a dynamic collection - so fetch hints wont work.
				result.Add("TotalInvoicedAmount"); //this is in a dynamic collection - so fetch hints wont work.
				result.Add("FTZAdmissionNumberFormatted");
				result.Add("SimplifiedEntryBillStatus");
				result.Add("SimplifiedEntryBillStatusDescription");
				result.Add("HLDOrEXMStatus");
				result.Add("InBondClosedDate");
				result.Add("InBondEntryTypes");
				result.Add(JobDeclaration.Schema.DISStatus); // WI00076482 - Tim.Van : temporarity ignore it for now, will come back and finish it later since it needs more thoughts.
				result.Add(JobDeclaration.Schema.DISStatusDescription);
				result.Add(JobDeclaration.Schema.IncompleteDispositionsCode);
				result.Add(JobDeclaration.Schema.IncompleteDispositionsDescription);
				return result;
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override Type GetExpectedJobDeclarationType() => typeof(JobDeclaration);

		protected override Type GetExpectedInvoiceHeaderType() => typeof(JobComInvoiceHeader);

		protected override Type GetExpectedInvoiceLineType() => typeof(JobComInvoiceLine);

		internal static void AssertCorrectFormIsOpened<T>(Action<T> extraAssertion = null)
			where T : ZFilterGridModule, new()
		{
			var factory = new BusinessObjectFactory();
			var expectResults = new Dictionary<JobDeclaration, Tuple<Type, ControllerID>>();
			var shipmentExportDec = factory.New<JobDeclaration>();
			shipmentExportDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var shipmentExport = factory.New<ForwardingShipment>();
			shipmentExportDec.JE_JS = shipmentExport.PK;
			expectResults.Add(shipmentExportDec, new Tuple<Type, ControllerID>(typeof(ShipmentForm), ControllerIDs.Customs.JobDeclarationPluggedIntoShipment));
			var shipmentImportDec = factory.New<JobDeclaration>();
			shipmentImportDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			shipmentImportDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var shipmentImport = factory.New<ForwardingShipment>();
			shipmentImportDec.JE_JS = shipmentImport.PK;
			expectResults.Add(shipmentImportDec, new Tuple<Type, ControllerID>(typeof(ShipmentForm), ControllerIDs.Customs.JobDeclarationPluggedIntoShipment));
			var shipmentImportByExternalBrokerDec = factory.New<JobDeclaration>();
			shipmentImportByExternalBrokerDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			var shipmentImportByExternalBroker = factory.New<ForwardingShipment>();
			shipmentImportByExternalBrokerDec.JE_JS = shipmentImportByExternalBroker.PK;
			expectResults.Add(shipmentImportByExternalBrokerDec, new Tuple<Type, ControllerID>(typeof(ShipmentForm), ControllerIDs.Customs.JobDeclarationPluggedIntoShipment));
			var shipmentMiscellaneousDec = factory.New<JobDeclaration>();
			shipmentMiscellaneousDec.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			var shipmentMiscellaneous = factory.New<ForwardingShipment>();
			shipmentMiscellaneousDec.JE_JS = shipmentMiscellaneous.PK;
			expectResults.Add(shipmentMiscellaneousDec, new Tuple<Type, ControllerID>(typeof(ShipmentForm), ControllerIDs.Customs.JobDeclarationPluggedIntoShipment));
			var exportDec = factory.New<JobDeclaration>();
			exportDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			expectResults.Add(exportDec, new Tuple<Type, ControllerID>(typeof(JobDeclarationForm), ControllerIDs.Customs.JobDeclaration));
			var importDec = factory.New<JobDeclaration>();
			importDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			expectResults.Add(importDec, new Tuple<Type, ControllerID>(typeof(JobDeclarationForm), ControllerIDs.Customs.JobDeclaration));
			var importByExternalBrokerDec = factory.New<JobDeclaration>();
			importByExternalBrokerDec.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			expectResults.Add(importByExternalBrokerDec, new Tuple<Type, ControllerID>(typeof(JobDeclarationForm), ControllerIDs.Customs.JobDeclaration));
			var miscellaneousDec = factory.New<JobDeclaration>();
			miscellaneousDec.JE_MessageType = JobMessageTypeList.Codes.Miscellaneous;
			expectResults.Add(miscellaneousDec, new Tuple<Type, ControllerID>(typeof(JobDeclarationForm), ControllerIDs.Customs.JobDeclaration));
			var protestDec = factory.New<JobDeclaration>();
			protestDec.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			expectResults.Add(protestDec, new Tuple<Type, ControllerID>(typeof(ProtestForm), ControllerIDs.Customs.US.Protest));
			var reconDec = factory.New<JobDeclaration>();
			reconDec.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconEntry = reconDec.CustomsEntryHeaders.AddNew();
			reconEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			expectResults.Add(reconDec, new Tuple<Type, ControllerID>(typeof(ReconDeclarationForm), ControllerIDs.Customs.US.Recon));
			var drawbackDec = factory.New<JobDeclaration>();
			drawbackDec.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			expectResults.Add(drawbackDec, new Tuple<Type, ControllerID>(typeof(JobDeclarationForm), ControllerIDs.Customs.US.Drawback));
			factory.Save();
			foreach (var pair in expectResults)
			{
				var dec = pair.Key;
				using (var module = new T())
				{
					module.GridCollection.AddRange(expectResults.Keys);
					module.UpdateModuleResultsCache();
					IFilterModuleInternalsForTesting moduleForTesting = module;
					module.ModuleDecisionProvider.HandleFindBoxOKButton(new[] { dec });
					var controller = moduleForTesting.LastController;
					AssertEquals(dec.JE_MessageType, pair.Value.Item2, controller.ID);
					var form = controller.LastShownForm;
					AssertEquals(dec.JE_MessageType, pair.Value.Item1, form.GetType());
					if (extraAssertion != null)
					{
						extraAssertion(module);
					}

					form.Dispose();
				}
			}
		}

		void AssertPreviousNextControl(JobDeclarationModule module)
		{
			IFilterModuleInternalsForTesting moduleForTesting = module;
			var controller = moduleForTesting.LastController;
			var form = controller.LastShownForm;
			IPreviousNextControlProvider provider = form as IPreviousNextControlProvider;
			var previousNextControl = provider.PreviousNextControlForTesting;
			AssertEquals(form.GetType() + ".PreviousNextControl should be visible", true, previousNextControl.Visible);
		}

		void AssertDutySavingData(ZGuid decPK, bool exists)
		{
			var factory = new BusinessObjectFactory();
			var dec = factory.Load<JobDeclaration>(decPK);
			AssertEquals(dec.JE_DeclarationReference + " Duty Saving Data", exists, dec.InvoiceLines
				.Cast<JobComInvoiceLine>()
				.Any(x => !x.US_FTADuty.IsEmpty || !x.US_FTAPayableMPF.IsEmpty || !x.US_NonFTADuty.IsEmpty || !x.US_NonFTAPayableMPF.IsEmpty));
			AssertEquals(dec.JE_DeclarationReference + " Exiting Duty Data not changed", false, dec.CustomsEntryHeaders
				.Cast<CusEntryHeader>()
				.SelectMany(x => x.MergedLines.Cast<CusEntryLine>())
				.SelectMany(x => x.Fees.Cast<CusEntryLineFee>())
				.Any(x => x.CF_ChargeAmount != 1m));
		}

		void ResetData(JobDeclaration dec)
		{
			foreach (JobComInvoiceLine invoiceLine in dec.InvoiceLines)
			{
				invoiceLine.ResetDutyAndFeeAnalysisData();
				var entryLine = invoiceLine.CusEntryLine;
				foreach (CusEntryLineFee fee in invoiceLine.CusEntryLine.Fees)
				{
					fee.CF_ChargeAmount = 1m;
				}
			}
		}

		JobHeader CreateJob(ZGuid pK, ZString tableCode, ZGuid branchPK, ZGuid companyPK)
		{
			var result = Factory.NewJobForTesting<JobHeader>();
			result.JH_ParentID = pK;
			result.JH_ParentTableCode = tableCode;
			result.JH_GB = branchPK;
			result.JH_GC = companyPK;
			return result;
		}

		JobCharge AddCharge(JobHeader job, ZGuid chargeCode, ZString apInvoiceNum, int trnCnt, string transactionNumber)
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_AC = chargeCode;
			charge.JR_APInvoiceNum = apInvoiceNum;
			charge.JR_APInvoiceDate = ZDateTime.Today;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSCostAmt = 50;
			charge.JR_OSSellAmt = 50;
			charge.JR_LocalSellAmt = 50;
			var invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = transactionNumber;
			invoice.AH_Ledger = "AR";
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_TransactionCount = (byte)trnCnt;
			invoice.AH_TransactionType = "INV";
			var invoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			invoiceLine.AL_AH = invoice.PK;
			invoiceLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			invoiceLine.AL_RevRecognitionType = "CUS";
			invoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge.JR_AL_ARLine = invoiceLine.PK;
			invoiceLine.AL_LineAmount = 50;
			invoiceLine.AL_OSAmount = 50;
			invoice.AH_OutstandingAmount = 50;
			invoice.AH_InvoiceAmount = 50;
			return charge;
		}

		JobDeclaration ProtestTypeDeclaration
		{
			get
			{
				if (protestTypeDeclaration == null)
				{
					protestTypeDeclaration = Factory.New<JobDeclaration>();
					protestTypeDeclaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
				}

				return protestTypeDeclaration;
			}
		}

		JobDeclaration protestTypeDeclaration;
		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForFetchHintTest(BusinessObjectFactory factory, string messageType, int i)
		{
			CreateChgCodes(factory);
			var declaration = (JobDeclaration)base.CreateDeclarationForFetchHintTest(factory, messageType, i);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var organisations = factory.GetCachedValue("BaseOrganisationDeclarationModuleTest", delegate
			{
				return factory.Load<OrgHeader>(new ZQuery()
				{ MaximumRows = 24 });
			});
			var vessels = factory.GetCachedValue("BaseVesselDeclarationModuleTest", delegate
			{
				return factory.Load<RefVessel>(new ZQuery()
				{ MaximumRows = 6 });
			});
			var unlocos = factory.GetCachedValue("BaseUNLOCODeclarationModuleTest", delegate
			{
				return factory.Load<RefUNLOCO>(new ZQuery()
				{ MaximumRows = 11 });
			});
			string number = i.ToString();
			string twoDigitsNumber = number.PadLeft(2, '0');
			int mod6 = i % 6;
			int mod3 = i % 3;
			var lookups = declaration.Lookups;
			declaration.US_EnableENS = true;
			declaration.US_EnableAII = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableINB = true;
			declaration.US_PaperlessEntry = "Y";
			declaration.US_SchDEntry = "8888";
			declaration.US_EntryFilerCode = "X" + twoDigitsNumber;
			declaration.US_TotalEnteredValue = 8888m;
			if (declaration.IsFormalImport)
			{
				declaration.ImportEntryNumber = "ENS123" + twoDigitsNumber;
			}

			declaration.JE_DeclarationReference = messageType + "B0000100" + i.ToString();
			declaration.JE_RL_NKPortOfFirstArrival = unlocos[i].RL_Code;
			declaration.JE_RL_NKFinalDestination = unlocos[i].RL_Code;
			declaration.JE_OH_Forwarder = organisations[mod6 + 12].PK;
			declaration.JE_OH_ShippingLine = organisations[mod6 + 18].PK;
			declaration.JE_OH_ExternalBroker = organisations[mod6 + 18].PK;
			var entryStatusList = lookups.EntryStatusList;
			declaration.JE_EntryStatus = entryStatusList[i % entryStatusList.Count].Code;
			declaration.JE_RL_NKOrigin = unlocos[(i + 1) % 10].RL_Code;
			var cargoIdTypeList = lookups.CargoIdTypeList;
			declaration.JE_ContainerMode = cargoIdTypeList[i % cargoIdTypeList.Count].Code;
			declaration.JE_RL_NKPortOfArrival = unlocos[(i + 2) % 10].RL_Code;
			declaration.JE_RL_NKPortOfLoading = unlocos[(i + 3) % 10].RL_Code;
			declaration.US_InbondType = factory.GetCachedValue<EntryTypeList>()[(i + 1) % 10].Code;
			declaration.IOROrgPK = declaration.JE_OH_Importer;
			declaration.ConsigneeAddressOrgPK = declaration.JE_OH_Importer;
			declaration.JE_OH_NotifyParty = declaration.JE_OH_Importer;
			declaration.SoldToPartyOrgPK = declaration.JE_OH_Importer;
			declaration.US_BondWaiverCode = "XX";
			declaration.US_InBondExportTransMode = declaration.JE_Calc_USTransportMode;
			declaration.US_EntryType = factory.GetCachedValue<EntryTypeList>()[i].Code;
			declaration.US_EstimatedEntryDate = ZDateTime.Now.AddMinutes(i);
			declaration.US_PaymentType = factory.GetCachedValue<PaymentTypeList>()[mod6].Code;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(i);
			declaration.US_PeriodicStatementMM = factory.GetCachedValue<MonthList>()[i].Code;
			declaration.US_SuretyCode = "7" + twoDigitsNumber;
			declaration.US_ADDCVDSuretyCode = "9" + twoDigitsNumber;
			declaration.US_LiveEntryIndicator = factory.GetCachedValue<YesNoDefaultList>()[i % 2].Code;
			declaration.US_ConsolidatedInformalIndicator = factory.GetCachedValue<ConsolidatedInformalList>()[mod3].Code;
			declaration.US_GeneralOrderNo = "GENORD" + number;
			declaration.US_BondProducerAccNo = "BNDPRC" + number;
			declaration.BLUStatus = factory.GetCachedValue<MessageStatusListBLU>()[mod3].Code;
			declaration.US_US_NKLocationOfGoods = "E182";
			declaration.FDAMsgStatus = factory.GetCachedValue<FDAStatusList>()[mod6].Code;
			declaration.ShippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "SC" + twoDigitsNumber, Core.Constants.CountryCodes.UnitedStates);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			declaration.US_SchDLoading = "8888";
			declaration.US_SchDArrival = "73477";
			declaration.JE_RL_NKPortOfArrival = "USSFO";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_DeferredTaxDueDate = ZDateTime.Today;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today.AddDays(i - 2);
			declaration.US_BRDRefNo = "7984798";
			declaration.US_CheckNo = "2";
			declaration.US_PresentationDate = ZDateTime.Now.AddDays(i);
			declaration.US_PreparerDistrictPort = "123";
			declaration.US_BondDispositionCode = "123";
			declaration.US_BondDispositionCode2 = "123";
			declaration.US_InsuranceDisposition = "A";
			declaration.US_PGAReplaceUpdateNeeded = i % 2 == 0 ? Customs.Business.YesNoList.Codes.Yes : string.Empty;
			declaration.US_PGACorrectionStatus = PGACorrectionStatusList.Codes.ClearPGADataCorrection;
			declaration.US_QuotaStatus = CargoReleaseProcessingResultList.Codes.QuotaPending;
			declaration.US_ConsolidatedJobNumber = "SV91234567";
			declaration.US_WHSEntryFilerCode = "SV9";
			declaration.US_WHSEntryNumber = "1234567";
			declaration.US_QtyInWHBeforeWithdrawal = 1m;
			declaration.US_QtyBeingWithdrawn = 2m;
			declaration.DocsAndCartage.JP_OrderItemsAsString = "DOCORD" + number;
			var exportEntry = declaration.CustomsEntryHeaders.AddNew();
			exportEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			exportEntry.CH_Status = factory.GetCachedValue<AESDirectCustomsEntryStatus>()[mod6].Code;
			var entrySummary = declaration.CustomsEntryHeaders.AddNew();
			entrySummary.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entrySummary.CH_Status = factory.GetCachedValue<ImportMessageStatusList>().AcceptedStatusToCancelRejectStatusInterested[mod6];
			entrySummary.CH_EntrySubmittedDate = ZDateTime.Today.AddDays(i - 1);
			entrySummary.CH_TotalPaid = 32340m + i;
			entrySummary.US_TIBExpiryDate = ZDateTime.Today.AddDays(1);
			var entryLine = factory.NewWithValidTestData<CusEntryLine>();
			entryLine.CL_CH = entrySummary.PK;
			entryLine.CL_CustomsValue = i * 100;
			var cargoReleaseEntry = declaration.CustomsEntryHeaders.AddNew();
			cargoReleaseEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			cargoReleaseEntry.CH_Status = factory.GetCachedValue<ImportMessageStatusList>()[i].Code;
			var inBondEntry = declaration.CustomsEntryHeaders.AddNew();
			inBondEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inBondEntry.CH_Status = cargoReleaseEntry.CH_Status;
			declaration.Logs.AddNew(Events.RecordAudited, "~SPI");
			declaration.Logs.AddNew(Events.RecordAudited, "~FDA");
			declaration.Logs.AddNew(Events.StatusUpdated, "~TIB");
			declaration.Logs.AddNew(Events.RecordAudited, "~CWO");
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_MessageStatus = factory.GetCachedValue<MessageStatusListEI>()[i].Code;
			var statementHeader = declaration.Factory.New<CusStatementHeader>();
			statementHeader.B2_PaymentAuthorizationDate = ZDateTime.Today.AddDays(2);
			statementHeader.B2_StatementNumber = "STN" + number;
			statementHeader.B2_Status = factory.GetCachedValue<StatementHeaderStatusList>()[mod3].Code;
			statementHeader.B2_PaymentStatus = factory.GetCachedValue<PaymentStatusList>()[mod3].Code;
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = declaration.EntryFilerCode;
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			var ogaDisposition = declaration.OGADispositionCodes.AddNew();
			ogaDisposition.US_OGAIdentifier = "FDA";
			ogaDisposition.US_OGADispositionStatusMessage = "FDA MAY PROCEED" + number;
			ogaDisposition.US_DispositionDate = ZDateTime.Now.AddDays(i);
			declaration.FDAStatus = FDAEntryLevelDispositionCodeList.Codes._01;
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			declaration.Liquidations.AddNew().B8_LiquidationDate = ZDateTime.Today;
			if (messageType == JobMessageTypeList.Codes.Import)
			{
				CreateTransactionData(declaration, i);
				CreateISFBillStatus(factory, i);
				declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings;
				declaration.FTZArrivalStatus = FTZMessageStatusList.Codes.AwaitingGoodsArrival;
				declaration.FTZConcurrenceStatus = FTZMessageStatusList.Codes.ClearConcurrence;
				declaration.FTZDeliveryOfGoodsStatus = FTZMessageStatusList.Codes.ClearDeliveryOfGoods;
				declaration.FTZPTTStatus = FTZMessageStatusList.Codes.ClearPermitToTransfer;
				declaration.FTZAdmissionNumber = "1530001|15|00000005";
				declaration.US_TeamNo = "124";
				entrySummary.US_ALDate = ZDateTime.Today.AddDays(1);
				entrySummary.US_CollectionDate = ZDateTime.Today.AddDays(-2);
				var billForSETesting = declaration.Bills.AddNew();
				billForSETesting.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				billForSETesting.CU_BillNum = "SE Test 1";
				billForSETesting.DispositionCodes.AddNewIfNotExist("95", ZDateTime.Today);
			}
			else if (messageType == JobMessageTypeList.Codes.Export)
			{
				CreateShipperReferenceNumber(declaration, i);
				declaration.US_DateOfExport = ZDateTime.Today;
				declaration.US_SchDExport = "3901";
				declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;
				invoice.InvoiceLines.AddNew();

				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESSeverityIndicator, "AESSeverityIndicator");
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESResponseCode, "AESResponseCode");
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESSeverityIndicator, "W", "Test 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AESResponseCode, "11A", "Test 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				var entry = declaration.CustomsEntryHeaders.Count > 0 ? declaration.CustomsEntryHeaders[0] : declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = declaration.JE_MessageType;
				entry.EntryNumber = "S40002511";
				entry.AESCusDispositions.AddCusDisposition("W", ZDateTime.Today, "11A");
			}

			var cusDisposition = declaration.EntryPGACusDispositions.AddNew();
			cusDisposition.CDI_Notes = "123";
			cusDisposition.CDI_Status = "07";
			cusDisposition.CDI_StatusKey = "FDA";
			cusDisposition.CDI_Type = Customs.Business.CusDispositionTypeCodeList.Codes.USPGAEntryStatus;
			cusDisposition.CDI_StatusDate = ZDateTime.BrettsBirthday;
			//var requiredDoc = declaration.DocsAndCartage.RequiredDocuments.AddNew();
			//requiredDoc.EQ_DocType = Core.Constants.RefDocTypes.CommercialInvoice;
			//var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			//requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			//requiredDocAddInfo.EX_Status = Enterprise.Customs.Common.US.DIS.StatusList.Codes.AOS;
			if (declaration.Importer != null && declaration.Importer.CustomsCodes.Count == 0)
			{
				declaration.Importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, string.Format("{0}4-56789012XX", number));
			}

			return declaration;
		}

		void CreateShipperReferenceNumber(JobDeclaration declaration, int i)
		{
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "BGM" + i.ToString();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
		}

		void CreateISFBillStatus(BusinessObjectFactory factory, int i)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			foreach (var isfBillStatus in GetISFBillStatusesFor(i))
			{
				Bill bill = declaration.Bills.AddNew();
				bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
				bill.CU_BillNum = "MBXXX" + i.ToString();
				if (!isfBillStatus.IsEmpty)
				{
					CreateISFJob(factory, bill.CU_BillNum, ISF.BillTypeList.Codes.OceanBillOfLading, isfBillStatus);
				}
			}
		}

		Integration.Customs.US.ISF.ICusISFBill CreateISFJob(BusinessObjectFactory factory, ZString billNumber, ZString billType, ZString customsStatus)
		{
			var header = factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			var bill = factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill.BB_BF = header.PK;
			bill.BB_BillType = billType;
			bill.BB_BillNum = billNumber;
			bill.BB_CustomsStatus = customsStatus;
			return bill;
		}

		ZString[] GetISFBillStatusesFor(int i)
		{
			switch (i)
			{
				case 1:
					return new ZString[] { ISF.DispositionCodeList.Codes.S2, ISF.DispositionCodeList.Codes.S3 };
				case 2:
					return Array.Empty<ZString>();
				case 3:
					return new ZString[] { ISF.DispositionCodeList.Codes.S4, ZString.Empty };
				case 4:
					return new ZString[] { ISF.DispositionCodeList.Codes.S5 };
				case 5:
					return new ZString[] { ISF.DispositionCodeList.Codes.S6 };
				case 6:
					return new ZString[] { ISF.DispositionCodeList.Codes.S7 };
				default:
					return new ZString[] { ISF.DispositionCodeList.Codes.S1 };
			}
		}

		sealed class JobDeclarationModuleForTest : JobDeclarationModule
		{
			public new ZController GetNewController(BusinessObject selectedBusinessObject) => base.GetNewController(selectedBusinessObject);
			public new MenuItem[] GetNewStandardMenuItems() => base.GetNewStandardMenuItems();
			public new FilterModuleMenuItemDescriptorCollection ImportMenuItems => base.ImportMenuItems;
			public new MenuItem[] GetNewActionMenuItems() => base.GetNewActionMenuItems();
			public override bool AllowNew => true;
			public BusinessObject[] SelectedBusinessObjectsForLC;
			protected override BusinessObject[] GetSelectedBusinessObjectsForBulkLC() => SelectedBusinessObjectsForLC ?? base.GetSelectedBusinessObjectsForBulkLC();
			public BusinessObject[] SelectedBusinessObjectsForTesting;
			public override BusinessObject[] GetSelectedBusinessObjects() => SelectedBusinessObjectsForTesting ?? base.GetSelectedBusinessObjects();
		}
	}
}
