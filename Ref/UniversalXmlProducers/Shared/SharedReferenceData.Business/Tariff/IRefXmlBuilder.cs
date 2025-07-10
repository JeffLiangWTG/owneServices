using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public interface IRefXmlBuilder
	{
		void BuildXml(DateTime publicationDate, List<ITariffModel> data, string outputPath, string chapterFilter);
	}
}
