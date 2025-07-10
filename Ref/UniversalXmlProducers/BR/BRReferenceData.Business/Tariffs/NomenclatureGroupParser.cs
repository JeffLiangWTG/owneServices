using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class NomenclatureGroupParser : BaseParser
	{
		public NomenclatureGroupParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refCusNomenclatureGroupList = GetRefCusNomenclatureGroup(inputFileStream);
			var writer = Helper.GetRefCusNomenclatureGroupWriterConfiguration();
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writer, refCusNomenclatureGroupList);
		}

		static List<RefCusNomenclatureGroup> GetRefCusNomenclatureGroup(Stream stream)
		{
			Argument.NotNull(stream, nameof(stream));

			var result = new List<RefCusNomenclatureGroup>();

			var xml = XDocument.Load(stream);
			Contract.Assume(xml != null);
			var nomenclatureGroups = xml.Root?.Descendants(NomenclatureGroupConstants.TagNCM);

			foreach (XElement element in nomenclatureGroups)
			{
				var refCusNomenclatureGroup = new RefCusNomenclatureGroup()
				{
					ZZ5_Description = DescriptionCleaner(element.GetElementValueAsString(NomenclatureGroupConstants.TagDescription, 0)),
					ZZ5_Value = element.GetElementValueAsString(NomenclatureGroupConstants.TagCode, 15)
				};

				if (refCusNomenclatureGroup.ZZ5_Value?.Length >= 8)
				{
					continue;
				}

				RefCusNomenclatureGroupUtils.BuildZZ1_CompositeKeyOnZZ5(refCusNomenclatureGroup);

				result.Add(refCusNomenclatureGroup);
			}
			return result;
		}

		static string DescriptionCleaner(string description)
		{
			return description?.Trim(new char[] { '-', ' ' });
		}
	}
}
