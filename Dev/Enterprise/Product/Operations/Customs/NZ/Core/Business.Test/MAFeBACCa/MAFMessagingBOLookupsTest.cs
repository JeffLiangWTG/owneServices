using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration;

	public class MAFMessagingBOLookupsTest : TestCaseWithFactory
	{
		public void TestMessagingStatuses()
		{
			AssertEquals("MessagingStatuses", typeof(MessagingStatusList), Lookups.MessagingStatuses.GetType());
		}

		public void TestCargoTypes()
		{
			AssertEquals("CargoTypes", typeof(CargoTypeList), Lookups.CargoTypes.GetType());
		}

		public void TestYesNoUnknowns()
		{
			AssertEquals("YesNoUnknowns", typeof(YesNoUnknownList), Lookups.YesNoUnknowns.GetType());
		}

		public void TestMeasurementUQs()
		{
			AssertEquals("MeasurementUQs", typeof(MeasurementUQList), Lookups.MeasurementUQs.GetType());
		}

		public void TestMAFProcessingOffices()
		{
			AssertEquals("MAFProcessingOffices", typeof(MAFProcessingOfficeList), Lookups.MAFProcessingOffices.GetType());
		}

		public void TestMAFPaymentMethods()
		{
			AssertEquals("MAFPaymentMethods", typeof(MAFPaymentMethodList), Lookups.MAFPaymentMethods.GetType());
		}

		public void TestConsignmentTypes()
		{
			AssertEquals("ConsignmentTypes", typeof(ConsignmentTypeList), Lookups.ConsignmentTypes.GetType());
		}

		public void TestTSW_IPIMessagingStatuses()
		{
			AssertEquals("TSW_IPIMessagingStatuses", typeof(ConsolIPIStatusList), Lookups.TSW_IPIMessagingStatuses.GetType());
		}

		MAFMessagingBOLookups Lookups
		{
			get { return lookups ?? (lookups = TestDataBuilder.GetMAFMessaging(Factory.New<JobDeclaration>()).Lookups); }
		}
		MAFMessagingBOLookups lookups;
	}
}
