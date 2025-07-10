using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ChapterToSectionMapper : IChapterToSectionMapper
	{
		IDictionary<int, ISection> _chapterToSectionMap;

		protected IDictionary<int, ISection> ChapterToSectionMap
		{
			get
			{
				if (_chapterToSectionMap == null || _chapterToSectionMap.Count == 0)
				{
					Initialize();
				}

				return _chapterToSectionMap;
			}
		}
		public int GetSection(int chapterNumber)
		{
			if (ChapterToSectionMap.ContainsKey(chapterNumber))
			{
				var section = ChapterToSectionMap[chapterNumber];
				var sectionNumber = section.Number;
				return sectionNumber;
			}

			return -1;
		}

		public IEnumerable<ISection> GetAllSection()
		{
			var sections = ChapterToSectionMap.Values.Distinct();
			return sections;
		}

		void Initialize()
		{
			var url = ApplicationConfig.SectionDetailsUrl;
			url = url.Replace("{publishDate}", ApplicationConfig.PublishDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture));

			using (var webDriverHelper = new WebDriverHelper())
			{
				var sectionDetailsScrapper = new SectionDetailsScrapper(webDriverHelper);
				_chapterToSectionMap = sectionDetailsScrapper.ExtractChapterToSectionMap(url);
			}
		}
	}
}
