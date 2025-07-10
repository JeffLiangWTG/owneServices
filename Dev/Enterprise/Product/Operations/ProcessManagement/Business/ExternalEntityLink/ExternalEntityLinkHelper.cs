using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	public static class ExternalEntityLinkHelper
	{
		public static IEnumerable<T> FilterOutPreviouslyLinkedEntities<T>(IEnumerable<T> entities, string systemCode, BusinessObjectFactory factory)
			where T : IExternalEntityLinkable
		{
			var results = new List<T>();
			var entitiesByCode = entities.GroupBy(x => x.ParentTableCode);

			foreach (var group in entitiesByCode)
			{
				var query = new ZQuery(ExternalEntityLinkSchema.EEL_ParentTableCode, group.Key);
				query.AddToFilter(ExternalEntityLinkSchema.EEL_SystemCode, systemCode);
				query.AddToFilter(ExternalEntityLinkSchema.EEL_ExternalCode, group.Select(x => x.ID));

				var links = factory.Load<ExternalEntityLink>(query);
				var alreadyImportedIDs = links.Select(x => x.EEL_ExternalCode.ToString());
				var entitiesToAdd = group.Where(x => !alreadyImportedIDs.Contains(x.ID));
				results.AddRange(entitiesToAdd);
			}

			return results;
		}

		public static ExternalEntityLink CreateLink(IExternalEntityLinkable externalEntity, BusinessObject convertedObject, string systemCode)
		{
			var link = convertedObject.Factory.New<ExternalEntityLink>();
			link.EEL_ParentID = convertedObject.PK;
			link.EEL_ParentTableCode = externalEntity.ParentTableCode;
			link.EEL_SystemCode = systemCode;
			link.EEL_ExternalCode = externalEntity.ID;

			return link;
		}

		public static IEnumerable<ExternalEntityLink> GetLinksForBusinessObject(BusinessObject businessObject)
		{
			var query = new ZQuery(ExternalEntityLinkSchema.EEL_ParentID, businessObject.PK);
			query.AddToFilter(ExternalEntityLinkSchema.EEL_ParentTableCode, businessObject.TablePrefix);

			return businessObject.Factory.Load<ExternalEntityLink>(query);
		}

		public static void DeleteLinksForBusinessObject(BusinessObject businessObject)
		{
			var links = GetLinksForBusinessObject(businessObject);
			links.ForEach(x => x.Delete());
		}
	}
}
