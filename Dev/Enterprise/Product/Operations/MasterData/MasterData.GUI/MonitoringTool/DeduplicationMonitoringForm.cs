using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.MasterData.GUI
{
	[TestExcludeZWinFormsAllHaveFormBashers]
	public partial class DeduplicationMonitoringForm : ZChildForm, IDeduplicationDebuggerParticipant
	{
		public readonly Dictionary<string, string> DeduplicationTypeDict = new()
		{
			{ Res.GetString("0112a14b-aca7-4fa5-be18-1e5ab66595db", "Organization De-duplication Results"), DeduplicationDebuggerParticipant.OrganizationPrefix },
			{ Res.GetString("1ffb5a8f-ffae-4559-b89e-2ea31e5a5fda", "Person De-duplication Results"), DeduplicationDebuggerParticipant.PersonPrefix }
		};

		public DeduplicationMonitoringForm()
			: base()
		{
			InitializeComponent();
			MonitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			DeduplicationUtils.DebuggerHubInstance.Register(this);
			tvDelegate = new AddTreeNode(AddNodes);
			ImportButton.Visible = ShouldShowImportButton;
			InitDropdownListItem();
		}

		bool IsTrackingResults { get; set; }
		bool IsImportData { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		[SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
		public static ConcurrentDictionary<string, MonitoringObjectValue> MonitoringObjects;

		void ToggleButton_Click(object sender, EventArgs e)
		{
			if (!IsTrackingResults)
			{
				IsTrackingResults = true;
				MessageStatusBarPanel.Text = (NoResString)"Tracking";
				ToggleButton.Text = Res.GetString("354f9b60-d4bf-496d-bd9a-e8c44454274a", "Stop Tracking Results");
				MainStatusBar.ForeColor = Color.LimeGreen;
				MainStatusBar.Refresh();
			}
			else
			{
				IsTrackingResults = false;
				MessageStatusBarPanel.Text = (NoResString)"Not Tracking";
				ToggleButton.Text = Res.GetString("7db41f07-54d3-41af-8e48-3bc5444ac31d", "Start Tracking Results");
				MainStatusBar.ForeColor = Color.Orange;
				MainStatusBar.Refresh();
			}
		}

		void BtnClear_Click(object sender, EventArgs e)
		{
			MonitoringObjects.Clear();
			DeduplicationMonitoringUserControl.SetDataContext(MonitoringObjects);
			ExportDiagnosticsButton.Enabled = false;
		}

		bool ShouldShowImportButton => (Env.CurrentUser != null && Env.CurrentUser.IsSupportUser) || ZArchitecture.Modules.ClientHookLoader.Instance?.Client == Clients.EDI;

		void ImportButton_Click(object sender, EventArgs e)
		{
			ImportDataFromFiles();
		}

		#region ImportFromJsonFile

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Not a code smell")]
		void ImportDataFromFiles()
		{
			if (ShouldShowImportButton)
			{
				using (var dlg = new ZOpenFileDialog())
				{
					dlg.Filter = "Extensible Markup Language (*.json)|*.json";
					if (dlg.ShowDialog() == DialogResult.OK)
					{
						try
						{
							IsImportData = true;

							using (var fileSteam = dlg.OpenFile())
							{
								var importResult = GetMonitoringObjectsFromJSONString(StreamToString(fileSteam));
								SendMonitoringObjectsFromString(importResult);
							}
						}
						finally
						{
							IsImportData = false;
						}
					}
				}
			}
		}

		static string StreamToString(Stream stream)
		{
			stream.Position = 0;
			using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
			{
				return reader.ReadToEnd();
			}
		}

		public ConcurrentDictionary<string, MonitoringObjectValue> GetMonitoringObjectsFromJSONString(string jsonStr)
		{
			jsonStr = ChangeToSimpleModel(jsonStr);
			var results = JsonConvert.DeserializeObject<ConcurrentDictionary<string, MonitoringObjectValue>>(jsonStr, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
			return results;
		}

		void SendMonitoringObjectsFromString(ConcurrentDictionary<string, MonitoringObjectValue> importMonitoringObjects)
		{
			foreach (var result in importMonitoringObjects.OrderBy(GetKeyIndex))
			{
				var keys = result.Key.Split('_');
				Receive(result.Value.Value, keys.Length > 1 ? string.Join("_", keys.Take(keys.Length - 1)) : result.Key, new TimeSpan(10000 * result.Value.ExecutionTimeInMilliseconds));
			}
		}

		int GetKeyIndex(KeyValuePair<string, MonitoringObjectValue> result)
		{
			var keys = result.Key.Split('_');

			if (int.TryParse(keys[keys.Length - 1], out var index))
			{
				return index;
			}

			return 0;
		}

		string ChangeToSimpleModel(string inputStr)
		{
			inputStr = inputStr.Replace("\"System.Linq.Enumerable+WhereEnumerableIterator`1[[CargoWise.Tools.DuplicateDetector.PatternMatchingResultModel, CargoWise.Tools.DuplicateDetector]], System.Core\"", "\"System.Collections.Generic.List`1[[CargoWise.Tools.DuplicateDetector.PatternMatchingResultModel, CargoWise.Tools.DuplicateDetector]], mscorlib\"");

			return inputStr;
		}

		#endregion

		void ExportButton_Click(object sender, EventArgs e)
		{
			SaveMonitoringObjectToFileUtils.SaveMonitoringObjectToFile(filteredMonitoringObjects);
		}

		ConcurrentDictionary<string, MonitoringObjectValue> filteredMonitoringObjects;

		public void AddNodes()
		{
			filteredMonitoringObjects = FilteredMonitoringObjects();
			DeduplicationMonitoringUserControl.SetDataContext(filteredMonitoringObjects);
		}

		#region IDeduplicationDebuggerParticipant

		IDeduplicationDebuggerHub debuggerHub;

		public string DebuggerName => DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName;

		public IDeduplicationDebuggerHub DebuggerHub
		{
			get { return debuggerHub; }
			set { debuggerHub = value; }
		}

		public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, object glowBizO)
		{
			//	There's nothing to send
		}

		public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, string prefix)
		{
		}

		int counter;
		public delegate void AddTreeNode();
		public AddTreeNode tvDelegate;

		public void Receive(object value, string methodName, TimeSpan executionTime)
		{
			if ((IsTrackingResults || IsImportData) && DeduplicationMonitoringUserControl != null && DeduplicationMonitoringUserControl.IsHandleCreated)
			{
				if (!MonitoringObjects.Any())
				{
					counter = 0;
				}

				counter++;

				if (MonitoringObjects.TryAdd(string.Join("_", methodName, counter.ToString(CultureInfo.InvariantCulture)), new MonitoringObjectValue { ExecutionTimeInMilliseconds = (long)executionTime.TotalMilliseconds, Value = value }))
				{
					DeduplicationMonitoringUserControl.Invoke(tvDelegate);
				}

				ExportDiagnosticsButton.Enabled = true;
			}
			else
			{
				IsTrackingResults = false;
			}
		}

		#endregion

		protected override void OnClosed(EventArgs e)
		{
			DeduplicationUtils.DebuggerHubInstance.RemoveParticipant(this);
			base.OnClosed(e);
		}

		#region Filter Deduplication Results

		void FilterButton_Click(object sender, EventArgs e)
		{
			FilterDropdownList.Visible = !FilterDropdownList.Visible;
		}

		void InitDropdownListItem()
		{
			var data = new ZBoolDescriptionPairList();
			foreach (var key in DeduplicationTypeDict.Keys)
			{
				data.AddNew(key, true);
			}
			FilterDropdownList.BindingItems = data;
			CheckedIndices = FilterDropdownList.CheckedIndices.Cast<int>().ToList();
			FilterDropdownList.ItemCheck += FilterDropdownList_ItemChecked;
		}

		List<int> CheckedIndices;

		void FilterDropdownList_ItemChecked(object sender, ItemCheckEventArgs eventArgs)
		{
			CheckedIndices = FilterDropdownList.CheckedIndices.Cast<int>().ToList();

			if (eventArgs.NewValue == CheckState.Checked)
			{
				CheckedIndices.Add(eventArgs.Index);
			}
			else
			{
				CheckedIndices.Remove(eventArgs.Index);
			}

			AddNodes();
		}

		ConcurrentDictionary<string, MonitoringObjectValue> FilteredMonitoringObjects()
		{
			ConcurrentDictionary<string, MonitoringObjectValue> monitoringObjects;
			var filters = FilterDropdownList.Items.Cast<ZString>().Where((item, index) => CheckedIndices.Contains(index)).Select(u => { DeduplicationTypeDict.TryGetValue(u, out var tag); return tag; }).ToArray();

			if (filters.Any())
			{
				if (filters.Length == DeduplicationTypeDict.Count)
				{
					monitoringObjects = MonitoringObjects;
				}
				else
				{
					monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();

					foreach (var monitoringObject in MonitoringObjects.ToArray())
					{
						if (filters.Any(prefix => monitoringObject.Key.StartsWith(prefix, StringComparison.Ordinal)))
						{
							monitoringObjects.TryAdd(monitoringObject.Key, monitoringObject.Value);
						}
					}
				}
			}
			else
			{
				monitoringObjects = new ConcurrentDictionary<string, MonitoringObjectValue>();
			}

			return monitoringObjects;
		}

		#endregion

		#region Export Diagnostic

		void ExportDiagnosticsButton_Click(object sender, EventArgs e)
		{
			var detectOrganisation = false;
			var organisationPkList = new List<ZGuid>();
			var model = DeduplicationMonitoringUserControl.model;
			if (model != null)
			{
				foreach (var dict in model.monitoringObject)
				{
					var list = dict.Value.Value as IEnumerable;

					if (dict.Key.StartsWith(DeduplicationDebuggerParticipant.OrganizationPrefix, StringComparison.Ordinal))
					{
						detectOrganisation = true;
						if (list is IEnumerable<DeduplicationDebuggerMaster> patternMatchingResultList)
						{
							if (patternMatchingResultList.Any())
							{
								// Master
								organisationPkList.Add(patternMatchingResultList.First().PK);
							}
						}

						if (list is IEnumerable<CargoWise.Glow.Model.Interfaces.IOrgHeader> orgHeaderList)
						{
							// Targets
							orgHeaderList.ForEach(x => organisationPkList.Add(x.OH_PK));
						}
					}
				}
			}

			if (!detectOrganisation)
			{
				Globals.Message.Show(Res.GetString("a0da0f37-e2ee-4f46-827b-c1766ac0ffcd", "This function is only available for organization duplicate detection."));
			}
			else
			{
				var isExportDataEmpty = true;
				if (organisationPkList.Any())
				{
					var exportOrganisations = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, organisationPkList.Distinct()));
					if (exportOrganisations.Any())
					{
						isExportDataEmpty = false;
						ExportService.ExportWithSave(exportOrganisations);
					}
				}

				if (isExportDataEmpty)
				{
					Globals.Message.Show(Res.GetString("a0da0f34-e2ee-4f46-827b-c1766ac1ffcd", "There is no available data to export."));
				}
			}
		}

		NativeXmlExportService ExportService
		{
			get
			{
				if (exportService == null)
				{
					var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
					var converter = new BusinessObjectToEntityConverter { DefinitionFinder = definitionFinder };
					var serializer = new NativeXmlSerializer { Converter = converter };
					var exportValidator = ObjectFactory.Get<IExportValidator>("NativeXmlExportValidator");

					exportService = new NativeXmlExportService
					{
						Validator = exportValidator,
						Serializer = serializer
					};
				}

				return exportService;
			}
		}
		internal NativeXmlExportService exportService;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

		#endregion
	}
}
