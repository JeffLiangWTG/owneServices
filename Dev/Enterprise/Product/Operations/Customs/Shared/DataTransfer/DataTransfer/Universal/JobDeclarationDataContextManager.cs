using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public sealed class JobDeclarationDataContextManager : ShipmentDataContextManager<BaseJobDeclaration>, IJobDeclarationDataContextManager, IDataContextManagerFromEDIMessage, IEventTransformer, IEventDataContextManagerWithTriggeringLog
	{
		public override DataContextType DataContextType
		{
			get { return DataContextType.CustomsDeclaration; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.JE_DeclarationReference; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			ZQuery result = null;
			if (matchingValues.DataObject is UniversalEvent)
			{
				result = JobDeclarationEventHelper.GetJobDeclarationQueryFor(factory, matchingValues.Key, matchingValues.CompanyCode, matchingValues.DataObject?.DataContext?.CountryCodeToImportInto ?? ZString.Empty);
			}
			else
			{
				// Universal Architecture has already setup the correct environment
				result = new ZQuery(JobDeclarationSchema.JE_DeclarationReference, matchingValues.Key);
				result.AddToFilter(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			}

			return result;
		}

		public override string DefaultOutputDirectory
		{
			get { return SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			var result = new List<KeyValuePair<TypeWithDescription, IZType>>();
			if (ParentBO != null)
			{
				var triggeringLog = ((IEventDataContextManagerWithTriggeringLog)this).TriggeringLogForUseInPopulatingEventContext;

				var jobDeclarationEventContextReader = GetCountrySpecificEventContextReader();
				jobDeclarationEventContextReader.AddJobDeclarationContextValues(result, triggeringLog?.Master as CusEntryHeader);
			}

			return result.Count == 0 ? null : result;
		}

		protected override IEnumerable<KeyValuePair<IZType, IZType>> GetAdditionalFieldsToUpdateValues()
		{
			var result = new List<KeyValuePair<IZType, IZType>>();
			if (ParentBO != null)
			{
				var jobDeclarationEventContextReader = GetCountrySpecificEventContextReader();
				jobDeclarationEventContextReader.AddAdditionalFieldsToUpdateValues(result);
			}

			return result.Count == 0 ? null : result;
		}

		JobDeclarationEventContextReader GetCountrySpecificEventContextReader()
		{
			var countryCode = ParentBO != null ? ParentBO.CountryCode.ToString() : GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			return ObjectFactory.GetCountrySpecificOrDefault<JobDeclarationEventContextReader>(countryCode, ParentBO);
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var countryCode = ParentBO != null ? ParentBO.CountryCode.ToString() : GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString();
			return ObjectFactory.GetCountrySpecificOrDefault<JobDeclarationEventParentFinder>(countryCode, factory, this, logger);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return recipientRoles.Any(o => o.Code == RecipientRoleType.BRO || o.Code == RecipientRoleType.BRI || o.Code == RecipientRoleType.BRE);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			if (universalShipment.GetMatchingDataTarget(DataContextType.ForwardingShipment) != null)
			{
				return null; // Should let the shipment context manager process this universal shipment and hence stopping the system from processing this twice.
			}
			else
			{
				return new CustomsShipmentDataObjectReaderProvider().GetReader(universalShipment, logger, factory, null);
			}
		}

		public override bool ManagesShipments
		{
			get { return true; }
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return GetShipmentDataObjectWriter(writeManager, ParentBO);
		}

		internal static ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager, BaseJobDeclaration declaration)
		{
			if (declaration == null)
			{
				declaration = writeManager.Action != null ? writeManager.Action.ParentBO as BaseJobDeclaration : null;
			}
#if DEBUG
			if (declaration == null)
			{
				throw new System.InvalidOperationException("Unable to get JobDeclaration from DataContextManager.ParentBO or IDataWritingManager.Action.ParentBO to retrieve country-specific writer.");
			}
#endif
			return GetDeclarationDataObjectWriter(writeManager, declaration) ?? new DeclarationDataObjectWriter(writeManager);
		}

		static ITopLevelDataObjectWriter GetDeclarationDataObjectWriter(IDataWritingManager writeManager, BaseJobDeclaration declaration)
		{
			ITopLevelDataObjectWriter result = null;
			if (declaration != null)
			{
				IUniversalCustomsDataObjectProvider provider = null;
				var applicationCode = declaration.JE_ApplicationCode;
				if (!applicationCode.IsEmpty)
				{
					provider = declaration.Factory.GetApplicationSpecificUniversalCustomsDataObjectProvider(applicationCode);
				}

				if (provider == null)
				{
					var countryCode = declaration.GetCountryCodeSafe();
					if (!countryCode.IsEmpty)
					{
						provider = declaration.Factory.GetUniversalCustomsDataObjectProvider(countryCode);
					}
				}

				result = provider?.GetNewDeclarationDataObjectWriter(writeManager);
			}
			return result;
		}

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			if (businessObject != null)
			{
				foreach (var processor in JobDeclarationEventProcessorsFactory.All)
				{
					if (processor.ProcessMessage(logger, eventDataObject, message, businessObject))
					{
						break;
					}
				}
			}
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			base.OnUniversalEventAddedCore(logger, eventAdded);
			(ParentBO as Business.Interfaces.IOnUniversalEventAddedHandler)?.OnUniversalEventAdded(logger, eventAdded);
		}

		public EventValue Transform(EventValue sourceEventValue, UniversalEvent sourceUniversalEvent, IStmALogParent logParent)
		{
			var logParentContainer = logParent as ForwardingContainer;

			return logParentContainer != null
				? ForwardingContainerEventTransformer.Transform(sourceEventValue, sourceUniversalEvent, logParentContainer)
				: sourceEventValue;
		}

		BaseStmALog IEventDataContextManagerWithTriggeringLog.TriggeringLogForUseInPopulatingEventContext { get; set; }
	}
}
