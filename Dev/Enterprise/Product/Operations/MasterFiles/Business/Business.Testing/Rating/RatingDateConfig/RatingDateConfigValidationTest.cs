using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	public class RatingDateConfigValidationTest : TestCaseWithFactory
	{
		public void TestValidateDate()
		{
			RatingDateConfig.RDT_AutoratingDate = ZString.Empty;
			AssertHasErrors(RatingDateConfig.RDT_AutoratingDateInfo);

			RatingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;
			AssertNoErrors(RatingDateConfig.RDT_AutoratingDateInfo);

			RatingDateConfig.RDT_AutoratingDate = "XXX";
			AssertHasErrors(RatingDateConfig.RDT_AutoratingDateInfo);
		}

		public void TestValidateLocation()
		{
			RatingDateConfig.RDT_Location = ZString.Empty;
			AssertNoErrors(RatingDateConfig.RDT_LocationInfo);

			RatingDateConfig.RDT_Location = "AUSYD";
			AssertNoErrors(RatingDateConfig.RDT_LocationInfo);

			RatingDateConfig.RDT_Location = "ZZ";
			AssertHasErrors(RatingDateConfig.RDT_LocationInfo);
		}

		public void TestDuplicateAutoRateDateSetup()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var ratingDateConfig1 = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			var ratingDateConfig2 = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			var ratingDateConfig3 = orgHeader.MiscServ.RatingDateConfigs.AddNew();

			ratingDateConfig1.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig2.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			ratingDateConfig3.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;

			ratingDateConfig1.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			ratingDateConfig2.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			ratingDateConfig3.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Export;

			ratingDateConfig1.RDT_TransportMode = Core.Constants.TransportModes.Sea;
			ratingDateConfig2.RDT_TransportMode = Core.Constants.TransportModes.Sea;
			ratingDateConfig3.RDT_TransportMode = Core.Constants.TransportModes.Sea;

			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.All;
			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.All;
			ratingDateConfig3.RDT_RateType = JobRateTypes.Codes.All;

			ratingDateConfig1.RDT_ContainerMode = Core.Constants.ContainerModes.All;
			ratingDateConfig2.RDT_ContainerMode = Core.Constants.ContainerModes.All;
			ratingDateConfig3.RDT_ContainerMode = Core.Constants.ContainerModes.All;

			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig1.RDT_Location = "AU";
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_Location = "AUSYD";
			ratingDateConfig3.RDT_Location = "AUMEL";
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);

			ratingDateConfig1.RDT_Location = "AUSYD";
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig1.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Import;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig1.RDT_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.Revenue;
			ratingDateConfig3.RDT_Location = "AUSYD";
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.Revenue;
			ratingDateConfig3.RDT_RateType = JobRateTypes.Codes.Cost;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.All;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.Cost;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig3.RDT_RateType = JobRateTypes.Codes.All;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig1.RDT_RateType = JobRateTypes.Codes.All;
			ratingDateConfig1.RDT_ContainerMode = Core.Constants.ContainerModes.FCL;
			ratingDateConfig2.RDT_RateType = JobRateTypes.Codes.All;
			ratingDateConfig3.RDT_Location = "AUSYD";
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_ContainerMode = Core.Constants.ContainerModes.FCL;
			ratingDateConfig3.RDT_ContainerMode = Core.Constants.ContainerModes.LCL;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig1.RDT_JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_ContainerMode = Core.Constants.ContainerModes.All;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig2.RDT_ContainerMode = Core.Constants.ContainerModes.Bulk;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);

			ratingDateConfig3.RDT_ContainerMode = Core.Constants.ContainerModes.All;
			orgHeader.MiscServ.RatingDateConfigs.RunPreSaveValidation();
			AssertNoErrors(ratingDateConfig1.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig2.RDT_JobTypeInfo);
			AssertNoErrors(ratingDateConfig3.RDT_JobTypeInfo);
		}

		string ExpectedDuplicateJobParametersError => "At least one more record already sets Auto Rate behavior for the same Job parameters.";

		public void TestValidateRateType()
		{
			RatingDateConfig.RDT_RateType = ZString.Empty;
			AssertNoErrors(RatingDateConfig.RDT_RateTypeInfo);

			RatingDateConfig.RDT_RateType = JobRateTypes.Codes.Cost;
			AssertNoErrors(RatingDateConfig.RDT_RateTypeInfo);

			RatingDateConfig.RDT_RateType = "ABC";
			AssertHasErrors(RatingDateConfig.RDT_RateTypeInfo);

			RatingDateConfig.RDT_RateType = "AA";
			AssertHasErrors(RatingDateConfig.RDT_RateTypeInfo);
		}

		public void TestValidateContainerMode()
		{
			RatingDateConfig.RDT_ContainerMode = ZString.Empty;
			AssertNoErrors(RatingDateConfig.RDT_ContainerModeInfo);

			RatingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfig.RDT_TransportMode = Core.Constants.TransportModes.Sea;
			RatingDateConfig.RDT_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoErrors(RatingDateConfig.RDT_ContainerModeInfo);

			RatingDateConfig.RDT_ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertHasErrors(RatingDateConfig.RDT_ContainerModeInfo);

			RatingDateConfig.RDT_ContainerMode = "ABC";
			AssertHasErrors(RatingDateConfig.RDT_ContainerModeInfo);

			RatingDateConfig.RDT_ContainerMode = "AA";
			AssertHasErrors(RatingDateConfig.RDT_ContainerModeInfo);
		}

		#region Implementation

		RatingDateConfig RatingDateConfig => ratingDateConfig ?? (ratingDateConfig = Factory.NewWithValidTestData<RatingDateConfig>());
		RatingDateConfig ratingDateConfig;

		#endregion
	}
}
