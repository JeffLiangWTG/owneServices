using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using Polly;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class CompositeKeyTreeGenerator : ICompositeKeyTreeGenerator
	{
		readonly IChapterToSectionMapper chapterToSectionMapper;
		readonly ICompositeKeyNode rootNode;
		readonly ICollection<ICompositeKeyNode> sectionNodes = new List<ICompositeKeyNode>();
		const int SectionNodeLevel = 0;

		public CompositeKeyTreeGenerator(IChapterToSectionMapper chapterToSectionMapper)
		{
			Argument.NotNull(chapterToSectionMapper, nameof(chapterToSectionMapper));

			this.chapterToSectionMapper = chapterToSectionMapper;
			this.rootNode = new CompositeKeyNode("root", "root", DateTime.Today, new DateTime(2079, 06, 06, 23, 59, 00),
				CompositeKeyNodeType.PlaceHolder, -1, null);
		}

		public ICompositeKeyNode GenerateTree(IEnumerable<INomenclatureRecord> nomenclatureRecords)
		{
			Argument.NotNull(nomenclatureRecords, nameof(nomenclatureRecords));
			var orderedList = nomenclatureRecords.OrderBy(n => n.TariffHeader);
			SetSectionNodes();

			ICompositeKeyNode currentChapterNode = null;
			ICompositeKeyNode currentSubchapterNode = null;
			ICompositeKeyNode currentHeading = null;
			ICompositeKeyNode previousNode = null;
			INomenclatureRecord previousRawRecord = null;

			foreach (var nomenclatureRecord in orderedList)
			{
				if (nomenclatureRecord.HierarchyPosition == 2)
				{
					var tariffHeader = nomenclatureRecord.TariffHeader;

					var chapterNumber = int.Parse(tariffHeader.Substring(0, 2), CultureInfo.InvariantCulture);
					var sectionNode = GetSectionNode(chapterNumber);
					currentChapterNode = EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, sectionNode);
					currentSubchapterNode = null;
					currentHeading = null;
					previousNode = currentChapterNode;
					previousRawRecord = nomenclatureRecord;
					continue;
				}

				if (nomenclatureRecord.HierarchyPosition == 4)
				{
					if (EUNUtils.BeginsWithRomanNumeral(nomenclatureRecord.Description))
					{
						var splitData = nomenclatureRecord.Description.Split('.');

						var romanNumeral = splitData.First();
						var tariffHeader = EUNUtils.ConvertFromRomanNumeral(romanNumeral);
						var subChapterValue = tariffHeader.ToString(CultureInfo.InvariantCulture);
						if (tariffHeader < 10)
						{
							subChapterValue = $"0{subChapterValue}";
						}

						currentSubchapterNode = new CompositeKeyNode(subChapterValue, nomenclatureRecord.Description, nomenclatureRecord.DeclarableStartDate, nomenclatureRecord.EndDate, CompositeKeyNodeType.NomenclatureGroup, currentChapterNode.Level + 1, currentChapterNode, nomenclatureRecord.Language.ToList());

						previousNode = currentSubchapterNode;
						currentHeading = null;
						previousRawRecord = nomenclatureRecord;
						continue;
					}

					if (currentSubchapterNode == null)
					{
						currentSubchapterNode = EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, currentChapterNode, "00");
					}

					currentHeading = EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, currentSubchapterNode);
					previousNode = currentHeading;
					previousRawRecord = nomenclatureRecord;

					if (nomenclatureRecord.IsTariff)
					{
						EUNUtils.CreateAndAssignNomenclatureGroupNodeFromTariff(nomenclatureRecord, currentSubchapterNode);
					}
					continue;
				}

				if (nomenclatureRecord.HierarchyPosition == 6)
				{
					previousNode = EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, currentHeading);
					previousRawRecord = nomenclatureRecord;
					continue;
				}

				ICompositeKeyNode parentNode = null;
				if (nomenclatureRecord.Level > previousRawRecord.Level)
				{
					parentNode = previousNode;
				}
				else
				{
					var differenceInLevel = previousRawRecord.Level - nomenclatureRecord.Level;
					parentNode = previousNode.Parent;
					for (var counter = 0; counter < differenceInLevel; counter++)
					{
						parentNode = parentNode.Parent;
					}
				}

				if (parentNode is null)
				{
					var message = $"WARNING: no parent node detected for nomenclature record. TariffHeader:'{nomenclatureRecord.TariffHeader}', Description:'{nomenclatureRecord.Description}', HierarchyPosition:{nomenclatureRecord.HierarchyPosition}, Level:{nomenclatureRecord.Level}";
					Console.Error.WriteLine(message);
					continue;
				}

				if (nomenclatureRecord.HierarchyPosition < 10 && nomenclatureRecord.IsTariff)
				{
					previousNode = EUNUtils.CreateAndAssignNomenclatureGroupNodeFromTariff(nomenclatureRecord, parentNode);
					EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, previousNode);
				}
				else
				{
					previousNode = EUNUtils.CreateAndAssignCompositeKeyTreeNode(nomenclatureRecord, parentNode);
				}

				previousRawRecord = nomenclatureRecord;
			}

			return rootNode;
		}

		void SetSectionNodes()
		{
			var policy = Policy.HandleResult<IEnumerable<ISection>>(r => !r.Any()).Retry(2);
			var sections = policy.Execute(chapterToSectionMapper.GetAllSection);
			if (sections == null || !sections.Any())
			{
				throw new InvalidOperationException("Can not load the section details.");
			}

			foreach (var section in sections)
			{
				var sectionNumber = section.Number.ToString("D2", CultureInfo.InvariantCulture);
				var sectionNode = new CompositeKeyNode(sectionNumber, section.Description, EUNUtils.MinDateTime, EUNUtils.MaxDateTime, CompositeKeyNodeType.NomenclatureGroup, SectionNodeLevel, rootNode);
				sectionNodes.Add(sectionNode);
			}
		}

		ICompositeKeyNode GetSectionNode(int chapterNumber)
		{
			var sectionNumber = chapterToSectionMapper.GetSection(chapterNumber);
			var sectionValue = sectionNumber.ToString("D2", CultureInfo.InvariantCulture);

			var section = sectionNodes.First(x => x.Value == sectionValue);

			return section;
		}
	}
}
