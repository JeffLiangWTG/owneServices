using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.Customs.US.GUI.Testing
{
	class JobDeclarationFormPerformanceTest : Customs.GUI.Testing.JobDeclarationFormPerformanceAbstractTest
	{
		Dictionary<string, int> USBaseLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 22 },
			{ CusCodeDataSchema.Constants.TableName, 9 },
			{ OrgAddressSchema.Constants.TableName, 6 },
		};

		Dictionary<string, int> USBaseValidateAllExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 22 },
			{ CusCodeDataSchema.Constants.TableName, 9 },
			{ CusDispositionSchema.Constants.TableName, 16 },
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 10 },
			{ OrgHeaderSchema.Constants.TableName, 6 },
			{ ZZRefCusCodeListCombinedSchema.Constants.TableName, 11 }
		};

		Dictionary<string, int> USBaseFormMergeExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 21 },
			{ CusCodeDataSchema.Constants.TableName, 9 },
			{ OrgAddressSchema.Constants.TableName, 6 },
			{ GenPivotSchema.Constants.TableName, 7 },
		};

		Dictionary<string, int> USBaseLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ JobDocAddressSchema.Constants.TableName, 6 },
			{ OrgHeaderSchema.Constants.TableName, 5 },
			{ OrgContainerDetentionSchema.Constants.TableName, 8 },
		};

		Dictionary<string, int> USBaseUniversalXMLExportExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 58 },
			{ CusCodeDataSchema.Constants.TableName, 61 },
			{ GenPivotSchema.Constants.TableName, 46 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 5 },
			{ OrgAddressSchema.Constants.TableName, 6 },
		};

		Dictionary<string, int> USBaseUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 23 },
			{ CusCodeDataSchema.Constants.TableName, 9 },
			{ EDIMessageSchema.Constants.TableName, 7 },
			{ GenPivotSchema.Constants.TableName, 8 },
			{ OrgAddressSchema.Constants.TableName, 15 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ OrgHeaderSchema.Constants.TableName, 12 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ StmDocDataOverrideSchema.Constants.TableName, 44 },
			{ StmNoteSchema.Constants.TableName, 72 },
			{ JobDocAddressSchema.Constants.TableName, 9 },
			{ ProcessTasksSchema.Constants.TableName, 5 },
			{ OrgContainerDetentionSchema.Constants.TableName, 5 },
			{ JobUSComInvoiceLineSchema.Constants.TableName, 60 },
			{ TariffViewSchema.Constants.TableName, 6 }
		};

		Dictionary<string, int> USBaseUniversalXMLAddExpectedHits => new Dictionary<string, int>
		{
			{ OrgAddressCapabilitySchema.Constants.TableName, 7 },
			{ OrgCusCodeSchema.Constants.TableName, 7 },
			{ OrgSupplierBuyerLinkSchema.Constants.TableName, 7 },
			{ OrgContainerDetentionSchema.Constants.TableName, 5 },
			{ TariffViewSchema.Constants.TableName, 6 }
		};

		Dictionary<string, int> USBaseDeleteExpectedHits => new Dictionary<string, int>
		{
			{ CusAddInfoSchema.Constants.TableName, 16 },
			{ CusCodeDataSchema.Constants.TableName, 8 },
			{ GenPivotSchema.Constants.TableName, 8 },
			{ JobDocAddressSchema.Constants.TableName, 10 },
			{ StmDocDataOverrideSchema.Constants.TableName, 38 },
			{ StmNoteSchema.Constants.TableName, 39 },
			{ OrgAddressSchema.Constants.TableName, 7 },
			{ CusDispositionSchema.Constants.TableName, 14 }
		};

		protected virtual Dictionary<string, int> USLoadEditableChildObjectsExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USValidateAllExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USLightFormValidationAndSaveExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USFormMergeExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USUniversalXMLExportExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USUniversalXMLImportUpdateExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USUniversalXMLAddExpectedHits => new Dictionary<string, int>();
		protected virtual Dictionary<string, int> USDeleteExpectedHits => new Dictionary<string, int>();

		protected override Dictionary<string, int> LoadEditableChildObjectsExpectedHits => ZipDictionaries(USBaseLoadEditableChildObjectsExpectedHits, USLoadEditableChildObjectsExpectedHits);
		protected override Dictionary<string, int> ValidateAllExpectedHits => ZipDictionaries(USBaseValidateAllExpectedHits, USValidateAllExpectedHits);
		protected override Dictionary<string, int> LightFormValidationAndSaveExpectedHits => ZipDictionaries(USBaseLightFormValidationAndSaveExpectedHits, USLightFormValidationAndSaveExpectedHits);
		protected override Dictionary<string, int> FormMergeExpectedHits => ZipDictionaries(USBaseFormMergeExpectedHits, USFormMergeExpectedHits);
		protected override Dictionary<string, int> UniversalXMLExportExpectedHits => ZipDictionaries(USBaseUniversalXMLExportExpectedHits, USUniversalXMLExportExpectedHits);
		protected override Dictionary<string, int> UniversalXMLImportUpdateExpectedHits => ZipDictionaries(USBaseUniversalXMLImportUpdateExpectedHits, USUniversalXMLImportUpdateExpectedHits);
		protected override Dictionary<string, int> UniversalXMLAddExpectedHits => ZipDictionaries(USBaseUniversalXMLAddExpectedHits, USUniversalXMLAddExpectedHits);
		protected override Dictionary<string, int> DeleteExpectedHits => ZipDictionaries(USBaseDeleteExpectedHits, USDeleteExpectedHits);

		protected override void DecorateDeclaration(Customs.Business.BaseJobDeclaration declaration)
		{
			base.DecorateDeclaration(declaration);
			var usDeclaration = (JobDeclaration)declaration;
			usDeclaration.US_EnableAII = true;
			usDeclaration.US_EnableCRL = true;
			usDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			usDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			usDeclaration.Supplier.OH_RL_NKClosestPort = "AUSYD";
			usDeclaration.Importer.OH_RL_NKClosestPort = "USLAX";
			usDeclaration.US_EnableENS = false;
			usDeclaration.JE_RL_NKOrigin = "AUSYD";
			usDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			usDeclaration.JE_RL_NKPortOfArrival = "USLAX";
			usDeclaration.JE_RL_NKFinalDestination = "USLAX";
			usDeclaration.ContractNumbers.AddNew().CY_Data = "C1";

			var container = usDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "TURE234232";

			var masterBill = usDeclaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MWB12321";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillNum = "HWB23423";

			var subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "SHB23423";
			subHouseBill.US_7512OpenArea = "HELLO";
			subHouseBill.ReferenceNos.AddNew("MM", "DSDS");

			var package = usDeclaration.Packages.AddNew();
			package.CW_HouseBill = subHouseBill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
		}

		protected override void DecorateInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoice)
		{
			base.DecorateInvoiceHeader(invoice);
			var usInvoice = (JobComInvoiceHeader)invoice;
			usInvoice.Charges.AddNew("OFT", 10m, "AUD");
			usInvoice.RelatedDocuments.AddNew(RelatedDocumentIdentifierList.Codes.HouseBillOfLadingNumber, "SHB23423");
		}

		protected override void DecorateInvoiceLine(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.DecorateInvoiceLine(invoiceLine);
			var usInvoiceLine = (JobComInvoiceLine)invoiceLine;

			usInvoiceLine.ApportionedCharges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 435m);
			usInvoiceLine.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 123m);
			usInvoiceLine.DOTs.AddNew().US_DOTCommercialDesc = "TEST";
			usInvoiceLine.DrawbackAdditionalExportTariffNumbers.AddNew().CY_Data = "B12LA12H";
			usInvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew().US_Tariff = "bl23ah";
			usInvoiceLine.DrawbackNAFTAs.AddNew().US_DRWNAFTACountryImportEntry = "1";
			usInvoiceLine.FCCs.AddNew().US_FCCTradeName = "TEST";

			var fda = usInvoiceLine.FDAs.AddNew();
			fda.US_TradeBrandName = "TEST";
			usInvoiceLine.FeeCusCodes.AddNew().CY_Code = "CC";
			usInvoiceLine.LineGroupingRanges.AddNew(1, 3);
			usInvoiceLine.US_CVD_NA = true;
			usInvoiceLine.US_ADD_NA = true;
		}

		ZString[] ScheduleBCodes
		{
			get
			{
				if (scheduleBCodes == null)
				{
					var factory = new BusinessObjectFactory();
					var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
					var shb = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.ScheduleB);
					factory.Save();
					var startDate = ZDateTime.Today.AddMonths(-1);
					var endDate = startDate.AddYears(1);
					var list = new List<TariffView>();
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0101210100", "NO", "HORSES, PUREBRED BREEDING, LIVE"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0201100110", "KG", "CARCASSES AND HALF-CARCASSES OF VEAL, FRESH OR CHI"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0301110100", "X", "ORNAMENTAL FRESHWATER FISH, LIVE"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0401100100", "L", "MILK AND CREAM, NOT CONCENTRATED, NOT SWEETENED, F"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0501000100", "KG", "HUMAN HAIR, UNWORKED, WHETHER OR NOT WASHED OR SCO"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0601100100", "NO", "BULBS, TUBERS, TUBEROUS ROOTS, CORMS, CROWNS AND R"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0701100100", "KG", "POTATOES, SEED, FRESH OR CHILLED"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0801110100", "KG", "COCONUTS, DESICCATED"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "0901110100", "KG", "COFFEE, NOT ROASTED, NOT DECAFFEINATED"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1001110100", "KG", "DURUM WHEAT SEED"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1101000100", "KG", "WHEAT OR MESLIN FLOUR"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1201900105", "KG", "SOYBEAN SEEDS OF A KIND USED AS OIL STOCK, WHETHER"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1301200100", "KG", "GUM ARABIC"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1401100100", "X", "BAMBOO, USED PRIMARILY FOR PLAITING"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1501200160", "KG", "YELLOW GREASE"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1601000110", "KG", "SAUSAGES AND SIMILAR PRODUCTS, OF POULTRY, OF CHIC"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1701913140", "KG", "CANE OR BEET SUGAR & CHEMICALLY PURE SUCROSE, REFI"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1801000100", "KG", "COCOA BEANS, WHOLE OR BROKEN, RAW OR ROASTED"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "1901100102", "KG", "PREPARATIONS FOR INFANT USE, PUT UP FOR RETAIL SAL"));
					list.Add(JobDeclarationFormAbstractTest.CreateTariff(factory, shb.PK, startDate, endDate, "2001100100", "KG", "CUCUMBERS INCLUDING GHERKINS, PREPARED OR PRESERVE"));
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

		ZGuid[] TariffPKs
		{
			get
			{
				if (tariffPKs == null)
				{
					ZString[] tariffCodes = new ZString[] {
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
					BusinessObjectFactory factory = new BusinessObjectFactory();
					List<USCTariff> list = new List<USCTariff>(20);
					foreach (ZString tariffCode in tariffCodes)
					{
						USCTariff tariff = factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode));
						if (tariff != null)
						{
							list.Add(tariff);
						}
					}
					var tariffs = list.ToArray();
					AssertEquals("There should be 20 USCTariffs loaded", 20, tariffs.Length);
					List<ZGuid> result = new List<ZGuid>(20);
					foreach (USCTariff tariff in tariffs)
					{
						result.Add(tariff.PK);
					}
					tariffPKs = result.ToArray();
				}
				return tariffPKs;
			}
		}

		ZGuid[] tariffPKs;

		ZString GetTariffCode(int index, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("TariffsForPerformanceTesting", () => factory.Load<USCTariff>(new ZQuery(USCTariffSchema.PK, TariffPKs)))[index % 20].UE_Tariff;
		}

		protected override void DecoratePart(OrgSupplierPart part)
		{
			base.DecoratePart(part);
			var factory = part.Factory;
			var usPart = (Business.OrgSupplierPart)part;

			var index = ZInt.ParseSafe(part.OP_PartNum.KeepNumericCharacters(), ZInt.Zero);
			var lookup = usPart.OP_PartNum + new Random().Next(1000000).ToString();

			var exportClassification = factory.New<CusClassification>();
			exportClassification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			exportClassification.CC_LookupCode = lookup;
			exportClassification.CC_TariffNum = GetScheduleBCode(index, factory);

			var importClassification = factory.New<CusClassification>();
			importClassification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			importClassification.CC_LookupCode = lookup;
			importClassification.CC_TariffNum = GetTariffCode(index, factory);

			var importPivot = usPart.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = importClassification.PK;

			var exportPivot = usPart.PivotsForBinding.AddNew();
			exportPivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			exportPivot.CI_CC = importClassification.PK;
		}

		protected override void Merge(Customs.Business.BaseJobDeclaration declaration)
		{
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		}

		//protected override int MaximumDBHitsForConstruction
		/*
CreateFullyPopulatedObject - Maximum hits : 150, Actual hits : 150
JobDeclaration: 1 selects
GlbBranch: 5 selects
GlbCompany: 1 selects
RefCountry: 2 selects
RatingHeader: 1 selects
RefUNLOCO: 4 selects
RefCountryStates: 2 selects
AccBankAccount: 8 selects
RefCurrency: 2 selects
OrgSupplierBuyerLink: 5 selects
RefLocoMap: 3 selects
Odyssey_RefDb_Ent_US.dbo.USCForeignPort: 1 selects
Odyssey_RefDb_Ent_US.dbo.USCRegionDistrictPort: 1 selects
Odyssey_RefDb_Ent_US.dbo.USCCountry: 2 selects
RefExchangeRate: 1 selects
Odyssey_RefDb_Ent_US.dbo.USCScheduleB: 2 selects
Odyssey_RefDb_Ent_US.dbo.USCTariff: 9 selects
OrgSupplierPart: 14 selects
RefPackType: 1 selects
CusClassification: 42 selects
Odyssey_RefDb_Ent_US.dbo.USCTariffDutyRate: 6 selects
RefPacks: 6 selects
GenAddOnColumn: 1 selects
Odyssey_RefDb_Ent_US.dbo.USCTariffValue: 6 selects
Odyssey_RefDb_Ent_US.dbo.USCTariffQuantity: 6 selects
Odyssey_RefDb_Ent_US.dbo.USCTariffRule: 6 selects
Odyssey_RefDb_Ent_US.dbo.USCTariffDateRestriction: 6 selects
StmEvent: 1 selects
ProcessTasks: 1 selects
	*/

		//protected override int MaximumDBHitsForFormOpen
		/*
Form Show - Maximum ms : 1500, Actual ms : 920
Form Show - Scaled Maximum ms : 00:00:01.1220000, Scaled Actual ms : 00:00:01.6160000
Form Show - Maximum hits : 31, Actual hits : 31
JobDeclaration: 1 selects
JobDocsAndCartage: 1 selects
JobConsolTransport: 1 selects
CusEntryHeader: 1 selects
EDIMessage: 2 selects
JobComInvoiceHeader: 1 selects
GlbBranch: 2 selects
GlbCompany: 1 selects
RefCountry: 1 selects
JobCartage: 2 selects
CusEntryNum: 1 selects
JobService: 1 selects
StmData: 2 selects
OrgHeader: 1 selects
OrgMiscServ: 1 selects
OrgAddress: 4 selects
OrgAddressCapability: 2 selects
OrgWebURL: 2 selects
StmNote: 1 selects
CusContainer: 1 selects
JobDocAddress: 1 selects
JobHeader: 1 selects
GenCustomColumnDefinition: 1 selects
GenCustomAddOnValue: 1 selects
		*/

		protected override void SetUp()
		{
			base.SetUp();
			ZArchitecture.Core.Testing.FountainTestListener.Instance.AfterEachTest(ZDateTime.UtcNow.ToDateTime());
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
		}
	}
}
