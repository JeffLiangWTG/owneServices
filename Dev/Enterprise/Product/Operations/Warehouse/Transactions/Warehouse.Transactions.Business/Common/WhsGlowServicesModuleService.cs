using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using OrgHeader = Enterprise.MasterFiles.Business.OrgHeader;
using WhsWarehouse = Enterprise.Warehouse.Environment.Business.WhsWarehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsGlowServicesModuleService : IWhsGlowServicesModuleService
	{
		#region CreateAdHocServiceJob

		public string CreateAdHocServiceJob(ZGuid jobPK, ZGuid clientPK, ZGuid warehousePK, ZDate billingDate, ZString? customerReference)
		{
			string errorMessage;
			var factory = new BusinessObjectFactory();

			if (factory.Load<WhsWarehouse>(warehousePK) == null)
			{
				errorMessage = Res.GetString("6ea403af-70ad-4f79-9555-db20781cd687", "Warehouse not found.");
			}
			else if (factory.Load<OrgHeader>(clientPK) == null)
			{
				errorMessage = Res.GetString("747a1492-52af-4598-bae9-60aed78dbdc4", "Client not found.");
			}
			else if (factory.Load<WhsAdHocServiceJob>(jobPK) != null)
			{
				errorMessage = Res.GetString("c54878a7-f574-4dc8-9ff0-d538044e3279", "Ad Hoc Service Job already exists.");
			}
			else
			{
				errorMessage = CreateAdHocServiceJobCore(factory, jobPK, clientPK, warehousePK, billingDate, customerReference);
			}

			return errorMessage;
		}

		string CreateAdHocServiceJobCore(BusinessObjectFactory factory, ZGuid jobPK, ZGuid clientPK, ZGuid warehousePK, ZDate billingDate, ZString? customerReference)
		{
			var errorMessage = string.Empty;
			var adHocServiceJob = factory.NewWithPrimaryKey<WhsAdHocServiceJob>(jobPK.ToGuid());
			adHocServiceJob.WSJ_WW_Whs = warehousePK;
			adHocServiceJob.WSJ_OH_Client = clientPK;
			adHocServiceJob.WSJ_BillingDate = billingDate;
			if (customerReference.HasValue)
			{
				adHocServiceJob.WSJ_CustomerReference = customerReference.Value;
			}
			adHocServiceJob.WSJ_IsFinalised = false;
			_ = adHocServiceJob.JobHeader;
			adHocServiceJob.RunPreSaveValidation();

			if (adHocServiceJob.Notifications.GetErrors().GetUniqueMessageList().Length > 0)
			{
				errorMessage = adHocServiceJob.Notifications.GetErrors().GetUniqueMessageList()[0];
			}
			else if (adHocServiceJob.Notifications.GetMessageErrors().GetUniqueMessageList().Length > 0)
			{
				errorMessage = adHocServiceJob.Notifications.GetMessageErrors().GetUniqueMessageList()[0];
			}
			else
			{
				var concurrencyErrorMessage = Res.GetString("913ea9bf-2578-45db-a9c1-4bf25ac71c2d", "Another user may have modified the Ad Hoc Service. Please try again.");
				errorMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
			}

			return errorMessage;
		}

		#endregion

		#region CreateWhsJobService

		public string CreateWhsJobService(ZGuid servicePK, ZString serviceType, ZDecimal serviceCount, ZGuid jobPK, ZString jobType, ZDateTimeOffset? bookedDateTimeOffset, ZGuid contractor, ZDateTime? duration, ZGuid locationPK, ZString subLocation, ZString reference, ZString? note)
		{
			var factory = new BusinessObjectFactory();
			var job = GetWhsServiceJob(factory, jobPK, jobType);

			string errorMessage;
			if (job != null)
			{
				if (factory.Load<JobService>(servicePK) != null)
				{
					errorMessage = Res.GetString("cfcb2966-a272-407a-a672-104c59d6238e", "Service already exists.");
				}
				else
				{
					var whsJobService = factory.NewWithPrimaryKey<WhsJobService>(servicePK.ToGuid());
					SetWhsJobServiceInfo(factory, whsJobService, serviceType, serviceCount, jobPK, jobType, bookedDateTimeOffset, null, contractor, duration, locationPK, subLocation, reference, note);
					errorMessage = ValidateBusinessObject(whsJobService);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = SaveFactory(factory);
				}
			}
			else
			{
				errorMessage = GetSpecificJobDoesNotExistErrorMsg(jobType);
			}

			return errorMessage;
		}

		#endregion

		#region CompleteWhsJobService

		public string CompleteWhsJobService(ZGuid servicePK, ZString serviceType, ZDecimal serviceCount, ZDateTimeOffset? bookedDateTimeOffset, ZGuid contractor, ZDateTime? duration, ZGuid locationPK, ZString subLocation, ZString reference, ZString? note, bool finaliseServiceJob)
		{
			var errorMessage = string.Empty;
			var factory = new BusinessObjectFactory();

			var whsJobService = factory.Load<WhsJobService>(servicePK);

			if (whsJobService == null)
			{
				errorMessage = Res.GetString("6a939ff1-6ff4-4064-befd-18665f8db810", "Warehouse job service not found.");
			}
			else
			{
				SetWhsJobServiceInfo(factory, whsJobService, serviceType, serviceCount, ZGuid.Empty, string.Empty, bookedDateTimeOffset, ZDateTimeOffset.Now, contractor, duration, locationPK, subLocation, reference, note);
				errorMessage = ValidateBusinessObject(whsJobService);
			}

			if (finaliseServiceJob && string.IsNullOrEmpty(errorMessage))
			{
				var adHocServiceJob = factory.Load<WhsAdHocServiceJob>(whsJobService.ES_ParentID);
				adHocServiceJob.FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = ValidateBusinessObject(adHocServiceJob);
				}
			}

			if (string.IsNullOrEmpty(errorMessage))
			{
				errorMessage = SaveFactory(factory);
			}

			return errorMessage;
		}

		#endregion

		#region CompleteAllWhsJobServices

		public string CompleteAllWhsJobServices(ZGuid jobPK, ZString jobType, bool finaliseServiceJob)
		{
			var errorMessage = string.Empty;
			var factory = new BusinessObjectFactory();

			var job = GetWhsServiceJob(factory, jobPK, jobType);

			if (job != null)
			{
				foreach (WhsJobService service in job.Services.Where(s => !s.ES_CompletedDateTimeOffset.IsValid))
				{
					service.ES_CompletedDateTimeOffset = ZDateTimeOffset.Now;
				}

				if (finaliseServiceJob)
				{
					((WhsAdHocServiceJob)job).FinaliseAdHocServiceJobWithNoNotifications((error) => errorMessage = error);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = ValidateBusinessObject((BusinessObject)job);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = SaveFactory(factory);
				}
			}
			else
			{
				errorMessage = GetSpecificJobDoesNotExistErrorMsg(jobType);
			}

			return errorMessage;
		}

		#endregion

		void SetWhsJobServiceInfo(
			BusinessObjectFactory factory,
			WhsJobService whsJobService,
			ZString serviceType,
			ZDecimal serviceCount,
			ZGuid jobPK,
			ZString jobType,
			ZDateTimeOffset? bookedDateTimeOffset,
			ZDateTimeOffset? completedDateTimeOffset,
			ZGuid contractor,
			ZDateTime? duration,
			ZGuid locationPK,
			ZString subLocation,
			ZString reference,
			ZString? note)
		{
			if (!string.IsNullOrEmpty(serviceType))
			{
				whsJobService.ES_ServiceCode = serviceType;
			}
			whsJobService.ES_ServiceCount = serviceCount;
			if (jobPK.IsValid)
			{
				whsJobService.ES_ParentID = jobPK;
			}
			if (!string.IsNullOrEmpty(jobType))
			{
				whsJobService.ES_ParentTableCode = jobType;
			}
			if (bookedDateTimeOffset != null && bookedDateTimeOffset.Value.IsValid)
			{
				whsJobService.ES_BookedDateTimeOffset = bookedDateTimeOffset.Value;
			}
			if (completedDateTimeOffset != null && completedDateTimeOffset.Value.IsValid)
			{
				whsJobService.ES_CompletedDateTimeOffset = completedDateTimeOffset.Value;
			}
			if (contractor.IsValid)
			{
				whsJobService.ES_OH_Contractor = contractor;
			}
			if (duration != null && duration.Value.IsValid)
			{
				whsJobService.ES_Duration = duration.Value;
			}
			if (locationPK.IsValid)
			{
				whsJobService.ES_OA_Location = locationPK;
			}
			if (!string.IsNullOrEmpty(subLocation))
			{
				whsJobService.ES_SubLocation = subLocation;
			}
			if (!string.IsNullOrEmpty(reference))
			{
				whsJobService.ES_References = reference;
			}
			if (!string.IsNullOrEmpty(note))
			{
				whsJobService.ES_ServiceNote = note.Value;
			}
		}

		string ValidateBusinessObject(BusinessObject businessObject)
		{
			var errorMessage = string.Empty;
			businessObject.RunPreSaveValidation();

			if (businessObject.Notifications.GetErrors().GetUniqueMessageList().Length > 0)
			{
				errorMessage = businessObject.Notifications.GetErrors().GetUniqueMessageList()[0];
			}
			else if (businessObject.Notifications.GetMessageErrors().GetUniqueMessageList().Length > 0)
			{
				errorMessage = businessObject.Notifications.GetMessageErrors().GetUniqueMessageList()[0];
			}

			return errorMessage;
		}

		string SaveFactory(BusinessObjectFactory factory)
		{
			var concurrencyErrorMessage = Res.GetString("3fad3141-3b20-4c8e-8b4a-bb5eaace0198", "Another user may have modified the Service. Please try again.");
			return WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
		}

		IHaveServices GetWhsServiceJob(BusinessObjectFactory factory, ZGuid jobPK, ZString jobType)
		{
			IHaveServices job = null;
			if (jobType.Equals(WhsAdHocServiceJobSchema.Constants.Prefix))
			{
				job = factory.Load<WhsAdHocServiceJob>(jobPK);
			}
			else if (jobType.Equals(WhsDocketSchema.Constants.Prefix))
			{
				job = factory.Load<WhsDocket>(jobPK);
			}
			else if (jobType.Equals(WhsVASOrderSchema.Constants.Prefix))
			{
				job = factory.Load<WhsVASOrder>(jobPK);
			}
			return job;
		}

		string GetSpecificJobDoesNotExistErrorMsg(ZString jobType)
		{
			var errorMsg = string.Empty;
			if (jobType.Equals(WhsAdHocServiceJobSchema.Constants.Prefix))
			{
				errorMsg = Res.GetString("5db9c5b5-4f16-43dc-bb50-743ebca50446", "Ad hoc service job not found.");
			}
			else if (jobType.Equals(WhsDocketSchema.Constants.Prefix))
			{
				errorMsg = Res.GetString("4f267f58-b155-4a34-a4bc-e34d9dcaaba4", "Warehouse docket not found.");
			}
			else if (jobType.Equals(WhsVASOrderSchema.Constants.Prefix))
			{
				errorMsg = Res.GetString("9c05ed72-74d8-4efc-b8fa-09ecd8d7b2df", "VAS order not found.");
			}
			return errorMsg;
		}
	}
}
