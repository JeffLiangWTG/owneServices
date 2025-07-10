using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class DataObjectCollectionReader<T1, T2>
		where T1 : IDataObject
		where T2 : BusinessObject
	{
		protected DataObjectCollectionReader(T1[] dataObjects)
		{
			DataObjects = dataObjects ?? System.Array.Empty<T1>();
			ContentType = DefaultCollectionContent;
		}

		protected DataObjectCollectionReader(DataObjectList<T1> dataObjects)
		{
			if (dataObjects != null)
			{
				DataObjects = dataObjects.ToArray();
				ContentType = dataObjects.Content ?? DefaultCollectionContent;
			}
			else
			{
				DataObjects = System.Array.Empty<T1>();
				ContentType = DefaultCollectionContent;
			}
		}

		protected virtual CollectionContent DefaultCollectionContent
		{
			get { return CollectionContent.Complete; }
		}

		protected CollectionContent ContentType { get; private set; }
		protected T1[] DataObjects { get; private set; }
		protected abstract T2[] BusinessObjects { get; }

		protected abstract void AddToCollection(T2 businessObject);
		protected abstract void RemoveFromCollection(T2 businessObject);
		protected abstract T2 FindMatchingBusinessObject(T1 dataObject);
		protected abstract T2 ReadIntoBusinessObject(T1 dataObject, T2 businessObject);

		#region Implementation

		public void ReadIntoCollection()
		{
			ReadIntoCollection(removeUnmatchedElements: true);
		}

		public void ReadIntoCollectionRetainingUnmatchedElements()
		{
			ReadIntoCollection(removeUnmatchedElements: false);
		}

		public void ReadIntoCollectionUnmatchedElementsOnly()
		{
			foreach (var dataObject in DataObjects)
			{
				if (!SkipEntity(dataObject))
				{
					var matchingBizObj = FindMatchingBusinessObject(dataObject);
					if (matchingBizObj == null)
					{
						var processedBizObject = ReadIntoBusinessObject(dataObject, matchingBizObj);

						if (processedBizObject != null)
						{
							AddToCollection(processedBizObject);
						}
					}
				}
			}
		}

		protected void ReadIntoCollection(bool removeUnmatchedElements)
		{
			var processedBizObjects = new HashSet<T2>();
			var skippedEntities = new List<T1>();

			foreach (var dataObject in DataObjects)
			{
				if (!SkipEntity(dataObject))
				{
					var matchingBizObj = FindMatchingBusinessObject(dataObject);
					var processedBizObject = ReadIntoBusinessObject(dataObject, matchingBizObj);

					if (processedBizObject != null)
					{
						processedBizObjects.Add(processedBizObject);
						if (matchingBizObj == null)
						{
							AddToCollection(processedBizObject);
						}
					}
				}
				else
				{
					skippedEntities.Add(dataObject);
				}
			}

			var unmatchedBusinessObjects = BusinessObjects.Except(processedBizObjects).ToArray();

			if (removeUnmatchedElements && ContentType == CollectionContent.Complete)
			{
				foreach (var bizObj in unmatchedBusinessObjects)
				{
					RemoveFromCollection(bizObj);
				}
			}
			else if (ContentType == CollectionContent.Partial)
			{
				ProcessSkippedEntities(skippedEntities, unmatchedBusinessObjects);
			}

			ReadIntoCollectionCore();
		}

		protected virtual void ReadIntoCollectionCore()
		{
		}

		protected virtual bool SkipEntity(T1 dataObject)
		{
			return false;
		}

		protected virtual void ProcessSkippedEntities(IEnumerable<T1> skippedEntities, IEnumerable<T2> unmatchedBusinessObjects)
		{
		}

		#endregion
	}
}
