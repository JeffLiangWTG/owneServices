using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(DateAndReferenceCollection))]
	public class DateAndReferenceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DateAndReferenceCollection>
	{
		#region TestGetDefault

		public void TestGetDefault()
		{
			var defaultValue = DateAndReferenceCollection.GetDefault();
			AssertEquals(12, defaultValue.Count);

			AssertDateAndReference(defaultValue[0], ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", DatesAndReference.Any, true, true, true, true, true, RequiredConstants.FCLAvailable, true, RequiredConstants.FCLStorage, true, true);
			AssertDateAndReference(defaultValue[1], ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CFS", Constants.ContainerModes.Loose, true, true, true, false, true, RequiredConstants.LCLAvailable, true, RequiredConstants.LCLStorage, true, true);
			AssertDateAndReference(defaultValue[2], ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, DatesAndReference.Any, DatesAndReference.Any, true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);
			AssertDateAndReference(defaultValue[3], ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, Constants.CartageDirection.Destination, InstructionTypes.Codes.Multi, "CFS", "LSE", false, true, true, false, true, RequiredConstants.LCLAvailable, true, RequiredConstants.LCLStorage, true, true);
			AssertDateAndReference(defaultValue[4], ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);

			AssertDateAndReference(defaultValue[5], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CTO", DatesAndReference.Any, true, true, true, true, true, RequiredConstants.FCLReceive, true, RequiredConstants.FCLCutOff, true, true);
			AssertDateAndReference(defaultValue[6], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CFS", Constants.ContainerModes.Loose, true, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
			AssertDateAndReference(defaultValue[7], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CYD", "CNT", true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.EmptyReturnBy, true, true);
			AssertDateAndReference(defaultValue[8], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, DatesAndReference.Any, DatesAndReference.Any, true, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);
			AssertDateAndReference(defaultValue[9], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", Constants.ContainerModes.Loose, false, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
			AssertDateAndReference(defaultValue[10], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, true, true, false, true, RequiredConstants.RequiredFrom, true, RequiredConstants.RequiredTo, true, true);

			AssertDateAndReference(defaultValue[11], ConfirmationTypes.Codes.ConNoteNo, ConfirmationTypes.Descriptions.ConnoteNo, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, DatesAndReference.Any, false, false, false, false, false, (NoResString)"", false, (NoResString)"", true, false);
		}

		void AssertDateAndReference(DateAndReference dateAndReference, ZString code, ZString description, ZString bookingDirection, ZString instructionType, ZString organisationType, ZString containerMode, ZBool autoAdd, ZBool allowActual, ZBool allowEstimated, ZBool allowSlot, ZBool allowReqFrom, ZString allowReqFromLabel, ZBool allowReqTo, ZString allowReqToLabel, ZBool allowRef, ZBool allowRecBy)
		{
			AssertEquals(code, dateAndReference.Code);
			AssertEquals(description, dateAndReference.Description);
			AssertEquals(bookingDirection, dateAndReference.BookingDirection);
			AssertEquals(instructionType, dateAndReference.InstructionType);
			AssertEquals(organisationType, dateAndReference.OrganisationType);
			AssertEquals(containerMode, dateAndReference.ContainerMode);
			AssertEquals(autoAdd, dateAndReference.AutoAdd);
			AssertEquals(allowActual, dateAndReference.AllowActualDate);
			AssertEquals(allowEstimated, dateAndReference.AllowEstimatedDate);
			AssertEquals(allowSlot, dateAndReference.AllowSlotDate);
			AssertEquals(allowReqFrom, dateAndReference.AllowRequiredFromDate);
			AssertEquals(allowReqFromLabel, dateAndReference.AllowRequiredFromLabel);
			AssertEquals(allowReqTo, dateAndReference.AllowRequiredToDate);
			AssertEquals(allowReqToLabel, dateAndReference.AllowRequiredToLabel);
			AssertEquals(allowRef, dateAndReference.AllowReference);
			AssertEquals(allowRecBy, dateAndReference.AllowReceivedBy);
			AssertEquals(true, dateAndReference.IsSystemDefined);
		}

		#endregion

		#region AllowNew

		public void TestAllowNew()
		{
			AssertEquals("Must allow new rows", true, Collection.AllowNew);
		}

		#endregion

		#region SetDefaultsForNewChild

		public void TestSetDefaultsForNewChild()
		{
			AssertEquals("Booking direction default should be ANY", DatesAndReference.Any, Collection.AddNew().BookingDirection);
			AssertEquals("Instruction type default should be ANY", DatesAndReference.Any, Collection.AddNew().InstructionType);
			AssertEquals("OrganisationType default should be ANY", DatesAndReference.Any, Collection.AddNew().OrganisationType);
			AssertEquals("ContainerMode default should be ANY", DatesAndReference.Any, Collection.AddNew().ContainerMode);
		}

		#endregion

		#region TestFindConfirmation

		public void TestFindConfirmation()
		{
			var defaultValue = DateAndReferenceCollection.GetDefault();
			AssertDateAndReference(defaultValue.FindConfirmation(ConfirmationTypes.Codes.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", ""), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", DatesAndReference.Any, true, true, true, true, true, "CTO Available", true, "CTO Storage Start", true, true);
			AssertDateAndReference(defaultValue.FindConfirmation(ConfirmationTypes.Codes.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CFS", "LSE"), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CFS", "LSE", true, true, true, false, true, "CFS Receival Start", true, "CFS Cut Off", true, true);
			AssertDateAndReference(defaultValue.FindConfirmation(ConfirmationTypes.Codes.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "WHS", ""), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, DatesAndReference.Any, DatesAndReference.Any, true, true, true, false, true, "Required From", true, "Required To", true, true);
			AssertDateAndReference(defaultValue.FindConfirmation(ConfirmationTypes.Codes.PickUp, Constants.CartageDirection.Destination, InstructionTypes.Codes.Multi, "CFS", "LSE"), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, Constants.CartageDirection.Destination, InstructionTypes.Codes.Multi, "CFS", "LSE", false, true, true, false, true, "CFS Available", true, "CFS Storage Start", true, true);
		}

		#endregion

		#region TestFindDefaultConfirmations

		public void TestFindDefaultConfirmations()
		{
			var defaultValue = DateAndReferenceCollection.GetDefault();
			var confirmationsForCTOPickupInstruction = defaultValue.FindDefaultConfirmations(DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", "");
			AssertDateAndReference(confirmationsForCTOPickupInstruction.Single(c => c.Code == ConfirmationTypes.Codes.PickUp), ConfirmationTypes.Codes.PickUp, ConfirmationTypes.Descriptions.PickUp, DatesAndReference.Any, InstructionTypes.Codes.PickUp, "CTO", DatesAndReference.Any, true, true, true, true, true, "CTO Available", true, "CTO Storage Start", true, true);
			AssertNull(defaultValue.FindDefaultConfirmations(Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", "LSE").SingleOrDefault());
			AssertDateAndReference(defaultValue.FindDefaultConfirmations(DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CYD", "CNT").Single(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, DatesAndReference.Any, InstructionTypes.Codes.Delivery, "CYD", "CNT", true, true, true, false, true, "Required From", true, "Return By", true, true);
		}

		#endregion

		#region TestFindDefaultConfirmationsWithoutContainerMode

		public void TestFindDefaultConfirmationsWithoutContainerMode()
		{
			var defaultValue = DateAndReferenceCollection.GetDefault();
			defaultValue[9].AutoAdd = true;
			AssertDateAndReference(defaultValue[9], ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", Constants.ContainerModes.Loose, true, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
			AssertDateAndReference(defaultValue.FindDefaultConfirmations(Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", "").SingleOrDefault(), ConfirmationTypes.Codes.Delivery, ConfirmationTypes.Descriptions.Delivery, Constants.CartageDirection.Origin, InstructionTypes.Codes.Multi, "CFS", "LSE", true, true, true, false, true, RequiredConstants.LCLReceive, true, RequiredConstants.LCLCutOff, true, true);
		}

		#endregion

		#region GetDateAndReferenceByOrganisationType

		public void TestGetDateAndReferenceByOrganisationType()
		{
			var defaultValue = DateAndReferenceCollection.GetDefault();
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("CTO").Length);
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("CNR").Length);
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("CFS").Length);
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("CNE").Length);
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("CYD").Length);
			AssertEquals(3, defaultValue.GetDateAndReferenceByOrganisationType("WHS").Length);
		}

		#endregion

		#region IsSystemDefined

		public void TestIsSystemDefined()
		{
			AssertEquals("New rows must not be system defined", false, this.Collection.AddNew().IsSystemDefined);
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DateAndReferenceCollection GetCollectionToTest()
		{
			return new DateAndReferenceCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DateAndReference();
		}

		#endregion
	}
}
