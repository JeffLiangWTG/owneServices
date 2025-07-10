using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CollectionReaderHelper<TBusinessObject>
			where TBusinessObject : BusinessObject
	{
		public void MarkUnprocessedExistingObjectFor(UniversalObjectFactory factory, ZQuery query)
		{
			foreach (var businessObject in factory.Load<TBusinessObject>(query))
			{
				if (!ExistingObjectProcessingDictionary.ContainsKey(businessObject))
				{
					ExistingObjectProcessingDictionary.Add(businessObject, false);
				}
			}
		}

		public void MarkProcessed(TBusinessObject businessObject)
		{
			if (businessObject != null && ExistingObjectProcessingDictionary.ContainsKey(businessObject))
			{
				ExistingObjectProcessingDictionary[businessObject] = true;
			}
		}

		public void DeleteUnprocessedObjectsFor(BusinessObject parentBO, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingObjectProcessingDictionary.ToArray())
			{
				if (!pair.Value && GetParentBOPK(pair.Key) == parentBO.PK)
				{
					var businessObject = pair.Key;
					businessObject.FetchStrategy.FetchForDelete();
				}
			}

			foreach (var pair in ExistingObjectProcessingDictionary.ToArray())
			{
				if (!pair.Value && GetParentBOPK(pair.Key) == parentBO.PK)
				{
					var businessObject = pair.Key;
					if (businessObject.CanDelete)
					{
						ExistingObjectProcessingDictionary.Remove(businessObject);
						logger.Log(LogType.Information, Res.GetString("27E7EFEE-88DA-4F6B-8101-36C3C54596FB", "Deleted {0} from {1}.", businessObject.HumanReadableName, "UniversalShipment"));
						businessObject.Delete();
					}
					else
					{
						logger.Log(LogType.Warning, Res.GetString("CBB540DA-8364-4C4A-9F5E-365FCE8E7509", "Could not delete {0} due to the following reason: {1}", businessObject.HumanReadableName, businessObject.ReasonForNotAbleToDelete));
					}
				}
			}
		}

		protected virtual ZGuid GetParentBOPK(TBusinessObject businessObject)
		{
			return ZGuid.Empty;
		}

		protected Dictionary<TBusinessObject, bool> ExistingObjectProcessingDictionary
		{
			get { return existingObjectProcessingDictionary ?? (existingObjectProcessingDictionary = new Dictionary<TBusinessObject, bool>()); }
		}
		Dictionary<TBusinessObject, bool> existingObjectProcessingDictionary;
	}
}
