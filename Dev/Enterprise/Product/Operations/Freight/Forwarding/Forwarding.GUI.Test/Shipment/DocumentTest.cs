using System.Drawing;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.GUI;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class DocumentTest : TestCaseWithDocumentFactory
	{
		[RequiresSTA]
		public void TestShowAQISDisclaimerMessage()
		{
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = MasterFactory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);

				var declaration = MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_DeclarationReference = "S00001000";

				Assert("Is an AU Declaration", declaration is IEDocDeliveryAuthorization);

				var main = MasterFactory.NewWithValidTestData<DocumentScanning.Business.StorageMain>();
				main.SM_ParentFK = declaration.PK;
				main.SM_Type = Core.Constants.DocManagerCodes.JobDeclaration;
				var document = main.Documents.AddNew();
				document.SC_FileName = "QRPTest";
				document.SC_DataType = Core.Constants.FileFormats.PDF;
				document.SC_DocType = Core.Constants.RefDocTypes.QuarantineRemotePrint;
				MasterFactory.Save();

				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var mainTabControl = form.FindSingle<ZTabControl>("MainTabControl", 1);
					var eDocTabPage = (ZTabPage)mainTabControl.TabPages["eDocsTabPage"];
					mainTabControl.SelectedTab = eDocTabPage;

					var plugIn = (eDocsPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
					var userControl = (eDocsUserControl)plugIn.UserControl;

					var parentsGrid = userControl.FindSingle<ZGrid>("RelatedParentsGrid");
					var decIndex = parentsGrid.ListManager.List.IndexOf(main);
					parentsGrid.ListManager.Position = decIndex;

					var grid = userControl.FindSingle<DocumentsZGrid>("StorageDocsGrid");
					grid.ListManager.Position = 0;
					var selectedDocument = (DocumentScanning.Business.StorageDocsBase)grid.SelectedElements[0];
					var startingClickPoint = new Point(10, 30);
					grid.LastClickPoint = startingClickPoint;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var viewMenuItem = grid.ContextMenu.MenuItems.FindByText(DocumentScanning.Business.Constants.DeliverDocumentMenuText);
					viewMenuItem.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains("To be able to print export documents"));
				}
			}
		}
	}
}
