using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	public class CommonExcelBuilderForTest : IDataBuilder<CommonExcelTestData>
	{
		public int BuildCallCount { get; set; }
		public int BuildModelCount { get; set; }

		public void BuildXml(DateTime publicationDate, IList<CommonExcelTestData> data, string outputPath)
		{
			BuildCallCount++;
			BuildModelCount += data.Count;
		}
	}
}
