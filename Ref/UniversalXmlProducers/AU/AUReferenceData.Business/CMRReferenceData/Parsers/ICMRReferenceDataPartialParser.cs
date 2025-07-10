using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public interface IPartialProcessor
	{
		void Parse(IXmlWriter xmlWriter, string content, ICMRReferenceDataPartialParser parser);
	}

	public interface ICMRReferenceDataPartialParser
	{
		RefDataRepoModelEntityType ProcessLine(string line);
		DateTime PublishedDate { get; }
		int ActionIndicatorPosition { get; }
		IPartialProcessor Processor { get; }
	}
}
