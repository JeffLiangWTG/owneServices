using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module
{
	public partial class OrgClientAssignedStaffFilterStrip : ZUserControl
	{
		public OrgClientAssignedStaffFilterStrip()
		{
			InitializeComponent();
		}

		public OrgClientAssignedStaffFilterStrip(bool enableComparisonOperatorControls)
		{
			EnableComparisonOperatorControls = enableComparisonOperatorControls;
			InitializeComponent();
		}

		bool EnableComparisonOperatorControls { get; set; }

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (EnableComparisonOperatorControls)
			{
				AssignedStaffOperatorDropEdit.Visible = true;

				SuspendLayout();
				try
				{
					AddControlsForSelectedFilters();
				}
				finally
				{
					ResumeLayout(false);
				}
			}
		}

		void AddControlsForSelectedFilters()
		{
			AssignedStaffFilterCollectionFindBox = CreateFilterCollectionFindBox();
			Controls.Add(AssignedStaffFilterCollectionFindBox);
		}

		ZFilterCollectionFindBox CreateFilterCollectionFindBox()
		{
			var codeFindBox = AssignedStaffCodeFindBox;
			var moduleFilter = (IModuleFilterWithSelectedFilters)CurrentDataItem;
			var filterFindBox = new ZFilterCollectionFindBox(moduleFilter, isPopupButtonEnabledWhenReadOnly: true);

			filterFindBox.ShowDescriptionBox = true;
			ControlDpiScalingHelper.SetTop(ref filterFindBox, codeFindBox.Top, false);
			ControlDpiScalingHelper.SetLeft(ref filterFindBox, codeFindBox.Left, false);
			ControlDpiScalingHelper.SetWidth(ref filterFindBox, codeFindBox.Width, false);
			filterFindBox.BindTo = nameof(moduleFilter.SelectedFilters);
			filterFindBox.BindToList = "List";
			filterFindBox.BindToForDescription = nameof(moduleFilter.SelectedFiltersDescription);
			filterFindBox.Name = "AssignedStaffFilterCollectionFindBox";
			BindingSource.SetBindingMember(filterFindBox, nameof(moduleFilter.SelectedFilters));

			void UpdateSelectedFilterDescription()
			{
				filterFindBox.DescriptionBox.Text = moduleFilter.SelectedFiltersDescription;
			}

			void OperatorChangedHandler(object s, EventArgs e)
			{
				SetAssignedStaffFindBoxVisibilities(new Control[] { codeFindBox }, filterFindBox, moduleFilter);
				UpdateSelectedFilterDescription();
			}

			moduleFilter.ComparisonOperatorChanged += OperatorChangedHandler;
			filterFindBox.Disposed += (s, e) => moduleFilter.ComparisonOperatorChanged -= OperatorChangedHandler;

			void SelectedFiltersChangedHandler(object s, EventArgs e)
			{
				using (filterFindBox.SuspendSelectedFiltersChangedFiring())
				{
					UpdateSelectedFilterDescription();
				}
			}

			moduleFilter.SelectedFiltersChanged += SelectedFiltersChangedHandler;
			filterFindBox.Disposed += (s, e) => moduleFilter.SelectedFiltersChanged -= SelectedFiltersChangedHandler;

			SetAssignedStaffFindBoxVisibilities(new Control[] { codeFindBox }, filterFindBox, moduleFilter);

			return filterFindBox;
		}

		static void SetAssignedStaffFindBoxVisibilities(IEnumerable<Control> nonFiltersMatchControls, Control filterFindBox, IModuleFilterWithSelectedFilters moduleFilter)
		{
			var useFilterMatchMode = moduleFilter.IsFilterCollectionComparisonOperatorSelected();
			filterFindBox.Visible = useFilterMatchMode;

			foreach (var control in nonFiltersMatchControls.WhereNotNull())
			{
				control.Visible = !useFilterMatchMode;
			}
		}
	}
}
