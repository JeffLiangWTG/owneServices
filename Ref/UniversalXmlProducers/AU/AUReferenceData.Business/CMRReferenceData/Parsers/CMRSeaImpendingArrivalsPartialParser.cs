using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using static CargoWise.RefDbRepo.AUReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class CMRSeaImpendingArrivalsPartialParser : CMRSeaImpendingArrivalsParser
	{
		protected override UpdateType UpdateType => UpdateType.Partial;
		protected override string IndexUri => ApplicationConfig.AUReferenceFilesPartialDirectory;
		protected override string FileNamePrefix => ApplicationConfig.CMRSeaImpendingArrivalsPartialFilePrefix;
		protected override string OutputXMLName => "RefVesselZZ_AU_CMRSeaImpendingArrivals_Partial.xml";

		static PropertyMapping<ChangeReport>[] ChangeReportMappings => new PropertyMapping<ChangeReport>[]
		{
			new PropertyMapping<ChangeReport>(entity => entity.ActionIndicator, 226, 1)
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var changeConverter = new LineToEntityConverter<ChangeReport>(ChangeReportMappings, 1);

			foreach (var line in content.NonEmptyLines())
			{
				var change = changeConverter.Convert(line);
				if (change.ActionIndicator != ChangeReportActions.Delete)
				{
					var code = ProcessLine(line);
					if (code != null)
					{
						xmlWriter.PopulateData(code);
					}
				}
			}
		}
	}
}
