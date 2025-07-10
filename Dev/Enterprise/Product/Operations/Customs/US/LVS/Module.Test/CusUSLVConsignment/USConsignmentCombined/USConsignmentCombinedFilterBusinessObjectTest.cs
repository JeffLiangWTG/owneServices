using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(USConsignmentCombinedFilterBusinessObject))]
	class USConsignmentCombinedFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Numbers and Reference

		#region TestEntryNumberFilter

		public void TestEntryNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryNumber, FilterCategories.NumbersAndReferences, "Entry Number");
		}

		public void TestEntryNumberFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			var cusEntryNum3 = CreateCusEntryNumber(declaration1);
			var cusEntryNum4 = CreateCusEntryNumber(declaration2);

			consignment1.CE_EntryNum = "1122";
			consignment2.CE_EntryNum = "1133";
			cusEntryNum3.CE_EntryNum = "6677";
			cusEntryNum4.CE_EntryNum = "6688";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryNumberFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			entryNumberFilter.Property = "1133";

			AssertTextFilterMatches(entryNumberFilter, bizo => bizo.UBV_EntryNum, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestEntryNumberFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			var cusEntryNum3 = CreateCusEntryNumber(declaration1);
			var cusEntryNum4 = CreateCusEntryNumber(declaration2);

			consignment1.CE_EntryNum = "1122";
			consignment2.CE_EntryNum = "1133";
			cusEntryNum3.CE_EntryNum = "6677";
			cusEntryNum4.CE_EntryNum = "6688";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryNumberFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			entryNumberFilter.Property = "66";

			AssertTextFilterMatches(entryNumberFilter, bizo => bizo.UBV_JobReference, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestJobNumberFilter

		public void TestJobNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.JobNumber, FilterCategories.NumbersAndReferences, "Job Number");
		}

		public void TestJobNumberFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var jobNumberFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.JobNumber] as ModuleTextFilter;
			jobNumberFilter.Property = consignment2.Shipment.ULH_JobNumber;

			AssertTextFilterMatches(jobNumberFilter, bizo => bizo.UBV_JobReference, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestJobNumberFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			declaration1.JE_DeclarationReference = "S00000211";
			declaration2.JE_DeclarationReference = "S00000212";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var jobNumberFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.JobNumber] as ModuleTextFilter;
			jobNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			jobNumberFilter.Property = "S000002";

			AssertTextFilterMatches(jobNumberFilter, bizo => bizo.UBV_JobReference, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestHouseBillFilter

		public void TestHouseBillFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.HouseBill, FilterCategories.NumbersAndReferences, "House Bill");
		}

		public void TestHouseBillFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ULB_HouseBill = "AA1234";
			consignment2.ULB_HouseBill = "AA5678";
			declaration1.JE_HouseBill = "BB1234";
			declaration2.JE_HouseBill = "BB5678";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.HouseBill] as ModuleTextFilter;
			houseBillFilter.Property = "AA5678";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.UBV_JobReference, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestHouseBillFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ULB_HouseBill = "AA1234";
			consignment2.ULB_HouseBill = "AA5678";
			declaration1.JE_HouseBill = "BB1234";
			declaration2.JE_HouseBill = "BB5678";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.HouseBill] as ModuleTextFilter;
			houseBillFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			houseBillFilter.Property = "BB";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.UBV_JobReference, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestMasterBillFilter

		public void TestMasterBillFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MasterBill, FilterCategories.NumbersAndReferences, "Master Bill");
		}

		public void TestMasterBillFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_MasterBill = "AA1234";
			consignment2.Shipment.ULH_MasterBill = "AA5678";
			declaration1.JE_MasterBill = "BB1234";
			declaration2.JE_MasterBill = "BB5678";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var masterBillFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MasterBill] as ModuleTextFilter;
			masterBillFilter.Property = "AA5678";

			AssertTextFilterMatches(masterBillFilter, bizo => bizo.UBV_MasterBill, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestMasterBillFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_MasterBill = "AA1234";
			consignment2.Shipment.ULH_MasterBill = "AA5678";
			declaration1.JE_MasterBill = "BB1234";
			declaration2.JE_MasterBill = "BB5678";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var masterBillFitler = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MasterBill] as ModuleTextFilter;
			masterBillFitler.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			masterBillFitler.Property = "BB";

			AssertTextFilterMatches(masterBillFitler, bizo => bizo.UBV_MasterBill, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#endregion

		#region StatusAndFlags

		#region TestMessageStatusFilter

		public void TestMessageStatusFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MessageStatus, FilterCategories.StatusAndFlags, "Message Status");
		}

		public void TestMessageStatusFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
			declaration1.JE_MessageStatus = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
			declaration2.JE_MessageStatus = ImportMessageStatusList.Codes.AwaitingDepartureAmendment;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var messageStatusFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MessageStatus] as ModuleTextFilter;
			messageStatusFilter.Property = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;

			AssertTextFilterMatches(messageStatusFilter, bizo => bizo.UBV_MessageStatus, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestMessageStatusFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			var cusEntryHeader1 = Factory.New<CusEntryHeader>();
			cusEntryHeader1.CH_JE = declaration1.PK;
			cusEntryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var cusEntryHeader2 = Factory.New<CusEntryHeader>();
			cusEntryHeader2.CH_JE = declaration2.PK;
			cusEntryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			consignment1.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingArrival;
			consignment2.ULB_MessageStatus = ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate;
			cusEntryHeader1.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;
			cusEntryHeader2.CH_Status = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var messageStatusFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.MessageStatus] as ModuleTextFilter;
			messageStatusFilter.Property = ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete;

			AssertTextFilterMatches(messageStatusFilter, bizo => bizo.UBV_MessageStatus, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestReleaseStatusFilter

		public void TestReleaseStatusFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseStatus, FilterCategories.StatusAndFlags, "Release Status");
		}

		public void TestReleaseStatusFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.CE_EntryStatus = ImportEntryStatusList.Codes.CRF;
			consignment2.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;
			declaration1.ReleaseStatus = ImportEntryStatusList.Codes.CRN;
			declaration2.ReleaseStatus = ImportEntryStatusList.Codes.MUL;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var releaseStatusFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseStatus] as ModuleTextFilter;
			releaseStatusFilter.Property = ImportEntryStatusList.Codes.CRL;

			AssertTextFilterMatches(releaseStatusFilter, bizo => bizo.UBV_ReleaseStatus, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestReleaseStatusFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.CE_EntryStatus = ImportEntryStatusList.Codes.CRF;
			consignment2.CE_EntryStatus = ImportEntryStatusList.Codes.CRL;
			declaration1.ReleaseStatus = ImportEntryStatusList.Codes.CRN;
			declaration2.ReleaseStatus = ImportEntryStatusList.Codes.CRN;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var releaseStatusFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseStatus] as ModuleTextFilter;
			releaseStatusFilter.Property = ImportEntryStatusList.Codes.CRN;

			AssertTextFilterMatches(releaseStatusFilter, bizo => bizo.UBV_ReleaseStatus, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestActionRequiredFilter

		public void TestActionRequiredFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ActionRequired, FilterCategories.StatusAndFlags, "Action Required");
		}

		public void TestActionRequiredFilter_HasPGAPending_SingleMatch()
		{
			const string tariffNumber = "8542996328";
			var tariff = CreateTariff(tariffNumber, OGARequirementList.Codes.FD1);

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var consignment3 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1.ULI_Tariff = tariffNumber;
			item3.ULI_Tariff = tariffNumber;
			item3.ACEFDAWrapper.DisclaimReason = "A";
			item3.ACEFDAWrapper.Indicator = "C";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var consignmentView3 = Factory.Load<USConsignmentCombined>(consignment3.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var hasPGAPendingFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ActionRequired] as ModuleTextFilter;
			hasPGAPendingFilter.Property = USConsignmentCombinedFilterBusinessObject.PGAApplyButNotDisclaimed;

			AssertTextFilterMatches(hasPGAPendingFilter, bizo => bizo.UBV_HasPGAPending, new[] { (consignmentView1, true), (consignmentView2, false), (consignmentView3, false), (declarationView1, false) });
		}

		public void TestActionRequiredFilter_HasPGAPending_MultipleMatch()
		{
			const string tariffNumber = "8542996328";
			var tariff = CreateTariff(tariffNumber, OGARequirementList.Codes.FD1);

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var consignment3 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1.ULI_Tariff = tariffNumber;
			item2.ULI_Tariff = tariffNumber;
			item3.ULI_Tariff = tariffNumber;
			item3.ACEFDAWrapper.DisclaimReason = "A";
			item3.ACEFDAWrapper.Indicator = "C";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var consignmentView3 = Factory.Load<USConsignmentCombined>(consignment3.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var hasPGAPendingFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ActionRequired] as ModuleTextFilter;
			hasPGAPendingFilter.Property = USConsignmentCombinedFilterBusinessObject.PGAApplyButNotDisclaimed;

			AssertTextFilterMatches(hasPGAPendingFilter, bizo => bizo.UBV_HasPGAPending, new[] { (consignmentView1, true), (consignmentView2, true), (consignmentView3, false), (declarationView1, false) });
		}

		public void TestActionRequiredFilter_PGANotSupport_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var consignment3 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1.ULI_AntiDumping = false;
			item2.ULI_AntiDumping = true;
			item3.ULI_AntiDumping = true;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var consignmentView3 = Factory.Load<USConsignmentCombined>(consignment3.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var pgaNotSupportedFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ActionRequired] as ModuleTextFilter;
			pgaNotSupportedFilter.Property = USConsignmentCombinedFilterBusinessObject.NotSupportedInLVS;

			AssertTextFilterMatches(pgaNotSupportedFilter, bizo => bizo.UBV_PGANotSupported, new[] { (consignmentView1, true), (consignmentView2, false), (consignmentView3, false), (declarationView1, false) });
		}

		public void TestActionRequiredFilter_PGANotSupport_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var consignment3 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();

			var item1 = consignment1.CusUSLVItems.AddNew();
			var item2 = consignment2.CusUSLVItems.AddNew();
			var item3 = consignment3.CusUSLVItems.AddNew();

			item1.ULI_AntiDumping = false;
			item2.ULI_AntiDumping = true;
			item3.ULI_AntiDumping = false;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var consignmentView3 = Factory.Load<USConsignmentCombined>(consignment3.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var pgaNotSupportedFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ActionRequired] as ModuleTextFilter;
			pgaNotSupportedFilter.Property = USConsignmentCombinedFilterBusinessObject.NotSupportedInLVS;

			AssertTextFilterMatches(pgaNotSupportedFilter, bizo => bizo.UBV_PGANotSupported, new[] { (consignmentView1, true), (consignmentView2, false), (consignmentView3, true), (declarationView1, false) });
		}

		#endregion
		#endregion

		#region Dates

		#region TestDepartureDateFilter

		public void TestDepartureDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DepartureDate, FilterCategories.Dates, "Departure");
		}

		[TestDate(2020, 01, 01)]
		public void TestDepartureDateFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_DepartureDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_DepartureDate = ZDate.Today.AddDays(-10);
			declaration1.JE_ExportDate = ZDate.Today.AddDays(-50);
			declaration2.JE_ExportDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var departureDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DepartureDate] as ModuleDateFilter;
			departureDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			departureDateFilter.Property2 = ZDateTime.Today;
			departureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(departureDateFilter, bizo => bizo.UBV_DepartureDate, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestDepartureDateFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_DepartureDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_DepartureDate = ZDate.Today.AddDays(-10);
			declaration1.JE_ExportDate = ZDate.Today.AddDays(-50);
			declaration2.JE_ExportDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var depatureDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DepartureDate] as ModuleDateFilter;
			depatureDateFilter.Property1 = ZDateTime.Today.AddDays(-60);
			depatureDateFilter.Property2 = ZDateTime.Today.AddDays(-7);
			depatureDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(depatureDateFilter, bizo => bizo.UBV_DepartureDate, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#region TestDischargeDateFilter

		public void TestDischargeDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargeDate, FilterCategories.Dates, "Discharge");
		}

		[TestDate(2020, 01, 01)]
		public void TestDischargeDateFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_DischargeDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_DischargeDate = ZDate.Today.AddDays(-10);
			declaration1.JE_DateOfArrival = ZDate.Today.AddDays(-50);
			declaration2.JE_DateOfArrival = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargeDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargeDate] as ModuleDateFilter;
			dischargeDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			dischargeDateFilter.Property2 = ZDateTime.Today;
			dischargeDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(dischargeDateFilter, bizo => bizo.UBV_DischargeDate, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestDischargeDateFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_DischargeDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_DischargeDate = ZDate.Today.AddDays(-10);
			declaration1.JE_DateOfArrival = ZDate.Today.AddDays(-50);
			declaration2.JE_DateOfArrival = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargeDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargeDate] as ModuleDateFilter;
			dischargeDateFilter.Property1 = ZDateTime.Today.AddDays(-60);
			dischargeDateFilter.Property2 = ZDateTime.Today.AddDays(-7);
			dischargeDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(dischargeDateFilter, bizo => bizo.UBV_DischargeDate, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#region TestEntryDateFilter

		public void TestEntryDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryDate, FilterCategories.Dates, "Port of Entry");
		}

		[TestDate(2020, 01, 01)]
		public void TestEntryDateFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_EntryDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_EntryDate = ZDate.Today.AddDays(-10);
			declaration1.US_EntryDate = ZDate.Today.AddDays(-50);
			declaration2.US_EntryDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.UBV_EntryDate, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestEntryDateFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_EntryDate = ZDate.Today.AddDays(-5);
			consignment2.Shipment.ULH_EntryDate = ZDate.Today.AddDays(-10);
			declaration1.US_EntryDate = ZDate.Today.AddDays(-50);
			declaration2.US_EntryDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-60);
			entryDateFilter.Property2 = ZDateTime.Today.AddDays(-7);
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.UBV_EntryDate, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#region TestReleaseDateFilter

		public void TestReleaseDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseDate, FilterCategories.Dates, "Released");
		}

		[TestDate(2020, 01, 01)]
		public void TestReleaseDateFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.CE_IssueDate = ZDate.Today.AddDays(-5);
			consignment2.CE_IssueDate = ZDate.Today.AddDays(-10);
			declaration1.JE_EntryAuthorisationDate = ZDate.Today.AddDays(-50);
			declaration2.JE_EntryAuthorisationDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var releaseDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseDate] as ModuleDateFilter;
			releaseDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			releaseDateFilter.Property2 = ZDateTime.Today;
			releaseDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(releaseDateFilter, bizo => bizo.UBV_ReleaseDate, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestReleaseDateFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.CE_IssueDate = ZDate.Today.AddDays(-5);
			consignment2.CE_IssueDate = ZDate.Today.AddDays(-10);
			declaration1.JE_EntryAuthorisationDate = ZDate.Today.AddDays(-50);
			declaration2.JE_EntryAuthorisationDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var releaseDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ReleaseDate] as ModuleDateFilter;
			releaseDateFilter.Property1 = ZDateTime.Today.AddDays(-60);
			releaseDateFilter.Property2 = ZDateTime.Today.AddDays(-7);
			releaseDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(releaseDateFilter, bizo => bizo.UBV_ReleaseDate, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#region TestSubmittedDateFilter

		public void TestSubmittedDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SubmittedDate, FilterCategories.Dates, "Submitted");
		}

		[TestDate(2020, 01, 01)]
		public void TestSubmittedDateFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			var cusEntryHeader1 = Factory.New<CusEntryHeader>();
			cusEntryHeader1.CH_JE = declaration1.PK;
			cusEntryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var cusEntryHeader2 = Factory.New<CusEntryHeader>();
			cusEntryHeader2.CH_JE = declaration2.PK;
			cusEntryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			consignment1.ULB_SubmittedDate = ZDate.Today.AddDays(-5);
			consignment2.ULB_SubmittedDate = ZDate.Today.AddDays(-10);
			cusEntryHeader1.CH_EntrySubmittedDate = ZDate.Today.AddDays(-50);
			cusEntryHeader2.CH_EntrySubmittedDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var submittedDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SubmittedDate] as ModuleDateFilter;
			submittedDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			submittedDateFilter.Property2 = ZDateTime.Today;
			submittedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(submittedDateFilter, bizo => bizo.UBV_SubmittedDate, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestSubmittedDateFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			var cusEntryHeader1 = Factory.New<CusEntryHeader>();
			cusEntryHeader1.CH_JE = declaration1.PK;
			cusEntryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var cusEntryHeader2 = Factory.New<CusEntryHeader>();
			cusEntryHeader2.CH_JE = declaration2.PK;
			cusEntryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			consignment1.ULB_SubmittedDate = ZDate.Today.AddDays(-5);
			consignment2.ULB_SubmittedDate = ZDate.Today.AddDays(-10);
			cusEntryHeader1.CH_EntrySubmittedDate = ZDate.Today.AddDays(-50);
			cusEntryHeader2.CH_EntrySubmittedDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var submittedDateFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SubmittedDate] as ModuleDateFilter;
			submittedDateFilter.Property1 = ZDateTime.Today.AddDays(-60);
			submittedDateFilter.Property2 = ZDateTime.Today.AddDays(-7);
			submittedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(submittedDateFilter, bizo => bizo.UBV_SubmittedDate, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#endregion

		#region Modes and Types

		#region TestTransportModeFilter

		public void TestTransportModeFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.TransportMode, FilterCategories.ModesAndTypes, "Transport Mode");
		}

		public void TestTransportModeFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_TransportMode = TransportModes.Air;
			consignment2.Shipment.ULH_TransportMode = TransportModes.Sea;
			declaration1.JE_TransportMode = TransportModes.Road;
			declaration2.JE_TransportMode = TransportModes.Rail;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var transportModeFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.TransportMode] as ModuleTextFilter;
			transportModeFilter.Property = TransportModes.Road;

			AssertTextFilterMatches(transportModeFilter, bizo => bizo.UBV_TransportMode, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, false) });
		}

		public void TestTransportModeFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_TransportMode = TransportModes.Air;
			consignment2.Shipment.ULH_TransportMode = TransportModes.Air;
			declaration1.JE_TransportMode = TransportModes.Road;
			declaration2.JE_TransportMode = TransportModes.Rail;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var transportModeFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.TransportMode] as ModuleTextFilter;
			transportModeFilter.Property = TransportModes.Air;

			AssertTextFilterMatches(transportModeFilter, bizo => bizo.UBV_TransportMode, new[] { (consignmentView1, true), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		#endregion

		#region TestVesselFlightVoyageFilter

		public void TestVesselFlightVoyageFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage, FilterCategories.ModesAndTypes, "Vessel and Flight/Voyage #");
		}

		public void TestVesselFlightVoyageFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_VoyageFlightNo = "111";
			consignment2.Shipment.ULH_VoyageFlightNo = "222";
			declaration1.JE_VoyageFlightNo = "333";
			declaration2.JE_VoyageFlightNo = "111";

			consignment1.Shipment.ULH_ConveyanceName = "AAA";
			consignment2.Shipment.ULH_ConveyanceName = "BBB";
			declaration1.JE_VesselName = "AAA";
			declaration2.JE_VesselName = "CCC";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var vesselFlightVoyageFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage] as ModuleTextAndNkFilter;
			vesselFlightVoyageFilter.Property = "111";
			vesselFlightVoyageFilter.NkProperty = "AAA";

			AssertTextAndNkFilterMatches(vesselFlightVoyageFilter, bizo => bizo.UBV_VoyageFlightNo, bizo => bizo.UBV_ConveyanceName, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestVesselFlightVoyageFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_VoyageFlightNo = "111";
			consignment2.Shipment.ULH_VoyageFlightNo = "222";
			declaration1.JE_VoyageFlightNo = "333";
			declaration2.JE_VoyageFlightNo = "111";

			consignment1.Shipment.ULH_ConveyanceName = "AAA";
			consignment2.Shipment.ULH_ConveyanceName = "BBB";
			declaration1.JE_VesselName = "AAA";
			declaration2.JE_VesselName = "AAA";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var vesselFlightVoyageFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage] as ModuleTextAndNkFilter;
			vesselFlightVoyageFilter.Property = "111";
			vesselFlightVoyageFilter.NkProperty = "AAA";

			AssertTextAndNkFilterMatches(vesselFlightVoyageFilter, bizo => bizo.UBV_VoyageFlightNo, bizo => bizo.UBV_ConveyanceName, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, true) });
		}

		#endregion

		#endregion

		#region Locations

		#region TestDischargePortFilter

		public void TestDischargePortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargePort, FilterCategories.Locations, "Discharge Port");
		}

		public void TestDischargePortFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfDischarge = "1234";
			consignment2.Shipment.ULH_PortOfDischarge = "5678";
			declaration1.US_SchDArrival = "9999";
			declaration2.US_SchDArrival = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargePortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargePort] as ModuleNkFilter;
			dischargePortFilter.Property = "1234";

			AssertTextFilterMatches(dischargePortFilter, bizo => bizo.UBV_PortOfDischarge, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestDischargePortFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfDischarge = "1234";
			consignment2.Shipment.ULH_PortOfDischarge = "5678";
			declaration1.US_SchDArrival = "9999";
			declaration2.US_SchDArrival = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargePortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.DischargePort] as ModuleNkFilter;
			dischargePortFilter.Property = "9999";

			AssertTextFilterMatches(dischargePortFilter, bizo => bizo.UBV_PortOfDischarge, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestEntryPortFilter

		public void TestEntryPortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryPort, FilterCategories.Locations, "Entry Port");
		}

		public void TestEntryPortFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfEntry = "1234";
			consignment2.Shipment.ULH_PortOfEntry = "5678";
			declaration1.US_SchDEntry = "9999";
			declaration2.US_SchDEntry = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryPortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryPort] as ModuleNkFilter;
			entryPortFilter.Property = "1234";

			AssertTextFilterMatches(entryPortFilter, bizo => bizo.UBV_PortOfEntry, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestEntryPortFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfEntry = "1234";
			consignment2.Shipment.ULH_PortOfEntry = "5678";
			declaration1.US_SchDEntry = "9999";
			declaration2.US_SchDEntry = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryPortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.EntryPort] as ModuleNkFilter;
			entryPortFilter.Property = "9999";

			AssertTextFilterMatches(entryPortFilter, bizo => bizo.UBV_PortOfEntry, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestLoadingPortFilter

		public void TestLoadingPortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadingPort, FilterCategories.Locations, "Loading Port");
		}

		public void TestLoadingPortFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfLoading = "1234";
			consignment2.Shipment.ULH_PortOfLoading = "5678";
			declaration1.US_SchDLoading = "9999";
			declaration2.US_SchDLoading = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadingPortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadingPort] as ModuleNkFilter;
			loadingPortFilter.Property = "1234";

			AssertTextFilterMatches(loadingPortFilter, bizo => bizo.UBV_PortOfLoading, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestLoadingPortFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_PortOfLoading = "1234";
			consignment2.Shipment.ULH_PortOfLoading = "5678";
			declaration1.US_SchDLoading = "9999";
			declaration2.US_SchDLoading = "9999";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadingPortFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadingPort] as ModuleNkFilter;
			loadingPortFilter.Property = "9999";

			AssertTextFilterMatches(loadingPortFilter, bizo => bizo.UBV_PortOfLoading, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestLoadDischargeUNLOCOFilter

		public void TestLoadDischargeUNLOCOFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO, FilterCategories.Locations, "Load/Discharge");
		}

		public void TestLoadDischargeUNLOCOFilter_SingleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_RL_NKPortOfLoading = "AAA";
			consignment2.Shipment.ULH_RL_NKPortOfLoading = "BBB";
			declaration1.JE_RL_NKPortOfLoading = "CCC";
			declaration2.JE_RL_NKPortOfLoading = "AAA";

			consignment1.Shipment.ULH_RL_NKPortOfDischarge = "XXX";
			consignment2.Shipment.ULH_RL_NKPortOfDischarge = "YYY";
			declaration1.JE_RL_NKPortOfArrival = "XXX";
			declaration2.JE_RL_NKPortOfArrival = "ZZZ";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadDischargeUNLOCOFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO] as ModuleLocationFilter;
			loadDischargeUNLOCOFilter.Property1 = "AAA";
			loadDischargeUNLOCOFilter.Property2 = "XXX";

			AssertLocationFilterMatches(loadDischargeUNLOCOFilter, bizo => bizo.UBV_RL_NKPortOfLoading, bizo => bizo.UBV_RL_NKPortOfDischarge, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestLoadDischargeUNLOCOFilter_MultipleMatch()
		{
			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_RL_NKPortOfLoading = "AAA";
			consignment2.Shipment.ULH_RL_NKPortOfLoading = "BBB";
			declaration1.JE_RL_NKPortOfLoading = "AAA";
			declaration2.JE_RL_NKPortOfLoading = "AAA";

			consignment1.Shipment.ULH_RL_NKPortOfDischarge = "XXX";
			consignment2.Shipment.ULH_RL_NKPortOfDischarge = "XXX";
			declaration1.JE_RL_NKPortOfArrival = "XXX";
			declaration2.JE_RL_NKPortOfArrival = "ZZZ";

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadDischargeUNLOCOFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO] as ModuleLocationFilter;
			loadDischargeUNLOCOFilter.Property1 = "AAA";
			loadDischargeUNLOCOFilter.Property2 = "XXX";

			AssertLocationFilterMatches(loadDischargeUNLOCOFilter, bizo => bizo.UBV_RL_NKPortOfLoading, bizo => bizo.UBV_RL_NKPortOfDischarge, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, true), (declarationView2, false) });
		}

		#endregion

		#endregion

		#region Organisations

		#region TestClientFilter

		public void TestImporterFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Client, FilterCategories.Organisations, "Client");
		}

		public void TestImporterFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_OH_Client = org1.PK;
			consignment2.Shipment.ULH_OH_Client = org2.PK;

			//Client is always null for a LowValue Bill created from a JobDeclaration

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var clientFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Client] as ModuleGuidFilter;
			clientFilter.Property = org2.PK;

			AssertGuidFilterMatches(clientFilter, bizo => bizo.UBV_OH_Client, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestImporterFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_OH_Client = org1.PK;
			consignment2.Shipment.ULH_OH_Client = org1.PK;

			//Client is always null for a LowValue Bill created from a JobDeclaration

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var clientFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Client] as ModuleGuidFilter;
			clientFilter.Property = org1.PK;

			AssertGuidFilterMatches(clientFilter, bizo => bizo.UBV_OH_Client, new[] { (consignmentView1, true), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		#endregion

		#region TestConsigneeFilter

		public void TestConsigneeFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Consignee, FilterCategories.Organisations, "Consignee");
		}

		public void TestConsigneeFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ConsigneeOrgPK = org1.PK;
			consignment2.ConsigneeOrgPK = org2.PK;
			declaration1.ConsigneeAddressOrgPK = org3.PK;
			declaration2.ConsigneeAddressOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var consigneeFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Consignee] as ModuleGuidFilter;
			consigneeFilter.Property = org2.PK;

			AssertGuidFilterMatches(consigneeFilter, bizo => bizo.UBV_OH_Consignee, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestConsigneeFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ConsigneeOrgPK = org1.PK;
			consignment2.ConsigneeOrgPK = org2.PK;
			declaration1.ConsigneeAddressOrgPK = org3.PK;
			declaration2.ConsigneeAddressOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var consigneeFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Consignee] as ModuleGuidFilter;
			consigneeFilter.Property = org3.PK;

			AssertGuidFilterMatches(consigneeFilter, bizo => bizo.UBV_OH_Consignee, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestConsigneeNameFilter

		public void TestConsigneeNameFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ConsigneeName, FilterCategories.Organisations, "Consignee Name");
		}

		public void TestConsigneeNameFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG1";
			org2.OH_FullName = "ORG2";
			org3.OH_FullName = "ORG3";

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ConsigneeOrgPK = org1.PK;
			consignment2.ConsigneeOrgPK = org2.PK;
			declaration1.ConsigneeAddressOrgPK = org3.PK;
			declaration2.ConsigneeAddressOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var consigneeNameFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ConsigneeName] as ModuleTextFilter;
			consigneeNameFilter.Property = "ORG1";

			AssertTextFilterMatches(consigneeNameFilter, bizo => bizo.UBV_ConsigneeName, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestConsigneeNameFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG1";
			org2.OH_FullName = "ORG2";
			org3.OH_FullName = "ORG3";

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.ConsigneeOrgPK = org1.PK;
			consignment2.ConsigneeOrgPK = org2.PK;
			declaration1.ConsigneeAddressOrgPK = org3.PK;
			declaration2.ConsigneeAddressOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var consigneeNameFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ConsigneeName] as ModuleTextFilter;
			consigneeNameFilter.Property = "ORG3";

			AssertTextFilterMatches(consigneeNameFilter, bizo => bizo.UBV_ConsigneeName, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestImporterOfRecordFilter

		public void TestImporterOfRecordFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ImporterOfRecord, FilterCategories.Organisations, "Importer of Record");
		}

		public void TestImporterOfRecordFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_OH_Importer = org1.PK;
			consignment2.Shipment.ULH_OH_Importer = org2.PK;
			declaration1.JE_OA_DeclarantAddress = org3.Addresses[0].PK;
			declaration2.JE_OA_DeclarantAddress = org3.Addresses[0].PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var importerOfRecordFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ImporterOfRecord] as ModuleGuidFilter;
			importerOfRecordFilter.Property = org2.PK;

			AssertGuidFilterMatches(importerOfRecordFilter, bizo => bizo.UBV_OH_Importer, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestImporterOfRecordFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.Shipment.ULH_OH_Importer = org1.PK;
			consignment2.Shipment.ULH_OH_Importer = org2.PK;
			declaration1.JE_OA_DeclarantAddress = org3.Addresses[0].PK;
			declaration2.JE_OA_DeclarantAddress = org3.Addresses[0].PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var importerOfRecordFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.ImporterOfRecord] as ModuleGuidFilter;
			importerOfRecordFilter.Property = org3.PK;

			AssertGuidFilterMatches(importerOfRecordFilter, bizo => bizo.UBV_OH_Importer, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestSellerFilter

		public void TestSellerFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Seller, FilterCategories.Organisations, "Seller");
		}

		public void TestSellerFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.SellerOrgPK = org1.PK;
			consignment2.SellerOrgPK = org2.PK;
			declaration1.SellerOrgPK = org3.PK;
			declaration2.SellerOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var sellerFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Seller] as ModuleGuidFilter;
			sellerFilter.Property = org2.PK;

			AssertGuidFilterMatches(sellerFilter, bizo => bizo.UBV_OH_Seller, new[] { (consignmentView1, false), (consignmentView2, true), (declarationView1, false), (declarationView2, false) });
		}

		public void TestSellerFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.SellerOrgPK = org1.PK;
			consignment2.SellerOrgPK = org2.PK;
			declaration1.SellerOrgPK = org3.PK;
			declaration2.SellerOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var sellerFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.Seller] as ModuleGuidFilter;
			sellerFilter.Property = org3.PK;

			AssertGuidFilterMatches(sellerFilter, bizo => bizo.UBV_OH_Seller, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#region TestSellerNameFilter

		public void TestSellerNameFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SellerName, FilterCategories.Organisations, "Seller Name");
		}

		public void TestSellerNameFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG1";
			org2.OH_FullName = "ORG2";
			org3.OH_FullName = "ORG3";

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.SellerOrgPK = org1.PK;
			consignment2.SellerOrgPK = org2.PK;
			declaration1.SellerOrgPK = org3.PK;
			declaration2.SellerOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var sellerNameFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SellerName] as ModuleTextFilter;
			sellerNameFilter.Property = "ORG1";

			AssertTextFilterMatches(sellerNameFilter, bizo => bizo.UBV_SellerName, new[] { (consignmentView1, true), (consignmentView2, false), (declarationView1, false), (declarationView2, false) });
		}

		public void TestSellerNameFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG1";
			org2.OH_FullName = "ORG2";
			org3.OH_FullName = "ORG3";

			var consignment1 = CreateValidConsignment();
			var consignment2 = CreateValidConsignment();
			var declaration1 = CreateValidDeclaration();
			var declaration2 = CreateValidDeclaration();

			consignment1.SellerOrgPK = org1.PK;
			consignment2.SellerOrgPK = org2.PK;
			declaration1.SellerOrgPK = org3.PK;
			declaration2.SellerOrgPK = org3.PK;

			Factory.Save();

			var consignmentView1 = Factory.Load<USConsignmentCombined>(consignment1.PK);
			var consignmentView2 = Factory.Load<USConsignmentCombined>(consignment2.PK);
			var declarationView1 = Factory.Load<USConsignmentCombined>(declaration1.PK);
			var declarationView2 = Factory.Load<USConsignmentCombined>(declaration2.PK);

			var filterBizo = GetNewFilterStripBusinessObject();
			var sellerNameFilter = filterBizo[USConsignmentCombinedFilterBusinessObject.FilterIdentifiers.SellerName] as ModuleTextFilter;
			sellerNameFilter.Property = "ORG3";

			AssertTextFilterMatches(sellerNameFilter, bizo => bizo.UBV_SellerName, new[] { (consignmentView1, false), (consignmentView2, false), (declarationView1, true), (declarationView2, true) });
		}

		#endregion

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USConsignmentCombinedFilterBusinessObject();

		void AssertDateFilterMatches(ModuleDateFilter filter, Func<USConsignmentCombined, object> propertyLookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, $"{filter.PropertySearch} [{filter.Property1}, {filter.Property2}]", propertyLookup, expectedMatches);
		}

		void AssertGuidFilterMatches(ModuleGuidFilter filter, Func<USConsignmentCombined, object> propertyLookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertTextFilterMatches(ModuleTextBaseFilter filter, Func<USConsignmentCombined, object> propertyLookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertTextAndNkFilterMatches(ModuleTextAndNkFilter filter, Func<USConsignmentCombined, object> textPropertyLookup, Func<USConsignmentCombined, object> nkPropertyLookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchDoubleFilterProperty(filter, filter.Property, textPropertyLookup, filter.NkProperty, nkPropertyLookup, expectedMatches);
		}

		void AssertLocationFilterMatches(ModuleLocationFilter filter, Func<USConsignmentCombined, object> property1Lookup, Func<USConsignmentCombined, object> property2Lookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchDoubleFilterProperty(filter, filter.Property1, property1Lookup, filter.Property2, property2Lookup, expectedMatches);
		}

		void AssertBizosMatchSingleFilterProperty(ModuleFilter filter, object filterProperty, Func<USConsignmentCombined, object> propertyLookup, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			CombineAssertions(() =>
			{
				expectedMatches.ForEach(expectedMatch =>
					AssertBizoMatchesFilter($"{filter.Description} filter - {filterProperty} matches {propertyLookup(expectedMatch.bizo)}:", filter.Query, expectedMatch.bizo, expectedMatch.shouldMatch)
				);
			});
		}

		void AssertBizosMatchDoubleFilterProperty(ModuleFilter filter, object filterProperty1, Func<USConsignmentCombined, object> propertyLookup1, object filterProperty2, Func<USConsignmentCombined, object> propertyLookup2, IEnumerable<(USConsignmentCombined bizo, bool shouldMatch)> expectedMatches)
		{
			CombineAssertions(() =>
			{
				expectedMatches.ForEach(expectedMatch =>
					AssertBizoMatchesFilter($"{filter.Description} filter - {filterProperty1} matches {propertyLookup1(expectedMatch.bizo)} AND {filterProperty2} matches {propertyLookup2(expectedMatch.bizo)}:", filter.Query, expectedMatch.bizo, expectedMatch.shouldMatch)
				);
			});
		}

		void AssertBizoMatchesFilter(string message, ZQuery query, BusinessObject bizo, bool expectedMatch)
		{
			AssertEquals(message, expectedMatch, bizo.MatchesFilter(query));
		}

		void AssertFilterCategoryAndDescription(string filterIdentifier, FilterCategory expectedCategory, string expectedDescription)
		{
			var filterBizo = GetNewFilterStripBusinessObject();
			var moduleFilter = filterBizo[filterIdentifier];
			AssertNotNull($"{filterIdentifier} - Filter should exist:", moduleFilter);
			CombineAssertions(() =>
			{
				AssertEquals($"{filterIdentifier} - Filter Category:", expectedCategory, moduleFilter.Category);
				AssertEquals($"{filterIdentifier} - Filter Description:", expectedDescription, moduleFilter.MultilingualDescription);
			});
		}

		CusUSLVConsignment CreateValidConsignment()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			return consignment;
		}

		CusEntryNumber CreateCusEntryNumber(BusinessObject parent)
		{
			var cusEntryNum = Factory.NewWithValidTestData<CusEntryNumber>();
			cusEntryNum.CE_ParentID = parent.PK;
			cusEntryNum.CE_ParentTable = parent.TableName;
			cusEntryNum.CE_EntryType = CusEntryNumberTypes.UnitedStates.EntrySummary;
			cusEntryNum.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			return cusEntryNum;
		}

		JobDeclaration CreateValidDeclaration()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.US_EntryType = EntryTypeList.Codes.LowValue;
			jobDeclaration.JE_MessageType = USJobMessageTypeList.Codes.Import;

			return jobDeclaration;
		}

		USCTariff CreateTariff(string tariffNumber, string pgaCodes)
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNumber;
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDate.Today;
			tariff.UE_PGACodes = pgaCodes;
			return tariff;
		}

		#endregion
	}
}
