using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Freight.QuotedBookings.GUI;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Freight;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class QuotedBookingController : TemplateRecordZController, IQuotedBookingController
	{
		protected override bool IsQuotedBooking => true;

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public QuotedBookingController()
			: this(GetDefaultQuotedBookingCreationState())
		{
		}

		public QuotedBookingController(QuotedBookingState quotedBookingState)
		{
			this.state = quotedBookingState;
		}

		#region State

		QuotedBookingState state;

		public void SetQuotedBookingState(QuotedBookingState quotedBookingState)
		{
			this.state = quotedBookingState;
		}

		static QuotedBookingState GetDefaultQuotedBookingCreationState()
		{
			string defaultType = FreightDataRegistry.Instance.DefaultBookingNewButton.Value;
			switch (defaultType)
			{
				case BookingNewButtonLabelList.Codes.QuickBooking:
					return QuotedBookingState.BookingOnly;

				default:
					return QuotedBookingState.AcceptedBookingWithQuote;
			}
		}

		#endregion

		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID => ModuleIDs.QuotedBookings;
		public override ControllerID ID => ControllerIDs.QuotedBookings;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ViewQuotedBooking); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ViewQuotedBooking view = businessEntity as ViewQuotedBooking;
			if (view != null)
			{
				businessEntity = view.QuotedBooking;
			}

			var form = new QuotedBookingForm((QuotedBooking)businessEntity)
			{
				SkipRecentItems = this.SkipRecentItems
			};

			return form;
		}

		public bool SkipRecentItems { private get; set; }

		#endregion

		#region Copy

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			var viewQuotedBooking = inMemorySourceEntity as ViewQuotedBooking;
			if (viewQuotedBooking?.QuotedBooking is ITemplateCopyable)
			{
				ShowCopyForm(inMemorySourceEntity, GetTemplateCopy);

				if (LastShownForm != null)
				{
					LastShownForm.DisplayMode = Enterprise.ZArchitecture.Core.ODisplayMode.Edit;
				}

				return LastShownForm;
			}
			else
			{
				return base.ShowTemplateCopyForm(inMemorySourceEntity);
			}
		}

		protected virtual IBusiness GetTemplateCopy(IBusiness loadedSourceEntity)
		{
			return ((ITemplateCopyable)loadedSourceEntity).TemplateCopy();
		}

		#endregion

		#region Reverse

		public override IZForm ShowCopyAndReverseForm(BusinessObject inMemorySourceEntity)
		{
			var viewQuotedBooking = inMemorySourceEntity as ViewQuotedBooking;
			if (viewQuotedBooking?.QuotedBooking is ITemplateCopyable && viewQuotedBooking?.QuotedBooking is ITemplateReversible)
			{
				ShowCopyForm(inMemorySourceEntity, loadedSourceEntity =>
				{
					IBusiness result = ((ITemplateCopyable)loadedSourceEntity).TemplateCopy();
					((ITemplateReversible)result).Reverse();
					return result;
				});

				if (LastShownForm != null)
				{
					LastShownForm.DisplayMode = ODisplayMode.Edit;
				}

				return LastShownForm;
			}
			else
			{
				return base.ShowCopyAndReverseForm(inMemorySourceEntity);
			}
		}

		#endregion

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			switch (state)
			{
				case QuotedBookingState.QuoteOnly:
					ZGuid quoteOnlyPK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
					return QuotedBooking.New(quoteOnlyPK, ZGuid.Empty, Factory);

				case QuotedBookingState.BookingOnly:
					using (ShipmentFieldStateChange.InitializingShipment(Factory))
					{
						ZGuid bookingOnlyPK = QuotedBooking.CreateNewBooking(Factory).PK;
						return QuotedBooking.New(ZGuid.Empty, bookingOnlyPK, Factory);
					}

				case QuotedBookingState.AcceptedBookingWithQuote:
					ZGuid quotePK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted).PK;
					using (ShipmentFieldStateChange.InitializingShipment(Factory))
					{
						ZGuid bookingPK = QuotedBooking.CreateNewBooking(Factory).PK;
						return QuotedBooking.New(quotePK, bookingPK, Factory);
					}

				default:
					throw new InvalidOperationException("Invalid State");
			}
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView => Env.Security.QuickBooking;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.QuickBookingNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.QuickBookingEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.QuickBookingDelete;

		#endregion

		protected override FormAction SwitchFormAction(IBusiness entity, FormAction originalAction)
		{
			if (entity is QuotedBooking quotedBooking
				&& quotedBooking.Booking != null
				&& quotedBooking.ObjectState == QuotedBookingState.BookingOnly
				&& quotedBooking.Booking.JS_IsForwardRegistered)
			{
				return FormAction.View;
			}
			return originalAction;
		}

		#region Showing Forms

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			if (!CheckLoginCompanyMatches(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowViewForm(sourceEntity);
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			if (!CheckLoginCompanyMatches(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowEditForm(sourceEntity);
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			if (!CheckLoginCompanyMatches(sourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowDeleteForm(sourceEntity);
		}

		protected override IZForm ShowCopyForm(BusinessObject inMemorySourceEntity, CopyOfBusinessObject returnsNewBusinessEntity)
		{
			if (!CheckLoginCompanyMatches(inMemorySourceEntity))
			{
				LastShownForm = null;
				return null;
			}

			return base.ShowCopyForm(inMemorySourceEntity, returnsNewBusinessEntity);
		}

		static bool CheckLoginCompanyMatches(BusinessObject sourceEntity)
		{
			var viewQuotedBooking = sourceEntity as ViewQuotedBooking;
			var quotedBooking = viewQuotedBooking?.QuotedBooking ?? sourceEntity as QuotedBooking;

			if (quotedBooking != null && quotedBooking.Quote != null)
			{
				var company = quotedBooking.Quote.Company;
				if (company != null && company.PK != Env.CurrentCompany.PK)
				{
					var countryDescription = company.Country != null ? company.Country.Description : ZString.Empty;
					Globals.Message.ShowError(
							ResString.GetMultilingualString("28199819-475b-4cf3-a7c8-7b663ad7027a", @"This {0} is for login users in {1}.
Please login to the relevant company to view the {0}.",
									quotedBooking.HumanReadableNameWithoutID,
									string.Format(CultureInfo.InvariantCulture, "{0} ({1})", countryDescription, company.GC_Code)),
							ResString.GetMultilingualString("766275eb-f8d6-4d42-a0a7-34a6597abcd2", "Access Denied: Incorrect login company")
						);

					return false;
				}
			}

			return true;
		}

		#endregion

		#region CRM Security

		protected virtual CRMSecurityProvider<ViewQuotedBooking> CRMSecurityProvider => new QuotedBookingCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
