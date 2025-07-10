using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using FluentValidation.TestHelper;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.ServiceHost.Tests
{
	class DeliveryDueDateQueryValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_TransportMode()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.TransportMode = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing TransportMode is mandatory.", result);

			query.TransportMode = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing TransportMode is mandatory.", result);

			query.TransportMode = "ABC"; // Some invalid TransportMode
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided TransportMode ('ABC') is not valid. It can only be one of these values: 'ALL', 'AIR', 'ULD', 'LSE', 'SEA', 'LCL', 'FCL', 'ROA', 'LRO', 'FRO', 'FTL', 'RAI', 'LRA', 'FRA', 'FWL', 'MAI'.", result);

			Assert(true);
		}

		public void TestValidate_PickupDate()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();
			validator
				.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupDate);

			query.PickupDate = default;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupDate)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing PickupDate is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_ServiceLevel()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.ServiceLevel = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.ServiceLevel)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing ServiceLevel is mandatory.", result);

			query.ServiceLevel = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.ServiceLevel)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing ServiceLevel is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_HBLDlvMode()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.HBLDlvMode = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.HBLDlvMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing HBLDlvMode is mandatory.", result);

			query.HBLDlvMode = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.HBLDlvMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing HBLDlvMode is mandatory.", result);

			query.HBLDlvMode = "SOME";
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.HBLDlvMode)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided HBLDlvMode ('SOME') is not valid. It can only be one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'CFS/DOOR', 'CFS/CFS', 'ARPT/ARPT', 'DOOR/ARPT', 'CFS/ARPT', 'ARPT/DOOR', 'ARPT/CFS'.", result);

			Assert(true);
		}

		public void TestValidate_PickupOrg()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.PickupOrg = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.PickupOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.PickupOrg = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.PickupOrg = null;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.PickupOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.PickupOrg = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_CFS;
			query.PickupOrg = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.PickupOrg = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupOrg);

			Assert(true);
		}

		public void TestValidate_PickupAddr()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.PickupAddr = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.PickupAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.PickupAddr = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.PickupAddr = null;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.PickupAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("PickupAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'DOOR/CFS', 'DOOR/ARPT'.", result);

			query.PickupAddr = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_CFS;
			query.PickupAddr = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.PickupAddr = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.PickupAddr);

			Assert(true);
		}

		public void TestValidate_PickupCFSOrg()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.PickupCFSOrg = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupCFSOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing PickupCFSOrg is mandatory.", result);

			query.PickupCFSOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupCFSOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing PickupCFSOrg is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_PickupCFSAddr()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.PickupCFSAddr = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupCFSAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing PickupCFSAddr is mandatory.", result);

			query.PickupCFSAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.PickupCFSAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing PickupCFSAddr is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_DeliveryOrg()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.DeliveryOrg = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.DeliveryOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.DeliveryOrg = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.DeliveryOrg = null;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.DeliveryOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryOrg is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.DeliveryOrg = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_CFS;
			query.DeliveryOrg = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryOrg);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.DeliveryOrg = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryOrg);

			Assert(true);
		}

		public void TestValidate_DeliveryAddr()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.DeliveryAddr = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			query.DeliveryAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.DeliveryAddr = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.DeliveryAddr = null;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			query.DeliveryAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryAddr is mandatory when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', 'ARPT/DOOR'.", result);

			query.DeliveryAddr = "Something";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_CFS;
			query.DeliveryAddr = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryAddr);

			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_CFS;
			query.DeliveryAddr = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryAddr);

			Assert(true);
		}

		public void TestValidate_DeliveryCFSOrg()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.DeliveryCFSOrg = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryCFSOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing DeliveryCFSOrg is mandatory.", result);

			query.DeliveryCFSOrg = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryCFSOrg)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing DeliveryCFSOrg is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_DeliveryCFSAddr()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.DeliveryCFSAddr = null;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryCFSAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing DeliveryCFSAddr is mandatory.", result);

			query.DeliveryCFSAddr = string.Empty;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryCFSAddr)
				.Single()
				.ErrorMessage;
			AssertEquals("Providing DeliveryCFSAddr is mandatory.", result);

			Assert(true);
		}

		public void TestValidate_DeliveryType()
		{
			var query = GetValidQuery();
			var validator = new DeliveryDueDateQueryValidator();

			query.DeliveryType = null;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryType);

			query.DeliveryType = string.Empty;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryType);

			query.DeliveryType = "DIRECTTOCNE";
			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryType);

			query.DeliveryType = "DIRECTTOCNE";
			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_DOOR;
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryType);

			query.DeliveryType = "DirectToCNE";
			validator.TestValidate(query)
				.ShouldNotHaveValidationErrorFor(q => q.DeliveryType);

			query.HBLDlvMode = HBLDeliveryModes.Codes.CFS_CFS;
			var result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryType)
				.Single()
				.ErrorMessage;
			AssertEquals("DeliveryType 'DirectToCNE' is only valid when HBLDlvMode is one of these values: 'DOOR/DOOR', 'CFS/DOOR', but 'CFS/CFS' was provided.", result);

			query.DeliveryType = "blackmarket";
			query.HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR;
			result = validator.TestValidate(query)
				.ShouldHaveValidationErrorFor(q => q.DeliveryType)
				.Single()
				.ErrorMessage;
			AssertEquals("Provided DeliveryType ('blackmarket') is not valid. It can only be 'DirectToCNE' or blank.", result);

			Assert(true);
		}

		DeliveryDueDateQuery GetValidQuery()
		{
			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupOrg.MainAddress.OA_Address1 = "Pickup Address";

			var pickupCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			pickupCFSOrg.MainAddress.OA_Address1 = "Pickup CFS Address";

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryOrg.MainAddress.OA_Address1 = "Delivery Address";

			var deliveryCFSOrg = Factory.NewWithValidTestData<OrgHeader>();
			deliveryCFSOrg.MainAddress.OA_Address1 = "Delivery CFS Address";

			var query = new DeliveryDueDateQuery()
			{
				TransportMode = "SEA",
				PickupDate = DateTime.Now,
				ServiceLevel = "STD",
				HBLDlvMode = HBLDeliveryModes.Codes.DOOR_DOOR,
				PickupOrg = pickupOrg.OH_Code,
				PickupAddr = pickupOrg.MainAddress.OA_Code,
				PickupCFSOrg = pickupCFSOrg.OH_Code,
				PickupCFSAddr = pickupCFSOrg.MainAddress.OA_Code,
				DeliveryOrg = deliveryOrg.OH_Code,
				DeliveryAddr = deliveryOrg.MainAddress.OA_Code,
				DeliveryCFSOrg = deliveryCFSOrg.OH_Code,
				DeliveryCFSAddr = deliveryCFSOrg.MainAddress.OA_Code,
			};

			return query;
		}
	}
}
