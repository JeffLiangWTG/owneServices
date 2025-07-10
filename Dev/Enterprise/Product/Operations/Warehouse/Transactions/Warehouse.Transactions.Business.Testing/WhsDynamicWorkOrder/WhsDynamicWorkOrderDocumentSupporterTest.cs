using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderDocumentSupporter))]
	class WhsDynamicWorkOrderDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsDynamicWorkOrder, DocSupporter.BusinessContext);
		}

		public override void TestGetContactOrganisation()
		{
			var dynamicWorkOrder = (WhsDynamicWorkOrder)GetNewBusinessObject();
			var contact = dynamicWorkOrder.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY);
			AssertNull(contact);
		}

		public override void TestGetDocBusinessObjects()
		{
			var docket = (WhsDynamicWorkOrder)BusinessObject;
			var orderWrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsDynamicWorkOrder, null);
			AssertNull(orderWrappers.Single());

			var pickableDocketWrappers = DocSupporter.GetDocumentWrappers(Core.Constants.DataContext.WhsPickableDocket, null);
			AssertEquals("DocWhsPickableDocket", pickableDocketWrappers[0].GetType().Name);
			AssertEquals(docket, pickableDocketWrappers[0].WrappedObject);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<WhsDynamicWorkOrder>();

		protected override Core.Constants.DataContext DataContext => Core.Constants.DataContext.WhsDynamicWorkOrder;

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsDynamicWorkOrderCustomiseDocuments;
	}
}
