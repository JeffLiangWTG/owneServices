using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
	public partial class RefCusTariffUserControl : ZUserControl
	{
		public RefCusTariffUserControl()
		{
			InitializeComponent();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = nameof(ZZ1_EndDateDateEdit) + " scaling is already done")]
		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (DataSource is TariffView tariffView)
			{
				var rateFormulaBoxColumnInfo = ((RateFormulaBoxColumnStyleInfo)RatesGrid.GetColumnStyle(RateView.Schema.ZZ2_RateFormula));
				rateFormulaBoxColumnInfo.UnitListProvider = tariffView;

				var dateTimeFormat = tariffView.DateTimeFormat;

				ZZ1_EndDateDateEdit.DateTimeFormat = dateTimeFormat;
				ZZ1_StartDateDateEdit.DateTimeFormat = dateTimeFormat;
				ZZ1_PublishedDateEdit.DateTimeFormat = dateTimeFormat;
				ZZ1_EndDateDateEdit.Location = new System.Drawing.Point(ZZ1_AlternateLanguageDescriptionTextBox.Location.X + ZZ1_AlternateLanguageDescriptionTextBox.Width - ZZ1_EndDateDateEdit.Width, ZZ1_EndDateDateEdit.Location.Y);

				((ZArchitecture.ZDateEditColumnStyleInfo)RatesGrid.GetColumnStyle(RateView.Schema.ZZ2_StartDate)).DateTimeFormat = dateTimeFormat;
				var isParentDataGrouping = tariffView.IsParentDataGrouping;
				EffectiveDataGroupingDropEdit.Visible = isParentDataGrouping;
				RatesGrid.ColumnLayoutContext = RatesGrid.ColumnLayoutContext + isParentDataGrouping;
				UnitsOfMeasureGrid.ColumnLayoutContext = UnitsOfMeasureGrid.ColumnLayoutContext + isParentDataGrouping;
				RateApplicabilitiesGrid.ColumnLayoutContext = RateApplicabilitiesGrid.ColumnLayoutContext + isParentDataGrouping;
				VatApplicabilitiesGrid.ColumnLayoutContext = VatApplicabilitiesGrid.ColumnLayoutContext + isParentDataGrouping;
				AdditionalCodesGrid.ColumnLayoutContext = AdditionalCodesGrid.ColumnLayoutContext + isParentDataGrouping;
				AdditionalCodesApplicabilitiesGrid.ColumnLayoutContext = AdditionalCodesApplicabilitiesGrid.ColumnLayoutContext + isParentDataGrouping;
				AttributesGrid.SetAvailability(tariffView.AdditionalAttributeInformationProvider.AdditionalDescriptionVisible, TariffAttributeView.Schema.AdditionalDescription);
				if (!isParentDataGrouping)
				{
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_ZZZ_NKDataGrouping);
					RateApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping);
					VatApplicabilitiesGrid.SetAvailability(false, VATApplicabilityView.Schema.ZX5_ZZZ_NKDataGrouping);
					AdditionalCodesGrid.SetAvailability(false, TariffAdditionalCodeView.Schema.ZY2_ZZZ_NKDataGrouping);
					AdditionalCodesApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping);
				}

				var nomenclatureGroups = tariffView.Wrapper.NomenclatureGroups;
				if (nomenclatureGroups.Count > 0)
				{
					var defaultLanguageNomenclatureTreeViewBuilder = new NomenclatureTreeViewBuilder(tariffView.Wrapper, nomenclatureGroups, DefaultLanguageNomenclatureTreeView,
						(x) => x.ZZ5_Description,
						(wrapper, group) => { wrapper.SelectedNomenclatureDescription = group.ZZ5_Description; });
					defaultLanguageNomenclatureTreeViewBuilder.Build();

					var alternateLanguageNomenclatureTreeViewBuilder = new NomenclatureTreeViewBuilder(tariffView.Wrapper, nomenclatureGroups, AlternateLanguageNomenclatureTreeView,
						(x) => x.ZZ5_AlternateLanguageDescription,
						(wrapper, group) => { wrapper.SelectedNomenclatureAlternateLanguageDescription = group.ZZ5_AlternateLanguageDescription; });
					alternateLanguageNomenclatureTreeViewBuilder.Build();
				}
				else
				{
					NomenclatureGroupBox.Visible = false;
				}

				var isSystem = tariffView.ZZ1_IsSystem;
				if (!isSystem)
				{
					NomenclatureGroupBox.Visible = false;
					AttributesGroupBox.Visible = false;
					RatesApplyToCountryFindBox.Visible = false;
					TariffDataTabControl.TabPages.Remove(ParentTariffTabPage);
					TariffDataTabControl.TabPages.Remove(ChildTariffsTabPage);
					TariffDataTabControl.TabPages.Remove(AdditionalCodesTabPage);
					TariffDataTabControl.TabPages.Remove(ConditionsTabPage);
					ZZ1_ZZI_NKTariffTypeDropEdit.ReadOnly = true;
					ZZ1_ZZI_NKTariffTypeDropEdit.Visible = true;
					ZZ1_ZZI_TariffTypeGuidDropEdit.Visible = false;

					if (isParentDataGrouping)
					{
						RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_ZZZ_NKDataGrouping);
					}
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_EndDate_ForDisplay);
					RatesGrid.SetAvailability(false, RateView.Schema.PreferenceCode);
					RatesGrid.SetAvailability(false, RateView.Schema.RateCode);
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_RX_NKCurrencyOverride);
					((ZArchitecture.ZDateEditColumnStyleInfo)RatesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate)).DateTimeFormat = dateTimeFormat;
					RateApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_AdditionalCode);
					RateApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ExcludedTradeGroupsConcatenated);
					AdditionalCodesApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_AdditionalCode);
					AdditionalCodesApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ExcludedTradeGroupsConcatenated);
				}
				else
				{
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_EndDate);
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_ZZS_Preference);
					RatesGrid.SetAvailability(false, RateView.Schema.ZZ2_ZY1_RateCode);
					((ZArchitecture.ZDateEditColumnStyleInfo)RatesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate_ForDisplay)).DateTimeFormat = dateTimeFormat;
					RateApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_ZZA_TradeGroup);
					RateApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_ZZA_SecondTradeGroup);
					AdditionalCodesApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_ZZA_TradeGroup);
					AdditionalCodesApplicabilitiesGrid.SetAvailability(false, CusRefApplicabilityView.Schema.ZZT_ZZA_SecondTradeGroup);
				}

				if (Parent is RefCusTariffForm parent)
				{
					VatApplicabilitiesGrid.Visible = parent.DisplayMode != ODisplayMode.New && parent.DisplayMode != ODisplayMode.Browse;
				}

				ZZ1_CRT_NKTariffVersionDropEdit.Visible = !isSystem;
			}
		}
	}

	class NomenclatureTreeViewBuilder
	{
		public NomenclatureTreeViewBuilder(TariffViewWrapper wrapper, RefCusNomenclatureGroupCollection nomenclatureGroups, ZTreeView treeView, Func<RefCusNomenclatureGroup, string> getDescription, Action<TariffViewWrapper, RefCusNomenclatureGroup> setSelectedNomenclatureDescription)
		{
			this.nomenclatureGroups = nomenclatureGroups;
			this.getDescription = getDescription;
			this.treeView = treeView;
			this.wrapper = wrapper;
			this.setSelectedNomenclatureDescription = setSelectedNomenclatureDescription;
		}
		readonly RefCusNomenclatureGroupCollection nomenclatureGroups;
		readonly ZTreeView treeView;
		readonly TariffViewWrapper wrapper;
		readonly Func<RefCusNomenclatureGroup, string> getDescription;
		readonly Action<TariffViewWrapper, RefCusNomenclatureGroup> setSelectedNomenclatureDescription;
		int nomenclatureTreeViewWidth;

		public void Build()
		{
			treeView.AfterSelect -= NomenclatureTreeView_AfterSelect;
			treeView.AfterSelect += NomenclatureTreeView_AfterSelect;
			treeView.SizeChanged -= NomenclatureTreeView_SizeChanged;
			treeView.SizeChanged += NomenclatureTreeView_SizeChanged;
			treeView.Nodes.Clear();
			var nodes = treeView.Nodes;
			TreeNode parentNode = null;
			nomenclatureTreeViewWidth = treeView.Width;
			foreach (var nomenclatureGroup in nomenclatureGroups.OrderBy(x => x.ZZ5_CompositeKey))
			{
				parentNode = nodes.Add(TruncateNodeText(nomenclatureGroup, parentNode));
				parentNode.Tag = nomenclatureGroup;
				nodes = parentNode.Nodes;
			}
			if (parentNode != null)
			{
				treeView.SelectedNode = parentNode;
			}
			treeView.ExpandAll();
		}

		void NomenclatureTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (wrapper != null)
			{
				setSelectedNomenclatureDescription?.Invoke(wrapper, (RefCusNomenclatureGroup)e.Node.Tag);
			}
		}

		void NomenclatureTreeView_SizeChanged(object sender, EventArgs e)
		{
			var treeViewWidth = treeView.Width;
			if (nomenclatureTreeViewWidth > 0 && treeView.Visible && nomenclatureTreeViewWidth != treeViewWidth && treeViewWidth > 0)
			{
				nomenclatureTreeViewWidth = treeView.Width;
				UpdateNodeText(treeView.Nodes, null);
			}
		}

		void UpdateNodeText(TreeNodeCollection nodes, TreeNode parentNode)
		{
			foreach (TreeNode node in nodes)
			{
				var nomenclatureGroup = (RefCusNomenclatureGroup)node.Tag;
				node.Text = TruncateNodeText(nomenclatureGroup, parentNode);
				UpdateNodeText(node.Nodes, node);
			}
		}

		string TruncateNodeText(RefCusNomenclatureGroup nomenclatureGroup, TreeNode parentNode)
		{
			string fullText = FormattableString.Invariant($"{nomenclatureGroup.ZZ5_Value} {getDescription(nomenclatureGroup)}");
			return fullText.TruncateToFit(treeView.Font, nomenclatureTreeViewWidth - ((parentNode?.Level ?? -1) + 2) * ControlDpiScalingHelper.ScaleToCurrentDpiX(26) - ControlDpiScalingHelper.ScaleToCurrentDpiX(4));
		}
	}
}
