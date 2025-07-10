using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Test
{
	[TestedType(typeof(RatingDateConfigViewModel))]
	public class RatingDateConfigViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFields()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var ratingDateConfig = orgHeader.MiscServ.RatingDateConfigs.AddNew();
			ratingDateConfig.RDT_JobType = JobInvoicingConsumerTypes.ForwardingConsol.Code;
			ratingDateConfig.RDT_Direction = Constants.FreightShipmentDirection.Code.Export;
			ratingDateConfig.RDT_TransportMode = Constants.TransportModes.Sea;
			ratingDateConfig.RDT_RateType = JobRateTypes.Codes.Cost;
			ratingDateConfig.RDT_ContainerMode = Constants.ContainerModes.FCL;
			ratingDateConfig.RDT_AutoratingDate = JobDateTypes.Codes.ArrivalDate;
			ratingDateConfig.RDT_Location = "AUSYD";
			ratingDateConfig.RDT_NoFallback = true;

			var ratingDateConfigViewModel = new RatingDateConfigViewModel(ratingDateConfig);
			AssertEquals("JobType", expected: JobInvoicingConsumerTypes.ForwardingConsol.Code, actual: ratingDateConfigViewModel.JobType);
			AssertEquals("Direction", expected: Constants.FreightShipmentDirection.Code.Export, actual: ratingDateConfigViewModel.DirectionCode);
			AssertEquals("TransportMode", expected: Constants.TransportModes.Sea, actual: ratingDateConfigViewModel.Mode);
			AssertEquals("RateType", expected: JobRateTypes.Codes.Cost, actual: ratingDateConfigViewModel.RateType);
			AssertEquals("ContainerMode", expected: Constants.ContainerModes.FCL, actual: ratingDateConfigViewModel.ContainerMode);
			AssertEquals("DateType", expected: JobDateTypes.Codes.ArrivalDate, actual: ratingDateConfigViewModel.DateType);
			AssertEquals("Location", expected: "AUSYD", actual: ratingDateConfigViewModel.Location);
			AssertEquals("NoFallback", expected: true, actual: ratingDateConfigViewModel.IsFallbackDisabled);
		}

		#region Validation

		public void TestValidation_JobType() => TestValidation(RatingDateConfigViewModel.JobTypeInfo);

		public void TestValidation_DirectionCode() => TestValidation(RatingDateConfigViewModel.DirectionCodeInfo);

		public void TestValidation_Mode() => TestValidation(RatingDateConfigViewModel.ModeInfo);

		public void TestValidation_RateType() => TestValidation(RatingDateConfigViewModel.RateTypeInfo);

		public void TestValidation_ContainerMode() => TestValidation(RatingDateConfigViewModel.ContainerModeInfo);

		public void TestValidation_DateType() => TestValidation(RatingDateConfigViewModel.DateTypeInfo);

		public void TestValidation_Location() => TestValidation(RatingDateConfigViewModel.LocationInfo);

		static void TestValidation(ZPropertyInfo propertyInfo)
		{
			AssertNoErrors(propertyInfo);
			propertyInfo.Value = (ZString)"XXX";
			AssertHasError(propertyInfo, "Enter a valid selection.");
		}

		#endregion

		#region ReadOnly

		public void TestReadOnly_Direction()
		{
			RatingDateConfigViewModel.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			RatingDateConfigViewModel.JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals("DirectionInfo.ReadOnly", expected: false, actual: RatingDateConfigViewModel.DirectionCodeInfo.ReadOnly);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				RatingDateConfigViewModel.JobType = "!@#";
				AssertEquals("DirectionInfo.ReadOnly", expected: true, actual: RatingDateConfigViewModel.DirectionCodeInfo.ReadOnly);
			}
		}

		public void TestReadOnly_Mode()
		{
			RatingDateConfigViewModel.Mode = Constants.TransportModes.Sea;
			RatingDateConfigViewModel.JobType = JobInvoicingConsumerTypes.GatewayConsolCode;
			AssertEquals(expected: false, actual: RatingDateConfigViewModel.ModeInfo.ReadOnly);

			using (NUnit.Framework.TestingState.SuspendIsRunningTests())
			{
				RatingDateConfigViewModel.JobType = "!@#";
				AssertEquals(expected: true, actual: RatingDateConfigViewModel.ModeInfo.ReadOnly);
			}
		}

		public void TestReadOnly_Location()
		{
			RatingDateConfigViewModel.DirectionCode = "";
			RatingDateConfigViewModel.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			AssertEquals(expected: false, actual: RatingDateConfigViewModel.LocationInfo.ReadOnly);

			RatingDateConfigViewModel.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			AssertEquals(expected: true, actual: RatingDateConfigViewModel.LocationInfo.ReadOnly);
		}

		public void TestReadOnly_IsFallbackDisabled()
		{
			RatingDateConfigViewModel.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfigViewModel.Mode = Constants.TransportModes.All;
			RatingDateConfigViewModel.DateType = JobDateTypes.Codes.DepartureDate;
			AssertEquals(expected: true, actual: RatingDateConfigViewModel.IsFallbackDisabledInfo.ReadOnly);

			RatingDateConfigViewModel.DateType = JobDateTypes.Codes.HouseBillIssueDate;
			AssertEquals(expected: false, actual: RatingDateConfigViewModel.IsFallbackDisabledInfo.ReadOnly);
		}

		public void TestReadOnly_Container()
		{
			RatingDateConfigViewModel.JobType = JobInvoicingConsumerTypes.ForwardingConsolCode;
			RatingDateConfigViewModel.Mode = Constants.TransportModes.Sea;
			AssertEquals(expected: false, actual: RatingDateConfigViewModel.ContainerModeInfo.ReadOnly);
			RatingDateConfigViewModel.Mode = Constants.TransportModes.Courier;
			AssertEquals(expected: true, actual: RatingDateConfigViewModel.ContainerModeInfo.ReadOnly);
		}

		#endregion

		#region Lookup

		public void TestLookups()
		{
			AssertEquals("Lookups.JobTypeList", RatingDateConfigViewModel.JobTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.DirectionList", RatingDateConfigViewModel.DirectionCodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.TransportModeList", RatingDateConfigViewModel.ModeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.DateTypeList", RatingDateConfigViewModel.DateTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.AutoRatingLocationCollection", RatingDateConfigViewModel.LocationInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.RateTypeList", RatingDateConfigViewModel.RateTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("Lookups.ContainerModeList", RatingDateConfigViewModel.ContainerModeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var ratingDateConfig = Factory.NewWithValidTestData<RatingDateConfig>();
			return new RatingDateConfigViewModel(ratingDateConfig);
		}

		RatingDateConfigViewModel RatingDateConfigViewModel => ratingDateConfigViewModel ?? (ratingDateConfigViewModel = (RatingDateConfigViewModel)GetNewBusinessObject());
		RatingDateConfigViewModel ratingDateConfigViewModel;

		#endregion
	}
}
