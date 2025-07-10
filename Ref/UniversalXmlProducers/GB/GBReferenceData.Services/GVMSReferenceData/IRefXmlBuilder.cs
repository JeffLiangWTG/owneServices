using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Models;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData
{
	public interface IRefXmlBuilder
	{
		void BuildXml(string outputPath, ReferenceData referenceData, DateTime publicationDate);
	}
}
