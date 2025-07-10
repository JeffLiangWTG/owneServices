using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.GUI.RateChooser;
using Api = WiseRates.Api;

namespace Enterprise.Rating.GUI
{
	public class RateChooserViewModel : NonPersistentBusinessObject, IRateChooserImages, IObsoleteValidation, INotifyPropertyChanged
	{
		public RateChooserViewModel(RateChooserModel model)
			: base(model.Criteria.Factory)
		{
			Model = model;
			containerCommodityList = new List<ChooserContainerCommodityViewModel>();

			foreach (var containerCommodity in model.ContainerGroups)
			{
				var subView = new ChooserContainerCommodityViewModel(containerCommodity, this);
				subView.PropertyChanged += ContainerCommodityRatesViewModelPropertyChanged;
				subView.SelectRelatedRates += SelectRelatedRates;

				containerCommodityList.Add(subView);
			}
		}

		readonly List<ChooserContainerCommodityViewModel> containerCommodityList;
		public RateChooserModel Model { get; }

		public IEnumerable<ChooserContainerCommodityViewModel> ContainerTabs => containerCommodityList;
		public IEnumerable<ChooserRateRow> SelectedRows
		{
			get
			{
				return ContainerTabs
					.Select(ct =>
						ct.SelectedRow != null ? new ChooserRateRow(ct.Model, ct.SelectedRow.Rate, this, containerCommodityList) : new ChooserRateRow(ct.Model, null, this, null)
					);
			}
		}

		public bool AllTabsHaveSelection => ContainerTabs.All(x => x.SelectedRow != null);

		public bool HasMorePages => Model.HasMorePages;

		public bool IsApplyEnabled
		{
			get
			{
				return containerCommodityList.Any(x => x.SelectedRow != null)
					&& !SelectionNotificationTextInfo.HasErrors();
			}
		}

		public List<ChooserContainerCommodityViewModel> GetContainerCommodityList()
		{
			return containerCommodityList;
		}

		public void ShowMoreRates()
		{
			Model.ShowMoreRates();
		}

		public virtual void SendRatesRequest(
			IRateSelectorFilterValueProvider filter,
			Api.Model.RatesQuery ratesQuery,
			bool isValidForRatesService = true,
			IEnumerable<string> universalCarrierLevels = null)
		{
			Model.SendRatesRequest(
				filter: filter,
				ratesQuery: ratesQuery,
				isValidForRatesService: isValidForRatesService,
				pageID: 0,
				universalCarrierLevels: universalCarrierLevels);
		}

		public void RefreshRates()
		{
			for (int i = 0; i < containerCommodityList.Count; i++)
			{
				containerCommodityList[i].RefreshRates(Model.Criteria?.JobID, containerCommodityList.Where((x, index) => index != i));
			}

			OnPropertyChanged(nameof(SelectedRows));
			AllRatesRefreshed?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// UI handles this event to refresh selection controls
		/// </summary>
		public event EventHandler<RateSelectionChangedEventArgs> RateSelectionChanged;

		/// <summary>
		/// UI will handle this event to generate rate cards in each tab.
		/// </summary>
		public event EventHandler<EventArgs> AllRatesRefreshed;

		void OnRateSelectionChanged(ChooserContainerCommodityViewModel containerViewModel)
		{
			RateSelectionChanged?.Invoke(this, new RateSelectionChangedEventArgs(containerViewModel));
		}

		/// <summary>
		/// User had removed a rate from the selection summary
		/// </summary>
		/// <param name="removedRate"></param>
		public void UnselectRateFromSummary(ChooserRateRow removedRate)
		{
			var containerViewModel = containerCommodityList.First(x => x.Model == removedRate.ContainerInfo);
			var selectedRow = containerViewModel.SelectedRow;
			if (selectedRow != null)
			{
				selectedRow.IsSelected = false;
			}
		}

		public void ClearAll()
		{
			var info = SelectionNotificationTextInfo;
			info.ClearAllNotifications();
			info.ClearValue();

			foreach (var item in SelectedRows)
			{
				UnselectRateFromSummary(item);
			}
		}

		/// <summary>
		/// Validate when apply button is clicked
		/// </summary>
		public void Validate(List<ZString> zeroCharges)
		{
			var model = Model;
			model.Validate(zeroCharges);
		}

		public void Validate() => Validate(new List<ZString>());

		public bool IsValid => Model.IsValid;

		void CheckSelectedRatesDifferences()
		{
			var info = SelectionNotificationTextInfo;
			info.ClearAllNotifications();

			if (Model.SelectedRatesHaveDifferentServiceProvider)
			{
				info.AddError(Res.GetString("38E12BD4-F055-469C-B542-1D977580D0F5", "The rates have different Service Provider."));
			}

			if (Model.SelectedRatesHaveDifferentCarrierServiceLevel)
			{
				info.AddError(Res.GetString("43E41F60-B534-42E8-9333-E43C787728B4", "The rates have different Carrier Service Level."));
			}

			if (Model.SelectedRatesHaveDifferentSchedule)
			{
				info.AddError(Res.GetString("EC9EF0CB-FE23-4F06-BCCE-7B34BCC65863", "The rates have different Schedules"));
			}

			if (Model.SelectedRatesHaveDifferentCarrierContractNumbers)
			{
				info.AddWarning(Res.GetString("C5829E30-8937-48F3-8734-3610A2EB53C5", "The rates have different Carrier Contract Numbers."));
			}

			if (Model.SelectedRatesHaveDifferentNamedAccounts)
			{
				info.AddWarning(Res.GetString("73AE5442-4B50-405D-9A2C-C8C087414762", "The rates have different Named Accounts."));
			}

			SelectionNotificationText = info.Notifications.FirstOrDefault()?.Message;
		}

		void SelectRelatedRates(object sender, EventArgs e)
		{
			var containerViewModel = sender as ChooserContainerCommodityViewModel;
			if (containerViewModel == null)
			{
				return;
			}

			var selectedRate = containerViewModel.SelectedRow;

			foreach (var container in containerCommodityList)
			{
				if (container == containerViewModel)
				{
					continue;
				}

				var rate = container.Rates.FirstOrDefault(x => x.Rate.IsRelatedTo(selectedRate.Rate));
				if (rate != null)
				{
					rate.IsSelected = true;
				}
			}

			SelectRelatedRatesClicked?.Invoke(containerViewModel, EventArgs.Empty);
			CheckSelectedRatesDifferences();
		}

		void ContainerCommodityRatesViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var viewModel = sender as ChooserContainerCommodityViewModel;
			if (viewModel == null)
			{
				return;
			}

			if (e.PropertyName == nameof(ChooserContainerCommodityViewModel.SelectedRow))
			{
				OnPropertyChanged(nameof(SelectedRows));
				CheckSelectedRatesDifferences();
				OnRateSelectionChanged(viewModel);
			}
		}

		public ZString SelectionNotificationText
		{
			get => selectionNotificationText;
			set
			{
				selectionNotificationText = value;
				SelectionNotificationTextInfo.RefreshBinding();
			}
		}
		ZString selectionNotificationText;

		public ZPropertyInfo SelectionNotificationTextInfo => GetZPropertyInfo(nameof(SelectionNotificationText));

		public event EventHandler TabSelectionChanged;

		public event EventHandler SelectRelatedRatesClicked;

		public void SummaryRowClicked(ChooserRateRow row)
		{
			if (row.Rate == null)
			{
				var containerViewModel = containerCommodityList.Single(x => x.Model == row.ContainerInfo);
				TabSelectionChanged?.Invoke(containerViewModel, EventArgs.Empty);
			}
		}

		public const string CargoSphereProviderCode = WiseRates.Constants.WRConstants.RateProviders.CargoSphere;
		public const string CargoGuideProviderCode = WiseRates.Constants.WRConstants.RateProviders.CargoGuide;

		public Image GetProviderIcon(string provider)
		{
			if (!providerIconMap.TryGetValue(provider, out var icon))
			{
				switch (provider)
				{
					//TODO: To display the CS icon when URS enabled. need to be confirmed that when URS enabled, the provider is "URS"
					case "URS":
					case CargoSphereProviderCode:
						icon = Properties.RatingResources.CargoSphereIcon;
						break;
					case CargoGuideProviderCode:
						icon = null;
						break;
					case "":
						icon = BrandingFactory.Instance.ProductIcon.ToImage();
						break;
				}

				providerIconMap[provider] = icon;
			}

			return icon;
		}

		readonly Dictionary<string, Image> providerIconMap = new Dictionary<string, Image>();

		#region Notification

		event PropertyChangedEventHandler propertyChanged;
		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				if (value != null)
				{
					if (propertyChanged == null || !propertyChanged.GetInvocationList().Select(i => i.Method).Contains(value.Method))
					{
						propertyChanged += value;
					}
				}
			}
			remove
			{
				propertyChanged -= value;
			}
		}

		protected void OnPropertyChanged(string name)
		{
			Argument.NotNullOrEmpty(name, nameof(name));
			if (propertyChanged != null)
			{
				propertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion
	}

	public interface IRateChooserImages
	{
		Image GetProviderIcon(string provider);
	}

	public class RateSelectionChangedEventArgs : EventArgs
	{
		public RateSelectionChangedEventArgs(ChooserContainerCommodityViewModel viewModel)
		{
			ViewModel = viewModel;
		}

		public ChooserContainerCommodityViewModel ViewModel;
	}
}
