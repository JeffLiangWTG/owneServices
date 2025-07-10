using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ComparisonQuoteResultCollection : NonPersistentBusinessObjectCollection<ComparisonQuoteResult>
	{
		#region Constructors

		public ComparisonQuoteResultCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Properties

		public ComparisonQuoteResult Selected
		{
			get
			{
				foreach (ComparisonQuoteResult result in this)
				{
					if (result.Selected)
					{
						return result;
					}
				}
				return null;
			}
		}

		public ComparisonQuoteResultCollection ServiceLevelSpecific(ZString serviceLevel)
		{
			ComparisonQuoteResultCollection result = new ComparisonQuoteResultCollection(Factory);

			foreach (ComparisonQuoteResult quoteResut in this)
			{
				if (quoteResut.ServiceLevel == serviceLevel)
				{
					result.Add(quoteResut);
				}
			}

			return result;
		}

		public ComparisonQuoteResultCollection ModeSpecific(ZString mode)
		{
			ComparisonQuoteResultCollection result = new ComparisonQuoteResultCollection(Factory);

			foreach (ComparisonQuoteResult quoteResut in this)
			{
				if (quoteResut.Mode == mode)
				{
					result.Add(quoteResut);
				}
			}

			return result;
		}

		public List<ZString> Modes
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (ComparisonQuoteResult quoteResult in this)
				{
					if (!result.Contains(quoteResult.Mode))
					{
						result.Add(quoteResult.Mode);
					}
				}
				return result;
			}
		}

		public List<ZString> ServiceLevels
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (ComparisonQuoteResult quoteResult in this)
				{
					if (!result.Contains(quoteResult.ServiceLevel))
					{
						result.Add(quoteResult.ServiceLevel);
					}
				}
				return result;
			}
		}

		#endregion

		#region Methods

		public void CleanZeroChargesAndResults()
		{
			for (int i = 0; i < this.Count; i++)
			{
				ComparisonQuoteResult quoteResult = this[i];
				if (quoteResult.Charges.TotalOSSellAmount == 0)
				{
					this.Remove(quoteResult);
					i--;
				}
				else
				{
					quoteResult.Charges.RemoveZeroValueCharges();
				}
			}
		}

		public ComparisonQuoteResult GetNextUnprocessedItem()
		{
			foreach (ComparisonQuoteResult result in this)
			{
				if (!result.IsProcessed)
				{
					return result;
				}
			}
			return null;
		}

		public void ClearProcessedFlag()
		{
			foreach (ComparisonQuoteResult result in this)
			{
				result.IsProcessed = ZBool.False;
			}
		}

		public bool Contains(ComparisonQuoteResult result)
		{
			foreach (ComparisonQuoteResult existingResult in this)
			{
				if (existingResult.Mode == result.Mode &&
					existingResult.ServiceLevel == result.ServiceLevel &&
					existingResult.CarrierPK == result.CarrierPK)
				{
					return true;
				}
			}
			return false;
		}

		public void Add(ComparisonQuoteResult result)
		{
			Add((BusinessObject)result);
		}

		#endregion

		#region Overrides

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComparisonQuoteResult(Factory);
		}

		#endregion
	}
}
