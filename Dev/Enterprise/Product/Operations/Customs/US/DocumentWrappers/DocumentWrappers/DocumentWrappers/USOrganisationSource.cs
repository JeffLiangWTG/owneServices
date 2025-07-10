using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class USOrganisationSource : DocBaseWrapper, IOrganisationDetails
	{
		USOrganisationSource(USOrganisation uSOrganisation, BusinessObjectFactory factory)
			: base(uSOrganisation, factory)
		{
		}

		public static USOrganisationSource New(USOrganisation uSOrganisation, BusinessObjectFactory factoryForWrapper)
		{
			USOrganisationSource result = null;
			if (uSOrganisation != null && !uSOrganisation.IsDeleted)
			{
				var usOrganisationDocAddress = uSOrganisation.USOrganisationDocAddress;
				if ((usOrganisationDocAddress != null && !usOrganisationDocAddress.IsDeleted && usOrganisationDocAddress.E2_AddressOverride) || uSOrganisation.Organisation != null)
				{
					result = new USOrganisationSource(uSOrganisation, factoryForWrapper);
				}
			}
			return result;
		}

		public DocDocAddress LocationAddress
		{
			get { return DocDocAddress.New(USOrganisation.USOrganisationDocAddress, Factory); }
		}

		#region IOrganisationDetails Members

		public ZString PartAttrib1Name
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PartAttrib1Name; }
		}

		public ZString PartAttrib2Name
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PartAttrib2Name; }
		}

		public ZString PartAttrib3Name
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PartAttrib3Name; }
		}

		public ZBool HasPartAttrib1
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.HasPartAttrib1; }
		}

		public ZBool HasPartAttrib2
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.HasPartAttrib2; }
		}

		public ZBool HasPartAttrib3
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.HasPartAttrib3; }
		}

		public ZBool HasSerialNumber
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.HasSerialNumber; }
		}

		public DocUSContacts DocUSContact
		{
			get { return DocUSContacts.New(USOrganisation, Factory); }
		}

		public DocContactsCollection Contacts
		{
			get
			{
				DocContactsCollection result = new DocContactsCollection(Factory);
				DocUSContacts docUSContact = this.DocUSContact;
				result.AddRange(OrgHeaderSource.Contacts);
				if (docUSContact != null)
				{
					result.Add(docUSContact);
				}
				return result;
			}
		}

		public DocContacts SalesContact
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.SalesContact; }
		}

		public DocContacts ExportFowardingContact
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.ExportFowardingContact; }
		}

		public DocContacts ImportFowardingContact
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.ImportFowardingContact; }
		}

		public DocContacts DefaultContact(ContactType defaultContactType)
		{
			return (OrgHeaderSource == null) ? null : OrgHeaderSource.DefaultContact(defaultContactType);
		}

		public DocContacts DefaultContact(Guid menuItem, ContactType defaultContactType)
		{
			return (OrgHeaderSource == null) ? null : OrgHeaderSource.DefaultContact(menuItem, defaultContactType);
		}

		public DocContacts ARContact
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.ARContact; }
		}

		public DocAddressCollection Addresses
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.Addresses; }
		}

		public DocDocAddressCollection DocAddresses
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.DocAddresses; }
		}

		public DocAddress MainAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.MainAddress; }
		}

		public DocAddress OrganisationPADPostalAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.OrganisationPADPostalAddress; }
		}

		public DocCusCodeCollection CustomCodes
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.CustomCodes; }
		}

		public DocAppointedAgentPortsCollection AppointedAgentPorts
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.AppointedAgentPorts; }
		}

		public ZString OrgType
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.OrgType; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZString[] AllNotes
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.AllNotes; }
		}

		public ZDateTime CurrentDate
		{
			get { return (OrgHeaderSource == null) ? ZDateTime.Empty : OrgHeaderSource.CurrentDate; }
		}

		public ZString AdditionalAddressInformation
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.AdditionalAddressInformation : ZString.Empty; }
		}

		public ZString Address1
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Address1 : LocationAddress.Address1; }
		}

		public ZString Address2
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Address2 : LocationAddress.Address2; }
		}

		public ZString City
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.City : LocationAddress.City; }
		}

		public ZString Code
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Code; }
		}

		public ZString Email
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Email; }
		}

		public ZString Fax
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Fax : LocationAddress.Fax; }
		}

		public ZString BusinessRegNo
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.BusinessRegNo; }
		}

		public ZString BusinessRegType
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.BusinessRegType; }
		}

		public ZString Name
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Name; }
		}

		public DocBranch Branch
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.Branch; }
		}

		public ZBool IsActive
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsActive; }
		}

		public ZBool IsAirCTO
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsAirCTO; }
		}

		public ZBool IsAirLine
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsAirLine; }
		}

		public ZBool IsAirWholesaler
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsAirWholesaler; }
		}

		public ZBool IsBroker
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsBroker; }
		}

		public ZBool IsCompetitor
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsCompetitor; }
		}

		public ZBool IsConsignee
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsConsignee; }
		}

		public ZBool IsConsignor
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsConsignor; }
		}

		public ZBool IsContainerPark
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsContainerPark; }
		}

		public ZBool IsCreditor
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsCreditor; }
		}

		public ZBool IsDebtor
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsDebtor; }
		}

		public ZBool IsForwarder
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsForwarder; }
		}

		public ZBool IsInlandWaterwayProvider
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsInlandWaterwayProvider; }
		}

		public ZBool IsLineHaulProvider
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsLineHaulProvider; }
		}

		public ZBool IsLocalTransport
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsLocalTransport; }
		}

		public ZBool IsMiscFreightServices
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsMiscFreightServices; }
		}

		public ZBool IsPackDepot
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsPackDepot; }
		}

		public ZBool IsRailProvider
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsRailProvider; }
		}

		public ZBool IsSalesLead
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsSalesLead; }
		}

		public ZBool IsSeaCTO
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsSeaCTO; }
		}

		public ZBool IsSeaWholesaler
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsSeaWholesaler; }
		}

		public ZBool IsShippingLine
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsShippingLine; }
		}

		public ZBool IsShippingProvider
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsShippingProvider; }
		}

		public ZBool IsTempAccount
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsTempAccount; }
		}

		public ZBool IsTransportClient
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsTransportClient; }
		}

		public ZBool IsUnpackDepot
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUnpackDepot; }
		}

		public ZBool IsWarehouseClient
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsWarehouseClient; }
		}

		public ZBool IsUserFlag1
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag1; }
		}

		public ZBool IsUserFlag2
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag2; }
		}

		public ZBool IsUserFlag3
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag3; }
		}

		public ZBool IsUserFlag4
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag4; }
		}

		public ZBool IsUserFlag5
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag5; }
		}

		public ZBool IsUserFlag6
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag6; }
		}

		public ZBool IsUserFlag7
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag7; }
		}

		public ZBool IsUserFlag8
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag8; }
		}

		public ZBool IsUserFlag9
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag9; }
		}

		public ZBool IsUserFlag10
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag10; }
		}

		public ZBool IsUserFlag11
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag11; }
		}

		public ZBool IsUserFlag12
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag12; }
		}

		public ZBool IsUserFlag13
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag13; }
		}

		public ZBool IsUserFlag14
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag14; }
		}

		public ZBool IsUserFlag15
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag15; }
		}

		public ZBool IsUserFlag16
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag16; }
		}

		public ZBool IsUserFlag17
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag17; }
		}

		public ZBool IsUserFlag18
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag18; }
		}

		public ZBool IsUserFlag19
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag19; }
		}

		public ZBool IsUserFlag20
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag20; }
		}

		public ZBool IsUserFlag21
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag21; }
		}

		public ZBool IsUserFlag22
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag22; }
		}

		public ZBool IsUserFlag23
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag23; }
		}

		public ZBool IsUserFlag24
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag24; }
		}

		public ZBool IsUserFlag25
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag25; }
		}

		public ZBool IsUserFlag26
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag26; }
		}

		public ZBool IsUserFlag27
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag27; }
		}

		public ZBool IsUserFlag28
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag28; }
		}

		public ZBool IsUserFlag29
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag29; }
		}

		public ZBool IsUserFlag30
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag30; }
		}

		public ZBool IsUserFlag31
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag31; }
		}

		public ZBool IsUserFlag32
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.IsUserFlag32; }
		}

		public ZString Mobile
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Mobile; }
		}

		public ZString Phone
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Phone : LocationAddress.Phone; }
		}

		public ZString PostCode
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PostCode : LocationAddress.PostCode; }
		}

		public DocUNLOCO Loco
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.Loco; }
		}

		public ZString State
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.State : LocationAddress.State; }
		}

		public ZString Web
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Web; }
		}

		public ZString HandlingInstructions
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.HandlingInstructions; }
		}

		public ZString GetHandlingInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetHandlingInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetHandlingInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		public ZString CartageInstructions
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CartageInstructions; }
		}

		public ZString GetCartageInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetCartageInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetCartageInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetCartageInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		public ZString LocalCustomsClientCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalCustomsClientCode; }
		}

		public ZString LocalCustomsCarrierCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalCustomsCarrierCode; }
		}

		public ZString LocalBusinessRegNo
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalBusinessRegNo; }
		}

		public ZString LocalCustomsSupplierCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalCustomsSupplierCode; }
		}

		public ZString LocalRebateUserCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalRebateUserCode; }
		}

		public ZString LocalVATCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LocalVATCode; }
		}

		public ZString CustomAttrib1
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CustomAttrib1; }
		}

		public ZString CustomAttrib2
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CustomAttrib2; }
		}

		public ZString CustomAttrib3
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CustomAttrib3; }
		}

		public ZDecimal CustomDecimal1
		{
			get { return (OrgHeaderSource == null) ? ZDecimal.Zero : OrgHeaderSource.CustomDecimal1; }
		}

		public ZDecimal CustomDecimal2
		{
			get { return (OrgHeaderSource == null) ? ZDecimal.Zero : OrgHeaderSource.CustomDecimal2; }
		}

		public ZDecimal CustomDecimal3
		{
			get { return (OrgHeaderSource == null) ? ZDecimal.Zero : OrgHeaderSource.CustomDecimal3; }
		}

		public ZDateTime CustomDate1
		{
			get { return (OrgHeaderSource == null) ? ZDateTime.Empty : OrgHeaderSource.CustomDate1; }
		}

		public ZDateTime CustomDate2
		{
			get { return (OrgHeaderSource == null) ? ZDateTime.Empty : OrgHeaderSource.CustomDate2; }
		}

		public ZDateTime CustomDate3
		{
			get { return (OrgHeaderSource == null) ? ZDateTime.Empty : OrgHeaderSource.CustomDate3; }
		}

		public ZBool CustomFlag1
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.CustomFlag1; }
		}

		public ZBool CustomFlag2
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.CustomFlag2; }
		}

		public ZBool CustomFlag3
		{
			get { return (OrgHeaderSource == null) ? ZBool.False : OrgHeaderSource.CustomFlag3; }
		}

		public ZString ApprovedExporterCode
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.ApprovedExporterCode; }
		}

		public ZString CarrierMasterBillPrefix
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CarrierMasterBillPrefix; }
		}

		public ZString CarrierAirlinePrefix
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CarrierAirlinePrefix; }
		}

		public ZString PostalAddress
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PostalAddress : LocationAddress.PostalAddress; }
		}

		public ZString PostalAddressInEnglish
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PostalAddressInEnglish : LocationAddress.PostalAddressInEnglish; }
		}

		public ZString PostalAddressExcludeCountryIfSame
		{
			get { return (LocationAddress == null) ? (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PostalAddressExcludeCountryIfSame : LocationAddress.PostalAddressExcludeCountryIfSame; }
		}

		public ZString PostalAddressExcludeName
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PostalAddressExcludeName; }
		}

		public DocCountry Country
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.Country; }
		}

		public DocCountryData CountryData
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.CountryData; }
		}

		public DocMiscServ MiscServ
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.MiscServ; }
		}

		public ZString SCAC
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.SCAC; }
		}

		public ZString ABN
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.ABN; }
		}

		public ZString GST
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GST; }
		}

		public ZString CBR
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CBR; }
		}

		public ZString CCC
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CCC; }
		}

		public ZString LSC
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.LSC; }
		}

		public ZString PAN
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.PAN; }
		}

		public ZString CSC
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CSC; }
		}

		public ZString CCD
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CCD; }
		}

		public ZString CID
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.CID; }
		}

		public ZString Kennitala
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.Kennitala; }
		}

		public ZString DisbursmentTerms
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.DisbursmentTerms; }
		}

		public ZString ShortDisbursementTerms
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.ShortDisbursementTerms; }
		}

		public ZString ShortInvoiceTerms
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.ShortInvoiceTerms; }
		}

		public DocAddress DeliverAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.DeliverAddress; }
		}

		public DocDocAddress DeliverDocAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.DeliverDocAddress; }
		}

		public DocAddress PickUpAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.PickUpAddress; }
		}

		public DocDocAddress PickUpDocAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.PickUpDocAddress; }
		}

		public DocAddress ARAddress
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.ARAddress; }
		}

		public DocAddress PostalAddressMain
		{
			get { return (OrgHeaderSource == null) ? null : OrgHeaderSource.PostalAddressMain; }
		}

		public ZString SplitFullName1
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.SplitFullName1; }
		}

		public ZString SplitFullName2
		{
			get { return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.SplitFullName2; }
		}

		public ZString GetSpecialInstructionsByTransportOrContainerMode(ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetSpecialInstructionsByTransportOrContainerMode(transportMode, containerMode);
		}

		public ZString GetSpecialInstructionsByDirectionAndTransportOrContainerMode(ZString direction, ZString transportMode, ZString containerMode)
		{
			return (OrgHeaderSource == null) ? ZString.Empty : OrgHeaderSource.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(direction, transportMode, containerMode);
		}

		#endregion

		USOrganisation USOrganisation
		{
			get { return (USOrganisation)WrappedObject; }
		}

		OrgHeaderSource OrgHeaderSource
		{
			get { return OrgHeaderSource.New(USOrganisation.Organisation, Factory); }
		}

		public DocOrgStaffAssignmentsCollection StaffAssignments
		{
			get
			{
				DocOrgStaffAssignmentsCollection result = new DocOrgStaffAssignmentsCollection(Factory);
				if (OrgHeaderSource != null)
				{
					result.AddRange(OrgHeaderSource.StaffAssignments);
				}
				return result;
			}
		}
	}
}
