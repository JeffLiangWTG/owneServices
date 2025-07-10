using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Rating;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RatingDocumentsChargeGroupingOrRollupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateModule()
		{
			var organisation = Factory.New<OrgHeader>();
			var collection = new RatingDocumentsChargeGroupingOrRollupCollection(organisation.CompanyData);

			var firstRecord = PreValidationRecordWithCorrectValues_1(collection);
			AssertNoErrors(firstRecord.RCG_ModuleInfo);

			var secondRecord = PreValidationRecordWithCorrectValues_2(collection);
			AssertNoErrors(secondRecord.RCG_ModuleInfo);

			var thirdRecord = collection.AddNew();

			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RCG_Module = ZString.Empty;
			thirdRecord.RunPreSaveValidation();
			Assert("Module: Mandatory Validation", thirdRecord.RCG_ModuleInfo.HasError("Please enter a value."));

			thirdRecord.RCG_Module = "ABC";
			thirdRecord.RunPreSaveValidation();
			Assert("Module: List Validation", thirdRecord.RCG_ModuleInfo.HasError("Enter a valid selection."));

			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RunPreSaveValidation();
			AssertNoErrors(thirdRecord.RCG_ModuleInfo);
		}

		public void TestValidateJobType()
		{
			var organisation = Factory.New<OrgHeader>();
			var collection = new RatingDocumentsChargeGroupingOrRollupCollection(organisation.CompanyData);

			var firstRecord = PreValidationRecordWithCorrectValues_1(collection);
			AssertNoErrors(firstRecord.RCG_JobTypeInfo);

			var secondRecord = PreValidationRecordWithCorrectValues_2(collection);
			AssertNoErrors(secondRecord.RCG_JobTypeInfo);

			var thirdRecord = collection.AddNew();

			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.RailFreight;
			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RCG_JobType = ZString.Empty;
			thirdRecord.RunPreSaveValidation();
			AssertHasError(thirdRecord.RCG_JobTypeInfo, "Please enter a value.");

			thirdRecord.RCG_JobType = "ABC";
			thirdRecord.RunPreSaveValidation();
			AssertHasError(thirdRecord.RCG_JobTypeInfo, "Enter a valid selection.");

			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			thirdRecord.RunPreSaveValidation();
			AssertNoErrors(thirdRecord.RCG_JobTypeInfo);
		}

		public void TestValidateTransportMode()
		{
			var organisation = Factory.New<OrgHeader>();
			var collection = new RatingDocumentsChargeGroupingOrRollupCollection(organisation.CompanyData);

			var firstRecord = PreValidationRecordWithCorrectValues_1(collection);
			AssertNoErrors(firstRecord.RCG_TransportModeInfo);

			var secondRecord = PreValidationRecordWithCorrectValues_2(collection);
			AssertNoErrors(secondRecord.RCG_TransportModeInfo);

			var thirdRecord = collection.AddNew();

			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.CFS;
			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RCG_TransportMode = ZString.Empty;
			thirdRecord.RunPreSaveValidation();
			AssertHasError(thirdRecord.RCG_TransportModeInfo, "Please enter a value.");

			thirdRecord.RCG_TransportMode = "ABC";
			thirdRecord.RunPreSaveValidation();
			AssertHasError(thirdRecord.RCG_TransportModeInfo, "Enter a valid selection.");
			Assert("TransportMode: List Validation", thirdRecord.RCG_TransportModeInfo.HasError("Enter a valid selection."));
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.AirFreight;
			thirdRecord.RunPreSaveValidation();
			AssertNoErrors(thirdRecord.RCG_TransportModeInfo);
		}

		public void TestValidateDisplay()
		{
			var organisation = Factory.New<OrgHeader>();
			var collection = new RatingDocumentsChargeGroupingOrRollupCollection(organisation.CompanyData);

			var firstRecord = PreValidationRecordWithCorrectValues_1(collection);
			AssertNoErrors(firstRecord.RCG_DisplayInfo);

			var secondRecord = PreValidationRecordWithCorrectValues_2(collection);
			AssertNoErrors(secondRecord.RCG_DisplayInfo);

			var thirdRecord = collection.AddNew();
			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.LinerAndAgency;
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RCG_Display = ZString.Empty;
			thirdRecord.RunPreSaveValidation();
			Assert("Display: Mandatory Validation", thirdRecord.RCG_DisplayInfo.HasError("Please enter a value."));

			thirdRecord.RCG_Display = "ABC";
			thirdRecord.RunPreSaveValidation();
			Assert("Display: List Validation", thirdRecord.RCG_DisplayInfo.HasError("Enter a valid selection."));

			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			thirdRecord.RunPreSaveValidation();
			AssertNoErrors(thirdRecord.RCG_DisplayInfo);
		}

		public void TestValidateStyle()
		{
			var organisation = Factory.New<OrgHeader>();
			var collection = new RatingDocumentsChargeGroupingOrRollupCollection(organisation.CompanyData);

			var firstRecord = PreValidationRecordWithCorrectValues_1(collection);
			AssertNoErrors(firstRecord.RCG_StyleInfo);

			var secondRecord = PreValidationRecordWithCorrectValues_2(collection);
			AssertNoErrors(secondRecord.RCG_StyleInfo);

			var thirdRecord = collection.AddNew();
			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.LinerAndAgency;
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			thirdRecord.RCG_Style = ZString.Empty;
			thirdRecord.RunPreSaveValidation();
			Assert("Style: Mandatory Validation", thirdRecord.RCG_StyleInfo.HasError("Please enter a value."));

			thirdRecord.RCG_Style = "ABC";
			thirdRecord.RunPreSaveValidation();
			Assert("Style: List Validation", thirdRecord.RCG_StyleInfo.HasError("Enter a valid selection."));

			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RunPreSaveValidation();
			AssertNoErrors(thirdRecord.RCG_StyleInfo);
		}

		RatingDocumentsChargeGroupingOrRollup PreValidationRecordWithCorrectValues_1(RatingDocumentsChargeGroupingOrRollupCollection collection)
		{
			var record = collection.AddNew();
			record.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			record.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			record.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			record.RCG_Display = DocRollupOrSortDisplayList.Codes.Default;
			record.RCG_Style = DocRollupOrSortStyleList.Codes.Default;
			record.RunPreSaveValidation();
			return record;
		}

		RatingDocumentsChargeGroupingOrRollup PreValidationRecordWithCorrectValues_2(RatingDocumentsChargeGroupingOrRollupCollection collection)
		{
			var record = collection.AddNew();
			record.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			record.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			record.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.SeaFreight;
			record.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			record.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			record.RunPreSaveValidation();
			return record;
		}

		public void TestValidateDuplicateLines()
		{
			var organisation = Factory.New<OrgHeader>();

			var collection = organisation.CompanyData.RatingDocRollupOrGroups;

			collection.RemoveAndDeleteAll(); // remove data that is created by CompanyData.SetRatingDocRollupOrGroupsDefaults

			var defaultRecord = collection.AddNew();

			defaultRecord.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			defaultRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			defaultRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			defaultRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Alphabetical;
			defaultRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			defaultRecord.RunPreSaveValidation();
			AssertNoErrors(defaultRecord);

			var secondRecord = collection.AddNew(); // New record with same Module, JobType and TransportMode
			secondRecord.RCG_Module = DocRollupOrSortModuleList.Codes.All;
			secondRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.All;
			secondRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			secondRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Sequence; // New Display
			secondRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			secondRecord.RunPreSaveValidation();

			AssertHasError(secondRecord.RCG_ModuleInfo, "Duplicate configurations with the same Module, Job Type and Mode is NOT allowed.");

			secondRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			secondRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			secondRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			secondRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Sequence; // New Display
			secondRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			secondRecord.RunPreSaveValidation();

			AssertNoErrors(secondRecord);

			var thirdRecord = collection.AddNew();
			thirdRecord.RCG_Module = DocRollupOrSortModuleList.Codes.Quotations;
			thirdRecord.RCG_JobType = DocRollupOrSortJobTypeList.Codes.Forwarding;
			thirdRecord.RCG_TransportMode = DocRollupOrSortTransportModeList.Codes.All;
			thirdRecord.RCG_Display = DocRollupOrSortDisplayList.Codes.Sequence;
			thirdRecord.RCG_Style = DocRollupOrSortStyleList.Codes.NoGrouping;
			thirdRecord.RunPreSaveValidation();

			AssertHasError(thirdRecord.RCG_ModuleInfo, "Duplicate configurations with the same Module, Job Type and Mode is NOT allowed.");
		}
	}
}
