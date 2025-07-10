using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business
{
	public class ActiveCusEntryHeaderCollection : Customs.Business.ActiveCusEntryHeaderCollection
	{
		public ActiveCusEntryHeaderCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.declaration; }
		}

		public new CusEntryHeader this[int index]
		{
			get { return (CusEntryHeader)base[index]; }
		}

		public new CusEntryHeader AddNew()
		{
			return (CusEntryHeader)base.AddNew();
		}

		public ZString EntryNumbersAsCommaDelimitedString
		{
			get
			{
				var result = new ZStringBuilder();

				foreach (CusEntryHeader entry in this)
				{
					result.AppendIfNotEmpty(entry.EntryNumber);
				}

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		internal IEnumerable<CusEntryLine> FindEntryLinesByMasterBill(ZGuid masterBill)
		{
			return this.Cast<CusEntryHeader>().SelectMany(header => header.MergedLines.Cast<CusEntryLine>()).Where(line => line.IsChildOf(masterBill)).OrderBy(x => x.CL_LineNumber);
		}

		public IEnumerable<CusEntryHeader> GetEntriesAboutToBeDiscarded()
		{
			return this.Cast<CusEntryHeader>().Where(entry => !Declaration.IsRelevantFor(entry.CH_MessageType));
		}

		public ZString InBondNumber
		{
			get { return InBondEntry != null ? InBondEntry.EntryNumber : ZString.Empty; }
		}

		public CusEntryHeader InBondEntry
		{
			get
			{
				if (inBondEntryCached == null)
				{
					inBondEntryCached = new CachedProperty<CusEntryHeader>(Factory, () => this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsInBond));
				}

				return inBondEntryCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> inBondEntryCached;

		public ZBool HasTemporaryImportationBond
		{
			get
			{
				if (hasTemporaryImportationBondCached == null)
				{
					hasTemporaryImportationBondCached = new CachedProperty<ZBool>(Factory, () => this.Cast<CusEntryHeader>().Any(entry => entry.IsTemporaryImportationBond));
				}
				return hasTemporaryImportationBondCached.Value;
			}
		}
		CachedProperty<ZBool> hasTemporaryImportationBondCached;

		public ZDecimal EntySummaryTotalAmountPayable
		{
			get
			{
				if (entySummaryTotalAmountPayableCached == null)
				{
					entySummaryTotalAmountPayableCached = new CachedProperty<ZDecimal>(Factory, delegate
						{
							ZDecimal result = ZDecimal.Zero;

							foreach (CusEntryHeader entry in this)
							{
								if (entry.IsFormalEntry)
								{
									result += entry.CH_TotalPaid;
								}
							}
							return result;
						});
				}
				return entySummaryTotalAmountPayableCached.Value;
			}
		}
		CachedProperty<ZDecimal> entySummaryTotalAmountPayableCached;

		public ZDecimal EntySummaryEnteredValue
		{
			get
			{
				if (entySummaryEnteredValueCached == null)
				{
					entySummaryEnteredValueCached = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = ZDecimal.Zero;

						foreach (CusEntryHeader entry in this)
						{
							if (entry.IsFormalEntry)
							{
								result += entry.CustomsValue;
							}
						}
						return result;
					});
				}
				return entySummaryEnteredValueCached.Value;
			}
		}
		CachedProperty<ZDecimal> entySummaryEnteredValueCached;

		public ZBool HasEntriesCargoReleaseBeingCertified
		{
			get
			{
				if (hasEntriesCargoReleaseBeingCertifiedCached == null)
				{
					hasEntriesCargoReleaseBeingCertifiedCached = new CachedProperty<ZBool>(Factory, () => this.Cast<CusEntryHeader>().Any(entry => entry.IsCargoReleaseBeingCertified));
				}

				return hasEntriesCargoReleaseBeingCertifiedCached.Value;
			}
		}
		CachedProperty<ZBool> hasEntriesCargoReleaseBeingCertifiedCached;

		public CusEntryHeader[] GetEntryWithType(ImportMessageStatusList.MessageType messageType)
		{
			return this.Cast<CusEntryHeader>().Where(entry => entry.IsRelevantFor(messageType)).ToArray();
		}

		public ZBool HasAtLeastOneEntryWithActiveMessages
		{
			get
			{
				if (hasAtLeastOneEntryWithActiveMessagesCached == null)
				{
					hasAtLeastOneEntryWithActiveMessagesCached = new CachedProperty<ZBool>(Factory, () => this.Cast<CusEntryHeader>().Any(entry => entry.HasActiveTransactionsWithCustoms));
				}
				return hasAtLeastOneEntryWithActiveMessagesCached.Value;
			}
		}
		CachedProperty<ZBool> hasAtLeastOneEntryWithActiveMessagesCached;

		public ZBool HaveAtLeastOneEntryWaitingForResponse
		{
			get
			{
				if (haveAtLeastOneEntryWaitingForResponseCached == null)
				{
					haveAtLeastOneEntryWaitingForResponseCached = new CachedProperty<ZBool>(Factory, () => this.Cast<CusEntryHeader>().Any(entry => entry.IsWaitingForResponse));
				}
				return haveAtLeastOneEntryWaitingForResponseCached.Value;
			}
		}
		CachedProperty<ZBool> haveAtLeastOneEntryWaitingForResponseCached;

		public CusEntryHeader FTZEntry
		{
			get
			{
				if (fTZEntryCached == null)
				{
					fTZEntryCached = new CachedProperty<CusEntryHeader>(Factory, () => this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsFTZAdmission));
				}
				return fTZEntryCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> fTZEntryCached;

		public CusEntryHeader EntrySummaryEntry
		{
			get
			{
				if (entrySummaryEntryCached == null)
				{
					entrySummaryEntryCached = new CachedProperty<CusEntryHeader>(Factory, () => this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsFormalEntry));
				}
				return entrySummaryEntryCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> entrySummaryEntryCached;

		public CusEntryHeader SimplifiedEntry
		{
			get
			{
				if (simplifiedEntryCached == null)
				{
					simplifiedEntryCached = new CachedProperty<CusEntryHeader>(Factory, () => this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsACECargoRelease));
				}
				return simplifiedEntryCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> simplifiedEntryCached;

		/// <summary>
		/// Cargo Release or Border Cargo Release entries
		/// </summary>
		public CusEntryHeader CargoReleaseEntry
		{
			get { return this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsCargoRelease || entry.IsBorderCargoRelease); }
		}

		/// <summary>
		/// If an entry with 'ENS' type exists, this returns the entry. 
		/// Otherwise it returns a related entry like CargoRelease or BorderCargoRelease entry
		/// </summary>
		public CusEntryHeader EntryHeaderWithENSEntryNumber
		{
			get
			{
				if (entryHeaderWithENSEntryNumberCached == null)
				{
					entryHeaderWithENSEntryNumberCached = new CachedProperty<CusEntryHeader>(Factory, delegate
						{
							CusEntryHeader result = EntrySummaryEntry;

							if (result == null)
							{
								foreach (CusEntryHeader entry in this)
								{
									if (entry.IsRelatedToENSEntry)
									{
										result = entry;
										break;
									}
								}
							}

							return result;
						});
				}

				return entryHeaderWithENSEntryNumberCached.Value;
			}
		}
		CachedProperty<CusEntryHeader> entryHeaderWithENSEntryNumberCached;

		public CusEntryHeader ReconciliationEntry
		{
			get { return this.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.IsReconEntry); }
		}

		public List<ZGuid> GetPKs(ZString cH_MessageType)
		{
			return (from CusEntryHeader entry in this
					where entry.CH_MessageType == cH_MessageType
					select entry.PK).ToList();
		}
	}
}
