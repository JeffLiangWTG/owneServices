using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// CusEntryHeader.IsActive is the filter
	/// </summary>
	public class ActiveCusEntryHeaderCollection : BusinessObjectCollectionView<CusEntryHeader>
	{
		public ActiveCusEntryHeaderCollection(BaseJobDeclaration declaration)
			: base(declaration.CustomsEntryHeaders as BusinessObjectCollection)
		{
			this.declaration = declaration;
			Rebuild();
		}

		protected readonly BaseJobDeclaration declaration;

		protected override void RebuildOnConstruction()
		{
			//will be manually rebuilt due to NZ requiring something from declaration to filter
		}

		public ZBool AreAllEntriesCleared
		{
			get
			{
				if (areAllEntriesClearedCached == null)
				{
					areAllEntriesClearedCached = new CachedProperty<ZBool>(Factory, delegate
					{
						var result = Count > 0;
						if (this.Cast<CusEntryHeader>().Any(entry => !entry.IsClearedEntry))
						{
							result = false;
						}
						return result;
					});
				}
				return areAllEntriesClearedCached.Value;
			}
		}
		CachedProperty<ZBool> areAllEntriesClearedCached;

		public void DeactivateAll()
		{
			foreach (CusEntryHeader entry in this.ToArray())
			{
				entry.IsActive = false;
			}
		}

		public bool HasAnEntryWithEntryNumber
		{
			get
			{
				foreach (CusEntryHeader entry in this)
				{
					if (!entry.EntryNumber.IsEmpty)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool HasEntriesWithNonAmendableChanges
		{
			get
			{
				foreach (CusEntryHeader entry in this)
				{
					if (entry.HasNonAmendableChanges)
					{
						return true;
					}
				}
				return false;
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((CusEntryHeader)element).IsActive;
		}

		public ZString BGMReferencesAsCommaDelimitedString
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();

				foreach (CusEntryHeader entry in this)
				{
					result.AppendIfNotEmpty(entry.CH_BGMReference);
				}

				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}
	}
}
