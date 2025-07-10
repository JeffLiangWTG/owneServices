using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using SystemDataRegistry = Enterprise.Registry.Business.SystemDataRegistry;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.DataTransfer.Testing
{
	public abstract class ConsolValueObjectDataAdapterRegistrySettingsTest : BaseFreightTest
	{
		public void TestUpdatingOnImportCorrectWithRespectToRegistryItemsWhenRunningABatch()
		{
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: IsBatchProcessor", true, Env.CurrentUser.IsBatchProcessor);

				foreach (bool updateConsol in BooleanValues)
				{
					foreach (bool updateSailing in BooleanValues)
					{
						foreach (bool updateRouting in BooleanValues)
						{
							foreach (bool updateShipments in BooleanValues)
							{
								foreach (bool updateContainers in BooleanValues)
								{
									SetRegistryValues(updateConsol, updateRouting, updateSailing, updateShipments, updateContainers);
									AssertUpdateingOnImportCorrectWithRespectToRegistryItems(updateConsol, updateRouting, updateSailing, updateShipments, updateContainers);
								}
							}
						}
					}
				}
			}
		}

		public void TestInsertingOnImportCorrectWithRespectToRegistryItemsWhenRunningABatch()
		{
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: IsBatchProcessor", true, Env.CurrentUser.IsBatchProcessor);

				foreach (bool updateConsol in BooleanValues)
				{
					foreach (bool updateRouting in BooleanValues)
					{
						foreach (bool updateSailing in BooleanValues)
						{
							foreach (bool updateShipments in BooleanValues)
							{
								foreach (bool updateContainers in BooleanValues)
								{
									SetRegistryValues(updateConsol, updateRouting, updateSailing, updateShipments, updateContainers);
									AssertInsertOnImportCorrectWithRespectToRegistryItems();
								}
							}
						}
					}
				}
			}
		}

		public void TestUpdateWhenNotBatch()
		{
			SetAlwayscheckRegistryValue();

			foreach (bool updateConsol in BooleanValues)
			{
				foreach (bool updateRouting in BooleanValues)
				{
					foreach (bool updateSailing in BooleanValues)
					{
						foreach (bool updateShipments in BooleanValues)
						{
							foreach (bool updateContainers in BooleanValues)
							{
								SetRegistryValues(updateConsol, updateRouting, updateSailing, updateShipments, updateContainers);
								AssertUpdateOnImportCorrectWithRespecttoAlwaysCheckFlagOnManualImport(updateConsol, updateRouting, updateSailing, updateContainers, updateShipments);
							}
						}
					}
				}
			}
		}

		#region Assert specific values update correctly

		void AssertInsertOnImportCorrectWithRespectToRegistryItems()
		{
			ConsolCollection consols = GetNewConsolsWithSpecificOriginalValues(Factory);
			Factory.Save();
			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();
			NotificationBuffer notify = new NotificationBuffer();

			var anotherFactory = new BusinessObjectFactory();
			foreach (CommonConsol consol in consols)
			{
				Xsd.Consol consolValue = adapter.ExportToValueObject(consol, new ValueObjectExportContext(notify));
				ValueObjectImportContext context = new ValueObjectImportContext(anotherFactory, notify);
				CommonConsol importedConsol = adapter.CreateOrUpdateFromValueObject(consolValue, context);
				Transport importedTransport = importedConsol.Transports.MostInterestingTransport;
				AssertNewTransportGotSpecificValue(importedTransport);
				AssertNewConsolGotSpecificValue(importedConsol);
				AssertNewShipmentsGotSpecificValue(importedConsol.Shipments);
				AssertNewContainersGotSpecificValue(importedConsol.Containers);
			}
			anotherFactory.Save();
		}

		void AssertUpdateOnImportCorrectWithRespecttoAlwaysCheckFlagOnManualImport(bool updateConsol, bool updateRouting, bool updateSailing, bool updateContainers, bool updateShipments)
		{
			AssertEquals("Precondition: Not IsBatchProcessor", false, Env.CurrentUser.IsBatchProcessor);
			ConsolCollection consols = GetAndSaveNewConsolsWithSpecificOriginalValues(Factory);
			Factory.Save();

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();

			TestCreateOrUpdateFromValueObject_NotificationSubscriber notify = new TestCreateOrUpdateFromValueObject_NotificationSubscriber(updateConsol, updateRouting, updateSailing, updateContainers, updateContainers, updateShipments);

			foreach (CommonConsol consol in consols)
			{
				Xsd.Consol consolValue = adapter.ExportToValueObject(consol, new ValueObjectExportContext(notify));
				ChangeValuesOnConsolAndRelatedObjects(consol);
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
				CommonConsol importedConsol = adapter.CreateOrUpdateFromValueObject(consolValue, context);
				AssertRoutingUpdatedCorrectly(consol.Transports[0], importedConsol.Transports[0], updateConsol, updateRouting);
				AssertTransportSpecificValueUpdatedCorrectly(consol.Transports[0], importedConsol.Transports[0], updateConsol, updateRouting, updateSailing);
				AssertConsolSpecificValueUpdatedCorrectly(consol, importedConsol, updateConsol);
				AssertShipmentsSpecificValueUpdatedCorrectly(consol.Shipments, importedConsol.Shipments, updateConsol, updateShipments);
				AssertContainersSpecificValueUpdatedCorrectly(consol.Containers, importedConsol.Containers, updateConsol, updateContainers);
			}
			DeleteConsolsAndChildElementsFromDatabase(consols, Factory);
		}

		void AssertUpdateingOnImportCorrectWithRespectToRegistryItems(bool updateConsol, bool updateRouting, bool updateSailing, bool updateShipments, bool updateContainers)
		{
			ConsolCollection consols = GetAndSaveNewConsolsWithSpecificOriginalValues(Factory);
			Factory.Save();

			ConsolValueObjectDataAdapterForTest<CommonConsol> adapter = new ConsolValueObjectDataAdapterForTest<CommonConsol>();

			NotificationBuffer notify = new NotificationBuffer();
			foreach (CommonConsol consol in consols)
			{
				Xsd.Consol consolValue = adapter.ExportToValueObject(consol, new ValueObjectExportContext(notify));
				ChangeValuesOnConsolAndRelatedObjects(consol);
				ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
				CommonConsol importedConsol = adapter.CreateOrUpdateFromValueObject(consolValue, context);
				AssertTransportSpecificValueUpdatedCorrectly(consol.Transports[0], importedConsol.Transports[0], updateConsol, updateRouting, updateSailing);
				AssertConsolSpecificValueUpdatedCorrectly(consol, importedConsol, updateConsol);
				AssertShipmentsSpecificValueUpdatedCorrectly(consol.Shipments, importedConsol.Shipments, updateConsol, updateShipments);
				AssertContainersSpecificValueUpdatedCorrectly(consol.Containers, importedConsol.Containers, updateConsol, updateContainers);
			}
			DeleteConsolsAndChildElementsFromDatabase(consols, Factory);
		}

		void AssertTransportSpecificValueUpdatedCorrectly(Transport original, Transport @new, bool updateConsol, bool updateRouting, bool updateSailing)
		{
			const string format = "\r\nUpdate Consol: {0}\r\nUpdate Routing: {1}\r\nUpdate Sailing: {2}\r\n";
			string extraDetails = string.Format(format, updateConsol, updateRouting, updateSailing);
			AssertSameBizO(original, @new);

			if (SailingShouldUpdate(updateConsol, updateRouting, updateSailing))
			{
				extraDetails = " Sailing Should have imported so date will have been set back to it's original value" + extraDetails;
				AssertEquals("ETA:" + extraDetails, E_ARVOriginal, @new.JW_ETA);
				AssertEquals("ETD:" + extraDetails, E_DEPOriginal, @new.JW_ETD);
				AssertEquals("DocsCutOff:" + extraDetails, CutoffOriginal, @new.Sailing.JX_JA_DocumentaryCutoff);
			}
			else
			{
				extraDetails = " Sailing should not have imported so date will still be new value" + extraDetails;
				AssertEquals("ETA:" + extraDetails, E_ARVNew, @new.JW_ETA);
				AssertEquals("ETD:" + extraDetails, E_DEPNew, @new.JW_ETD);
				AssertEquals("DocsCutOff:" + extraDetails, CutoffNew, @new.Sailing.JX_JA_DocumentaryCutoff);
			}
		}

		void AssertRoutingUpdatedCorrectly(Transport original, Transport @new, bool updateConsol, bool updateRouting)
		{
			const string format = "\r\nUpdate Consol: {0}\r\nUpdate Routing: {1}\r\n";
			string extraDetails = string.Format(format, updateConsol, updateRouting);
			AssertSameBizO(original, @new);

			if (RoutingShouldUpdate(updateConsol, updateRouting))
			{
				extraDetails = " Routing Should have imported" + extraDetails;
				AssertEquals("TransportType:" + extraDetails, TransportTypeOriginal, @new.JW_TransportType);
			}
			else
			{
				extraDetails = " Routing should not have imported" + extraDetails;
				AssertEquals("TransportType:" + extraDetails, TransportTypeNew, @new.JW_TransportType);
			}
		}

		void AssertConsolSpecificValueUpdatedCorrectly(CommonConsol original, CommonConsol @new, bool updateConsol)
		{
			const string format = "\r\nUpdate Consol:{0}\r\n";
			string extraDetails = string.Format(format, updateConsol);

			AssertSameBizO(original, @new);
			if (updateConsol)
			{
				AssertEquals("Consol should have imported so agent type will have been set back to it's original value" + extraDetails, AgentTypeValueOriginal, @new.JK_AgentType);
			}
			else
			{
				AssertEquals("Consol should not have imported so agent type will still be new value " + extraDetails, AgentTypeValueNew, @new.JK_AgentType);
			}
		}

		void AssertNewConsolGotSpecificValue(CommonConsol @new)
		{
			AssertEquals("Consol should have inserted so agent type will be specific value", AgentTypeValueOriginal, @new.JK_AgentType);
		}

		void AssertNewTransportGotSpecificValue(Transport transport)
		{
			string extraDetails = " Sailing should have imported so date will be specific value";
			AssertEquals("ETD:" + extraDetails, E_DEPOriginal, transport.JW_ETD);
			AssertEquals("ATD:" + extraDetails, A_DEPOriginal, transport.JW_ATD);
			AssertEquals("ETA:" + extraDetails, E_ARVOriginal, transport.JW_ETA);
			AssertEquals("ATA:" + extraDetails, A_ARVOriginal, transport.JW_ATA);
			AssertEquals("DocsCutOff:" + extraDetails, CutoffOriginal, transport.Sailing.JX_JA_DocumentaryCutoff);
		}

		void AssertShipmentsSpecificValueUpdatedCorrectly(ConsolShipmentCollection originalShipments, ConsolShipmentCollection newShipments, bool updateConsol, bool updateShipments)
		{
			const string format = "\r\nUpdate Consol: {0}\r\nUpdate Shipments: {1}\r\n";
			string extraDetails = string.Format(format, updateConsol, updateShipments);

			foreach (CommonShipment shipment in newShipments)
			{
				AssertCollectionContains(originalShipments, shipment);
				if (ShipmentsShouldUpdate(updateConsol, updateShipments))
				{
					AssertEquals("Shipment should have imported so transport mode will have been set back to it's original value" + extraDetails, TransportModeOriginal, shipment.JS_TransportMode);
				}
				else
				{
					AssertEquals("Shipment not should not have imported so transport mode will still be new value" + extraDetails, TransportModeNew, shipment.JS_TransportMode);
				}
			}
		}

		void AssertNewShipmentsGotSpecificValue(ConsolShipmentCollection newShipments)
		{
			foreach (CommonShipment shipment in newShipments)
			{
				AssertEquals("Shipment should have imported so transport mode will have specific value", TransportModeOriginal, shipment.JS_TransportMode);
			}
		}

		void AssertContainersSpecificValueUpdatedCorrectly(CommonContainerCollection originalContainers, CommonContainerCollection newContainers, bool updateConsol, bool updateContainers)
		{
			string format = "\r\nUpdate Consol: {0}\r\nUpdate Containers: {1}\r\n";
			string extraDetails = string.Format(format, updateConsol, updateContainers);

			foreach (CommonContainer container in newContainers)
			{
				AssertCollectionContains(originalContainers, container);
				if (ContainersShouldUpdate(updateConsol, updateContainers))
				{
					AssertEquals("Container should have imported so container mode will have been set back to it's original value" + extraDetails, ContainerModeOriginal, container.JC_ContainerMode);
				}
				else
				{
					AssertEquals("Container should not have imported so container mode will still be new value" + extraDetails, ContainerModeNew, container.JC_ContainerMode);
				}
			}
		}

		void AssertNewContainersGotSpecificValue(CommonContainerCollection newContainers)
		{
			foreach (CommonContainer container in newContainers)
			{
				AssertEquals("Container should have imported so container mode will be specific value", ContainerModeOriginal, container.JC_ContainerMode);
			}
		}

		void AssertSameBizO(BusinessObject original, BusinessObject @new)
		{
			AssertEquals("Data adapter should always find existing object", original.PK, @new.PK);
		}

		void AssertCollectionContains(BusinessObjectCollection collection, BusinessObject @object)
		{
			AssertEquals("Data adapter should always find existing object", true, collection.Contains(@object.PK));
		}

		#endregion

		#region Should Children udpate abstracts
		protected abstract bool RoutingShouldUpdate(bool updateConsol, bool updateRouting);
		protected abstract bool SailingShouldUpdate(bool updateConsol, bool updateRouting, bool updateSailing);
		protected abstract bool ShipmentsShouldUpdate(bool updateConsol, bool updateShipments);
		protected abstract bool ContainersShouldUpdate(bool updateConsol, bool updateContainers);
		#endregion

		#region Get new and initialise consols and children

		ConsolCollection GetAndSaveNewConsolsWithSpecificOriginalValues(BusinessObjectFactory factory)
		{
			ConsolCollection consols = GetNewConsolsWithSpecificOriginalValues(factory);
			factory.Save();
			return consols;
		}

		ConsolCollection GetNewConsolsWithSpecificOriginalValues(BusinessObjectFactory factory)
		{
			CommonShipment shipment = factory.New<CommonShipment>();
			InitialiseShipment(shipment);
			ConsolCollection consols = new ConsolCollection(shipment);
			InitialiseConsolAndRelatedObjects(consols.AddNew());
			return consols;
		}

		void InitialiseConsolAndRelatedObjects(CommonConsol consol)
		{
			consol.JK_AgentType = AgentTypeValueOriginal;
			consol.JK_TransportMode = TransportModeOriginal;
			InitialiseSailing(consol);
			InitialiseContainer(consol.Containers.AddNew(), "1");
			InitialiseContainer(consol.Containers.AddNew(), "2");
		}

		void InitialiseSailing(CommonConsol consol)
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_TransportType = TransportTypeOriginal;
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_VoyageFlight = "voyageno";
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_ETA = E_ARVOriginal;
			transport.JW_ETD = E_DEPOriginal;
			transport.JW_ATD = A_DEPOriginal;
			transport.JW_ATA = A_ARVOriginal;
			transport.JW_DocumentaryCutOff = CutoffOriginal;

			consol.JK_MasterBillNum = "TEST";
		}

		void InitialiseContainer(CommonContainer container, string iD)
		{
			container.JC_ContainerMode = ContainerModeOriginal;
			container.JC_ContainerNum = iD;
		}

		void InitialiseShipment(CommonShipment shipment)
		{
			shipment.JS_TransportMode = TransportModeOriginal;
			shipment.JS_HouseBill = "TEST";
			shipment.ConsignorPK = Consignor.PK;
			shipment.ConsigneePK = Consignee.PK;
		}

		OrgHeader Consignor
		{
			get { return consignor ?? (consignor = Factory.LoadTop1<OrgHeader>(new ZQuery())); }
		}
		OrgHeader consignor;

		OrgHeader Consignee
		{
			get { return consignee ?? (consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, Consignor.PK))); }
		}
		OrgHeader consignee;

		#endregion

		#region Change specific values
		void ChangeConsolSpecificValue(CommonConsol consol)
		{
			consol.JK_AgentType = AgentTypeValueNew;
		}

		void ChangeTransportSpecificValue(Transport transport)
		{
			transport.JW_ETD = E_DEPNew;
			transport.JW_ETA = E_ARVNew;
			transport.JW_DocumentaryCutOff = CutoffNew;
			transport.JW_TransportType = TransportTypeNew;
		}

		void ChangeShipmentsSpecificValue(ConsolShipmentCollection shipments)
		{
			foreach (CommonShipment shipment in shipments)
			{
				shipment.JS_TransportMode = TransportModeNew;
			}
		}

		void ChangeContainersSpecificValue(CommonContainerCollection containers)
		{
			foreach (CommonContainer container in containers)
			{
				container.JC_ContainerMode = ContainerModeNew;
			}
		}

		void ChangeValuesOnConsolAndRelatedObjects(CommonConsol consol)
		{
			ChangeConsolSpecificValue(consol);
			ChangeTransportSpecificValue(consol.Transports[0]);
			ChangeShipmentsSpecificValue(consol.Shipments);
			ChangeContainersSpecificValue(consol.Containers);
		}

		#endregion

		#region Clean Up factories and database during tests

		void DeleteConsolsAndChildElementsFromDatabase(ConsolCollection consols, BusinessObjectFactory factory)
		{
			JobVoyage[] voyages1 = (JobVoyage[])factory.Load(typeof(JobVoyage), new ZQuery());
			BusinessObjectFactory currentFac = consols[0].Factory;
			DeleteConsolsAndChildElementsFromDatabase(consols);
			JobVoyage[] voyages2 = (JobVoyage[])factory.Load(typeof(JobVoyage), new ZQuery());
		}

		void DeleteConsolsAndChildElementsFromDatabase(ConsolCollection consols)
		{
			foreach (CommonConsol consol in consols.ToArray())
			{
				DeleteConsolAndChildElementsFromDatabase(consol);
			}
		}

		void DeleteConsolAndChildElementsFromFactory(CommonConsol consol)
		{
			///Removing bizos only for test purposes instead of creating new ones
			foreach (CommonShipment shipment in consol.Shipments.ToArray())
			{
				shipment.JS_IsForwardRegistered = false;
				shipment.Delete();
			}

			foreach (CommonContainer container in consol.Containers.ToArray())
			{
				container.Delete();
			}

			foreach (Transport transport in consol.Transports)
			{
				if (transport.Voyage != null)
				{
					transport.Voyage.Delete();
				}
			}

			consol.Delete();
		}

		void DeleteConsolAndChildElementsFromDatabase(CommonConsol consol)
		{
			DeleteConsolAndChildElementsFromFactory(consol);
			consol.Factory.Save();
		}

		#endregion

		#region original and new values for specific value tests
		const string AgentTypeValueOriginal = Constants.AgentType.Agent;
		const string AgentTypeValueNew = Constants.AgentType.Direct;
		const string TransportModeOriginal = Constants.TransportModes.Sea;
		const string TransportModeNew = Constants.TransportModes.AirSea;
		const string ContainerModeOriginal = Constants.ContainerModes.AgentConsol;
		const string ContainerModeNew = Constants.ContainerModes.AIR;
		const string TransportTypeOriginal = Constants.TransportPlanningType.MainVessel;
		const string TransportTypeNew = Constants.TransportPlanningType.Other;

		ZDateTime CutoffOriginal
		{
			get
			{
				if (!fCutoffOriginal.IsValid)
				{
					fCutoffOriginal = new ZDateTime(2005, 9, 2);
				}
				return fCutoffOriginal;
			}
		}
		ZDateTime fCutoffOriginal;
		ZDateTime CutoffNew
		{
			get
			{
				if (!fCutoffNew.IsValid)
				{
					fCutoffNew = new ZDateTime();
				}
				return fCutoffNew;
			}
		}
		ZDateTime fCutoffNew;
		ZDateTime E_ARVNew
		{
			get
			{
				if (!fE_ARVNew.IsValid)
				{
					fE_ARVNew = new ZDateTime(2005, 9, 4);
				}
				return fE_ARVNew;
			}
		}
		ZDateTime fE_ARVNew;
		ZDateTime E_DEPNew
		{
			get
			{
				if (!fE_DEPNew.IsValid)
				{
					fE_DEPNew = new ZDateTime(2005, 9, 3);
				}
				return fE_DEPNew;
			}
		}
		ZDateTime fE_DEPNew;
		ZDateTime E_DEPOriginal
		{
			get
			{
				if (!fE_DEPOriginal.IsValid)
				{
					fE_DEPOriginal = new ZDateTime(2005, 9, 5);
				}
				return fE_DEPOriginal;
			}
		}
		ZDateTime fE_DEPOriginal;
		ZDateTime E_ARVOriginal
		{
			get
			{
				if (!fE_ARVOriginal.IsValid)
				{
					fE_ARVOriginal = new ZDateTime(2005, 9, 6);
				}
				return fE_ARVOriginal;
			}
		}
		ZDateTime fE_ARVOriginal;

		ZDateTime A_DEPOriginal
		{
			get
			{
				if (!fA_DEPOriginal.IsValid)
				{
					fA_DEPOriginal = new ZDateTime(2005, 9, 5);
				}
				return fA_DEPOriginal;
			}
		}
		ZDateTime fA_DEPOriginal;
		ZDateTime A_ARVOriginal
		{
			get
			{
				if (!fA_ARVOriginal.IsValid)
				{
					fA_ARVOriginal = new ZDateTime(2005, 9, 6);
				}
				return fA_ARVOriginal;
			}
		}
		ZDateTime fA_ARVOriginal;

		#endregion

		#region setup registry stuff

		readonly bool[] BooleanValues = new bool[] { true, false };

		void SetRegistryValues(bool consol, bool routing, bool sailing, bool shipments, bool containers)
		{
			SetAlwayscheckRegistryValue();
			SystemRegistry.UpdateConsolDuringAutomaticImportOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, consol);
			SystemRegistry.UpdateConsolDuringAutomaticImportAir.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, consol);
			SystemRegistry.UpdateConsolDuringAutomaticImportSea.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, consol);
			SystemRegistry.UpdateConsolsRoutingInformationDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, routing);
			SystemRegistry.UpdateSailingSchedulesDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sailing);
			SystemRegistry.UpdateConsolShipmentsDuringAutomaticImportOther.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, shipments);
			SystemRegistry.UpdateConsolContainersDuringAutomaticImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, containers);
		}

		void SetAlwayscheckRegistryValue()
		{
			//SystemRegistry.AlwaysCheckForConsolSailingShipmentsAndContainers.SetValue(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, AllwaysCheckForConsolsSailingShipmentsAndContainersValue);
			SystemRegistry.AlwaysCheckForConsolSailingShipmentsAndContainers.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AllwaysCheckForConsolsSailingShipmentsAndContainersValue);
		}
		protected abstract bool AllwaysCheckForConsolsSailingShipmentsAndContainersValue { get; }

		SystemDataRegistry SystemRegistry
		{
			get { return SystemDataRegistry.Instance; }
		}

		#endregion

		#region dummy notification subscriber with Answers

		class TestCreateOrUpdateFromValueObject_NotificationSubscriber : INotifications, INotificationSubscriberQueryUser
		{
			public TestCreateOrUpdateFromValueObject_NotificationSubscriber(bool consol, bool routing, bool sailing, bool container1, bool container2, bool shipment)
			{
				replies.Enqueue(new KeyValuePair<string, bool>("Consol", consol));
				replies.Enqueue(new KeyValuePair<string, bool>("routing", routing));

				if (routing)
				{
					replies.Enqueue(new KeyValuePair<string, bool>("Sailing", sailing));
				}

				replies.Enqueue(new KeyValuePair<string, bool>("Container", container1));
				replies.Enqueue(new KeyValuePair<string, bool>("Container", container2));
				replies.Enqueue(new KeyValuePair<string, bool>("Shipment", shipment));
			}

			#region INotifications Members

			void INotifications.Add(INotification notification)
			{
			}

			public void QueryUser(IQueryUserEventArgs e)
			{
				if (e is QueryUserYesNoYesAllNoAllEventArgs)
				{
					QueryUserYesNoYesAllNoAllEventArgs args = (QueryUserYesNoYesAllNoAllEventArgs)e;
					KeyValuePair<string, bool> pair = replies.Dequeue();

					args.Response = pair.Value;

					if (!args.Message.Contains(pair.Key))
					{
						throw new InvalidOperationException(string.Format("Expecting a message containing '{0}' but got '{1}'", pair.Key, args.Message));
					}
				}
			}

			#endregion

			readonly Queue<KeyValuePair<string, bool>> replies = new Queue<KeyValuePair<string, bool>>();
		}

		#endregion

		protected bool IsBatchJob
		{
			get { return Env.CurrentUser.IsBatchProcessor; }
		}
	}
}
