using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Documents.DocDataObjects;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using IDocument = Enterprise.DocumentVisualizer.Core.IDocument;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ZA.DataTransfer.Universal.Documents.Testing
{
	sealed class CustomsDocDataObjectUXmlWriterTest : TestCaseWithFactory
	{
		public void TestGetDataObject_CargoDuesBrokerage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var parameters = new DummyDocDataObjectParameters();
			var cargoDues = new CargoDuesBrokerageDocDataBuilder(declaration, parameters).Build();
			var data = cargoDues.MakeDynamic();
			var document = new Mock<IDocument>();
			document.Setup(d => d.Data).Returns(data);
			document.Setup(d => d.DataContext).Returns(DataContext.CargoDuesBrokerage);
			var writer = new CustomsDocDataObjectUXmlWriter();
			var dataObject = writer.GetDataObject(DefaultDataObjectWriterStrategy.TestInstance, document.Object) as UniversalShipment;
			AssertNotNull("DataObject has been produced", dataObject);
		}

		sealed class DummyDocDataObjectParameters : IDocDataObjectParameters
		{
			public string DocumentTitle { get; set; }

			public string DataStoreName { get; set; }

			public object Data { get; set; }

			public IStmALogProvider LogProvider { get; set; }
		}
	}
}
