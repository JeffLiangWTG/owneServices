using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TW.Business
{
	public static class CloneStrategyHelper
	{
		public static void DeepCloneNotesTo(this IEnumerable<StmNote> notes, Notes clonedResult)
		{
			notes.ForEach(note => clonedResult.Add(note.Clone()));
		}
	}
}
