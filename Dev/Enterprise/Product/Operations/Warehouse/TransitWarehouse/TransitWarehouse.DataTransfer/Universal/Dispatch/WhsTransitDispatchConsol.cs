using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	[UniversalDataContext(DataContextType.TransitDispatchConsol)]
	public class WhsTransitDispatchConsol : NonPersistentBusinessObject, IJobNumber, IStmALogParent
	{
		#region JobNumber

		public string JobNumber { get { return (NoResString)"Non persistent consol"; } }  // Job Number on Non persistent BizO must not be translatable.

		#endregion

		#region IStmALogParent members

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return System.Array.Empty<BusinessObject>(); }
		}

		Logs IStmALogProvider.Logs
		{
			get { return new Logs(this); }
		}

		BusinessObjectFactory IStmALogProvider.LogsFactory
		{
			get { return new ReadOnlyBusinessObjectFactory(); }
		}

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return ""; }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
			// do nothing
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region Testing

#if DEBUG
		[BusinessObjectTestExclude]
		public List<WhsItemDispatchConsignment> PopulatedConsignmentsForTesting { get; internal set; }

		[BusinessObjectTestExclude]
		public List<WhsItemDispatchTransportationUnit> PopulatedDispatchTransportationUnitsForTesting { get; internal set; }

		[BusinessObjectTestExclude]
		public WhsItemDispatchLoadList PopulatedDispatchLoadListForTesting { get; internal set; }
#endif

		#endregion
	}
}
