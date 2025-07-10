using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class QuotedBookingStmNote : StmNote
	{
		public QuotedBookingStmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new QuotedBookingStmNoteValidation(this);
		}

		public QuotedBooking QuotedBooking { get; set; }

		public override bool IsParentTemplateRecord => QuotedBooking?.IsTemplate ?? false;

		public override ZString ST_NoteSource
		{
			get
			{
				switch (ST_Table)
				{
					case ForwardingShipment.Schema.TableName:
						return Res.GetString("1264e5f1-7f95-486c-a749-69544a9f9193", "Booking");
					case Quote.Schema.TableName:
						return Res.GetString("271dd1af-feb3-4fd0-b9fd-5b24f9098019", "Quote");
					default:
						return base.ST_NoteSource;
				}
			}
		}

		public override IStmNoteParent Master
		{
			get { return base.Master; }
			set
			{
				IStmNoteParent newMaster = value;

				QuotedBooking quotedBookingMaster = newMaster as QuotedBooking;
				if (quotedBookingMaster != null)
				{
					newMaster = FindMaster(quotedBookingMaster, this);
				}

				if (Master != newMaster)
				{
					base.Master = newMaster;
					ST_Table = newMaster != null ? newMaster.NotesParentTableName : "";
					ST_ParentID = newMaster != null ? newMaster.NotesParentPK : ZGuid.Empty;
				}
			}
		}

		public override ICollection<IStmNoteParent> OverrideValidationMasters
		{
			get
			{
				var result = new List<IStmNoteParent>();
				if (QuotedBooking.Booking != null)
				{
					result.Add(QuotedBooking.Booking);
				}

				if (QuotedBooking.Quote != null)
				{
					result.Add(QuotedBooking.Quote);
				}

				return result;
			}
		}

		public override ZString ST_Description
		{
			get { return base.ST_Description; }
			set
			{
				if (ST_Description != value)
				{
					base.ST_Description = value;

					if (QuotedBooking != null)
					{
						Master = FindMaster(QuotedBooking, this);
					}
				}
			}
		}

		public override NoteTypeCollection ST_Description_List
		{
			get { return QuotedBooking != null ? QuotedBooking.NoteTypes : new NoteTypeCollection(); }
		}

		protected override bool IsRelatedInView(StmNoteCollectionView view)
		{
			QuotedBooking quotedBooking = view != null ? view.Parent as QuotedBooking : null;
			return quotedBooking != null ? !quotedBooking.IsNoteParent(this) : base.IsRelatedInView(view);
		}

		internal static IStmNoteParent FindMaster(QuotedBooking quotedBooking, QuotedBookingStmNote note)
		{
			IStmNoteParent result = null;

			if (quotedBooking != null && note != null)
			{
				if (IsBookingNote(quotedBooking, note.ST_DescriptionInDatabase))
				{
					result = quotedBooking.Booking;
				}
				else if (IsQuoteNote(quotedBooking, note.ST_DescriptionInDatabase))
				{
					result = quotedBooking.Quote;
				}
				else if (quotedBooking.Booking != null)
				{
					result = quotedBooking.Booking;
				}
				else if (quotedBooking.Quote != null)
				{
					result = quotedBooking.Quote;
				}
			}

			return result;
		}

		static bool IsBookingNote(QuotedBooking quotedBooking, ZString description)
		{
			return quotedBooking.Booking != null &&
				   quotedBooking.Booking.NoteTypes.Cast<PredefinedNoteType>().Any((noteType) => description == noteType.MultilingualDescription.GetUnresolvedString());
		}

		static bool IsQuoteNote(QuotedBooking quotedBooking, ZString description)
		{
			return quotedBooking.Quote != null &&
				quotedBooking.Quote.NoteTypes.Cast<PredefinedNoteType>().Any((noteType) => description == noteType.MultilingualDescription.GetUnresolvedString());
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (QuotedBookingStmNote)base.CloneInternal(args);
			clone.QuotedBooking = this.QuotedBooking;
			return clone;
		}
	}
}
