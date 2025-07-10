using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TreatmentCodesProducer : IProducer
	{
		public TreatmentCodesProducer(DateTime publicationTime = default(DateTime), string outPutFilePath = null)
		{
			this._outPutFilePath = outPutFilePath;
			this.publicationTime = publicationTime;
		}

		public string OutPutFilePath => _outPutFilePath ?? (_outPutFilePath = Path.Combine(ApplicationConfig.OutputPath, PublicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string _outPutFilePath;

		public DateTime PublicationTime
		{
			get
			{
				if (publicationTime == default(DateTime))
				{
					DateTime ttCodePublicationTime = DateTime.TryParse(ApplicationConfig.TTCodePublicationTime, out ttCodePublicationTime)
						? ttCodePublicationTime : DateTime.Today;
					return ttCodePublicationTime;
				}
				else
				{
					return publicationTime;
				}
			}
		}

		DateTime publicationTime;

		public string FunctionCode => Constants.ProgramFunctions.TTCode;

		public string WorkingFileOrDirectory => _workingFolder ?? (_workingFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.TTCodeFileName));
		string _workingFolder;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start parsing TT Code.");
			if (ParserHelper.TTCodeList.Any())
			{
				Console.WriteLine($"There will be {ParserHelper.TTCodeList.Count} TT Codes populated into the final XML");
				var xmlWriter = new XmlWriter(XMLWriterHelper.GetRefCusPreferenceWriterConfiguration());
				xmlWriter.SetDataSource(Constants.DataSource.Preference);
				xmlWriter.SetUpdateType(UpdateType.Full);

				foreach (var ttCode in ParserHelper.TTCodeList)
				{
					var preference = new RefCusPreference()
					{
						ZZS_Preference = ttCode.Code,
						ZZS_Description = ttCode.Description
					};
					xmlWriter.PopulateData(preference);
				}

				xmlWriter.SetPublicationTime(PublicationTime);
				xmlWriter.SaveXml(OutPutFilePath);
				Console.WriteLine($"Final XML populated {OutPutFilePath}.");
			}
		}
	}
}
