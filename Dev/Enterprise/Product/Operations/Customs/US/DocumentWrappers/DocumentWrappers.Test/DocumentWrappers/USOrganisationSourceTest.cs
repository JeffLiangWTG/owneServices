using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Testing
{
	[TestedType(typeof(USOrganisationSource))]
	sealed class USOrganisationSourceTest : USOrganisationWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { USOrganisationSource.New(USOrganisation, Factory) };
		}

		public void TestNew()
		{
			AssertNull("New", USOrganisationSource.New(null, Factory));
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("New", USOrganisationSource.New(USOrganisation, Factory));
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("New", USOrganisationSource.New(USOrganisation, Factory));
		}

		public void TestNewWhenUSOrganisationIsDeleted()
		{
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			USOrganisation.Delete();
			AssertNull("Null", USOrganisationSource.New(USOrganisation, Factory));
		}

		public void TestNewWhenUSOrganisationDocAddressIsDeleted()
		{
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			USOrganisation.USOrganisationDocAddress.Delete();
			AssertNull("Null", USOrganisationSource.New(USOrganisation, Factory));
		}

		public void TestLocationAddress()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("LocationAddress's type", typeof(Enterprise.DocumentWrappers.DocDocAddress), orgWrapper.LocationAddress.GetType());
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
		}

		#region IOrganisationDetails Members

		public void TestPartAttrib1Name()
		{
			AssertEquals("PartAttrib1Name", headerWrapper.PartAttrib1Name, orgWrapper.PartAttrib1Name);
		}

		public void TestPartAttrib2Name()
		{
			AssertEquals("PartAttrib2Name", headerWrapper.PartAttrib2Name, orgWrapper.PartAttrib2Name);
		}

		public void TestPartAttrib3Name()
		{
			AssertEquals("PartAttrib3Name", headerWrapper.PartAttrib3Name, orgWrapper.PartAttrib3Name);
		}

		public void TestHasPartAttrib1()
		{
			AssertEquals("HasPartAttrib1", headerWrapper.HasPartAttrib1, orgWrapper.HasPartAttrib1);
		}

		public void TestHasPartAttrib2()
		{
			AssertEquals("HasPartAttrib2", headerWrapper.HasPartAttrib2, orgWrapper.HasPartAttrib2);
		}

		public void TestHasPartAttrib3()
		{
			AssertEquals("HasPartAttrib3", headerWrapper.HasPartAttrib3, orgWrapper.HasPartAttrib3);
		}

		public void TestHasSerialNumber()
		{
			AssertEquals("HasSerialNumber", headerWrapper.HasSerialNumber, orgWrapper.HasSerialNumber);
		}

		public void TestDocUSContact()
		{
			USOrganisation.ZO_OH_Organisation = Supplier.PK;
			AssertNotNull("DocUSContact", orgWrapper.DocUSContact);
			AssertEquals("DocUSContact's type", typeof(DocUSContacts), orgWrapper.DocUSContact.GetType());
			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull("DocUSContact", orgWrapper.DocUSContact);
		}

		public void TestContacts()
		{
			AssertEquals("Contacts.Count", headerWrapper.Contacts.Count + 1, orgWrapper.Contacts.Count);
			bool found = false;
			foreach (object item in headerWrapper.Contacts)
			{
				found = false;
				foreach (object checkItem in orgWrapper.Contacts)
				{
					if (item.ToString() == checkItem.ToString())
					{
						found = true;
					}
				}
				Assert(string.Format("Item {0} not found", item.ToString()), found);
			}

			found = false;
			foreach (object checkItem in orgWrapper.Contacts)
			{
				if (orgWrapper.DocUSContact.ToString() == checkItem.ToString())
				{
					found = true;
				}
			}
			Assert(string.Format("Item {0} not found", orgWrapper.DocUSContact.ToString()), found);
		}

		public void TestSalesContact()
		{
			AssertEquals("SalesContact", headerWrapper.SalesContact.ToString(), orgWrapper.SalesContact.ToString());
		}

		public void TestExportFowardingContact()
		{
			AssertEquals("ExportFowardingContact", headerWrapper.ExportFowardingContact.ToString(), orgWrapper.ExportFowardingContact.ToString());
		}

		public void TestImportFowardingContact()
		{
			AssertEquals("ImportFowardingContact", headerWrapper.ImportFowardingContact.ToString(), orgWrapper.ImportFowardingContact.ToString());
		}

		public void TestDefaultContact()
		{
			AssertEquals("DefaultContact(ContactType.Consignee)", headerWrapper.DefaultContact(ContactType.Consignee).ToString(), orgWrapper.DefaultContact(ContactType.Consignee).ToString());
		}

		public void TestDefaultContactForMenuItem()
		{
			OrgHeader header = (OrgHeader)headerWrapper.WrappedObject;
			OrgDocument doc = header.Contacts[0].Documents.AddNew();
			StmMenuItem menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery());
			doc.OD_SU_MenuItem = menuItem.PK;

			AssertEquals(headerWrapper.DefaultContact(menuItem.PK.ToGuid(), ContactType.Consignee).ToString(), orgWrapper.DefaultContact(menuItem.PK.ToGuid(), ContactType.Consignee).ToString());
		}

		public void TestAddresses()
		{
			AssertEquals("Addresses.Count", headerWrapper.Addresses.Count, orgWrapper.Addresses.Count);
			foreach (object item in headerWrapper.Addresses)
			{
				bool found = false;
				foreach (object checkItem in orgWrapper.Addresses)
				{
					if (item.ToString() == checkItem.ToString())
					{
						found = true;
					}
				}
				Assert(string.Format("Item {0} not found", item.ToString()), found);
			}
		}

		public void TestDocAddresses()
		{
			AssertEquals("DocAddresses.Count", headerWrapper.DocAddresses.Count, orgWrapper.DocAddresses.Count);
			foreach (object item in headerWrapper.DocAddresses)
			{
				bool found = false;
				foreach (object checkItem in orgWrapper.DocAddresses)
				{
					if (item.ToString() == checkItem.ToString())
					{
						found = true;
					}
				}
				Assert(string.Format("Item {0} not found", item.ToString()), found);
			}
		}

		public void TestMainAddress()
		{
			AssertEquals("MainAddress", headerWrapper.MainAddress.ToString(), orgWrapper.MainAddress.ToString());
		}

		public void TestOrganisationPADPostalAddress()
		{
			AssertEquals("OrganisationPADPostalAddress", headerWrapper.OrganisationPADPostalAddress, orgWrapper.OrganisationPADPostalAddress);
		}

		public void TestCustomCodes()
		{
			AssertEquals("CustomCodes.Count", headerWrapper.CustomCodes.Count, orgWrapper.CustomCodes.Count);
			foreach (Enterprise.DocumentWrappers.DocCusCode item in headerWrapper.CustomCodes)
			{
				AssertEquals("CustomCodes", true, orgWrapper.CustomCodes.ContainsWrappedObject(item));
			}
		}

		public void TestAppointedAgentPorts()
		{
			AssertEquals("AppointedAgentPorts.Count", headerWrapper.AppointedAgentPorts.Count, orgWrapper.AppointedAgentPorts.Count);
			foreach (Enterprise.DocumentWrappers.DocAppointedAgentPorts item in headerWrapper.AppointedAgentPorts)
			{
				AssertEquals("AppointedAgentPorts", true, orgWrapper.AppointedAgentPorts.Contains(item));
			}
		}

		public void TestOrgType()
		{
			AssertEquals("OrgType", headerWrapper.OrgType, orgWrapper.OrgType);
		}

		public void TestAllNotes()
		{
			AssertEquals("AllNotes.Length", headerWrapper.AllNotes.Length, orgWrapper.AllNotes.Length);
			foreach (ZString note in headerWrapper.AllNotes)
			{
				AssertCollectionContains("AllNotes", note, orgWrapper.AllNotes);
			}
		}

		public void TestCurrentDate()
		{
			AssertEquals("CurrentDate", headerWrapper.CurrentDate, orgWrapper.CurrentDate);
		}

		public void TestAddress1()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Address1", orgWrapper.LocationAddress.Address1, orgWrapper.Address1);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Address1", ZString.Empty, orgWrapper.Address1);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_Address1 = "Road";
			AssertEquals("Address1", "Road", orgWrapper.Address1);
		}

		public void TestAddress2()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Address2", orgWrapper.LocationAddress.Address2, orgWrapper.Address2);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Address2", ZString.Empty, orgWrapper.Address2);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_Address2 = "District";
			AssertEquals("Address2", "District", orgWrapper.Address2);
		}

		public void TestCity()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("City", orgWrapper.LocationAddress.City, orgWrapper.City);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("City", ZString.Empty, orgWrapper.City);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_City = "ShangHai";
			AssertEquals("City", "ShangHai", orgWrapper.City);
		}

		public void TestCode()
		{
			AssertEquals("Code", headerWrapper.Code, orgWrapper.Code);
		}

		public void TestEmail()
		{
			AssertEquals("Email", headerWrapper.Email, orgWrapper.Email);
		}

		public void TestFax()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Fax", orgWrapper.LocationAddress.Fax, orgWrapper.Fax);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Fax", ZString.Empty, orgWrapper.Fax);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_Fax = "123456";
			AssertEquals("Fax", "123456", orgWrapper.Fax);
		}

		public void TestBusinessRegNo()
		{
			AssertEquals("BusinessRegNo", headerWrapper.BusinessRegNo, orgWrapper.BusinessRegNo);
		}

		public void TestBusinessRegType()
		{
			AssertEquals("BusinessRegType", headerWrapper.BusinessRegType, orgWrapper.BusinessRegType);
		}

		public void TestName()
		{
			AssertEquals("Name", headerWrapper.Name, orgWrapper.Name);
		}

		public void TestBranch()
		{
			AssertEquals("Branch", headerWrapper.Branch, orgWrapper.Branch);
		}

		public void TestIsActive()
		{
			AssertEquals("IsActive", headerWrapper.IsActive, orgWrapper.IsActive);
		}

		public void TestIsAirCTO()
		{
			AssertEquals("IsAirCTO", headerWrapper.IsAirCTO, orgWrapper.IsAirCTO);
		}

		public void TestIsAirLine()
		{
			AssertEquals("IsAirLine", headerWrapper.IsAirLine, orgWrapper.IsAirLine);
		}

		public void TestIsAirWholesaler()
		{
			AssertEquals("IsAirWholesaler", headerWrapper.IsAirWholesaler, orgWrapper.IsAirWholesaler);
		}

		public void TestIsBroker()
		{
			AssertEquals("IsBroker", headerWrapper.IsBroker, orgWrapper.IsBroker);
		}

		public void TestIsCompetitor()
		{
			AssertEquals("IsCompetitor", headerWrapper.IsCompetitor, orgWrapper.IsCompetitor);
		}

		public void TestIsConsignee()
		{
			AssertEquals("IsConsignee", headerWrapper.IsConsignee, orgWrapper.IsConsignee);
		}

		public void TestIsConsignor()
		{
			AssertEquals("IsConsignor", headerWrapper.IsConsignor, orgWrapper.IsConsignor);
		}

		public void TestIsContainerPark()
		{
			AssertEquals("IsContainerPark", headerWrapper.IsContainerPark, orgWrapper.IsContainerPark);
		}

		public void TestIsCreditor()
		{
			AssertEquals("IsCreditor", headerWrapper.IsCreditor, orgWrapper.IsCreditor);
		}

		public void TestIsDebtor()
		{
			AssertEquals("IsDebtor", headerWrapper.IsDebtor, orgWrapper.IsDebtor);
		}

		public void TestIsForwarder()
		{
			AssertEquals("IsForwarder", headerWrapper.IsForwarder, orgWrapper.IsForwarder);
		}

		public void TestIsLineHaulProvider()
		{
			AssertEquals("IsLineHaulProvider", headerWrapper.IsLineHaulProvider, orgWrapper.IsLineHaulProvider);
		}

		public void TestIsLocalTransport()
		{
			AssertEquals("IsLocalTransport", headerWrapper.IsLocalTransport, orgWrapper.IsLocalTransport);
		}

		public void TestIsMiscFreightServices()
		{
			AssertEquals("IsMiscFreightServices", headerWrapper.IsMiscFreightServices, orgWrapper.IsMiscFreightServices);
		}

		public void TestIsPackDepot()
		{
			AssertEquals("IsPackDepot", headerWrapper.IsPackDepot, orgWrapper.IsPackDepot);
		}

		public void TestIsRailProvider()
		{
			AssertEquals("IsRailProvider", headerWrapper.IsRailProvider, orgWrapper.IsRailProvider);
		}

		public void TestIsSalesLead()
		{
			AssertEquals("IsSalesLead", headerWrapper.IsSalesLead, orgWrapper.IsSalesLead);
		}

		public void TestIsSeaCTO()
		{
			AssertEquals("IsSeaCTO", headerWrapper.IsSeaCTO, orgWrapper.IsSeaCTO);
		}

		public void TestIsSeaWholesaler()
		{
			AssertEquals("IsSeaWholesaler", headerWrapper.IsSeaWholesaler, orgWrapper.IsSeaWholesaler);
		}

		public void TestIsShippingLine()
		{
			AssertEquals("IsShippingLine", headerWrapper.IsShippingLine, orgWrapper.IsShippingLine);
		}

		public void TestIsShippingProvider()
		{
			AssertEquals("IsShippingProvider", headerWrapper.IsShippingProvider, orgWrapper.IsShippingProvider);
		}

		public void TestIsTempAccount()
		{
			AssertEquals("IsTempAccount", headerWrapper.IsTempAccount, orgWrapper.IsTempAccount);
		}

		public void TestIsTransportClient()
		{
			AssertEquals("IsTransportClient", headerWrapper.IsTransportClient, orgWrapper.IsTransportClient);
		}

		public void TestIsUnpackDepot()
		{
			AssertEquals("IsUnpackDepot", headerWrapper.IsUnpackDepot, orgWrapper.IsUnpackDepot);
		}

		public void TestIsUserFlag1()
		{
			AssertEquals("IsUserFlag1", headerWrapper.IsUserFlag1, orgWrapper.IsUserFlag1);
		}

		public void TestIsUserFlag2()
		{
			AssertEquals("IsUserFlag2", headerWrapper.IsUserFlag2, orgWrapper.IsUserFlag2);
		}

		public void TestIsUserFlag3()
		{
			AssertEquals("IsUserFlag3", headerWrapper.IsUserFlag3, orgWrapper.IsUserFlag3);
		}

		public void TestIsUserFlag4()
		{
			AssertEquals("IsUserFlag4", headerWrapper.IsUserFlag4, orgWrapper.IsUserFlag4);
		}

		public void TestIsUserFlag5()
		{
			AssertEquals("IsUserFlag5", headerWrapper.IsUserFlag5, orgWrapper.IsUserFlag5);
		}

		public void TestIsUserFlag6()
		{
			AssertEquals("IsUserFlag6", headerWrapper.IsUserFlag6, orgWrapper.IsUserFlag6);
		}

		public void TestIsUserFlag7()
		{
			AssertEquals("IsUserFlag7", headerWrapper.IsUserFlag7, orgWrapper.IsUserFlag7);
		}

		public void TestIsUserFlag8()
		{
			AssertEquals("IsUserFlag8", headerWrapper.IsUserFlag8, orgWrapper.IsUserFlag8);
		}

		public void TestIsUserFlag9()
		{
			AssertEquals("IsUserFlag9", headerWrapper.IsUserFlag9, orgWrapper.IsUserFlag9);
		}

		public void TestIsUserFlag10()
		{
			AssertEquals("IsUserFlag10", headerWrapper.IsUserFlag10, orgWrapper.IsUserFlag10);
		}

		public void TestIsUserFlag11()
		{
			AssertEquals("IsUserFlag11", headerWrapper.IsUserFlag11, orgWrapper.IsUserFlag11);
		}

		public void TestIsUserFlag12()
		{
			AssertEquals("IsUserFlag12", headerWrapper.IsUserFlag12, orgWrapper.IsUserFlag12);
		}

		public void TestIsUserFlag13()
		{
			AssertEquals("IsUserFlag13", headerWrapper.IsUserFlag13, orgWrapper.IsUserFlag13);
		}

		public void TestIsUserFlag14()
		{
			AssertEquals("IsUserFlag14", headerWrapper.IsUserFlag14, orgWrapper.IsUserFlag14);
		}

		public void TestIsUserFlag15()
		{
			AssertEquals("IsUserFlag15", headerWrapper.IsUserFlag15, orgWrapper.IsUserFlag15);
		}

		public void TestIsUserFlag16()
		{
			AssertEquals("IsUserFlag16", headerWrapper.IsUserFlag16, orgWrapper.IsUserFlag16);
		}

		public void TestIsUserFlag17()
		{
			AssertEquals("IsUserFlag17", headerWrapper.IsUserFlag17, orgWrapper.IsUserFlag17);
		}

		public void TestIsUserFlag18()
		{
			AssertEquals("IsUserFlag18", headerWrapper.IsUserFlag18, orgWrapper.IsUserFlag18);
		}

		public void TestIsUserFlag19()
		{
			AssertEquals("IsUserFlag19", headerWrapper.IsUserFlag19, orgWrapper.IsUserFlag19);
		}

		public void TestIsUserFlag20()
		{
			AssertEquals("IsUserFlag20", headerWrapper.IsUserFlag20, orgWrapper.IsUserFlag20);
		}

		public void TestIsUserFlag21()
		{
			AssertEquals("IsUserFlag21", headerWrapper.IsUserFlag21, orgWrapper.IsUserFlag21);
		}

		public void TestIsUserFlag22()
		{
			AssertEquals("IsUserFlag22", headerWrapper.IsUserFlag22, orgWrapper.IsUserFlag22);
		}

		public void TestIsUserFlag23()
		{
			AssertEquals("IsUserFlag23", headerWrapper.IsUserFlag23, orgWrapper.IsUserFlag23);
		}

		public void TestIsUserFlag24()
		{
			AssertEquals("IsUserFlag24", headerWrapper.IsUserFlag24, orgWrapper.IsUserFlag24);
		}

		public void TestIsUserFlag25()
		{
			AssertEquals("IsUserFlag25", headerWrapper.IsUserFlag25, orgWrapper.IsUserFlag25);
		}

		public void TestIsUserFlag26()
		{
			AssertEquals("IsUserFlag26", headerWrapper.IsUserFlag26, orgWrapper.IsUserFlag26);
		}

		public void TestIsUserFlag27()
		{
			AssertEquals("IsUserFlag27", headerWrapper.IsUserFlag27, orgWrapper.IsUserFlag27);
		}

		public void TestIsUserFlag28()
		{
			AssertEquals("IsUserFlag28", headerWrapper.IsUserFlag28, orgWrapper.IsUserFlag28);
		}

		public void TestIsUserFlag29()
		{
			AssertEquals("IsUserFlag29", headerWrapper.IsUserFlag29, orgWrapper.IsUserFlag29);
		}

		public void TestIsUserFlag30()
		{
			AssertEquals("IsUserFlag30", headerWrapper.IsUserFlag30, orgWrapper.IsUserFlag30);
		}

		public void TestIsUserFlag31()
		{
			AssertEquals("IsUserFlag31", headerWrapper.IsUserFlag31, orgWrapper.IsUserFlag31);
		}

		public void TestIsUserFlag32()
		{
			AssertEquals("IsUserFlag32", headerWrapper.IsUserFlag32, orgWrapper.IsUserFlag32);
		}

		public void TestIsWarehouseClient()
		{
			AssertEquals("IsWarehouseClient", headerWrapper.IsWarehouseClient, orgWrapper.IsWarehouseClient);
		}

		public void TestMobile()
		{
			AssertEquals("Mobile", headerWrapper.Mobile, orgWrapper.Mobile);
		}

		public void TestPhone()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Phone", orgWrapper.LocationAddress.Phone, orgWrapper.Phone);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("Phone", ZString.Empty, orgWrapper.Phone);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_Phone = "1234567890";
			AssertEquals("Phone", "1234567890", orgWrapper.Phone);
		}

		public void TestPostCode()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostCode", orgWrapper.LocationAddress.PostCode, orgWrapper.PostCode);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostCode", ZString.Empty, orgWrapper.PostCode);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_Postcode = "200001";
			AssertEquals("PostCode", "200001", orgWrapper.PostCode);
		}

		public void TestLoco()
		{
			AssertEquals("Loco", headerWrapper.Loco, orgWrapper.Loco);
		}

		public void TestState()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("State", orgWrapper.LocationAddress.State, orgWrapper.State);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("State", ZString.Empty, orgWrapper.State);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			USOrganisation.USOrganisationDocAddress.E2_State = "31";
			AssertEquals("State", "31", orgWrapper.State);
		}

		public void TestWeb()
		{
			AssertEquals("Web", headerWrapper.Web, orgWrapper.Web);
		}

		public void TestHandlingInstructions()
		{
			AssertEquals("HandlingInstructions", headerWrapper.HandlingInstructions, orgWrapper.HandlingInstructions);
		}

		public void TestCartageInstructions()
		{
			AssertEquals("CartageInstructions", headerWrapper.CartageInstructions, orgWrapper.CartageInstructions);
		}

		public void TestGetCartageInstructionsByTransportMode()
		{
			Enterprise.DocumentWrappers.FreightHelperClass.AddNote(USOrganisation.Organisation, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Note Data", Enterprise.ZArchitecture.Business.StmNoteContextUtils.StmNoteContextsAll);
			AssertEquals("Preconditions: HeaderWrapper.GetCartageInstructionsByTransportMode should return non-empty value.", false, headerWrapper.GetCartageInstructionsByTransportOrContainerMode("", "").IsEmpty);
			AssertEquals("GetCartageInstructionsByTransportMode", headerWrapper.GetCartageInstructionsByTransportOrContainerMode("", ""), orgWrapper.GetCartageInstructionsByTransportOrContainerMode("", ""));
		}

		public void TestLocalCustomsClientCode()
		{
			AssertEquals("LocalCustomsClientCode", headerWrapper.LocalCustomsClientCode, orgWrapper.LocalCustomsClientCode);
		}

		public void TestLocalCustomsCarrierCode()
		{
			AssertEquals("LocalCustomsCarrierCode", headerWrapper.LocalCustomsCarrierCode, orgWrapper.LocalCustomsCarrierCode);
		}

		public void TestLocalBusinessRegNo()
		{
			AssertEquals("LocalBusinessRegNo", headerWrapper.LocalBusinessRegNo, orgWrapper.LocalBusinessRegNo);
		}

		public void TestLocalCustomsSupplierCode()
		{
			AssertEquals("LocalCustomsSupplierCode", headerWrapper.LocalCustomsSupplierCode, orgWrapper.LocalCustomsSupplierCode);
		}

		public void TestLocalRebateUserCode()
		{
			AssertEquals("LocalRebateUserCode", headerWrapper.LocalRebateUserCode, orgWrapper.LocalRebateUserCode);
		}

		public void TestLocalVATCode()
		{
			AssertEquals("LocalVATCode", headerWrapper.LocalVATCode, orgWrapper.LocalVATCode);
		}

		public void TestCustomAttrib1()
		{
			AssertEquals("CustomAttrib1", headerWrapper.CustomAttrib1, orgWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			AssertEquals("CustomAttrib2", headerWrapper.CustomAttrib2, orgWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			AssertEquals("CustomAttrib3", headerWrapper.CustomAttrib3, orgWrapper.CustomAttrib3);
		}

		public void TestCustomDecimal1()
		{
			AssertEquals("CustomDecimal1", headerWrapper.CustomDecimal1, orgWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			AssertEquals("CustomDecimal2", headerWrapper.CustomDecimal2, orgWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			AssertEquals("CustomDecimal3", headerWrapper.CustomDecimal3, orgWrapper.CustomDecimal3);
		}

		public void TestCustomDate1()
		{
			AssertEquals("CustomDate1", headerWrapper.CustomDate1, orgWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			AssertEquals("CustomDate2", headerWrapper.CustomDate2, orgWrapper.CustomDate2);
		}

		public void TestCustomDate3()
		{
			AssertEquals("CustomDate3", headerWrapper.CustomDate3, orgWrapper.CustomDate3);
		}

		public void TestCustomFlag1()
		{
			AssertEquals("CustomFlag1", headerWrapper.CustomFlag1, orgWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			AssertEquals("CustomFlag2", headerWrapper.CustomFlag2, orgWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			AssertEquals("CustomFlag3", headerWrapper.CustomFlag3, orgWrapper.CustomFlag3);
		}

		public void TestApprovedExporterCode()
		{
			AssertEquals("ApprovedExporterCode", headerWrapper.ApprovedExporterCode, orgWrapper.ApprovedExporterCode);
		}

		public void TestCarrierMasterBillPrefix()
		{
			AssertEquals("CarrierMasterBillPrefix", headerWrapper.CarrierMasterBillPrefix, orgWrapper.CarrierMasterBillPrefix);
		}

		public void TestCarrierAirlinePrefix()
		{
			AssertEquals("CarrierAirlinePrefix", headerWrapper.CarrierAirlinePrefix, orgWrapper.CarrierAirlinePrefix);
		}

		public void TestPostalAddress()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddress, orgWrapper.PostalAddress);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", ZString.Empty, orgWrapper.PostalAddress);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddress, orgWrapper.PostalAddress);
		}

		public void TestPostalAddressInEnglish()
		{
			var staffDE = Factory.New<GlbStaff>();
			staffDE.GS_Code = "ABC";
			staffDE.GS_LoginName = "Dieter";
			staffDE.GS_FullName = "Dieter";
			staffDE.GS_WorkingLanguage = Core.SharedConstants.Languages.German;

			Factory.Save();

			using (Env.Instance.SetTemporaryUserContext(staffDE.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				USOrganisation.ZO_OA_Address = SupplierAddress.PK;
				AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
				AssertEquals("PostalAddressInEnglish", orgWrapper.LocationAddress.PostalAddressInEnglish, orgWrapper.PostalAddressInEnglish);

				USOrganisation.ZO_OA_Address = ZGuid.Empty;
				AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
				AssertEquals("PostalAddress", ZString.Empty, orgWrapper.PostalAddressInEnglish);

				USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
				AssertEquals("PostalAddressInEnglish", orgWrapper.LocationAddress.PostalAddressInEnglish, orgWrapper.PostalAddressInEnglish);
			}
		}

		public void TestPostalAddressExcludeCountryIfSame_SameCountry()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddressExcludeCountryIfSame, orgWrapper.PostalAddressExcludeCountryIfSame);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", ZString.Empty, orgWrapper.PostalAddressExcludeCountryIfSame);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddressExcludeCountryIfSame, orgWrapper.PostalAddressExcludeCountryIfSame);
		}

		public void TestPostalAddressExcludeCountryIfSame_DifferentCountry()
		{
			USOrganisation.ZO_OA_Address = SupplierAddress.PK;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddressExcludeCountryIfSame, orgWrapper.PostalAddressExcludeCountryIfSame);
			USOrganisation.ZO_OA_Address = ZGuid.Empty;
			AssertNotNull("LocationAddress", orgWrapper.LocationAddress);
			AssertEquals("PostalAddress", ZString.Empty, orgWrapper.PostalAddressExcludeCountryIfSame);

			USOrganisation.USOrganisationDocAddress.E2_AddressOverride = true;
			AssertEquals("PostalAddress", orgWrapper.LocationAddress.PostalAddressExcludeCountryIfSame, orgWrapper.PostalAddressExcludeCountryIfSame);
		}

		public void TestPostalAddressExcludeName()
		{
			AssertEquals("PostalAddressExcludeName", headerWrapper.PostalAddressExcludeName, orgWrapper.PostalAddressExcludeName);
		}

		public void TestCountry()
		{
			AssertEquals("Country", headerWrapper.Country, orgWrapper.Country);
		}

		public void TestCountryData()
		{
			AssertEquals("CountryData", headerWrapper.CountryData.ToString(), orgWrapper.CountryData.ToString());
		}

		public void TestMiscServ()
		{
			AssertEquals("MiscServ", headerWrapper.MiscServ.ToString(), orgWrapper.MiscServ.ToString());
		}

		public void TestSCAC()
		{
			AssertEquals("SCAC", headerWrapper.SCAC, orgWrapper.SCAC);
		}

		public void TestABN()
		{
			AssertEquals("ABN", headerWrapper.ABN, orgWrapper.ABN);
		}

		public void TestGST()
		{
			AssertEquals("GST", headerWrapper.GST, orgWrapper.GST);
		}

		public void TestCBR()
		{
			AssertEquals("CBR", headerWrapper.CBR, orgWrapper.CBR);
		}

		public void TestCCC()
		{
			AssertEquals("CCC", headerWrapper.CCC, orgWrapper.CCC);
		}

		public void TestCSC()
		{
			AssertEquals("CSC", headerWrapper.CSC, orgWrapper.CSC);
		}

		public void TestCCD()
		{
			AssertEquals("CCD", headerWrapper.CCD, orgWrapper.CCD);
		}

		public void TestCID()
		{
			AssertEquals("CID", headerWrapper.CID, orgWrapper.CID);
		}

		public void TestLSC()
		{
			AssertEquals("LSC", headerWrapper.LSC, orgWrapper.LSC);
		}

		public void TestPAN()
		{
			AssertEquals("PAN", headerWrapper.PAN, orgWrapper.PAN);
		}

		public void TestDisbursmentTerms()
		{
			AssertEquals("DisbursmentTerms", headerWrapper.DisbursmentTerms, orgWrapper.DisbursmentTerms);
		}

		public void TestShortDisbursementTerms()
		{
			AssertEquals("ShortDisbursementTerms", headerWrapper.ShortDisbursementTerms, orgWrapper.ShortDisbursementTerms);
		}

		public void TestShortInvoiceTerms()
		{
			AssertEquals("ShortInvoiceTerms", headerWrapper.ShortInvoiceTerms, orgWrapper.ShortInvoiceTerms);
		}

		public void TestDeliverAddress()
		{
			AssertEquals("DeliverAddress", headerWrapper.DeliverAddress.ToString(), orgWrapper.DeliverAddress.ToString());
		}

		public void TestDeliverDocAddress()
		{
			AssertEquals("DeliverDocAddress", headerWrapper.DeliverDocAddress.ToString(), orgWrapper.DeliverDocAddress.ToString());
		}

		public void TestPickUpAddress()
		{
			AssertEquals("PickUpAddress", headerWrapper.PickUpAddress.ToString(), orgWrapper.PickUpAddress.ToString());
		}

		public void TestPickUpDocAddress()
		{
			AssertEquals("PickUpDocAddress", headerWrapper.PickUpDocAddress.ToString(), orgWrapper.PickUpDocAddress.ToString());
		}

		public void TestARAddress()
		{
			AssertEquals("ARAddress", headerWrapper.ARAddress.ToString(), orgWrapper.ARAddress.ToString());
		}

		public void TestSplitFullName1()
		{
			AssertEquals("SplitFullName1", headerWrapper.SplitFullName1, orgWrapper.SplitFullName1);
		}

		public void TestSplitFullName2()
		{
			AssertEquals("SplitFullName2", headerWrapper.SplitFullName2, orgWrapper.SplitFullName2);
		}

		public void TestStaffAssignments()
		{
			AssertNotNull(USOrganisation.Organisation);
			USOrganisation.Organisation.StaffAssignments.RemoveAndDeleteAll();
			OrgStaffAssignments item = USOrganisation.Organisation.StaffAssignments.AddNew();
			item.O8_Department = "SEA";
			AssertEquals("Count", 1, orgWrapper.StaffAssignments.Count);
			AssertEquals("SEA", orgWrapper.StaffAssignments[0].O8_Department);

			USOrganisation.ZO_OH_Organisation = ZGuid.Empty;
			AssertNull(USOrganisation.Organisation);
			AssertEquals("Count", 0, orgWrapper.StaffAssignments.Count);
		}

		#endregion

		#region Implementation
		Enterprise.DocumentWrappers.OrgHeaderSource headerWrapper;
		USOrganisationSource orgWrapper;

		protected override void SetUp()
		{
			base.SetUp();
			orgWrapper = USOrganisationSource.New(USOrganisation, Factory);
			AssertNotNull("OrgWrapper created not null", orgWrapper);
			headerWrapper = Enterprise.DocumentWrappers.OrgHeaderSource.New(USOrganisation.Organisation, Factory);
			AssertNotNull("HeaderWrapper created not null", headerWrapper);
		}

		#endregion
	}
}
