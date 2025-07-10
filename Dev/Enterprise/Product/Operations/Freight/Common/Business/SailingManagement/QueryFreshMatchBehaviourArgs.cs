using CargoWise.Types;
using Enterprise.Security;

namespace Enterprise.Freight.Common.Business
{
	public class QueryFreshMatchBehaviourArgs
	{
		#region Properties

		public SecurityCheckpoint AddCheckpoint
		{
			get;
			set;
		}

		public SecurityCheckpoint EditCheckpoint
		{
			get;
			set;
		}

		public ZString DateName
		{
			get;
			set;
		}

		public ZString TransportMode { get; set; }
		public ZBool IsImportingData { get; set; }

		public ZDateTime RequestedDate
		{
			get;
			set;
		}

		public ZDateTime FoundDate
		{
			get;
			set;
		}

		#endregion
	}
}
