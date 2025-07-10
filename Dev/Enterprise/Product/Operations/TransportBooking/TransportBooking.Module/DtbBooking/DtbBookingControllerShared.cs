using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public abstract class DtbBookingControllerShared : ZController
	{
		protected DtbBookingControllerShared()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBooking; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DtbBooking); }
		}

		public override bool IsFormShownFor(IBusiness businessEntity)
		{
			var isOwnFormShowing = IsOwnFormShowing(businessEntity);
			return isOwnFormShowing || IsConsolidationFormShowing((DtbBooking)businessEntity);
		}

		protected bool IsOwnFormShowing(IBusiness businessEntity) => base.IsFormShownFor(businessEntity);

		bool IsConsolidationFormShowing(DtbBooking booking)
		{
			var consolidation = booking.ConsolidationSingleJob;
			return consolidation != null && ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation).IsFormShownFor(consolidation);
		}

		/// <summary>
		/// Switch to own form if open, otherwise try switch to it's Consolidation form if open 
		/// </summary>
		public override void SwitchToFormFor(IBusiness businessEntity)
		{
			var isOwnFormShowing = IsOwnFormShowing(businessEntity); // do not check for consolidation
			if (isOwnFormShowing)
			{
				base.SwitchToFormFor(businessEntity);
			}
			else // try switch to Consolidation if shown (show msg if behind sibling)
			{
				var booking = (DtbBooking)businessEntity;
				var consolidation = booking.ConsolidationSingleJob;
				var consolidationController = ZControllerFactory.Create(ControllerIDs.DtbBookingConsolidation);
				if (consolidation != null && consolidationController.IsFormShownFor(consolidation))
				{
					consolidationController.SwitchToFormFor(consolidation);
					var consolidationForm = (TransportBookingMultiForm)consolidationController.LastShownForm;
					LastShownForm = consolidationForm; // used in testing.
					if (consolidationForm != null)
					{
						if (consolidationForm.OwnedForms != null && consolidationForm.OwnedForms.Length > 0)
						{
							Globals.Message.ShowInformation(ResString.GetMultilingualString("52D5FE85-F8BC-445C-839E-3ACC0FCD35ED",
									"A Transport Booking screen belonging to the same consolidation is open."));
						}
						else
						{
							consolidationForm.SelectBooking(booking);
						}
					}
				}
			}
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var booking = (DtbBooking)businessEntity;
			return new TransportBookingForm(booking);
		}

		protected override void ShowModelessForm(IZForm form)
		{
			base.ShowModelessForm(form);

			var tbForm = form as TransportBookingForm;
			if (tbForm != null)
			{
				tbForm.SetInstructionView(DtbDeliveryManager.DefaultBookingView);
			}
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			DtbChildEditableService.SetState(factory, DtbChildEditableServiceState.Transport);
			DtbFormStateService.SetState(factory, DtbFormState.Booking);
		}

		protected sealed override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var booking = (DtbBooking)base.GetNewBusinessEntityInLocalFactory();
			var consolidation = (DtbBookingConsolidation)Factory.New(TransportConsolidationType);
			consolidation.KB_JobDirection = NewMasterBookingConsolidationJobDirection;
			using (booking.SuspendSettingHasChanges())
			{
				SetupNewBusinessEntity(booking);
				consolidation.Bookings.Add(booking);
			}
			return booking;
		}

		protected string NewMasterBookingConsolidationJobDirection { get; set; }

		protected virtual void SetupNewBusinessEntity(DtbBooking booking)
		{
		}

		Type TransportConsolidationType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.DtbBookingView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.DtbBookingEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.DtbBookingNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.DtbBookingDelete; }
		}

		readonly DtbBookingCRMSecurityProvider SecurityProvider = new DtbBookingCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as DtbBooking, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as DtbBooking, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return SecurityProvider.GetSecurityCheckpoint(bizObject as DtbBooking, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;
	}
}
