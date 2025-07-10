using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class LicencedContainerStockManagerTest : ContainerStockManagerTest
	{
		public void TestNew()
		{
			AssertType(typeof(LicencedContainerStockManager), ContainerStockManager.New(Container));
		}

		[ExpectNoExceptions]
		public void TestNumberIsNotAcceptableWhenDoesntFit()
		{
			Container.JC_RC = RC_20GP_PK;
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Container.JC_ContainerNum = InvalidContainerNum2;
			RefContainerStock stock7 = Container.Stock;
			Container.JC_ContainerNum = "12345678901234567890";
			RefContainerStock stock8 = Container.Stock;
		}

		public void TestChangingContainerNumberDoesNotChangeForEmptyValues()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var existingStock = Factory.New<RefContainerStock>();
			existingStock.R6_ContainerNum = ValidContainerNum1;
			existingStock.R6_RC = RC_40RE_PK;
			existingStock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			var emptyStock = Factory.New<RefContainerStock>();
			emptyStock.R6_ContainerNum = ZString.Empty;
			emptyStock.R6_RC = RC_20RE_PK;
			emptyStock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			Container.JC_ContainerNum = ValidContainerNum1;
			AssertEquals("Sets Ref Container", RC_40RE_PK, Container.JC_RC);
			Container.JC_ContainerNum = ZString.Empty;
			AssertEquals("Resets Ref Container", RC_40RE_PK, Container.JC_RC);
		}

		public void TestChangingContainerNumber()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			RefContainerStock existingStock1 = Factory.New<RefContainerStock>();
			existingStock1.R6_ContainerNum = ValidContainerNum1;
			existingStock1.R6_RC = RC_40RE_PK;
			existingStock1.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			RefContainerStock existingStock2 = Factory.New<RefContainerStock>();
			existingStock2.R6_ContainerNum = ValidContainerNum4;
			existingStock2.R6_RC = RC_40RE_PK;
			existingStock2.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.CarrierOwned;
			Factory.Save();
			Container.JC_RC = RC_20GP_PK;
			Container.JC_ContainerNum = "Container1";
			AssertNull("Invalid Container Number", Container.Stock);
			Container.JC_ContainerNum = InvalidContainerNum1;
			AssertNull("Invalid Container Number", Container.Stock);
			Container.JC_ContainerNum = ValidContainerNum2;
			RefContainerStock stock1 = Container.Stock;
			AssertNotNull("Valid continer number", stock1);
			AssertEquals("Should have the correct owner type", "", stock1.R6_OwnerType);
			AssertEquals("Found the correct stock.", ValidContainerNum2, stock1.R6_ContainerNum);
			AssertEquals(ValidContainerNum2 + " is a new container number and so should not be in the database.", false, stock1.IsInDatabase);
			AssertEquals(ValidContainerNum2 + " should be 20GP.", RC_20GP_PK, stock1.R6_RC);
			Container.JC_ContainerNum = ValidContainerNum1;
			RefContainerStock stock2 = Container.Stock;
			AssertNotNull("Valid container number", stock2);
			AssertEquals("Found the correct stock.", existingStock1.PK, stock2.PK);
			AssertEquals("Should have updated the container type.", RC_40RE_PK, Container.JC_RC);
			AssertEquals("Should be shipper owned", true, Container.JC_IsShipperOwned);
			AssertEquals("Should have deleted the old unused stock.", true, stock1.IsDeleted);
			Container.JC_ContainerNum = ValidContainerNum3;
			RefContainerStock stock3 = Container.Stock;
			AssertNotNull("Valid container number", stock3);
			AssertEquals("Found the correct stock", ValidContainerNum3, stock3.R6_ContainerNum);
			AssertEquals("Should not have changed the container type", RC_40RE_PK, Container.JC_RC);
			AssertEquals("Should be shipper owned", true, Container.JC_IsShipperOwned);
			AssertEquals("Should not have deleted the existing stock", false, stock2.IsDeleted);
			Container.JC_ContainerNum = ValidContainerNum4;
			RefContainerStock stock4 = Container.Stock;
			AssertNotNull("Valid container number", stock4);
			AssertEquals("Found the correct stock.", existingStock2.PK, stock4.PK);
			AssertEquals("Should be shipper owned", false, Container.JC_IsShipperOwned);
			AssertEquals("Should have updated the container type.", RC_40RE_PK, Container.JC_RC);
			AssertEquals("Should have deleted the old unused stock.", true, stock3.IsDeleted);
			Container.JC_ContainerNum = "";
			AssertNull("Should not reference a stock.", Container.Stock);
			AssertEquals("Should not have changed the container type", RC_40RE_PK, Container.JC_RC);
			AssertEquals("Should be shipper owned", false, Container.JC_IsShipperOwned);
			Container.JC_ContainerNum = ValidContainerNum2;
			RefContainerStock stock5 = Container.Stock;
			AssertNotNull("Valid container number", stock5);
			Container.JC_ContainerNum = "";
			AssertNull("Should not reference a stock.", Container.Stock);
			AssertEquals("Should have deleted the existing stock", true, stock5.IsDeleted);
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Container.JC_ContainerNum = InvalidContainerNum1;
			RefContainerStock stock6 = Container.Stock;
			AssertNotNull("Invalid continer number, but invalid numbers are allowed now", stock6);
			AssertEquals("Stock should not have an owner type set.", "SHP", stock6.R6_OwnerType);
			AssertEquals("Stock should have the correct container number.", InvalidContainerNum1, stock6.R6_ContainerNum);
			AssertEquals(InvalidContainerNum1 + " is a new container number and so should not be in the database.", false, stock6.IsInDatabase);
			AssertEquals(InvalidContainerNum1 + " should be 40RE.", RC_40RE_PK, stock6.R6_RC);
			Container.JC_ContainerNum = InvalidContainerNum2;
			RefContainerStock stock7 = Container.Stock;
			AssertNotNull("Invalid continer number, but invalid numbers are allowed now", stock7);
			AssertEquals("Stock should not have an owner type set.", "SHP", stock7.R6_OwnerType);
			AssertEquals("Stock should have the correct container number.", InvalidContainerNum2, stock7.R6_ContainerNum);
			AssertEquals(InvalidContainerNum2 + " is a new container number and so should not be in the database.", false, stock7.IsInDatabase);
			AssertEquals(InvalidContainerNum2 + " should be 40RE.", RC_40RE_PK, stock7.R6_RC);
		}

		public void TestChangingContainerType()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Container.JC_ContainerNum = ValidContainerNum1;
			Container.JC_RC = ZGuid.Empty;
			AssertNull("No container type.", Container.Stock);
			Container.JC_RC = RC_40GP_PK;
			RefContainerStock stock1 = Container.Stock;
			AssertNotNull("Valid container type.", stock1);
			AssertEquals("New container stock.", false, stock1.IsInDatabase);
			AssertEquals("Should have the correct container type.", RC_40GP_PK, stock1.R6_RC);
			AssertEquals("Should have the correct owner type", "", stock1.R6_OwnerType);
			Container.JC_RC = RC_20RE_PK;
			AssertEquals("Should not have changed stock.", stock1.PK, Container.Stock.PK);
			AssertEquals("Should have updated the container type if not saved.", RC_20RE_PK, stock1.R6_RC);
			Container.JC_RC = ZGuid.Empty;
			AssertNull("Should not be referencing a stock object.", Container.Stock);
			AssertEquals("Referenced stock was not saved and so should be deleted.", true, stock1.IsDeleted);
			Container.JC_IsShipperOwned = true;
			Container.JC_RC = RC_20RE_PK;
			RefContainerStock stock2 = Container.Stock;
			AssertEquals("Should not have changed stock.", stock2.PK, Container.Stock.PK);
			AssertEquals("Should have updated the container type if not saved.", RC_20RE_PK, stock2.R6_RC);
			AssertEquals("Should have the correct owner type", Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned, stock2.R6_OwnerType);
			Factory.Save();
			Container.JC_RC = RC_40GP_PK;
			AssertEquals("Should not have changed stock.", stock2.PK, Container.Stock.PK);
			AssertEquals("Should not have updated the container type if saved.", RC_20RE_PK, stock2.R6_RC);
			Container.JC_RC = ZGuid.Empty;
			AssertEquals("Referenced stock was saved and so should not be deleted.", false, stock2.IsDeleted);
			Container.JC_ContainerNum = InvalidContainerNum1;
			Container.JC_RC = RC_40GP_PK;
			AssertNull("Container number is invalid", Container.Stock);
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Container.JC_RC = RC_20RE_PK;
			RefContainerStock stock3 = Container.Stock;
			AssertNotNull("Should have found a container stock.", stock3);
			AssertEquals("Should have the correct container number", InvalidContainerNum1, stock3.R6_ContainerNum);
			AssertEquals("Should have the correct container type", RC_20RE_PK, stock3.R6_RC);
		}

		public void TestIsShipperOwnedChangingOwnerType()
		{
			Container.JC_ContainerNum = ValidContainerNum3;
			AssertNull("Should not reference a stock.", Container.Stock);
			Container.JC_RC = RC_40GP_PK;
			RefContainerStock stock1 = Container.Stock;
			Container.JC_IsShipperOwned = true;
			AssertEquals(Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned, stock1.R6_OwnerType);
			Container.JC_IsShipperOwned = false;
			AssertEquals("", stock1.R6_OwnerType);
			Factory.Save();
		}

		const string ValidContainerNum1 = "TEST4100013";
		const string ValidContainerNum2 = "TEST4100029";
		const string ValidContainerNum3 = "TEST4100034";
		const string ValidContainerNum4 = "TEST4100040";
		const string InvalidContainerNum1 = "TEST4100011";
		const string InvalidContainerNum2 = "TEST4100012";
	}
}
