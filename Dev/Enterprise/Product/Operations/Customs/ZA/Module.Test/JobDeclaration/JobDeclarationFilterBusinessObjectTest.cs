using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ZAJobMessageTypeList = Enterprise.Customs.ZA.Business.ZAJobMessageTypeList;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : Customs.Module.Testing.JobDeclarationFilterBusinessObjectTest
	{
		public void TestCaseNumberSearch()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction11 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction11.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123300").Document_Status = DocumentStatusCodes.Codes.PND;
			var instruction12 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction12.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123400").Document_Status = DocumentStatusCodes.Codes.SNT;
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction21 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction21.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123500").Document_Status = DocumentStatusCodes.Codes.FAL;
			Factory.Save();
			CombineAssertions("Start with Partial, Multiple match", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "123";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2 }, filteredResult);
			});
			CombineAssertions("Start with Partial, single match 1", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "1234";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			});
			CombineAssertions("Start with Partial, single match 2", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "1235";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2 }, filteredResult);
			});
			CombineAssertions("Starts with empty", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2 }, filteredResult);
			});
			CombineAssertions("startwith something that does not exist", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "XXX";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredResult.Length);
			});
			CombineAssertions("equals", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CaseNumber];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "123400";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			});
		}

		public void TestSupportingDocumentStatusSearch()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction11 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction11.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123300").Document_Status = DocumentStatusCodes.Codes.PND;
			var instruction12 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction12.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123400").Document_Status = DocumentStatusCodes.Codes.SNT;
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction21 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction21.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123500").Document_Status = DocumentStatusCodes.Codes.FAL;
			var instruction22 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction22.CaseNumbers.AddNew(CaseNumberTypeList.Codes.DocumentInspectionCases, "123600").Document_Status = DocumentStatusCodes.Codes.SNT;
			Factory.Save();
			CombineAssertions("Match PND", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.StatusFilterTypes.SupportingDocumentStatus];
				filter.IsActive = true;
				filter.Property = "PND";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			});
			CombineAssertions("Match SNT", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.StatusFilterTypes.SupportingDocumentStatus];
				filter.IsActive = true;
				filter.Property = "SNT";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2 }, filteredResult);
			});
			CombineAssertions("Match FAL", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.StatusFilterTypes.SupportingDocumentStatus];
				filter.IsActive = true;
				filter.Property = "FAL";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2 }, filteredResult);
			});
		}

		public void TestVINSearch()
		{
			var dec = Factory.New<JobDeclaration>();
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_VIN = "34TEST343";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.VIN];
			filter.IsActive = true;
			filter.Property = "TEST";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
			filter.Property = "34TEST";
			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
			filter.Property = "34TEST343";
			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
		}

		public void TestSerialNoSearch()
		{
			const string BGMReference = "123";

			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.CustomsEntryHeaders.AddNew();
			header.CH_BGMReference = "123";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.SerialNumber];
			filter.IsActive = true;
			filter.Property = BGMReference;
			var filteredDecs = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertEquals(1, filteredDecs.Length);
			AssertEquals(BGMReference, filteredDecs[0].CustomsEntryHeaders[0].CH_BGMReference);
		}

		public void TestSerialNumberFilterUsesClusterKey()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.SerialNumber];
			filter.IsActive = true;
			filter.Property = "123";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("CH_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestEntryStatusFilterForReleaseStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.CustomsEntryHeaders.AddNew().CH_Status = "1";
			var filter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			filter.IsActive = true;
			filter.Property = "1";
			Factory.Save();
			AssertEquals(0, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
		}

		public void TestNotSentIsDifferentFromSearchingForAll()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration1.JE_TransportMode = "SEA";
			declaration1.JE_VoyageFlightNo = "xxxxx"; //a sign to distinguish/authenticate this declaration
			declaration1.JE_EntryStatus = ZAMessageStatusList.Codes.NotSent;
			declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			declaration2.JE_TransportMode = "SEA";
			declaration2.JE_VoyageFlightNo = "yyyyy"; //a sign to distinguish/authenticate this declaration
			declaration2.CustomsEntryHeaders.AddNew().CH_Status = ZAMessageStatusList.Codes.Error;
			declaration2.JE_EntryStatus = EntryStatusList.Codes.Error;
			declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
			var filterStatus = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			filterStatus.IsActive = true;
			filterStatus.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			var foundxxxxxDec = Factory.LoadTop1<JobDeclaration>(filterBO.Filter);
			AssertEquals("xxxxx", foundxxxxxDec.JE_VoyageFlightNo);
			filterStatus.Property = ""; //No filter must be made
			var filterVoyage = (ModuleTextAndNkFilter)filterBO[DeclarationFilterConstants.FlightVoyageVessel];
			filterVoyage.IsActive = true;
			filterVoyage.Property = declaration2.JE_VoyageFlightNo;
			AssertEquals("Filter incorrectly contains JE_EntryStatus", -1, filterBO.Filter.LiteralTextADO.IndexOf("JE_EntryStatus"));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(JobDeclaration), filterBO.Filter));
		}

		public void TestPreviousMRNSearch()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction11 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction11.CEI_PreviousMRN = "123300";
			var instruction12 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction12.CEI_PreviousMRN = "123400";
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction21 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction21.CEI_PreviousMRN = "123500";
			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction31 = declaration3.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction31.CEI_PreviousMRN = "223500";
			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction41 = declaration4.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var declaration5 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			CombineAssertions("Start with Partial, Multiple match", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "123";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2 }, filteredResult);
			});
			CombineAssertions("Start with Partial, single match 1", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "1234";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			});
			CombineAssertions("Start with Partial, single match 2", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "1235";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2 }, filteredResult);
			});
			CombineAssertions("Starts with empty", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2, declaration3, declaration4, declaration5 }, filteredResult);
			});
			CombineAssertions("startwith something doesnot exist", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
				filter.Property = "XXX";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredResult.Length);
			});
			CombineAssertions("Equals Empty won't apply filter", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2, declaration3, declaration4, declaration5 }, filteredResult);
			});
			CombineAssertions("Equals won't match on partial", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "1234";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobDeclaration>(), filteredResult);
			});
			CombineAssertions("Equals strong pointing", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "123400";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			});
			CombineAssertions("Equals something doesnot exist", () =>
			{
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
				filter.IsActive = true;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.Property = "XXX";
				var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
				AssertEquals(0, filteredResult.Length);
			});
		}

		public void TestGetPreviousMRNQueryUsesClusterKey()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PreviousMRN];
			filter.IsActive = true;
			filter.Property = "93601";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("CEI_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestProcedureCodesSearch()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>(); // AA/XX
			var instruction11 = declaration1.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction11.CEI_Style = "AA";
			var invHeader11 = declaration1.Invoices.AddNew();
			var invLine111 = invHeader11.InvoiceLines.AddNew();
			invLine111.JI_Procedure = invLine111.EntryInstruction.CEI_Style + "XX";
			invLine111.JI_CEI = instruction11.PK;
			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>(); // AA/XX but not linking
			var instruction21 = declaration2.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction21.CEI_Style = "AA";
			var invHeader21 = declaration2.Invoices.AddNew();
			var invLine211 = invHeader21.InvoiceLines.AddNew();
			invLine211.JI_CEI = ZGuid.Empty;
			invLine211.JI_Procedure = "00XX";
			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>(); // BB/YY 
			var instruction31 = declaration3.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction31.CEI_Style = "BB";
			var invHeader31 = declaration3.Invoices.AddNew();
			var invLine311 = invHeader31.InvoiceLines.AddNew();
			invLine311.JI_CEI = instruction31.PK;
			invLine311.JI_Procedure = invLine311.EntryInstruction.CEI_Style + "YY";
			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>(); // AA/YY 
			var instruction41 = declaration4.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction41.CEI_Style = "AA";
			var invHeader41 = declaration4.Invoices.AddNew();
			var invLine411 = invHeader41.InvoiceLines.AddNew();
			invLine411.JI_CEI = instruction41.PK;
			invLine411.JI_Procedure = invLine411.EntryInstruction.CEI_Style + "YY";
			var declaration5 = Factory.NewWithValidTestData<JobDeclaration>(); // BB/XX 
			var instruction51 = declaration5.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction51.CEI_Style = "BB";
			var invHeader51 = declaration5.Invoices.AddNew();
			var invLine511 = invHeader51.InvoiceLines.AddNew();
			invLine511.JI_CEI = instruction51.PK;
			invLine511.JI_Procedure = invLine511.EntryInstruction.CEI_Style + "XX";
			var declaration6 = Factory.NewWithValidTestData<JobDeclaration>(); // AA Only 
			var instruction61 = declaration6.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction61.CEI_Style = "AA";
			var declaration7 = Factory.NewWithValidTestData<JobDeclaration>(); // XX Only 
			var invHeader71 = declaration7.Invoices.AddNew();
			var invLine711 = invHeader71.InvoiceLines.AddNew();
			invLine711.JI_Procedure = "00XX";
			var declaration8 = Factory.NewWithValidTestData<JobDeclaration>(); // Covers All Above cases
			var instruction81 = declaration8.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction81.CEI_Style = "AA";
			instruction81.CEI_Description = "AA1";
			var instruction82 = declaration8.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction82.CEI_Style = "AA";
			instruction82.CEI_Description = "AA2";
			var instruction83 = declaration8.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction83.CEI_Style = "BB";
			var invHeader81 = declaration8.Invoices.AddNew();
			var invLine811 = invHeader81.InvoiceLines.AddNew();
			invLine811.JI_CEI = instruction81.PK;
			invLine811.JI_Procedure = invLine811.EntryInstruction.CEI_Style + "XX";
			var invLine812 = invHeader81.InvoiceLines.AddNew();
			invLine812.JI_CEI = instruction82.PK;
			invLine812.JI_Procedure = invLine812.EntryInstruction.CEI_Style + "YY";
			var invLine813 = invHeader81.InvoiceLines.AddNew();
			invLine813.JI_CEI = instruction83.PK;
			invLine813.JI_Procedure = invLine813.EntryInstruction.CEI_Style + "XX";
			var invLine814 = invHeader81.InvoiceLines.AddNew();
			invLine814.JI_CEI = instruction83.PK;
			invLine814.JI_Procedure = invLine814.EntryInstruction.CEI_Style + "YY";
			var invLine815 = invHeader81.InvoiceLines.AddNew();
			invLine815.JI_CEI = ZGuid.Empty;
			invLine815.JI_Procedure = "00XX";
			var invLine816 = invHeader81.InvoiceLines.AddNew();
			invLine816.JI_CEI = ZGuid.Empty;
			invLine816.JI_Procedure = "00YY";
			declaration1.JE_DeclarationReference = "Declaration1";
			declaration2.JE_DeclarationReference = "Declaration2";
			declaration3.JE_DeclarationReference = "Declaration3";
			declaration4.JE_DeclarationReference = "Declaration4";
			declaration5.JE_DeclarationReference = "Declaration5";
			declaration6.JE_DeclarationReference = "Declaration6";
			declaration7.JE_DeclarationReference = "Declaration7";
			declaration8.JE_DeclarationReference = "Declaration8";
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertCPCPPCSearchResult("AA__", "AA", "", new JobDeclaration[] { declaration1, declaration2, declaration4, declaration6, declaration8 });
				AssertCPCPPCSearchResult("AAXX", "AA", "XX", new JobDeclaration[] { declaration1, declaration8 });
				AssertCPCPPCSearchResult("AAYY", "AA", "YY", new JobDeclaration[] { declaration4, declaration8 });
				AssertCPCPPCSearchResult("AAZZ", "AA", "ZZ", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("BB__", "BB", "", new JobDeclaration[] { declaration3, declaration5, declaration8 });
				AssertCPCPPCSearchResult("BBXX", "BB", "XX", new JobDeclaration[] { declaration5, declaration8 });
				AssertCPCPPCSearchResult("BBYY", "BB", "YY", new JobDeclaration[] { declaration3, declaration8 });
				AssertCPCPPCSearchResult("BBZZ", "BB", "ZZ", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("CC__", "CC", "", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("CCXX", "CC", "XX", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("CCYY", "CC", "YY", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("CCZZ", "CC", "ZZ", System.Array.Empty<JobDeclaration>());
				AssertCPCPPCSearchResult("____", "", "", new JobDeclaration[] { declaration1, declaration2, declaration3, declaration4, declaration5, declaration6, declaration7, declaration8 });
				AssertCPCPPCSearchResult("__XX", "", "XX", new JobDeclaration[] { declaration1, declaration2, declaration5, declaration7, declaration8 });
				AssertCPCPPCSearchResult("__YY", "", "YY", new JobDeclaration[] { declaration3, declaration4, declaration8 });
				AssertCPCPPCSearchResult("__ZZ", "", "ZZ", System.Array.Empty<JobDeclaration>());
			});
		}

		void AssertCPCPPCSearchResult(string description, string cpc, string ppc, JobDeclaration[] expected)
		{
			var filter = (ProcedureCodesModuleFilter)filterBO[DeclarationFilterConstants.CPCAndPPC];
			filter.IsActive = true;
			filter.Property1 = cpc;
			filter.Property2 = ppc;
			var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(description + filterBO.Filter.LiteralTextSqlFormatted, expected.Select(x => x.JE_DeclarationReference).ToArray(), filteredResult.Select(x => x.JE_DeclarationReference).ToArray());
		}

		public void TestEntryNumberUCRFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration1.CustomsEntryHeaders.AddNew();
			var cusEntryNumber1 = Factory.New<CusEntryNumber>();
			cusEntryNumber1.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber1.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber1.CE_EntryNum = "test1";
			var cusEntryNumber2 = Factory.New<CusEntryNumber>();
			cusEntryNumber2.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber2.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber2.CE_EntryNum = "test2";
			var cusEntryNumber3 = Factory.New<CusEntryNumber>();
			cusEntryNumber3.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber3.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber3.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber3.CE_EntryNum = "test3";
			var cusEntryNumber4 = Factory.New<CusEntryNumber>();
			cusEntryNumber4.CE_ParentID = declaration1.PK;
			cusEntryNumber4.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber4.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber4.CE_EntryNum = "test4";
			var cusEntryNumber5 = Factory.New<CusEntryNumber>();
			cusEntryNumber5.CE_ParentID = declaration1.PK;
			cusEntryNumber5.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber5.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber5.CE_EntryNum = "test5";
			cusEntryHeader = declaration1.CustomsEntryHeaders.AddNew();
			var cusEntryNumber6 = Factory.New<CusEntryNumber>();
			cusEntryNumber6.CE_ParentID = cusEntryHeader.PK;
			cusEntryNumber6.CE_ParentTable = CusEntryHeader.Schema.TableName;
			cusEntryNumber6.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber6.CE_EntryNum = "test6";
			var declaration2 = Factory.New<JobDeclaration>();
			var cusEntryNumber7 = Factory.New<CusEntryNumber>();
			cusEntryNumber7.CE_ParentID = declaration2.PK;
			cusEntryNumber7.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber7.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber7.CE_EntryNum = "test7";
			var cusEntryNumber8 = Factory.New<CusEntryNumber>();
			cusEntryNumber8.CE_ParentID = declaration2.PK;
			cusEntryNumber8.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber8.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber8.CE_EntryNum = "test8";
			var declaration3 = Factory.New<JobDeclaration>();
			var cusEntryNumber9 = Factory.New<CusEntryNumber>();
			cusEntryNumber9.CE_ParentID = declaration3.PK;
			cusEntryNumber9.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber9.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			cusEntryNumber9.CE_EntryNum = "test9";
			var declaration4 = Factory.New<JobDeclaration>();
			var cusEntryNumber10 = Factory.New<CusEntryNumber>();
			cusEntryNumber10.CE_ParentID = declaration4.PK;
			cusEntryNumber10.CE_ParentTable = JobDeclaration.Schema.TableName;
			cusEntryNumber10.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			cusEntryNumber10.CE_EntryNum = "test10";
			Factory.Save();
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.UniqueConsignmentReference];
			filter.Property = "test1";
			filter.IsActive = true;
			var filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration4 }, filteredResult);
			filter.Property = "test2";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			filter.Property = "test3";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobDeclaration>(), filteredResult);
			filter.Property = "test4";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			filter.Property = "test5";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobDeclaration>(), filteredResult);
			filter.Property = "test6";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1 }, filteredResult);
			filter.Property = "test7";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2 }, filteredResult);
			filter.Property = "test8";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobDeclaration>(), filteredResult);
			filter.Property = "test9";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<JobDeclaration>(), filteredResult);
			filter.Property = "test10";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration4 }, filteredResult);
			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "test1";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2, declaration3 }, filteredResult);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "test1";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2, declaration3 }, filteredResult);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "test1";
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration2, declaration3, declaration4 }, filteredResult);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration3 }, filteredResult);
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			filter.IsActive = true;
			filteredResult = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new JobDeclaration[] { declaration1, declaration2, declaration4 }, filteredResult);
		}

		public void TestRelPrintIndFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_RelPrintInd = "";
			var declaration2 = Factory.New<JobDeclaration>();
			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_RelPrintInd = "Y";
			var declaration3 = Factory.New<JobDeclaration>();
			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_RelPrintInd = "N";
			var declaration4 = Factory.New<JobDeclaration>();
			var entry4_1 = declaration4.CustomsEntryHeaders.AddNew();
			entry4_1.CH_RelPrintInd = "";
			var entry4_2 = declaration4.CustomsEntryHeaders.AddNew();
			entry4_2.CH_RelPrintInd = "Y";
			var entry4_3 = declaration4.CustomsEntryHeaders.AddNew();
			entry4_3.CH_RelPrintInd = "N";
			Factory.Save();
			var filterObj = new JobDeclarationFilterBusinessObject();
			var printFilter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator];
			printFilter.Property = "";
			printFilter.IsActive = true;
			Assert(declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3.MatchesFilter(filterObj.Filter));
			Assert(declaration4.MatchesFilter(filterObj.Filter));
			printFilter.Property = "Y";
			printFilter.IsActive = true;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(declaration2.MatchesFilter(filterObj.Filter));
			Assert(!declaration3.MatchesFilter(filterObj.Filter));
			Assert(declaration4.MatchesFilter(filterObj.Filter));
			printFilter.Property = "N";
			printFilter.IsActive = true;
			Assert(!declaration1.MatchesFilter(filterObj.Filter));
			Assert(!declaration2.MatchesFilter(filterObj.Filter));
			Assert(declaration3.MatchesFilter(filterObj.Filter));
			Assert(declaration4.MatchesFilter(filterObj.Filter));
		}

		public void TestGetRelPrintQueryUsesClusterKey()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.StatusFilterTypes.ReleasePrinterIndicator];
			filter.IsActive = true;
			filter.Property = "Y";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("CH_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestEntryInstructionCaseSubGroupGetSubQueryUsesClusterKey()
		{
			var entryInstructionCaseSubGroup = new JobDeclarationFilterBusinessObject.EntryInstructionCaseSubGroup();
			var subQuery = entryInstructionCaseSubGroup.GetSubQuery(new ZQuery());
			AssertContains("JE_ClusterKey", subQuery.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("CEI_ClusterKey", subQuery.LiteralTextSqlFormatted, ignoreCase: true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new JobDeclarationFilterBusinessObject();
		}
	}
}
