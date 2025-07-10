using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface IDataBuilder<T> where T : class
	{
		void BuildXml(DateTime publicationDate, IList<T> data, string outputPath);
	}
}
