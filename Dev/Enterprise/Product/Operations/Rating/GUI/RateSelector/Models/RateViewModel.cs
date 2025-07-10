using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.GUI.RateSelection;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public abstract class RateViewModel : ViewModelWithNotificationBase, ICloneable, IDisposable
	{
		protected RateViewModel()
		{
			FreightCharges = new ChargesViewModel(ResStrings.FreightCharges);
			SubjectToCharges = new ChargesViewModel(ResStrings.SubjectToCharges);
			OptionalCharges = new ChargesViewModel(ResStrings.OptionalCharges);
			OtherCharges = new ChargesViewModel(ResStrings.OtherCharges);

			FreightCharges.PropertyChanged += ChargesCollectionChanged;
			SubjectToCharges.PropertyChanged += ChargesCollectionChanged;
			OptionalCharges.PropertyChanged += ChargesCollectionChanged;
			OtherCharges.PropertyChanged += ChargesCollectionChanged;

			//TOOD: Add disposable leak listener to make sure this is disposed
			//      and event handles cleaned up
			//      DisposableLeakListener.Instance.RegisterDisposable(this) here
			//      DisposableLeakListener.Instance.UnRegisterDisposable(this) on the dispose method
		}

		protected RateViewModel(BusinessObjectFactory factory)
			: this()
		{
			Factory = Argument.NotNull(factory, nameof(factory));
		}

		public BusinessObjectFactory Factory { get; }

		public IEnumerable<IRateLine> Lines { get; protected set; }

		public OrgHeader CarrierOrg { get; set; }
		public string CarrierCode
		{
			get { return carrierCode; }
			set
			{
				carrierCode = value;
				OnPropertyChanged(nameof(CarrierCode));
			}
		}

		public string CarrierName
		{
			get { return carrierName; }
			set
			{
				carrierName = value;
				OnPropertyChanged(nameof(CarrierName));
			}
		}

		public string[] GetZeroCharges()
			=> Charges
				.SelectMany(chargeViewModel => chargeViewModel.Charges)
				.Where(chargeViewModel => chargeViewModel.IsSelected && chargeViewModel.Amount == 0 && !chargeViewModel.IsIncluded && chargeViewModel.AccChargeCode != null)
				.Select(chargeViewModel => chargeViewModel.ChargeCode)
				.ToArray();

		public string ContractNumber { get; set; }
		public string Origin { get; set; }
		public string Via { get; set; }
		public string Destination { get; set; }
		public string ContainerType { get; set; }
		/// <summary>
		/// Represents all the commodity codes(CW1) or group name (CG) in the
		/// rate in a comma separated list. There may be more than one item. 
		/// </summary>
		public string Commodities { get; set; }
		/// <summary>
		/// For Cargoguide rates, this represents the commodity groups on the
		/// rate itself.
		/// For CW1 rates, this represents the commodity groups that are
		/// mapped to the commodity code in the rate. 
		/// </summary>
		public IEnumerable<string> CommodityGroups { get; set; }
		/// <summary>
		/// Represents the commodity codes from the CW1 rates
		/// </summary>
		public IEnumerable<string> CommodityCodes { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string CarrierServiceLevel { get; set; }
		public string PaymentTerms { get; set; }
		public string Currency { get; set; }

		public ChargesViewModel FreightCharges { get; }
		public ChargesViewModel SubjectToCharges { get; }
		public ChargesViewModel OptionalCharges { get; }
		public ChargesViewModel OtherCharges { get; }
		public ObservableCollection<TransportLegViewModel> TransportLegs { get; set; }

		public bool IsExpanded
		{
			get
			{
				return isExpended;
			}
			set
			{
				isExpended = value;
				OnPropertyChanged(nameof(IsExpanded));
			}
		}

		protected IDisposable EffectiveDateDisposer { get; set; }

		#region Calculated Properties

		public bool IsValid => !ErrorLevel.HasFlag(ErrorLevel.Error);

		public virtual ErrorLevel ErrorLevel
		{
			get
			{
				var errorLevel = ErrorLevel.None;

				Charges
					.SelectMany(c => c.Charges)
					.ForEach(c => errorLevel |= c.ErrorLevel);

				return errorLevel;
			}
		}

		public virtual bool DisplayPrice => Env.Security.MaintainConsolJobInvoicingAllowViewPricesInRateSelection.IsAllowed;

		public string Routing => string.IsNullOrEmpty(Via)
			? $"{Origin} > {Destination}"               // Not translatable
			: $"{Origin} > {Via} > {Destination}";      // Not translatable

		public string TotalPriceError
		{
			get
			{
				var errors = Charges
					.Select(c => c.TotalPriceError)
					.Where(error => !string.IsNullOrEmpty(error))
					.Distinct()
					.ToArray();

				if (errors.Any())
				{
					return string.Join(System.Environment.NewLine, errors);
				}

				return string.Empty;
			}
		}

		public decimal TotalPrice => Charges.Sum(c => c.TotalPrice);

		protected virtual Money TotalPriceMoney => new Money(TotalPrice, GlbCompany.CurrentCompany.LocalCurrency);

		public decimal TotalPriceAmount => TotalPriceMoney.Amount;

		public string TotalPriceString
		{
			get
			{
				return TotalPriceMoney.ToString();
			}
		}

		public IEnumerable<ChargesViewModel> Charges
		{
			get
			{
				var charges = new[]
				{
					FreightCharges,
					SubjectToCharges,
					OptionalCharges,
					OtherCharges
				};

				return charges.Where(c => c.Charges.Any());
			}
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "Code analysis is silly")]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				FreightCharges.PropertyChanged -= ChargesCollectionChanged;
				SubjectToCharges.PropertyChanged -= ChargesCollectionChanged;
				OptionalCharges.PropertyChanged -= ChargesCollectionChanged;
				OtherCharges.PropertyChanged -= ChargesCollectionChanged;

				FreightCharges.Dispose();
				SubjectToCharges.Dispose();
				OptionalCharges.Dispose();
				OtherCharges.Dispose();
			}
		}

		#endregion

		#region IClonable

		public abstract object Clone();

		#endregion

		public void CleanUpAfterSearch()
		{
			EffectiveDateDisposer?.Dispose();
		}

		public void Calculate(RatingCriteria criteria, IEnumerable<string> originalChargeCodeGroups)
		{
			criteria.ValuesCanBeSet = true;
			UpdateCriteria(criteria, originalChargeCodeGroups);

			var filterOptions = new NotApplicableRateLineRemover.FilterOptions
			{
				DisableSpotFilter = true,
				DisableInvalidRatesFilter = true
			};

			var entries = Lines.Select(l => l.ParentRateEntry).Distinct().ToList();
			// When calculating lines with CST/CTB calculators, we will have to cache the line and traverse to original lines using autoRater.
			// Without passing the same factory to new RatingContext, removing a line from a cached list fails.
			// It may lead to duplicate key caching by Line.PK later.
			// For example, in FreightAutoRater.LoadCostOrCompanyTariffRateLines: linesRepository.Remove(fastLine,...)
			var context = Business.RatingContext.CreateInstance(Lines.FirstOrDefault());
			var freightAutoRater = new FreightAutoRater(context);

			var results = freightAutoRater.CalculateResultsForBestMatches(CostSell.Cost, criteria, entries, filterOptions);
			results.SumUpSameCharges();

			var logs = string.Join(System.Environment.NewLine,
				context.Logger.GetLogs(l =>
					FormattableString.Invariant(
						$"{l.Type}\t{l.Message}"))); // just logs

			SetCharges(results, logs);
		}

		public abstract IEnumerable<AutoRateInfo> GetAutoRateInfos();
		protected abstract void SetCharges(IEnumerable<AutoRateInfo> autoRateInfoCollection, string logs);

		protected virtual void UpdateCriteria(RatingCriteria criteria, IEnumerable<string> originalChargeCodeGroups)
		{
			var carrier = this.GetCarrierToApplyToJob();
			if (carrier == null)
			{
				return;
			}

			var chargeCodeGroups = criteria.Creditors?.ChargeCodeGroups ?? Array.Empty<string>();
			var source = new List<string> { (NoResString)"CS Calculated Rate Selector" };   // internal use

			if (chargeCodeGroups.IsNullOrEmpty() || chargeCodeGroups.Any(string.IsNullOrWhiteSpace))
			{
				criteria.Creditors = Creditors.New(OrgWithSource.New(carrier, source));
			}
			else
			{
				var creditors = new Creditors();

				foreach (var chargeCodeGroup in chargeCodeGroups)
				{
					creditors[chargeCodeGroup].Add(1, OrgWithSource.New(carrier, source));
				}

				criteria.Creditors = creditors;
			}
		}

		void ChargesCollectionChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ChargesViewModel.TotalPrice))
			{
				OnPropertyChanged(nameof(TotalPrice));
				OnPropertyChanged(nameof(TotalPriceString));
			}
			else if (e.PropertyName == nameof(ChargesViewModel.TotalPriceError))
			{
				OnPropertyChanged(nameof(TotalPriceError));
			}

			OnPropertyChanged(nameof(IsValid));
		}

		bool isExpended;
		string carrierCode;
		string carrierName;
	}
}
