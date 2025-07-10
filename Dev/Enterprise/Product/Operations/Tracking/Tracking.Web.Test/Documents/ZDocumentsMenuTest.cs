using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ZDocumentsMenuTest : TestCaseWithFactory
	{
		public void TestRenderControl()
		{
			AssertRenderControlFromDataSource(false, "No html is expected if no documents is available", string.Empty);
			AssertRenderControlFromAssignedDocumentsMenuProvider(false, "No html is expected if no documents is available", string.Empty);

			string expectedHTML = string.Format("<input type='button' onclick='document.location=\"{0}\";' value=\"{1}\"> ",
									DocumentRequestHandler.RequestHelper.GetHandlerUrl(typeof(DummyDocumentMenuHelper), DataContentTypes.Pdf, new ZGuid[] { DummyDocumentMenuHelper.BizObjPK, DummyDocumentMenuHelper.DocumentCommand.PK }),
									DummyDocumentMenuHelper.DocumentCommand.SU_MenuName);

			AssertRenderControlFromDataSource(true, "Incorrect html", expectedHTML);
			AssertRenderControlFromAssignedDocumentsMenuProvider(true, "Incorrect html", expectedHTML);
		}

		void AssertRenderControlFromDataSource(bool hasDocuments, string message, string expectedString)
		{
			using (DummyZPage page = new DummyZPage())
			{
				ZDocumentsMenuForTest docMenu = new ZDocumentsMenuForTest();
				page.Controls.Add(docMenu);

				AssertNull("DocumentMenuProvider", docMenu.DocumentMenuProvider);
				page.LoadDataSource();
				AssertNotNull("DocumentMenuProvider", docMenu.DocumentMenuProvider);

				((DummyBizO)page.DataSource).HasDocuments = hasDocuments;
				AssertEquals(message, expectedString, docMenu.GetHTML());
			}
		}

		void AssertRenderControlFromAssignedDocumentsMenuProvider(bool hasDocuments, string message, string expectedString)
		{
			using (DummyZPage page = new DummyZPage())
			{
				ZDocumentsMenuForTest docMenu = new ZDocumentsMenuForTest();
				page.Controls.Add(docMenu);

				DummyDocumentsMenuProvider provider = new DummyDocumentsMenuProvider();
				docMenu.DocumentMenuProvider = provider;

				provider.HasDocuments = hasDocuments;
				AssertEquals(message, expectedString, docMenu.GetHTML());
			}
		}

		#region Dummy Classes

		#region DummyZPage

		class DummyZPage : ZPage
		{
			public void LoadDataSource()
			{
				LoadOrCreateDataSource();
			}

			protected override BusinessObject GetNewDataSource()
			{
				return new DummyBizO();
			}
		}

		#endregion

		#region DummyBizO

		class DummyBizO : NonPersistentBusinessObject, IDocumentsMenuProvider
		{
			public bool HasDocuments { get; set; }

			#region IDocumentsMenuProvider Members

			DocumentsMenuHelper IDocumentsMenuProvider.DocumentsMenuHelper
			{
				get
				{
					return new DummyDocumentMenuHelper(HasDocuments);
				}
			}

			#endregion

		}

		#endregion

		#region DummyDocumentsMenuProvider

		class DummyDocumentsMenuProvider : IDocumentsMenuProvider
		{
			public bool HasDocuments { get; set; }

			#region IDocumentsMenuProvider Members

			DocumentsMenuHelper IDocumentsMenuProvider.DocumentsMenuHelper
			{
				get
				{
					return new DummyDocumentMenuHelper(HasDocuments);
				}
			}
		}

		#endregion

		#region DummyDocumentMenuHelper

		class DummyDocumentMenuHelper : DocumentsMenuHelper
		{
			public DummyDocumentMenuHelper(bool hasDocuments)
			{
				this.hasDocuments = hasDocuments;
			}

			readonly bool hasDocuments;

			#region IDocumentsMenuHelper Members

			public override List<DocumentsMenuItem> GetAvailableDocuments()
			{
				List<DocumentsMenuItem> menuItems = new List<DocumentsMenuItem>();

				if (hasDocuments)
				{
					DocumentsMenuItem item = new DocumentsMenuItem(DocumentCommand, DataContentTypes.Pdf);
					menuItems.Add(item);
				}

				return menuItems;
			}

			public override ZGuid PKForBizOCreation
			{
				get
				{
					return BizObjPK;
				}
			}

			public override IDocumentSupportable GetDocumentSupportable()
			{
				return null;
			}

			#endregion

			#region Static

			public static DocumentCommand DocumentCommand
			{
				get
				{
					if (documentCommand == null)
					{
						documentCommand = new BusinessObjectFactory().New<DocumentCommand>();
						documentCommand.SU_MenuName = "Doc #1";
					}
					return documentCommand;
				}
			}
			static DocumentCommand documentCommand;
			public static ZGuid BizObjPK = new ZGuid("494B8630-48F8-4df6-B03A-C1184922F940");

			#endregion

		}

		#endregion

		#region ZDocumentsMenuForTest

		class ZDocumentsMenuForTest : ZDocumentsMenu
		{
			public string GetHTML()
			{
				StringBuilder sb = new StringBuilder();

				using (HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(sb)))
				{
					Render(writer);
				}

				return sb.ToString();
			}
		}

		#endregion

		#endregion

		#endregion

	}
}
