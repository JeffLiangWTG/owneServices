using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class SectionHelper
	{
		public int GetSectionNumber(int chapter)
		{
			var result = 0;

			if (chapter > 0)
			{
				result = Sections.FirstOrDefault(x => x.MinimumChapter <= chapter && chapter <= x.MaximumChapter)?.SectionNumber ?? 0;
			}

			return result;
		}

		public List<Section> GetSectionsByChapter(string chapterFilter)
		{
			if (chapterFilter.Length > 1)
			{
				if (int.TryParse(chapterFilter, out var chapter))
				{
					return Sections.Where(x => x.MinimumChapter <= chapter && chapter <= x.MaximumChapter).ToList();
				}
			}

			return Sections.Where(x => x.MinimumChapter.ToString("00", CultureInfo.InvariantCulture).StartsWith(chapterFilter, StringComparison.Ordinal) ||
										x.MaximumChapter.ToString("00", CultureInfo.InvariantCulture).StartsWith(chapterFilter, StringComparison.Ordinal)).ToList();
		}

		internal List<Section> Sections => sections ?? (sections = GetSections());
		List<Section> sections;
		protected virtual List<Section> GetSections() => new List<Section>
		{
			// Data from https://www.trade-tariff.service.gov.uk/sections and EUN Tariffs as the complete data set is not available in the XML files as yet
			new Section { SectionNumber = 1, MinimumChapter = 1, MaximumChapter = 5, Description = "Live animals; animal products" },
			new Section { SectionNumber = 2, MinimumChapter = 6, MaximumChapter = 14, Description = "Vegetable products" },
			new Section { SectionNumber = 3, MinimumChapter = 15, MaximumChapter = 15, Description = "Animal or vegetable fats and oils and their cleavage products; prepared edible fats; animal or vegetable waxes" },
			new Section { SectionNumber = 4, MinimumChapter = 16, MaximumChapter = 24, Description = "Prepared foodstuffs; beverages, spirits and vinegar; tobacco and manufactured tobacco substitutes" },
			new Section { SectionNumber = 5, MinimumChapter = 25, MaximumChapter = 27, Description = "Mineral products" },
			new Section { SectionNumber = 6, MinimumChapter = 28, MaximumChapter = 38, Description = "Products of the chemical or allied industries" },
			new Section { SectionNumber = 7, MinimumChapter = 39, MaximumChapter = 40, Description = "Plastics and articles thereof; rubber and articles thereof" },
			new Section { SectionNumber = 8, MinimumChapter = 41, MaximumChapter = 43, Description = "Raw hides and skins, leather, furskins and articles thereof; saddlery and harness; travel goods, handbags and similar containers; articles of animal gut (other than silkworm gut)" },
			new Section { SectionNumber = 9, MinimumChapter = 44, MaximumChapter = 46, Description = "Wood and articles of wood; wood charcoal; cork and articles of cork; manufactures of straw, of esparto or of other plaiting materials; basket-ware and wickerwork" },
			new Section { SectionNumber = 10, MinimumChapter = 47, MaximumChapter = 49, Description = "Pulp of wood or of other fibrous cellulosic material; recovered (waste and scrap) paper or paperboard; paper and paperboard and articles thereof" },
			new Section { SectionNumber = 11, MinimumChapter = 50, MaximumChapter = 63, Description = "Textiles and textile articles" },
			new Section { SectionNumber = 12, MinimumChapter = 64, MaximumChapter = 67, Description = "Footwear, headgear, umbrellas, sun umbrellas, walking-sticks, seat-sticks, whips, riding-crops and parts thereof; prepared feathers and articles made therewith; artificial flowers; articles of human hair" },
			new Section { SectionNumber = 13, MinimumChapter = 68, MaximumChapter = 70, Description = "Articles of stone, plaster, cement, asbestos, mica or similar materials; ceramic products; glass and glassware" },
			new Section { SectionNumber = 14, MinimumChapter = 71, MaximumChapter = 71, Description = "Natural or cultured pearls, precious or semi-precious stones, precious metals, metals clad with precious metal and articles thereof; imitation jewellery; coins" },
			new Section { SectionNumber = 15, MinimumChapter = 72, MaximumChapter = 83, Description = "Base metals and articles of base metal" },
			new Section { SectionNumber = 16, MinimumChapter = 84, MaximumChapter = 85, Description = "Machinery and mechanical appliances; electrical equipment; parts thereof, sound recorders and reproducers, television image and sound recorders and reproducers, and parts and accessories of such articles" },
			new Section { SectionNumber = 17, MinimumChapter = 86, MaximumChapter = 89, Description = "Vehicles, aircraft, vessels and associated transport equipment" },
			new Section { SectionNumber = 18, MinimumChapter = 90, MaximumChapter = 92, Description = "Optical, photographic, cinematographic, measuring, checking, precision, medical or surgical instruments and apparatus; clocks and watches; musical instruments; parts and accessories thereof" },
			new Section { SectionNumber = 19, MinimumChapter = 93, MaximumChapter = 93, Description = "Arms and ammunition; parts and accessories thereof" },
			new Section { SectionNumber = 20, MinimumChapter = 94, MaximumChapter = 96, Description = "Miscellaneous manufactured articles" },
			new Section { SectionNumber = 21, MinimumChapter = 97, MaximumChapter = 98, Description = "Works of art, collectors' pieces and antiques" },
			new Section { SectionNumber = 22, MinimumChapter = 99, MaximumChapter = 99, Description = "SPECIAL CLASSIFICATION PROVISIONS" }
		};
	}
}
