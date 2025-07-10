using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class AQISPremisesCodesPartialParser : AQISPremisesCodesParser, ICMRReferenceDataPartialParser
	{
		protected override UpdateType UpdateType => UpdateType.Partial;
		protected override string IndexUri => ApplicationConfig.AUReferenceFilesPartialDirectory;
		protected override string FileNamePrefix => ApplicationConfig.AQISPremisesCodesPartialFilePrefix;
		protected override string OutputXMLName => "RefCusCodeList_AU_AQISPremisesCodes_Partial.xml";

		protected override void ParseCore(IXmlWriter xmlWriter, string content) => Processor.Parse(xmlWriter, content, this);

		#region ICMRReferenceDataPartialParser

		RefDataRepoModelEntityType ICMRReferenceDataPartialParser.ProcessLine(string line) => ProcessLine(line);

		DateTime ICMRReferenceDataPartialParser.PublishedDate => PublishedDate;

		public int ActionIndicatorPosition => 259;

		public IPartialProcessor Processor => new RefCusCodeListPartialProcessor(GetRefDataLoader(), CodeType);

		#endregion

		protected virtual IRefDataLoader GetRefDataLoader() => new RefDataLoader();
	}
}
