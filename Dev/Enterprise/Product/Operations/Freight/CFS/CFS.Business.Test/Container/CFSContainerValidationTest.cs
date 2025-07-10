using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerValidationTest : BaseFreightTest
	{
		public void TestValidateJC_Purpose()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			container.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			container.Validation.ValidateJC_Purpose();
			AssertEquals(false, container.JC_PurposeInfo.HasErrors());

			container.JC_Purpose = "CRP";
			container.Validation.ValidateJC_Purpose();
			AssertEquals(true, container.JC_PurposeInfo.HasErrors());
		}

		#region TestValidatePackUnPackStatus

		public void TestValidatePackUnPackStatus()
		{
			string originalHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			try
			{
				string branchError = "Please edit your branch details to add a home port for your branch.";
				string distinctPortError = "The load and discharge ports cannot be the same.";

				CFSContainer container = Factory.New<CFSContainer>();
				CFSContainerValidation validation = new CFSContainerValidation(container);

				validation.ValidateAll();
				AssertNoErrors(container.JC_JXInfo);

				container.JC_JX = GetSailing(HomePort, OverseasPort).PK;
				validation.ValidateAll();

				AssertNoErrors(container.JC_JXInfo);
				AssertNoErrors(container.JC_JA_NKPortOfLoadingInfo);
				AssertNoErrors(container.JC_JB_NKPortOfDischargeInfo);

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "";
				container.JC_JX = GetSailing(OverseasPort, OverseasPort2).PK;
				validation.ValidateAll();
				AssertHasError(container.JC_JXInfo, branchError);
				AssertHasError(container.JC_JA_NKPortOfLoadingInfo, branchError);
				AssertHasError(container.JC_JB_NKPortOfDischargeInfo, branchError);
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalHomePort;

				container.JC_JX = GetSailing(HomePort, HomePort).PK;
				validation.ValidateAll();
				AssertHasError(container.JC_JXInfo, distinctPortError);
				AssertHasError(container.JC_JA_NKPortOfLoadingInfo, distinctPortError);
				AssertHasError(container.JC_JB_NKPortOfDischargeInfo, distinctPortError);
			}
			finally
			{
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = originalHomePort;
			}
		}

		#endregion

		#region GetSailing

		JobSailing GetSailing(ZString load, ZString discharge)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = load;
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = discharge;
			voyage.GenerateSailings();

			if (voyage.Sailings.Count > 0)
			{
				return voyage.Sailings[0];
			}
			else
			{
				JobSailing sailing = Factory.New<JobSailing>();
				sailing.JX_JA = voyage.Origins[0].PK;
				sailing.JX_JB = voyage.Destinations[0].PK;

				return sailing;
			}
		}

		#endregion

		#region TestCheckJC_FCLAvailable

		public void TestCheckJC_FCLAvailable()
		{
			CFSContainer cFSContainer = Factory.New<CFSContainer>();
			cFSContainer.Validation.ValidateJC_FCLAvailable();
			AssertEquals(false, cFSContainer.JC_ArrivalCTOStorageStartDateInfo.HasErrors());

			cFSContainer.JC_ArrivalTime = ZDateTime.Now;
			cFSContainer.JC_FCLAvailable = ZDateTime.Now;
			cFSContainer.Validation.ValidateJC_FCLAvailable();
			AssertEquals(false, cFSContainer.JC_ArrivalCTOStorageStartDateInfo.HasErrors());

			cFSContainer.JC_ArrivalTime = ZDateTime.Now;
			cFSContainer.JC_FCLAvailable = ZDateTime.Now.AddDays(1);
			cFSContainer.Validation.ValidateJC_FCLAvailable();
			AssertEquals(true, cFSContainer.JC_FCLAvailableInfo.HasWarnings());
		}

		#endregion

		#region TestValidateJC_LCLUnpack

		public void TestValidateJC_LCLUnpack()
		{
			TallyContainer container = Factory.New<TallyContainer>();
			CFSShipment shipment = container.PackUnpackShipments.AddNew();
			CFSPackLine pack = null;
			if (shipment.OuterPackLines.Count > 0)
			{
				pack = shipment.OuterPackLines[0];
			}
			else
			{
				pack = shipment.OuterPackLines.AddNew();
			}

			AssertEquals("Pack.PackageCount", 0, pack.JL_PackageCount);
			AssertEquals("Pack.Outturn", 0, pack.JL_Outturn);

			container.Validation.ValidateJC_LCLUnpack();
			AssertEquals("JC_LCLUnpackInfo.HasNotifications", false, container.JC_LCLUnpackInfo.HasNotifications());

			pack.JL_Outturn = 1;
			//Factory.Save();
			AssertEquals("Container PackLine Outturn should reflect the Shipments PackLine", 1, container.PackLines[0].JL_Outturn);
			container.JC_LCLUnpack = ZDateTime.Empty;
			AssertEquals("should warn on outturn without unpack date", true, container.JC_LCLUnpackInfo.HasNotifications());

			pack.JL_Outturn = 0;
			container.Validation.ValidateJC_LCLUnpack();
			AssertEquals("should have removed warning", false, container.JC_LCLUnpackInfo.HasNotifications());

			pack.JL_OutturnComment = "foobah";
			container.Validation.ValidateJC_LCLUnpack();
			AssertEquals("should warn on outturn without unpack date", true, container.JC_LCLUnpackInfo.HasNotifications());

			pack.JL_OutturnComment = "";
			pack.JL_PackageCount = 5;
			container.Validation.ValidateJC_LCLUnpack();
			AssertEquals("JC_LCLUnpackInfo.HasNotifications", false, container.JC_LCLUnpackInfo.HasNotifications());

			pack.JL_Outturn = 3;
			container.JC_LCLUnpack = ZDateTime.Empty;
			container.Validation.ValidateJC_LCLUnpack();
			AssertEquals("JC_LCLUnpackInfo.HasWarnings()", true, container.JC_LCLUnpackInfo.HasWarnings());

			container.JC_LCLUnpack = ZDateTime.Now;
			AssertEquals("JC_LCLUnpackInfo.HasNotifications", false, container.JC_LCLUnpackInfo.HasNotifications());
		}

		#endregion

		#region TestValidateJC_StorageCommences

		public void TestValidateJC_StorageCommences()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_LCLAvailable = ZDateTime.Now;
			container.JC_LCLStorageCommences = ZDateTime.Empty;
			container.RunPreSaveValidation();
			AssertEquals("JC_StorageCommences must greater then availability date when availability set", true, container.JC_LCLStorageCommencesInfo.HasNotifications());

			container.JC_LCLAvailable = ZDateTime.Empty;
			container.RunPreSaveValidation();
			AssertEquals("JC_StorageCommences expects no error", false, container.JC_LCLStorageCommencesInfo.HasNotifications());

			container.JC_LCLAvailable = ZDateTime.Now;
			container.JC_LCLStorageCommences = ZDateTime.Now.AddDays(-1);
			container.RunPreSaveValidation();
			AssertEquals("JC_StorageCommences must greater then availability date when availability set", true, container.JC_LCLStorageCommencesInfo.HasNotifications());
		}

		#endregion

		#region TestValidateJC_LCLAvailable

		public void TestValidateJC_LCLAvailable()
		{
			var now = ZDateTime.Now;

			CFSContainer container = Factory.New<CFSContainer>();
			container.CFSDispatch.EU_PickupDeliveryTime = now;
			container.JC_LCLAvailable = ZDateTime.Empty;
			container.RunPreSaveValidation();
			AssertEquals("No errors should stop the saving of the Container Record on LCLAvailability", false, container.JC_LCLAvailableInfo.HasErrors());

			container.JC_LCLUnpack = now;
			container.JC_LCLAvailable = now;
			container.RunPreSaveValidation();
			AssertEquals("Should have no errors if Unpack Date and Available Date are the same time", false, container.JC_LCLAvailableInfo.HasErrors());

			container.JC_LCLUnpack = now.AddDays(2);
			container.JC_LCLAvailable = now;
			container.RunPreSaveValidation();
			AssertEquals("CFSContainer Available Date should have an error if before Unpack Date", true, container.JC_LCLAvailableInfo.HasErrors());

			container.JC_LCLUnpack = now.AddDays(-1);
			container.JC_LCLAvailable = now;
			container.RunPreSaveValidation();
			AssertEquals("Should have no error if Available Date comes after Unpack Date", false, container.JC_LCLAvailableInfo.HasErrors());

			container.JC_LCLUnpack = ZDateTime.Empty;
			container.JC_LCLAvailable = now;
			container.RunPreSaveValidation();
			AssertEquals("Should have no error if if Unpack Date is removed", false, container.JC_LCLAvailableInfo.HasErrors());
		}

		#endregion

		#region TestJY_RH_NKContainerCommodityCodeDontHaveErrorsForInvalidCodes

		public void TestJY_RH_NKContainerCommodityCodeDontHaveErrorsForInvalidCodes()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			container.JC_RH_NKContainerCommodityCode = "INVD";
			AssertNoErrors("Should not have any errors", container.JC_RH_NKContainerCommodityCodeInfo);
		}

		#endregion

		#region TestValidateJC_ContainerNum_VGM

		public void TestValidateVGMJC_ContainerNum()
		{
			var container = Factory.New<CFSContainer>();
			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container.JC_ContainerNum = "";
			container.Validation.ValidateJC_ContainerNum();
			AssertHasError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.NotVerified;
			container.Validation.ValidateJC_ContainerNum();
			AssertNoError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");

			container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method2Packages;
			container.Validation.ValidateJC_ContainerNum();
			AssertHasError(container.JC_ContainerNumInfo, "Container number is blank. Container number is required for gross weight verification. Enter the container number or change VGM Verification Method to NON.");
		}
		#endregion

		#region TestValidateJC_ContainerMode

		public void TestValidateJC_ContainerMode()
		{
			Container.JC_ContainerMode = Constants.ContainerModes.AIR;
			Container.JC_TransportMode = Constants.TransportModes.Air;
			AssertHasErrors(container.JC_ContainerModeInfo);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var reloadedContainer = anotherFactory.Load<CFSContainer>(Container.PK);
			reloadedContainer.JC_TransportMode = Constants.TransportModes.Air;

			reloadedContainer.Validation.ValidateJC_ContainerMode();
			AssertNoErrors(reloadedContainer.JC_ContainerModeInfo);

			reloadedContainer.JC_ContainerMode = Constants.ContainerModes.ULD;
			AssertNoErrors(reloadedContainer.JC_ContainerModeInfo);

			anotherFactory.Save();

			reloadedContainer.JC_ContainerMode = Constants.ContainerModes.AIR;
			AssertHasErrors(reloadedContainer.JC_ContainerModeInfo);
		}

		#endregion

		#region TestErrorInfoWhenDuplicatedContainerNum

		public void TestErrorInfoWhenDuplicatedContainerNum()
		{
			var consol = Factory.New<CommonConsol>();

			var container1 = Factory.New<CFSContainer>();
			container1.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container1.JC_JK = consol.PK;
			container1.JC_ContainerNum = "FAAB6159341";

			var container2 = Factory.New<CFSContainer>();
			container2.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container2.JC_JK = consol.PK;
			container2.JC_ContainerNum = "FAAB6159341";

			AssertHasError(container2.JC_ContainerNumInfo, "Duplicate Container Number is entered.");

			var sailing = Factory.New<JobSailing>();
			var container3 = Factory.New<CFSContainer>();
			container3.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container3.JC_JX = sailing.PK;
			container3.JC_ContainerNum = "FAAB6159342";

			var container4 = Factory.New<CFSContainer>();
			container4.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container;
			container4.JC_JX = sailing.PK;
			container4.JC_ContainerNum = "FAAB6159342";

			AssertHasError(container4.JC_ContainerNumInfo, "Duplicate Container Number is entered.");
		}

		#endregion

		#region Validate Date Range

		public void TestValidDateRangeJC_LCLUnpack()
		{
			TestValidateDateRange(JobContainerSchema.JC_LCLUnpack.Name, Container.JC_LCLUnpackInfo);
		}

		public void TestValidDateRangeJC_LCLAvailable()
		{
			TestValidateDateRange(JobContainerSchema.JC_LCLAvailable.Name, Container.JC_LCLAvailableInfo);
		}

		public void TestValidDateRangeJC_LCLStorageCommences()
		{
			TestValidateDateRange(JobContainerSchema.JC_LCLStorageCommences.Name, Container.JC_LCLStorageCommencesInfo);
		}

		void TestValidateDateRange(string propertyToTest, ZPropertyInfo infoToTest)
		{
			ZDateTime today = ZDateTime.Today;

			Container[propertyToTest] = ZDateTime.Empty;
			Container.RunPreSaveValidation();

			AssertNoErrors("Pre-condition: there should be no warning or errors on this date", infoToTest);

			Container[propertyToTest] = today.AddYears(5).AddDays(1);
			Container.RunPreSaveValidation();

			AssertHasErrors("Should have an error as the date from today is more than 5 years into the future", infoToTest);

			Container[propertyToTest] = today.AddYears(5).AddDays(-1);
			Container.RunPreSaveValidation();

			AssertNoErrors("Should no errors a warning as the date from today is less than 5 years into the future from today", infoToTest);
			AssertHasWarnings("Should have a warning as the date from today is more than a year in the future", infoToTest);

			Container[propertyToTest] = today;
			Container.RunPreSaveValidation();

			AssertNoErrors("Should have no errors as a valid date is entered", infoToTest);
			AssertNoWarnings("Should have no warnings as a valid date is entered", infoToTest);

			Container[propertyToTest] = today.AddYears(-11);
			Container.RunPreSaveValidation();

			AssertHasErrors("Should have an error as the date from today is more than 10 years into the past", infoToTest);

			Container[propertyToTest] = ZDateTime.Invalid;
			Container.RunPreSaveValidation();

			AssertEquals("Should show an invalid date, ZDateTime.Invalid", ZDateTime.Invalid, Container[propertyToTest]);
			AssertHasErrors("Should have an error as the date is invalid", infoToTest);
		}

		#endregion

		#region Implementation

		CFSContainer Container
		{
			get { return container ?? (container = Factory.New<CFSContainer>()); }
		}
		CFSContainer container;

		#endregion
	}
}
