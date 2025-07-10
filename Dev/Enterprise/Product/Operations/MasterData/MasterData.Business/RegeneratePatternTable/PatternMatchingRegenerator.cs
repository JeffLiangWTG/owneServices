using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingRegenerator<TBizo> : IPatternMatchingRegenerator<TBizo>
		where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		protected PatternMatchingRecalculator<TBizo> patternMatchingRecalculator;

		protected PatternMatchingRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
		{
			patternMatchingRecalculator = recalculator;
		}

		protected abstract List<string> TablesPrefixList { get; }

		public abstract int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int GetActualAddCount();

		protected abstract int Delete();

		protected abstract int Add(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int Update(TBizo bizo, BusinessObjectFactory factory);

		protected void ReportProgress()
		{
			var recalculatingEventArgs = patternMatchingRecalculator?.RecalculatingEventArgs;

			if (recalculatingEventArgs != null)
			{
				recalculatingEventArgs.Current++;

				recalculatingEventArgs.ProgressText = string.Format(CultureInfo.InvariantCulture, Res.GetString("1864542C-0A1A-41C1-A611-1C3BE0AC33A8", "Recalculating Hashes:- {0} of {1} ({2}%)"),
					recalculatingEventArgs.Current,
					recalculatingEventArgs.Total,
					recalculatingEventArgs.Progress);

				patternMatchingRecalculator.RecalculatingInvokeMethod(recalculatingEventArgs);
			}
		}

		public int Regenerate(TBizo bizo, BusinessObjectFactory factory)
		{
			if (patternMatchingRecalculator.DebuggerMessages == null)
			{
				patternMatchingRecalculator.DebuggerMessages = new List<DeduplicationDebuggerMaster>();
			}

			var deletedItems = Delete();
			var updatedItems = Update(bizo, factory);
			var addedItems = Add(bizo, factory);

			factory.Save();

			return deletedItems + updatedItems + addedItems;
		}

		#region  IPatternMatchingRegenerator

		public int InitializeDataCounter(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCount(bizo, factory);
		}

		public int RegeneratePatterns(TBizo bizo, BusinessObjectFactory factory)
		{
			return Regenerate(bizo, factory);
		}

		#endregion
	}
}
