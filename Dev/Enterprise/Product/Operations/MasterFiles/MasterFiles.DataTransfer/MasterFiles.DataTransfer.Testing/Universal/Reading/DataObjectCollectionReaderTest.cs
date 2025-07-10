using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	[TestsSubclassesOf(typeof(DataObjectCollectionReader<,>), ExcludePrivate = true)]
	public abstract class DataObjectCollectionReaderTest : TestCaseWithUniversalObjectFactory
	{
		public abstract void TestReadIntoCollection();
	}

	[TestedType(typeof(DataObjectCollectionReaderForTest))]
	public class DataObjectCollectionReaderFunctionalityTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			BusinessObjects.AddRange(new[] { dummy1, dummy2, dummy3 });

			var dummy4 = Factory.New<DummyBusinessObject>();
			var dummy1DataObject = CreateDataObjectFromDummy(dummy1);
			var dummy2DataObject = CreateDataObjectFromDummy(dummy2);
			var dummy4DataObject = CreateDataObjectFromDummy(dummy4);
			DataObjects.AddRange(new[] { dummy1DataObject, dummy2DataObject, dummy4DataObject });

			var collectionReader = (DataObjectCollectionReaderForTest)GetNewDataObjectCollectionReader();
			collectionReader.ReadIntoBusinessObjectImpl = (shipment, dummy) =>
			{
				if (shipment == dummy4DataObject)
				{
					AssertNull("should not find dummy4 in BusinessObjects", dummy);
					return dummy4;
				}
				return dummy;
			};

			collectionReader.ReadIntoCollection();
			AssertContainsExactElementsInAnyOrder(new[] { dummy1, dummy2, dummy4 }, BusinessObjects);
		}

		public void TestReadIntoCollectionUnmatchedElementsOnly()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var dummy2 = Factory.New<DummyBusinessObject>();
			var dummy3 = Factory.New<DummyBusinessObject>();
			BusinessObjects.AddRange(new[] { dummy1, dummy2, dummy3 });

			var dummy4 = Factory.New<DummyBusinessObject>();
			var dummy1DataObject = CreateDataObjectFromDummy(dummy1);
			var dummy2DataObject = CreateDataObjectFromDummy(dummy2);
			var dummy4DataObject = CreateDataObjectFromDummy(dummy4);
			DataObjects.AddRange(new[] { dummy1DataObject, dummy2DataObject, dummy4DataObject });

			var collectionReader = (DataObjectCollectionReaderForTest)GetNewDataObjectCollectionReader();
			collectionReader.ReadIntoBusinessObjectImpl = (shipment, dummy) =>
			{
				if (shipment == dummy4DataObject)
				{
					AssertNull("should not find dummy4 in BusinessObjects", dummy);
					return dummy4;
				}
				return dummy;
			};

			collectionReader.ReadIntoCollectionUnmatchedElementsOnly();
			AssertContainsExactElementsInAnyOrder(new[] { dummy1, dummy2, dummy3, dummy4 }, BusinessObjects);
		}

		protected DataObjectCollectionReader<UniversalShipment, DummyBusinessObject> GetNewDataObjectCollectionReader()
		{
			var collectionReader = new DataObjectCollectionReaderForTest(DataObjects.ToArray());
			collectionReader.AddToCollectionImpl = dummy => BusinessObjects.Add(dummy);
			collectionReader.RemoveFromCollectionImpl = dummy => BusinessObjects.Remove(dummy);
			collectionReader.BusinessObjectsImpl = () => BusinessObjects.ToArray();
			collectionReader.FindMatchingBusinessObjectImpl = FindMatchingDummyBusinessObject;
			return collectionReader;
		}

		UniversalShipment CreateDataObjectFromDummy(DummyBusinessObject dummy)
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataTarget(DataContextType.DummyBusinessObject, dummy.PK.ToString());
			return dataObject;
		}

		Func<UniversalShipment, DummyBusinessObject> FindMatchingDummyBusinessObject
		{
			get
			{
				return (dataObject) =>
				{
					ZGuid dummyPkFromShipment = GetDummyPkFromShipment(dataObject);
					return dummyPkFromShipment.IsValid ?
						BusinessObjects.FirstOrDefault(bizObj => dummyPkFromShipment == bizObj.PK) : null;
				};
			}
		}

		ZGuid GetDummyPkFromShipment(UniversalShipment dataObject)
		{
			ZGuid result = ZGuid.Empty;

			if (dataObject.DataContext != null && dataObject.DataContext.DataTargetCollection != null)
			{
				var dataTarget = dataObject.DataContext.DataTargetCollection.FirstOrDefault();
				if (dataTarget != null)
				{
					result = new ZGuid(dataTarget.Key);
				}
			}

			return result;
		}

		List<UniversalShipment> DataObjects
		{
			get { return shipments ?? (shipments = new List<UniversalShipment>()); }
		}
		List<UniversalShipment> shipments;

		List<DummyBusinessObject> BusinessObjects
		{
			get { return businessObjects ?? (businessObjects = new List<DummyBusinessObject>()); }
		}
		List<DummyBusinessObject> businessObjects;
	}

	class DataObjectCollectionReaderForTest : DataObjectCollectionReader<UniversalShipment, DummyBusinessObject>
	{
		public DataObjectCollectionReaderForTest(UniversalShipment[] dataObjects)
			: base(dataObjects)
		{
		}

		public Func<DummyBusinessObject[]> BusinessObjectsImpl { get; set; }
		protected override DummyBusinessObject[] BusinessObjects
		{
			get { return BusinessObjectsImpl != null ? BusinessObjectsImpl() : null; }
		}

		public Action<DummyBusinessObject> AddToCollectionImpl { get; set; }
		protected override void AddToCollection(DummyBusinessObject businessObject)
		{
			if (AddToCollectionImpl != null)
			{
				AddToCollectionImpl(businessObject);
			}
		}

		public Action<DummyBusinessObject> RemoveFromCollectionImpl { get; set; }
		protected override void RemoveFromCollection(DummyBusinessObject businessObject)
		{
			if (RemoveFromCollectionImpl != null)
			{
				RemoveFromCollectionImpl(businessObject);
			}
		}

		public Func<UniversalShipment, DummyBusinessObject> FindMatchingBusinessObjectImpl { get; set; }
		protected override DummyBusinessObject FindMatchingBusinessObject(UniversalShipment dataObject)
		{
			return FindMatchingBusinessObjectImpl != null ? FindMatchingBusinessObjectImpl(dataObject) : null;
		}

		public Func<UniversalShipment, DummyBusinessObject, DummyBusinessObject> ReadIntoBusinessObjectImpl { get; set; }
		protected override DummyBusinessObject ReadIntoBusinessObject(UniversalShipment dataObject, DummyBusinessObject businessObject)
		{
			return ReadIntoBusinessObjectImpl != null ? ReadIntoBusinessObjectImpl(dataObject, businessObject) : null;
		}
	}
}
