using System;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RevenueRecognitionValidationTest : JobConfigurationSelectorValidationTest
	{
		public override void TestValidateJobType()
		{
			AssertNoErrors("Precondition: JobType should not have errors.", BizObj.JobTypeInfo);

			BizObj.JobType = "";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "ABC";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			var setting1 = (IRevenueRecognition)BizObjCollection.AddNew();
			var setting2 = (IRevenueRecognition)BizObjCollection.AddNew();
			var setting3 = (IRevenueRecognition)BizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting2.JobTypeInfo should not have errors.", setting2.JobTypeInfo);
			AssertNoErrors("Precondition: setting3.JobTypeInfo should not have errors.", setting3.JobTypeInfo);

			setting1.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting1.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			setting2.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting2.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			setting3.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			//setting3.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;

			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting1.JobType = "AWB";
			setting2.JobType = "SHP";
			setting2.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting2.Mode = Core.Constants.TransportModes.Sea;
			setting2.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(setting3.JobTypeInfo);

			setting3.JobType = "SHP";
			setting3.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting3.Mode = Core.Constants.TransportModes.Sea;
			setting3.BrokerCode = RevenueRecognitionLookups.BrokerCodes.External;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting3.Mode = Core.Constants.TransportModes.Courier;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting2.Mode = Core.Constants.TransportModes.Courier;
			setting2.BrokerCode = RevenueRecognitionLookups.BrokerCodes.External;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting2.JobTypeInfo);
			AssertHasErrors(ExpectedDuplicateJobParametersError, setting3.JobTypeInfo);

			setting2.BrokerCode = RevenueRecognitionLookups.BrokerCodes.Internal;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting2.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting2.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
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

		public void TestValidateRecognitionDateOption()
		{
			AssertNoErrors("Precondition: Mode should not have errors.", BizObj.RecognitionDateOptionCodeInfo);

			BizObj.RecognitionDateOptionCode = "ABC";
			AssertHasErrors(BizObj.RecognitionDateOptionCodeInfo);

			BizObj.RecognitionDateOptionCode = "";
			AssertHasErrors(BizObj.RecognitionDateOptionCodeInfo);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AssertNoErrors(BizObj.RecognitionDateOptionCodeInfo);

			// Permitted date options for various job types
			AssertIsPermittedRecognitionDateOption("", CompleteRecognitionDateOptionList);
			AssertIsPermittedRecognitionDateOption("ALL", RecognitionDateOptionPermittedForOthersList);
			AssertIsPermittedRecognitionDateOption("SHP", RecognitionDateOptionPermittedForShipmentsList);
			AssertIsPermittedRecognitionDateOption("BRK", RecognitionDateOptionPermittedForDeclarationsList);
			AssertIsPermittedRecognitionDateOption("AGS", RecognitionDateOptionPermittedForShippingManagerList);
			AssertIsPermittedRecognitionDateOption("AGB", RecognitionDateOptionPermittedForShippingManagerList);
			AssertIsPermittedRecognitionDateOption("TCW", RecognitionDateOptionPermittedForTransportConsignmentList);
			AssertIsPermittedRecognitionDateOption("LTC", RecognitionDateOptionPermittedForTransportConsignmentList);

			foreach (ICodeDescription jobType in JobTypeList)
			{
				if (jobType.Code != "ALL" && jobType.Code != "SHP" && jobType.Code != "FCN" && jobType.Code != "GCN" && jobType.Code != "BRK" && jobType.Code != "AGS" && jobType.Code != "AGB" && jobType.Code != "QSH" && jobType.Code != "TCW" && jobType.Code != "LTC")
				{
					AssertIsPermittedRecognitionDateOption(jobType.Code, RecognitionDateOptionPermittedForOthersList);
				}
			}
		}

		public void TestValidateOffset()
		{
			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			BizObj.Offset = -1;
			AssertHasError(BizObj.OffsetInfo, "Days Offset must be between 0 and 100.");

			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Periods;
			AssertHasError(BizObj.OffsetInfo, "Periods Offset must be between 0 and 3.");

			BizObj.Offset = 0;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 3;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 4;
			AssertHasError(BizObj.OffsetInfo, "Periods Offset must be between 0 and 3.");

			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			BizObj.Offset = 10;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 100;
			AssertNoErrors(BizObj.OffsetInfo);

			BizObj.Offset = 101;
			AssertHasError(BizObj.OffsetInfo, "Days Offset must be between 0 and 100.");
		}

		public void TestValidateOffsetType()
		{
			AssertNoErrors("Precondition: OffsetType should not have errors.", BizObj.OffsetTypeInfo);

			BizObj.OffsetType = "ABC";
			AssertHasError(BizObj.OffsetTypeInfo, "Enter a valid selection.");

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
			BizObj.OffsetType = "AA";
			AssertNoErrors(BizObj.OffsetTypeInfo);

			BizObj.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.DeliveryDate;
			BizObj.OffsetType = "";
			AssertHasError(BizObj.OffsetTypeInfo, "Please enter an Offset Type.");

			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Periods;
			AssertNoErrors(BizObj.OffsetTypeInfo);

			BizObj.OffsetType = JobConfigurationSelectorHelper.OffsetTypeCodes.Days;
			AssertNoErrors(BizObj.OffsetTypeInfo);
		}

		public void TestValidateBroker()
		{
			// setting job to empty to make BrokerCode non-read-only
			BizObj.JobType = ZString.Empty;

			Assert("BrokerCode is readonly, that will prevent its validation.", !BizObj.BrokerCode_ReadOnly);

			AssertNoErrors("Precondition: Broker should not have errors.", BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = "ABC";
			AssertHasErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = "";
			AssertHasErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = RevenueRecognitionLookups.BrokerCodes.All;
			AssertNoErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = RevenueRecognitionLookups.BrokerCodes.Internal;
			AssertNoErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = RevenueRecognitionLookups.BrokerCodes.External;
			AssertNoErrors(BizObj.BrokerCodeInfo);
		}

		#region Implementation

		protected override string ExpectedDuplicateJobParametersError
		{
			get { return "At least one more record already sets revenue recognition behaviour for the same Job parameters."; }
		}

		protected new IRevenueRecognition BizObj
		{
			get { return (IRevenueRecognition)base.BizObj; }
			set { base.BizObj = value; }
		}

		CodeDescriptionPairList fCompleteRecognitionDateOptionList;
		CodeDescriptionPairList CompleteRecognitionDateOptionList
		{
			get { return fCompleteRecognitionDateOptionList ?? (fCompleteRecognitionDateOptionList = RevenueRecognitionLookups.CompleteRecognitionDateOptionList); }
		}

		void AssertIsPermittedRecognitionDateOption(string jobType, CodeDescriptionPairList permittedOptionsList)
		{
			BizObj.JobType = jobType;
			foreach (ICodeDescription availableRecognitionDateOption in CompleteRecognitionDateOptionList)
			{
				BizObj.RecognitionDateOptionCode = availableRecognitionDateOption.Code;
				Assert(String.Format("Recognition Date Option '{0}' is not permitted for job type {1}", availableRecognitionDateOption.Code, jobType),
					permittedOptionsList.Contains(availableRecognitionDateOption) ^ BizObj.RecognitionDateOptionCodeInfo.HasErrors());
			}
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForOthersList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForOthersList
		{
			get { return fRecognitionDateOptionPermittedForOthersList ?? (fRecognitionDateOptionPermittedForOthersList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForOthersList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForShipmentsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForShipmentsList
		{
			get { return fRecognitionDateOptionPermittedForShipmentsList ?? (fRecognitionDateOptionPermittedForShipmentsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForShipmentsList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForDeclarationsList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForDeclarationsList
		{
			get { return fRecognitionDateOptionPermittedForDeclarationsList ?? (fRecognitionDateOptionPermittedForDeclarationsList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForDeclarationsList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForShippingManagerList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForShippingManagerList
		{
			get { return fRecognitionDateOptionPermittedForShippingManagerList ?? (fRecognitionDateOptionPermittedForShippingManagerList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForShippingManagerList); }
		}

		CodeDescriptionPairList fRecognitionDateOptionPermittedForTransportConsignmentList;
		CodeDescriptionPairList RecognitionDateOptionPermittedForTransportConsignmentList
		{
			get { return fRecognitionDateOptionPermittedForTransportConsignmentList ?? (fRecognitionDateOptionPermittedForTransportConsignmentList = BizObj.RevenueRecognitionLookups.RecognitionDateOptionPermittedForTransportConsignmentList); }
		}

		#endregion
	}
}
