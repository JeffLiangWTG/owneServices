using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Packing.DataTransfer.Universal.Testing
{
	class DummyHandle : ObjectHandle
	{
		public DummyHandle()
			: this(() => null)
		{
		}

		public DummyHandle(Func<UniversalShipment> getShipmentToReturnFromPopulateDataObject)
			: this(p => getShipmentToReturnFromPopulateDataObject(), null)
		{
		}

		public DummyHandle(Func<PkgPackage, UniversalShipment> getShipmentToReturnFromPopulateDataObject)
			: this(getShipmentToReturnFromPopulateDataObject, null)
		{
		}

		public DummyHandle(Func<PkgPackage, UniversalShipment> getShipmentToReturnFromPopulateDataObject, Dictionary<ZGuid, ZInt> orderLineDictionary)
		{
			GetShipmentToReturnFromPopulateDataObject = getShipmentToReturnFromPopulateDataObject;
			GetMock = GetNewWriter(orderLineDictionary);
		}

		static Func<IDataWritingManager, MockDataObjectWriter> GetNewWriter(Dictionary<ZGuid, ZInt> orderLineDictionary)
		{
			MockDataObjectWriter mock = null;

			return (writeManager) =>
			{
				if (mock == null)
				{
					if (writeManager == null)
					{
						mock = null;
					}
					else if (orderLineDictionary != null)
					{
						mock = new MockDataObjectWriterWithOrderLineDictionary(writeManager, orderLineDictionary);
					}
					else
					{
						mock = new MockDataObjectWriter(writeManager);
					}
				}

				return mock;
			};
		}

		Func<PkgPackage, UniversalShipment> GetShipmentToReturnFromPopulateDataObject { get; }
		Func<IDataWritingManager, MockDataObjectWriter> GetMock { get; }

		public DataObjectWriter<PkgPackage, UniversalShipment> Writer => GetMock(null);
		public PkgPackage PackagePassedIntoPopulateDataObject => ((MockDataObjectWriter)Writer)?.PackagePassedIntoPopulateDataObject;

		#region GetObject

		public override object GetObject(params object[] arguments)
		{
			var mock = GetMock((IDataWritingManager)arguments[0]);
			mock.GetShipmentToReturnFromPopulateDataObject = GetShipmentToReturnFromPopulateDataObject;
			return mock;
		}

		class MockDataObjectWriter : TopLevelDataObjectWriter<PkgPackage, UniversalShipment>
		{
			public MockDataObjectWriter(IDataWritingManager writeManager)
				: base(writeManager)
			{
			}

			protected override void PopulateDataObject(PkgPackage sourceBO, UniversalShipment dataObject)
			{
				PackagePassedIntoPopulateDataObject = sourceBO;
			}

			protected override void InsertParents(PkgPackage sourceBO, ref UniversalShipment dataObject)
			{
				base.InsertParents(sourceBO, ref dataObject);
				var shipmentDataObject = GetShipmentToReturnFromPopulateDataObject(sourceBO);
				dataObject = shipmentDataObject;
			}

			protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			protected override DataContextType GetTopLevelDataContextType() => DataContextType.DummyBusinessObject;

			public Func<PkgPackage, UniversalShipment> GetShipmentToReturnFromPopulateDataObject { get; set; }

			public PkgPackage PackagePassedIntoPopulateDataObject { get; private set; }
		}

		class MockDataObjectWriterWithOrderLineDictionary : MockDataObjectWriter, IOrderLineDictionaryProvider
		{
			public MockDataObjectWriterWithOrderLineDictionary(IDataWritingManager writeManager, Dictionary<ZGuid, ZInt> orderLineDictionary)
				: base(writeManager)
			{
				OrderLineDictionary = orderLineDictionary;
			}

			Dictionary<ZGuid, ZInt> IOrderLineDictionaryProvider.GetOrderLineDictionary() => OrderLineDictionary;
			Dictionary<ZGuid, ZInt> OrderLineDictionary { get; }
		}

		#endregion
	}
}
