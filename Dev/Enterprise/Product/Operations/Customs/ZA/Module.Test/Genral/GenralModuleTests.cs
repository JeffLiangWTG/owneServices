using System;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	class GenralMessageControllerTest : ZControllerTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new GenralMessageController();
			var message = Factory.New<GENRALMessage>();
			var expectedCheckpoint = Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAGenralMessage);
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForView(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForNew(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForEdit(message));
			AssertEquals(expectedCheckpoint, controller.GetCheckPointForDelete(message));
		}
	}

	class GenralMessageModuleTest : ZModuleTest
	{
	}

	[TestedType(typeof(GenralMessageFilterStripBusinessObject))]
	class GenralMessageFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestMessageTypeSearch()
		{
			var message1 = CreateTestMessage();
			message1.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			var message2 = CreateTestMessage();
			message2.EM_MessageType = CustomsResponseMessageTypeList.Codes.RES;
			Factory.Save();

			var collection = new GenralMessageCollection(Factory);
			var filterBO = GetNewFilterStrip();
			collection.LoadWithMoreFiltering(filterBO.Filter);

			AssertCollectionContains(message1, collection);
			AssertCollectionNotContains(message2, collection);
		}

		public void TestMessageSubTypeSearch()
		{
			var message1 = CreateTestMessage();
			message1.EM_MessageSubType = GenralMessageSubTypeList.Codes.PEN;
			var message2 = CreateTestMessage();
			message2.EM_MessageSubType = GenralMessageSubTypeList.Codes.UCR;
			Factory.Save();

			var collection = new GenralMessageCollection(Factory);
			var filterBO = GetNewFilterStrip();
			var filter = (ModuleTextFilter)filterBO["Purpose"];
			filter.IsActive = true;
			filter.Property = GenralMessageSubTypeList.Codes.PEN;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);

			AssertCollectionContains(message1, collection);
			AssertCollectionNotContains(message2, collection);

			filter.Property = GenralMessageSubTypeList.Codes.UCR;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			collection.LoadWithMoreFiltering(filterBO.Filter);

			AssertCollectionNotContains(message1, collection);
			AssertCollectionContains(message2, collection);
		}

		GENRALMessage CreateTestMessage()
		{
			var message = Factory.New<GENRALMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SouthAfricanCustoms;
			message.EM_MessageType = CustomsResponseMessageTypeList.Codes.GEN;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
			return message;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GenralMessageFilterStripBusinessObject();

		GenralMessageFilterStripBusinessObject GetNewFilterStrip() => (GenralMessageFilterStripBusinessObject)GetNewFilterStripBusinessObject();
	}

	[TestedType(typeof(GenralMessageModule))]
	class GenralModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.GenralMessage;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;
	}

	[TestedType(typeof(GenralMessageController))]
	class GenralControllerBasherTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType => typeof(GenralMessageController);

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;

		protected override Type GetBusinessObjectType() => typeof(GENRALMessage);

		protected override ControllerID GetControllerID() => ZAControllerIDs.GenralMessage;
	}
}
