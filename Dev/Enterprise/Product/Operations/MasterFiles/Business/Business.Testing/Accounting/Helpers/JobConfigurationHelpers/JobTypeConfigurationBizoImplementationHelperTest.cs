using System.Collections;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers.Testing
{
	sealed class JobTypeConfigurationBizoImplementationHelperTest : TestCaseWithFactory
	{
		public void TestJobTypeContainsCorrectElements()
		{
			var jobTypeConfigurationMock = new Mock<IJobTypeConfiguration>();
			var helper = GetJobTypeConfigurationHelper(jobTypeConfigurationMock.Object);
			jobTypeConfigurationMock.SetupGet(x => x.JobTypeCode).Returns(ZString.Empty);
			AssertNull("GetJobType():", helper.GetJobType());

			jobTypeConfigurationMock.SetupGet(x => x.JobTypeCode).Returns("AAA");
			AssertNull("GetJobType():", helper.GetJobType());

			jobTypeConfigurationMock.SetupGet(x => x.JobTypeCode).Returns(JobInvoicingConsumerTypes.ShipmentCode);
			AssertType("GetJobType():", typeof(ShipmentConsumerType), helper.GetJobType());

			jobTypeConfigurationMock.SetupGet(x => x.JobTypeCode).Returns(new AllJobsConsumerType().Code);
			AssertType("GetJobType():", typeof(AllJobsConsumerType), helper.GetJobType());
		}

		public void TestGetListContainsJobInvoicingConsumerTypesWithAllType()
		{
			var jobTypeConfigurationMock = new Mock<IJobTypeConfiguration>();
			var helper = GetJobTypeConfigurationHelper(jobTypeConfigurationMock.Object);

			var expectedJobTypes = new string[] { "ALL", "SHP", "QSH", "FCN", "GCN", "BRK", "PCB", "AWB", "CSH", "CLL", "CST", "TRN", "ABK", "TBM", "ATB", "TCW", "LTC", "WIN", "WOU", "WST", "WSC", "WSJ", "WVO", "ACR", "UBR", "CTO", "AHW", "AHE", "AGS", "AGB", "ACD", "AVA", "ASC", "ISF", "MAN", "CAE", "WKI", "WKP", "WKR", "TRC", "TDC", "TRU", "TDL", "TDU", "YRA", "YRE", "YTU", "MWO", "YAO", "NCT", "LPC", "STO", "YPI" };

			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", helper.GetLookupList()[0].Code);
			AssertContainsExactElementsInAnyOrder(expectedJobTypes, helper.GetLookupList().GetAllCodes());
		}

		public void TestReadOnlyStatusForJobType()
		{
			var jobTypeConfigurationMock = new Mock<IJobTypeConfiguration>();
			var helper = GetJobTypeConfigurationHelper(jobTypeConfigurationMock.Object);
			Assert(!helper.GetReadOnlyStatus());
		}

		public void TestValidate_CheckEnteredAndInvalidCodeOnJobType()
		{
			var jobTypeConfigurationMock = Factory.New<DummyBizoWithJobTypeConfiguration>();
			var jobTypeConfigurationHelper = GetJobTypeConfigurationHelper(jobTypeConfigurationMock);
			var jobTypeList = new CodeDescriptionPairList();
			jobTypeList.AddPair("VALID1", "Desc1");
			jobTypeList.AddPair("VALID2", "Desc2");
			jobTypeConfigurationMock.JobType_List = jobTypeList;

			jobTypeConfigurationMock.JobType = ZString.Empty;
			jobTypeConfigurationHelper.Validate();
			AssertHasErrorContaining(jobTypeConfigurationMock.JobTypeInfo, "Please enter");

			jobTypeConfigurationMock.JobType = "INVALID";
			jobTypeConfigurationHelper.Validate();
			AssertHasErrorContaining(jobTypeConfigurationMock.JobTypeInfo, "Enter a valid");

			jobTypeConfigurationMock.JobType = "VALID1";
			jobTypeConfigurationHelper.Validate();
			AssertNoErrors(jobTypeConfigurationMock.JobTypeInfo);

			jobTypeConfigurationMock.JobType = "VALID2";
			jobTypeConfigurationHelper.Validate();
			AssertNoErrors(jobTypeConfigurationMock.JobTypeInfo);
		}

		IJobTypeConfigurationHelper GetJobTypeConfigurationHelper(IJobTypeConfiguration jobTypeConfig) => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetJobTypeHelper(jobTypeConfig);

		class DummyBizoWithJobTypeConfiguration : DummyBusinessObject, IJobTypeConfiguration, IObsoleteValidation
		{
			public DummyBizoWithJobTypeConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZString IJobTypeConfiguration.JobTypeCode => JobType;

			ZPropertyInfo IJobTypeConfiguration.JobTypeCodeInfo => JobTypeInfo;

			public ZString JobType { get => Z0_Description; set => Z0_Description = value; }

			public ZPropertyInfo JobTypeInfo { get => Z0_DescriptionInfo; }

			[List("JobType_List")]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			public IList JobType_List { get; set; }
		}
	}
}
