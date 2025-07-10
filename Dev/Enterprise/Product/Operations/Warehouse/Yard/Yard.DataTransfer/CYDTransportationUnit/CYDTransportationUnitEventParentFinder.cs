using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal
{
	public class CYDTransportationUnitEventParentFinder : EventParentFinder
	{
		public CYDTransportationUnitEventParentFinder(BusinessObjectFactory factory, IEventDataContextManager manager, IXmlImportLogger logger) : base(factory, manager, logger)
		{
		}

		protected override BusinessObject[] GetLogParentsForEventUsingContext(UniversalEvent xmlEvent)
		{
			if (xmlEvent.HasRecipientRole(RecipientRoleType.CYD))
			{
				var result = (string)xmlEvent.EventType switch
				{
					AutoEvents.CancelledCode => FindMatchingTransportationUnit(xmlEvent, DataContextType.GateVehicleMovement).Cast<BusinessObject>().ToArray(),
					_ => [],
				};
				return result;
			}

			return null;
		}

		CYDTransportationUnit[] FindMatchingTransportationUnit(UniversalEvent xmlEvent, DataContextType dataContextType)
		{
			var dataSource = xmlEvent.GetMatchingDataSource(dataContextType);
			if (dataSource?.Key is null)
			{
				throw new DataObjectReadFailureException("Matching data source not found");
			}

			var stmUniversalJobLinkQuery = new ZDBOnlySubQuery(typeof(StmUniversalJobLink), StmUniversalJobLinkSchema.UCL_ParentID);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_ParentTableCode, CYDTransportationUnitSchema.Constants.Prefix);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceType, dataSource.Type);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_SourceKey, dataSource.Key);
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_CompanyCode, logger.TopLevelDataContext.GetCompanyCode());
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_EnterpriseCode, logger.TopLevelDataContext.GetEnterpriseCode());
			stmUniversalJobLinkQuery.AddToFilter(StmUniversalJobLinkSchema.UCL_ServerCode, logger.TopLevelDataContext.GetServerCode());

			var query = new ZDBOnlyQuery(typeof(CYDTransportationUnit));
			query.AddSubQuery(stmUniversalJobLinkQuery, JoinCondition.And);
			var tpu = factory.LoadTop1<CYDTransportationUnit>(query) ?? throw new DataObjectReadFailureException("Matching TPU not found");

			return new CYDTransportationUnit[] { tpu };
		}
	}
}
