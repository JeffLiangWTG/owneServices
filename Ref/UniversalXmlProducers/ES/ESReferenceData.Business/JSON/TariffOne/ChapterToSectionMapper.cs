using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ESReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ChapterToSectionMapper : IChapterToSectionMapper
	{
		public ChapterToSectionMapper(string sectionsFileContent, StringBuilder errors)
		{
			this.sectionsFileContent = Argument.NotNull(sectionsFileContent, nameof(sectionsFileContent));
			this.errors = Argument.NotNull(errors, nameof(errors));
		}
		readonly string sectionsFileContent;
		readonly StringBuilder errors;

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
			var sectionsList = JsonHelper.GetListItemsFromJsonl<SectionsSchema>(sectionsFileContent, errors);
			_chapterToSectionMap = new Dictionary<int, ISection>();
			foreach (var s in sectionsList)
			{
				var id = s.section_id;
				var description = s.descriptions.FirstOrDefault(x => x.lang == "en")?.text ?? s.descriptions.FirstOrDefault(x => x.lang == "es")?.text;
				if (description == null)
				{
					errors.AppendLine("No description found for this section: " + id);
					continue;
				}
				var section = new Section(id, description);

				foreach (var chapter in s.chapters)
				{
					_chapterToSectionMap.Add(chapter, section);
				}
			}
		}
	}
}
