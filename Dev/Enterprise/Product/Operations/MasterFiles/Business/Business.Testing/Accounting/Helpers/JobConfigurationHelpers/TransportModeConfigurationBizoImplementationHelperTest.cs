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
	sealed class TransportModeConfigurationBizoImplementationHelperTest : TestCaseWithFactory
	{
		public void TestGetListContainsTransportModesWithAllType()
		{
			var transportModeConfigurationMock = new Mock<ITransportModeConfiguration>();
			var helper = GetTransportModeConfigurationHelper(transportModeConfigurationMock.Object);

			AssertEquals("The TransportModeList should contain 'ALL' as the first element", "ALL", helper.GetLookupList()[0].Code);
			AssertContainsExactElementsInAnyOrder(new string[] { "ALL", "AIR", "SEA", "FSA", "FAS", "ROA", "RAI", "COU" }, helper.GetLookupList().GetAllCodes());
		}

		public void TestReadOnlyStatusForTransportMode()
		{
			var transportModeConfigurationMock = new Mock<ITransportModeConfiguration>();
			var transportModeConfigurationHelper = GetTransportModeConfigurationHelper(transportModeConfigurationMock.Object);

			transportModeConfigurationMock.Setup(x => x.JobTypeCode).Returns(ZString.Empty);
			Assert(!transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationMock.Setup(x => x.JobTypeCode).Returns("AAA");
			Assert(!transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationMock.Setup(x => x.JobTypeCode).Returns(JobInvoicingConsumerTypes.ShipmentCode);
			Assert("Should NOT be ReadOnly when JobType- Shipment is TransportModeSupported", !transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationMock.Setup(x => x.JobTypeCode).Returns(JobInvoicingConsumerTypes.CTOCusMAWBCode);
			Assert("Should be ReadOnly when JobType- CTOCusMAWB is NOT TransportModeSupported", transportModeConfigurationHelper.GetReadOnlyStatus());
		}

		public void TestValidate_CheckEnteredAndInvalidCodeOnTransportMode()
		{
			var transportModeConfigurationMock = Factory.New<DummyBizoWithTransportModeConfiguration>();
			var transportModeConfigurationHelper = GetTransportModeConfigurationHelper(transportModeConfigurationMock);
			var transportModeList = new CodeDescriptionPairList();
			transportModeList.AddPair("VALID1", "Desc1");
			transportModeList.AddPair("VALID2", "Desc2");
			transportModeConfigurationMock.TransportMode_List = transportModeList;

			transportModeConfigurationMock.JobType = JobInvoicingConsumerTypes.CTOCusMAWBCode;
			Assert("Precondition: GetReadOnlyStatus()", transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationMock.TransportMode = ZString.Empty;
			transportModeConfigurationMock.TransportModeInfo.ClearAllNotifications();
			transportModeConfigurationHelper.Validate();
			AssertNoErrors(transportModeConfigurationMock.TransportModeInfo);

			transportModeConfigurationMock.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert("Precondition: GetReadOnlyStatus()", !transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationMock.TransportMode = ZString.Empty;
			transportModeConfigurationHelper.Validate();
			AssertHasErrorContaining(transportModeConfigurationMock.TransportModeInfo, "Please enter");

			transportModeConfigurationMock.TransportMode = "INVALID";
			transportModeConfigurationHelper.Validate();
			AssertHasErrorContaining(transportModeConfigurationMock.TransportModeInfo, "Enter a valid");

			transportModeConfigurationMock.TransportMode = "VALID1";
			transportModeConfigurationHelper.Validate();
			AssertNoErrors(transportModeConfigurationMock.TransportModeInfo);

			transportModeConfigurationMock.TransportMode = "VALID2";
			transportModeConfigurationHelper.Validate();
			AssertNoErrors(transportModeConfigurationMock.TransportModeInfo);
		}

		public void TestJobTypeSetterLogic_ResetTransportModeInfo()
		{
			var transportModeConfigurationMock = Factory.New<DummyBizoWithTransportModeConfiguration>();
			var transportModeConfigurationHelper = GetTransportModeConfigurationHelper(transportModeConfigurationMock);

			transportModeConfigurationMock.TransportMode = "VALID1";
			AssertEquals("VALID1", transportModeConfigurationMock.TransportModeInfo.Value);

			transportModeConfigurationMock.JobType = JobInvoicingConsumerTypes.ShipmentCode;
			Assert("Precondition: GetReadOnlyStatus()", !transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationHelper.JobTypeSetterLogic();
			AssertEquals("VALID1", transportModeConfigurationMock.TransportModeInfo.Value);

			transportModeConfigurationMock.JobType = JobInvoicingConsumerTypes.CTOCusMAWBCode;
			Assert("Precondition: GetReadOnlyStatus()", transportModeConfigurationHelper.GetReadOnlyStatus());

			transportModeConfigurationHelper.JobTypeSetterLogic();
			AssertEquals(ZString.Empty, transportModeConfigurationMock.TransportModeInfo.Value);
			AssertEquals(ZString.Empty, transportModeConfigurationMock.TransportMode);
		}

		ITransportModeConfigurationHelper GetTransportModeConfigurationHelper(ITransportModeConfiguration transportModeConfig) => ObjectFactory.Get<IJobConfigurationHelperFactory>().GetTransportModeHelper(transportModeConfig);

		class DummyBizoWithTransportModeConfiguration : DummyBusinessObject, ITransportModeConfiguration, IObsoleteValidation
		{
			public DummyBizoWithTransportModeConfiguration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZPropertyInfo ITransportModeConfiguration.TransportModeInfo => TransportModeInfo;

			ZString IJobTypeConfiguration.JobTypeCode => JobType;

			ZPropertyInfo IJobTypeConfiguration.JobTypeCodeInfo => JobTypeInfo;

			public ZString JobType { get => Z0_Code; set => Z0_Code = value; }

			public ZPropertyInfo JobTypeInfo { get => Z0_CodeInfo; }

			public ZString TransportMode { get => Z0_Description; set => Z0_Description = value; }

			public ZPropertyInfo TransportModeInfo { get => Z0_DescriptionInfo; }

			[List("TransportMode_List")]
			public override ZString Z0_Description { get => base.Z0_Description; set => base.Z0_Description = value; }

			public IList TransportMode_List { get; set; }
		}
	}
}
