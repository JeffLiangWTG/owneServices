using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CCIPreviousDocumentTypeProducer : CommonXMLProducer
	{
		public CCIPreviousDocumentTypeProducer() :
			base(Constants.CCIPreviousDocumentType.OutputFileName,
				Constants.CCIPreviousDocumentType.OutputFileDataSource,
				XmlWriterHelper.GetRefNctsCodesWriterConfiguration(Constants.CCIPreviousDocumentType.CodeType))
		{
		}

		protected override CommonDataParser DataParser => new CCIPreviousDocumentTypeDataParser();

		protected override Dependency[] GetDependencies(DateTime? dependencyDateTime)
		{
			return new Dependency[]
			{
				new Dependency(RefCusCodeTypeProducer.DataSource, dependencyDateTime.Value, DependencyType.Required)
			};
		}

		protected override RefCusCodeTypeProducer RefCusCodeTypeProducer => new CL214IM_RefCusCodeTypeProducer();
	}
}
