using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class OrganizationDataContextManager : EventDataContextManager<OrgHeader>, IDataContextManagerFromEDIMessage
	{
		const string DunsNumberType = "DunsNumber";
#pragma warning disable CW1161 // Res.GetString Analyzer
		const string EnrollmentRequestMessageType = "Enrollment Request";
#pragma warning restore CW1161 // Res.GetString Analyzer

		public override DataContextType DataContextType
		{
			get { return DataContextType.Organization; }
		}

		public override ZString DataContextKey
		{
			get { return ParentBO.OH_Code; }
		}

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var result = new ZQuery(OrgHeaderSchema.OH_Code, matchingValues.Key);

			return result;
		}

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new OrganizationEventParentFinder(factory, this, logger);
		}

		protected override void OnUniversalEventAddedCore(IXmlSessionTracker logger, UniversalEvent eventAdded)
		{
			var isEventTypeMAA = (eventAdded.EventType ?? string.Empty) == Events.MessageAccepted.Code;

			if (isEventTypeMAA)
			{
				var isBoleroEvent = IsBoleroEventParameters(eventAdded.EventParameters);
				var isReferenceNumberEmpty = eventAdded.EventParameters?.ReferenceNumber.Value.IsEmpty ?? true;
				var isBoleroRecipientRole = IsBoleroRecipientRole(eventAdded);

				if (isBoleroEvent && isBoleroRecipientRole && !isReferenceNumberEmpty)
				{
					var orgCusCodeTRI = ParentBO.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(code => code.OK_CodeType == OrgCusCode.CodeTypes.BoleroTitleRegisterID) ?? ParentBO.CustomsCodes.AddNew();

					orgCusCodeTRI.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
					orgCusCodeTRI.OK_CustomsRegNo = eventAdded.EventParameters.ReferenceNumber.Value;

					if (ParentBO.OH_IsConsignor)
					{
						ParentBO.MiscServ.OM_FWRequiresElectronicBOLForDirectConsol = true;
					}

					if (ParentBO.OH_IsForwarder)
					{
						ParentBO.MiscServ.OM_FWRequiresElectronicBOLForNonDirectConsol = true;
					}

					logger.Log(LogType.Information, $"MAA event processed successfully, and the Bolero Entity Identifier(TRI) code value {orgCusCodeTRI.OK_CustomsRegNo} will be updated into Org {ParentBO.OH_Code}.");
				}
				else if (isBoleroEvent || isBoleroRecipientRole)
				{
					var errorMessages = new List<string>();

					if (isBoleroEvent && !isBoleroRecipientRole)
					{
						logger.Log(LogType.Error, $"Invalid RecipientRole : RecipientRole with code '{MessageRecipientPartyTypeList.Codes.Bolero}' not found in RecipientRoleCollection.");
					}
					else if (isBoleroRecipientRole && !isBoleroEvent)
					{
						logger.Log(LogType.Error, $"Invalid EventParameters : Expected MessageType '{EnrollmentRequestMessageType}' and Department '{nameof(MessageRecipientPartyTypeList.Codes.Bolero)}' not found in EventParameters.");
					}

					if (isReferenceNumberEmpty)
					{
						logger.Log(LogType.Error, "Invalid EventParameters : ReferenceNumber in EventParameters is missing.");
					}
				}
			}

			base.OnUniversalEventAddedCore(logger, eventAdded);
		}

		bool IsBoleroEventParameters(EventParameters eventParameters) =>
			(eventParameters.MessageType ?? string.Empty) == EnrollmentRequestMessageType &&
			(eventParameters?.Department.Value ?? string.Empty) == nameof(MessageRecipientPartyTypeList.Codes.Bolero);

		public void OnLogParentFoundFromEDIMessage(IXmlSessionTracker logger, IXmlEventValueObject eventDataObject, IEDIMessage message, BusinessObject businessObject)
		{
			var hashtable = ObjectFactory.Get<Hashtable>("OrganizationEventProcessors");
			var dataSourceCollection = eventDataObject?.DataContext?.DataSourceCollection;
			if (dataSourceCollection != null)
			{
				foreach (var target in dataSourceCollection.Where(x => x.Type.GetValueOrDefault().EqualsIgnoringCase("DataProvider") && (!x.Key?.IsEmpty ?? false)).Select(x => (string)x.Key))
				{
					if (hashtable[target] is ObjectHandle targetHandle && targetHandle.GetObject() is IEventProcessor targetProcessor)
					{
						targetProcessor.ProcessMessage(logger, eventDataObject, message, businessObject);
					}
				}
			}
		}

		class OrganizationEventParentFinder : EventParentFinder
		{
			internal OrganizationEventParentFinder(BusinessObjectFactory factory, OrganizationDataContextManager manager, IXmlImportLogger logger)
				: base(factory, manager, logger)
			{
			}

			protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
			{
				BusinessObject[] results = null;

				var dunsContext = xmlEvent?.ContextCollection?.FirstOrDefault(c => (string)c.Type.Type == DunsNumberType && !string.IsNullOrWhiteSpace(c.Value));

				if (dunsContext != null)
				{
					results = LoadOrganisationsByDuns(dunsContext.Value);
				}
				else
				{
					if (IsBoleroRecipientRole(xmlEvent))
					{
						var key = xmlEvent.DataContext?.DataTargetCollection?.FirstOrDefault()?.Key;
						var pk = ZGuid.ParseSafe(key);
						results = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, pk));
					}
				}

				return results;
			}

			BusinessObject[] LoadOrganisationsByDuns(string duns)
			{
				var dunsQuery = new ZDBOnlyQuery(typeof(OrgHeader));
				var findDunsQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				findDunsQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, "DUN");
				findDunsQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, duns);
				dunsQuery.AddSubQuery(findDunsQuery, JoinCondition.And);

				return factory.Load<OrgHeader>(dunsQuery);
			}
		}

		static bool IsBoleroRecipientRole(UniversalEvent xmlEvent) => xmlEvent.DataContext?.RecipientRoleCollection?.FirstOrDefault(c => c.Code.Value == RecipientRoleType.BOR) != null;
	}
}

