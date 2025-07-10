using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	static class UniversalXmlSampleSavingContextMenuManager
	{
		internal static void AttachToGrid(ZGrid actionsGrid)
		{
#if DEBUG
			MenuItem saveSampleXmlSeparatorTop = new ZMenuItem("-");
			MenuItem saveSampleXmlMenuItem = new ZMenuItem(SaveSampleXmlCaption);
			MenuItem saveSampleXmlSeparatorBottom = new ZMenuItem("-");
			saveSampleXmlMenuItem.Click += (sender, eventArgs) =>
			{
				var notification = actionsGrid.ListManager.GetCurrent() as ProcessTaskNotification;
				SaveSample(notification?.Parent?.GetJob(), notification);
			};

			var actionsContextMenu = actionsGrid.ContextMenu;
			actionsContextMenu.MenuItems.Add(actionsGrid.ContextMenu.MenuItems.Count - 1, saveSampleXmlSeparatorTop);
			actionsContextMenu.MenuItems.Add(actionsGrid.ContextMenu.MenuItems.Count - 1, saveSampleXmlMenuItem);
			actionsContextMenu.MenuItems.Add(actionsGrid.ContextMenu.MenuItems.Count - 1, saveSampleXmlSeparatorBottom);

			actionsContextMenu.Popup += (sender, eventArgs) =>
			{
				if (actionsGrid.ListManager.Position >= 0)
				{
					bool saveSampleXMLIsVisible = CanShow(actionsGrid.ListManager.GetCurrent() as ProcessTaskNotification);

					saveSampleXmlSeparatorTop.Visible = saveSampleXMLIsVisible;
					saveSampleXmlMenuItem.Visible = saveSampleXMLIsVisible;
					saveSampleXmlSeparatorBottom.Visible = saveSampleXMLIsVisible;
				}
			};
#endif
		}
		internal static bool CanShow(ProcessTaskNotification action)
		{
			return action != null
					&& action.PQ_TriggerType.EqualsAny(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)
					&& action.Parent.GetJob().GetAttribute<UniversalDataContextAttribute>() != null;
		}

		internal static void SaveSample(BusinessObject parentBO, ProcessTaskNotification action)
		{
			if (action != null && parentBO != null)
			{
				if (parentBO.HasChanges)
				{
					Globals.Message.ShowError(Res.GetString("9b76d1fd-f475-46a4-be1a-9a06e7792da5", "Please save changes before generating sample XML."), SaveSampleXmlCaption);
				}
				else
				{
					var dataSourceManager = parentBO.GetUniversalDataContextManager();
					var workflowDescriptor = action.Parent.GetWorkflowDescriptor();
					var testFileWriter = workflowDescriptor.GetTestFileWriter(action, parentBO);
					if (testFileWriter != null)
					{
						var result = testFileWriter.Export(dataSourceManager.DefaultOutputDirectory);
						if (result.Success)
						{
							Globals.Message.ShowInformation(result.Message, SaveSampleXmlCaption);
						}
						else
						{
							Globals.Message.ShowError(result.Message, SaveSampleXmlCaption);
						}
					}
				}
			}
		}

		internal static MultilingualString SaveSampleXmlCaption
		{
			get { return ResString.GetMultilingualString("8532C525-AF24-457C-A264-616A3F031B79", "Save sample XML (Debug Build Only)"); }
		}
	}
}
