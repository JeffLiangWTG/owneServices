using System;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.TransportBookings.Module
{
	public abstract class BaseCreateDtbBookingsFromDtbBookingParentsApplicator : OperationalActionMethodApplicator, ICreateDtbBookingsFromDtbBookingParentsApplicator
	{
		protected BaseCreateDtbBookingsFromDtbBookingParentsApplicator(string name, BusinessObjectFactory factory) : base(name, factory)
		{
		}

		public static class Schema
		{
			public const int DirectionMaxLength = 3;
			public const int BookingTemplateMaxLength = 4;
		}

		[MaxLength(Schema.DirectionMaxLength)]
		[List(nameof(DirectionList))]
		public ZString Direction
		{
			[DebuggerStepThrough]
			get => direction;
			set
			{
				CheckMaximumLength(DirectionInfo, value);
				SetNonPersistentPropertyValue(DirectionInfo, ref direction, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDirection();
				}
			}
		}

		public ZPropertyInfo DirectionInfo => GetZPropertyInfo(nameof(Direction));

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString direction;

		[MaxLength(Schema.BookingTemplateMaxLength)]
		[List(nameof(BookingTemplateList))]
		public ZString BookingTemplate
		{
			[DebuggerStepThrough]
			get => bookingTemplate;
			set
			{
				CheckMaximumLength(BookingTemplateInfo, value);
				SetNonPersistentPropertyValue(BookingTemplateInfo, ref bookingTemplate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateBookingTemplate();
				}
			}
		}

		public ZPropertyInfo BookingTemplateInfo => GetZPropertyInfo(nameof(BookingTemplate));

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		ZString bookingTemplate;

		protected sealed override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public CreateDtbBookingsFromDtbBookingParentsValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		CreateDtbBookingsFromDtbBookingParentsValidation GetNewValidation()
		{
			return new CreateDtbBookingsFromDtbBookingParentsValidation(this);
		}

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				if (directionList == null)
				{
					directionList = new CodeDescriptionPairList
					{
						new CodeDescriptionPair(nameof(DtbBookingDirection.PIC), Res.GetString("bd01afc7-1c34-4802-a24c-77ebc04228a9", "Pickup")),
						new CodeDescriptionPair(nameof(DtbBookingDirection.DLV), Res.GetString("54135dd3-4ac6-4ce0-af58-eca360b649d0", "Delivery")),
					};
				}

				return directionList;
			}
		}
		CodeDescriptionPairList directionList;

		public CodeDescriptionPairList BookingTemplateList
		{
			get
			{
				if (bookingTemplateList == null)
				{
					var query = new ZQuery();

					var bookingTemplateQuery = new ZDBOnlyQuery(typeof(DtbBookingTmpl));
					query.AddToFilter(bookingTemplateQuery);

					var bookingTmpls = Factory.Load<DtbBookingTmpl>(query);

					bookingTemplateList = new CodeDescriptionPairList();
					foreach (var bookingTemplate in bookingTmpls)
					{
						bookingTemplateList.Add(new CodeDescriptionPair(bookingTemplate.KT_Code.ToString(), bookingTemplate.KT_DescriptionMultilingual));
					}
				}

				return bookingTemplateList;
			}
		}
		CodeDescriptionPairList bookingTemplateList;
	}

	public sealed class CreateDtbBookingsFromDtbBookingParentsValidation : ZValidation
	{
		public CreateDtbBookingsFromDtbBookingParentsValidation(BaseCreateDtbBookingsFromDtbBookingParentsApplicator parent)
			: base(parent)
		{
			this.parent = parent;
			ZValidationInternals = this;
			ParentListInternals = parent;
		}

		public void Add(CreateDtbBookingsFromDtbBookingParentsValidation validation)
		{
			ZValidationInternals.Add(validation);
		}

		public void Remove(CreateDtbBookingsFromDtbBookingParentsValidation validation)
		{
			ZValidationInternals.Remove(validation);
		}

		public override void ValidateAll()
		{
			using (ParentListInternals.SuspendListChanged())
			{
				ValidateAllCore();
			}
		}

		void ValidateAllCore()
		{
			ValidateDirection();
			ValidateBookingTemplate();
		}

		public void ValidateDirection()
		{
			ZValidationInternals.Validate(Parent.DirectionInfo, new RunValidationInvoker(DirectionValidationInvoker));
		}

		void DirectionValidationInvoker()
		{
			CheckDirectionIsWesternEuropean();
			CheckDirection();
		}

		void CheckDirectionIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.DirectionInfo);
		}

		void CheckDirection()
		{
			MandatoryValidation.CheckEntered(Parent.DirectionInfo);
			ListValidation.ErrorIfInvalidCode(Parent.DirectionInfo, Parent.DirectionList);
		}

		public void ValidateBookingTemplate()
		{
			ZValidationInternals.Validate(Parent.BookingTemplateInfo, new RunValidationInvoker(BookingTemplateValidationInvoker));
		}

		void BookingTemplateValidationInvoker()
		{
			CheckBookingTemplateIsWesternEuropean();
			CheckBookingTemplate();
		}

		void CheckBookingTemplateIsWesternEuropean()
		{
			EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.BookingTemplateInfo);
		}

		void CheckBookingTemplate()
		{
			MandatoryValidation.CheckEntered(Parent.BookingTemplateInfo);
			ListValidation.ErrorIfInvalidCode(Parent.BookingTemplateInfo, Parent.BookingTemplateList);
		}

		public override Type AutoValidationType
		{
			get
			{
				return typeof(CreateDtbBookingsFromDtbBookingParentsValidation);
			}
		}

		public BaseCreateDtbBookingsFromDtbBookingParentsApplicator Parent
		{
			[DebuggerStepThrough]
			get
			{
				return parent;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly BaseCreateDtbBookingsFromDtbBookingParentsApplicator parent;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly IValidationInternals ZValidationInternals;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly ISingleElementListInternal ParentListInternals;
	}
}
