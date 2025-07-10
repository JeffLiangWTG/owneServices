using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI.Testing
{
	class ParentTransportBookingModuleGridTest : DtbBookingTestCaseWithFactory
	{
		public void TestEditButtonClick()
		{
			using (var form = new ZForm())
			{
				var grid = new ParentTransportBookingModuleGridForTesting();
				form.Controls.Add(grid);

				form.Show();

				grid.EditButton.PerformClick();
				AssertEquals("No valid Booking Parent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditButtonClick_WithParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();

				grid.EditButton.Enabled = true;
				grid.EditButton.PerformClick();
				AssertEquals("Could not find existing Transport Booking.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditButtonText()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("View/Edit", grid.EditButtonText.Caption);
			}
		}

		public void TestEditButtonClick_WithCancelledParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.IsCancelled = true;
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("View", grid.EditButtonText.Caption);
			}
		}

		public void TestEditButtonClick_WhenParentChangedToCancelled()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("View/Edit", grid.EditButtonText.Caption);

				dummy.IsCancelled = true;
				dummy.Z0_Description = "CHANGE!";
				AssertEquals("View", grid.EditButtonText.Caption);
			}
		}

		public void TestCreateTransportBookingButton()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = new ParentTransportBookingModuleGridForTesting();
				form.Controls.Add(grid);

				AssertEquals("Should be in correct position.", grid.CreateTransportBookingSplitButton, grid.CreateTransportBookingSplitButton.GetCurrentParent().Items[0]);
				AssertEquals(ToolStripItemDisplayStyle.ImageAndText, grid.CreateTransportBookingSplitButton.DisplayStyle);
				AssertEquals(Icons.GetImage(IconTypes.NewButtonRest), grid.CreateTransportBookingSplitButton.Image);
				AssertContainsExactElementsInAnyOrder(new[] { grid.CreateBookingButton, grid.CombineContainersButton }, grid.CreateTransportBookingSplitButton.DropDownItems);
			}
		}

		public void TestCreateTransportBookingButtonText()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("Create", grid.CreateTransportBookingSplitButton.Text);
				AssertEquals("Create", grid.CreateBookingButton.Text);
			}
		}

		public void TestCreateTransportBookingButtonText_ExistingJob()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var helper = new TransportBookingTestHelper(Factory);
			helper.CreateBooking(helper.CreateConsolidation(dummy));
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("Create/Override", grid.CreateTransportBookingSplitButton.Text);
				AssertEquals("Create/Override", grid.CreateBookingButton.Text);
			}
		}

		public void TestCreateTransportBookingButtonVisibility_WithCancelledParent()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.IsCancelled = true;
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);
				form.Show();
				AssertEquals(false, grid.CreateTransportBookingSplitButton.Visible);
			}
		}

		public void TestCreateTransportBookingButtonVisibility_WhenParentChangedToCancelled()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals(true, grid.CreateTransportBookingSplitButton.Visible);

				dummy.IsCancelled = true;
				dummy.Z0_Description = "CHANGE!";
				AssertEquals(false, grid.CreateTransportBookingSplitButton.Visible);
			}
		}

		public void TestCreateTransportBookingButtonClick()
		{
			using (var form = new ZForm())
			{
				var grid = new ParentTransportBookingModuleGridForTesting();
				form.Controls.Add(grid);

				form.Show();

				grid.CreateTransportBookingSplitButton.PerformButtonClick();
				AssertEquals("No valid Booking Parent.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				grid.CreateBookingButton.PerformClick();
				AssertEquals("No valid Booking Parent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTransportBookingButtonClick_ParentJobIsCancelled()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.IsCancelled = true;
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();

				grid.CreateTransportBookingSplitButton.Visible = true;
				grid.CreateTransportBookingSplitButton.Enabled = true;
				grid.CreateTransportBookingSplitButton.PerformButtonClick();
				AssertEquals("Parent Job is Canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateTransportBookingButtonClick_WithParent()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM123";
			Factory.Save();
			var startingSavesOnMainFactory = Factory.SaveCount;
			var startingGlobalSaves = BusinessObjectFactory.GlobalSaveCount;

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				DtbDeliveryManager manager = null;
				grid.SetActionForDeliveryManager(m => manager = m);
				form.Show();

				var bookingCreatedHitCount = 0;
				grid.BookingCreated += (sender, e) => bookingCreatedHitCount++;
				AssertEquals("Create", grid.CreateTransportBookingSplitButton.Text);

				AssertEquals("Precondition: Main Factory should not have saved since Factory.Save() was called", startingSavesOnMainFactory, Factory.SaveCount);
				AssertEquals("Precondition: There should not have been any saves since Factory.Save() was called", startingGlobalSaves, BusinessObjectFactory.GlobalSaveCount);
				grid.CreateTransportBookingSplitButton.PerformButtonClick();
				AssertEquals("Main Factory should not have saved in creating a transport booking", startingSavesOnMainFactory, Factory.SaveCount);
				AssertGreaterThan("There should have been two factory saves called in creating a transport booking", BusinessObjectFactory.GlobalSaveCount, startingGlobalSaves);
				manager.LastControllerForTest.LastShownForm.Dispose();
				AssertEquals("Booking Created Event should have fired.", 1, bookingCreatedHitCount);
				AssertEquals("Create/Override", grid.CreateTransportBookingSplitButton.Text);

				var otherFactory = new BusinessObjectFactory();
				var dummyInOtherFactory = otherFactory.Load<DummyWithDtbBooking>(dummy.PK);
				var consolidation = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(otherFactory, dummyInOtherFactory, DtbBookingDirection.PIC);
				AssertNotNull("Should have created new Booking Consolidation.", consolidation);
				Assert("Should have created new Transport Booking", consolidation.Bookings.Any());
			}
		}

		public void TestCreateTransportBookingDropDownButtonClick_WithParent()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM123";
			Factory.Save();
			var startingSavesOnMainFactory = Factory.SaveCount;
			var startingGlobalSaves = BusinessObjectFactory.GlobalSaveCount;

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				DtbDeliveryManager manager = null;
				grid.SetActionForDeliveryManager(m => manager = m);
				form.Show();

				AssertEquals("Precondition: Main Factory should not have saved since Factory.Save() was called", startingSavesOnMainFactory, Factory.SaveCount);
				AssertEquals("Precondition: There should not have been any saves since Factory.Save() was called", startingGlobalSaves, BusinessObjectFactory.GlobalSaveCount);
				grid.CreateBookingButton.PerformClick();
				AssertEquals("Main Factory should not have saved in creating a transport booking", startingSavesOnMainFactory, Factory.SaveCount);
				AssertGreaterThan("There should have been two factory saves called in creating a transport booking", BusinessObjectFactory.GlobalSaveCount, startingGlobalSaves);
				manager.LastControllerForTest.LastShownForm.Dispose();
				AssertEquals("Create/Override", grid.CreateBookingButton.Text);

				var otherFactory = new BusinessObjectFactory();
				var dummyInOtherFactory = otherFactory.Load<DummyWithDtbBooking>(dummy.PK);
				var consolidation = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(otherFactory, dummyInOtherFactory, DtbBookingDirection.PIC);
				AssertNotNull("Should have created new Booking Consolidation.", consolidation);
				Assert("Should have created new Transport Booking", consolidation.Bookings.Any());
			}
		}

		public void TestCombineContainersButton()
		{
			using (var form = new ZForm())
			{
				var grid = new ParentTransportBookingModuleGridForTesting();
				form.Controls.Add(grid);

				form.Show();

				AssertEquals("Create/Override Multi-Container", grid.CombineContainersButton.Text);
			}
		}

		public void TestCombineContainersButtonText()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("Create Multi-Container", grid.CombineContainersButton.Text);
			}
		}

		public void TestCombineContainersButtonText_ExistingJob()
		{
			var dummy = Factory.New<DummyWithDtbBooking>();
			var helper = new TransportBookingTestHelper(Factory);
			var consol = helper.CreateConsolidation(dummy);
			var booking = Helper.CreateBooking(consol, "IFFD", "Test", "PIC", "REF");
			var container1 = helper.CreatePackageContainer("CONT1");
			var container2 = helper.CreatePackageContainer("CONT2");
			booking.PackageJob.Packages.Add(container1);
			booking.PackageJob.Packages.Add(container2);
			booking.Instructions[0].DivotsWithPackages.AddPackage(container1);
			booking.Instructions[0].DivotsWithPackages.AddPackage(container2);

			Factory.Save();

			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				form.Show();
				AssertEquals("Create/Override Multi-Container", grid.CombineContainersButton.Text);
			}
		}

		public void TestCombineContainersButtonClick()
		{
			using (var form = new ZForm())
			{
				var grid = new ParentTransportBookingModuleGridForTesting();
				form.Controls.Add(grid);

				form.Show();

				grid.CombineContainersButton.PerformClick();
				AssertEquals("No valid Booking Parent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCombineContainersButtonClick_WithParent()
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes; // options form
			ZFormModaliser.ShowDialogsInTest = true;

			var dummy = Factory.New<DummyWithDtbBooking>();
			dummy.Z0_Description = "DUM123";
			Factory.Save();
			var startingSavesOnMainFactory = Factory.SaveCount;

			using (ObjectFactory.Get<IDtbParentInfoLoader>().SetDummyWriterDecider(isFCL: true))
			using (var form = new ZForm(new DtbBookingParentWrapper(dummy, DtbBookingDirection.PIC)))
			{
				var grid = SetupGridForBinding(form);

				DtbDeliveryManager manager = null;
				grid.SetActionForDeliveryManager(m => manager = m);
				form.Show();

				var startingGlobalSaves = BusinessObjectFactory.GlobalSaveCount;
				AssertEquals("Precondition: Main Factory should not have saved since Factory.Save() was called", startingSavesOnMainFactory, Factory.SaveCount);
				grid.CombineContainersButton.PerformClick();
				AssertEquals("Main Factory should not have saved in creating a transport booking", startingSavesOnMainFactory, Factory.SaveCount);
				AssertGreaterThan("There should have been two factory saves called in creating a transport booking", BusinessObjectFactory.GlobalSaveCount, startingGlobalSaves);
				manager.LastControllerForTest.LastShownForm.Dispose();
				AssertEquals(true, manager.CombineContainersForTest);
				AssertEquals("Create/Override Multi-Container", grid.CombineContainersButton.Text);

				var otherFactory = new BusinessObjectFactory();
				var dummyInOtherFactory = otherFactory.Load<DummyWithDtbBooking>(dummy.PK);
				var consolidation = DtbBookingConsolidation.FindExistingTransportBookingConsolidation(otherFactory, dummyInOtherFactory, DtbBookingDirection.PIC);
				AssertNotNull("Should have created new Booking Consolidation.", consolidation);
				Assert("Should have created new Transport Booking", consolidation.Bookings.Any());
			}
		}

		static ParentTransportBookingModuleGridForTesting SetupGridForBinding(ZForm form)
		{
			var userControl = new ZUserControl();
			var grid = new ParentTransportBookingModuleGridForTesting();
			userControl.BindingSource.SetBindingMember(grid, "Bookings");
			grid.BindToFindBoxList = ".";

			var column = new ZTextBoxColumnStyleInfo();
			column.ColumnName = "KM_JobId";
			grid.ColumnStyles.Add(column);

			userControl.Controls.Add(grid);
			form.Controls.Add(userControl);

			return grid;
		}

		class ParentTransportBookingModuleGridForTesting : ParentTransportBookingModuleGrid
		{
			public ZToolStripButton EditButton => FindToolStripButton(Buttons.Edit);
		}
	}
}
