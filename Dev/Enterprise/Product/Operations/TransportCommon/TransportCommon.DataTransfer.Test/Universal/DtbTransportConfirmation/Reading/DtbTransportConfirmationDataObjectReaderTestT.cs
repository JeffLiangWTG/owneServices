using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Testing
{
	public abstract class DtbTransportConfirmationDataObjectReaderTest<T> : OrganizationAddressTestHelper
			where T : DtbTransportConfirmation
	{
		#region TestBasicFieldMappings

		public void TestBasicFieldMappings()
		{
			var now = new ZDateTime(ZDateTime.Now.Year, 1, 2);

			var confirmationDataObject = new Confirmation();
			confirmationDataObject.ActualDate = now;
			confirmationDataObject.EstimatedDate = now.AddDays(1);
			confirmationDataObject.RequiredFromDate = now.AddDays(2);
			confirmationDataObject.RequiredToDate = now.AddDays(3);
			confirmationDataObject.SlotDate = now.AddDays(4);
			confirmationDataObject.Quantity = 10;
			confirmationDataObject.ReceivedBy = "Fred";
			confirmationDataObject.Reference = "CONFREF";
			confirmationDataObject.IsEmptyContainer = true;
			confirmationDataObject.SlotReference = "SlotRef1";

			AssertConfirmation(confirmationDataObject, now);
			AssertConfirmation(confirmationDataObject, now);
		}

		void AssertConfirmation(Confirmation confirmationDataObject, ZDateTime now)
		{
			var divot = GetNewDivot();
			var reader = GetNewReader(confirmationDataObject, Logger, Factory, divot.Row());
			var confirmation = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("confirmation.KK_Actual", now, confirmation.KK_Actual);
				AssertEquals("confirmation.KK_Estimated", now.AddDays(1), confirmation.KK_Estimated);
				AssertEquals("confirmation.KK_IsEmptyContainer", true, confirmation.KK_IsEmptyContainer);
				AssertEquals("confirmation.KK_Quantity", 10, confirmation.KK_Quantity);
				AssertEquals("confirmation.KK_ReceivedBy", "Fred", confirmation.KK_ReceivedBy);
				AssertEquals("confirmation.KK_ReferenceNum", "CONFREF", confirmation.KK_ReferenceNum);
				AssertEquals("confirmation.KK_RequiredFrom", now.AddDays(2), confirmation.KK_RequiredFrom);
				AssertEquals("confirmation.KK_RequiredTo", now.AddDays(3), confirmation.KK_RequiredTo);
				AssertEquals("confirmation.KK_SlotDateTime", now.AddDays(4), confirmation.KK_SlotDateTime);
				AssertEquals("confirmation.KK_SlotReference", "SlotRef1", confirmation.KK_SlotReference);
			});
		}

		protected abstract DtbTransportInstructionPkgDivot GetNewDivot();

		#endregion

		#region TestParentRowCannotBeNull

		public void TestParentRowCannotBeNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => GetNewReader(new Confirmation(), Logger, Factory, null));
		}

		#endregion

		#region TestQuantityIsNotSetForInstructionConfirmations

		public void TestQuantityIsNotSetForInstructionConfirmations()
		{
			var confirmationDataObject = new Confirmation();
			confirmationDataObject.Quantity = 10;
			confirmationDataObject.Reference = "CONFREF";

			var instruction = GetNewInstruction();
			var reader = GetNewReader(confirmationDataObject, Logger, Factory, instruction.Row());
			var confirmation = reader.ReadIntoBusinessObject();

			var query = new ZQuery(DtbBookingConfirmationSchema.KK_Quantity, 0);
			// this tests that the actual DB field/column KK_Quantity does *not* get set. When we have an Instruction Confirmation the property getter is calculated
			// (i.e does not get the DB Value), so it is not possible to tell the DB value through the Property, instead we use a ZQuery to check the DB value.
			var confirmationFromQuery = Factory.LoadTop1<T>(query);
			AssertEquals("Quantity of DB field should be Zero", confirmation, confirmationFromQuery);
			AssertEquals("confirmation.KK_ReferenceNum", "CONFREF", confirmation.KK_ReferenceNum);
		}

		protected abstract DtbTransportInstruction GetNewInstruction();

		#endregion

		#region Implementation

		protected abstract DtbTransportConfirmationDataObjectReader<T> GetNewReader(Confirmation confirmationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer parent);

		#endregion

	}
}
