using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public partial class ContainerBatchUpdate : BasePageWithAuthorisation
	{
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveChanges.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (BatchHolder == null)
			{
				ContainerBatchLabel.Text = Res.GetString("c3fd494d-b7ed-438c-986e-579e5a607ee8", "Containers Not Found");
				NotFoundLabel.Text = Res.GetString("1dc27b57-ba35-4bbf-9374-780e468ac4e8", "Containers were not found in the database or you don't have rights to edit them.");
				BatchContents.Visible = false;
			}
			else
			{
				NotFoundError.Visible = false;
				BatchContents.Visible = true;
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanEditTrackingContainers; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (SiteUser != null)
			{
				RequiredDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersRequiredDeliveryDate;
				ConfirmedDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersConfirmedDeliveryDate;
				ActualDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersActualDeliveryDate;
				EstimatedDehire.Enabled = isAuthorised && SiteUser.CanEditContainersEstimatedDehireDate;
				Pickup.Enabled = isAuthorised && SiteUser.CanEditContainersPickupDate;
				ActualDehire.Enabled = isAuthorised && SiteUser.CanEditContainersActualDehireDate;
			}
		}

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			ContainerBatchHolder result = null;
			IReadOnlyCollection<ZGuid> newPKs = ContainerPKs;

			if (newPKs.Count > 0)
			{
				result = new ContainerBatchHolder(Factory, newPKs.ToArray());
			}

			return result;
		}

		protected ContainerBatchHolder BatchHolder
		{
			get { return DataSource as ContainerBatchHolder; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			if (!DataSource.HasErrors && !DataSource.HasMessageErrors)
			{
				RedirectToContainerModule();
			}
			else
			{
				NotificationFlags.DisplayAll = true;
				NotificationFlags.DisplayWarnings = false;
			}
		}

		protected void RedirectToContainerModule()
		{
			Response.Redirect(AppInstance.ContainersPage);
		}

		#endregion BusinessObject

		#region ContainerPKs

		protected IReadOnlyCollection<ZGuid> ContainerPKs
		{
			get { return GetContainerPKs(); }
		}

		protected ZGuid[] GetContainerPKs()
		{
			List<ZGuid> result = new List<ZGuid>();

			ZString param = GetStringFromParameter(RefParameterName);
			if (!param.IsEmpty)
			{
				ZString[] stringPKs = param.Split(',');
				for (int i = 0; i < stringPKs.Length; i++)
				{
					ZGuid pk = new ZGuid(stringPKs[i]);
					if (pk.IsValid && !pk.IsEmpty)
					{
						result.Add(pk);
					}
				}
			}

			return result.ToArray();
		}

		#endregion

		#region Grid setup

		protected override void SetupGrids()
		{
			SetupContainerGrid();
		}

		protected void SetupContainerGrid()
		{
			SelectedContainersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("ef7919fa-f6a4-454e-8ee2-7d57a762786e", "Container #"), TrackingContainer.Schema.JC_ContainerCode));
			SelectedContainersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("aaf1885c-c79e-436b-8ea9-bc22a4923a4d", "Shipment #"), TrackingContainer.Schema.ShipmentNumbers));

			SelectedContainersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("e517c275-4f97-4d90-8034-69702e093973", "Pack #"), TrackingContainer.Schema.Packs));

			SelectedContainersGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("a41c505b-26ab-48e5-804d-e9b17e389f32", "Arrival"), TrackingContainer.Schema.Arrival));

			ZDateTimeColumn requiredDelivery = GetNewZDateTimeColumn(Res.GetString("3d6980f2-9b27-42ef-942b-a6496337cc3a", "Required Delivery"),
				TrackingContainer.Schema.RequiredDelivery,
				SiteUser != null && SiteUser.CanEditContainersRequiredDeliveryDate);
			SelectedContainersGrid.Columns.Add(requiredDelivery);

			ZDateTimeColumn confirmedDelivery = GetNewZDateTimeColumn(Res.GetString("6b73fb3c-f5cc-45e8-9554-bff94f4982e9", "Confirmed Delivery"),
				TrackingContainer.Schema.ConfirmedDelivery,
				SiteUser != null && SiteUser.CanEditContainersConfirmedDeliveryDate);
			SelectedContainersGrid.Columns.Add(confirmedDelivery);

			ZDateTimeColumn actualDelivery = GetNewZDateTimeColumn(Res.GetString("d57446a1-a08e-4363-ba0f-6ec8bff87d4f", "Actual Delivery"),
				TrackingContainer.Schema.ActualDelivery,
				SiteUser != null && SiteUser.CanEditContainersActualDeliveryDate);
			SelectedContainersGrid.Columns.Add(actualDelivery);

			ZDateTimeColumn estimatedDehire = GetNewZDateTimeColumn(Res.GetString("66b77ad8-7872-4e16-8b42-a3bc20235b13", "Empty Ready"),
				TrackingContainer.Schema.EmptyReady,
				SiteUser != null && SiteUser.CanEditContainersEstimatedDehireDate);
			SelectedContainersGrid.Columns.Add(estimatedDehire);

			ZDateTimeColumn pickup = GetNewZDateTimeColumn(Res.GetString("9825541d-37a2-4069-a404-6492c2557eab", "Empty Pickup"),
				TrackingContainer.Schema.EmptyPickup,
				SiteUser != null && SiteUser.CanEditContainersPickupDate);
			SelectedContainersGrid.Columns.Add(pickup);

			ZDateTimeColumn actualDehire = GetNewZDateTimeColumn(Res.GetString("fb5b58c9-456d-4080-aefc-66f38d7487ae", "Actual De-hire"),
				TrackingContainer.Schema.ActualDehire,
				SiteUser != null && SiteUser.CanEditContainersActualDehireDate);
			SelectedContainersGrid.Columns.Add(actualDehire);

			ZCalcEditColumn sequence = new ZCalcEditColumn(Res.GetString("4711ad28-a9c8-4879-9ad8-80ce93f3b0fb", "Sequence"), TrackingContainer.Schema.JC_DeliverySequence);
			sequence.Decimals = 0;

			if (SiteUser != null && SiteUser.CanEditContainersSequence)
			{
				sequence.ReadOnly = false;
			}
			SelectedContainersGrid.Columns.Add(sequence);
		}

		ZDateTimeColumn GetNewZDateTimeColumn(string headerText, string bindTo, bool canEdit)
		{
			ZDateTimeColumn dateColumn = new ZDateTimeColumn(headerText, bindTo, ZDateTimePickerFormat.Long);

			dateColumn.ReadOnly = !canEdit;
			if (canEdit)
			{
				dateColumn.DateTimeFormat = ZDateTimePickerFormat.Long;
			}

			return dateColumn;
		}

		#endregion Grid setup

		#region EventHandlers

		protected void Apply_Click(object sender, EventArgs e)
		{
			ApplyGridChanges();
			BatchHolder.Apply();
			Page.DataBind();
		}

		protected void SaveChanges_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = true;
			SuppressErrorDialog = false;
			ApplyGridChanges();
			//SaveDataGridChanges(sender);
			SaveDataSourceFactory();
		}

		void ApplyGridChanges()
		{
			foreach (DataGridItem item in SelectedContainersGrid.Items)
			{
				if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem)
				{
					BusinessObject boundObject = BatchHolder.Containers[item.DataSetIndex] as BusinessObject;

					if (boundObject != null)
					{
						for (int i = 0; i < item.Cells.Count; i++)
						{
							if (item.Cells[i].Controls.Count > 0)
							{
								ISelfBindingPostbackWebControl ctrl = item.Cells[i].Controls[0] as ISelfBindingPostbackWebControl;
								if (ctrl != null && ctrl.HasChanges)
								{
									ctrl.Bind(boundObject);
								}
							}
						}
					}
				}
			}
		}

		protected void CancelChanges_Click(object sender, EventArgs e)
		{
			RedirectToContainerModule();
		}

		#endregion

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ContainerBatchUpdatePage;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ContainerBatchUpdate;
		}
	}
}
