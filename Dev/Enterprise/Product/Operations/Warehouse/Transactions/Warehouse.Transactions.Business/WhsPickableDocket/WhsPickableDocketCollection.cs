using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickableDocketCollection : WhsDocketCollection, IOrdersDocumentSupport
	{
		#region Constructors

		public WhsPickableDocketCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsPickableDocketCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public WhsPickableDocketCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public static WhsPickableDocketCollection GetCollectionFromPick(WhsPick pick)
		{
			var result = new WhsPickableDocketCollection(pick.Factory, new AdhocCollectionRelationship(typeof(WhsPickableDocket)));
			var dockets = result.Factory.Load<WhsPickableDocket>(new ZQuery(WhsDocketSchema.WD_WP, pick.PK));
			result.AddRange(dockets);

			return result;
		}

		#endregion

		#region SuspendAddToCollection

		/// <summary>
		/// DO NOT USE THIS IF YOU DON'T KNOW WHAT IT'S FOR!!!!
		/// This exists to prevent the architecture from Adding an Order
		/// via the Module Button Grid when the 'New' Button is first clicked.
		/// The legacy BizO Collection had a specific method that was called
		/// for this purpose, which Active Collection has no equivalent for
		/// and Architecture decided to call .Add() instead. This is to circumvent
		/// this behaviour, since for Adhoc Relationships, this results in duplicate elements.
		/// When this collection is made into a proper active collection (not this adhoc thing),
		/// this code can probably (and should) be removed.
		/// </summary>
		public IDisposable SuspendAddToCollection() => new SemaphoreManager(AddToCollectionSuspender);

		Semaphore AddToCollectionSuspender => addToCollectionSuspender ?? (addToCollectionSuspender = new Semaphore());
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		Semaphore addToCollectionSuspender;

		protected override void SetRelationshipDefaultsForElementCore(WhsDocket newElement, bool throwIfRelationshipNotSupported)
		{
			if (addToCollectionSuspender == null || !addToCollectionSuspender.IsSuspended)
			{
				base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			}
		}

		#endregion

		public new WhsPickableDocket this[int index]
		{
			get { return (WhsPickableDocket)(base[index]); }
		}

		protected override bool AllowNewDocket
		{
			get { return false; }
		}

		protected override void OnAdded(WhsDocket bizOAdded)
		{
			Type bizOType = bizOAdded.GetType();
			foreach (WhsPickableDocket pickableDocket in this)
			{
				if (pickableDocket.GetType() != bizOType)
				{
					throw new ArgumentException("PickableDocketCollection cannot contain a mix of both Orders and WorkOrders.");
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

		protected override IEnumerable<string> DocketTypes
		{
			get { return new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder }; }
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
