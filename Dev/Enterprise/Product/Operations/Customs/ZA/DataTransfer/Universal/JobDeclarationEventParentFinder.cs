using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	class JobDeclarationEventParentFinder : Customs.DataTransfer.Universal.JobDeclarationEventParentFinder
	{
		public JobDeclarationEventParentFinder(BusinessObjectFactory factory, JobDeclarationDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override void UpdateEntryDetails(CusEntryHeader entry, Event eventDataObject)
		{
			base.UpdateEntryDetails(entry, eventDataObject);
			if (entry.EntryNumber.IsEmpty)
			{
				entry.EntryNumber = ((IXmlEventValueObject)eventDataObject).Context.EntryNumber;
			}
		}
	}
}
