using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.Business
{
	public static class RelatedPortXmlWriter
	{
		const string RefUNLOCORelatedPortDataSource = "UNLOCO Related Ports";

		public static void Write(
			List<PortMapping> portMappings,
			string outputFilePath,
			DateTime publicationTime
		)
		{
			var writer = new XmlWriter(RefUNLOCORelatedPortXmlConfiguration.GetConfiguration());
			writer.SetDataSource(RefUNLOCORelatedPortDataSource);
			writer.SetUpdateType(UpdateType.Full);
			writer.SetPublicationTime(publicationTime);

			for (var i = 0; i < portMappings.Count; i++)
			{
				foreach (var relatedUnloco in portMappings[i].relatedUnlocos)
				{
					writer.PopulateData(new RefUNLOCORelatedPort()
					{
						RLR_GroupNumber = (short)(i + 1),
						RLR_RL_NKRelatedPort = relatedUnloco
					});
				}
			}

			writer.SaveXml(outputFilePath);
		}
	}
}
