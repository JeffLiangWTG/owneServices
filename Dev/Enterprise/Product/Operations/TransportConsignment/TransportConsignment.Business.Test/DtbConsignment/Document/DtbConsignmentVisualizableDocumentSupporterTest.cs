using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentVisualizableDocumentSupporter))]
	class DtbConsignmentVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			AssertEquals(nameof(supporter.CustomizeFormCheckpoint), Env.Security.DtbConsignmentCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetDocDataObject()
		{
			var docDataObject = supporter.GetDocDataObject(consignment, DataContext.CMRConsignmentNote, null);
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CMRConsignmentNoteDocDataObjectCollection>($"{nameof(docDataObject.Right)} type", docDataObject.Right);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consignment = Factory.NewWithValidTestData<DtbConsignment>();
			supporter = new DtbConsignmentVisualizableDocumentSupporter(consignment);
		}

		DtbConsignment consignment;
		DtbConsignmentVisualizableDocumentSupporter supporter;
	}
}
