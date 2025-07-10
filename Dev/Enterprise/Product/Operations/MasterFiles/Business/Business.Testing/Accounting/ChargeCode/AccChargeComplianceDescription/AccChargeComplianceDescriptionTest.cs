using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeComplianceDescription))]
	sealed class AccChargeComplianceDescriptionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIJobTypeConfigurationImplementation_JobTypeCode()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobTypeConfig = complianceDescription as IJobTypeConfiguration;

			complianceDescription.ADE_JobType = ZString.Empty;
			AssertEquals(ZString.Empty, jobTypeConfig.JobTypeCode);

			complianceDescription.ADE_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			AssertEquals(JobInvoicingConsumerTypes.ShipmentCode, jobTypeConfig.JobTypeCode);
		}

		public void TestIJobTypeConfigurationImplementation_JobTypeCodeInfo()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobTypeConfig = complianceDescription as IJobTypeConfiguration;

			AssertEquals(complianceDescription.ADE_JobTypeInfo, jobTypeConfig.JobTypeCodeInfo);
		}

		public void TestITransportModeConfigurationImplementation_TransportModeInfo()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var transportModeConfig = complianceDescription as ITransportModeConfiguration;

			AssertEquals(complianceDescription.ADE_TransportModeInfo, transportModeConfig.TransportModeInfo);
		}

		public void TestISupplyTypeConfigurationImplementation_SupplyTypeCode()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var supplyTypeConfig = complianceDescription as ISupplyTypeConfiguration;

			complianceDescription.ADE_SupplyType = ZString.Empty;
			AssertEquals(ZString.Empty, supplyTypeConfig.SupplyTypeCode);

			complianceDescription.ADE_SupplyType = SupplyTypeClassificationCodes.LOC;
			AssertEquals(SupplyTypeClassificationCodes.LOC, supplyTypeConfig.SupplyTypeCode);
		}

		public void TestISupplyTypeConfigurationImplementation_SupplyTypeCodeInfo()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var supplyTypeConfig = complianceDescription as ISupplyTypeConfiguration;

			AssertEquals(complianceDescription.ADE_SupplyTypeInfo, supplyTypeConfig.SupplyTypeCodeInfo);
		}

		public void TestADE_TransportMode_ReadOnly_GetReadOnlyStatus()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var transportModeConfigHelperMock = new Mock<ITransportModeConfigurationHelper>();

			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);
			jobConfigurationHelperFactoryMock.Setup(x => x.GetJobTypeHelper(complianceDescription)).Returns(new Mock<IJobTypeConfigurationHelper>().Object);
			jobConfigurationHelperFactoryMock.Setup(x => x.GetTransportModeHelper(complianceDescription)).Returns(transportModeConfigHelperMock.Object);

			transportModeConfigHelperMock.Setup(x => x.GetReadOnlyStatus()).Returns(true);
			Assert(complianceDescription.ADE_TransportModeInfo.ReadOnly);

			transportModeConfigHelperMock.Setup(x => x.GetReadOnlyStatus()).Returns(false);
			Assert(!complianceDescription.ADE_TransportModeInfo.ReadOnly);
		}

		[ExpectNoExceptions]
		public void TestADE_JobTypeSetterInvokeJobTypeSetterLogic()
		{
			var complianceDescription = Factory.New<AccChargeComplianceDescription>();
			var jobConfigurationHelperFactoryMock = new Mock<IJobConfigurationHelperFactory>();
			var transportModeConfigHelperMock = new Mock<ITransportModeConfigurationHelper>();

			ObjectFactory.Substitute(jobConfigurationHelperFactoryMock.Object);
			jobConfigurationHelperFactoryMock.Setup(x => x.GetJobTypeHelper(complianceDescription)).Returns(new Mock<IJobTypeConfigurationHelper>().Object);
			jobConfigurationHelperFactoryMock.Setup(x => x.GetTransportModeHelper(complianceDescription)).Returns(transportModeConfigHelperMock.Object);

			complianceDescription.ADE_JobType = "SHP";
			transportModeConfigHelperMock.Verify(x => x.JobTypeSetterLogic(), Times.Once);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			complianceDescription.ADE_AC = chargeCode.PK;

			var accountingMasterFilesDependencyFactoryMock = new Mock<IAccountingMasterFilesDependencyFactory>();
			var accountingLogHelperMock = new Mock<IAccountingLogHelper>();

			ObjectFactory.Substitute(accountingMasterFilesDependencyFactoryMock.Object);
			accountingMasterFilesDependencyFactoryMock.Setup(x => x.GetAccountingLogHelper()).Returns(accountingLogHelperMock.Object);

			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				complianceDescription.ADE_Description = "TST2";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				complianceDescription.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestComplianceDescriptionJobTypesWithDBConstraints()
		{
			//if you add a new ADE_JobType value in AccChargeComplianceDescription.JobTypeList, you also need to update the DB Constraint on ADE_JobType
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();

			foreach (var jobType in new AccChargeComplianceDescriptionLookups(complianceDescription).JobTypeList.GetAllCodes())
			{
				complianceDescription.ADE_JobType = jobType;
			}
			AssertNoExceptionThrown("All AccChargeComplianceDescription.JobTypeList need to be synchronize with the DB Constraint on ADE_JobType", () => Factory.Save());
		}

		public void TestComplianceDescriptionTransportModesWithDBConstraints()
		{
			//if you add a new ADE_TransportMode value in AccChargeComplianceDescription.TransportModeList, you also need to update the DB Constraint on ADE_TransportMode
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();

			foreach (var transportMode in new AccChargeComplianceDescriptionLookups(complianceDescription).TransportModeList.GetAllCodes())
			{
				complianceDescription.ADE_TransportMode = transportMode;
			}
			AssertNoExceptionThrown("All AccChargeComplianceDescription.TransportModeList need to be synchronize with the DB Constraint on ADE_TransportMode", () => Factory.Save());
		}

		public void TestComplianceDescriptionSupplyTypesWithDBConstraints()
		{
			//if you add a new ADE_SupplyType value in AccChargeComplianceDescription.SupplyTypeList, you also need to update the DB Constraint on ADE_SupplyType
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();

			foreach (var supplyType in new AccChargeComplianceDescriptionLookups(complianceDescription).SupplyTypeList.GetAllCodes())
			{
				complianceDescription.ADE_SupplyType = supplyType;
			}
			AssertNoExceptionThrown("All AccChargeComplianceDescription.SupplyTypeList need to be synchronize with the DB Constraint on ADE_SupplyType", () => Factory.Save());
		}

		public void TestADE_JobTypeListAttribute()
		{
			var complainceDescription = Factory.New<AccChargeComplianceDescription>();
			AssertEquals("Lookups.JobTypeList", complainceDescription.ADE_JobTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestADE_TransportModeListAttribute()
		{
			var complainceDescription = Factory.New<AccChargeComplianceDescription>();
			AssertEquals("Lookups.TransportModeList", complainceDescription.ADE_TransportModeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestADE_SupplyTypeListAttribute()
		{
			var complainceDescription = Factory.New<AccChargeComplianceDescription>();
			AssertEquals("Lookups.SupplyTypeList", complainceDescription.ADE_SupplyTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestUniqueIndex_ComplianceDescription()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var complianceDescription1 = chargeCode.ChargeComplianceDescriptions.AddNew();
			var complianceDescription2 = chargeCode.ChargeComplianceDescriptions.AddNew();

			complianceDescription1.ADE_JobType = "ALL";
			complianceDescription2.ADE_JobType = "ALL";

			complianceDescription1.ADE_SupplyType = "LOC";
			complianceDescription2.ADE_SupplyType = "LOC";

			complianceDescription1.ADE_Description = "TEST DESC.";
			complianceDescription2.ADE_Description = "TEST DESC.";

			try
			{
				Factory.Save();
				Fail("Expected a unique index violation exception");
			}
			catch (ZSaveException ex)
			{
				AssertEquals("1 bizo should be involved with the unique constraint violation (one should save fine)", 1, ex.BusinessObjects.Length);

				var uniqueIndexName = ex.IndexNameIfUniqueIndexViolation;
				AssertEquals("Correct unique constraint should be violated", AccChargeComplianceDescriptionSchema.Constants.Indexes.NR_UC__ADE_AC_ADE_JobType_ADE_TransportMode_ADE_SupplyType, uniqueIndexName);
			}
		}

		public void TestIDuplicateValidationItemImplementation_IsDuplicated()
		{
			var complianceDescription1 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			var complainceDescription2 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			complianceDescription1.ADE_JobType = complainceDescription2.ADE_JobType = "SHP";
			complianceDescription1.ADE_TransportMode = complainceDescription2.ADE_TransportMode = "ALL";
			complianceDescription1.ADE_SupplyType = complainceDescription2.ADE_SupplyType = "LOX";

			AssertNotEquals("Precondition: PK", complainceDescription2.PK, complianceDescription1.PK);
			AssertEquals("Precondition: ADE_JobType", complainceDescription2.ADE_JobType, complianceDescription1.ADE_JobType);
			AssertEquals("Precondition: ADE_TransportMode", complainceDescription2.ADE_TransportMode, complianceDescription1.ADE_TransportMode);
			AssertEquals("Precondition: ADE_SupplyType", complainceDescription2.ADE_SupplyType, complianceDescription1.ADE_SupplyType);
			Assert(((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complainceDescription2.ADE_JobType = "ALL";
			AssertNotEquals("Precondition: ADE_JobType", complainceDescription2.ADE_JobType, complianceDescription1.ADE_JobType);
			Assert(!((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complianceDescription1.ADE_JobType = complainceDescription2.ADE_JobType;
			Assert(((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complainceDescription2.ADE_TransportMode = "AIR";
			AssertNotEquals("Precondition: ADE_TransportMode", complainceDescription2.ADE_TransportMode, complianceDescription1.ADE_TransportMode);
			Assert(!((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complainceDescription2.ADE_TransportMode = complianceDescription1.ADE_TransportMode;
			Assert(((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complainceDescription2.ADE_SupplyType = "LOC";
			AssertNotEquals("Precondition: ADE_SupplyType", complainceDescription2.ADE_SupplyType, complianceDescription1.ADE_SupplyType);
			Assert(!((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));

			complainceDescription2.ADE_SupplyType = complianceDescription1.ADE_SupplyType;
			Assert(((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription1).IsDuplicated(complainceDescription2));
		}

		public void TestIDuplicateValidationItemImplementation_AddRowError()
		{
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			AssertNoRowErrors(complianceDescription);

			var errorMessage = "Test Add Error Message";
			((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription).AddRowError(errorMessage);

			AssertHasRowError(complianceDescription, errorMessage);
		}

		public void TestIDuplicateValidationItemImplementation_RemoveRowError()
		{
			var complianceDescription = Factory.NewWithValidTestData<AccChargeComplianceDescription>();

			var errorMessage = "Test Remove Error Message";
			complianceDescription.AddRowError(errorMessage);
			AssertHasRowError("Precondition:", complianceDescription, errorMessage);

			((IDuplicateValidationItem<AccChargeComplianceDescription>)complianceDescription).RemoveRowError(errorMessage);
			AssertNoRowErrors(complianceDescription);
		}
	}
}
