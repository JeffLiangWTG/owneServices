using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationFilterBusinessObject))]
	class ConsolidatedDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumber()
		{
			BusinessObject[] filteredDecs = null;
			AddConsolidatedDeclaration("B00001910");
			AddConsolidatedDeclaration("B00001911");
			AddConsolidatedDeclaration("B00001912");
			AddConsolidatedDeclaration("B00001916");

			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.DeclarationReference];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			Assert("Should find 4 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 4);
			filter.Property = "S";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			Assert("Should find 0 records but found " + filteredDecs.Length, filteredDecs.Length == 0);
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "B0000";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Records Expected for " + filter.SqlComparisonOperator.ToString() + " [" + filter.Property + "]", 4, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "912";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Records Expected for " + filter.SqlComparisonOperator.ToString() + " [" + filter.Property + "]", 1, filteredDecs.Length);
		}

		public void TestEntryNumberFilter()
		{
			AddCusEntryNumber("1M15353189291", AddConsolidatedDeclaration("DEC NUM 1").LeadDeclaration);
			AddCusEntryNumber("1M41818302981", AddConsolidatedDeclaration("DEC NUM 2").LeadDeclaration);
			AddCusEntryNumber("1S11818192873", AddConsolidatedDeclaration("DEC NUM 3").LeadDeclaration.ActiveEntryHeaders[0]);
			AddCusEntryNumber("1A15353192273", AddConsolidatedDeclaration("DEC NUM 4").LeadDeclaration.ActiveEntryHeaders[0]);
			Business.Testing.TestHelper.DisableMergeRequirementForAllDeclarationsInFactory(Factory);
			Factory.Save();
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "";
			var filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			Assert("Should find 4 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 4);
			filter.Property = "1M1";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
			AssertEquals(ModuleNumberFilter.MultiplyMaxLength(35), filter.MaxLength);
			filter.Property = "M1";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
			filter.Property = "1M1M";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "1S11818192873";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			filter.Property = "1M1";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
		}

		public void TestMasterBillSearch()
		{
			BusinessObject[] filteredDecs = null;
			var testDec1 = AddConsolidatedDeclaration("000");
			var testDec2 = AddConsolidatedDeclaration("111");
			var testDec3 = AddConsolidatedDeclaration("222");
			var testDec4 = AddConsolidatedDeclaration("333");
			var testDec5 = AddConsolidatedDeclaration("444");
			testDec1.LeadDeclaration.JE_MasterBill = "999-00119291";
			testDec2.LeadDeclaration.JE_MasterBill = "999-00119291";
			testDec3.LeadDeclaration.JE_MasterBill = "999-00119291";
			testDec4.LeadDeclaration.JE_MasterBill = "999-00119291";
			testDec5.LeadDeclaration.JE_MasterBill = "999-00119291";
			var testDec6 = AddConsolidatedDeclaration("555");
			var testDec7 = AddConsolidatedDeclaration("666");
			var testDec8 = AddConsolidatedDeclaration("777");
			testDec6.LeadDeclaration.JE_MasterBill = "999-01192918";
			testDec7.LeadDeclaration.JE_MasterBill = "999-01192918";
			testDec8.LeadDeclaration.JE_MasterBill = "999-01192918";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.MasterBill];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
			filter.Property = "999-";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should find 8 Records", 8, filteredDecs.Length);
			filter.Property = "999-00119291";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should find 5 Records", 5, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should find 5 Records", 5, filteredDecs.Length);
			filter.Property = "999-0119291";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should find 0 Records", 0, filteredDecs.Length);
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertEquals("Should find 3 Records", 3, filteredDecs.Length);
			filter.Property = "";
			filteredDecs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			Assert("Should find 8 or more records but only found " + filteredDecs.Length, filteredDecs.Length >= 8);
		}

		public void TestMasterBillIsBlank()
		{
			var consolidatedDeclaration1 = AddConsolidatedDeclaration("123");
			var consolidatedDeclaration2 = AddConsolidatedDeclaration("456");
			var consolidatedDeclaration3 = AddConsolidatedDeclaration("789");
			var bill2 = consolidatedDeclaration2.LeadDeclaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			var bill3 = consolidatedDeclaration3.LeadDeclaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_MasterBill = "Test";
			var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.MasterBill];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			Factory.Save();
			var retrievedBOs = Factory.Load(typeof(ConsolidatedDeclaration), filterBO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consolidatedDeclaration1, consolidatedDeclaration2 }, retrievedBOs);
		}

		public void TestPortOfLoadingSearch()
		{
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			consolidatedDeclaration.LeadDeclaration.JE_RL_NKPortOfLoading = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.LoadDischarge];
			filter.IsActive = true;
			filter.Property1 = "NZAKL";
			AssertNull(Factory.LoadTop1<ConsolidatedDeclaration>(filterBO.Filter));
			filter.Property1 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<ConsolidatedDeclaration>(filterBO.Filter));
		}

		public void TestPortOfArrivalSearch()
		{
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			consolidatedDeclaration.LeadDeclaration.JE_RL_NKPortOfArrival = "AUSYD";
			Factory.Save();
			ModuleLocationFilter filter = (ModuleLocationFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.LoadDischarge];
			filter.IsActive = true;
			filter.Property2 = "NZAKL";
			AssertNull(Factory.LoadTop1<ConsolidatedDeclaration>(filterBO.Filter));
			filter.Property2 = "AUSYD";
			AssertNotNull(Factory.LoadTop1<ConsolidatedDeclaration>(filterBO.Filter));
		}

		public void TestFlightVoyageVesselFilter()
		{
			AssertNotNull(filterBO[DeclarationFilterConstants.FlightVoyageVessel]);
			var consolidatedDeclaration1 = AddConsolidatedDeclaration("123");
			consolidatedDeclaration1.LeadDeclaration.JE_TransportMode = "SEA";
			consolidatedDeclaration1.LeadDeclaration.JE_VoyageFlightNo = "12345";
			consolidatedDeclaration1.LeadDeclaration.JE_VesselName = "The Spitfire";
			var consolidatedDeclaration2 = AddConsolidatedDeclaration("456");
			consolidatedDeclaration2.LeadDeclaration.JE_TransportMode = "SEA";
			consolidatedDeclaration2.LeadDeclaration.JE_VoyageFlightNo = "6789";
			consolidatedDeclaration2.LeadDeclaration.JE_VesselName = "Flying Dutchman";
			var consolidatedDeclaration3 = AddConsolidatedDeclaration("789");
			consolidatedDeclaration3.LeadDeclaration.JE_TransportMode = "SEA";
			consolidatedDeclaration3.LeadDeclaration.JE_VoyageFlightNo = "25874";
			consolidatedDeclaration3.LeadDeclaration.JE_VesselName = "Boaty McBoatface";
			Factory.Save();

			var voyageAndVesselFilter = (ModuleTextAndNkFilter)filterBO[DeclarationFilterConstants.FlightVoyageVessel];
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.IsActive = true;
			var found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals(string.Format("declaration1 is in Collection, dec = {0}", found.Length), true, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is not in Collection", false, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is not in Collection", false, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.NkProperty = "The Spitfire";
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.IsActive = true;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals(string.Format("declaration1 is in Collection, dec = {0}", found.Length), true, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is not in Collection", false, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is not in Collection", false, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "12345";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is in Collection", true, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is not in Collection", false, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is not in Collection", false, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "Flying Dutchman";
			voyageAndVesselFilter.IsActive = true;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is not in Collection", false, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is in Collection", true, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is not in Collection", false, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "2";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is in Collection", true, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is not in Collection", false, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is in Collection", true, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "c";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is not in Collection", false, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is in Collection", true, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is in Collection", true, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is in Collection", true, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is in Collection", true, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is in Collection", true, found.Contains(consolidatedDeclaration3));
			voyageAndVesselFilter.Property = "";
			voyageAndVesselFilter.NkProperty = "";
			voyageAndVesselFilter.IsActive = true;
			voyageAndVesselFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("declaration1 is not in Collection", false, found.Contains(consolidatedDeclaration1));
			AssertEquals("declaration2 is not in Collection", false, found.Contains(consolidatedDeclaration2));
			AssertEquals("declaration3 is not in Collection", false, found.Contains(consolidatedDeclaration3));
			AssertEquals(JobDeclarationSchema.JE_VoyageFlightNo.MaxLength, voyageAndVesselFilter.MaxLength);
			AssertEquals(JobDeclarationSchema.JE_VesselName.MaxLength, voyageAndVesselFilter.NkMaxLength);
		}

		public void TestPeriodTo()
		{
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			consolidatedDeclaration.CRD_PeriodTo = ZDate.Today.AddDays(-1);
			Factory.Save();
			var filter = (ModuleDateFilter)filterBO[ConsolidatedDeclarationFilterBusinessObject.Constants.PeriodTo];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.Future;
			AssertEquals(0, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
			filter.PropertySearch = ModuleDateFilter.Past;
			AssertEquals(1, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
		}

		public void TestImporterSearch()
		{
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Importer, 1, DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, true, "123");
			TestOrganisationSearch(BaseJobDeclaration.Schema.JE_OH_Importer, 1, DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier, false, "456");
		}

		public virtual void TestSupplierSearch()
		{
			OrgHeader org = OrgHeader.New(Factory);
			org.MainAddress.OA_Address1 = "addr1";
			org.OH_Code = "-1-";
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			BaseJobComInvoiceHeader invoice = consolidatedDeclaration.LeadDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice.JZ_JE = consolidatedDeclaration.CRD_JE_LeadDeclaration;
			invoice.JZ_OH_Supplier = org.PK;
			var noMatchDec = AddConsolidatedDeclaration("456");
			BaseJobComInvoiceHeader noMatchInvoice = noMatchDec.LeadDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			noMatchInvoice.JZ_JE = noMatchDec.CRD_JE_LeadDeclaration;
			OrgHeader decSupplier = OrgHeader.New(Factory);
			decSupplier.MainAddress.OA_Address1 = "Dec Supplier Address 1";
			decSupplier.OH_Code = "DECSUP";
			consolidatedDeclaration.LeadDeclaration.JE_OH_Supplier = decSupplier.PK;
			Factory.Save();
			ModuleGuidsFilter filter = (ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier];
			filter.IsActive = true;
			filter.Property2 = org.PK;
			var found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("Matched supplier on Invoice Header", 1, found.Length);
			AssertEquals("Matched supplier on Invoice Header", org.PK, found[0].LeadDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JZ_OH_Supplier);
			filter.Property2 = ZGuid.NewZGuid();
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 0, found.Length);
			filter.Property2 = decSupplier.PK;
			found = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			AssertEquals("Matched supplier on Declaration", 1, found.Length);
			AssertEquals("Matched supplier on Declaration", decSupplier.PK, found[0].LeadDeclaration.JE_OH_Supplier);
		}

		public void TestMessageStatus()
		{
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			consolidatedDeclaration.CRD_MessageStatus = "ABC";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.MessageStatusText];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "XZY";
			AssertEquals(0, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
			filter.Property = "ABC";
			AssertEquals(1, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
		}

		public void TestCustomsStatus()
		{
			var consolidatedDeclaration = AddConsolidatedDeclaration();
			consolidatedDeclaration.CRD_CustomsStatus = "ABC";
			Factory.Save();
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryStatusText];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "XZY";
			AssertEquals(0, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
			filter.Property = "ABC";
			AssertEquals(1, Factory.Load<ConsolidatedDeclaration>(filterBO.Filter).Length);
		}

		public virtual void TestCustomsStatus_NotSentFilter()
		{
			Assert("Filter does not contain JE_EntrySCRD_CustomsStatustatus = '' when entry status empty", filterBO.Filter.LiteralTextADO.IndexOf("CRD_CustomsStatus = ''") == -1);
			var entryStatus = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryStatusText];
			entryStatus.IsActive = true;
			entryStatus.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			entryStatus.SqlComparisonOperator = SQLComparisonOperator.Equal;
			Assert("Filter contains CRD_CustomsStatus = '' when NotSent is filtered on", filterBO.Filter.LiteralTextADO.IndexOf("CRD_CustomsStatus = ''") != -1);
		}

		public virtual void TestCustomsStatusFilterMaxLength()
		{
			var entryStatus = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryStatusText];
			entryStatus.IsActive = true;
			AssertEquals(BaseJobDeclaration.Schema.JE_EntryStatusMaxLength, entryStatus.MaxLength);
		}

		public void TestDefaultCreatedTimeFilter()
		{
			var decFilterBO = new ConsolidatedDeclarationFilterBusinessObject();
			decFilterBO.QueryObjectType = typeof(ConsolidatedDeclaration);
			var filter = decFilterBO.ModuleFilters["Created Time"] as ModuleDateFilter;
			AssertEquals(true, filter.Visible);
			AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Last3Mths, filter.PropertySearch);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBO;

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = new ConsolidatedDeclarationFilterBusinessObject();
			filterBO.QueryObjectType = typeof(ConsolidatedDeclaration);
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(CusEntryHeaderSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			exclusions.Add(TableFilter(CusEntryNumSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			exclusions.Add(TableFilter(JobDeclarationSchema.Constants.TableName, DeclarationFilterConstants.NumberFilterTypes.EntryNumber));
			exclusions.Add(TableFilter(CusDecHouseBillSchema.Constants.TableName, "Master Bill"));
			return exclusions;
		}

		void TestOrganisationSearch(string declPropName, int property1Or2, string orgFilterType, bool isOrgActive, ZString referenceNum)
		{
			var org = OrgHeader.New(Factory);
			org.OH_Code = isOrgActive ? "_!_!_!" : "ABC";
			org.OH_IsActive = isOrgActive;
			var consolidatedDeclaration = AddConsolidatedDeclaration(referenceNum);
			consolidatedDeclaration.LeadDeclaration[declPropName] = org.PK;
			Factory.Save();
			var filter = filterBO[orgFilterType];
			if (filter is ModuleGuidsFilter guidsFilter)
			{
				if (property1Or2 == 1)
				{
					guidsFilter.Property1 = org.PK;
				}
				else
				{
					guidsFilter.Property2 = org.PK;
				}
			}
			else if (filter is ModuleGuidFilter guidFilter)
			{
				guidFilter.Property = org.PK;
			}

			filter.IsActive = true;
			var collection = Factory.Load<ConsolidatedDeclaration>(filterBO.Filter);
			if (isOrgActive)
			{
				if (filter is ModuleGuidsFilter moduleGuidsFilter)
				{
					if (property1Or2 == 1)
					{
						AssertNoWarning(moduleGuidsFilter.Property1Info, "Organization is in-active.");
					}
					else
					{
						AssertNoWarning(moduleGuidsFilter.Property2Info, "Organization is in-active.");
					}
				}
				else if (filter is ModuleGuidFilter moduleGuidFilter)
				{
					AssertNoWarning(moduleGuidFilter.PropertyInfo, "Organization is in-active.");
				}
			}
			else
			{
				if (filter is ModuleGuidsFilter moduleGuidsFilter)
				{
					if (property1Or2 == 1)
					{
						AssertHasWarning(moduleGuidsFilter.Property1Info, "Organization is in-active.");
					}
					else
					{
						AssertHasWarning(moduleGuidsFilter.Property2Info, "Organization is in-active.");
					}
				}
				else if (filter is ModuleGuidFilter moduleGuidFilter)
				{
					AssertHasWarning(moduleGuidFilter.PropertyInfo, "Organization is in-active.");
				}
			}

			AssertEquals("Matched " + orgFilterType + " on declaration", 1, collection.Length);
			AssertEquals("Matched " + orgFilterType + " on declaration", org.PK, collection[0].LeadDeclaration[declPropName]);
		}

		ConsolidatedDeclaration AddConsolidatedDeclaration(string decRef = "123")
		{
			var result = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			Factory.Save();
			result.CRD_JobReferenceNumber = decRef;
			return result;
		}

		CusEntryNumber AddCusEntryNumber(ZString entryNumber, BusinessObject parent)
		{
			CusEntryNumber result = Factory.New<CusEntryNumber>();
			result.CE_EntryNum = entryNumber;
			result.CE_ParentID = parent.PK;
			result.CE_ParentTable = parent.TableName;
			result.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return result;
		}

		ConsolidatedDeclarationFilterBusinessObject filterBO;
	}
}
