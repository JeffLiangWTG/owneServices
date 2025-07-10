using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVConsignmentForACASWrapper))]
	public class HVLVConsignmentForACASWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConsigneeName()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeName.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeNameInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ConsigneeName = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeNameInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeNameInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeNameMaxLength, propertyMaxLength);
		}

		public void TestConsigneeAddress1()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeAddress1.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeAddress1Info.HasMessageError("You have not entered a value."));

			consignment.HVC_ConsigneeAddress1 = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeAddress1Info.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeAddress1Info.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeAddress1MaxLength, propertyMaxLength);
		}

		public void TestConsigneeCity()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeCity.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeCityInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ConsigneeCity = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeCityInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeCityInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeCityMaxLength, propertyMaxLength);
		}

		public void TestConsigneeState()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeState.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeStateInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ConsigneeState = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeStateInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeStateInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeStateMaxLength, propertyMaxLength);
		}

		public void TestConsigneePostcode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneePostcode.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneePostcodeInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ConsigneePostcode = "123";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneePostcodeInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneePostcodeInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneePostcodeMaxLength, propertyMaxLength);
		}

		public void TestConsigneeCountryCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_RN_NKConsigneeCountryCode.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeCountryCodeInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_RN_NKConsigneeCountryCode = "AB";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeCountryCodeInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeCountryCodeInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_RN_NKConsigneeCountryCodeMaxLength, propertyMaxLength);
		}

		public void TestConsigneeMobile()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeMobile.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeMobileInfo.HasMessageError("You have not entered a value. It is strongly suggested to send a value as per 21/08/24 CBP ACAS update."));

			consignment.HVC_ConsigneeMobile = "+1234567890";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeMobileInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeMobileInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeMobileMaxLength, propertyMaxLength);
		}

		public void TestConsigneeEmail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ConsigneeEmail.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ConsigneeEmailInfo.HasMessageError("You have not entered a value. It is strongly suggested to send a value as per 21/08/24 CBP ACAS update."));

			consignment.HVC_ConsigneeEmail = "testConsignee@email.com";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ConsigneeEmailInfo.HasNotifications());

			var propertyMaxLength = wrapper.ConsigneeEmailInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ConsigneeEmailMaxLength, propertyMaxLength);
		}

		public void TestShipperName()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperName.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperNameInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ShipperName = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperNameInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperNameInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperNameMaxLength, propertyMaxLength);
		}

		public void TestShipperAddress1()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperAddress1.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperAddress1Info.HasMessageError("You have not entered a value."));

			consignment.HVC_ShipperAddress1 = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperAddress1Info.HasNotifications());

			var propertyMaxLength = wrapper.ShipperAddress1Info.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperAddress1MaxLength, propertyMaxLength);
		}

		public void TestShipperCity()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperCity.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperCityInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ShipperCity = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperCityInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperCityInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperCityMaxLength, propertyMaxLength);
		}

		public void TestShipperState()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperState.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperStateInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ShipperState = "ABC";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperStateInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperStateInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperStateMaxLength, propertyMaxLength);
		}

		public void TestShipperPostcode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperPostcode.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperPostcodeInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_ShipperPostcode = "123";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperPostcodeInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperPostcodeInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperPostcodeMaxLength, propertyMaxLength);
		}

		public void TestShipperCountryCode()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_RN_NKShipperCountryCode.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperCountryCodeInfo.HasMessageError("You have not entered a value."));

			consignment.HVC_RN_NKShipperCountryCode = "AB";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperCountryCodeInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperCountryCodeInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_RN_NKShipperCountryCodeMaxLength, propertyMaxLength);
		}

		public void TestShipperMobile()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperMobile.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperMobileInfo.HasMessageError("You have not entered a value. It is strongly suggested to send a value as per 21/08/24 CBP ACAS update."));

			consignment.HVC_ShipperMobile = "+1234567890";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperMobileInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperMobileInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperMobileMaxLength, propertyMaxLength);
		}

		public void TestShipperEmail()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Assert("Precondition: property is empty", consignment.HVC_ShipperEmail.IsEmpty);

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);
			wrapper.RunPreSaveValidation();
			Assert("property has message error", wrapper.ShipperEmailInfo.HasMessageError("You have not entered a value. It is strongly suggested to send a value as per 21/08/24 CBP ACAS update."));

			consignment.HVC_ShipperEmail = "testShipper@email.com";
			wrapper.RunPreSaveValidation();
			Assert(!wrapper.ShipperEmailInfo.HasNotifications());

			var propertyMaxLength = wrapper.ShipperEmailInfo.MaxLength;
			AssertEquals(AutoHVLVConsignment.Schema.HVC_ShipperEmailMaxLength, propertyMaxLength);
		}

		public void TestGoodsDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = "HVL";
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKDestination = "USLAX";
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();

			Factory.Save();

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var wrapper = new HVLVConsignmentForACASWrapper(consignment);

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;

			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertHasMessageError("error when consignment.GoodsDescription is null and all itemLine.GoodsDescription are null",
				wrapper.GoodsDescriptionInfo, "Goods Description is required when sending ACAS report.");

			consignment.HVC_GoodsDescription = "AA";
			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertNoMessageErrors(wrapper.GoodsDescriptionInfo);

			consignment.HVC_GoodsDescription = "";
			itemLine.HVS_GoodsDescription = "AAA";
			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertNoMessageErrors(wrapper.GoodsDescriptionInfo);

			itemLine.HVS_GoodsDescription = "安";
			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertHasMessageError(wrapper.GoodsDescriptionInfo, @"This text contains characters not supported by the United States Customs (CBP).
				Only characters shown directly on a keyboard with US layout are acceptable for this message, not typed or special characters.");

			itemLine.HVS_GoodsDescription = "";
			consignment.HVC_GoodsDescription = @"We need a lot of characters Bacon ipsum dolor amet tongue meatloaf turkey prosciutto filet mignon. 
Drumstick ball tip boudin, ham fatback rump burgdoggen prosciutto. Andouille tenderloin bresaola alcatra doner.
Beef ham hock alcatra, short ribs pork belly landjaeger swine. Chuck hamburger jowl alcatra brisket.
Filet mignon boudin salami landjaeger, meatloaf ball tip buffalo cow meatball shank ribeye beef ribs. 
Corned beef turkey tongue cow ball tip.Tongue biltong landjaeger turducken, t-bone capicola shank drumstick.
Shoulder turducken porchetta sausage rump tenderloin.";
			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertHasMessageError(wrapper.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters when sending ACAS report.");

			consignment.HVC_GoodsDescription = "";
			for (var i = 0; i < 7; i++)
			{
				itemLine = item.Lines.AddNew();
				itemLine.HVS_GoodsDescription = @"We need a lot of characters Bacon ipsum dolor amet tongue meatloaf turkey";
				itemLine.HVS_Quantity = 1;
			}

			Factory.Save();
			wrapper.RunPreSaveValidation();

			AssertHasMessageError(wrapper.GoodsDescriptionInfo, "Goods Description has a limit of 490 characters when sending ACAS report.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new HVLVConsignmentForACASWrapper(Factory.New<HVLVConsignment>());
		}
	}
}
