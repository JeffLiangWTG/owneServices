using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingBookingContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJC_ContainerNum()
		{
			const string message = "You have not entered a Container Number.";
			Container.JC_Purpose = ContainerBookedStatus.Codes.Booked;
			Container.JC_ContainerNum = "";
			Container.Validation.ValidateJC_ContainerNum();
			AssertNoMessageError("Should not have the message error on booked containers", Container.JC_ContainerNumInfo, message);
			Container.JC_Purpose = ContainerBookedStatus.Codes.Real;
			Container.JC_ContainerNum = "";
			Container.Validation.ValidateJC_ContainerNum();
			AssertHasMessageError("Should have the message error", Container.JC_ContainerNumInfo, message);
			Container.JC_ContainerNum = "FAKE4100011";
			AssertNoMessageError("Should not have the message error", Container.JC_ContainerNumInfo, message);
		}

		public void TestJC_IsEmptyContainer()
		{
			const string error = "This container has no pack lines packed into it and yet is not marked as empty";
			Container.Validation.ValidateJC_IsEmptyContainer();
			AssertHasMessageError("Error if not Empty, not packed and Real", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_IsEmptyContainer = true;
			AssertNoMessageError("No error if Empty, not packed and Real", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_Purpose = ContainerBookedStatus.Codes.Booked;
			Container.Validation.ValidateJC_IsEmptyContainer();
			AssertNoMessageError("No error if Empty, not packed and Booked", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_IsEmptyContainer = false;
			AssertNoMessageError("No error if not Empty, not packed and Booked", Container.JC_IsEmptyContainerInfo, error);
			PackLine.JL_JC = Container.PK;
			Container.Validation.ValidateJC_IsEmptyContainer();
			AssertNoMessageError("No error if not Empty, packed and Booked", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_IsEmptyContainer = true;
			AssertNoMessageError("No error if Empty, packed and Booked", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_Purpose = ContainerBookedStatus.Codes.Real;
			Container.Validation.ValidateJC_IsEmptyContainer();
			AssertNoMessageError("No error if Empty, packed and Real", Container.JC_IsEmptyContainerInfo, error);
			Container.JC_IsEmptyContainer = false;
			AssertNoMessageError("No error if not Empty, packed and Real", Container.JC_IsEmptyContainerInfo, error);
		}

		#region Implementation
		BillOfLading Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<BillOfLading>());
			}
		}

		BillOfLading shipment;
		BillOfLadingContainer Container
		{
			get
			{
				return container ?? (container = Shipment.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container;
		BillOfLadingPackLine PackLine
		{
			get
			{
				return packline ?? (packline = Shipment.OuterPackLines.AddNew());
			}
		}

		BillOfLadingPackLine packline;
		#endregion
	}
}
