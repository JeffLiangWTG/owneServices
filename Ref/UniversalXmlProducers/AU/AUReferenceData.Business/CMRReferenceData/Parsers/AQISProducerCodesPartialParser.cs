using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business.CMRReferenceData
{
	public class AQISProducerCodesPartialParser : AQISProducerCodesParser, ICMRReferenceDataPartialParser
	{
		protected override UpdateType UpdateType => UpdateType.Partial;

		protected override string IndexUri => ApplicationConfig.AUReferenceFilesPartialDirectory;

		protected override string FileNamePrefix => ApplicationConfig.AQISProducerCodesPartialFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISProducerCodes_Partial.xml";

		protected override bool SaveWriter => true;

		protected override void ParseCore(IXmlWriter xmlWriter, string content) => Processor.Parse(xmlWriter, content, this);

		#region ICMRReferenceDataPartialParser

		RefDataRepoModelEntityType ICMRReferenceDataPartialParser.ProcessLine(string line) => ProcessLine(line);

		DateTime ICMRReferenceDataPartialParser.PublishedDate => PublishedDate;

		public int ActionIndicatorPosition => 294;

		public IPartialProcessor Processor => new RefCusCodeListPartialProcessor(GetRefDataLoader(), CodeType);

		#endregion

		protected virtual IRefDataLoader GetRefDataLoader() => new RefDataLoader();
	}
}
