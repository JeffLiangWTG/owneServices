using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingNotes : Notes
	{
		public QuotedBookingNotes(QuotedBooking quotedBooking)
			: base(quotedBooking)
		{
			Argument.NotNull(quotedBooking, "quotedBooking");
		}

		protected override Type ElementType
		{
			get { return typeof(QuotedBookingStmNote); }
		}

		public new QuotedBooking Parent
		{
			get { return (QuotedBooking)base.Parent; }
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new QuotedBookingStmNoteCollection(Parent);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new QuotedBookingStmNoteCollectionWithRelatedElements(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new QuotedBookingStmNoteCollectionView(Parent);
		}

		protected override StmNoteContexts GetNoteContextsForRelatedBizObject(BusinessObject relatedBizObject)
		{
			QuotedBooking quotedBooking = Parent;

			if (relatedBizObject == null)
			{
				throw new ArgumentNullException(nameof(relatedBizObject));
			}

			StmNoteContexts parentContexts = base.GetNoteContextsForRelatedBizObject(relatedBizObject);
			StmNoteContexts contexts =
				new StmNoteContexts
				{
					Module = parentContexts.Module,
					Direction = parentContexts.Direction,
					FreightMode = parentContexts.FreightMode
				};

			if (relatedBizObject == quotedBooking.Consignor && relatedBizObject != quotedBooking.Consignee)
			{
				contexts.Direction &= ~StmNoteContextDirection.I;
				if (quotedBooking.IsImport())
				{
					contexts.Direction |= StmNoteContextDirection.E;
				}
			}
			else if (relatedBizObject == quotedBooking.Consignee)
			{
				contexts.Direction &= ~StmNoteContextDirection.E;
				if (quotedBooking.IsExport())
				{
					contexts.Direction |= StmNoteContextDirection.I;
				}
			}

			return contexts;
		}

		protected override ZQuery RebuildAllElementsInternalQuery
			=> QuotedBookingStmNoteCollection.BuildRelationshipQuery(Parent);

		public void ReloadIfLoaded()
		{
			if (IsElementsLoaded)
			{
				ReloadFromDB();
			}
		}
	}
}
