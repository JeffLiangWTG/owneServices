using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.DataTransfer.Universal.AccCashAdvanceRequestMessageConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public class AccCashAdvanceRequestHeaderEventParentFinder : EventParentFinder
	{
		public AccCashAdvanceRequestHeaderEventParentFinder(BusinessObjectFactory factory, AccCashAdvanceRequestHeaderDataContextManager manager, IXmlImportLogger logger)
			: base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			if (xmlEvent.DataContext?.DataTargetCollection == null
							|| !xmlEvent.DataContext.DataTargetCollection.Any(x => x.Type.HasValue && x.Type.Value == nameof(DataContextType.AccCashAdvanceRequest)))
			{
				return null;
			}
			var cashAdvanceReference = xmlEvent.GetMatchingDataTarget(manager.DataContextType)?.Key ?? ZString.Empty;
			if (string.IsNullOrEmpty(cashAdvanceReference))
			{
				logger.LogBoth(LogType.Error, $"Could not find a valid Advance Payment Reference Number in the <DataTarget> Key parameter. Value found: '{cashAdvanceReference}'");
				return null;
			}

			if (xmlEvent.ContextCollection == null)
			{
				logger.LogBoth(LogType.Error, (NoResString)"No Context Collection found.");
				return null;
			}

			var companyCode = xmlEvent.DataContext.CompanyCodeToImportInto;
			GlbCompany company = null;
			if (companyCode.IsEmpty)
			{
				logger.LogBoth(LogType.Error, $"Missing or Empty field {XUEFieldNames.Company} in Universal Event.");
			}
			else
			{
				company = factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, companyCode);
			}

			var query = new ZQuery(AccCashAdvanceRequestHeaderSchema.CAH_RequestReferenceNumber, cashAdvanceReference);
			query.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_GC_Company, company?.PK ?? ZGuid.Empty);
			return factory.Load<AccCashAdvanceRequestHeader>(query);
		}
	}

	public static class AccCashAdvanceRequestMessageConstants
	{
		public static class XUEFieldNames
		{
			public static ZString Company => nameof(Company);
			public static ZString LedgerType => nameof(LedgerType);
			public static ZString Status => nameof(Status);
			public static ZString[] GetRequiredFields() => new[] { LedgerType, Status };
		}
	}
}
