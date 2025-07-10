using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ProofOfPaymentFilterBusinessObject))]
	sealed class ProofOfPaymentFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilterConstants()
		{
			var filter = new ProofOfPaymentFilterBusinessObject();
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.Importer]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.JobNumber]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.FinancialAccountNumber]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.TransactionDate]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.ReceiptDate]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.CustomsOffice]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.LocalReferenceNumber]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.MRN]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.VATAmount]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.ReceiptNumber]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.TotalVATforReceipt]);
			AssertNotNull(filter[ProofOfPaymentFilterBusinessObject.FilterConstants.AgentsReference]);
		}

		public void TestVATAmountSearch()
		{
			CreateJobDeclarationWithPayInfo(0m);
			CreateJobDeclarationWithPayInfo(1m);
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleNumberRangeSubFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.VATAmount];
			AssertNotNull("VAT Amount Filter exists", filter);
			filter.IsActive = true;
			filter.PropertySearch = "Greater than";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals(1m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestAgentsReferenceSearch()
		{
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			declaration1.JE_AgentsReference = "Agent007";
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			declaration2.JE_AgentsReference = "Agent86";
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.AgentsReference];
			AssertNotNull("Agents Reference Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "Agent007";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("Agent007", filteredPayInfo.AgentsReference);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestAgentsReferenceQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.AgentsReference];
			filter.IsActive = true;
			filter.Property = "Agent007";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestImporterQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleGuidFilter filter = (ModuleGuidFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.Importer];
			filter.IsActive = true;
			filter.Property = ZGuid.BrettsGuid;
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestJobNumberSearch()
		{
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			declaration1.JE_DeclarationReference = "BCCL123";
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			declaration2.JE_DeclarationReference = "AAA";
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.JobNumber];
			AssertNotNull("Job Number Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "BCCL123";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("BCCL123", filteredPayInfo.JobNumber);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestJobQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.JobNumber];
			filter.IsActive = true;
			filter.Property = "BCCL123";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestFANQuerySearch()
		{
			var testHelper = new Business.Testing.ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("DFM");
			testHelper.CreateCustomsOfficeCusCodeEntry("JBG");
			Factory.Save();
			var orgheader = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			orgheader.OH_FullName = "CCLFFRPAR";
			orgheader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.CodeTypes.AgentCode, "11223344", Core.Constants.CountryCodes.SouthAfrica);
			var orgheader2 = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();
			orgheader2.OH_FullName = "GLECOR_ZA";
			orgheader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(MasterFiles.Business.OrgCusCode.CodeTypes.AgentCode, "22334455", Core.Constants.CountryCodes.SouthAfrica);
			var maps = new FinancialAccountNumberPortMapCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var mapping1 = maps.AddNew();
			mapping1.AccountStartDay = 5;
			mapping1.ImporterPays = true;
			mapping1.OrganizationPK = orgheader.PK;
			mapping1.CustomsOfficeCode = "DFM";
			mapping1.FinancialAccountNumber = "1111111111";
			var mapping2 = maps.AddNew();
			mapping2.AccountStartDay = 5;
			mapping2.ImporterPays = true;
			mapping2.OrganizationPK = orgheader2.PK;
			mapping2.CustomsOfficeCode = "JBG";
			mapping2.FinancialAccountNumber = "2222222222";
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, maps);
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			declaration1.JE_OA_DeclarantAddress = orgheader.MainAddress.PK;
			declaration1.JE_CustomsOffice = "DFM";
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			declaration2.JE_OA_DeclarantAddress = orgheader2.MainAddress.PK;
			declaration2.JE_CustomsOffice = "JBG";
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.FinancialAccountNumber];
			AssertNotNull("Financial Acount Number Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "1111111111";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("1111111111", filteredPayInfo.FANumber);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestCustomsOfficeSearch()
		{
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			declaration1.JE_CustomsOffice = "USA";
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			declaration2.JE_CustomsOffice = "JBG";
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.CustomsOffice];
			AssertNotNull("Customs Office Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "USA";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("USA", filteredPayInfo.CustomsOffice);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestCustomsOfficeQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.CustomsOffice];
			filter.IsActive = true;
			filter.Property = "Y";
			AssertContains("JE_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestLocalReferenceNumberSearch()
		{
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			declaration1.ActiveEntryHeaders[0].CH_BGMReference = "00505655JSA";
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			declaration2.ActiveEntryHeaders[0].CH_BGMReference = "9876543";
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.LocalReferenceNumber];
			AssertNotNull("Local Reference Number Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "00505655JSA";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("00505655JSA", filteredPayInfo.LRNumber);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestLocalReferenceNumberQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.LocalReferenceNumber];
			filter.IsActive = true;
			filter.Property = "Y";
			AssertContains("CH_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		public void TestMRNSearch()
		{
			var declaration1 = CreateJobDeclarationWithPayInfo(23.47m);
			var header1 = declaration1.ActiveEntryHeaders[0];
			var entryNumber1 = Factory.New<CusEntryNumber>();
			entryNumber1.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber1.CE_EntryNum = "AFR1";
			entryNumber1.CE_ParentID = header1.PK;
			entryNumber1.CE_ParentTable = header1.TableName;
			var declaration2 = CreateJobDeclarationWithPayInfo(500m);
			var header2 = declaration2.ActiveEntryHeaders[0];
			var entryNumber2 = Factory.New<CusEntryNumber>();
			entryNumber2.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
			entryNumber2.CE_EntryNum = "22ABCD";
			entryNumber2.CE_ParentID = header2.PK;
			entryNumber2.CE_ParentTable = header2.TableName;
			Factory.Save();
			var filterBO = GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.MRN];
			AssertNotNull("Movement Reference Number Filter exists", filter);
			filter.IsActive = true;
			filter.Property = "AFR1";
			var filteredInfos = Factory.Load(typeof(CusEntryPayInfo), filterBO.Filter);
			AssertEquals(1, filteredInfos.Length);
			var filteredPayInfo = (CusEntryPayInfo)filteredInfos[0];
			AssertEquals("AFR1", filteredPayInfo.MRNumber);
			AssertEquals(23.47m, filteredPayInfo.C9_PaymentAmount);
		}

		public void TestMRNQueryUsesClusterKey()
		{
			var filterBO = GetNewFilterStripBusinessObject();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[ProofOfPaymentFilterBusinessObject.FilterConstants.MRN];
			filter.IsActive = true;
			filter.Property = "Y";
			AssertContains("CH_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
			AssertContains("C9_ClusterKey", filter.Query.LiteralTextSqlFormatted, ignoreCase: true);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ProofOfPaymentFilterBusinessObject();

		JobDeclaration CreateJobDeclarationWithPayInfo(decimal payAmount)
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var payInfo = cusEntryHeader.EntryPayInfos.AddNew();
			payInfo.C9_PaymentAmount = payAmount;
			return declaration;
		}
	}
}
