using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Module.Testing
{
	[TestedType(typeof(CusUSLVClearanceFilterBusinessObject))]
	public class CusUSLVClearanceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Numbers and References

		#region TestJobNumberFilter

		public void TestJobNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.JobNumber, FilterCategories.NumbersAndReferences, "Job Number");
		}

		public void TestJobNumberFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_JobNumber = "AAA";
			clearance2.ULH_JobNumber = "BBB";
			clearance3.ULH_JobNumber = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var jobNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.JobNumber] as ModuleTextFilter;
			jobNumberFilter.Property = "AAA";

			AssertTextFilterMatches(jobNumberFilter, bizo => bizo.ULH_JobNumber, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestJobNumberFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_JobNumber = "AAA";
			clearance2.ULH_JobNumber = "BBB";
			clearance3.ULH_JobNumber = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var jobNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.JobNumber] as ModuleTextFilter;
			jobNumberFilter.Property = "AA";

			AssertTextFilterMatches(jobNumberFilter, bizo => bizo.ULH_JobNumber, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestEntryNumberFilter

		public void TestEntryNumberFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber, FilterCategories.NumbersAndReferences, "Entry Number");
		}

		public void TestEntryNumberFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABD";
			entry3.CE_EntryNum = "XYZ";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "ABC";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestEntryNumberFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABD";
			entry3.CE_EntryNum = "XYZ";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "AB";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, true), (clearance2, true), (clearance3, false) });
		}

		public void TestEntryNumberFilter_OnlyMatchesEntrySummaryMessageType()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.CargoRelease, Constants.CountryCodes.UnitedStates);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABC";
			entry3.CE_EntryNum = "ABC";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "ABC";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestEntryNumberFilter_OnlyMatchesUSContryCode()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedKingdom);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.NewZealand);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABC";
			entry3.CE_EntryNum = "ABC";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "ABC";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, false), (clearance2, false), (clearance3, true) });
		}

		public void TestEntryNumberFilter_OnlyCustomsPermitClearenceCategory()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.InspectionStatus;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABC";
			entry3.CE_EntryNum = "ABC";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "ABC";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestEntryNumberFilter_IgnoresInactiveConsignments()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_IsActive = false;

			var entry1 = CusEntryNumber.New(consignment1, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry2 = CusEntryNumber.New(consignment2, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);
			var entry3 = CusEntryNumber.New(consignment3, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Constants.CountryCodes.UnitedStates);

			entry1.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry2.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			entry3.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;

			entry1.CE_EntryNum = "ABC";
			entry2.CE_EntryNum = "ABC";
			entry3.CE_EntryNum = "ABC";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var cusEntryNumberFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryNumber] as ModuleTextFilter;
			cusEntryNumberFilter.Property = "ABC";

			AssertTextFilterMatches(cusEntryNumberFilter, bizo => bizo.CusUSLVConsignments[0].CE_EntryNum, new[] { (clearance1, false), (clearance2, true), (clearance3, true) });
		}

		#endregion

		#region TestMasterBillFilter

		public void TestMasterBillFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MasterBill, FilterCategories.NumbersAndReferences, "Master Bill");
		}

		public void TestMasterBillFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_MasterBill = "AAA";
			clearance2.ULH_MasterBill = "BBB";
			clearance3.ULH_MasterBill = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var masterBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MasterBill] as ModuleTextFilter;
			masterBillFilter.Property = "AAA";

			AssertTextFilterMatches(masterBillFilter, bizo => bizo.ULH_MasterBill, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestMasterBillFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_MasterBill = "AAA";
			clearance2.ULH_MasterBill = "BBB";
			clearance3.ULH_MasterBill = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var masterBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MasterBill] as ModuleTextFilter;
			masterBillFilter.Property = "AA";

			AssertTextFilterMatches(masterBillFilter, bizo => bizo.ULH_MasterBill, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestHouseBillFilter

		public void TestHouseBillFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.HouseBill, FilterCategories.NumbersAndReferences, "House Bill");
		}

		public void TestHouseBillFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_HouseBill = "AAA";
			consignment2.ULB_HouseBill = "BBB";
			consignment3.ULB_HouseBill = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.HouseBill] as ModuleTextFilter;
			houseBillFilter.Property = "AAA";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_HouseBill, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestHouseBillFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_HouseBill = "AAA";
			consignment2.ULB_HouseBill = "BBB";
			consignment3.ULB_HouseBill = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.HouseBill] as ModuleTextFilter;
			houseBillFilter.Property = "AA";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_HouseBill, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		public void TestHouseBillFilter_IgnoresInactiveConsignments()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_HouseBill = "ABC";
			consignment2.ULB_HouseBill = "ABC";
			consignment3.ULB_HouseBill = "ABC";

			consignment1.ULB_IsActive = false;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.HouseBill] as ModuleTextFilter;
			houseBillFilter.Property = "ABC";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_HouseBill, new[] { (clearance1, false), (clearance2, true), (clearance3, true) });
		}

		#endregion

		#region TestContainerFilter

		public void TestContainerFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Container, FilterCategories.NumbersAndReferences, "Container");
		}

		public void TestContainerFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_EquipmentNumber = "AAA";
			consignment2.ULB_EquipmentNumber = "BBB";
			consignment3.ULB_EquipmentNumber = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Container] as ModuleTextFilter;
			houseBillFilter.Property = "AAA";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_EquipmentNumber, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestContainerFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_EquipmentNumber = "AAA";
			consignment2.ULB_EquipmentNumber = "BBB";
			consignment3.ULB_EquipmentNumber = "AAB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Container] as ModuleTextFilter;
			houseBillFilter.Property = "AA";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_EquipmentNumber, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		public void TestContainerFilter_IgnoresInactiveConsignments()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance1.CusUSLVConsignments.AddNew();
			var consignment2 = clearance2.CusUSLVConsignments.AddNew();
			var consignment3 = clearance3.CusUSLVConsignments.AddNew();

			consignment1.ULB_EquipmentNumber = "ABC";
			consignment2.ULB_EquipmentNumber = "ABC";
			consignment3.ULB_EquipmentNumber = "ABC";

			consignment1.ULB_IsActive = false;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var houseBillFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Container] as ModuleTextFilter;
			houseBillFilter.Property = "ABC";

			AssertTextFilterMatches(houseBillFilter, bizo => bizo.CusUSLVConsignments[0].ULB_EquipmentNumber, new[] { (clearance1, false), (clearance2, true), (clearance3, true) });
		}

		#endregion

		#region MatchingKeyFilter

		public void TestMatchingKeyFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MatchingKey, FilterCategories.NumbersAndReferences, "Matching Key");
		}

		public void TestMatchingKeyFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_MatchingKey = "AAA";
			clearance2.ULH_MatchingKey = "BBB";
			clearance3.ULH_MatchingKey = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var matchingKeyFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MatchingKey] as ModuleTextFilter;
			matchingKeyFilter.Property = "AAA";

			AssertTextFilterMatches(matchingKeyFilter, bizo => bizo.ULH_MatchingKey, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		public void TestMatchingKeyFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_MatchingKey = "AAA";
			clearance2.ULH_MatchingKey = "BBB";
			clearance3.ULH_MatchingKey = "AAB";

			var filterBizo = GetNewFilterStripBusinessObject();
			var matchingKeyFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.MatchingKey] as ModuleTextFilter;
			matchingKeyFilter.Property = "AA";

			AssertTextFilterMatches(matchingKeyFilter, bizo => bizo.ULH_MatchingKey, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#endregion

		#region Modes and Types

		#region TestContainerModeFilter

		public void TestContainerModeFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ContainerMode, FilterCategories.ModesAndTypes, "Container Mode (Customs)");
		}

		public void TestContainerModeFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_ContainerMode = Constants.ContainerModes.Bulk;
			clearance2.ULH_ContainerMode = Constants.ContainerModes.Containerised;
			clearance3.ULH_ContainerMode = Constants.ContainerModes.Bulk;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerModeFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ContainerMode] as ModuleTextFilter;
			containerModeFilter.Property = Constants.ContainerModes.Containerised;

			AssertTextFilterMatches(containerModeFilter, bizo => bizo.ULH_ContainerMode, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestContainerModeFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_ContainerMode = Constants.ContainerModes.Bulk;
			clearance2.ULH_ContainerMode = Constants.ContainerModes.Containerised;
			clearance3.ULH_ContainerMode = Constants.ContainerModes.Bulk;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerModeFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ContainerMode] as ModuleTextFilter;
			containerModeFilter.Property = Constants.ContainerModes.Bulk;

			AssertTextFilterMatches(containerModeFilter, bizo => bizo.ULH_ContainerMode, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestTransportModeFilter

		public void TestTransportModeFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.TransportMode, FilterCategories.ModesAndTypes, "Transport Mode");
		}

		public void TestTransportModeFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_TransportMode = Constants.TransportModes.Air;
			clearance2.ULH_TransportMode = Constants.TransportModes.Sea;
			clearance3.ULH_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerModeFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.TransportMode] as ModuleTextFilter;
			containerModeFilter.Property = Constants.TransportModes.Sea;

			AssertTextFilterMatches(containerModeFilter, bizo => bizo.ULH_TransportMode, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestTransportModeFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_TransportMode = Constants.TransportModes.Air;
			clearance2.ULH_TransportMode = Constants.TransportModes.Sea;
			clearance3.ULH_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var containerModeFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.TransportMode] as ModuleTextFilter;
			containerModeFilter.Property = Constants.TransportModes.Air;

			AssertTextFilterMatches(containerModeFilter, bizo => bizo.ULH_TransportMode, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestVesselFlightVoyageFilter

		public void TestVesselFlightVoyageFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage, FilterCategories.ModesAndTypes, "Vessel and Flight/Voyage #");
		}

		public void TestVesselFlightVoyageFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var clearance4 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_VoyageFlightNo = "111";
			clearance2.ULH_VoyageFlightNo = "222";
			clearance3.ULH_VoyageFlightNo = "333";
			clearance4.ULH_VoyageFlightNo = "111";

			clearance1.ULH_ConveyanceName = "AAA";
			clearance2.ULH_ConveyanceName = "BBB";
			clearance3.ULH_ConveyanceName = "AAA";
			clearance4.ULH_ConveyanceName = "CCC";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var vesselFlightVoyageFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage] as ModuleTextAndNkFilter;
			vesselFlightVoyageFilter.Property = "111";
			vesselFlightVoyageFilter.NkProperty = "AAA";

			AssertTextAndNkFilterMatches(vesselFlightVoyageFilter, bizo => bizo.ULH_VoyageFlightNo, bizo => bizo.ULH_ConveyanceName, new[] { (clearance1, true), (clearance2, false), (clearance3, false), (clearance4, false) });
		}

		public void TestVesselFlightVoyageFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var clearance4 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_VoyageFlightNo = "111";
			clearance2.ULH_VoyageFlightNo = "222";
			clearance3.ULH_VoyageFlightNo = "111";
			clearance4.ULH_VoyageFlightNo = "111";

			clearance1.ULH_ConveyanceName = "AAA";
			clearance2.ULH_ConveyanceName = "AAA";
			clearance3.ULH_ConveyanceName = "AAA";
			clearance4.ULH_ConveyanceName = "BBB";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var vesselFlightVoyageFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.VesselFlightVoyage] as ModuleTextAndNkFilter;
			vesselFlightVoyageFilter.Property = "111";
			vesselFlightVoyageFilter.NkProperty = "AAA";

			AssertTextAndNkFilterMatches(vesselFlightVoyageFilter, bizo => bizo.ULH_VoyageFlightNo, bizo => bizo.ULH_ConveyanceName, new[] { (clearance1, true), (clearance2, false), (clearance3, true), (clearance4, false) });
		}

		#endregion

		#endregion

		#region Locations

		#region TestDischargePortFilter

		public void TestDischargePortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargePort, FilterCategories.Locations, "Discharge Port");
		}

		public void TestDischargePortFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfDischarge = "3786";
			clearance2.ULH_PortOfDischarge = "1234";
			clearance3.ULH_PortOfDischarge = "3787";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargePortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargePort] as ModuleNkFilter;
			dischargePortFilter.Property = "1234";

			AssertTextFilterMatches(dischargePortFilter, bizo => bizo.ULH_PortOfDischarge, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestDischargePortFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfDischarge = "3786";
			clearance2.ULH_PortOfDischarge = "1234";
			clearance3.ULH_PortOfDischarge = "3786";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var dischargePortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargePort] as ModuleNkFilter;
			dischargePortFilter.Property = "3786";

			AssertTextFilterMatches(dischargePortFilter, bizo => bizo.ULH_PortOfDischarge, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestEntryPortFilter

		public void TestEntryPortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryPort, FilterCategories.Locations, "Entry Port");
		}

		public void TestEntryPortFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfEntry = "3786";
			clearance2.ULH_PortOfEntry = "1234";
			clearance3.ULH_PortOfEntry = "3787";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryPortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryPort] as ModuleNkFilter;
			entryPortFilter.Property = "1234";

			AssertTextFilterMatches(entryPortFilter, bizo => bizo.ULH_PortOfEntry, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestEntryPortFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfEntry = "3786";
			clearance2.ULH_PortOfEntry = "1234";
			clearance3.ULH_PortOfEntry = "3786";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryPortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.EntryPort] as ModuleNkFilter;
			entryPortFilter.Property = "3786";

			AssertTextFilterMatches(entryPortFilter, bizo => bizo.ULH_PortOfEntry, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestLoadingPortFilter

		public void TestLoadingPortFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadingPort, FilterCategories.Locations, "Loading Port");
		}

		public void TestLoadingPortFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfLoading = "4901";
			clearance2.ULH_PortOfLoading = "1234";
			clearance3.ULH_PortOfLoading = "4901";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadingPortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadingPort] as ModuleNkFilter;
			loadingPortFilter.Property = "1234";

			AssertTextFilterMatches(loadingPortFilter, bizo => bizo.ULH_PortOfLoading, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestLoadingPortFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_PortOfLoading = "4901";
			clearance2.ULH_PortOfLoading = "1234";
			clearance3.ULH_PortOfLoading = "4901";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadingPortFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadingPort] as ModuleNkFilter;
			loadingPortFilter.Property = "4901";

			AssertTextFilterMatches(loadingPortFilter, bizo => bizo.ULH_PortOfLoading, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestLoadDischargeUNLOCOFilter

		public void TestLoadDischargeUNLOCOFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO, FilterCategories.Locations, "Load/Discharge");
		}

		public void TestLoadDischargeUNLOCOFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var clearance4 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_RL_NKPortOfLoading = "AAA";
			clearance2.ULH_RL_NKPortOfLoading = "BBB";
			clearance3.ULH_RL_NKPortOfLoading = "CCC";
			clearance4.ULH_RL_NKPortOfLoading = "AAA";

			clearance1.ULH_RL_NKPortOfDischarge = "XXX";
			clearance2.ULH_RL_NKPortOfDischarge = "YYY";
			clearance3.ULH_RL_NKPortOfDischarge = "XXX";
			clearance4.ULH_RL_NKPortOfDischarge = "ZZZ";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadDischargeUNLOCOFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO] as ModuleLocationFilter;
			loadDischargeUNLOCOFilter.Property1 = "AAA";
			loadDischargeUNLOCOFilter.Property2 = "XXX";

			AssertLocationFilterMatches(loadDischargeUNLOCOFilter, bizo => bizo.ULH_RL_NKPortOfLoading, bizo => bizo.ULH_RL_NKPortOfDischarge, new[] { (clearance1, true), (clearance2, false), (clearance3, false), (clearance4, false) });
		}

		public void TestLoadDischargeUNLOCOFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();
			var clearance4 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_RL_NKPortOfLoading = "AAA";
			clearance2.ULH_RL_NKPortOfLoading = "BBB";
			clearance3.ULH_RL_NKPortOfLoading = "AAA";
			clearance4.ULH_RL_NKPortOfLoading = "AAA";

			clearance1.ULH_RL_NKPortOfDischarge = "XXX";
			clearance2.ULH_RL_NKPortOfDischarge = "XXX";
			clearance3.ULH_RL_NKPortOfDischarge = "XXX";
			clearance4.ULH_RL_NKPortOfDischarge = "ZZZ";

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var loadDischargeUNLOCOFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LoadDiscargeUNLOCO] as ModuleLocationFilter;
			loadDischargeUNLOCOFilter.Property1 = "AAA";
			loadDischargeUNLOCOFilter.Property2 = "XXX";

			AssertLocationFilterMatches(loadDischargeUNLOCOFilter, bizo => bizo.ULH_RL_NKPortOfLoading, bizo => bizo.ULH_RL_NKPortOfDischarge, new[] { (clearance1, true), (clearance2, false), (clearance3, true), (clearance4, false) });
		}

		#endregion

		#endregion

		#region Organisations

		#region TestImporterFilter

		public void TestImporterFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Importer, FilterCategories.Organisations, "Importer of Record");
		}

		public void TestImporterFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_OH_Importer = org1.PK;
			clearance2.ULH_OH_Importer = org2.PK;
			clearance3.ULH_OH_Importer = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var importerFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Importer] as ModuleGuidFilter;
			importerFilter.Property = org2.PK;

			AssertGuidFilterMatches(importerFilter, bizo => bizo.ULH_OH_Importer, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestImporterFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_OH_Importer = org1.PK;
			clearance2.ULH_OH_Importer = org2.PK;
			clearance3.ULH_OH_Importer = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var importerFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.Importer] as ModuleGuidFilter;
			importerFilter.Property = org1.PK;

			AssertGuidFilterMatches(importerFilter, bizo => bizo.ULH_OH_Importer, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#region TestLocalClientFilter

		public void TestLocalClientFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LocalClient, FilterCategories.Organisations, "Local Client");
		}

		public void TestLocalClientFilter_SingleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_OH_Client = org1.PK;
			clearance2.ULH_OH_Client = org2.PK;
			clearance3.ULH_OH_Client = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var localClientFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LocalClient] as ModuleGuidFilter;
			localClientFilter.Property = org2.PK;

			AssertGuidFilterMatches(localClientFilter, bizo => bizo.ULH_OH_Client, new[] { (clearance1, false), (clearance2, true), (clearance3, false) });
		}

		public void TestLocalClientFilter_MultipleMatch()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_OH_Client = org1.PK;
			clearance2.ULH_OH_Client = org2.PK;
			clearance3.ULH_OH_Client = org1.PK;

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var localClientFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.LocalClient] as ModuleGuidFilter;
			localClientFilter.Property = org1.PK;

			AssertGuidFilterMatches(localClientFilter, bizo => bizo.ULH_OH_Client, new[] { (clearance1, true), (clearance2, false), (clearance3, true) });
		}

		#endregion

		#endregion

		#region Dates

		#region TestEntryDateFilter

		public void TestEntryDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ArrivalDate, FilterCategories.Dates, "Arrival Date");
		}

		[TestDate(2020, 01, 01)]
		public void TestEntryDateFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_EntryDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_EntryDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_EntryDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ArrivalDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_EntryDate, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestEntryDateFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_EntryDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_EntryDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_EntryDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.ArrivalDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-14);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_EntryDate, new[] { (clearance1, true), (clearance2, true), (clearance3, false) });
		}

		#endregion

		#region TestDischargeDateFilter

		public void TestDischargeDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargeDate, FilterCategories.Dates, "Discharge Date");
		}

		[TestDate(2020, 01, 01)]
		public void TestDischargeDateFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_DischargeDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_DischargeDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_DischargeDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargeDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_DischargeDate, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestDischargeDateFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_DischargeDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_DischargeDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_DischargeDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DischargeDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-14);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_DischargeDate, new[] { (clearance1, true), (clearance2, true), (clearance3, false) });
		}

		#endregion

		#region TestDepartureDateFilter

		public void TestDepartureDateFilter_CategoryAndDescription()
		{
			AssertFilterCategoryAndDescription(CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DepartureDate, FilterCategories.Dates, "Departure Date");
		}

		[TestDate(2020, 01, 01)]
		public void TestDepartureDateFilter_SingleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_DepartureDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_DepartureDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_DepartureDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DepartureDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-7);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_DepartureDate, new[] { (clearance1, true), (clearance2, false), (clearance3, false) });
		}

		[TestDate(2020, 01, 01)]
		public void TestDepartureDateFilter_MultipleMatch()
		{
			var clearance1 = Factory.New<CusUSLVClearance>();
			var clearance2 = Factory.New<CusUSLVClearance>();
			var clearance3 = Factory.New<CusUSLVClearance>();

			clearance1.ULH_DepartureDate = ZDate.Today.AddDays(-5);
			clearance2.ULH_DepartureDate = ZDate.Today.AddDays(-10);
			clearance3.ULH_DepartureDate = ZDate.Today.AddDays(-100);

			Factory.Save();

			var filterBizo = GetNewFilterStripBusinessObject();
			var entryDateFilter = filterBizo[CusUSLVClearanceFilterBusinessObject.FilterIdentifiers.DepartureDate] as ModuleDateFilter;
			entryDateFilter.Property1 = ZDateTime.Today.AddDays(-14);
			entryDateFilter.Property2 = ZDateTime.Today;
			entryDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			AssertDateFilterMatches(entryDateFilter, bizo => bizo.ULH_DepartureDate, new[] { (clearance1, true), (clearance2, true), (clearance3, false) });
		}

		#endregion

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusUSLVClearanceFilterBusinessObject();

		void AssertDateFilterMatches(ModuleDateFilter filter, Func<CusUSLVClearance, object> propertyLookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, $"{filter.PropertySearch} [{filter.Property1}, {filter.Property2}]", propertyLookup, expectedMatches);
		}

		void AssertGuidFilterMatches(ModuleGuidFilter filter, Func<CusUSLVClearance, object> propertyLookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertTextFilterMatches(ModuleTextBaseFilter filter, Func<CusUSLVClearance, object> propertyLookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchSingleFilterProperty(filter, filter.Property, propertyLookup, expectedMatches);
		}

		void AssertTextAndNkFilterMatches(ModuleTextAndNkFilter filter, Func<CusUSLVClearance, object> textPropertyLookup, Func<CusUSLVClearance, object> nkPropertyLookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchDoubleFilterProperty(filter, filter.Property, textPropertyLookup, filter.NkProperty, nkPropertyLookup, expectedMatches);
		}

		void AssertLocationFilterMatches(ModuleLocationFilter filter, Func<CusUSLVClearance, object> property1Lookup, Func<CusUSLVClearance, object> property2Lookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			AssertBizosMatchDoubleFilterProperty(filter, filter.Property1, property1Lookup, filter.Property2, property2Lookup, expectedMatches);
		}

		void AssertBizosMatchSingleFilterProperty(ModuleFilter filter, object filterProperty, Func<CusUSLVClearance, object> propertyLookup, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
		{
			CombineAssertions(() =>
			{
				expectedMatches.ForEach(expectedMatch =>
					AssertBizoMatchesFilter($"{filter.Description} filter - {filterProperty} matches {propertyLookup(expectedMatch.bizo)}:", filter.Query, expectedMatch.bizo, expectedMatch.shouldMatch)
				);
			});
		}

		void AssertBizosMatchDoubleFilterProperty(ModuleFilter filter, object filterProperty1, Func<CusUSLVClearance, object> propertyLookup1, object filterProperty2, Func<CusUSLVClearance, object> propertyLookup2, IEnumerable<(CusUSLVClearance bizo, bool shouldMatch)> expectedMatches)
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

		#endregion
	}
}
