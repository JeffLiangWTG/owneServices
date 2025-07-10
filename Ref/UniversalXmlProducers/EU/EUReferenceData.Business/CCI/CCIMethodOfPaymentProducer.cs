using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class CCIMethodOfPaymentProducer : CommonXMLProducer
	{
		public CCIMethodOfPaymentProducer() :
			base(Constants.CCIMethodOfPayment.OutputFileName,
				Constants.CCIMethodOfPayment.OutputFileDataSource,
				XmlWriterHelper.GetRefNctsCodesWriterConfiguration(Constants.CCIMethodOfPayment.CodeType))
		{
		}

		protected override CommonDataParser DataParser => new CCIMethodOfPaymentDataParser();

		protected override Dependency[] GetDependencies(DateTime? dependencyDateTime)
		{
			return new Dependency[]
			{
				new Dependency(RefCusCodeTypeProducer.DataSource, dependencyDateTime.Value, DependencyType.Required)
			};
		}

		protected override RefCusCodeTypeProducer RefCusCodeTypeProducer => new CL104IM_RefCusCodeTypeProducer();
	}
}
