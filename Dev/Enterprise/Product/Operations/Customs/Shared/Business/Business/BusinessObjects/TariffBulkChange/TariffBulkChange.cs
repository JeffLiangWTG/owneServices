using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class TariffBulkChange : BaseTariffBulkChange, IObsoleteValidation
	{
		public TariffBulkChange(BusinessObjectFactory factory)
			: base(factory, BaseCusClassification.ClassificationType.Both)
		{
		}

		public TariffBulkChange(BusinessObjectFactory factory, ZString lookupType)
			: base(factory, lookupType)
		{
		}

		#region Overrides

		protected override bool IsOldTariffUsedCore(string oldTariffNum)
		{
			var result = base.IsOldTariffUsedCore(oldTariffNum);
			if (!result && IsPivotTariffNumSupported)
			{
				result = Factory.LoadTop1<BaseCusClassPartPivot>(GetPivotTariffQuery(oldTariffNum)) != null;
			}
			return result;
		}

		protected override TariffBulkChangeOldTariff AddNewTariffNumPair(ZString oldtariffnum, ZString newtariffnum)
		{
			var oldTariff = TariffBulkChangeOldTariffs.AddNew();
			oldTariff.OldTariffNum = oldtariffnum;
			AddAdditionalNewTariffNum(oldTariff, newtariffnum);
			return oldTariff;
		}

		protected override void AddAdditionalNewTariffNum(TariffBulkChangeOldTariff bulkChangeOldTariff, ZString newtariffnum)
		{
			if (bulkChangeOldTariff != null)
			{
				bulkChangeOldTariff.TariffBulkChangeNewTariffs.AddNew().NewTariffNum = newtariffnum;
			}
		}

		protected override void RemoveTariffBulkChangeOldTariff(TariffBulkChangeOldTariff bulkChangeOldTariff)
		{
			TariffBulkChangeOldTariffs.Remove(bulkChangeOldTariff);
		}

		protected override void ProcessOne2One(ZString oldTariffNum, ZString newTariffNum)
		{
			var formattedNewTariffNum = CurrentTariffFormatter.DisplayFormat(newTariffNum);

			var oldTariffClassificationCollection = GetTBCClassificationCollection(Factory, GetClassificationQuery(oldTariffNum));
			oldTariffClassificationCollection.Load();
			foreach (TBCClassification classification in oldTariffClassificationCollection)
			{
				var oldTariff = classification.CC_FormattedTariffNum;
				classification.NewTariffNum = oldTariff.Length <= formattedNewTariffNum.Length ||
					formattedNewTariffNum.Length > 10 ? formattedNewTariffNum.ToString() : formattedNewTariffNum + oldTariff.Substring(formattedNewTariffNum.Length);
			}

			if (IsPivotTariffNumSupported)
			{
				var pivots = new TariffBulkChangerPivotCollection(Factory);
				pivots.Load(GetPivotTariffQuery(oldTariffNum));
				foreach (BaseCusClassPartPivot pivot in pivots)
				{
					var oldTariff = pivot.CI_FormattedTariffNum;
					pivot.NewTariffNum = oldTariff.Length <= formattedNewTariffNum.Length ||
						formattedNewTariffNum.Length > 10 ? formattedNewTariffNum.ToString() : formattedNewTariffNum + oldTariff.Substring(formattedNewTariffNum.Length);
				}
			}
		}

		protected override ZQuery GetClassificationQuery(ZString oldtariffnum)
		{
			var dummyOldTariff = new TariffBulkChangeOldTariff(this);
			dummyOldTariff.OldTariffNum = oldtariffnum;
			var result = new ZQuery();
			var tariffNumFilter = new ZQuery();
			tariffNumFilter.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, dummyOldTariff.TariffNumNoStats);
			tariffNumFilter.AddToFilter(JoinCondition.Or, CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, CurrentTariffFormatter.Format(dummyOldTariff.TariffNumNoStats));
			tariffNumFilter.AddToFilter(JoinCondition.Or, CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.StartsWith, dummyOldTariff.TariffNumNoStats.Replace(".", ""));
			result.AddToFilter(tariffNumFilter);
			if (dummyOldTariff.StatsCode != "")
			{
				result.AddToFilter(CusClassificationSchema.CC_TariffNum, SQLComparisonOperator.EndsWith, dummyOldTariff.StatsCode);
			}
			result.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
			result.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, CountryCode);
			return result;
		}

		protected ZQuery GetPivotTariffQuery(ZString oldtariffnum)
		{
			var dummyOldTariff = new TariffBulkChangeOldTariff(this);
			dummyOldTariff.OldTariffNum = oldtariffnum;
			var result = new ZQuery();
			var tariffNumFilter = new ZQuery();
			tariffNumFilter.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, dummyOldTariff.TariffNumNoStats);
			tariffNumFilter.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, CurrentTariffFormatter.Format(dummyOldTariff.TariffNumNoStats));
			tariffNumFilter.AddToFilter(JoinCondition.Or, CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.StartsWith, dummyOldTariff.TariffNumNoStats.Replace(".", ""));
			result.AddToFilter(tariffNumFilter);
			if (dummyOldTariff.StatsCode != "")
			{
				result.AddToFilter(CusClassPartPivotSchema.CI_TariffNum, SQLComparisonOperator.EndsWith, dummyOldTariff.StatsCode);
			}
			result.AddToFilter(CusClassPartPivotSchema.CI_ChildType, ValidPivotTypes);
			result.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, CountryCode);
			return result;
		}

		#endregion

		#region Methods

		public void UpdateOriginalLookups(TariffBulkChangeOldTariff selectedOldTariff,
			TariffBulkChangeNewTariff selectedNewTariff,
			BusinessObject[] selectedOriginalClassifications)
		{
			ZString selectedTariff = selectedNewTariff.NewTariffNum;
			foreach (TBCClassification classification in selectedOriginalClassifications)
			{
				if (classification.CC_FormattedTariffNum.Length <= selectedTariff.Length ||
					selectedTariff.Length > 10)
				{
					classification.NewTariffNum = selectedTariff;
				}
				else
				{
					classification.NewTariffNum = selectedTariff + classification.CC_FormattedTariffNum.Substring(selectedTariff.Length);
				}
				if (classification.NewLookupCode.IsEmpty)
				{
					classification.NewLookupCode = classification.CC_LookupCode;
				}
			}
			selectedOldTariff.fOriginalClassifications = null;
			selectedOldTariff.fNewClassifications = null;
			selectedOldTariff.RefreshBindingIncludingChildren();
		}

		public void UpdateProducts(TariffBulkChangeOldTariff selectedOldTariff,
			BaseCusClassification selectedNewClassification, TariffBulkChangeNewTariff selectedNewTariff,
			BusinessObject[] selectedPivots)
		{
			foreach (BaseCusClassPartPivot pivot in selectedPivots)
			{
				if (selectedNewClassification != null)
				{
					pivot.NewLookUpPK = selectedNewClassification.PK;
					selectedNewClassification.UnRegisterEditableChildObject(pivot);
					pivot.NewTariffNum = ZString.Empty;
				}
				else if (selectedNewTariff != null)
				{
					var selectedTariff = selectedNewTariff.NewTariffNum;
					var oldTariff = pivot.OldTariffCode;
					pivot.NewTariffNum = oldTariff.Length <= selectedTariff.Length ||
						selectedTariff.Length > 10 ? selectedTariff.ToString() : selectedTariff + oldTariff.Substring(selectedTariff.Length);
					pivot.NewLookUpPK = ZGuid.Empty;
				}
			}
			selectedOldTariff.TariffItemPivots.RefreshBindingIncludingChildren();
		}

		public void MakeNewClassifications(TariffBulkChangeOldTariff selectedOldTariff,
			BusinessObject[] selectedNewTariffs)
		{
			foreach (TariffBulkChangeNewTariff newTariff in selectedNewTariffs)
			{
				TBCClassification newClassification = selectedOldTariff.NewClassifications.AddNew();
				if (newTariff.NewTariffNum.Length > 10)
				{
					newClassification.CC_TariffNum = CurrentTariffFormatter.Format(newTariff.NewTariffNum);
				}
				else
				{
					newClassification.CC_TariffNum = CurrentTariffFormatter.Format(newTariff.NewTariffNum) + selectedOldTariff.TariffSuffix;
				}
				newClassification.NewTariffNum = CurrentTariffFormatter.DisplayFormat(newClassification.CC_TariffNum);
				newClassification.CC_ClassificationType = lookupType;
				newClassification.CC_Description = newClassification.NewTariffNum;
				newClassification.Validation.ValidateAll();
			}
			selectedOldTariff.fOriginalClassifications = null;
			selectedOldTariff.fNewClassifications = null;
			selectedOldTariff.RefreshBindingIncludingChildren();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "when filter pushed CC over threshold. This method will need to be refactored in another WI.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Baseline")]
		public void ApplyPendingTariffChanges()
		{
			int maxRows = 100;
#if DEBUG
			maxRows = 2;
#endif
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZGuid lastProcessedPK = ZGuid.Empty;
			int lastCount = 100;
			int fails = 0;
			while (lastCount > 0)
			{
				ZGuid previousLastProcessedPK = lastProcessedPK;
				ZDBOnlyQuery filter1 = new ZDBOnlyQuery(typeof(TBCClassification));
				filter1.OrderBy = CusClassificationSchema.PK.Name + " ASC";
				filter1.MaximumRows = maxRows;
				filter1.AddToFilter(CusClassificationSchema.PK, SQLComparisonOperator.GreaterThan, lastProcessedPK);
				filter1.AddToFilter(CusClassificationSchema.CC_ClassificationType, lookupType);
				filter1.AddToFilter(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				filter1.AddToFilter(CusClassificationSchema.CC_TariffChangePending, true);
				var classifications = GetTBCClassificationCollection(factory);
				classifications.Load(filter1);
				foreach (TBCClassification @class in classifications)
				{
					if (!@class.NewLookupCode.IsEmpty && @class.CC_LookupCode != @class.NewLookupCode)
					{
						@class.CC_LookupCode = @class.NewLookupCode;
						if (@class.CC_LookupCodeInfo.HasErrors())
						{
							@class.CC_LookupCode = (ZString)@class.CC_LookupCodeInfo.OriginalValue;
						}
					}

					var unformattedNewtariffNum = CurrentTariffFormatter.Format(@class.NewTariffNum);
					if (!unformattedNewtariffNum.IsEmpty && @class.CC_TariffNum != unformattedNewtariffNum)
					{
						@class.CC_TariffNum = unformattedNewtariffNum;
					}
					@class.CC_TariffChangePending = false;
					LogsForNominatedEvent fTBCChangeDataLogs = new LogsForNominatedEvent(@class.Logs, ChangeDataEvent);
					if (fTBCChangeDataLogs != null)
					{
						fTBCChangeDataLogs.CancelAll();
					}
					lastProcessedPK = @class.PK;
				}
				try
				{
					factory.Save();
					fails = 0;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					fails++;
					if (fails < 3)
					{
						lastProcessedPK = previousLastProcessedPK;
					}
					else
					{
						fails = 0;
					}
				}
				lastCount = classifications.Count;
				factory = new BusinessObjectFactory();
			}

			lastProcessedPK = ZGuid.Empty;
			lastCount = 100;
			fails = 0;
			while (lastCount > 0)
			{
				var previousLastProcessedPK = lastProcessedPK;
				var filter = new ZDBOnlyQuery(typeof(BaseCusClassPartPivot));
				filter.OrderBy = CusClassPartPivotSchema.PK.Name + " ASC";
				filter.MaximumRows = maxRows;
				filter.AddToFilter(CusClassPartPivotSchema.PK, SQLComparisonOperator.GreaterThan, lastProcessedPK);
				filter.AddToFilter(CusClassPartPivotSchema.CI_RN_NKCountry, CountryCode);
				filter.AddToFilter(CusClassPartPivotSchema.CI_TariffChangePending, true);

				var pivots = new BaseCusClassPartPivotCollection(factory);
				pivots.Load(filter);
				bool pivotUpdated;
				foreach (BaseCusClassPartPivot pivot in pivots)
				{
					pivotUpdated = false;
					if (!pivot.NewLookUpPK.IsEmpty)
					{
						var classification = pivot.NewClassification;
						if (classification != null && classification.CC_ClassificationType == lookupType)
						{
							pivotUpdated = true;
							pivot.CI_CC = pivot.NewLookUpPK;
							pivot.CI_TariffNum = ZString.Empty;
						}
					}
					else if (!pivot.NewTariffNum.IsEmpty && ValidPivotTypes.Contains(pivot.CI_ChildType))
					{
						pivotUpdated = true;
						pivot.CI_TariffNum = CurrentTariffFormatter.Format(pivot.NewTariffNum);
						pivot.CI_CC = ZGuid.Empty;
					}
					if (pivotUpdated)
					{
						pivot.CI_TariffChangePending = false;
						var fTBCChangeDataLogs = new LogsForNominatedEvent(pivot.Logs, ChangeDataEvent);
						if (fTBCChangeDataLogs != null)
						{
							fTBCChangeDataLogs.CancelAll();
						}
					}
					lastProcessedPK = pivot.PK;
				}
				try
				{
					factory.Save();
					fails = 0;
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					fails++;
					if (fails < 3)
					{
						lastProcessedPK = previousLastProcessedPK;
					}
					else
					{
						fails = 0;
					}
				}
				lastCount = pivots.Count;
				factory = new BusinessObjectFactory();
			}
		}

		#endregion

		#region properties

		protected virtual bool IsPivotTariffNumSupported => false;

		protected virtual ZString[] ValidPivotTypes => Array.Empty<ZString>();

		public virtual TariffBulkChangeOldTariffCollection TariffBulkChangeOldTariffs
		{
			get
			{
				if (fTariffBulkChangeOldTariffs == null)
				{
					fTariffBulkChangeOldTariffs = new TariffBulkChangeOldTariffCollection(this);
					RegisterEditableChildObject(fTariffBulkChangeOldTariffs);
				}
				return fTariffBulkChangeOldTariffs;
			}
		}
		TariffBulkChangeOldTariffCollection fTariffBulkChangeOldTariffs;

		#endregion

		protected virtual ITBCClassificationCollection<TBCClassification> GetTBCClassificationCollection(BusinessObjectFactory factory) => new TBCClassificationCollection<TBCClassification>(factory);

		protected virtual ITBCClassificationCollection<TBCClassification> GetTBCClassificationCollection(BusinessObjectFactory factory, ZQuery filter) => new TBCClassificationCollection<TBCClassification>(factory, filter);

		public class TariffBulkChangeOldTariff : NonPersistentBusinessObject, IObsoleteValidation
		{
			#region Schema

			public static class Schema
			{
				public const string OldTariffNum = "OldTariffNum";
			}

			#endregion

			public TariffBulkChangeOldTariff()
			{
			}

			public TariffBulkChangeOldTariff(TariffBulkChange parent)
				: base(parent.Factory)
			{
				this.parent = parent;
			}

			readonly TariffBulkChange parent;

			#region Properties

			[MaxLength(15)]
			public ZString OldTariffNum
			{
				get { return fOldTariffNum; }
				set
				{
					fOriginalClassifications = null;
					fTariffItemPivots = null;
					fNewClassifications = null;
					CheckMaximumLength(OldTariffNumInfo, value);
					if (parent != null)
					{
						SetNonPersistentPropertyValue(OldTariffNumInfo, ref fOldTariffNum, parent.CurrentTariffFormatter.DisplayFormat(value));
					}
					else
					{
						SetNonPersistentPropertyValue(OldTariffNumInfo, ref fOldTariffNum, value);
					}
					if (fOldTariffNum.Length <= 10)
					{
						fTariffNumNoStats = fOldTariffNum;
						fStatsCode = "";
					}
					else
					{
						fTariffNumNoStats = fOldTariffNum.Left(10);
						fStatsCode = fOldTariffNum.Right(2);
					}
				}
			}
			ZString fOldTariffNum;
			ZString fTariffNumNoStats;
			ZString fStatsCode;

			public ZPropertyInfo OldTariffNumInfo => GetZPropertyInfo(Schema.OldTariffNum);

			public ZString TariffNumNoStats => fTariffNumNoStats;

			public ZString StatsCode => fStatsCode;

			public ZString TariffSuffix;

			#endregion

			#region Collections

			public virtual TariffBulkChangeNewTariffCollection TariffBulkChangeNewTariffs
			{
				get
				{
					if (fTariffBulkChangeNewTariffs == null)
					{
						if (parent != null)
						{
							fTariffBulkChangeNewTariffs = new TariffBulkChangeNewTariffCollection(parent, this);
						}
						else
						{
							fTariffBulkChangeNewTariffs = new TariffBulkChangeNewTariffCollection(Factory);
						}
						RegisterEditableChildObject(fTariffBulkChangeNewTariffs);
					}
					return fTariffBulkChangeNewTariffs;
				}
			}
			TariffBulkChangeNewTariffCollection fTariffBulkChangeNewTariffs;

			public ITBCClassificationCollection<TBCClassification> OriginalClassifications
			{
				get
				{
					if (fOriginalClassifications == null)
					{
						if (!OldTariffNum.IsEmpty)
						{
							ZQuery filter = parent.GetClassificationQuery(OldTariffNum);
							foreach (TariffBulkChangeNewTariff newTariff in TariffBulkChangeNewTariffs)
							{
								if (newTariff.NewTariffNum == OldTariffNum)
								{
									fOriginalClassifications = parent?.GetTBCClassificationCollection(Factory);
									var dummyClassification = Factory.LoadTop1<BaseCusClassification>(filter);
									if (dummyClassification != null && dummyClassification.CC_FormattedTariffNum.Length > 10)
									{
										TariffSuffix = dummyClassification.CC_FormattedTariffNum.Right(2);
									}
									break;
								}
							}
							if (fOriginalClassifications == null)
							{
								fOriginalClassifications = parent?.GetTBCClassificationCollection(Factory, filter) ?? new TBCClassificationCollection<TBCClassification>(Factory, filter);
								fOriginalClassifications.Load();
								if (fOriginalClassifications.Count > 0 && fOriginalClassifications[0].CC_FormattedTariffNum.Length > 10)
								{
									TariffSuffix = fOriginalClassifications[0].CC_FormattedTariffNum.Right(2);
								}
							}
						}
						else
						{
							fOriginalClassifications = parent?.GetTBCClassificationCollection(Factory) ?? new TBCClassificationCollection<TBCClassification>(Factory);
						}
						for (int i = 0; i < fOriginalClassifications.Count; i++)
						{
							if (fOriginalClassifications[i].CC_TariffChangePending && !fOriginalClassifications[i].NewTariffNum.IsEmpty && fOriginalClassifications[i].NewTariffNum != OldTariffNum)
							{
								fOriginalClassifications.Remove(fOriginalClassifications[i]);
								i--;
							}
						}
						RegisterEditableChildObject(fOriginalClassifications);
					}
					return fOriginalClassifications;
				}
			}
			public ITBCClassificationCollection<TBCClassification> fOriginalClassifications;

			public ITBCClassificationCollection<TBCClassification> NewClassifications
			{
				get
				{
					if (fNewClassifications == null)
					{
						ZQuery filter;
						ITBCClassificationCollection<TBCClassification> tempClassifications;
						fNewClassifications = parent?.GetTBCClassificationCollection(Factory) ?? new TBCClassificationCollection<TBCClassification>(Factory);
						if (TariffBulkChangeNewTariffs != null)
						{
							foreach (TariffBulkChangeNewTariff newTariff in TariffBulkChangeNewTariffs)
							{
								if (!newTariff.NewTariffNum.IsEmpty)
								{
									filter = parent.GetClassificationQuery(newTariff.NewTariffNum);
									tempClassifications = parent?.GetTBCClassificationCollection(Factory, filter) ?? new TBCClassificationCollection<TBCClassification>(Factory, filter);
									tempClassifications.Load();
									fNewClassifications.AddRange(tempClassifications);
								}
							}
						}
						if (!OldTariffNum.IsEmpty)
						{
							filter = parent.GetClassificationQuery(OldTariffNum);
							filter.AddToFilter(CusClassificationSchema.CC_TariffChangePending, true);
							tempClassifications = parent?.GetTBCClassificationCollection(Factory, filter) ?? new TBCClassificationCollection<TBCClassification>(Factory, filter);
							tempClassifications.Load();
							for (int i = 0; i < tempClassifications.Count; i++)
							{
								if (tempClassifications[i].NewTariffNum.IsEmpty || tempClassifications[i].NewTariffNum == OldTariffNum)
								{
									tempClassifications.Remove(tempClassifications[i]);
									i--;
								}
							}
							fNewClassifications.AddRange(tempClassifications);
						}
						RegisterEditableChildObject(fNewClassifications);
					}
					return fNewClassifications;
				}
			}
			internal ITBCClassificationCollection<TBCClassification> fNewClassifications;

			public TariffBulkChangerPivotCollection TariffItemPivots
			{
				get
				{
					if (fTariffItemPivots == null)
					{
						fTariffItemPivots = new TariffBulkChangerPivotCollection(Factory);
						if (!OldTariffNum.IsEmpty)
						{
							fTariffItemPivots.Load(GetPivotQuery(OldTariffNum));
						}

						RegisterEditableChildObject(fTariffItemPivots);
					}
					return fTariffItemPivots;
				}
			}
			TariffBulkChangerPivotCollection fTariffItemPivots;

			ZQuery GetPivotQuery(ZString oldTariffNum)
			{
				ZQuery result = new ZQuery();
				result.IsNoResultQuery = true;
				if (!oldTariffNum.IsEmpty)
				{
					if (parent.IsPivotTariffNumSupported)
					{
						result.AddToFilter(parent.GetPivotTariffQuery(oldTariffNum));
						result.IsNoResultQuery = false;
					}
					var classificationQuery = parent.GetClassificationQuery(oldTariffNum);
					var classifications = Factory.Load<BaseCusClassification>(classificationQuery);
					if (classifications.Length > 0)
					{
						var classificationPKs = new ZGuid[classifications.Length];
						for (int i = 0; i < classifications.Length; i++)
						{
							classificationPKs[i] = classifications[i].PK;
						}
						if (parent.IsPivotTariffNumSupported)
						{
							result.AddToFilter(new ZQuery(CusClassPartPivotSchema.CI_CC, classificationPKs), JoinCondition.Or);
						}
						else
						{
							result.AddToFilter(CusClassPartPivotSchema.CI_CC, classificationPKs);
						}
						result.IsNoResultQuery = false;
					}
				}
				return result;
			}

			#endregion
		}

		public class TariffBulkChangeOldTariffCollection : NonPersistentBusinessObjectCollection<TariffBulkChangeOldTariff>, IObsoleteValidation
		{
			public TariffBulkChangeOldTariffCollection(TariffBulkChange parent)
				: base(parent.Factory)
			{
				this.parent = parent;
			}

			readonly TariffBulkChange parent;

			protected override BusinessObject CreateNonPersistentBusinessObject() => new TariffBulkChangeOldTariff(parent);
		}

		public class TariffBulkChangeNewTariff : NonPersistentBusinessObject, IObsoleteValidation
		{
			public static class Schema
			{
				public const string NewTariffNum = "NewTariffNum";
			}

			public TariffBulkChangeNewTariff()
			{
			}

			public TariffBulkChangeNewTariff(TariffBulkChange parent, TariffBulkChangeOldTariff oldTariff)
				: base(parent.Factory)
			{
				this.oldTariff = oldTariff;
				this.parent = parent;
			}

			readonly TariffBulkChangeOldTariff oldTariff;
			readonly TariffBulkChange parent;

			[MaxLength(15)]
			public ZString NewTariffNum
			{
				get { return fNewTariffNum; }
				set
				{
					if (oldTariff != null)
					{
						oldTariff.fNewClassifications = null;
					}
					CheckMaximumLength(NewTariffNumInfo, value);
					if (parent != null)
					{
						SetNonPersistentPropertyValue(NewTariffNumInfo, ref fNewTariffNum, parent.CurrentTariffFormatter.DisplayFormat(value));
					}
					else
					{
						SetNonPersistentPropertyValue(NewTariffNumInfo, ref fNewTariffNum, value);
					}
				}
			}
			ZString fNewTariffNum;

			public ZPropertyInfo NewTariffNumInfo => GetZPropertyInfo(Schema.NewTariffNum);
		}

		public class TariffBulkChangeNewTariffCollection : NonPersistentBusinessObjectCollection<TariffBulkChangeNewTariff>, IObsoleteValidation
		{
			public TariffBulkChangeNewTariffCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public TariffBulkChangeNewTariffCollection(TariffBulkChange parent, TariffBulkChangeOldTariff oldTariff)
				: base(parent.Factory)
			{
				this.oldTariff = oldTariff;
				this.parent = parent;
			}

			readonly TariffBulkChangeOldTariff oldTariff;
			readonly TariffBulkChange parent;

			protected override BusinessObject CreateNonPersistentBusinessObject() => new TariffBulkChangeNewTariff(parent, oldTariff);
		}

		public class TariffBulkChangerPivotCollection : BaseCusClassPartPivotCollection
		{
			public TariffBulkChangerPivotCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void OnAdded(BusinessObject bizOAdded)
			{
				base.OnAdded(bizOAdded);
				var part = ((BaseCusClassPartPivot)bizOAdded).Part;
				if (part != null)
				{
					part.RemoveNonEssentialValidationForBulkTariffUpdate = true;
				}
			}

			protected override bool AllowNewCore => false;
		}

		public interface ITBCClassificationCollection<out T> : IBusinessObjectCollection<T> where T : TBCClassification
		{
			void Load(ZQuery filter);
			new void Remove(BusinessObject businessObject);
		}

		public class TBCClassificationCollection<T> : BaseClassificationCollection<T>, ITBCClassificationCollection<T> where T : TBCClassification
		{
			public TBCClassificationCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public TBCClassificationCollection(BusinessObjectFactory factory, ZQuery filter)
				: base(factory, filter)
			{
			}

			protected override bool AllowNewCore => false;
		}

		public class TBCClassification : BaseCusClassification
		{
			public TBCClassification(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new class Schema : BaseCusClassification.Schema
			{
				public const string NewTariffNum = "NewTariffNum";
				public const string NewLookupCode = "NewLookupCode";
			}

			[MaxLength(BaseCusClassification.Schema.CC_TariffNumMaxLength)]
			public ZString NewTariffNum
			{
				get
				{
					return TBCClassManager.NewTariffNum;
				}
				set
				{
					ZString formattedTariffNum = CurrentTariffFormatter.DisplayFormat(value);
					if (formattedTariffNum.IsEmpty)
					{
						formattedTariffNum = value;
					}
					CheckMaximumLength(NewTariffNumInfo, formattedTariffNum);
					TBCClassManager.NewTariffNum = formattedTariffNum;
					NewTariffNumInfo.RefreshBinding();
					((TBCClassificationValidation)Validation).ValidateNewTariffNum();
				}
			}

			public ZPropertyInfo NewTariffNumInfo => GetZPropertyInfo(Schema.NewTariffNum);

			[MaxLength(BaseCusClassification.Schema.CC_LookupCodeMaxLength)]
			public ZString NewLookupCode
			{
				get
				{
					return TBCClassManager.NewLookupCode;
				}
				set
				{
					CheckMaximumLength(NewLookupCodeInfo, value);
					TBCClassManager.NewLookupCode = value;
					NewLookupCodeInfo.RefreshBinding();
					((TBCClassificationValidation)Validation).ValidateNewLookupCode();
				}
			}

			public ZPropertyInfo NewLookupCodeInfo => GetZPropertyInfo(Schema.NewLookupCode);

			public TBCClassManager TBCClassManager
			{
				get
				{
					if (fTBCClassManager == null)
					{
						fTBCClassManager = new TBCClassManager(this);
					}
					return fTBCClassManager;
				}
			}
			TBCClassManager fTBCClassManager;

			protected override CusClassificationValidation GetNewValidation() => new TBCClassificationValidation(this);

			protected override void RunPreSaveValidationCore()
			{
				base.RunPreSaveValidationCore();
				((TBCClassificationValidation)Validation).ValidateNewTariffNum();
				((TBCClassificationValidation)Validation).ValidateNewLookupCode();
			}

			public override void OnSaving()
			{
				if (!hasSetConcurrencyPolicy && IsInDatabase)
				{
					hasSetConcurrencyPolicy = true;
					SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				}
				base.OnSaving();
			}
			bool hasSetConcurrencyPolicy;
		}

		public class TBCClassificationValidation : CusClassificationValidation
		{
			public TBCClassificationValidation(TBCClassification parent)
				: base(parent)
			{
			}

			public new TBCClassification Parent => (TBCClassification)base.Parent;

			public void ValidateNewTariffNum()
			{
				ValidateCalculatedProperty(Parent.NewTariffNumInfo);
			}

			public void ValidateNewLookupCode()
			{
				ValidateCalculatedProperty(Parent.NewLookupCodeInfo);
				if (!Parent.NewLookupCode.IsEmpty && Parent.NewLookupCode != Parent.CC_LookupCode)
				{
					var filter = new ZQuery(CusClassificationSchema.CC_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
					filter.AddToFilter(CusClassificationSchema.CC_ClassificationType, Parent.CC_ClassificationType);
					filter.AddToFilter(CusClassificationSchema.CC_LookupCode, Parent.NewLookupCode);
					if (Parent.Factory.Exists(typeof(TBCClassification), filter))
					{
						Parent.NewLookupCodeInfo.AddError(Res.GetString("9a9d4438-f957-4fd1-974e-a406f862b6a1", "There is already a record with Lookup Code {0}", Parent.NewLookupCode));
					}
				}
			}
		}
	}
}
