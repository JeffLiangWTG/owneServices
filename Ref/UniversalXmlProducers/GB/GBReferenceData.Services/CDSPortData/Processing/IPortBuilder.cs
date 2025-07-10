using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.Processing
{
	public interface IPortBuilder
	{
		void BuildXml(DateTime publicationDate, IEnumerable<PortData> data, string outputPath, Task<IEnumerable<CCSUKLocation>> ccsukData);
	}
}
