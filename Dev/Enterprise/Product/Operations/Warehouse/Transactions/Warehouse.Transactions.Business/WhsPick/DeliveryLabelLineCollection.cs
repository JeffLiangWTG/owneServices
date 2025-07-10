using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	[ExcludeFromOverriddenAddNewTest]
	public class DeliveryLabelLineCollection : WhsDocketLabelLineCollection
	{
		#region Constructors

		public DeliveryLabelLineCollection(WhsOrder order, BusinessObjectFactory factory)
			: base(factory)
		{
			if (order != null)
			{
				AddTotals(order);
			}
		}

		public DeliveryLabelLineCollection(WhsPickableDocketCollection pickableDocketCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (pickableDocketCollection != null)
			{
				foreach (WhsPickableDocket pickableDocket in pickableDocketCollection)
				{
					if (pickableDocket is WhsOrder)
					{
						AddTotals(pickableDocket);
					}
				}
			}
		}

		public DeliveryLabelLineCollection(WhsLegacyPickableDocketCollection pickableDocketCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			if (pickableDocketCollection != null)
			{
				foreach (WhsPickableDocket pickableDocket in pickableDocketCollection)
				{
					if (pickableDocket is WhsOrder)
					{
						AddTotals(pickableDocket);
					}
				}
			}
		}

		#endregion

		void AddTotals(WhsPickableDocket docket)
		{
			int totalLabels = 0;
			totalLabels = GetTotalLabelCountWithFallback(docket);
			Add(new WhsDocketLabelLine(docket, totalLabels, totalLabels));
		}

		int GetTotalLabelCountWithFallback(WhsPickableDocket docket)
		{
			if (!docket.WD_PalletsSent.IsEmpty)
			{
				return docket.WD_PalletsSent;
			}
			else if (!docket.WD_PackagesSent.IsEmpty)
			{
				return docket.WD_PackagesSent;
			}
			else
			{
				return (int)docket.WD_UnitsSent;
			}
		}
	}
}
