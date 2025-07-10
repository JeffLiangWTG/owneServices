using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeComplianceDescriptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsIsUsedBy()
		{
			AssertType<AccChargeComplianceDescriptionLookups>(Factory.New<AccChargeComplianceDescription>().Lookups);
		}

		public void TestJobTypeList()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var jobTypeHelperMock = new Mock<IJobTypeConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("JJ1", "JobType Description1");
			expectedList.AddPair("JJ2", "JobType Description2");

			jobConfigurationHelperFactoryMock.Setup(x => x.GetJobTypeHelper(complianceDescription)).Returns(jobTypeHelperMock.Object);
			jobTypeHelperMock.Setup(x => x.GetLookupList()).Returns(expectedList);

			var jobTypeList = complianceDescription.Lookups.JobTypeList;

			AssertEquals("jobTypeList: ", expectedList, jobTypeList);
			jobTypeHelperMock.Verify(x => x.GetLookupList(), Times.Once);
		}

		public void TestTransportModeList()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var transportModeConfigHelperMock = new Mock<ITransportModeConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("TT1", "TransportMode Description1");
			expectedList.AddPair("TT2", "TransportMode Description2");

			jobConfigurationHelperFactoryMock.Setup(x => x.GetTransportModeHelper(complianceDescription)).Returns(transportModeConfigHelperMock.Object);
			transportModeConfigHelperMock.Setup(x => x.GetLookupList()).Returns(expectedList);

			var transportModeList = complianceDescription.Lookups.TransportModeList;

			AssertEquals("transportModeList: ", expectedList, transportModeList);
			transportModeConfigHelperMock.Verify(x => x.GetLookupList(), Times.Once);
		}

		public void TestSupplyTypeList()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var jobConfigHelperMock = new Mock<IJobConfigurationHelper>();
			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("SS1", "SupplyType Description1");
			expectedList.AddPair("SS2", "SupplyType Description2");

			jobConfigurationHelperFactoryMock.Setup(x => x.GetSupplyTypeHelper(complianceDescription)).Returns(jobConfigHelperMock.Object);
			jobConfigHelperMock.Setup(x => x.GetLookupList()).Returns(expectedList);

			var supplyTypeList = complianceDescription.Lookups.SupplyTypeList;

			AssertEquals("supplyTypeList: ", expectedList, supplyTypeList);
			jobConfigHelperMock.Verify(x => x.GetLookupList(), Times.Once);
		}
	}
}
