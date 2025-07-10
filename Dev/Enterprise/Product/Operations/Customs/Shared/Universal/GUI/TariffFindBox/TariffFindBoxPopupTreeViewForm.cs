using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class TariffFindBoxTreeViewForm : ZChildForm
		, IDisposable
		, IFindBoxPopup
	{
		public TariffFindBoxTreeViewForm()
		{
		}

		public TariffFindBoxTreeViewForm(TariffSearchHelper helper)
			: base(helper)
		{
			SetDefaultFilterBusinessObjects(helper);
			InitializeComponent();
			TariffTreeView.BeforeExpand += TariffTreeView_BeforeExpand;
			TariffTreeView.AfterSelect += TariffTreeView_AfterSelect;
			contextMenuStrip.Opening += ContextMenuStrip_Opening;
			oKButton.Enabled = false;
			ViewButton.Enabled = false;
			InitializeHideShowButton();
			SetFilterStripGroupBoxCaption(helper);
			selectNomenclatureModes = helper.SelectNomenclatureModes;

			TariffTreeView.AllowOverlap(FilterStripGroupBox);
		}

		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			HandleTreeViewSizing();
		}

		void HandleTreeViewSizing()
		{
			if (TariffTreeView != null && StripControl != null)
			{
				var paddingBetweenStripControlAndTariffTreeView = ControlDpiScalingHelper.ScaleToCurrentDpiY(3); // 3
				var paddingBetweendTariffTreeViewAndFullDescriptionTextBox = ControlDpiScalingHelper.ScaleToCurrentDpiY(16); //16
				var groupBoxLeftPadding = ControlDpiScalingHelper.ScaleToCurrentDpiY(4); //4
				var groupBoxPaddingBottom = ControlDpiScalingHelper.ScaleToCurrentDpiY(38); //38

				var toolStripHelp = StripControl.FindSingle<ZToolStrip>(x => x.Name == "ToolStripHelp");
				var treeViewHeight = FullDescriptionTextBox.Top - MainPanel.Top - TariffTreeView.Top - paddingBetweendTariffTreeViewAndFullDescriptionTextBox;
				var mainPanelExpected = ControlDpiScalingHelper.NewScaledPoint(0, ToolBarPanel.Bottom, false);
				var treeViewExpected = ControlDpiScalingHelper.NewScaledPoint(0, (StripControl.Visible ? toolStripHelp.Bottom + StripControl.Top + paddingBetweenStripControlAndTariffTreeView : FilterStripGroupBox.Top), false);
				var stripControlExpected = ClientSize.Width - groupBoxLeftPadding;

				Func<bool> shouldResize = () =>
				{
					return MainPanel.Location != mainPanelExpected
						|| TariffTreeView.Location != treeViewExpected
						|| TariffTreeView.Left != 0
						|| TariffTreeView.Right != ClientRectangle.Right
						|| TariffTreeView.Height != treeViewHeight
						|| StripControl.Width != stripControlExpected;
				};

				if (shouldResize())
				{
					MainPanel.Location = mainPanelExpected;
					TariffTreeView.Location = treeViewExpected;
					ControlDpiScalingHelper.SetHeight(MainPanel, TariffTreeView.Top + treeViewHeight, false);
					ControlDpiScalingHelper.SetWidth(MainPanel, ClientSize.Width, false);
					ControlDpiScalingHelper.SetWidth(FilterStripGroupBox, MainPanel.Width, false);
					ControlDpiScalingHelper.SetHeight(FilterStripGroupBox, StripControl.Height + groupBoxPaddingBottom, false);
					ControlDpiScalingHelper.SetWidth(StripControl, stripControlExpected, false);
					ControlDpiScalingHelper.SetHeight(TariffTreeView, treeViewHeight, false);
					ControlDpiScalingHelper.SetWidth(TariffTreeView, MainPanel.Width, false);

					TariffTreeView.BringToFront();
				}
			}
		}

		protected void SetDefaultFilterBusinessObjects(TariffSearchHelper helper)
		{
			FilterBusinessObject = new RefCusTariffFilterStripBusinessObject(helper);
			var defaults = new FilterBusinessObjectDefaults();

			if (!helper.ChapterHeadingTariff.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.TariffCode, "Property", helper.ChapterHeadingTariff));
			}
			if (!helper.PartialDescription.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.DefaultLanguageDescription, "Property", helper.PartialDescription));
			}
			if (!helper.EffectiveDate.IsEmpty)
			{
				defaults.Add(new FilterBusinessObjectDefault(Constants.RefCusTariffFilters.EffectiveDate, "Property1", helper.EffectiveDate));
			}
			FilterBusinessObject.SetExternalDefaults(defaults);
		}

		public void InitializeHideShowButton()
		{
			var showHideFilterMenuItem = new ZMenuItem(Res.GetData("13C284C4-1AD2-47E7-8F82-592F12D2636E", "Hide/Show Filters"), ShowFilterMenuItem_Click, IconTypes.CollapseButtonActive, IconTypes.CollapseButtonRest);
			HideShowToolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(showHideFilterMenuItem);
			HideShowToolStripItem.Image = Icons.ImageList.Images[Icons.GetImageIndex(IconTypes.CollapseButtonActive)];
			Toolstrip.Items.Add(HideShowToolStripItem);
			ToolBarPanel.BackColor = SystemDataRegistry.Instance.ColorTheme.ToolbarColor;
		}

		protected void SetFilterStripGroupBoxCaption(TariffSearchHelper helper)
		{
			var dataGroups = new ZStringBuilder();
			ZString[] groupingCodes = helper.DataGroupingCodes();
			foreach (var group in groupingCodes)
			{
				dataGroups.Append(group);
			}

			var caption = Res.GetString("3FD38F6E-500F-4A50-8DB8-EAAA5683EF2E", "{0} Tariff Search Criteria", dataGroups.ToStringWithDelimiterBetweenAppends(","));
			this.FilterStripGroupBox.Text = caption;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			base.InitialiseForm();
		}

		void ShowFilterMenuItem_Click(object sender, EventArgs e)
		{
			StripControl.Visible = !StripControl.Visible;
			if (StripControl.Visible)
			{
				HideShowToolStripItem.ImageIndex = Icons.GetImageIndex(IconTypes.CollapseButtonRest);
				HideShowToolStripItem.Image = Icons.ImageList.Images[Icons.GetImageIndex(IconTypes.CollapseButtonActive)];
			}
			else
			{
				HideShowToolStripItem.ImageIndex = Icons.GetImageIndex(IconTypes.ExpandButtonRest);
				HideShowToolStripItem.Image = Icons.ImageList.Images[Icons.GetImageIndex(IconTypes.ExpandButtonActive)];
			}
			HandleTreeViewSizing();
		}

		public new TariffSearchHelper BusinessEntity
		{
			get { return (TariffSearchHelper)base.BusinessEntity; }
		}

		TreeNode AddMember(TariffDataObject bizObj, TreeNode parentNode, TreeNodeCollection nodes)
		{
			var children = bizObj.RelatedDataCollection.Take(2).ToArray();
			if (children.Length == 1 && children[0].FullDescription == bizObj.FullDescription)
			{
				bizObj = children[0];
			}
			var node = nodes.Add(TruncateNodeText(bizObj, parentNode));
			node.Tag = bizObj;
			if (!bizObj.IsNomenclatureGroup && matchedTreeNode == null && bizObj == matchedTariff)
			{
				node.BackColor = Color.LightGreen;
				matchedTreeNode = node;
			}
			if (bizObj.RelatedDataCollection.Any())
			{
				if (shouldExpand)
				{
					AddRelatedDataCollection(bizObj.RelatedDataCollection, node, node.Nodes);
				}
				else
				{
					node.Nodes.Add(PleaseWaitLoading);
				}
			}
			return node;
		}

		string TruncateNodeText(TariffDataObject bizObj, TreeNode parentNode)
		{
			string fullText = bizObj?.TariffCodeAndDescription.Replace("\r", "").Replace("\n", "") ?? string.Empty;
			return fullText.TruncateToFit(TariffTreeView.Font, TariffTreeView.Width - ((parentNode?.Level ?? -1) + 2) * ControlDpiScalingHelper.ScaleToCurrentDpiX(26) - ControlDpiScalingHelper.ScaleToCurrentDpiX(4));
		}

		protected static string PleaseWaitLoading => Res.GetString("DF7C6C54-903E-48E5-A7F4-7C5D895515D5", "Please wait - Loading");

		TariffDataObject SelectedElement => (TariffDataObject)TariffTreeView.SelectedNode?.Tag;

		protected void ShowContentsOfNode(TreeNode node)
		{
			if (node.Nodes.Count >= 1)
			{
				if (node.Nodes[0].Text == PleaseWaitLoading)
				{
					if (node.Nodes.Count > 1)
					{
						ErrorReporter.ReportOnce("TariffFindBoxTreeViewForm.BadNodeCount", "Incorrect number of nodes - expected 1 but was " + node.Nodes.Count.ToString(Culture.Invariant));
					}
					node.Nodes.Clear();
					var bizObj = (TariffDataObject)node.Tag;
					AddRelatedDataCollection(bizObj.RelatedDataCollection, node, node.Nodes);
				}
			}
		}

		void AddRelatedDataCollection(IEnumerable<TariffDataObject> relatedDataCollection, TreeNode parentNode, TreeNodeCollection nodes)
		{
			foreach (var relatedData in relatedDataCollection)
			{
				AddMember(relatedData, parentNode, nodes);
			}
		}

		#region IFindBoxPopup Members

		IFindBox findBox;

		void IFindBoxPopup.SelectRowByPK(ZGuid pK)
		{
			// not necessary as can't add a new record to the TreeView
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			this.findBox = findBox;
			if (this.findBox != null)
			{
				SetInitialCodeForSearch(this.findBox.Code, Constants.RefCusTariffFilters.TariffCode);
			}
			ZFormModaliser.Show(this, parentForm);
		}

		internal void SetInitialCodeForSearch(ZString code, string propertyName)
		{
			FilterBusinessObject.SetInitialCodeForSearch(code, propertyName);
		}
		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup)
		{
			return SilentSelectResult.None;
		}

		#endregion

		void TariffTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			ShowContentsOfNode(e.Node);
		}

		void TariffTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			BusinessEntity.TariffDataObjects.RemoveAll();
			var bizObj = (TariffDataObject)e.Node.Tag;
			BusinessEntity.TariffDataObjects.Add(bizObj);
			oKButton.Enabled = CanSelectTariff(bizObj, selectNomenclatureModes);
			ViewButton.Enabled = !bizObj.IsNomenclatureGroup;
		}

		void oKButton_Click(object sender, EventArgs e)
		{
			var selectedElement = this.SelectedElement;
			if (selectedElement != null && CanSelectTariff(selectedElement, selectNomenclatureModes))
			{
				DialogResult = DialogResult.OK;
				findBox.Code = selectedElement.TariffCode;
				findBox.Description = selectedElement.FullDescription;
				Close();
			}
		}

		bool CanSelectTariff(TariffDataObject selectedElement, List<SelectionStyle> selectModes)
		{
			var tariffCode = selectedElement?.TariffCode ?? ZString.Empty;
			if (!selectedElement.IsNomenclatureGroup)
			{
				return selectModes.Any(x => x == SelectionStyle.Tariff);
			}
			else
			{
				return selectModes.Any(x => ((int)x) == tariffCode.Length);
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			StripControl.SetFindStatus(false);

			var filterBizO = (sender as ZFilterStripBaseControl)?.FilterBusinessObject as RefCusTariffFilterStripBusinessObject;
			BusinessEntity.RunPreSaveValidation();
			if (BusinessEntity.HasErrors)
			{
				ShowErrorsDialog();
				StripControl.SetFindStatus(true);
			}
			else
			{
				matchedTreeNode = null;
				TariffTreeView.Nodes.Clear();

				BusinessEntity.TariffDataObjects.RemoveAll();

				if (filterBizO != null)
				{
					Search(filterBizO);
				}
				else
				{
					UpdateStatusBar(Res.GetString("BEF1A487-1ACF-4471-8AA9-4AD6455E6A5D", "No filter specified."), CargoWise.ComponentModel.NotificationType.Information);
					StripControl.SetFindStatus(true);
				}
			}
		}

		void Search(RefCusTariffFilterStripBusinessObject filterBizO)
		{
			matchedTariff = null;
			var businessEntity = BusinessEntity;
			if (businessEntity != null && filterBizO != null)
			{
				var searchFilters = GetSearchFilters();
				var result = businessEntity.Search(filterBizO.Filter, searchFilters.DateOnlyFilter, searchFilters.DescriptionOnlyFilter, searchFilters.TariffOnlyFilter);
				BuildTree(filterBizO, result);
				(ZQuery DateOnlyFilter, ZQuery DescriptionOnlyFilter, ZQuery TariffOnlyFilter) GetSearchFilters()
				{
					filterBizO.ModuleFilters.SupportsQueryCaching = false;
					var dateOnlyFilter = filterBizO.ModuleFilters.GetFilterQuery(filterBizO.ActiveModuleFilters.OfType<ModuleSingleDateFilter>().Where(x => x.Description == Constants.RefCusTariffFilters.EffectiveDate).Take(1));
					var descriptionOnlyFilter = filterBizO.ModuleFilters.GetFilterQuery(filterBizO.ActiveModuleFilters.OfType<ModuleTextFilter>().Where(x => x.Description == Constants.RefCusTariffFilters.DefaultLanguageDescription).Take(1));
					var tariffFilter = filterBizO.ActiveModuleFilters.OfType<ModuleTariffFilter>().FirstOrDefault(x => x.Description == Constants.RefCusTariffFilters.TariffCode);
					var tariffOnlyFilter = filterBizO.ModuleFilters.GetFilterQuery(tariffFilter == null ? Array.Empty<ModuleFilter>() : new ModuleFilter[] { tariffFilter });
					businessEntity.ChapterHeadingTariff = tariffFilter?.Property ?? ZString.Empty;
					filterBizO.ModuleFilters.SupportsQueryCaching = true;
					return (dateOnlyFilter, descriptionOnlyFilter, tariffOnlyFilter);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected void BuildTree(RefCusTariffFilterStripBusinessObject filterBizO, TariffDataObject[] tariffDataResult)
		{
			if (tariffDataResult.Length > 0)
			{
				matchedTariff = BusinessEntity.TariffMatched;
				shouldExpand = filterBizO.ActiveModuleFilters.OfType<ModuleFlagsFilter>()
					.Any(filter => filter.Description == Constants.RefCusTariffFilters.ShowExpandedResults && filter.Property0);
				var nodesToExpand = new List<TreeNode>();
				foreach (var tariffData in tariffDataResult)
				{
					var node = AddMember(tariffData, null, TariffTreeView.Nodes);

					if (shouldExpand)
					{
						nodesToExpand.Add(node);
					}
				}

				if (shouldExpand)
				{
					var totalLeafNodes = tariffDataResult.Sum(x => x.GetNumberOfLeafNodes());
					Action expandAction = () =>
					{
						foreach (var node in nodesToExpand)
						{
							node.ExpandAll();
						}
					};

					if (totalLeafNodes < TariffSearchHelper.MaxRecommendedRowsToLoad)
					{
						expandAction();
					}
					else
					{
						ExpandSlownessWarning(expandAction);
					}
				}
			}

			UpdateStatusBar(BusinessEntity.NumberRecordsLoadedLabelText, CargoWise.ComponentModel.NotificationType.Information);

			if (BusinessEntity.ShouldShowNumberLoadedMessageBox)
			{
				Globals.Message.Show(BusinessEntity.NumberRecordsLoadedLabelText, Res.GetString("TariffSearchHelper|SearchResults", "Search Results"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}

			if (matchedTreeNode != null)
			{
				TariffTreeView.SelectedNode = matchedTreeNode;
				TariffTreeView.Focus();
			}

			StripControl.SetFindStatus(true);
		}

		void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
		{
			var selectedElement = this.SelectedElement;
			ViewToolStripMenuItem.Enabled = selectedElement != null && !selectedElement.IsNomenclatureGroup;
		}

		void ExpandSlownessWarning(Action expand)
		{
			if (expand != null && Globals.Message.Show(Res.GetString("{B0DBC2E4-5F97-4DAC-B960-FF6DE79BC161}", "Expanding tariff data can take a long time. Do you wish to continue?"), Res.GetString("{6882FB70-A8E5-449A-8E01-6CBEA33F9379}", "LOADING DATA"), MessageBoxButtons.YesNo, DialogResult.Yes) == DialogResult.Yes)
			{
				var formHandle = this.Handle;
				try
				{
					this.Cursor = Cursors.AppStarting;
#if !WINZOR
					if (SafeNativeMethods.CanLockWindow)
					{
						SafeNativeMethods.LockWindowUpdate(formHandle);
					}
#endif

					expand();
				}
				finally
				{
#if !WINZOR
					if (SafeNativeMethods.CurrentlyLockedWindow == formHandle)
					{
						SafeNativeMethods.UnlockWindowUpdate(formHandle);
					}
#endif
					this.Cursor = Cursors.Default;
				}
			}
		}

		void ViewToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ViewButton_Click(sender, e);
		}

		void CollapseAllToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TariffTreeView.CollapseAll();
		}

		void ExpandToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedElement = this.SelectedElement;
			if (selectedElement != null && selectedElement.IsNomenclatureGroup)
			{
				var selectedNode = TariffTreeView.SelectedNode;
				if (selectedNode != null)
				{
					ExpandSlownessWarning(selectedNode.ExpandAll);
				}
			}
		}

		void CollapseToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var selectedElement = this.SelectedElement;
			if (selectedElement != null)
			{
				TariffTreeView.SelectedNode?.Collapse();
			}
		}

		void ViewButton_Click(object sender, EventArgs e)
		{
			var selectedElement = this.SelectedElement;
			if (selectedElement != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
				controller.ShowViewForm(selectedElement.CusTariff);
			}
		}

		void StripControl_Layout(object sender, LayoutEventArgs e)
		{
			HandleTreeViewSizing();
		}

		void FindTextToolStripMenuItem_Click(object sender, EventArgs e)
		{
			TariffTreeView.ShowFindForm();
		}

		void StripControl_PerformSearch(object sender, EventArgs e)
		{
			FindButton_Click(sender, e);
		}

		void TariffTreeView_DoubleClick(object sender, EventArgs e)
		{
			oKButton_Click(this, EventArgs.Empty);
		}

		TariffDataObject matchedTariff;
		TreeNode matchedTreeNode;
		bool shouldExpand;
		readonly List<SelectionStyle> selectNomenclatureModes = new List<SelectionStyle> { SelectionStyle.Tariff };
	}
}
