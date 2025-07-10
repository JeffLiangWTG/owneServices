using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.Registry;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	[TestedType(typeof(DocAutoDeliverStmMenuItemCollection))]
	sealed class DocAutoDeliverStmMenuItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DocAutoDeliverStmMenuItemCollection(Factory);
		}

		public void TestCreateRelationshipFilter_FiltersByFormType_WithRegistryOn()
		{
			using (DocumentsDataRegistry.Instance.EnableFormSupportforDocumentDeliveryCompletionActions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var doc = CreateTestMenuItem("doc", Core.Constants.StmMenuItemTypes.Documents);
				var web = CreateTestMenuItem("web", Core.Constants.StmMenuItemTypes.WebReports);
				var action = CreateTestMenuItem("action", Core.Constants.StmMenuItemTypes.OperationalActions);
				var form = CreateTestMenuItem("form", Core.Constants.StmMenuItemTypes.Forms);

				Factory.Save();

				var collection = new DocAutoDeliverStmMenuItemCollection(Factory);
				collection.Load();

				AssertCollectionContains("Collection should contain item of DOC type", doc, collection);
				AssertCollectionContains("Collection should contain item of WEB type", web, collection);
				AssertCollectionContains("Collection should contain item of FRM type", form, collection);
				AssertCollectionNotContains("Collection should not contain item of ACT type", action, collection);
			}
		}

		public void TestCreateRelationshipFilter_FiltersByFormType_WithRegistryOff()
		{
			using (DocumentsDataRegistry.Instance.EnableFormSupportforDocumentDeliveryCompletionActions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var doc = CreateTestMenuItem("doc", Core.Constants.StmMenuItemTypes.Documents);
				var web = CreateTestMenuItem("web", Core.Constants.StmMenuItemTypes.WebReports);
				var action = CreateTestMenuItem("action", Core.Constants.StmMenuItemTypes.OperationalActions);
				var form = CreateTestMenuItem("form", Core.Constants.StmMenuItemTypes.Forms);

				Factory.Save();

				var collection = new DocAutoDeliverStmMenuItemCollection(Factory);
				collection.Load();

				AssertCollectionContains("Collection should contain item of DOC type", doc, collection);
				AssertCollectionContains("Collection should contain item of WEB type", web, collection);
				AssertCollectionNotContains("Collection should not contain item of FRM type", form, collection);
				AssertCollectionNotContains("Collection should not contain item of ACT type", action, collection);
			}
		}

		StmMenuItem CreateTestMenuItem(string menuName, string menuType)
		{
			var item = Factory.New<StmMenuItem>();
			item.SU_MenuName = menuName;
			item.SU_MenuType = menuType;
			item.SU_ContactType = "ALL";
			item.SU_PreventAutoDelivery = false;
			return item;
		}
	}
}
