using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.MasterFiles.Module.Organisation.OrgImport
{
	public abstract class MergeFlattenedDataTransferProcessor<Header, Flattened> : DataTransferProcessor
		where Header : BusinessObject
		where Flattened : BusinessObject
	{
		protected MergeFlattenedDataTransferProcessor(IBusinessObjectCollection headerCollection, IImportCollectionInfo flattenedImportCollectionInfo)
		{
			this.headerCollection = headerCollection;
			this.flattenedImportCollectionInfo = flattenedImportCollectionInfo;
			this.flattenedCollection = flattenedImportCollectionInfo.Collection;

			if (headerCollection is IActiveBusinessObjectCollection activeHeaderCollection
				&& !typeof(AdhocCollectionRelationship).IsAssignableFrom(activeHeaderCollection.Relationship.GetType()))
			{
				throw new ArgumentException("Attempt to use non-AdHoc ActiveCollection for headerCollection"); // See WI00220544
			}
		}

		protected readonly IBusinessObjectCollection headerCollection;
		protected readonly IImportCollectionInfo flattenedImportCollectionInfo;
		protected readonly IBusinessObjectCollection flattenedCollection;

		#region Header

		protected abstract IEnumerable<string> HeaderColumnsOnFlattened { get; }

		bool HasSameHeaderValues(ReadonlyImportValuesDictionary x, ReadonlyImportValuesDictionary y)
		{
			return HeaderColumnsOnFlattened.All(column => x[column].Equals(y[column]));
		}

		#endregion

		#region Unique Children Mergers

		public readonly IList<FlattenedToUniqueChildrenMerger<Header, Flattened>> UniqueChildrenMergers = new List<FlattenedToUniqueChildrenMerger<Header, Flattened>>();
		protected void AddUniqueChildrenMerger(FlattenedToUniqueChildrenMerger<Header, Flattened> uniqueChildrenMerger)
		{
			UniqueChildrenMergers.Add(uniqueChildrenMerger);
			uniqueChildrenMerger.Logged += UniqueChildrenMerger_Logged;
		}

		void UniqueChildrenMerger_Logged(string message)
		{
			Log += message + "\r\n";
		}

		#endregion

		#region Import

		protected abstract string GetProgressChangedStatus(int recordsProcessed);
		protected abstract Header CreateHeader(IBusinessObjectCollection headerCollection, Flattened flattenedRecord);

		public override void Import()
		{
			var collectionCount = flattenedCollection.Count;
			if (collectionCount > 0)
			{
				ListChangedEventHandler listChangedHandler = (sender, e) => ReportFlattenedCollectionChanged(collectionCount, sender, e);
				flattenedCollection.ListChanged += listChangedHandler;
				ImportCore();
				flattenedCollection.ListChanged -= listChangedHandler;
			}
		}

		protected virtual void ImportCore()
		{
			ReadonlyImportValuesDictionary previousFlattenedImportValues = null;
			Header previousNewHeader = null;

			var recordsProcessed = 0;
			foreach (Flattened flattenedRecord in flattenedCollection)
			{
				if (IsCanceled)
				{
					break;
				}

				if (!OnProgressChanged((recordsProcessed + 1) * 100 / flattenedCollection.Count, GetProgressChangedStatus(recordsProcessed + 1)))
				{
					break;
				}

				Header newHeader = null;
				ReadonlyImportValuesDictionary originalFlattenedImportValues = new ReadonlyImportValuesDictionary(flattenedRecord, flattenedImportCollectionInfo);
				if (previousFlattenedImportValues != null && HasSameHeaderValues(previousFlattenedImportValues, originalFlattenedImportValues))
				{
					newHeader = previousNewHeader;
				}
				else
				{
					HeadersToCreate++;
					newHeader = CreateHeader(headerCollection, flattenedRecord);
					if (newHeader == null)
					{
						HeadersExcluded++;
					}

					foreach (var uniqueChildrenMerger in UniqueChildrenMergers)
					{
						uniqueChildrenMerger.NotifiyParentChanged();
					}
				}

				foreach (var uniqueChildrenMerger in UniqueChildrenMergers)
				{
					uniqueChildrenMerger.CreateChildRecordIfUnique(newHeader, flattenedRecord, originalFlattenedImportValues);
				}

				previousNewHeader = newHeader;
				previousFlattenedImportValues = originalFlattenedImportValues;
				recordsProcessed++;
			}

			HeadersCreated += headerCollection.Count;
		}

		void ReportFlattenedCollectionChanged(int originalCollectionCount, object sender, ListChangedEventArgs e)
		{
			var currentCollectionCount = flattenedCollection.Count;
			if (originalCollectionCount != currentCollectionCount)
			{
				ErrorReporter.ReportOnce("Collection changed during the import.", string.Format("Collection should not be changed. original count = {0}, current count = {1}", originalCollectionCount, currentCollectionCount));
			}
		}

		public override void Rollback()
		{
			foreach (var item in headerCollection.ToArray())
			{
				headerCollection.Delete(item);
			}
		}

		#endregion

		#region Logging

		public string Log { get; set; }

		public int HeadersToCreate { get; set; }
		public int HeadersCreated { get; set; }
		public int HeadersExcluded { get; set; }

		#endregion
	}

	public abstract class FlattenedToUniqueChildrenMerger<Header, Flattened>
		where Header : BusinessObject
		where Flattened : BusinessObject
	{
		protected FlattenedToUniqueChildrenMerger(IEnumerable<string> childColumnsOnFlattened)
		{
			this.childColumnsOnFlattened = childColumnsOnFlattened;
		}

		readonly IEnumerable<string> childColumnsOnFlattened;

		#region Create Child Record

		public void CreateChildRecordIfUnique(Header parent, Flattened flattenedRecord, ReadonlyImportValuesDictionary flattenedImportValues)
		{
			if (ContainsChildRecord(flattenedImportValues))
			{
				ChildRecordsFound++;
				if (!ContainsDuplicateChildRecord(flattenedImportValues))
				{
					if (parent != null && CreateChildRecord(parent, flattenedRecord))
					{
						ChildRecordsCreated++;
						ChildRecordsCreatedForCurrentParent++;
					}
					else
					{
						ChildRecordsExcluded++;
					}

					processedChildImportValuesForCurrentParent.Add(flattenedImportValues);
				}
				else
				{
					DuplicateChildRecordsFound++;
				}
			}
		}

		protected abstract bool CreateChildRecord(Header parent, Flattened record);

		protected virtual bool ContainsChildRecord(ReadonlyImportValuesDictionary importValues)
		{
			return childColumnsOnFlattened.Any(column =>
				{
					var value = importValues[column];
					if (value != null)
					{
						var valueAsZType = value as IZType;
						if (valueAsZType != null)
						{
							return !valueAsZType.IsDefault;
						}
						else
						{
							return !string.IsNullOrEmpty(value.ToString());
						}
					}
					return false;
				});
		}

		bool ContainsDuplicateChildRecord(ReadonlyImportValuesDictionary importValues)
		{
			foreach (var processedChildRecord in processedChildImportValuesForCurrentParent)
			{
				if (childColumnsOnFlattened.All(column => processedChildRecord[column].Equals(importValues[column])))
				{
					return true;
				}
			}

			return false;
		}

		public void NotifiyParentChanged()
		{
			ChildRecordsCreatedForCurrentParent = 0;
			processedChildImportValuesForCurrentParent.Clear();
		}

		readonly IList<ReadonlyImportValuesDictionary> processedChildImportValuesForCurrentParent = new List<ReadonlyImportValuesDictionary>();

		#endregion

		#region ChildHumanReadableNameForPlural

		public string ChildHumanReadableNameForPlural
		{
			get { return ChildHumanReadableNameForPluralCore; }
		}

		protected abstract string ChildHumanReadableNameForPluralCore { get; }

		#endregion

		#region Logging

		public delegate void LogHandler(string message);
		public event LogHandler Logged;
		public void Log(string message)
		{
			if (Logged != null)
			{
				Logged(message);
			}
		}

		public int ChildRecordsFound { get; set; }
		public int DuplicateChildRecordsFound { get; set; }
		public int ChildRecordsCreated { get; set; }
		public int ChildRecordsExcluded { get; set; }

		protected int ChildRecordsCreatedForCurrentParent { get; set; }

		#endregion
	}

	public class ReadonlyImportValuesDictionary
	{
		public ReadonlyImportValuesDictionary(BusinessObject businessObject, IImportCollectionInfo importCollectionInfo)
		{
			innerDictionary = new Dictionary<string, object>();
			foreach (var importProperty in importCollectionInfo.Properties)
			{
				var propertyName = importProperty.MappingName;
				innerDictionary.Add(propertyName, businessObject[propertyName]);
			}
		}

		readonly IDictionary<string, object> innerDictionary;

		public object this[string propertyName]
		{
			get { return innerDictionary[propertyName]; }
		}
	}
}
