using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface IExchangeRatesBuilder<TNode>
	{
		void BuildXml(DateTime publicationDate, IEnumerable<TNode> data, string outputPath);
	}
}
