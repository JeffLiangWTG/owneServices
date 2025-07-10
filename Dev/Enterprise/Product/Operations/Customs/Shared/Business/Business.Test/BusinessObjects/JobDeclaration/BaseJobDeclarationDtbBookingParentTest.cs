using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportBookings.Shared.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseJobDeclaration))]
	public class BaseJobDeclarationDtbBookingParentTest : IDtbBookingParentTestCase<BaseJobDeclaration>
	{
		public virtual void TestSupportedDirections()
		{
			var declaration = GetNewParent();
			SetExport(declaration);
			var bookingParent = declaration as IDtbBookingParent;
			AssertContainsExactElementsInAnyOrder(bookingParent.GetSupportedDirections(), new[] { DtbBookingDirection.PIC });

			SetImport(declaration);
			bookingParent = declaration;
			AssertContainsExactElementsInAnyOrder(bookingParent.GetSupportedDirections(), new[] { DtbBookingDirection.DLV });
		}

		public void TestControllerID()
		{
			var declaration = GetNewParent();
			var bookingParent = declaration as IDtbBookingParent;
			AssertEquals(ControllerIDs.Customs.JobDeclaration, bookingParent.ControllerID);
		}

		public void TestJobDescription()
		{
			var declaration = GetNewParent();
			declaration.JE_DeclarationReference = "1";
			var bookingParent = declaration as IDtbBookingParent;
			AssertEquals("Declaration 1", bookingParent.JobDescription);
			AssertEquals("Declaration", bookingParent.JobTypeDescription);
		}

		public void TestJobNumber()
		{
			var declaration = GetNewParent();
			var bookingParent = declaration as IDtbBookingParent;
			declaration.JE_DeclarationReference = "1";
			AssertEquals(declaration.JE_DeclarationReference, bookingParent.JobNumber);
		}

		public void TestJobStatus()
		{
			var declaration = GetNewParent();
			var bookingParent = declaration as IDtbBookingParent;
			AssertEquals(declaration.JE_EntryStatusDescription, bookingParent.JobStatus);
		}

		public void TestTransportBookingTransportModeForImport()
		{
			CombineAssertions(() =>
			{
				AssertTransportBookingTransportModeForImport((d) => d.TransportModeAirCodeForTesting, Constants.TransportModes.Air);
				AssertTransportBookingTransportModeForImport((d) => d.TransportModeSeaCodeForTesting, Constants.TransportModes.Sea);
				AssertTransportBookingTransportModeForImport((d) => d.TransportModeRoadCodeForTesting, Constants.TransportModes.Road);
				AssertTransportBookingTransportModeForImport((d) => d.TransportModeRailCodeForTesting, Constants.TransportModes.Rail);
				AssertTransportBookingTransportModeForImport((d) => d.TransportModeMailCodeForTesting, Constants.TransportModes.Mail);
				AssertTransportBookingTransportModeForImport((d) => "D!3", string.Empty);
			});
		}

		public void TestCanCreateTransportBooking()
		{
			var declaration = GetNewParent();
			var bookingParent = declaration as IDtbBookingParent;
			AssertEquals("CanCreateTransportBooking should always return true", true, bookingParent.CanCreateTransportBooking);
		}

		public void TestBookingParentPK()
		{
			var declaration = GetNewParent();
			var dtbBookingParent = declaration as IDtbBookingParent;
			AssertEquals("BookingParentPK should be the Declaration PK.", declaration.PK, dtbBookingParent.BookingParentPK);
		}

		public void TestBookingParentTablePrefix()
		{
			var declaration = GetNewParent();
			var dtbBookingParent = declaration as IDtbBookingParent;
			AssertEquals("BookingParentTablePrefix should be the Declaration table prefix.", declaration.TablePrefix, dtbBookingParent.BookingParentTablePrefix);
		}

		public void TestGetExtendingConfirmMessageBeforeCreateTransportBooking()
		{
			var declaration = GetNewParent();
			var dtbBookingParent = declaration as IDtbBookingParent;

			var (isShouldShow, caption, message, confirmation) = dtbBookingParent.GetExtendingConfirmMessageBeforeCreateTransportBooking();
			AssertEquals("IsShouldShow should return false.", false, isShouldShow);
			AssertNullOrEmpty("Caption should be null.", caption);
			AssertNullOrEmpty("Message should be null.", message);
			AssertNullOrEmpty("Confirmation should be null.", confirmation);
		}

		void AssertTransportBookingTransportModeForImport(Func<BaseJobDeclaration, ZString> getTransportMode, ZString expectedMode)
		{
			var declaration = GetNewParent();
			SetImport(declaration);
			var transportMode = getTransportMode(declaration);
			declaration.JE_TransportMode = transportMode;
			AssertEquals($"Converting '{transportMode}' for Import", expectedMode, declaration.TransportBookingTransportMode);
		}

		protected override BaseJobDeclaration GetNewParent()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected override bool IsWorkflowDescriptorSupportsCreateTransportBooking()
		{
			return true;
		}

		protected void SetExport(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = declaration.GetDefaultMessageType(false);
		}

		protected void SetImport(BaseJobDeclaration declaration)
		{
			declaration.JE_MessageType = declaration.GetDefaultMessageType(true);
		}

		protected override bool CanHaveDirectCartageChild => true;
	}
}
