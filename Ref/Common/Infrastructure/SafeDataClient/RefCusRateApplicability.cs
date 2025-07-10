using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New
{
	[NonPersistentObject]
	public class RefCusRateApplicability : INonPersistentBusinessObjectFlatten
	{
		public RefCusRateApplicability(ISafeRepository repo)
		{
			S01_PK = Guid.NewGuid();
			RefCusRateApplicabilityUOMs = new HashSet<RefCusRateApplicabilityUOM>();
			RefCusExcludedTradeGroupNews = new HashSet<RefCusExcludedTradeGroupNew>();
			repo.TrackNonPersistentFlattenObject(this);
		}

		public RefCusRateApplicability(RefCusRate rate, RefCusApplicability app, ISafeRepository repo)
			: this(repo)
		{
			S01_ZZ1_Tariff = rate.ZZ2_ZZ1_Tariff;
			S01_ZZW_TariffNationalCode = rate.ZZ2_ZZW_TariffNationalCode;
			S01_StartDate = app.ZZT_StartDate > rate.ZZ2_StartDate ? app.ZZT_StartDate : rate.ZZ2_StartDate;
			S01_EndDate = app.ZZT_EndDate < rate.ZZ2_EndDate ? app.ZZT_EndDate : rate.ZZ2_EndDate;
			S01_ZY1_RateCode = rate.ZZ2_ZY1_RateCode;
			S01_RateFormula = rate.ZZ2_RateFormula;
			S01_ZZS_Preference = rate.ZZ2_ZZS_Preference;
			S01_SelectorFormula = rate.ZZ2_SelectorFormula;
			S01_ZZZ_NKDataGrouping = rate.ZZ2_ZZZ_NKDataGrouping;
			S01_RateFormulaDerivedFrom = rate.ZZ2_RateFormulaDerivedFrom;
			S01_RX_NKCurrencyOverride = rate.ZZ2_RX_NKCurrencyOverride;
			S01_ZZA_TradeGroup = app.ZZT_ZZA_TradeGroup;
			S01_AdditionalCode = app.ZZT_AdditionalCode;
			S01_OrderNumber = app.ZZT_OrderNumber;
			S01_ZZA_SecondTradeGroup = app.ZZT_ZZA_SecondTradeGroup;
			S01_ZY2_AdditionalCode = app.ZZT_ZY2_AdditionalCode;
			S01_ZZH_TariffRelationship = app.ZZT_ZZH_TariffRelationship;
			RefCusApplicability = app;
			RefCusRate = rate;
			app.RefCusRateApplicabilities.Add(this);
			rate.RefCusRateApplicabilities.Add(this);
			foreach (var uom in rate.RefCusRateUOMs)
			{
				var rateAppUOM = new RefCusRateApplicabilityUOM(uom, this);
				rateAppUOM.S02_S01_RateApplicability = S01_PK;
				RefCusRateApplicabilityUOMs.Add(rateAppUOM);
			}
			foreach (var ex in app.RefCusExcludedTradeGroups)
			{
				var exNew = new RefCusExcludedTradeGroupNew(ex, this);
				exNew.S03_S01_RateApplicability = S01_PK;
				RefCusExcludedTradeGroupNews.Add(exNew);
			}
		}

		public IEnumerable<object> LinkedObjects
		{
			get
			{
				yield return RefCusApplicability;
				yield return RefCusRate;
				foreach (var uom in RefCusRateApplicabilityUOMs)
				{
					yield return uom.RefCusRateUOM;
				}
				foreach (var ex in RefCusExcludedTradeGroupNews)
				{
					yield return ex.RefCusExcludedTradeGroup;
				}
			}
		}

		public void Link(INonPersistentBusinessObjectParent parent, INonPersistentBusinessObjectComparison comp, out IEnumerable<object> unLinkedObjs)
		{
			if (!IsCreatedNewCalled)
			{
				throw new InvalidOperationException("CreateNew must be called before Link");
			}
			var rate = (RefCusRate)parent;
			var results = new List<object>();
			var unlinkedRate = UnlinkRefCusRate();
			RefCusApplicability unlinkedApp = null;
			results.Add(unlinkedRate);
			var matchedApp = rate.RefCusApplicabilities.FirstOrDefault(x => comp.IsIdentical(this, x));
			if (matchedApp == null)
			{
				matchedApp = RefCusApplicability;
				rate.RefCusApplicabilities.Add(matchedApp);
				matchedApp.ZZT_ZZ2_Rate = rate.ZZ2_PK;
				unlinkedRate.RefCusApplicabilities.Remove(matchedApp);
			}
			else
			{
				unlinkedApp = UnlinkRefCusApplicability();
				results.Add(unlinkedApp);
			}
			Link(matchedApp, rate);
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				var matchedEx = matchedApp.RefCusExcludedTradeGroups.FirstOrDefault(x => comp.IsIdentical(exNew, x));
				if (matchedEx == null)
				{
					matchedEx = exNew.RefCusExcludedTradeGroup;
					matchedEx.ZZC_ZZT_Applicability = matchedApp.ZZT_PK;
					matchedApp.RefCusExcludedTradeGroups.Add(matchedEx);
					if (unlinkedApp != null)
					{
						unlinkedApp.RefCusExcludedTradeGroups.Remove(matchedEx);
					}
				}
				else
				{
					results.AddRange(exNew.Unlink());
				}
				exNew.Link(matchedEx);
			}
			foreach (var rateAppUOM in RefCusRateApplicabilityUOMs)
			{
				var matchedUOM = rate.RefCusRateUOMs.FirstOrDefault(x => comp.IsIdentical(rateAppUOM, x));
				if (matchedUOM == null)
				{
					matchedUOM = rateAppUOM.RefCusRateUOM;
					matchedUOM.ZXG_ZZ2_Rate = rate.ZZ2_PK;
					rate.RefCusRateUOMs.Add(matchedUOM);
					unlinkedRate.RefCusRateUOMs.Remove(matchedUOM);
				}
				else
				{
					results.AddRange(rateAppUOM.Unlink());
				}
				rateAppUOM.Link(matchedUOM);
			}
			unLinkedObjs = results;
		}

		void Link(RefCusApplicability app, RefCusRate rate)
		{
			RefCusApplicability = app;
			RefCusRate = rate;
			if (!app.RefCusRateApplicabilities.Contains(this))
			{
				app.RefCusRateApplicabilities.Add(this);
			}
			if (!rate.RefCusRateApplicabilities.Contains(this))
			{
				rate.RefCusRateApplicabilities.Add(this);
			}
		}

		public IEnumerable<object> Create()
		{
			IsCreatedNewCalled = true;
			var result = new List<object>();
			if (RefCusApplicability == null)
			{
				var rate = new RefCusRate() { ZZ2_PK = Guid.NewGuid() };
				rate.ZZ2_ZZ1_Tariff = S01_ZZ1_Tariff;
				rate.ZZ2_ZZW_TariffNationalCode = S01_ZZW_TariffNationalCode;
				result.Add(rate);
				var app = new RefCusApplicability() { ZZT_PK = Guid.NewGuid() };
				rate.RefCusApplicabilities.Add(app);
				app.ZZT_ZZ2_Rate = rate.ZZ2_PK;
				Link(app, rate);
				result.Add(app);
				UpdateCore(true);
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				if (exNew.RefCusExcludedTradeGroup == null)
				{
					var ex = RefCusApplicability.RefCusExcludedTradeGroups.FirstOrDefault(x => nonPersistentObjecComparison.IsIdentical(exNew, x));
					if (ex == null)
					{
						ex = new RefCusExcludedTradeGroup() { ZZC_PK = Guid.NewGuid() };
						ex.ZZC_ZZT_Applicability = RefCusApplicability.ZZT_PK;
						RefCusApplicability.RefCusExcludedTradeGroups.Add(ex);
						result.Add(ex);
					}
					exNew.Link(ex);
					exNew.Update();
				}
			}
			foreach (var rateAppUOM in RefCusRateApplicabilityUOMs)
			{
				if (rateAppUOM.RefCusRateUOM == null)
				{
					var uom = RefCusRate.RefCusRateUOMs.FirstOrDefault(x => nonPersistentObjecComparison.IsIdentical(rateAppUOM, x));
					if (uom == null)
					{
						uom = new RefCusRateUOM() { ZXG_PK = Guid.NewGuid() };
						uom.ZXG_ZZ2_Rate = RefCusRate.ZZ2_PK;
						RefCusRate.RefCusRateUOMs.Add(uom);
						result.Add(uom);
					}
					rateAppUOM.Link(uom);
					rateAppUOM.Update();
				}
			}
			return result;
		}

		public IEnumerable<object> Unlink()
		{
			var result = new List<object>();
			foreach (var rateAppUOMs in RefCusRateApplicabilityUOMs)
			{
				result.AddRange(rateAppUOMs.Unlink());
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				result.AddRange(exNew.Unlink());
			}
			result.Add(UnlinkRefCusApplicability());
			result.Add(UnlinkRefCusRate());
			return result;
		}

		RefCusRate UnlinkRefCusRate()
		{
			var result = RefCusRate;
			result.RefCusRateApplicabilities.Remove(this);
			RefCusRate = null;
			return result;
		}

		RefCusApplicability UnlinkRefCusApplicability()
		{
			var result = RefCusApplicability;
			result.RefCusRateApplicabilities.Remove(this);
			RefCusApplicability = null;
			return result;
		}

		void UpdateCore(bool updateParent)
		{
			var app = RefCusApplicability;
			var rate = RefCusRate;

			if (updateParent)
			{
				rate.ZZ2_StartDate = NonPersistentBusinessObjectComparison.MinStartDate;
				rate.ZZ2_EndDate = NonPersistentBusinessObjectComparison.MaxEndDate;
				rate.ZZ2_ZY1_RateCode = S01_ZY1_RateCode;
				rate.ZZ2_RateFormula = S01_RateFormula;
				rate.ZZ2_ZZS_Preference = S01_ZZS_Preference;
				rate.ZZ2_SelectorFormula = S01_SelectorFormula;
				rate.ZZ2_ZZZ_NKDataGrouping = S01_ZZZ_NKDataGrouping;
				rate.ZZ2_RateFormulaDerivedFrom = S01_RateFormulaDerivedFrom;
				rate.ZZ2_RX_NKCurrencyOverride = S01_RX_NKCurrencyOverride;
			}
			app.ZZT_ZZA_TradeGroup = S01_ZZA_TradeGroup;
			app.ZZT_AdditionalCode = S01_AdditionalCode;
			app.ZZT_OrderNumber = S01_OrderNumber;
			app.ZZT_ZZA_SecondTradeGroup = S01_ZZA_SecondTradeGroup;
			app.ZZT_ZY2_AdditionalCode = S01_ZY2_AdditionalCode;
			app.ZZT_StartDate = S01_StartDate;
			app.ZZT_EndDate = S01_EndDate;
		}

		public void Update(bool updateParent)
		{
			UpdateCore(updateParent);
			foreach (var rateAppUOM in RefCusRateApplicabilityUOMs)
			{
				rateAppUOM.Update();
			}
			foreach (var exNew in RefCusExcludedTradeGroupNews)
			{
				exNew.Update();
			}
		}

		bool IsCreatedNewCalled;
		INonPersistentBusinessObjectComparison nonPersistentObjecComparison = new NonPersistentBusinessObjectComparison();

		public Guid? TariffPK => S01_ZZ1_Tariff;
		public Guid? TariffNationalCodePK => S01_ZZW_TariffNationalCode;
		public Guid? NomenclatureGroupPK => null;
		public INonPersistentBusinessObjectParent Parent => RefCusRate;
		public ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects => new HashSet<INonPersistentBusinessObjectFlatten> { this };

		public Guid S01_PK { get; set; }
		public Guid? S01_ZZ1_Tariff { get; set; }
		public Guid? S01_ZZW_TariffNationalCode { get; set; }
		public DateTimeOffset S01_StartDate { get; set; }
		public DateTimeOffset S01_EndDate { get; set; }
		public Guid? S01_ZY1_RateCode { get; set; }
		public string S01_RateFormula { get; set; }
		public Guid? S01_ZZS_Preference { get; set; }
		public string S01_SelectorFormula { get; set; }
		public string S01_ZZZ_NKDataGrouping { get; set; }
		public string S01_RateFormulaDerivedFrom { get; set; }
		public string S01_RX_NKCurrencyOverride { get; set; }
		public Guid? S01_ZZA_TradeGroup { get; set; }
		public string S01_AdditionalCode { get; set; }
		public string S01_OrderNumber { get; set; }
		public Guid? S01_ZZA_SecondTradeGroup { get; set; }
		public Guid? S01_ZY2_AdditionalCode { get; set; }
		public Guid? S01_ZZH_TariffRelationship { get; set; }
		public ICollection<RefCusRateApplicabilityUOM> RefCusRateApplicabilityUOMs { get; set; }
		public ICollection<RefCusExcludedTradeGroupNew> RefCusExcludedTradeGroupNews { get; set; }
		public RefCusApplicability RefCusApplicability { get; private set; }
		public RefCusRate RefCusRate { get; private set; }
	}
}
