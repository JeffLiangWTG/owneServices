using System;
using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Aga.Business.Tree;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationMonitoringUserControl : ZUserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Clickable Text constants")]
		readonly string[] clickableItems = { "Target", "Org" };

		class ToolTipProvider : IToolTipProvider
		{
			public string GetToolTip(TreeNodeAdv node, NodeControl nodeControl)
			{
				if (node.Tag is DeduplicationMonitoringTypeItem)
				{
					return Res.GetString("1e6ddfa7-b396-4487-9c1f-59b4b4d64684", "Double click to view details");
				}

				return null;
			}
		}

		internal DeduplicationMonitoringModel model;

		public DeduplicationMonitoringUserControl()
		{
			InitializeComponent();

			name.ToolTipProvider = new ToolTipProvider();
			name.EditorShowing += new CancelEventHandler(Name_EditorShowing);

			MonitoringObject = new ConcurrentDictionary<string, MonitoringObjectValue>();
			model = new DeduplicationMonitoringModel(MonitoringObject);
		}

		ConcurrentDictionary<string, MonitoringObjectValue> MonitoringObject { get; set; }

		public void SetDataContext(ConcurrentDictionary<string, MonitoringObjectValue> monitoringObject)
		{
			MonitoringObject = monitoringObject;
			model.SetContext(monitoringObject);
			treeView.Model = new SortedTreeModel(model);
		}

		void Model_NodesRemoved(object sender, TreeModelEventArgs e)
		{
		}

		void Model_NodesInserted(object sender, TreeModelEventArgs e)
		{
		}

		void Name_EditorShowing(object sender, CancelEventArgs e)
		{
			if (treeView.CurrentNode.Tag is DeduplicationMonitoringMethodNameItem)
			{
				e.Cancel = true;
			}
		}

		void TreeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			if (e.Node.Tag is DeduplicationMonitoringTypeItem typeItem)
			{
				if (MonitoringObject.TryGetValue(typeItem.Parent.ItemPath, out MonitoringObjectValue objValue))
				{
					var list = objValue.Value as IEnumerable;
					var counter = 0;
					var objType = objValue.Value.GetType().GetGenericArguments()[0];

					foreach (var listItem in list)
					{
						counter++;

						if (typeItem.Tag.ToString() == counter.ToString(CultureInfo.InvariantCulture))
						{
							ZString debugText = ZString.Empty;

							if (objType == typeof(ScoringResult))
							{
								debugText = ((ScoringResult)listItem).DebuggingReport();
							}
							else if (objType == typeof(PatternMatchingResultModel))
							{
								debugText = ((PatternMatchingResultModel)listItem).Reporter.DebuggingReport;
							}

							if (!debugText.IsEmpty)
							{
								var reporter = new DeduplicationDebugReporter(debugText);
								var detailsForm = new DeduplicationMonitoringDetailsForm(reporter)
								{
									TopMost = true
								};

								ZFormModaliser.ShowDialogAndDispose(detailsForm);
							}
						}
					}
				}
			}
			else if (e.Node.Tag is DeduplicationMonitoringValueItem valueItem)
			{
				if (clickableItems.Contains(valueItem.Name))
				{
					MatchCollection pks = Regex.Matches(valueItem.DataValue, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}");

					if (pks.Count > 0)
					{
						BusinessObjectFactory factory = new BusinessObjectFactory();
						var organisation = factory.Load<OrgHeader>(Guid.Parse(pks[0].Value));

						if (organisation != null && Env.Security.OrganisationView.IsAllowed)
						{
							ZFormModaliser.ShowDialogAndDispose(new ZOrganisationsForm(organisation));
						}
					}
				}
			}
			else if (e.Node.Tag is DeduplicationMonitoringMethodNameItem nameItem)
			{
				ZString debugText = nameItem.DebugLog;

				if (!debugText.IsEmpty)
				{
					var reporter = new DeduplicationDebugReporter(debugText);
					var detailsForm = new DeduplicationMonitoringDetailsForm(reporter)
					{
						TopMost = true
					};

					ZFormModaliser.ShowDialogAndDispose(detailsForm);
				}
			}
		}

		void TreeView_NodeMouseClick(object sender, TreeNodeAdvMouseEventArgs e)
		{
			if (e.Node.Tag is DeduplicationMonitoringMethodNameItem item)
			{
				if (!String.IsNullOrEmpty(item.DBCommandText) && e.Control?.ParentColumn == queryColumn)
				{
					ZFormModaliser.Show(new DeduplicationQueryAnalyzer(item), null);
				}
			}
		}
	}
}
