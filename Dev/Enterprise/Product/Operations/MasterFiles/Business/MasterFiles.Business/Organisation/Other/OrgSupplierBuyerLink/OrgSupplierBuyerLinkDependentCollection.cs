using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public abstract class OrgSupplierBuyerLinkDependentCollection : DependentBusinessObjectCollection<OrgSupplierBuyerLink, OrgHeader>
	{
		protected OrgSupplierBuyerLinkDependentCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public const int DefaultExpectedShipmentMonths = 3;

		public void SetNewDateOnFirstExpectedShipment()
		{
			foreach (OrgSupplierBuyerLink link in this)
			{
				if (link.SelectedForPrinting)
				{
					link.OL_RoutingOrderPrinted = ZDateTime.Today;

					if (link.OL_InitialShipmentExpected.IsEmpty)
					{
						ZInt months;
						ZInt.TryParse(link.ExpectedShipmentMonthsAddition, out months);
						link.OL_InitialShipmentExpected = ZDateTime.Today.AddMonths(months);
					}
				}
			}

			Factory.Save();
		}

		public OrgSupplierBuyerLink FindFirstRelationshipWithError()
		{
			RunPreSaveValidation();
			OrgSupplierBuyerLink result = null;

			foreach (OrgSupplierBuyerLink link in this)
			{
				if (link.HasErrors)
				{
					result = link;
					break;
				}
			}

			return result;
		}

		public void ResetExpectedShipmentMonths()
		{
			foreach (OrgSupplierBuyerLink link in this)
			{
				link.ResetExpectedShipmentMonthsAddition();
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			OrgSupplierBuyerLink link = (OrgSupplierBuyerLink)child;
			OrgSupBuyLinkTrnMode linkMode = link.OrgSupBuyLinkTrnModes[0]; //should always exist

			linkMode.PF_IncoTerm = GetDefaultIncoTerm();
		}

		protected abstract ZString GetDefaultIncoTerm();
	}

	#region OrgSupplierBuyerLinkCollection Read Only View

	/// <summary>
	/// A view of an OrgSupplierBuyerLinkCollection.
	/// All elements in the collection also exist in the view.
	/// The view is only created to mark the collection as readonly for GUI binding purposes.
	/// </summary>
	public class OrgSupplierBuyerLinkCollectionReadOnlyView : BusinessObjectCollectionView<OrgSupplierBuyerLink>
	{
		public OrgSupplierBuyerLinkCollectionReadOnlyView(BusinessObjectCollection collection)
			: base(collection)
		{
			foreach (OrgSupplierBuyerLink link in collection)
			{
				if (link.OL_InitialShipmentExpected.IsEmpty)
				{
					link.UpdateShipmentDate = true;
					link.UpdateShipmentDate_ReadOnly = false;
				}
				else
				{
					link.UpdateShipmentDate = false;
					link.UpdateShipmentDate_ReadOnly = true;
				}

				link.ResetExpectedShipmentMonthsAddition();
			}
		}

		public OrgSupplierBuyerLinkDependentCollection SupplierBuyerCollection
		{
			get { return (OrgSupplierBuyerLinkDependentCollection)base.CollectionToFilter; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}
	}

	#endregion
}
