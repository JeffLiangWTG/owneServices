using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingModuleShipmentCollection))]
	sealed class ForwardingModuleShipmentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddExactCopyFromDataRefreshBus()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			collection.Load();
			try
			{
				collection.IsManagedForDataRefresh = true;

				int oldCount = collection.Count;

				BusinessObjectFactory factory2 = new BusinessObjectFactory();
				CommonShipment shipment = CommonShipment.New(factory2);
				shipment.JS_RL_NKDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "AUMEL";

				factory2.Save();

				AssertEquals("Collection added to by DataRefreshBus", oldCount + 1, collection.Count);
				AssertEquals("Correct type added to collection", typeof(ForwardingModuleShipment), collection.FindByPK(shipment.PK).GetType());
			}
			finally
			{
				collection.IsManagedForDataRefresh = false;
			}
		}

		public void TestModuleHouseBillAir()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			BusinessObject mAWB = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			BusinessObject hAWB = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			hAWB[ZArchitecture.Schema.CusHAWBSchema.CS_CM] = mAWB.PK;
			hAWB[ZArchitecture.Schema.CusHAWBSchema.CS_MsgStatus] = "MSG";
			hAWB[ZArchitecture.Schema.CusHAWBSchema.CS_CustomsStatus] = "HBW";
			hAWB[ZArchitecture.Schema.CusHAWBSchema.CS_JS.Name] = shipment.PK;
			Factory.Save();
			AssertEquals("HBW", collection.GetCMRCustomsStatus(shipment));
			AssertEquals("MSG", collection.GetCMRMessageStatus(shipment));
		}

		public void TestModuleHouseBillSea()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			BusinessObject oBL = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusSCAOceanBill>();
			BusinessObject hBL = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusSCAHouse>();
			hBL[ZArchitecture.Schema.CusSCAHouseSchema.CA_CB] = oBL.PK;
			hBL[ZArchitecture.Schema.CusSCAHouseSchema.CA_MessageStatus] = "MSG";
			hBL[ZArchitecture.Schema.CusSCAHouseSchema.CA_ShipmentStatus] = "HBW";
			hBL[ZArchitecture.Schema.CusSCAHouseSchema.CA_JS.Name] = shipment.PK;
			Factory.Save();
			AssertEquals("HBW", collection.GetCMRCustomsStatus(shipment));
			AssertEquals("MSG", collection.GetCMRMessageStatus(shipment));
		}

		public void TestLoadCusEntryNumbersFromShipment()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			CusEntryNumber crn = Factory.New<CusEntryNumber>();
			crn.CE_ParentID = shipment.PK;
			crn.CE_ParentTable = shipment.TableName;
			crn.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			crn.CE_EntryIsSystemGenerated = true;
			crn.CE_EntryNum = "ENTRY1";
			Factory.Save();
			AssertEquals("ENTRY1", collection.GetEntryNum(shipment));
		}

		public void TestLoadCusEntryNumbersFromShipment_SG()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			CusEntryNumber crn = Factory.New<CusEntryNumber>();
			crn.CE_ParentID = shipment.PK;
			crn.CE_ParentTable = shipment.TableName;
			crn.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			crn.CE_EntryIsSystemGenerated = true;
			crn.CE_EntryNum = "ENTRY1";
			Factory.Save();
			GlbCompany.CurrentCompany.SetCountry("SG");
			AssertEquals("ENTRY1", collection.GetEntryNum(shipment));
		}

		public void TestLoadCusEntryNumbersFromDeclaration()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			BusinessObject jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			CusEntryNumber crn = Factory.New<CusEntryNumber>();
			crn.CE_ParentID = jobDeclaration.PK;
			crn.CE_ParentTable = jobDeclaration.TableName;
			crn.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			crn.CE_EntryIsSystemGenerated = true;
			crn.CE_EntryNum = "ENTRY2";
			jobDeclaration[JobDeclarationSchema.JE_EntryStatus] = "NOT";
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			Factory.Save();
			AssertEquals("ENTRY2", collection.GetEntryNum(shipment));
		}

		public void TestLoadCusEntryNumbersFromDeclarationSG()
		{
			ForwardingModuleShipmentCollection collection = new ForwardingModuleShipmentCollection(Factory);
			ForwardingModuleShipment shipment = collection.AddNew();
			BusinessObject jobDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			CusEntryNumber crn = Factory.New<CusEntryNumber>();
			crn.CE_ParentID = jobDeclaration.PK;
			crn.CE_ParentTable = jobDeclaration.TableName;
			crn.CE_EntryType = CusEntryNumberTypes.Australia.EX1;
			crn.CE_EntryIsSystemGenerated = true;
			crn.CE_EntryNum = "ENTRY2";
			jobDeclaration[JobDeclarationSchema.JE_EntryStatus] = "NOT";
			jobDeclaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			Factory.Save();
			GlbCompany.CurrentCompany.SetCountry("SG");
			AssertEquals("ENTRY2", collection.GetEntryNum(shipment));
		}

		public void TestIFilterModuleExtraNotificationProvider()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			var collection = new ForwardingModuleShipmentCollection(Factory) { ParentConsol = consol };

			var helper = new Mock<IShipmentVsConsolMessageHelper>(MockBehavior.Strict);
			using (FreightShipmentVsConsolMessageHelper.OverrideHelperInstance(helper.Object))
			{
				var attachRequest = new ShipmentConsolAttachRequest(null, null);
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, shipment)).Returns(attachRequest);
				helper.SetupProperty(p => p.IsGatewayServiceLevelCheckSuspended).SetReturnsDefault(true);
				var notificationProvider = (IFilterModuleExtraNotificationProvider)collection;
				AssertNull(notificationProvider.GetExtraNotification(null));
				AssertNull(notificationProvider.GetExtraNotification(Factory.New<DummyBusinessObject>()));

				INotification notification = notificationProvider.GetExtraNotification(shipment);
				AssertNull("Attach is allowed, no notification", notification);
				attachRequest = new ShipmentConsolAttachRequest(() => "ERROR?", null);
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, shipment)).Returns(attachRequest);
				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(shipment);
				AssertEquals("Attach is not allowed, error notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
				AssertEquals("Attach is not allowed, error notification", "ERROR?", notification.Message);
				attachRequest = new ShipmentConsolAttachRequest(null, () => "WARNING?");
				helper.Setup(m => m.IsAllowedToAttachShipment(consol, shipment)).Returns(attachRequest);
				notificationProvider = collection;
				notification = notificationProvider.GetExtraNotification(shipment);
				AssertEquals("warning notification", true, notification.Type == CargoWise.ComponentModel.NotificationType.Warning);
				AssertEquals("warning notification", "WARNING?", notification.Message);
			}
		}

		public void TestFindBoxListProvider()
		{
			var collection = new ForwardingModuleShipmentCollection(Factory);
			AssertEquals(typeof(FindBoxListProvider), collection.FindBoxListProviderExposedForTest.GetType());
			collection.AllowTemplateRecords = true;
			AssertEquals(typeof(TemplateRecordFindboxListProvider), collection.FindBoxListProviderExposedForTest.GetType());
		}

		public void TestExternalListValidationIsValidTemplateRecordPK()
		{
			var templateRecord1 = Factory.New<StmTemplateRecord>();
			templateRecord1.STR_ModuleID = nameof(ModuleId.JobShipment);
			var templateRecord2 = Factory.New<StmTemplateRecord>();
			templateRecord2.STR_ModuleID = "XYZ";
			Factory.Save();

			var collection = new ForwardingModuleShipmentCollection(new BusinessObjectFactory());
			collection.AllowTemplateRecords = true;

			Assert(((IExternalListValidation)collection).IsValidTemplateRecordPK(templateRecord1.PK));
			Assert(!((IExternalListValidation)collection).IsValidTemplateRecordPK(templateRecord2.PK));
			Assert(!((IExternalListValidation)collection).IsValidTemplateRecordPK(ZGuid.NewZGuid()));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ForwardingModuleShipmentCollection(Factory);
		}

		#endregion
	}
}
