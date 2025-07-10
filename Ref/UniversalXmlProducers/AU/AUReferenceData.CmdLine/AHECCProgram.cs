using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Business;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	public class AHECCProgram
	{
		public void RunPartialUpdate()
		{
			PartialParser.Parse();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Aggregating")]
		public void RunFullUpdate()
		{
			var exceptions = new List<Exception>();

			try
			{
				FullParser.Parse();
			}
			catch (Exception e)
			{
				exceptions.Add(e);
			}

			try
			{
				FullTestParser.Parse();
			}
			catch (Exception e)
			{
				exceptions.Add(e);
			}

			if (exceptions.Count > 0)
			{
				throw new AggregateException(exceptions);
			}
		}

		protected virtual IDateTimeProvider DateTimeProvider => new DateTimeProvider();

		protected virtual BaseAHECCParser PartialParser => new AHECCPartialParser(DateTimeProvider);
		protected virtual BaseAHECCParser FullParser => new AHECCFullParser(DateTimeProvider);
		protected virtual BaseAHECCParser FullTestParser => new AHECCFullTestParser(DateTimeProvider);
	}
}
