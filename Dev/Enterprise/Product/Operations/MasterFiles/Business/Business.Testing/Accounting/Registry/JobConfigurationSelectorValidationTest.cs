using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class JobConfigurationSelectorValidationTest : TestCaseWithFactory
	{
		public virtual void TestValidateJobType()
		{
			AssertNoErrors("Precondition: JobType should not have errors.", BizObj.JobTypeInfo);

			BizObj.JobType = "";
			BizObj.ValidateJobType();
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "ABC";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			var setting1 = (IJobConfigurationSelector)BizObjCollection.AddNew();
			var setting2 = (IJobConfigurationSelector)BizObjCollection.AddNew();
			var setting3 = (IJobConfigurationSelector)BizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);
			AssertNoErrors("Precondition: setting3.JobTypeInfo should not have errors.", setting3.JobTypeInfo);

			setting1.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting2.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting3.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;

			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting1.JobType = "AWB";
			setting2.JobType = "SHP";
			setting2.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting2.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(setting3.JobTypeInfo);

			setting3.JobType = "SHP";
			setting3.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting3.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting3.Mode = Core.Constants.TransportModes.Courier;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting2.Mode = Core.Constants.TransportModes.Courier;
			//setting2.BrokerCode = RevenueRecognitionLookups.BrokerCodes.External;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting2.Mode = Core.Constants.TransportModes.Sea;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(setting3.JobTypeInfo);

			setting2.JobType = "AWB";
			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);
		}

		protected abstract string ExpectedDuplicateJobParametersError { get; }

		public virtual void TestValidateDirection()
		{
			// setting job to empty to make DirectionCode non-read-only
			BizObj.JobType = ZString.Empty;

			Assert("DirectionCode is readonly, that will prevent its validation.", !BizObj.DirectionCode_ReadOnly);

			AssertNoErrors("Precondition: Direction should not have errors.", BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = "ABC";
			AssertHasErrors(BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = "";
			AssertHasErrors(BizObj.DirectionCodeInfo);

			BizObj.JobType = "AWB";
			BizObj.DirectionCode = "";
			AssertNoErrors(BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			AssertNoErrors(BizObj.DirectionCodeInfo);
		}

		public virtual void TestValidateMode()
		{
			// setting job to empty to make Mode non-read-only
			BizObj.JobType = ZString.Empty;

			Assert("Mode is readonly, that will prevent its validation.", !BizObj.Mode_ReadOnly);

			string expectedError = "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type.";
			AssertNoErrors("Precondition: Mode should not have errors.", BizObj.ModeInfo);

			BizObj.Mode = "ABC";
			AssertHasErrors(BizObj.ModeInfo);

			BizObj.Mode = "";
			AssertHasErrors(BizObj.ModeInfo);

			BizObj.JobType = "AWB";
			BizObj.Mode = "";
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Courier;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.JobType = "CLL";
			BizObj.Mode = BizObj.Mode;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.Mode = Core.Constants.TransportModes.Air;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Road;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Rail;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.SeaAir;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.JobType = "CSH";
			BizObj.Mode = Core.Constants.TransportModes.AirSea;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.Mode = Core.Constants.TransportModes.Air;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Sea;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Road;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.Rail;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Core.Constants.TransportModes.SeaAir;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.JobType = "SHP";
			BizObj.Mode = BizObj.Mode;
			AssertNoErrors(BizObj.ModeInfo);
		}

		public abstract void TestRunPreSaveValidation();

		#region Implementation

		protected abstract IJobConfigurationSelector GetNewBizObj { get; }

		protected abstract IRegistrySettingCollection GetNewBizObjCollection { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
			BizObjCollection = GetNewBizObjCollection;
		}

		protected IJobConfigurationSelector BizObj { get; set; }
		protected IRegistrySettingCollection BizObjCollection;

		protected CodeDescriptionPairList fJobTypeList;
		protected CodeDescriptionPairList JobTypeList
		{
			get { return fJobTypeList ?? (fJobTypeList = BizObj.JobTypeList); }
		}

		#endregion
	}
}
