using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class RefContainerStockValidationTest : BusinessObjectValidationTestCase
	{
		public void TestR6_ContainerNum_EnforceValid()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			OtherStock.R6_ContainerNum = "FAkU4100027";
			Stock.R6_ContainerNum = "FAKU4100010";
			AssertHasError(Stock.R6_ContainerNumInfo, "Container number does not have a valid check (last) digit. The check digit should be 5.");
			Stock.R6_ContainerNum = "FAKU4100015";
			AssertNoNotifications(Stock.R6_ContainerNumInfo);
			Stock.R6_ContainerNum = "";
			AssertHasError(Stock.R6_ContainerNumInfo, "Please enter a " + Stock.R6_ContainerNumInfo.Description + ".");
			Stock.R6_ContainerNum = "FAKU4100027";
			AssertHasError(Stock.R6_ContainerNumInfo, "This container number is already in use.");
		}

		public void TestR6_ContainerNum_DontEnforceValid()
		{
			AgencyRegistry.Instance.AllowNonStandardContainerNumbersInContainerManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			OtherStock.R6_ContainerNum = "FAkU4100027";
			Stock.R6_ContainerNum = "FAKU4100010";
			AssertHasWarning(Stock.R6_ContainerNumInfo, "Container number does not have a valid check (last) digit. The check digit should be 5.");
			Stock.R6_ContainerNum = "FAKU4100015";
			AssertNoNotifications(Stock.R6_ContainerNumInfo);
			Stock.R6_ContainerNum = "";
			AssertHasError(Stock.R6_ContainerNumInfo, "Please enter a " + Stock.R6_ContainerNumInfo.Description + ".");
			Stock.R6_ContainerNum = "FAKU4100027";
			AssertHasError(Stock.R6_ContainerNumInfo, "This container number is already in use.");
		}

		public void TestR6_RC()
		{
			Stock.R6_RC = ZGuid.Missing;
			AssertHasError(Stock.R6_RCInfo, "The selected " + Stock.R6_RCInfo.Description + " is no longer valid. Please choose a new " + Stock.R6_RCInfo.Description + " from the list.");
			Stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			AssertNoNotifications(Stock.R6_RCInfo);
			Stock.R6_RC = ZGuid.Empty;
			AssertHasError(Stock.R6_RCInfo, "Please enter a " + Stock.R6_RCInfo.Description + ".");
		}

		public void TestR6_OwnerType()
		{
			Stock.R6_OwnerType = "CRP";
			AssertHasError(Stock.R6_OwnerTypeInfo, "Enter a valid " + Stock.R6_OwnerTypeInfo.Description + ".");
			Stock.R6_OwnerType = Enterprise.Core.Constants.ContainerOwnership.Codes.ShipperOwned;
			AssertNoNotifications(Stock.R6_OwnerTypeInfo);
			Stock.R6_OwnerType = "";
			AssertHasError(Stock.R6_OwnerTypeInfo, "Please enter an " + Stock.R6_OwnerTypeInfo.Description + ".");
		}

		public void TestR6_Owner()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Stock.R6_OH_Owner = ZGuid.Empty;
			AssertNoNotifications(Stock.R6_OH_OwnerInfo);
			Stock.R6_OH_Owner = ZGuid.Missing;
			AssertHasError(Stock.R6_OH_OwnerInfo, "The selected " + Stock.R6_OH_OwnerInfo.Description + " is no longer valid. Please choose a new " + Stock.R6_OH_OwnerInfo.Description + " from the list.");
			Stock.R6_OH_Owner = org.PK;
			AssertNoNotifications(Stock.R6_OH_OwnerInfo);
		}

		#region Implementation
		RefContainerStock Stock
		{
			get
			{
				return stock ?? (stock = Factory.New<RefContainerStock>());
			}
		}

		RefContainerStock stock;
		RefContainerStock OtherStock
		{
			get
			{
				return otherStock ?? (otherStock = Factory.New<RefContainerStock>());
			}
		}

		RefContainerStock otherStock;
		#endregion
	}
}
