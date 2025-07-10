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
	[UniversalDataContext(DataContextType.TransitReceiveConsol)]
	public class WhsTransitReceiveConsol : NonPersistentBusinessObject, IJobNumber, IStmALogParent
	{
		public string JobNumber { get { return (NoResString)"Non persistent consol"; } } // Job Number on Non persistent BizO must not be translatable.

#if DEBUG
		[BusinessObjectTestExclude]
		public WhsItemReceiveConsignment[] PopulatedConsignmentsForTesting { get; internal set; }
#endif

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
			get
			{
				// Should not save any logs added to this bizo. Universal will try to add a DIM event.
				return new ReadOnlyBusinessObjectFactory();
			}
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
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion
	}
}
