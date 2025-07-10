using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DeniedPartyScreening.Integration
{
	public interface IStmEntityScreeningLogCollection : IList
	{
		void InvalidateByLocalDataChanges();
		string MostRecentStatus { get; }
		new IStmEntityScreeningLog this[int index] { get; }
		void AddUpdateRelatedJobScreeningStatusLog();
		void AddMarkAsJobClearLog(string clearReason);

		void AddRemoveOrInsertPartyScreeningLog(List<ScreeningPartiesSnapshot> orglist, List<ScreeningPartiesSnapshot> curlist);
	}

	public struct ScreeningPartiesSnapshot
	{
		public string Description { get; set; }
		public ZGuid Key { get; set; }
	}
}
