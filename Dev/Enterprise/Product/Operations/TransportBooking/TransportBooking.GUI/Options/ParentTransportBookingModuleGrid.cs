using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI
{
	public class ParentTransportBookingModuleGrid : ZModuleButtonGrid
	{
		public ParentTransportBookingModuleGrid()
		{
			SetupCreateTransportBookingButton();
		}

		void SetupCreateTransportBookingButton()
		{
			CombineContainersButton = new ZToolStripButton();
			// we pre-emptively set the largest caption so that the button is correctly sized to fit all versions of the button text
			CombineContainersButton.CaptionResourceString = ExistingMultiContainerText;
			CombineContainersButton.Name = "CombineContainersButton";
			CombineContainersButton.Click += CombineContainersButton_Click;

			CreateBookingButton = new ZToolStripButton();
			CreateBookingButton.Name = "CreateBookingButton";
			CreateBookingButton.Click += NewButton_Click;

			CreateTransportBookingSplitButton = new ZToolStripSplitButton();
			CreateTransportBookingSplitButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
			CreateTransportBookingSplitButton.DropDownItems.AddRange(new[]
			{
				CreateBookingButton,
				CombineContainersButton
			});
			CreateTransportBookingSplitButton.ImageTransparentColor = Color.Magenta;
			CreateTransportBookingSplitButton.Name = "CreateTransportBookingSplitButton";
			CreateTransportBookingSplitButton.ButtonClick += NewButton_Click;
			CreateTransportBookingSplitButton.Image = Icons.GetImage(IconTypes.NewButtonRest);

			toolStrip.Items.Insert(0, CreateTransportBookingSplitButton);
		}

		internal ZToolStripSplitButton CreateTransportBookingSplitButton;
		internal ZToolStripButton CombineContainersButton;
		internal ZToolStripButton CreateBookingButton;

		DtbBookingParentWrapper ParentWrapper => (DtbBookingParentWrapper)DataSource;

		internal void SetActionForDeliveryManager(Action<DtbDeliveryManager> actionForDeliveryManager)
		{
			ActionForDeliveryManager = actionForDeliveryManager;
		}

		Action<DtbDeliveryManager> ActionForDeliveryManager;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var previousDataSourceForDispose = dataSource == null ? ParentWrapper : null;

			base.SetDataBinding(dataSource, dataMember);

			UpdateButtonTextIfParentCancelled();

			var parent = (IBusiness)ParentWrapper?.Parent;
			if (parent != null)
			{
				parent.HasChangesChanged -= ParentTransportBookingModuleGrid_HasChangesChanged;
				parent.HasChangesChanged += ParentTransportBookingModuleGrid_HasChangesChanged;
			}
			else if (previousDataSourceForDispose != null)
			{
				((IBusiness)previousDataSourceForDispose.Parent).HasChangesChanged -= ParentTransportBookingModuleGrid_HasChangesChanged;
			}
		}

		void UpdateButtonTextIfParentCancelled()
		{
			if (ParentIsCancelled)
			{
				EditButtonText = DtbDeliveryManager.ViewTextWhenParentCancelled;
				CreateTransportBookingSplitButton.Visible = false;
			}
			else
			{
				EditButtonText = DtbDeliveryManager.ViewText;
				UpdateNewAndCombineContainersButtonText();
				CreateTransportBookingSplitButton.Visible = true;
			}
		}

		void UpdateNewAndCombineContainersButtonText()
		{
			if (ParentWrapper != null)
			{
				var newButtonText = DtbDeliveryManager.HasExistingBooking(ParentWrapper.Consolidation, combineContainers: false)
					? DtbDeliveryManager.CreateTextWhenHasExistingBooking
					: DtbDeliveryManager.CreateText;

				CreateTransportBookingSplitButton.CaptionResourceString = newButtonText;
				CreateBookingButton.CaptionResourceString = newButtonText;

				CombineContainersButton.CaptionResourceString = DtbDeliveryManager.HasExistingBooking(ParentWrapper.Consolidation, combineContainers: true)
					 ? ExistingMultiContainerText
					 : Res.GetData("ParentTransportBookingModuleGrid|CombineContainersButton|NoBooking", DtbDeliveryManager.CreateEnglishCaption + " " + DtbDeliveryManager.MultiContainerEnglishCaption);
			}
		}

		static ResourceStringData ExistingMultiContainerText => Res.GetData("ParentTransportBookingModuleGrid|CombineContainersButton|ExistingBooking", DtbDeliveryManager.CreateEnglishCaptionWhenHasExistingBooking + " " + DtbDeliveryManager.MultiContainerEnglishCaption);

		bool ParentIsCancelled => ParentWrapper != null && ParentWrapper.Parent is ICancellable cancellableParent && cancellableParent.IsCancelled;

		void CombineContainersButton_Click(object sender, EventArgs e)
		{
			CreateTransportBooking(combineContainers: true);
		}

		void ParentTransportBookingModuleGrid_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			UpdateButtonTextIfParentCancelled();
		}

		protected override void NewButton_Click(object sender, EventArgs e)
		{
			CreateTransportBooking(combineContainers: false);
		}

		void CreateTransportBooking(bool combineContainers)
		{
			var wrapper = ParentWrapper;
			if (wrapper != null)
			{
				var isParentEnabled = !ParentIsCancelled;
				if (isParentEnabled)
				{
					var factory = new BusinessObjectFactory();
					DtbDeliveryManager.CreateTransportBooking(factory, wrapper.Parent, wrapper.Direction, combineContainers, ActionForDeliveryManager);
					if (ParentWrapper.Bookings.Count == 0 && ParentWrapper.Consolidation != null)
					{
						BookingCreated?.Invoke(this, EventArgs.Empty);
					}

					UpdateNewAndCombineContainersButtonText();
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("175c4445-2c45-4d5e-b39a-830874654d40", "Parent Job is Canceled."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("856f917f-2849-46b6-9ef4-668791a4d986", "No valid Booking Parent."));
			}
		}

		public event EventHandler BookingCreated;

		protected override void EditButton_Click(object sender, EventArgs e)
		{
			var wrapper = ParentWrapper;
			if (wrapper != null)
			{
				var deliveryManager = new DtbDeliveryManager(wrapper.Parent.Factory, wrapper.Parent, wrapper.Direction, combineContainers: false);
				deliveryManager.ViewTransportBooking();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("856f917f-2849-46b6-9ef4-668791a4d986", "No valid Booking Parent."));
			}
		}
	}
}
