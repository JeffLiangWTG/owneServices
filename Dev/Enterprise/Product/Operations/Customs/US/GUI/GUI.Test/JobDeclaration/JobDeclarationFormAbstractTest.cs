using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class JobDeclarationFormAbstractTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public void TestPlugInsForDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertNull(ControllerIDs.LandedCosting.Name, form.PlugIns.GetPlugIn(ControllerIDs.Routing));
				AssertNull(ControllerIDs.LandedCosting.Name, form.PlugIns.GetPlugIn(ControllerIDs.LandedCosting));
				AssertNotNull(ControllerIDs.JobInvoicing.Name, form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull(ControllerIDs.DocAddresses.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				AssertNotNull(ControllerIDs.eDocsPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull(ControllerIDs.DocDataPlugIn.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNull(ControllerIDs.Customs.US.InBond.Name, form.PlugIns.GetPlugIn(ControllerIDs.Customs.US.InBond));
				AssertNull(ControllerIDs.DocumentVisualizer.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var form = new JobDeclarationForm(declaration))
			{
				AssertNotNull(ControllerIDs.Customs.US.InBond.Name, form.PlugIns.GetPlugIn(ControllerIDs.Customs.US.InBond));
				AssertNotNull(ControllerIDs.DocumentVisualizer.Name, form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));
			}
		}

		public void TestWarningOnDeactivateRejectedExportEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.US_HazardousCargo = "Y";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.US_HazardousCargo = "N";
			invoice2.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.InvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(invoice => invoice == invoice1) != null);

			entry1.CH_Status = Enterprise.Customs.Common.US.AESDirectCustomsEntryStatus.Codes.Error;
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;

			using (var form = new JobDeclarationForm(declaration))
			{
				var continueWithSave = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, continueWithSave);
				invoice1.Delete();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				continueWithSave = form.FireSaveButton();
				AssertEquals(ContinueWithSave.No, continueWithSave);
				entry1.CH_Status = Enterprise.Customs.Common.US.AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				continueWithSave = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, continueWithSave);
			}
		}

		protected override JobDeclaration GetPopulatedDeclarationForFormBashingCore()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashingCore();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			header.BH_OverrideFreightDefaults = ZBool.True;
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return declaration;
		}

		public void TestAESPromptToSendWithdrawalMessage()
		{
			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "123456789";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = Factory.New<Freight.Forwarding.Business.ForwardingShipment>().PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.US_HazardousCargo = "Y";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry1 = declaration.ActiveEntryHeaders[0];
			entry1.CH_Status = Enterprise.Customs.Common.US.AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry1.US_IsDeactivated = true;
			declaration.Factory.Save();

			using (var form = new JobDeclarationForm(declaration))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var continueWithSave = form.FireSaveButton();
				AssertEquals(true, entry1.US_SendWithdrawn);
				AssertEquals(typeof(AESMessageSubmitForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public override void TestMinimumSizeNotTooBig()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var minScreenWidthSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1310);
			var minScreenHeightSupported = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(740);
			using (var form = new JobDeclarationForm(declaration))
			{
				// Do not extend the MinimumSize casually, only if you have tested this form on a small screen with size 1366*768, and make sure that nothing was covered. See WI00250245 for details.
				Assert("US Declaration Form min size too wide (" + form.MinimumSize.Width + ") for the screen. Should be less than or equal to " + minScreenWidthSupported, form.MinimumSize.Width <= minScreenWidthSupported);
				Assert("US Declaration Form min size too high (" + form.MinimumSize.Height + ") for the screen. Should be less than or equal to " + minScreenHeightSupported, form.MinimumSize.Height <= minScreenHeightSupported);
			}
		}

		OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignor, true);
					query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "AUSYD");
					supplier = Factory.LoadTop1<OrgHeader>(query);
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					ZQuery query = new ZQuery(OrgHeaderSchema.OH_IsConsignee, true);
					query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, "USLAX");
					importer = Factory.LoadTop1<OrgHeader>(query);
				}
				return importer;
			}
		}
		OrgHeader importer;

		protected override Customs.Business.BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			JobDeclaration usDeclaration = (JobDeclaration)base.CreateDeclarationForPerformanceTest(factory);
			usDeclaration.US_EnableAII = true;
			usDeclaration.US_EnableCRL = true;
			usDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			usDeclaration.JE_OH_Supplier = Supplier.PK;
			usDeclaration.JE_OH_Importer = Importer.PK;
			usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			usDeclaration.JE_RL_NKOrigin = "AUSYD";
			usDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			usDeclaration.JE_RL_NKPortOfArrival = "USLAX";
			usDeclaration.JE_RL_NKFinalDestination = "USLAX";
			usDeclaration.ContractNumbers.AddNew().CY_Data = "C1";
			CusContainer container = usDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TURE234232";
			Bill masterBill = usDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MWB12321";
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HWB23423";
			Bill subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "SHB23423";
			subHouseBill.US_7512OpenArea = "HELLO";
			subHouseBill.ReferenceNos.AddNew("MM", "DSDS");
			Package package = usDeclaration.Packages.AddNew();
			package.CW_HouseBill = subHouseBill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			return usDeclaration;
		}

		protected override Customs.Business.BaseJobComInvoiceHeader CreateInvoiceHeaderForPerformanceTest(Customs.Business.BaseJobDeclaration testDec)
		{
			JobComInvoiceHeader usInvoice = (JobComInvoiceHeader)base.CreateInvoiceHeaderForPerformanceTest(testDec);
			usInvoice.Charges.AddNew("OFT", 10m, "AUD");
			usInvoice.RelatedDocuments.AddNew(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber, "SHB23423");
			return usInvoice;
		}

		internal static TariffView CreateTariff(BusinessObjectFactory factory, ZGuid tariffTypePK, ZDateTime startDate, ZDateTime endDate, ZString tariffCode, ZString uq1, ZString description)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var result = helper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypePK, tariffCode, startDate, endDate, description: description, ensureDataGroupingExists: false);
			helper.CreateTariffUOM(result, Constants.UnitOfMeasureTypes.StatisticalUOMType, uq1, Core.Constants.CountryCodes.UnitedStates);
			return result;
		}

		ZString[] ScheduleBCodes
		{
			get
			{
				if (scheduleBCodes == null)
				{
					var factory = new BusinessObjectFactory();
					var helper = new UniversalReferenceTestDataHelper(factory);
					var shb = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
					factory.Save();
					var startDate = ZDateTime.Today.AddMonths(-1);
					var endDate = startDate.AddYears(1);
					var list = new List<TariffView>();
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0101210100", "NO", "HORSES, PUREBRED BREEDING, LIVE"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0201100110", "KG", "CARCASSES AND HALF-CARCASSES OF VEAL, FRESH OR CHI"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0301110100", "X", "ORNAMENTAL FRESHWATER FISH, LIVE"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0401100100", "L", "MILK AND CREAM, NOT CONCENTRATED, NOT SWEETENED, F"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0501000100", "KG", "HUMAN HAIR, UNWORKED, WHETHER OR NOT WASHED OR SCO"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0601100100", "NO", "BULBS, TUBERS, TUBEROUS ROOTS, CORMS, CROWNS AND R"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0701100100", "KG", "POTATOES, SEED, FRESH OR CHILLED"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0801110100", "KG", "COCONUTS, DESICCATED"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "0901110100", "KG", "COFFEE, NOT ROASTED, NOT DECAFFEINATED"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1001110100", "KG", "DURUM WHEAT SEED"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1101000100", "KG", "WHEAT OR MESLIN FLOUR"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1201900105", "KG", "SOYBEAN SEEDS OF A KIND USED AS OIL STOCK, WHETHER"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1301200100", "KG", "GUM ARABIC"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1401100100", "X", "BAMBOO, USED PRIMARILY FOR PLAITING"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1501200160", "KG", "YELLOW GREASE"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1601000110", "KG", "SAUSAGES AND SIMILAR PRODUCTS, OF POULTRY, OF CHIC"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1701913140", "KG", "CANE OR BEET SUGAR & CHEMICALLY PURE SUCROSE, REFI"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1801000100", "KG", "COCOA BEANS, WHOLE OR BROKEN, RAW OR ROASTED"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "1901100102", "KG", "PREPARATIONS FOR INFANT USE, PUT UP FOR RETAIL SAL"));
					list.Add(CreateTariff(factory, shb.PK, startDate, endDate, "2001100100", "KG", "CUCUMBERS INCLUDING GHERKINS, PREPARED OR PRESERVE"));
					factory.Save();
					scheduleBCodes = list.Select(x => x.ZZ1_TariffCode).ToArray();
				}
				return scheduleBCodes;
			}
		}
		ZString[] scheduleBCodes;

		ZString GetScheduleBCode(int index, BusinessObjectFactory factory)
		{
			return ScheduleBCodes[index % 20];
		}

		IEnumerable<ZGuid> TariffPKs
		{
			get
			{
				if (tariffPKs == null)
				{
					var tariffCodes = new ZString[] {
						"0101100010",
						"0201105010",
						"0301100000",
						"0401100000",
						"0501000000",
						"0601101500",
						"0701100020",
						"0801110000",
						"0901110010",
						"1001100010",
						"1101000010",
						"1201000020",
						"1301100020",
						"1401100000",
						"1501000020",
						"1601002010",
						"1701110500",
						"1801000000",
						"1901100500",
						"2001100000"
					};
					var query = new ZQuery(USCTariffSchema.UE_Tariff, tariffCodes);
					query.MaximumRows = 20;
					var tariffs = new BusinessObjectFactory().Load<USCTariff>(query);
					AssertEquals("There should be 20 USCTariffs loaded", 20, tariffs.Length);
					tariffPKs = tariffs.Select(x => x.PK);
				}
				return tariffPKs;
			}
		}
		IEnumerable<ZGuid> tariffPKs;

		ZString GetTariffCode(int index, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("TariffsForPerformanceTesting", () => factory.Load<USCTariff>(new ZQuery(USCTariffSchema.PK, TariffPKs)))[index % 20].UE_Tariff;
		}

		Business.OrgSupplierPart[] Parts
		{
			get
			{
				if (parts == null)
				{
					List<Business.OrgSupplierPart> list = new List<Business.OrgSupplierPart>(20);
					BusinessObjectFactory factory = new BusinessObjectFactory();
					for (int index = 1; index <= 20; index++)
					{
						Business.OrgSupplierPart usPart = factory.New<Business.OrgSupplierPart>();
						usPart.OP_PartNum = "PART" + index.ToString();
						usPart.RelatedOrganisations.AddOrganisationIfNotExist(Supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);

						OrgPartUnit partUnit = usPart.PartUnits.AddNew();
						partUnit.OF_QuantityInParent = 24m;
						partUnit.OF_ParentPackType = "CTN";
						ZString lookup = usPart.OP_PartNum + new Random().Next(1000000).ToString();

						CusClassification exportClassification = factory.New<CusClassification>();
						exportClassification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
						exportClassification.CC_LookupCode = lookup;
						exportClassification.CC_TariffNum = GetScheduleBCode(index, factory);
						CusClassification importClassification = factory.New<CusClassification>();
						importClassification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
						importClassification.CC_LookupCode = lookup;
						importClassification.CC_TariffNum = GetTariffCode(index, factory);

						CusClassPartPivot importPivot = usPart.PivotsForBinding.AddNew();
						importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
						importPivot.CI_CC = importClassification.PK;

						CusClassPartPivot schedulePivot = usPart.PivotsForBinding.AddNew();
						schedulePivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
						schedulePivot.CI_CC = exportClassification.PK;

						CusClassPartPivot exportPivot = usPart.PivotsForBinding.AddNew();
						exportPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
						exportPivot.CI_CC = exportClassification.PK;

						list.Add(usPart);
					}
					factory.Save();
					parts = list.ToArray();
				}
				return parts;
			}
		}
		Business.OrgSupplierPart[] parts;

		ZString GetPartNum(int index)
		{
			return Parts[index % Parts.Length].OP_PartNum;
		}

		protected override Customs.Business.BaseJobComInvoiceLine CreateInvoiceLineForPerformanceTest(Customs.Business.BaseJobComInvoiceHeader invoiceHeader, int index, int invoiceIndex)
		{
			JobComInvoiceHeader usInvoiceHeader = (JobComInvoiceHeader)invoiceHeader;
			usInvoiceHeader.JobDeclaration.US_CargoReleaseType = "ACS";
			JobComInvoiceLine usInvoiceLine = usInvoiceHeader.JobComInvoiceLines.AddNew();
			usInvoiceLine.US_TariffType = index % 2 == 1 ? TariffTypeList.Codes.HTS : TariffTypeList.Codes.ScheduleB;
			usInvoiceLine.JI_PartNo = GetPartNum(index);
			usInvoiceLine.ApportionedCharges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OtherCharges, 435m);
			usInvoiceLine.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 123m);
			usInvoiceLine.DOTs.AddNew().US_DOTCommercialDesc = "TEST";
			usInvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew().CY_Data = "B12LA12H";
			usInvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew().US_Tariff = "bl23ah";
			usInvoiceLine.DrawbackNAFTAs.AddNew().US_DRWNAFTACountryImportEntry = "1";
			usInvoiceLine.FCCs.AddNew().US_FCCTradeName = "TEST";
			FDA fda = usInvoiceLine.FDAs.AddNew();
			fda.US_TradeBrandName = "TEST";
			usInvoiceLine.FeeCusCodes.AddNew().CY_Code = "CC";
			usInvoiceLine.LineGroupingRanges.AddNew(1, 3);
			AIILine aiiLine = usInvoiceLine.FirstAIILine;
			aiiLine.RegoNumbers.AddNew(RegoNumberCodeList.Codes.ChassisNumber, "C111");
			usInvoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew().JI_ParentID = usInvoiceLine.PK;
			usInvoiceLine.US_CVD_NA = true;
			usInvoiceLine.US_ADD_NA = true;
			return usInvoiceLine;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}

	sealed class JobDeclarationFormForTest : JobDeclarationForm
	{
		public JobDeclarationFormForTest(JobDeclaration declaration)
			: base(declaration)
		{
		}

		internal ContinueWithSave ShowPreSaveDialogsInternal() => ShowPreSaveDialogs();
	}
}
