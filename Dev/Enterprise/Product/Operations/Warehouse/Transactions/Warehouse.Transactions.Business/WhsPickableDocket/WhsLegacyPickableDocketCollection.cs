using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	// Please do not use this collection as we will be deleting this in Work Item WI00211486
	public class WhsLegacyPickableDocketCollection : BusinessObjectCollection<WhsPickableDocket>, IOrdersDocumentSupport
	{
		public WhsLegacyPickableDocketCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsLegacyPickableDocketCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return base.CreateRelationshipFilter().AddToFilter(WhsDocketSchema.WD_DocketType,
				new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
		}

		#region AllowNew

		public void SetAllowNew(bool value)
		{
			allowNew = value;
		}

		protected sealed override bool AllowNewCore
		{
			get { return allowNew.HasValue && allowNew.Value; }
		}
		bool? allowNew;

		#endregion

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			Type bizOType = bizOAdded.GetType();
			foreach (WhsPickableDocket pickableDocket in this)
			{
				if (pickableDocket.GetType() != bizOType)
				{
					throw new ArgumentException("WhsLegacyPickableDocketCollection cannot contain a mix of both Orders and WorkOrders.");
				}
			}
			base.OnAdded(bizOAdded);
		}

		public bool ContainsWorkOrders
		{
			get
			{
				foreach (WhsPickableDocket pickableDocket in this)
				{
					return pickableDocket is WhsComponentOrder;  // Only need to check the first one according to the OnAdded rule above.
				}
				return false;
			}
		}

		#region IOrdersDocumentSupport Members

		DocumentWrapper[] IOrdersDocumentSupport.GetPackageLabelDocumentWrappers()
		{
			if (!this.ContainsWorkOrders)
			{
				List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
				foreach (WhsOrder order in this)
				{
					WhsDocketLabelControl legacyDocketLabelControl = new WhsDocketLabelControl(order, order.WD_PackagesSent);
					wrappers.AddRange(new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsOrder, legacyDocketLabelControl) });
				}
				return wrappers.ToArray();
			}
			else
			{
				return Array.Empty<DocumentWrapper>();
			}
		}

		DocumentWrapper[] IOrdersDocumentSupport.GetDeliveryLabelDocumentWrappers(Action<object, WhsOrderToPrintEventArgs> onPrintEvent)
		{
			if (!this.ContainsWorkOrders)
			{
				List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
				DeliveryLabelLineCollection deliveryLabelLines = new DeliveryLabelLineCollection(this, Factory);
				WhsDocketsLabelControl ordersLabelControl = new WhsDocketsLabelControl(deliveryLabelLines);
				WhsOrderToPrintEventArgs eventArgs = new WhsOrderToPrintEventArgs(ordersLabelControl, Core.Constants.DataContext.WhsDeliveryLabels);
				onPrintEvent(this, eventArgs);
				if (eventArgs.ContinueToPrint)
				{
					foreach (WhsDocketLabelLine line in eventArgs.DocketsLabelControl.Lines)
					{
						if (line.NumberOfLabelsToPrint > 0)
						{
							wrappers.AddRange(new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsOrder, line.LegacyDocketLabelControl) });
						}
					}
				}
				return wrappers.ToArray();
			}
			else
			{
				return Array.Empty<DocumentWrapper>();
			}
		}

		#endregion
	}
}
