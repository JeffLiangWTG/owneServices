using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.MasterData.GUI
{
	public partial class FilterOperationUserControl : ZUserControl
	{
		public const string OrgDynamicFilterKey = "MDMPanelDynamicOrganizationFiltersKey_";

		public FilterOperationUserControl()
		{
			InitializeComponent();
		}

		public DeduplicationOrganisationResultDetail DuplicationResultDetail { get; set; }

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory { get => factory ?? (factory = new BusinessObjectFactory()); }

		List<string> SavedFilters { get; } = new List<string>();

		Func<List<FilterItemUserControl>> GetFilterItemUserControls => () => OrganisationFilterContentControl.DynamicFilterPanel.Controls.OfType<FilterItemUserControl>().ToList();

		public AdvancedFilterCriteriaControl AdvancedFilterCriteriaControl { get; set; }

		OrganisationFilterContentControl OrganisationFilterContentControl => AdvancedFilterCriteriaControl?.OrganisationFilterContentControl;

		public PotentialDuplicatesUserControl PotentialDuplicatesUserControl { get; set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
			SavedFilters.Clear();

			findQuery.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, OrgDynamicFilterKey);
			var stmData = Factory.Load<StmData>(findQuery);
			stmData.ForEach(x => SavedFilters.Add(x.SD_Name.Substring(OrgDynamicFilterKey.Length, x.SD_Name.Length - OrgDynamicFilterKey.Length)));
			SavedFilters.Sort();
			SavedFilterComboBox.DataSource = SavedFilters;
		}

		void AddFilterButton_Click(object sender, EventArgs e)
		{
			if (AdvancedFilterCriteriaControl != null)
			{
				OrganisationFilterContentControl.AddFilterAndResizeControl();
				OrganisationFilterContentControl.DynamicFilterPanel.Controls.OfType<FilterItemUserControl>().ForEach(v => v.RemoveFilterButton.Enabled = true);
			}
		}

		void LoadButtonOnClick(object sender, EventArgs e)
		{
			var filterName = SavedFilterComboBox.Text;
			if (string.IsNullOrWhiteSpace(filterName))
			{
				Globals.Message.ShowWarning(Res.GetString("8DFE250C-9186-4B5E-AA69-D600E650B86D", "Please select a saved Dynamic Filter to load."));
			}
			else
			{
				var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
				findQuery.AddToFilter(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, OrgDynamicFilterKey);
				var stmData = Factory.Load<StmData>(findQuery).FirstOrDefault(x => x.SD_Name.Substring(OrgDynamicFilterKey.Length, x.SD_Name.Length - OrgDynamicFilterKey.Length) == filterName);
				if (stmData != null)
				{
					OrganisationFilterContentControl.DynamicFilterPanel.Controls.RemoveAndDisposeAll();

					var filtersToLoadWithName = JsonConvert.DeserializeObject<Tuple<ZString, IEnumerable<((FilterStyle, ZString), object)>>>(stmData.SD_BinaryValue.ToUTF8(),
												new JsonSerializerSettings
												{
													TypeNameHandling = TypeNameHandling.All
												});
					var filtersToLoad = filtersToLoadWithName.Item2.ToList();

					foreach (var filterToLoad in filtersToLoad)
					{
						var addedFilter = OrganisationFilterContentControl.AddFilterAndResizeControl();
						var source = addedFilter.OrgFilterItemDataSource;
						source.OrgFilterType = filterToLoad.Item1.Item2;

						if (source.FilterStyle == FilterStyle.Input)
						{
							var textFilterDetail = filterToLoad.Item2 as (DynamicFilterConditionType?, ZString?)?;
							source.OrgFilterOption = addedFilter.FilterConditionTypeDictionary.TryGetValue(textFilterDetail?.Item1 ?? DynamicFilterConditionType.Contains, out var description) ? description : OrgFilterOptionList.Descriptions.Contains.GetUnresolvedString();
							source.OrgFilterKeyword = new ZString(textFilterDetail?.Item2);
						}
						else if (source.FilterStyle == FilterStyle.CheckBox)
						{
							var selectedCheckBoxes = filterToLoad.Item2 as List<ZString>;
							foreach (var selectedCheckBox in selectedCheckBoxes)
							{
								var checkBoxToTick = addedFilter.FindSingle<ZCheckBox>(x => x.Tag.ToString() == selectedCheckBox);
								checkBoxToTick.Checked = true;
							}
						}

						if (filtersToLoad.Count == 1)
						{
							addedFilter.RemoveFilterButton.Enabled = false;
						}
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("ED4EBF48-AE56-4D92-AFFF-8F797B7768E1", "The filter '{0}' can't be found.", filterName));
				}
			}
		}

		void DeleteButtonOnClick(object sender, EventArgs e)
		{
			var selectedItem = SavedFilterComboBox.SelectedItem?.ToString();
			if (string.IsNullOrWhiteSpace(selectedItem))
			{
				Globals.Message.ShowWarning(Res.GetString("36E1546F-C542-49E5-A3CD-B49C00E7464B", "Please select a saved Dynamic Filter to delete."));
			}
			else
			{
				var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
				findQuery.AddToFilter(StmDataSchema.SD_Name, OrgDynamicFilterKey + selectedItem);

				var stmData = Factory.LoadTop1<StmData>(findQuery);

				if (stmData != null && Globals.Message.Show(Res.GetString("1CDD4071-D93E-461A-BBE3-C8F3C6B79052", "Are you sure you want to delete the Dynamic Filter '{0}'?", selectedItem),
						Res.GetString("CC7128FF-D867-4104-A20A-EDE2FE8B3C2F", "Delete Dynamic Filter Confirmation"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
				{
					stmData.Delete();
					Factory.Save();

					Globals.Message.ShowInformation(Res.GetString("272AED0E-29CA-4A87-81BA-A9DBB5FAA1AF", "The Dynamic Filter '{0}' has been deleted.", selectedItem));
					SavedFilters.Remove(selectedItem);
					RefreshComboBoxDataSource();
				}
			}
		}

		void SaveButtonOnClick(object sender, EventArgs e)
		{
			var filterName = FilterNameTextBox.Text;
			if (string.IsNullOrWhiteSpace(filterName))
			{
				Globals.Message.ShowWarning(Res.GetString("2A482681-164E-4F41-A7E6-F970F28E0FB1", "Please specify a name."));
			}
			else if (SavedFilters.Contains(filterName))
			{
				Globals.Message.ShowError(Res.GetString("0DCC7E77-A44F-4D21-9E40-F9DA89BF05E0", "The filter can't be saved as '{0}' because another saved filter has already used the same name.", filterName));
			}
			else
			{
				var filterDetails = new List<((FilterStyle, ZString), object)>();
				var filterItemUserControlsToBeSaved = AdvancedFilterCriteriaControl.FindAll<FilterItemUserControl>().ToList();
				foreach (var item in filterItemUserControlsToBeSaved)
				{
					item.AddFilterResultToFilterDetails(filterDetails);
				}

				var filterValue = new Tuple<ZString, IEnumerable<((FilterStyle, ZString), object)>>(filterName, filterDetails);
				SaveToDB(filterValue);
				SavedFilters.Add(filterName);
				SavedFilters.Sort();
				RefreshComboBoxDataSource();
				Globals.Message.ShowInformation(Res.GetString("525BE091-0DF8-465F-8C09-52E0C4E47617", "The filter is saved as '{0}' successfully.", filterName));
			}
		}

		void FindButtonOnClick(object sender, EventArgs e)
		{
			if (DuplicationResultDetail != null)
			{
				DuplicationResultDetail.FilterAndUpdate();
			}
		}

		#region implementation

		void RefreshComboBoxDataSource()
		{
			SavedFilterComboBox.DataSource = null;
			SavedFilterComboBox.DataSource = SavedFilters;
			SavedFilterComboBox.SelectedIndex = -1;
		}

		void SaveToDB(Tuple<ZString, IEnumerable<((FilterStyle, ZString), object)>> filtersValue)
		{
			var stmData = Factory.New<StmData>();
			stmData.SD_Owner = Env.CurrentUser.PK;
			stmData.SD_Name = OrgDynamicFilterKey + filtersValue.Item1;
			stmData.SD_Type = RegistryDataTypes.Codes.Binary;
			stmData.SD_BinaryValue = SerializeDynamicFiltersValue(filtersValue);
			Factory.Save();
		}

		ZBlob SerializeDynamicFiltersValue(Tuple<ZString, IEnumerable<((FilterStyle, ZString), object)>> filtersValue)
		{
			var jsonStr = JsonConvert.SerializeObject(filtersValue, Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});

			return ZBlob.FromUTF8(jsonStr);
		}

		internal void SetupDataContext(DeduplicationOrganisationResultDetail duplicationResultDetail, PotentialDuplicatesUserControl potentialDuplicatesUserControl, AdvancedFilterCriteriaControl advancedFilterCriteriaControl)
		{
			DuplicationResultDetail = duplicationResultDetail;
			PotentialDuplicatesUserControl = potentialDuplicatesUserControl;
			AdvancedFilterCriteriaControl = advancedFilterCriteriaControl;
			DuplicationResultDetail.GetDynamicFilterItemUserControls = GetFilterItemUserControls;
			PerformLastFilter();
		}

		void PerformLastFilter()
		{
			if (DuplicationResultDetail?.CandidatesCount > 0 && DuplicationResultDetail?.SelectedCandidatePK == DuplicationResultDetail?.DuplicationCandidates[0].TargetPK)
			{
				FindButtonOnClick(null, null);
			}
		}

		#endregion
	}
}
