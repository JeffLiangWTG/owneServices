using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	[UniversalDataContext(DataContextType.LandTransportConsignmentConsol)]
	public class LTConsignmentConsolidation : NonPersistentBusinessObject, IJobNumber, IStmALogParent
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Job Number on Non persistent BizO must not be translatable.")]
		public string JobNumber { get { return "Non persistent consol"; } }

#if DEBUG
		[BusinessObjectTestExclude]
		public IReadOnlyList<DtbConsignment> ConsignmentsCreatedDuringImport_ForTesting { get; set; }
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
	}
}
