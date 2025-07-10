using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business
{
	public class LandedCostHistoryCollection : ActiveBusinessObjectCollection<LandedCostHistory>
	{
		internal class LCLineComparer : PropertyComparer
		{
			public override bool Equals(object obj)
			{
				bool result = false;

				if (obj.GetType() == typeof(LCLineComparer))
				{
					LCLineComparer passedComparer = (LCLineComparer)obj;
					result = passedComparer.PropertyDescriptor == this.PropertyDescriptor &&
						passedComparer.Direction == this.Direction;
				}
				return result;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required because Equals is overridden")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}

			public LCLineComparer(PropertyDescriptor propertyDescriptor, ListSortDirection direction) : base(propertyDescriptor, direction)
			{
			}

			public override int Compare(BusinessObject x, BusinessObject y)
			{
				int result = 0;

				if (PropertyDescriptor.DisplayName == LandedCostHistory.Schema.HumanReadableUltimateDistributeeCode)
				{
					LandedCostHistory lCLineX = x as LandedCostHistory;
					LandedCostHistory lCLineY = y as LandedCostHistory;

					if (lCLineX != null && lCLineY != null)
					{
						IUltimateDistributee distributeeX = lCLineX.UltimateDistributee;
						IUltimateDistributee distributeeY = lCLineY.UltimateDistributee;

						if (distributeeX != null && distributeeY != null)
						{
							ZPropertyInfo[] infosToSortByX = distributeeX.HumanReadableCodeInfos;
							ZPropertyInfo[] infosToSortByY = distributeeY.HumanReadableCodeInfos;

							for (int index = 0; index < infosToSortByX.Length; index++)
							{
								try
								{
									result = infosToSortByX[index].Value.CompareTo(infosToSortByY[index].Value);
								}
								catch (ArgumentException ex)
								{
									ErrorReporter.ReportOnce(
										string.Format("\r\nUnable to compare array elements:\r\nElement 1 - Value: {0}, Type: {1}\r\nElement 2 - Value: {2}, Type: {3}\r\n",
										infosToSortByX[index].Value,
										infosToSortByX[index].Value.GetType().FullName,
										infosToSortByY[index].Value,
										infosToSortByY[index].Value.GetType().FullName),
										ex
									);
									throw;
								}
								if (result != 0)
								{
									break;
								}
							}
							result = Direction == ListSortDirection.Ascending ? result : -result;
						}
					}
				}
				else
				{
					result = base.Compare(x, y);
				}

				return result;
			}
		}

		public LandedCostHistoryCollection(LandedCostHeader lCHeader) : base(lCHeader)
		{
			this.LCHeader = lCHeader;
		}

		public LandedCostHistory Get(IUltimateDistributee ultimateDistributee)
		{
			foreach (LandedCostHistory lCHistory in this)
			{
				if (lCHistory.LH_ParentID == ultimateDistributee.PK && lCHistory.LH_ParentTableCode == ultimateDistributee.TableCode)
				{
					return lCHistory;
				}
			}
			return null;
		}

		public LandedCostHistory GetOrCreate(IUltimateDistributee ultimateDistributee)
		{
			LandedCostHistory result = Get(ultimateDistributee);

			if (result == null)
			{
				result = AddNew();
				result.LH_ParentID = ultimateDistributee.PK;
				result.LH_ParentTableCode = ultimateDistributee.TableCode;

#if DEBUG
				result.UltimateDistributee = ultimateDistributee;
#endif
				result.LH_OP = ultimateDistributee.FKToProduct;

				result.DefaultDutyRateFromHeaderIfNecessary();
			}

			return result;
		}

		#region Calculated

		public ZDecimal TotalInvoiceCost
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandedCostHistory lCHistory in this)
				{
					result += lCHistory.InvoiceCostInLocalCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalLinePrice
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandedCostHistory lCHistory in this)
				{
					result += lCHistory.LinePriceInInvoiceCurrency;
				}
				return result;
			}
		}

		public ZDecimal TotalCost
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandedCostHistory lCHistory in this)
				{
					result += lCHistory.TotalCost;
				}
				return result;
			}
		}

		public ZDecimal TotalCostWithMarkup1Applied
		{
			get
			{
				ZDecimal result = 0m;
				foreach (LandedCostHistory lCHistory in this)
				{
					result += lCHistory.TotalCostWithMarkup1Applied;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		protected readonly LandedCostHeader LCHeader;

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(LandedCostHistorySchema.LH_LT, LCHeader.PK);
			return result;
		}

		protected override void SetDefaultsForNewElementCore(LandedCostHistory newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.LH_LandedCostHistoryLineType = LCHeader.LT_LandedCostType;
			newElement.LH_RN_NKCountryOfEntry = LCHeader.Company.GC_RN_NKCountryCode;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy()
		{
			return new LandedCostHistoryCollectionFetchStrategy(this);
		}

		#endregion

		#region GetComparerForSort

		protected override IComparer GetSortComparerForProperty(PropertyDescriptor property, ListSortDirection direction)
		{
			return new LCLineComparer(property, direction);
		}

		#endregion
	}
}
