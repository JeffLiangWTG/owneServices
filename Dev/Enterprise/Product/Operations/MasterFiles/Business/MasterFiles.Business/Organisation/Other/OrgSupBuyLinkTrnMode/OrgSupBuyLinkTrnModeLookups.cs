using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupBuyLinkTrnModeLookups : AutoOrgSupBuyLinkTrnModeLookups
	{
		public OrgSupBuyLinkTrnModeLookups(AutoOrgSupBuyLinkTrnMode parent)
			: base(parent)
		{
		}

		#region ContainerModeList

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var transportMode = OrgSupBuyLinkTrnMode.PF_TransportMode;
				return Factory.GetCachedValue("OrgSupBuyLinkTrnModeLookups.ContainerModeList." + transportMode, () =>
				{
					var result = ObjectFactory.Get<IFreightCodePairListProvider>().GetContainerModeList(transportMode);
					var customsList = ObjectFactory.Get<Enterprise.Integration.Customs.ICustomsCodePairListProvider>().GetContainerModeList(transportMode);
					foreach (ICodeDescription pair in customsList)
					{
						result.AddPairIfNotExist(pair.Code, pair.Description);
					}
					return result;
				});
			}
		}

		#endregion

		#region TransportModeList

		public CodeDescriptionPairList TransportModeList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode); }
		}

		#endregion

		#region IncoTermList

		public CodeDescriptionPairList IncoTermList
		{
			get
			{
				CodeDescriptionPairList result = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms);
				result.AddRange(new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms));
				return result;
			}
		}

		#endregion

		#region IncoTermModeList

		public CodeDescriptionPairList IncoTermModeList
		{
			get
			{
				return new OrgSupBuyLinkTrnModeCodeDescriptionPairList();
			}
		}

		#endregion

		#region CarrierLines

		public override OrgHeaderCollection CarrierLines
		{
			get
			{
				switch (OrgSupBuyLinkTrnMode.PF_TransportMode)
				{
					case Constants.TransportModes.Air:
						return new AirShippingProviderCollection(Factory);

					case Constants.TransportModes.Rail:
						return new RailShippingProviderCollection(Factory);

					case Constants.TransportModes.InlandWaterwayTransport:
						return new InlandWaterwayShippingProviderCollection(Factory);

					case Constants.TransportModes.Sea:
						return new SeaShippingProviderCollection(Factory);

					default:
						return new ShippingProviderCollection(Factory);
				}
			}
		}

		#endregion

		#region ImportCustomsAgents

		public override OrgHeaderCollection ImportCustomsAgents
		{
			get
			{
				if (importCustomsAgents == null)
				{
					importCustomsAgents = new BrokerCollection(Factory);
				}
				return importCustomsAgents;
			}
		}
		BrokerCollection importCustomsAgents;

		#endregion

		#region ControllingCustomers

		public override OrgHeaderCollection ControllingCustomers
		{
			get
			{
				if (controllingCustomers == null)
				{
					controllingCustomers = new OrgHeaderCollection(Factory);
				}
				return controllingCustomers;
			}
		}
		OrgHeaderCollection controllingCustomers;

		#endregion

		#region Forwarders

		public ForwarderCollection Forwarders
		{
			get
			{
				if (forwarders == null)
				{
					forwarders = new ForwarderCollection(Factory);
				}
				return forwarders;
			}
		}
		ForwarderCollection forwarders;

		#endregion

		#region Organisations

		public OrgHeaderCollection PickupOrganisations => pickupOrganisations ?? (pickupOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection pickupOrganisations;

		public OrgHeaderCollection DeliveryOrganisations => deliveryOrganisations ?? (deliveryOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection deliveryOrganisations;

		public OrgHeaderCollection NotifyPartyOrganisations => notifyPartyOrganisations ?? (notifyPartyOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection notifyPartyOrganisations;

		#endregion

		#region Addresses

		public new OrgAddressDependentCollection CustomsControlledArrivalLocations
		{
			get
			{
				return BuyerAddresses;
			}
		}

		public new OrgAddressDependentCollection CustomsExamSites
		{
			get
			{
				return BuyerAddresses;
			}
		}

		OrgAddressDependentCollection BuyerAddresses
		{
			get
			{
				if (buyerAddresses == null)
				{
					var link = OrgSupBuyLinkTrnMode.SupplierBuyerLink;
					if (link != null && link.Buyer != null)
					{
						link.Buyer.Addresses.Load();
						buyerAddresses = link.Buyer.Addresses;
					}
					else
					{
						buyerAddresses = new OrgAddressDependentCollection(Factory);
					}
				}
				return buyerAddresses;
			}
		}

		OrgAddressDependentCollection buyerAddresses;

		#endregion

		#region Contacts

		public new OrgContactDependentCollection OverrideConsigneeContacts
		{
			get
			{
				if (OrgSupBuyLinkTrnMode?.OverrideDeliveryAddress?.Header != null)
				{
					OrgSupBuyLinkTrnMode.OverrideDeliveryAddress.Header.Contacts.Load();
					return OrgSupBuyLinkTrnMode.OverrideDeliveryAddress.Header.Contacts;
				}
				else if (OrgSupBuyLinkTrnMode?.SupplierBuyerLink?.Buyer != null)
				{
					OrgSupBuyLinkTrnMode.SupplierBuyerLink.Buyer.Contacts.Load();
					return OrgSupBuyLinkTrnMode.SupplierBuyerLink.Buyer.Contacts;
				}
				else
				{
					return new OrgContactDependentCollection(Factory);
				}
			}
		}

		public new OrgContactDependentCollection OverrideSupplierContacts
		{
			get
			{
				if (OrgSupBuyLinkTrnMode?.OverridePickupAddress?.Header != null)
				{
					OrgSupBuyLinkTrnMode.OverridePickupAddress.Header.Contacts.Load();
					return OrgSupBuyLinkTrnMode.OverridePickupAddress.Header.Contacts;
				}
				else if (OrgSupBuyLinkTrnMode?.SupplierBuyerLink?.Supplier != null)
				{
					OrgSupBuyLinkTrnMode.SupplierBuyerLink.Supplier.Contacts.Load();
					return OrgSupBuyLinkTrnMode.SupplierBuyerLink.Supplier.Contacts;
				}
				else
				{
					return new OrgContactDependentCollection(Factory);
				}
			}
		}

		public new OrgContactDependentCollection OverrideNotifyParties
		{
			get
			{
				if (OrgSupBuyLinkTrnMode?.OverrideNotifyPartyAddress?.Header != null)
				{
					return OrgSupBuyLinkTrnMode.OverrideNotifyPartyAddress.Header.GetActiveContacts();
				}
				else if (OrgSupBuyLinkTrnMode?.SupplierBuyerLink?.Buyer != null)
				{
					return OrgSupBuyLinkTrnMode.SupplierBuyerLink.Buyer.GetActiveContacts();
				}
				else
				{
					return new OrgContactDependentCollection(Factory);
				}
			}
		}

		#endregion

		#region US Reference Files

		public BusinessObjectCollection USPortsOfLading
		{
			get
			{
				Type collectionType = OrgSupBuyLinkTrnMode.IsUSImporterCountry ? ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>() : ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>();
				return (BusinessObjectCollection)Activator.CreateInstance(collectionType, new object[] { Factory });
			}
		}

		public BusinessObjectCollection USPortsOfUnLading
		{
			get
			{
				Type collectionType = OrgSupBuyLinkTrnMode.IsUSImporterCountry ? ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSRegionDistrictPortCollection>() : ObjectFactory.GetType<Enterprise.Integration.Customs.US.IUSCForeignPortCollection>();
				return (BusinessObjectCollection)Activator.CreateInstance(collectionType, new object[] { Factory });
			}
		}

		#endregion

		#region Implementation

		OrgSupBuyLinkTrnMode OrgSupBuyLinkTrnMode
		{
			get { return (OrgSupBuyLinkTrnMode)Parent; }
		}

		#endregion
	}
}
