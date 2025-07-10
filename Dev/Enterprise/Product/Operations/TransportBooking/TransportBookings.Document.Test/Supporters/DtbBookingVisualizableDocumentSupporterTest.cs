using CargoWise.Application;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Moq;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Document.Testing
{
	[TestedType(typeof(DtbBookingVisualizableDocumentSupporter))]
	class DtbBookingVisualizableDocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomizeFormCheckpoint()
		{
			AssertEquals(nameof(supporter.CustomizeFormCheckpoint), Env.Security.DtbBookingCustomiseForms, supporter.CustomizeFormCheckpoint);
		}

		public void TestGetDocDataObject()
		{
			var pickup = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var delivery = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			var provider = new DtbBookingDocDataObjectProvider();

			var parameters = new DummyDocDataObjectParameters
			{
				DocumentTitle = "title",
				Data = new DtbBookingInstruction[] { pickup, delivery }
			};

			var docDataObject = supporter.GetDocDataObject(booking, DataContext.CMRConsignmentNote, parameters);
			AssertNotNull(nameof(docDataObject.Right), docDataObject.Right);
			AssertType<CMRConsignmentNoteDocDataObjectCollection>($"{nameof(docDataObject.Right)} type", docDataObject.Right);
		}

		public void TestGetAdditionalData_CMRConsignment()
		{
			//Booking does not contain any instructions
			AssertEquals("Precondition", 0, booking.Instructions.Count);

			supporter = new DtbBookingVisualizableDocumentSupporter(booking);
			var menuItem = CreateMenuItem(DataContext.CMRConsignmentNote);
			var data = supporter.GetAdditionalData(booking, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When booking doesn't have any instructions, supporter should show error message.",
				"Transport Booking does not contain PIC or DLV Instructions.",
				data.Left);

			//Booking contains only one pickup instruction
			booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			supporter = new DtbBookingVisualizableDocumentSupporter(booking);
			data = supporter.GetAdditionalData(booking, menuItem);
			Assert(data.IsLeft);
			AssertEquals(
				"When booking contains only pickup instructions, supporter should show error message.",
				"Transport Booking does not contain PIC or DLV Instructions.",
				data.Left);

			//Booking contains only one delivery instruction
			booking.Instructions.RemoveAll(i => i.IsPickUp);
			booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);
			supporter = new DtbBookingVisualizableDocumentSupporter(booking);
			data = supporter.GetAdditionalData(booking, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When booking contains only delivery instructions, supporter should show error message.",
				"Transport Booking does not contain PIC or DLV Instructions.",
				data.Left);

			//Booking contains more than one pickup and  more than one delivery instructions
			booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			supporter = new DtbBookingVisualizableDocumentSupporter(booking);
			data = supporter.GetAdditionalData(booking, menuItem);

			Assert(data.IsLeft);
			AssertEquals(
				"When booking contains multiple pickup and delivery instructions, supporter should show error message.",
				"Transport Booking contains excessive PIC and DLV Instructions.",
				data.Left);

			booking.Instructions.RemoveAll(i => i.IsPickUp || i.IsDelivery);

			//Booking contains one pickup and one delivery instruction
			var pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var delivery1 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			var selector = new Mock<IInstructionSelector>();

			using (ObjectFactory.Substitute(selector.Object))
			{
				supporter = new DtbBookingVisualizableDocumentSupporter(booking);
				data = supporter.GetAdditionalData(booking, menuItem);

				//When booking contains one pickup and one delivery instruction, supporter should not call selector
				selector.Verify(s => s.SelectInstruction(It.IsAny<DtbBookingInstruction[]>()), Times.Never());

				Assert(data.IsRight);
				AssertContainsExactElementsInAnyOrder(
					"When booking contains one pickup and one delivery instruction, supporter should return both instructions.",
					[pickup1, delivery1],
					data.Right as DtbBookingInstruction[]);
			}

			//Booking contains multiple pickup and one delivery instruction
			var pickup2 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			selector.Setup(s => s.SelectInstruction(It.IsAny<DtbBookingInstruction[]>())).Returns(pickup2);

			using (ObjectFactory.Substitute(selector.Object))
			{
				supporter = new DtbBookingVisualizableDocumentSupporter(booking);
				data = supporter.GetAdditionalData(booking, menuItem);

				//When booking contains multiple pickup and one delivery instruction, supporter should call selector
				selector.Verify(s => s.SelectInstruction(It.IsAny<DtbBookingInstruction[]>()), Times.Once());

				Assert(data.IsRight);
				AssertContainsExactElementsInAnyOrder(
					"supporter should return the selected pickup instruction and delivery instruction.",
					[pickup2, delivery1],
					data.Right as DtbBookingInstruction[]);
			}

			//Booking contains one pickup and multiple delivery instructions
			booking.Instructions.RemoveAll(i => i.IsPickUp);
			pickup1 = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var delivery2 = booking.Instructions.AddNew(InstructionTypes.Codes.Delivery);

			selector = new Mock<IInstructionSelector>();
			selector.Setup(s => s.SelectInstruction(It.IsAny<DtbBookingInstruction[]>())).Returns(delivery2);

			using (ObjectFactory.Substitute(selector.Object))
			{
				supporter = new DtbBookingVisualizableDocumentSupporter(booking);
				data = supporter.GetAdditionalData(booking, menuItem);
				//When booking contains one pickup and multiple delivery instruction, supporter should call selector
				selector.Verify(s => s.SelectInstruction(It.IsAny<DtbBookingInstruction[]>()), Times.Once());
				Assert(data.IsRight);
				AssertContainsExactElementsInAnyOrder(
					"supporter should return the selected delivery instruction and pickup instruction.",
					[pickup1, delivery2],
					data.Right as DtbBookingInstruction[]);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			booking = Factory.NewWithValidTestData<DtbBooking>();
			supporter = new DtbBookingVisualizableDocumentSupporter(booking);
		}

		StmMenuItem CreateMenuItem(string context, string menuName = "")
		{
			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = context;
			var menuItem = Factory.New<DocumentCommand>();
			menuItem.SU_MenuName = menuName;
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;

			return menuItem;
		}

		DtbBooking booking;
		DtbBookingVisualizableDocumentSupporter supporter;
	}
}
