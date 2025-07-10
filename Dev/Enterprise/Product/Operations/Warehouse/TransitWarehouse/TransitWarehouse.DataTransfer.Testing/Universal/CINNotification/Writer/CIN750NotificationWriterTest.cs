using System;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Document.Testing
{
	abstract class CIN750NotificationWriterTest<TDocDataObjectSource, TWriter> : TestCaseWithFactory where TDocDataObjectSource : CIN750Notification where TWriter : CIN750NotificationWriter<TDocDataObjectSource>
	{
		[TestDate(2023, 12, 31)]
		public void TestPopulateShipment()
		{
			var notification = GetSource();
			notification.MessageID = Guid.Empty.ToString();
			var writer = GetWriter();
			var shipment = writer.GetDataObject(notification);

			AssertUXml(shipment, ExpectedXML());
		}

		#region AssertUXml

		protected void AssertUXml(UniversalShipment universalShipment, string expectedXml)
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var xmlWriter = ObjectFactory.Get<IXmlWriter>();
				xmlWriter.WriteXML(universalShipment, stream);

				using (var reader = new StreamReader(stream))
				{
					var result = reader.ReadToEnd();
					AssertMultilineASCIIEquals("UXml", expectedXml, result);
				}
			}
		}

		#endregion

		protected abstract TWriter GetWriter();

		protected abstract TDocDataObjectSource GetSource();

		protected abstract ZString ExpectedXML();

		#region Implementation

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		protected IContext Context => context ?? (context = new TransitCommonContext(Factory));
		IContext context;

		#endregion
	}
}
