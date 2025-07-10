using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class MultiSelectModuleForm : EmbeddedModulePopup, IEmbeddedModulePopupOKButtonStrategy
	{
		#region Constructor

		public MultiSelectModuleForm(IMultiSelectHandler parent)
			: base((ZFilterModule)ZModuleFactory.Instance.Create(parent.FilterModuleId))
		{
			this.parent = parent;
			Argument.NotNull(parent, "parent");
			InitializeComponent();

			EmbeddedModulePopupOKButtonStrategy = this;

			SetupModule1();
			SetupModule2();
			AdjustFormSize();
		}

		void SetupModule1()
		{
			module1 = Module;
			var filterControl = ((IFilterControl)module1.EmbeddedControl);
			module1.AddAdditionalDisplayFilter = AddModule1AdditionalFilterCachingCurrentFilter;

			grid1 = filterControl.FilteredGrid;
			grid1.MouseDoubleClick += AddToSelectionButton_Click;
			collection1 = (IActiveBusinessObjectCollection)module1.GridCollection;
			pkColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(BusinessObjectFactory.GetTableNameFromType(collection1.TypeOfElements));

			FilterControlPanel.Controls.Remove(module1.EmbeddedControl);
			SplitContainer.Panel1.Controls.Add(module1.EmbeddedControl);
		}

		void SetupModule2()
		{
			module2 = (ZFilterModule)ZModuleFactory.Instance.Create(parent.FilterModuleId);
			grid2 = ((IFilterControl)module2.EmbeddedControl).FilteredGrid;
			grid2.Dock = DockStyle.Fill;
			grid2.MouseDoubleClick += RemoveFromSelectionButton_Click;
			collection2 = (IActiveBusinessObjectCollection)module2.GridCollection;

			SelectedGroupBox.Controls.Add(grid2);
			SelectedGroupBox.Text = "Selected " + module2.Description;
		}

		void AdjustFormSize()
		{
			var clientWidthDifference = Size.Width - ClientSize.Width;
			var clientHeightDifference = Size.Height - ClientSize.Height;

			var width = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(module1.EmbeddedControl.Width + clientWidthDifference);
			var height = ControlDpiScalingHelper.UnscaleFromCurrentDpiY((module1.EmbeddedControl.Height + module2.EmbeddedControl.Height) + clientHeightDifference);
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(width, height);
			if (ClientSize.Width < width || (ClientSize.Height - ButtonPanel.Height) < height)
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(Module.EmbeddedControl.Size.Width, height + ButtonPanel.Height);
			}
		}

		#endregion

		#region Dispose

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (module1 != null)
				{
					module1.Dispose();
				}

				if (module2 != null)
				{
					module2.Dispose();
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Implementation

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			collection2.AdditionalFilter = ZQuery.NoResultQuery;
			grid2.SetDataBinding(collection2, string.Empty);
		}

		protected override BusinessObject[] GetSelectedBusinessObjects()
		{
			return grid2.ListManager != null ? grid2.ListManager.List.Cast<BusinessObject>().ToArray() : null;
		}

		void IEmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(BusinessObject[] selectedObjects)
		{
			parent.HandleSelectedObjects(selectedObjects);
			Close();
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		void AddToSelectionButton_Click(object sender, EventArgs e)
		{
			if (grid1.SelectedRowCount > 0)
			{
				pks.AddRange(grid1.SelectedElements.Select(b => b.PK));
				RefreshCollections();
			}
			else
			{
				Globals.Message.ShowInformation("Please select rows from the first grid to add.", "Add To Selection");
			}
		}

		void RemoveFromSelectionButton_Click(object sender, EventArgs e)
		{
			if (grid2.SelectedRowCount > 0)
			{
				foreach (var pk in grid2.SelectedElements.Select(b => b.PK))
				{
					pks.Remove(pk);
				}
				RefreshCollections();
			}
			else
			{
				Globals.Message.ShowInformation("Please select rows from the second grid to rollback.", "Rollback");
			}
		}

		void RefreshCollections()
		{
			collection1.AdditionalFilter = AddModule1AdditionalFilter(new ZQuery(mainFilterCached));
			collection2.AdditionalFilter = new ZQuery(pkColumn, pks);
		}

		void AddModule1AdditionalFilterCachingCurrentFilter(ZQuery query)
		{
			mainFilterCached = query.DeepClone();
			AddModule1AdditionalFilter(query);
		}

		ZQuery AddModule1AdditionalFilter(ZQuery query)
		{
			query.AddToFilter(parent.AdditionalFilter);
			query.AddToFilter(pkColumn, SQLComparisonOperator.NotEqual, pks);
			return query;
		}

		ZFilterModule module1;
		ZFilterModule module2;
		ZFilterGrid grid1;
		ZFilterGrid grid2;
		IActiveBusinessObjectCollection collection1;
		IActiveBusinessObjectCollection collection2;
		readonly IMultiSelectHandler parent;
		readonly List<ZGuid> pks = new List<ZGuid>();
		SchemaPKColumn pkColumn;
		ZQuery mainFilterCached;

		#endregion
	}
}
