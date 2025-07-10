using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.NLReferenceData.Business;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface ITariffBuilder
	{
		void BuildXml(DateTime publicationDate, IEnumerable<Measure> data, string outputPath);
	}
}
