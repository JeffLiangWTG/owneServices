using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DataTransfer.Universal.Extensions;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	class ISFEventParentFinder : EventParentFinder
	{
		public ISFEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent eventDataObject)
		{
			BusinessObject[] result;
			if (eventDataObject.IsUSCATAIRMessageEvent(DataContextType.USImporterSecurityFiling) && USCATAIRMessageEventParentFinder.IsBondStatusNotificationMessageEvent(eventDataObject))
			{
				var eBondProcessor = new ISFBondEventProcessor(eventDataObject, factory);
				result = eBondProcessor.Process();
			}
			else
			{
				var headerQuery = new ZDBOnlyQuery(typeof(CusISFHeader));
				var eventValueObject = (IXmlEventValueObject)eventDataObject;
				if (!eventValueObject.Context.DeclarationReference.IsEmpty)
				{
					headerQuery.AddToFilter(CusISFHeaderSchema.BF_JobReference, eventValueObject.Context.DeclarationReference);
				}
				else if (!eventValueObject.Context.EntryNumber.IsEmpty
						 && eventValueObject.Context.EntryNumberType == ISFConstants.EntryNumberConstants.ISF
						 && eventValueObject.Context.EntryNumberCountryOfIssue == Core.Constants.CountryCodes.UnitedStates)
				{
					headerQuery.AddToFilter(CusISFHeaderSchema.BF_CustomsReference, eventValueObject.Context.EntryNumber);
				}
				else
				{
					return null;
				}

				var headers = factory.Load<CusISFHeader>(headerQuery);
				result = (headers.Length > 0) ? headers.Cast<BusinessObject>().ToArray() : null;
			}
			return result;
		}
	}
}
