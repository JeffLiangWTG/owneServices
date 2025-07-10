using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsGlowServicesModuleServiceTest : WhsTestCaseWithFactory
	{
		#region TestCreateAdHocServiceJob

		public void TestCreateAdHocServiceJob()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			Factory.Save();
			var jobPK = ZGuid.NewZGuid();
			var customerReference = "CusRef0001";
			var billingDate = ZDate.Today;

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateAdHocServiceJob(jobPK, clientPK, warehousePK, billingDate, customerReference);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var jobInDB = newFactory.Load<WhsAdHocServiceJob>(jobPK);
			AssertNotNull("Should have created a new WhsAdHocServiceJob.", jobInDB);
			AssertNotNullOrEmpty("Should have generated a job number for the new job.", jobInDB.WSJ_JobNumber);
			AssertEquals("Client should be correct.", clientPK, jobInDB.WSJ_OH_Client);
			AssertEquals("Warehouse should be correct.", warehousePK, jobInDB.WSJ_WW_Whs);
			AssertEquals("Customer Reference should be correct.", customerReference, jobInDB.WSJ_CustomerReference);
			AssertEquals("Billing Date should be correct.", billingDate, jobInDB.WSJ_BillingDate);
			AssertEquals("Job should not be Finalised.", false, jobInDB.WSJ_IsFinalised);
		}

		public void TestCreateAdHocServiceJob_WarehouseNotFound()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = ZGuid.NewZGuid();
			var clientPK = data.Org1.PK;
			Factory.Save();
			var jobPK = ZGuid.NewZGuid();
			var customerReference = "CusRef0001";
			var billingDate = ZDate.Today;

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateAdHocServiceJob(jobPK, clientPK, warehousePK, billingDate, customerReference);

			// Assert
			AssertEquals("Should have an error message.", "Warehouse not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var jobInDB = newFactory.Load<WhsAdHocServiceJob>(jobPK);
			AssertNull("Should NOT have created a new WhsAdHocServiceJob.", jobInDB);
		}

		public void TestCreateAdHocServiceJob_ClientNotFound()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = ZGuid.NewZGuid();
			Factory.Save();
			var jobPK = ZGuid.NewZGuid();
			var customerReference = "CusRef0001";
			var billingDate = ZDate.Today;

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateAdHocServiceJob(jobPK, clientPK, warehousePK, billingDate, customerReference);

			// Assert
			AssertEquals("Should have an error message.", "Client not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var jobInDB = newFactory.Load<WhsAdHocServiceJob>(jobPK);
			AssertNull("Should NOT have created a new WhsAdHocServiceJob.", jobInDB);
		}

		public void TestCreateAdHocServiceJob_JobAlreadyExists()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var oldJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDate.Today.AddDays(1));
			Factory.Save();

			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			Factory.Save();
			var jobPK = oldJob.PK;
			var customerReference = "CusRef0001";
			var billingDate = ZDate.Today;

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateAdHocServiceJob(jobPK, clientPK, warehousePK, billingDate, customerReference);

			// Assert
			AssertEquals("Should have an error message.", "Ad Hoc Service Job already exists.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var jobInDB = newFactory.Load<WhsAdHocServiceJob>(new ZQuery(WhsAdHocServiceJobSchema.WSJ_CustomerReference, customerReference)).FirstOrDefault();
			AssertNull("Should NOT have created a new WhsAdHocServiceJob.", jobInDB);
		}

		public void TestCreateAdHocServiceJob_CustomerReferenceShouldDefaultToJobNumber()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			Factory.Save();
			var jobPK = ZGuid.NewZGuid();
			var billingDate = ZDate.Today;

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateAdHocServiceJob(jobPK, clientPK, warehousePK, billingDate, null);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var jobInDB = newFactory.Load<WhsAdHocServiceJob>(jobPK);
			AssertNotNull("Should have created a new WhsAdHocServiceJob.", jobInDB);
			AssertNotNullOrEmpty("Should have generated a job number for the new job.", jobInDB.WSJ_JobNumber);
			AssertEquals("Customer Reference should default to job number.", jobInDB.WSJ_JobNumber, jobInDB.WSJ_CustomerReference);
		}

		#endregion

		#region TestCreateWhsJobService

		public void TestCreateWhsJobService_AdHocJobService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNotNull("Should have created a new jobService.", serviceInDB);
			AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
			AssertEquals("Should have correct parent table code.", WhsAdHocServiceJobSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
			AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
			AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
			AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
			AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
			AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
			AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
			AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
			AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
			AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
		}

		public void TestCreateWhsJobService_DocketService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, job.PK, WhsDocketSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNotNull("Should have created a new jobService.", serviceInDB);
			AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
			AssertEquals("Should have correct parent table code.", WhsDocketSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
			AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
			AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
			AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
			AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
			AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
			AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
			AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
			AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
			AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
		}

		public void TestCreateWhsJobService_VASOrderService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, job.PK, WhsVASOrderSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNotNull("Should have created a new jobService.", serviceInDB);
			AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
			AssertEquals("Should have correct parent table code.", WhsVASOrderSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
			AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
			AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
			AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
			AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
			AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
			AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
			AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
			AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
			AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
		}

		public void TestCreateWhsJobService_JobNotFound_AdhocJob()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var jobPk = ZGuid.NewZGuid();
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, jobPk, WhsAdHocServiceJobSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertEquals("Should have an error message.", "Ad hoc service job not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNull("Should NOT have created a new job Service.", serviceInDB);
		}

		public void TestCreateWhsJobService_JobNotFound_Docket()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var jobPk = ZGuid.NewZGuid();
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, jobPk, WhsDocketSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertEquals("Should have an error message.", "Warehouse docket not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNull("Should NOT have created a new job Service.", serviceInDB);
		}

		public void TestCreateWhsJobService_JobNotFound_VASOrder()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var jobPk = ZGuid.NewZGuid();
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, jobPk, WhsVASOrderSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertEquals("Should have an error message.", "VAS order not found.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(servicePK);
			AssertNull("Should NOT have created a new job Service.", serviceInDB);
		}

		public void TestCreateWhsJobService_ServiceAlreadyExists()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDate.Today.AddDays(1));
			var oldService = job.Services.AddNew();
			oldService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			Factory.Save();

			var servicePK = oldService.PK;
			var serviceType = Constants.FreightServiceType.Codes.QuarantineUnpack;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var completedDateTimeOffset = new ZDateTimeOffset(2024, 11, 21);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertEquals("Should have an error message.", "Service already exists.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(new ZQuery(JobServiceSchema.ES_ServiceCode, Constants.FreightServiceType.Codes.QuarantineUnpack)).FirstOrDefault();
			AssertNull("Should NOT have created a new Job Service.", serviceInDB);
		}

		public void TestCreateWhsJobService_ValidationError()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = "ERR";
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CreateWhsJobService(servicePK, serviceType, serviceCount, job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note);

			// Assert
			AssertEquals("Should have an error message.", "Error - ES_ServiceCode: Enter a valid Service Type.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(new ZQuery(JobServiceSchema.ES_ServiceCode, Constants.FreightServiceType.Codes.QuarantineUnpack)).FirstOrDefault();
			AssertNull("Should NOT have created a new Job Service.", serviceInDB);
		}

		#endregion

		#region TestCompleteWhsJobService

		public void TestCompleteWhsJobService_AdHocJobService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial", job.PK, jobService.ES_ParentID);
				AssertEquals("Initial", WhsAdHocServiceJobSchema.Constants.Prefix, jobService.ES_ParentTableCode);
				AssertEquals("Initial", Constants.FreightServiceType.Codes.QuarantineInspection, jobService.ES_ServiceCode);
				AssertEquals("Initial", 0m, jobService.ES_ServiceCount);
			});

			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(jobService.PK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, false);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(jobService.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
				AssertEquals("Should have correct parent table code.", WhsAdHocServiceJobSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
				AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
				AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
				AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
				AssertNotNullOrEmpty("Should have correct Completed Date Time.", serviceInDB.ES_CompletedDateTimeOffset.ToString());
				AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
				AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
				AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
				AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
				AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
				AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
			});
		}

		public void TestCompleteWhsJobService_AdHocJobService_Finalise()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial", job.PK, jobService.ES_ParentID);
				AssertEquals("Initial", WhsAdHocServiceJobSchema.Constants.Prefix, jobService.ES_ParentTableCode);
				AssertEquals("Initial", Constants.FreightServiceType.Codes.QuarantineInspection, jobService.ES_ServiceCode);
				AssertEquals("Initial", 0m, jobService.ES_ServiceCount);
			});

			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(jobService.PK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, true);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(jobService.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
				AssertEquals("Should have correct parent table code.", WhsAdHocServiceJobSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
				AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
				AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
				AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
				AssertNotNullOrEmpty("Should have correct Completed Date Time.", serviceInDB.ES_CompletedDateTimeOffset.ToString());
				AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
				AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
				AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
				AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
				AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
				AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
				AssertEquals("Job should finalised", true, job.IsFinalised);
			});
		}

		public void TestCompleteWhsJobService_DocketService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial", job.PK, jobService.ES_ParentID);
				AssertEquals("Initial", WhsDocketSchema.Constants.Prefix, jobService.ES_ParentTableCode);
				AssertEquals("Initial", Constants.FreightServiceType.Codes.QuarantineInspection, jobService.ES_ServiceCode);
				AssertEquals("Initial", 0m, jobService.ES_ServiceCount);
			});

			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(jobService.PK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, false);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(jobService.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
				AssertEquals("Should have correct parent table code.", WhsDocketSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
				AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
				AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
				AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
				AssertNotNullOrEmpty("Should have correct Completed Date Time.", serviceInDB.ES_CompletedDateTimeOffset.ToString());
				AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
				AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
				AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
				AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
				AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
				AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
			});
		}

		public void TestCompleteWhsJobService_VASOrderService()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Initial", job.PK, jobService.ES_ParentID);
				AssertEquals("Initial", WhsVASOrderSchema.Constants.Prefix, jobService.ES_ParentTableCode);
				AssertEquals("Initial", Constants.FreightServiceType.Codes.QuarantineInspection, jobService.ES_ServiceCode);
				AssertEquals("Initial", 0m, jobService.ES_ServiceCount);
			});

			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new TimeSpan(2, 30, 0);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(jobService.PK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, false);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			var newFactory = new BusinessObjectFactory();
			var serviceInDB = newFactory.Load<WhsJobService>(jobService.PK);
			CombineAssertions(() =>
			{
				AssertNotNull("Should have created a new jobService.", serviceInDB);
				AssertEquals("Should have correct parent.", job.PK, serviceInDB.ES_ParentID);
				AssertEquals("Should have correct parent table code.", WhsVASOrderSchema.Constants.Prefix, serviceInDB.ES_ParentTableCode);
				AssertEquals("Should have correct Service Code.", Constants.FreightServiceType.Codes.Fumigation, serviceInDB.ES_ServiceCode);
				AssertEquals("Should have correct Service Count.", 5m, serviceInDB.ES_ServiceCount);
				AssertEquals("Should have correct Booked Date Time.", bookedDateTimeOffset, serviceInDB.ES_BookedDateTimeOffset);
				AssertNotNullOrEmpty("Should have correct Completed Date Time.", serviceInDB.ES_CompletedDateTimeOffset.ToString());
				AssertEquals("Should have correct contractor.", clientPK, serviceInDB.ES_OH_Contractor);
				AssertEquals("Should have correct duration.", (ZDateTime)duration, serviceInDB.ES_Duration);
				AssertEquals("Should have correct location.", location.PK, serviceInDB.ES_OA_Location);
				AssertEquals("Should have correct sub location.", "Test Sub Location", serviceInDB.ES_SubLocation);
				AssertEquals("Should have correct reference.", "Test Reference", serviceInDB.ES_References);
				AssertEquals("Should have correct service note.", "Test Note", serviceInDB.ES_ServiceNote);
			});
		}

		public void TestCompleteWhsJobService_ServicNotFound()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			Factory.Save();

			var servicePK = ZGuid.NewZGuid();
			var serviceType = Constants.FreightServiceType.Codes.Fumigation;
			var serviceCount = 5m;
			var jobPk = ZGuid.NewZGuid();
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(servicePK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, false);

			// Assert
			AssertEquals("Should have an error message.", "Warehouse job service not found.", errorMessage);
		}

		public void TestCompleteWhsJobService_ValidationError()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			Factory.Save();
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			Factory.Save();

			var serviceType = "ERR";
			var serviceCount = 5m;
			var bookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			var contractor = clientPK;
			var duration = new DateTime(DateTime.Now.Year, 1, 1).AddHours(2).AddMinutes(30);
			var locationPK = location.PK;
			var subLocation = "Test Sub Location";
			var reference = "Test Reference";
			var note = "Test Note";

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteWhsJobService(jobService.PK, serviceType, serviceCount, bookedDateTimeOffset, contractor, duration, locationPK, subLocation, reference, note, false);

			// Assert
			AssertEquals("Should have an error message.", "Error - ES_ServiceCode: Enter a valid Service Type.", errorMessage);
		}

		#endregion

		#region TestCompleteAllServiceAndFinaliseServiceJob

		public void TestCompleteAllWhsJobServices_EnableFinaliseJob()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			for (var i = 0; i < 10; i++)
			{
				var jobService = job.Services.AddNew();
				jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
				jobService.ES_BookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			}
			Factory.Save();

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteAllWhsJobServices(job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, finaliseServiceJob: true);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			foreach (var jobService in job.Services.Cast<WhsJobService>())
			{
				AssertEquals("service should completed", true, jobService.ES_CompletedDateTimeOffset.IsValid);
			}

			AssertEquals("job should finalized", true, job.IsFinalised);
		}

		public void TestCompleteAllWhsJobServices_DisableFinaliseJob()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			for (var i = 0; i < 10; i++)
			{
				var jobService = job.Services.AddNew();
				jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
				jobService.ES_BookedDateTimeOffset = new ZDateTimeOffset(2024, 11, 15);
			}
			Factory.Save();

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteAllWhsJobServices(job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, finaliseServiceJob: false);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			foreach (var jobService in job.Services.Cast<WhsJobService>())
			{
				AssertNotNull("service should completed", jobService.ES_CompletedDateTimeOffset);
			}

			AssertEquals("job should not finalized", false, job.IsFinalised);
		}

		public void TestCompleteAllWhsJobServices_CompletedServicesShouldNotUpdate()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehousePK = data.Whs1.PK;
			var clientPK = data.Org1.PK;
			var location = data.Org1.Addresses.AddNew();
			location.OA_Address1 = "test";
			location.OA_Email = "test@test.com";
			var job = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today.AddDays(6));
			var bookedDateTime = new ZDateTimeOffset(2024, 11, 15);
			var completedDateTime = new ZDateTimeOffset(2024, 11, 17);
			var jobService = job.Services.AddNew();
			jobService.ES_ServiceCode = Constants.FreightServiceType.Codes.QuarantineInspection;
			jobService.ES_BookedDateTimeOffset = bookedDateTime;
			jobService.ES_CompletedDateTimeOffset = completedDateTime;
			Factory.Save();

			// Act
			var service = new WhsGlowServicesModuleService();
			var errorMessage = service.CompleteAllWhsJobServices(job.PK, WhsAdHocServiceJobSchema.Constants.Prefix, finaliseServiceJob: false);

			// Assert
			AssertNullOrEmpty("Should have no error message.", errorMessage);

			AssertEquals("Completed Date Time should not be updated", new ZDateTimeOffset(2024, 11, 17), jobService.ES_CompletedDateTimeOffset);

			AssertEquals("job should not finalized", false, job.IsFinalised);
		}

		#endregion
	}
}
