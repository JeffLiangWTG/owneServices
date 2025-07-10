using System;
using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class BuyerSupplierLinksHelperTest : TestCaseWithFactory
	{
		// Tested in Freight, add new tests here

		public void TestRestoreContainerMode_ContainerIsContainerised()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var shipment = new ShipmentForTest(Factory);

			var buyerJobDocAddress = Factory.New<JobDocAddress>();
			buyerJobDocAddress.OrganisationPK = buyer.PK;
			var supplierJobDocAddress = Factory.New<JobDocAddress>();
			supplierJobDocAddress.OrganisationPK = buyer.PK;

			shipment.JS_PackingMode = string.Empty;

			shipment.ConsigneeDocumentaryAddress = buyerJobDocAddress;
			shipment.ConsignorDocumentaryAddress = supplierJobDocAddress;

			var link = SetupShipmentWithLink(buyer, supplier, shipment, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.FCL);

			shipment.JS_PackingMode = string.Empty;
			shipment.ConsigneeDocumentaryAddress = buyerJobDocAddress;
			shipment.ConsignorDocumentaryAddress = supplierJobDocAddress;

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = buyer.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			link.RestoreTransportMode();

			AssertEquals("ShouldDefaultContainerModeAndIsContainerised return true then TransportMode should be set to Sea on Shipment.", Core.Constants.TransportModes.Sea, shipment.JS_TransportMode);
			AssertEquals("ShouldDefaultContainerModeAndIsContainerised return true so the ContainerMode should be set on Shipment.", Core.Constants.ContainerModes.FCL, shipment.JS_PackingMode);
		}

		public void TestRestoreContainerMode_ContainerIsNotContainerised()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = new ShipmentForTest(Factory);

			var buyerJobDocAddress = Factory.New<JobDocAddress>();
			buyerJobDocAddress.OrganisationPK = buyer.PK;
			var supplierJobDocAddress = Factory.New<JobDocAddress>();
			supplierJobDocAddress.OrganisationPK = buyer.PK;

			shipment.JS_PackingMode = string.Empty;

			shipment.ConsigneeDocumentaryAddress = buyerJobDocAddress;
			shipment.ConsignorDocumentaryAddress = supplierJobDocAddress;

			var link = SetupShipmentWithLink(buyer, supplier, shipment, Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Bulk);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.Containerised;
			shipment.ConsigneeDocumentaryAddress = buyerJobDocAddress;
			shipment.ConsignorDocumentaryAddress = supplierJobDocAddress;

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = buyer.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;

			shipment.ConsigneeDocumentaryAddress.OrganisationPK = buyer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;

			link.RestoreTransportMode();

			AssertEquals("ShouldDefaultContainerModeAndIsContainerised return false then TransportMode should be set to the value of the link transport mode (here AIR) on Shipment.", "AIR", shipment.JS_TransportMode);
			AssertEquals("ShouldDefaultContainerModeAndIsContainerised return false so the ContainerMode should keep his value (here CNT).", Core.Constants.ContainerModes.Containerised, shipment.JS_PackingMode);
		}

		public void TestAddNewBuyerSupplierLink_ContainerMode()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var buyerJobDocAddress = Factory.New<JobDocAddress>();
			buyerJobDocAddress.OrganisationPK = buyer.PK;
			var supplierJobDocAddress = Factory.New<JobDocAddress>();
			supplierJobDocAddress.OrganisationPK = supplier.PK;

			var shipment = new ShipmentForTest(Factory) { ConsigneeDocumentaryAddress = buyerJobDocAddress, ConsignorDocumentaryAddress = supplierJobDocAddress };

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(shipment);

			shipment.JS_PackingMode = ZString.Empty;
			var buyerSupplierLink1 = linksHelper.AddNewBuyerSupplierLink();
			AssertEquals(ZString.Empty, buyerSupplierLink1.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);

			shipment.JS_PackingMode = "SOM";
			var buyerSupplierLink2 = linksHelper.AddNewBuyerSupplierLink();
			AssertEquals("SOM", buyerSupplierLink2.OrgSupBuyLinkTrnModes[0].PF_ContainerMode);
		}

		BuyerSupplierLinksHelper<ShipmentForTest> SetupShipmentWithLink(OrgHeader buyer, OrgHeader supplier, ShipmentForTest bO, string linkTransportMode, string linkContainerMode)
		{
			// set up the buyer/suppler and their link
			buyer.OH_RL_NKClosestPort = "AUSYD";
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var link = supplier.BuyerLinks.AddNew();

			link.OL_OH_Buyer = buyer.PK;

			// set the defaults on the link and supplier/misc serv
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = linkTransportMode;
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = linkContainerMode;
			link.OL_RN_NKImporterCountry = "AU";

			// make sure the defaults come from the link appropriately
			((ISupportDataImporting)bO).IsImportingData = true;

			var linksHelper = new BuyerSupplierLinksHelper<ShipmentForTest>(bO);

			linksHelper.Register();
			return linksHelper;
		}

		internal class ShipmentForTest : NonPersistentBusinessObject, IBuyerSupplierRelationshipConsumer, ISupportDataImporting, IObsoleteValidation
		{
			public ShipmentForTest(BusinessObjectFactory factory)
			: base(factory)
			{
			}

			#region IBuyerSupplierRelationshipConsumer Members

			public OrgHeader Consignee
			{
				get
				{
					if (ConsigneeDocumentaryAddress == null || !ConsigneeDocumentaryAddress.HasRealOrganisation)
					{
						return null;
					}
					return ConsigneeDocumentaryAddress.Organisation;
				}
			}

			public OrgHeader Consignor
			{
				get
				{
					if (ConsignorDocumentaryAddress == null || !ConsignorDocumentaryAddress.HasRealOrganisation)
					{
						return null;
					}
					return ConsignorDocumentaryAddress.Organisation;
				}
			}

			public ZPropertyInfo JS_TransportModeInfo
			{
				[DebuggerStepThrough]
				get
				{
					return GetZPropertyInfo(nameof(JS_TransportMode));
				}
			}

			public ZString JS_TransportMode
			{
				get
				{
					return transportMode;
				}
				set
				{
					transportMode = value;
				}
			}
			ZString transportMode;

			public ZString JS_PackingMode
			{
				get
				{
					return packingMode;
				}
				set
				{
					packingMode = value;
				}
			}
			ZString packingMode;

			public ZPropertyInfo JS_PackingModeInfo
			{
				[DebuggerStepThrough]
				get
				{
					return GetZPropertyInfo(nameof(JS_PackingMode));
				}
			}

			public bool IsCoLoadMaster;

			public bool IsBlindCoLoadMaster;

			event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
			{
				add
				{
					JS_TransportModeInfo.ValueChanged += value;
					JS_PackingModeInfo.ValueChanged += value;
				}
				remove
				{
					JS_TransportModeInfo.ValueChanged -= value;
					JS_PackingModeInfo.ValueChanged -= value;
				}
			}

			JobDocAddress fConsigneeDocumentaryAddress;
			JobDocAddress fConsignorDocumentaryAddress;

			public JobDocAddress ConsigneeDocumentaryAddress
			{
				get
				{
					return fConsigneeDocumentaryAddress;
				}
				set
				{
					fConsigneeDocumentaryAddress = value;
				}
			}

			public JobDocAddress ConsignorDocumentaryAddress
			{
				get
				{
					return fConsignorDocumentaryAddress;
				}

				set
				{
					fConsignorDocumentaryAddress = value;
				}
			}

			void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
			{
			}

			void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
			{
			}

			event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
			{
				add { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
				remove { ConsigneeDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
			}

			event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
			{
				add { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged += value; }
				remove { ConsignorDocumentaryAddress.E2_OA_AddressInfo.ValueChanged -= value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.Origin
			{
				get { return string.Empty; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.Destination
			{
				get { return string.Empty; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.LoadPort
			{
				get { return string.Empty; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.DischargePort
			{
				get { return string.Empty; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.ContainerMode
			{
				get { return JS_PackingMode; }
				set { JS_PackingMode = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.TransportMode
			{
				get { return JS_TransportMode; }
				set { JS_TransportMode = value; }
			}

			ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
			{
				get { return string.Empty; }
				set { }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
			{
				ServiceLevelFallBackSet = true;
			}

			public bool ServiceLevelFallBackSet;

			ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
			{
				get { return ZGuid.Empty; }
				set { }
			}

			ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK { get; set; }

			ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK { get; set; }

			ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
			{
				get { return ZGuid.Empty; }
				set { }
			}

			ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
			{
				get { return ZGuid.Empty; }
				set { }
			}

			ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
			{
				get { return ZGuid.Empty; }
				set { }
			}

			ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
			{
				get { return string.Empty; }
				set { }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
			{
			}

			ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
			{
				get { return string.Empty; }
				set { }
			}

			ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
			{
				get { return !IsCoLoadMaster && !IsBlindCoLoadMaster; }
			}

			ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
			{
				get { return ZByte.Zero; }
				set { }
			}

			ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
			{
				get { return ZByte.Zero; }
				set { }
			}

			void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
			{
			}

			void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
			{
			}

			ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
			{
				get
				{
					return false;
				}
			}

			public void RestoreImportBrokerFallback()
			{
			}

			public ZBool ShouldDefaultContainerModeAndIsContainerised(ZString containerMode)
			{
				return containerMode == "FCL";
			}

			ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
			{
				get { return string.Empty; }
				set { }
			}

			ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
			{
				get;
			}

			public ZBool ShouldRestoreHandlingInformation
			{
				get;
				set;
			}

			ZString IBuyerSupplierRelationshipConsumer.ReleaseType
			{
				get { return string.Empty; }
			}

			OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
			{
				get { return Consignee; }
				set
				{
					if (!ConsigneeDocumentaryAddress.E2_AddressOverride)
					{
						ConsigneeDocumentaryAddress.OrganisationPK = value.PK;
					}
				}
			}

			OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
			{
				get { return Consignor; }
				set
				{
					if (!ConsignorDocumentaryAddress.E2_AddressOverride)
					{
						ConsignorDocumentaryAddress.OrganisationPK = value.PK;
					}
				}
			}

			#endregion

			#region IsInDatabase

			public bool IsInDatabaseSetter;
			public override bool IsInDatabase
			{
				get
				{
					return IsInDatabaseSetter;
				}
			}

			#endregion

			ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
			{
				get { return true; }
			}

			public JobDocAddress ConsignorPickupAddress => null;

			public JobDocAddress ConsigneeDeliveryAddress => null;

			public JobDocAddress NotifyPartyDocumentaryAddress => null;

			public ZBool IsSettingDefaultValues { get; set; }
			public bool IsImportingData { get; set; }
		}
	}
}
