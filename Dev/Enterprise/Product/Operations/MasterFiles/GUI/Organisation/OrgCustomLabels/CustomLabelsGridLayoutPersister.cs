using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public sealed class CustomLabelsGridLayoutPersister : Component
	{
		public CustomLabelsGridLayoutPersister(ZGrid grid, ICustomLabelsProvider customLabelsProvider)
			: this(grid, customLabelsProvider, "", "")
		{
		}

		public CustomLabelsGridLayoutPersister(ZGrid grid, ICustomLabelsProvider customLabelsProvider, string columnMappingPrefix)
			: this(grid, customLabelsProvider, columnMappingPrefix, "")
		{
		}

		/// <param name="columnMappingPrefix">Allows a column to be bound to a property in another object</param>
		/// <param name="customLabelPrefixToMatch">Allows additional filtering on configured labels. eg. Labels configured for ComInvoiceLine in organisation does not affect ContainerGrid.LayoutCategoryPK.</param>
		public CustomLabelsGridLayoutPersister(ZGrid grid, ICustomLabelsProvider customLabelsProvider, string columnMappingPrefix, string customLabelPrefixToMatch)
		{
			this.customLabelsProvider = customLabelsProvider;
			this.columnMappingPrefix = columnMappingPrefix;
			this.customLabelPrefixToMatch = customLabelPrefixToMatch;
			this.grid = grid;
			this.grid.QueryHasCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(HasCustomisedColumns);
			this.grid.QueryHasNoOtherCustomisedColumns = new ZGrid.QueryHasCustomisedColumnsEventHandler(HasNoOtherCustomisedColumns);

			parentFormWeakReference = new WeakReference(grid.FindForm());

			customLabelsProvider.ConfigOrgProvider.ConfigOrgChanged += ConfigOrg_Changed;

			Form parentForm = parentFormFromWeakReference;
			if (parentForm != null)
			{
				parentForm.Closing += Form_Closing;
			}

			originalGridLayout = new ArrayList(grid.ColumnStyles);
			ConfigOrg_Changed(this, EventArgs.Empty);

#if DEBUG
			DisposableLeakListener.Instance.RegisterDisposable(this);
#endif
		}
		readonly string columnMappingPrefix;
		readonly string customLabelPrefixToMatch;
		readonly ICustomLabelsProvider customLabelsProvider;
		readonly ZGrid grid;
		readonly ArrayList originalGridLayout;
		readonly WeakReference parentFormWeakReference;

		void LoadGridLayout()
		{
			if (grid.DataSource != null)
			{
				Guid previousLayoutCategoryPK = grid.LayoutCategoryPK;

				grid.LayoutCategoryPK = GetLayoutCategoryPK();

				if (previousLayoutCategoryPK != grid.LayoutCategoryPK)
				{
					grid.LoadUserLayoutSettings();
				}
			}

			ConvertLayoutsToGlobalIfNoCustomColumns();
		}

		void SaveGridLayout()
		{
			if (grid.DataSource != null)
			{
				Guid previousLayoutCategoryPK = grid.LayoutCategoryPK;

				grid.LayoutCategoryPK = GetLayoutCategoryPK();

				if (previousLayoutCategoryPK != grid.LayoutCategoryPK)
				{
					grid.SaveUserLayoutSettings();
				}
			}
		}

		Guid GetLayoutCategoryPK()
		{
			Guid result = Guid.Empty;

			if (currentConfigOrgForGridLayout != null && ContainsCustomFieldGridColumns(currentConfigOrgForGridLayout))
			{
				result = currentConfigOrgForGridLayout.PK.ToGuid();
			}

			return result;
		}

		bool ContainsCustomFieldGridColumns(OrgHeader organisation)
		{
			bool result = false;

			if (organisation != null)
			{
				if (string.IsNullOrEmpty(customLabelPrefixToMatch))
				{
					result = true;
				}
				else
				{
					foreach (OrgCustomLabels customLabel in organisation.CustomFormLabels)
					{
						if (customLabel.OT_FieldName.StartsWith(customLabelPrefixToMatch))
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}

#if DEBUG
		internal
#endif
		void RefreshGridLayout()
		{
			var newConfigOrg = customLabelsProvider.ConfigOrgProvider.ConfigOrg;
			if (newConfigOrg != null)
			{
				newConfigOrg = RenderNewConfigOrg(newConfigOrg);
			}

			if (currentConfigOrgForGridLayout?.PK != newConfigOrg?.PK)
			{
				SaveGridLayout();

				if (ContainsCustomFieldGridColumns(currentConfigOrgForGridLayout))
				{
					var fields = customLabelsProvider.GetCustomFields(currentConfigOrgForGridLayout, customLabelsProvider.ConfigOrgProvider.Factory);
					new CustomLabelControlFactory(fields).ResetToOriginalCustomColumns(grid, columnMappingPrefix, originalGridLayout);
				}

				if (ContainsCustomFieldGridColumns(newConfigOrg))
				{
					var fields = customLabelsProvider.GetCustomFields(newConfigOrg, customLabelsProvider.ConfigOrgProvider.Factory);
					new CustomLabelControlFactory(fields).AddOrRefreshCustomFieldGridColumns(grid, columnMappingPrefix);
				}

				currentConfigOrgForGridLayout = newConfigOrg;
			}

			LoadGridLayout();
		}

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "It's okay, just checking to see if it's empty.")]
		OrgHeader RenderNewConfigOrg(OrgHeader currentConfigOrg)
		{
			var newFactory = customLabelsProvider.ConfigOrgProvider.Factory.CreateNewFactory();
			newFactory.NameForDebugging = "CustomLabelsGridLayoutPersister_RenderNewConfigOrg";
			var proxyOrgInNewFactory = newFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var currentConfigContainPartAttribute = false;
			OrgHeader newConfigOrg = null;

			#region PartAttributes

			if (currentConfigOrg.MiscServ.OM_IMPartAttrib1Name.IsEmpty && currentConfigOrg.MiscServ.OM_IMPartAttrib2Name.IsEmpty && currentConfigOrg.MiscServ.OM_IMPartAttrib3Name.IsEmpty)
			{
				if (proxyOrgInNewFactory != null)
				{
					if (!proxyOrgInNewFactory.MiscServ.OM_IMPartAttrib1Name.IsEmpty || !proxyOrgInNewFactory.MiscServ.OM_IMPartAttrib2Name.IsEmpty || !proxyOrgInNewFactory.MiscServ.OM_IMPartAttrib3Name.IsEmpty)
					{
						newConfigOrg = proxyOrgInNewFactory;
					}
				}
			}
			else
			{
				currentConfigContainPartAttribute = true;
			}

			#endregion

			#region CustomLabels

			if (currentConfigOrg.CustomLabels.Count == 0)
			{
				if (proxyOrgInNewFactory?.CustomLabels.Count > 0)
				{
					if (newConfigOrg == null && !currentConfigContainPartAttribute)
					{
						newConfigOrg = proxyOrgInNewFactory;
					}
					else if (currentConfigContainPartAttribute)
					{
						newConfigOrg = newFactory.Load<OrgHeader>(currentConfigOrg.PK);
					}

					if (newConfigOrg.PK == currentConfigOrg.PK)
					{
						newConfigOrg.CustomLabels.RemoveAll();

						foreach (OrgCustomLabels customLabel in proxyOrgInNewFactory.CustomLabels)
						{
							customLabel.OT_OH = newConfigOrg.PK;
							newConfigOrg.CustomLabels.Add(customLabel);
						}
					}
				}
				else
				{
					if (currentConfigContainPartAttribute)
					{
						newConfigOrg = newFactory.Load<OrgHeader>(currentConfigOrg.PK);
					}
				}
			}
			else if (newConfigOrg == null)
			{
				newConfigOrg = currentConfigOrg;
			}
			else if (newConfigOrg.PK == proxyOrgInNewFactory?.PK)
			{
				newConfigOrg.CustomLabels.RemoveAll();

				var currentConfigOrgInNewFactory = newFactory.Load<OrgHeader>(currentConfigOrg.PK);

				foreach (OrgCustomLabels customLabel in currentConfigOrgInNewFactory.CustomLabels)
				{
					customLabel.OT_OH = newConfigOrg.PK;
					newConfigOrg.CustomLabels.Add(customLabel);
				}
			}

			#endregion

			return newConfigOrg;
		}

		void ConvertLayoutsToGlobalIfNoCustomColumns()
		{
			if (!string.IsNullOrEmpty(grid.GridId))
			{
				var orgPK = currentConfigOrgForGridLayout?.PK ?? customLabelsProvider.ConfigOrgProvider.ConfigOrg?.PK;
				if (orgPK == null || orgPK == ZGuid.Empty)
				{
					return;
				}
				var newKey = new DataGridLayoutContextKeyProvider(grid, false).ContextKeyForStmModuleFilter;
				var oldKey = newKey + "|" + orgPK.ToString();
				var factory = customLabelsProvider.ConfigOrgProvider.Factory.CreateNewFactory();

				var layouts = new ZArchitecture.Business.StmModuleFilter.Loader(factory).FindByID(oldKey);

				if (layouts.Any())
				{
					foreach (var layout in layouts)
					{
						var visibleColumns = ZColumnsCustomise.GetVisibleColumnNames(factory, layout).ToArray();

						if (grid.QueryHasCustomisedColumns != null && !grid.QueryHasCustomisedColumns.Invoke(visibleColumns))
						{
							layout.S9_ModuleID = newKey;
						}
					}

					factory.Save();
				}
			}
		}

#if DEBUG
		internal OrgHeader CurrentConfigOrgForGridLayout
		{
			get { return currentConfigOrgForGridLayout; }
		}
#endif
		OrgHeader currentConfigOrgForGridLayout;

		public void ConfigOrgChanged()
		{
			ConfigOrg_Changed(null, EventArgs.Empty);
		}

		void ConfigOrg_Changed(object sender, EventArgs e)
		{
			if (currentConfigOrgForListChanged != null)
			{
				((IBindingList)currentConfigOrgForListChanged.CustomLabels).ListChanged -= CustomLabels_ListChanged;
			}
			OrgHeader newConfigOrg = customLabelsProvider.ConfigOrgProvider.ConfigOrg;
			if (grid.Columns.Count > 0 && grid.DataSource != null)
			{
				RefreshGridLayout();
			}
			if (newConfigOrg != null)
			{
				((IBindingList)newConfigOrg.CustomLabels).ListChanged += CustomLabels_ListChanged;
			}
			currentConfigOrgForListChanged = newConfigOrg;
		}
		OrgHeader currentConfigOrgForListChanged;

		void CustomLabels_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshGridLayout();
		}

		void Form_Closing(object sender, CancelEventArgs e)
		{
			SaveGridLayout();
		}

		Form parentFormFromWeakReference
		{
			get { return (Form)parentFormWeakReference.Target; }
		}

		#region IDisposable
		protected override void Dispose(bool isNotFinalising)
		{
			if (isNotFinalising)
			{
				if (grid != null)
				{
					Form parentForm = this.parentFormFromWeakReference;
					if (parentForm != null)
					{
						parentForm.Closing -= Form_Closing;
					}
				}

				if (currentConfigOrgForListChanged != null)
				{
					((IBindingList)currentConfigOrgForListChanged.CustomLabels).ListChanged -= CustomLabels_ListChanged;
					currentConfigOrgForListChanged = null;
				}

				customLabelsProvider.ConfigOrgProvider.ConfigOrgChanged -= ConfigOrg_Changed;

#if DEBUG
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
#endif
			}
		}
		#endregion

		#region QueryHasCustomisedColumns Members

		bool HasCustomisedColumns(string[] visibleColumnsToSave)
		{
			CustomLabelInfoList fields = customLabelsProvider.GetCustomFields(currentConfigOrgForGridLayout, customLabelsProvider.ConfigOrgProvider.Factory);
			List<string> customisedAttributes = new List<string>(new CustomLabelControlFactory(fields).GetCustomisedMappingNames(columnMappingPrefix, false));

			foreach (string columnName in visibleColumnsToSave)
			{
				if (customisedAttributes.Contains(columnName))
				{
					return true;
				}
			}

			return false;
		}

		bool HasNoOtherCustomisedColumns(string[] visibleColumnsToSave)
		{
			CustomLabelInfoList fields = customLabelsProvider.GetCustomFields(currentConfigOrgForGridLayout, customLabelsProvider.ConfigOrgProvider.Factory);
			List<string> customisedAttributesNotInGrid = new List<string>(new CustomLabelControlFactory(fields).GetCustomisedMappingNames(columnMappingPrefix, true));

			foreach (string columnName in visibleColumnsToSave)
			{
				if (customisedAttributesNotInGrid.Contains(columnName))
				{
					return false;
				}
			}

			return true;
		}

		#endregion
	}
}
