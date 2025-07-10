using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RequiredDocumentsUserControl : ZUserControl
	{
		public RequiredDocumentsUserControl()
		{
			InitializeComponent();
			HideCountryDependantControls();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			var disHost = GetDISHostFromDataSource();
			if (disHost != null)
			{
				this.DISDataButton.CaptionResourceString = Res.GetData("B044C836-F99F-4D0B-A3A8-BB394A2EF3BD", "View/Edit {0} Data").Format(disHost.HumanReadable);
			}
		}

		void HideCountryDependantControls()
		{
			if (!DesignModeFinder.IsDesigning && GlbCompany.CurrentCompany?.GC_RN_NKCountryCode.ToString() != Core.Constants.CountryCodes.China)
			{
				ReturnedToShipperDateEdit.Visible = false;
				RcvdFromBrokerDateEdit.Visible = false;
				SentToBrokerDateEdit.Visible = false;

				DocumentsGrid.RemoveFromAvailableColumns(JobRequiredDocument.Schema.EQ_SntToCustomsBroker);
				DocumentsGrid.RemoveFromAvailableColumns(JobRequiredDocument.Schema.EQ_RcvFromCustomsBroker);
				DocumentsGrid.RemoveFromAvailableColumns(JobRequiredDocument.Schema.EQ_ReturnToShipper);
			}
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			IBusiness dataSourceForBinding = null;
			var dataMemberForBinding = "";
			var assignedDataSourceForBinding = true;
			var hasAttributes = dataSource is IHaveRequiredDocumentsWithAttributes;

			if (dataSource != null)
			{
				// if dataSource passed in is a collection, use it and ignore dataMember
				var collection = dataSource as IBusinessObjectCollection;
				if (collection == null)
				{
					// if dataSource and dataMember passed in lead to a collection, use it
					var dataSourcePotentialList = dataSource;
					var currencyManager = BindingContext[dataSource, dataMember] as CurrencyManager;
					if (currencyManager != null)
					{
						dataSourcePotentialList = currencyManager.List;
					}

					collection = dataSourcePotentialList as IBusinessObjectCollection;

					if (collection == null)
					{
						// if no collection could be found, use the dataSource and figure out the documents
						var storageMain = dataSource as IStorageMain;
						if (storageMain == null)
						{
							dataSourceForBinding = GetIHaveRequiredDocuments(dataSource as IBusiness, false);
						}
						else
						{
							IBusiness bizObj = storageMain.DocumentOwner;
							if (bizObj == null && FindForm() is ZForm zf)
							{
								bizObj = zf.BusinessEntity;
							}

							dataSourceForBinding = GetIHaveRequiredDocuments(bizObj, true);
						}
						hasAttributes |= dataSourceForBinding is IHaveRequiredDocumentsWithAttributes;
					}
					else
					{
						if (typeof(IHaveRequiredDocuments).IsAssignableFrom(collection.TypeOfElements))
						{
							dataSourceForBinding = dataSource as IBusiness;
							dataMemberForBinding = dataMember;
						}
						else
						{
							assignedDataSourceForBinding = false;
						}
					}
				}
				else
				{
					if (typeof(IHaveRequiredDocuments).IsAssignableFrom(collection.TypeOfElements))
					{
						dataSourceForBinding = dataSource as IBusiness;
					}
					else
					{
						assignedDataSourceForBinding = false;
					}
				}

				if (!assignedDataSourceForBinding)
				{
					ErrorReporter.ReportOnce(
						collection.GetType().FullName + " elements (" + collection.TypeOfElements.FullName + ") DataSourceMustImplementInterface ",
						"The data source for this control must implement " + typeof(IHaveRequiredDocuments).FullName);
				}
			}
			AttributesGroupBox.Visible = hasAttributes;
			base.SetDataBinding(dataSourceForBinding, dataMemberForBinding);

			var disHost = GetDISHostFromDataSource();

			UpdateDISFeatureVisibility();

			if (disHost != null)
			{
				disHost.DISFeatureVisibilityChanged += disHost_DISFeatureVisibilityChanged;
			}
		}

		void disHost_DISFeatureVisibilityChanged(object sender, EventArgs e)
		{
			UpdateDISFeatureVisibility();
		}

		void UpdateDISFeatureVisibility()
		{
			var disHost = GetDISHostFromDataSource();
			this.DISDataButton.Visible = disHost != null && disHost.ShowDISFeatures;
		}

		IBusiness GetIHaveRequiredDocuments(IBusiness businessObject, bool parentIsStorageMain)
		{
			var requiredDocumentsParent = businessObject as IHaveRequiredDocuments;

			if (requiredDocumentsParent == null)
			{
				var docsAndCartageParent = businessObject as IDocsAndCartageParent;

				if (docsAndCartageParent == null)
				{
					if (!parentIsStorageMain)
					{
						ErrorReporter.ReportOnce(
							GetType().FullName + "DataSourceMustImplementInterface",
							"The top-level data source for this control must implement " + typeof(IHaveRequiredDocuments).FullName +
							" or " + typeof(IDocsAndCartageParent).FullName);
					}
				}
				else
				{
					requiredDocumentsParent = docsAndCartageParent.RequiredDocumentsProvider;
				}
			}

			return requiredDocumentsParent;
		}

		#endregion

		void DISDataButton_Click(object sender, EventArgs e)
		{
			var mainForm = FindForm() as ZForm;
			var disHost = GetDISHostFromDataSource();
			DISHelper.DISButtonClick(mainForm, disHost, (isEditAllowed) => ShowDISForm(mainForm, disHost, isEditAllowed));
		}

#if DEBUG
		protected virtual
#endif
		void ShowDISForm(Form mainForm, IDISHost disHost, bool isEditAllowed)
		{
			var controllerID = ControllerIDs.Customs.DocumentImageSystem;
			var controller = ZControllerFactory.Create(controllerID);
			controller.SetFormsModalTo(mainForm);
			var disHostBO = (BusinessObject)disHost;
			if (isEditAllowed)
			{
				controller.ShowEditForm(disHostBO);
			}
			else
			{
				controller.ShowViewForm(disHostBO);
			}
		}

		IDISHost GetDISHostFromDataSource()
		{
			var disHostProvider = DataSource as IDISHostProvider;
			return disHostProvider?.DISHost;
		}
	}
}
