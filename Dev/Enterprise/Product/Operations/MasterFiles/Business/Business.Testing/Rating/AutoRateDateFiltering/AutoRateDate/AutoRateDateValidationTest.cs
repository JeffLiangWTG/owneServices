using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AutoRateDateValidationTest : JobConfigurationSelectorValidationTest
	{
		public override void TestValidateJobType()
		{
			AssertNoErrors("Precondition: JobType should not have errors.", BizObj.JobTypeInfo);

			BizObj.JobType = "";
			BizObj.ValidateJobType();
			AssertHasError("Empty JobType should have error", BizObj.JobTypeInfo, "Please enter a Job Type.");

			BizObj.JobType = "ABC";
			AssertHasError("Invalid JobType type should have error", BizObj.JobTypeInfo, "Enter a valid selection.");

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			var setting1 = (IJobConfigurationSelector)BizObjCollection.AddNew();
			var setting2 = (IJobConfigurationSelector)BizObjCollection.AddNew();
			var setting3 = (IJobConfigurationSelector)BizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);
			AssertNoErrors("Precondition: setting3.JobTypeInfo should not have errors.", setting3.JobTypeInfo);

			setting1.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			setting2.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;
			setting3.JobType = JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;

			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting1.JobType = "AWB";
			setting2.JobType = "SHP";
			setting2.DirectionCode = Core.Constants.FreightShipmentDirection.Code.All;
			setting2.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.JobType = "SHP";
			setting3.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			setting3.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.Mode = Core.Constants.TransportModes.Courier;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			setting2.Mode = Core.Constants.TransportModes.Courier;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting2.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.Mode = JobConfigurationSelectorLookups.ModeAdditionalCodes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.JobType = "AWB";
			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoErrors(setting3.JobTypeInfo);
		}

		public void TestValidateDate()
		{
			var parent = new AutoRateDate();
			var validation = new AutoRateDateValidation(parent);

			parent.DateType = ZString.Empty;
			AssertHasErrors(parent.DateTypeInfo);

			parent.DateType = JobDateTypes.Codes.ArrivalDate;
			AssertNoErrors(parent.DateTypeInfo);

			parent.DateType = "XXX";
			AssertHasErrors(parent.DateTypeInfo);
		}

		public void TestValidateLocation()
		{
			var parent = new AutoRateDate();
			var validation = new AutoRateDateValidation(parent);

			parent.Location = ZString.Empty;
			AssertNoErrors(parent.LocationInfo);

			parent.Location = "AUSYD";
			AssertNoErrors(parent.LocationInfo);

			parent.Location = "ZZ";
			AssertHasErrors(parent.LocationInfo);
		}

		public void TestDuplicateAutoRateDateSetup()
		{
			var setting1 = (AutoRateDate)BizObjCollection.AddNew();
			var setting2 = (AutoRateDate)BizObjCollection.AddNew();
			var setting3 = (AutoRateDate)BizObjCollection.AddNew();

			setting1.JobType = "SHP";
			setting2.JobType = "SHP";
			setting3.JobType = "SHP";

			setting1.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			setting2.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			setting3.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;

			setting1.Mode = Core.Constants.TransportModes.Sea;
			setting2.Mode = Core.Constants.TransportModes.Sea;
			setting3.Mode = Core.Constants.TransportModes.Sea;

			setting1.RateType = JobRateTypes.Codes.All;
			setting2.RateType = JobRateTypes.Codes.All;
			setting3.RateType = JobRateTypes.Codes.All;

			setting1.ContainerMode = Core.Constants.ContainerModes.All;
			setting2.ContainerMode = Core.Constants.ContainerModes.All;
			setting3.ContainerMode = Core.Constants.ContainerModes.All;

			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting1.Location = "AU";
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting2.Location = "AUSYD";
			setting3.Location = "AUMEL";
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);

			setting1.Location = "AUSYD";
			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoErrors(setting3.JobTypeInfo);

			setting1.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Import;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting1.DirectionCode = Core.Constants.FreightShipmentDirection.Code.Export;
			setting1.RateType = JobRateTypes.Codes.Revenue;
			setting3.Location = "AUSYD";
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting2.RateType = JobRateTypes.Codes.Revenue;
			setting3.RateType = JobRateTypes.Codes.Cost;
			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.RateType = JobRateTypes.Codes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertNoErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting2.RateType = JobRateTypes.Codes.Cost;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting3.RateType = JobRateTypes.Codes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting1.RateType = JobRateTypes.Codes.All;
			setting1.ContainerMode = Core.Constants.ContainerModes.FCL;
			setting2.RateType = JobRateTypes.Codes.All;
			setting3.Location = "AUSYD";
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting3.JobTypeInfo, ExpectedDuplicateJobParametersError);

			setting2.ContainerMode = Core.Constants.ContainerModes.All;
			setting2.ContainerMode = Core.Constants.ContainerModes.Other;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.ContainerMode = Core.Constants.ContainerModes.FCL;
			setting3.ContainerMode = Core.Constants.ContainerModes.LCL;
			BizObjCollection.RunPreSaveValidation();
			AssertHasError(setting1.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertHasError(setting2.JobTypeInfo, ExpectedDuplicateJobParametersError);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.ContainerMode = Core.Constants.ContainerModes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.ContainerMode = Core.Constants.ContainerModes.Bulk;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.ContainerMode = Core.Constants.ContainerModes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);
		}

		public void TestValidateRateType()
		{
			var parent = new AutoRateDate();
			var validation = new AutoRateDateValidation(parent);

			parent.RateType = ZString.Empty;
			AssertNoErrors(parent.RateTypeInfo);

			parent.RateType = "REV";
			AssertNoErrors(parent.RateTypeInfo);

			parent.RateType = "CST";
			AssertNoErrors(parent.RateTypeInfo);

			parent.RateType = "ABC";
			AssertHasErrors(parent.RateTypeInfo);

			parent.RateType = "AA";
			AssertHasErrors(parent.RateTypeInfo);
		}

		public void TestValidateContainerMode()
		{
			var parent = new AutoRateDate();
			var validation = new AutoRateDateValidation(parent);

			parent.ContainerMode = ZString.Empty;
			AssertNoErrors(parent.ContainerModeInfo);

			parent.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			parent.Mode = Core.Constants.TransportModes.Sea;
			parent.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoErrors(parent.ContainerModeInfo);

			parent.ContainerMode = Core.Constants.ContainerModes.Groupage;
			AssertHasErrors(parent.ContainerModeInfo);

			parent.ContainerMode = "ABC";
			AssertHasErrors(parent.ContainerModeInfo);

			parent.ContainerMode = "AA";
			AssertHasErrors(parent.ContainerModeInfo);
		}

		#region implementation

		protected override string ExpectedDuplicateJobParametersError => "At least one more record already sets Auto Rate behavior for the same parameters.";

		new AutoRateDate BizObj => (AutoRateDate)base.BizObj;

		protected override IJobConfigurationSelector GetNewBizObj
		{
			get { return new AutoRateDate(); }
		}

		protected override IRegistrySettingCollection GetNewBizObjCollection
		{
			get { return new AutoRateDateCollection(); }
		}

		public override void TestRunPreSaveValidation()
		{
			BizObj.JobType = "";
			BizObj.DirectionCode = "!@#";
			BizObj.Mode = "ABC";
			BizObj.DateType = "XXX";
			BizObj.Location = "ZZ";
			BizObj.RateType = "CCC";

			BizObj.RunPreSaveValidation();

			AssertHasErrors(BizObj.JobTypeInfo);
			AssertHasErrors(BizObj.DirectionCodeInfo);
			AssertHasErrors(BizObj.ModeInfo);
			AssertHasErrors(BizObj.DateTypeInfo);
			AssertHasErrors(BizObj.LocationInfo);
			AssertHasErrors(BizObj.RateTypeInfo);
		}

		#endregion
	}
}
