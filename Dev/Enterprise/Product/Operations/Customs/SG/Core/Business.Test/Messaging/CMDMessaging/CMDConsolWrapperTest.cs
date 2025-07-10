using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDConsolWrapper))]
	class CMDConsolWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCMDShipments()
		{
			AssertEquals("Should only include air shipments", 2, ConsolWrapper.CMDShipments.Length);
			AssertEquals("AIR01", ConsolWrapper.CMDShipments[0].Shipment.JS_UniqueConsignRef);
			AssertEquals("AIR02", ConsolWrapper.CMDShipments[1].Shipment.JS_UniqueConsignRef);
		}

		public void TestShipmentsCanBeCMDShipments()
		{
			fConsol = Factory.New<ForwardingConsol>();
			Consol.JK_UniqueConsignRef = "C002";
			Consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Consol.JK_RL_NKLoadPort = "HKHKG";
			Consol.JK_RL_NKDischargePort = "IDJKT";
			SetupShipmentCollection();
			AssertEquals("Should only include air shipments that orginate or terminate in SG - i.e. SG Import or Export", 0, ConsolWrapper.CMDShipments.Length);
		}

		public void TestCMDShipmentsRebuiltWhenShipmentsCountIsChanged()
		{
			AssertEquals("Pre-condition", 2, ConsolWrapper.CMDShipments.Length);
			ForwardingShipment newAirShipment = Consol.Shipments.AddNew();
			newAirShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			ForwardingShipment newSeaShipment = Consol.Shipments.AddNew();
			newSeaShipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("CMDShipments collection should be rebuilt", 3, ConsolWrapper.CMDShipments.Length);
		}

		public void TestRunPreSendValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(ConsolWrapperWithoutShipments.Consol);
			ConsolWrapperWithoutShipments.RunPreSendValidation(notificationBuffer);
			ZString expectedErrorMessage = string.Format("{0}\r\nThere is no Shipment to send/delete the CMD Messages for", ConsolWrapperWithoutShipments.Consol.HumanReadableName);
			AssertErrorMessage(notificationBuffer, expectedErrorMessage);
			notificationBuffer = new CMDNotificationBuffer(ConsolWrapper);
			ConsolWrapper.RunPreSendValidation(notificationBuffer);
			expectedErrorMessage = Consol.Shipments[0].HumanReadableName + "\r\nThere is no Exemption Code or Permit Numbers entered to send\r\nTotal Outer Packs has to be greater than zero\r\n\r\n" + Consol.Shipments[2].HumanReadableName + "\r\nThere is no Exemption Code or Permit Numbers entered to send\r\nTotal Outer Packs has to be greater than zero";
			AssertErrorMessage(notificationBuffer, expectedErrorMessage);
		}

		public void TestRunPreDeleteValidation()
		{
			CMDNotificationBuffer notificationBuffer = new CMDNotificationBuffer(ConsolWrapperWithoutShipments.Consol);
			ConsolWrapperWithoutShipments.RunPreDeleteValidation(notificationBuffer);
			ZString expectedErrorMessage = string.Format("{0}\r\nThere is no Shipment to send/delete the CMD Messages for", ConsolWrapperWithoutShipments.Consol.HumanReadableName);
			AssertErrorMessage(notificationBuffer, expectedErrorMessage);
			notificationBuffer = new CMDNotificationBuffer(ConsolWrapper);
			ConsolWrapper.RunPreDeleteValidation(notificationBuffer);
			expectedErrorMessage = Consol.Shipments[0].HumanReadableName + "\r\nThere is no previously sent CMD Messages to be deleted\r\n\r\n" + Consol.Shipments[2].HumanReadableName + "\r\nThere is no previously sent CMD Messages to be deleted";
			AssertErrorMessage(notificationBuffer, expectedErrorMessage);
		}

		#region Send / Delete CMD Messages
		public void TestSendMessage()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			ConsolWrapperForSendDeleteTests.SendMessage(notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[0] as CMDShipmentWrapperForTest, "SendMessage", notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[1] as CMDShipmentWrapperForTest, "SendMessage", notificationBuffer);
		}

		public void TestSendMessageWithRecipient()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			ConsolWrapperForSendDeleteTests.SendMessage("MEH", notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[0] as CMDShipmentWrapperForTest, "SendMessageWithRecipient", "MEH", notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[1] as CMDShipmentWrapperForTest, "SendMessageWithRecipient", "MEH", notificationBuffer);
		}

		public void TestDeleteExistingCMDMessages()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			ConsolWrapperForSendDeleteTests.DeleteExistingCMDMessages(notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[0] as CMDShipmentWrapperForTest, "DeleteExistingCMDMessages", notificationBuffer);
			AssertCMDShipmentWrapperForTest(ConsolWrapperForSendDeleteTests.CMDShipments[1] as CMDShipmentWrapperForTest, "DeleteExistingCMDMessages", notificationBuffer);
		}

		CMDConsolWrapper ConsolWrapperForSendDeleteTests
		{
			get
			{
				if (fConsolWrapperForSendDeleteTests == null)
				{
					fConsolWrapperForSendDeleteTests = new CMDConsolWrapperForTest(Factory);
				}

				return fConsolWrapperForSendDeleteTests;
			}
		}

		void AssertCMDShipmentWrapperForTest(CMDShipmentWrapperForTest shipmentWrapper, string expectedLastMethodCalled, params object[] expectedLastMethodArgs)
		{
			AssertEquals("Different number of arguments passed in", expectedLastMethodArgs.Length, shipmentWrapper.LastMethodCallArguments.Length);
			for (int i = 0; i < expectedLastMethodArgs.Length; i++)
			{
				AssertEquals(expectedLastMethodArgs[i], shipmentWrapper.LastMethodCallArguments[i]);
			}

			AssertEquals(expectedLastMethodCalled, shipmentWrapper.LastMethodCalled);
		}

		CMDConsolWrapper fConsolWrapperForSendDeleteTests;
		class CMDConsolWrapperForTest : CMDConsolWrapper
		{
			public CMDConsolWrapperForTest(BusinessObjectFactory factory) : base(factory.New<ForwardingConsol>())
			{
			}

			public override CMDShipmentWrapper[] CMDShipments
			{
				get
				{
					if (fCMDShipments == null)
					{
						fCMDShipments = new CMDShipmentWrapperForTest[] { new CMDShipmentWrapperForTest(Factory), new CMDShipmentWrapperForTest(Factory) };
					}

					return fCMDShipments;
				}
			}

			CMDShipmentWrapper[] fCMDShipments;
		}

		class CMDShipmentWrapperForTest : CMDShipmentWrapper
		{
			public CMDShipmentWrapperForTest(BusinessObjectFactory factory) : base(factory.New<ForwardingShipment>())
			{
			}

			public override void SendMessage(INotifications notifications)
			{
				LastMethodCalled = "SendMessage";
				LastMethodCallArguments = new object[] { notifications };
			}

			public override void SendMessage(string recipient, INotifications notifications)
			{
				LastMethodCalled = "SendMessageWithRecipient";
				LastMethodCallArguments = new object[] { recipient, notifications };
			}

			public override void DeleteExistingCMDMessages(INotifications notifications)
			{
				LastMethodCalled = "DeleteExistingCMDMessages";
				LastMethodCallArguments = new object[] { notifications };
			}

			public string LastMethodCalled;
			public object[] LastMethodCallArguments;
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			return new CMDConsolWrapper(consol);
		}

		CMDConsolWrapper ConsolWrapper
		{
			get
			{
				if (fConsolWrapper == null)
				{
					fConsolWrapper = new CMDConsolWrapper(Consol);
				}

				return fConsolWrapper;
			}
		}

		CMDConsolWrapper ConsolWrapperWithoutShipments
		{
			get
			{
				if (fConsolWrapperWithoutShipments == null)
				{
					ForwardingConsol consolWithoutShipments = Factory.New<ForwardingConsol>();
					consolWithoutShipments.JK_UniqueConsignRef = "NOSHIPMENTS";
					fConsolWrapperWithoutShipments = new CMDConsolWrapper(consolWithoutShipments);
				}

				return fConsolWrapperWithoutShipments;
			}
		}

		ForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<ForwardingConsol>();
					fConsol.JK_UniqueConsignRef = "C001";
					fConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
					fConsol.JK_RL_NKLoadPort = "SGSIN";
					fConsol.JK_RL_NKDischargePort = "DEFRA";
					SetupShipmentCollection();
				}

				return fConsol;
			}
		}

		void AssertErrorMessage(CMDNotificationBuffer notificationBuffer, ZString expectedErrorMessages)
		{
			AssertEquals(expectedErrorMessages, notificationBuffer.GetErrorMessages());
		}

		void SetupShipmentCollection()
		{
			ForwardingShipment airShipment1 = Consol.Shipments.AddNew();
			airShipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
			airShipment1.JS_UniqueConsignRef = "AIR01";
			airShipment1.JS_TotalPackageCount = 1;
			ForwardingShipment seaShipment1 = Consol.Shipments.AddNew();
			seaShipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			seaShipment1.JS_UniqueConsignRef = "SEA01";
			seaShipment1.JS_TotalPackageCount = 1;
			ForwardingShipment airShipment2 = Consol.Shipments.AddNew();
			airShipment2.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			airShipment2.JS_UniqueConsignRef = "AIR02";
			airShipment2.JS_TotalPackageCount = 1;
			ForwardingShipment seaShipment2 = Consol.Shipments.AddNew();
			seaShipment2.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			seaShipment2.JS_UniqueConsignRef = "SEA02";
			seaShipment2.JS_TotalPackageCount = 1;
			ForwardingShipment roadShipment = Consol.Shipments.AddNew();
			roadShipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			roadShipment.JS_UniqueConsignRef = "ROAD1";
			roadShipment.JS_TotalPackageCount = 1;
			ForwardingShipment otherShipment = Consol.Shipments.AddNew();
			otherShipment.JS_TransportMode = Core.Constants.TransportModes.Other;
			otherShipment.JS_UniqueConsignRef = "OTH01";
			otherShipment.JS_TotalPackageCount = 1;
		}

		CMDConsolWrapper fConsolWrapperWithoutShipments;
		CMDConsolWrapper fConsolWrapper;
		ForwardingConsol fConsol;
		#endregion
	}
}
