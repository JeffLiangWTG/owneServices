using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.SADH.Testing
{
	sealed class SADHFormDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.WeightUQList; });
			AssertEquals("SADHData.Lookups.WeightUQList.ContainsCode(\"KG\")", true, SADHData.Lookups.WeightUQList.ContainsCode("KG"));
			AssertEquals("SADHData.Lookups.WeightUQList.ContainsCode(\"OZ\")", true, SADHData.Lookups.WeightUQList.ContainsCode("OZ"));
			AssertEquals("SADHData.Lookups.WeightUQList.ContainsCode(\"TN\")", true, SADHData.Lookups.WeightUQList.ContainsCode("TN"));
		}

		public void TestInvoiceLineUQList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.InvoiceLineUQList; });
			AssertEquals("SADHData.Lookups.InvoiceLineUQList.ContainsCode(\"BAG\")", true, SADHData.Lookups.InvoiceLineUQList.ContainsCode("BAG"));
			AssertEquals("SADHData.Lookups.InvoiceLineUQList.ContainsCode(\"CAS\")", true, SADHData.Lookups.InvoiceLineUQList.ContainsCode("CAS"));
			AssertEquals("SADHData.Lookups.InvoiceLineUQList.ContainsCode(\"DOZ\")", true, SADHData.Lookups.InvoiceLineUQList.ContainsCode("DOZ"));
		}

		public void TestVesselList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.VesselList; });
			AssertEquals(((IBusinessObjectCollection)Declaration.Lookups.Vessels).TypeOfElements, ((IBusinessObjectCollection)SADHData.Lookups.VesselList).TypeOfElements);
			AssertEquals(Declaration.Lookups.Vessels.CompleteFilter, SADHData.Lookups.VesselList.CompleteFilter);
		}

		public void TestBranchesList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.BranchesList; });
			AssertEquals(Declaration.Lookups.BranchCollection.TypeOfElements, SADHData.Lookups.BranchesList.TypeOfElements);
			AssertEquals(Declaration.Lookups.BranchCollection.Count, SADHData.Lookups.BranchesList.Count);
		}

		public void TestPackagesTypeList()
		{
			AssertEquals(Declaration.Lookups.JE_TotalNoOfPacksPackType_List, SADHData.Lookups.PackagesTypeList);
		}

		public void TestDeliveryTermsList()
		{
			AssertEquals(Declaration.Lookups.IncoTermList.ElementsAsString, SADHData.Lookups.DeliveryTermsList.ElementsAsString);
		}

		public void TestModeOfTransportAtBorderList()
		{
			AssertEquals(Declaration.Lookups.TransportTypeList.ElementsAsString, SADHData.Lookups.ModeOfTransportAtBorderList.ElementsAsString);
		}

		public void TestMessageTypeList()
		{
			AssertEquals(Declaration.Lookups.MessageTypeList.ElementsAsString, SADHData.Lookups.MessageTypeList.ElementsAsString);
		}

		public void TestCountryList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.CountryList; });
			RefCountryCollection countryList = SADHData.Lookups.CountryList;
			Assert("Contains AU", countryList.Contains(RefCountry.LoadFromCountryCode(Factory, "AU")));
			Assert("Contains NZ", countryList.Contains(RefCountry.LoadFromCountryCode(Factory, "NZ")));
			Assert("Contains GB", countryList.Contains(RefCountry.LoadFromCountryCode(Factory, "GB")));
			Assert("Contains US", countryList.Contains(RefCountry.LoadFromCountryCode(Factory, "US")));
		}

		public void TestOriginList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.OriginList; });

			RefUNLOCO nzakl = new RefUNLOCO.Loader(Factory).Load("NZAKL");
			RefUNLOCO ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			RefUNLOCO gblon = new RefUNLOCO.Loader(Factory).Load("GBLON");
			RefUNLOCO gblhr = new RefUNLOCO.Loader(Factory).Load("GBLHR");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
				AssertUNLOCOIsInError(SADHData.Lookups.OriginList, nzakl.Code, false);
				AssertUNLOCOIsInError(SADHData.Lookups.OriginList, ausyd.Code, false);
				//AssertUNLOCOIsInError(SADHData.Lookups.OriginList, gblon.Code, true);
				//AssertUNLOCOIsInError(SADHData.Lookups.OriginList, gblhr.Code, true);

				SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
				//AssertUNLOCOIsInError(SADHData.Lookups.OriginList, nzakl.Code, true);
				//AssertUNLOCOIsInError(SADHData.Lookups.OriginList, ausyd.Code, true);
				AssertUNLOCOIsInError(SADHData.Lookups.OriginList, gblon.Code, false);
				AssertUNLOCOIsInError(SADHData.Lookups.OriginList, gblhr.Code, false);

				PortListCheckDefaults(SADHData.Lookups.OriginList);
			}
		}

		public void TestFinalDestinationList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.FinalDestinationList; });

			RefUNLOCO nzakl = new RefUNLOCO.Loader(Factory).Load("NZAKL");
			RefUNLOCO ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			RefUNLOCO gblon = new RefUNLOCO.Loader(Factory).Load("GBLON");
			RefUNLOCO gblhr = new RefUNLOCO.Loader(Factory).Load("GBLHR");
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
				//AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, nzakl.Code, true);
				//AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, ausyd.Code, true);
				AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, gblon.Code, false);
				AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, gblhr.Code, false);

				SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
				AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, nzakl.Code, false);
				AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, ausyd.Code, false);
				//AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, gblon.Code, true);
				//AssertUNLOCOIsInError(SADHData.Lookups.FinalDestinationList, gblhr.Code, true);

				PortListCheckDefaults(SADHData.Lookups.FinalDestinationList);
			}
		}

		public void TestSuppliersList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.SuppliersList; });

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "REEBOK";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "RIFLES";
			consignee.OH_IsConsignor = true;
			consignee.OH_IsConsignee = true;

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, false);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, false);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			consignor.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			consignor.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, true);

			consignee.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, false);
			consignee.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, true);

			consignor.OH_IsConsignor = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			consignee.OH_IsConsignee = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, true);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;
			consignee.OH_IsConsignor = true;
			consignee.OH_IsConsignee = true;

			consignor.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			consignor.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, true);

			consignee.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, false);
			consignee.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, true);

			consignor.OH_IsConsignor = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignor.OH_Code, false);
			consignee.OH_IsConsignee = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.SuppliersList, consignee.OH_Code, true);
		}

		public void TestImportersList()
		{
			AssertNoExceptionThrown(delegate
			{ object fred = SADHData.Lookups.ImportersList; });

			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "REEBOK";
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "RIFLES";
			consignee.OH_IsConsignor = true;
			consignee.OH_IsConsignee = true;

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, false);
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, false);
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Import;
			consignor.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, false);
			consignor.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, true);

			consignee.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);
			consignee.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, true);

			consignee.OH_IsConsignee = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);
			consignor.OH_IsConsignor = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, true);

			SADHData.D1_MessageType = JobMessageTypeList.Codes.Export;
			consignor.OH_IsConsignor = true;
			consignor.OH_IsConsignee = true;
			consignee.OH_IsConsignor = true;
			consignee.OH_IsConsignee = true;

			consignor.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, false);
			consignor.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, true);

			consignee.OH_IsConsignor = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);
			consignee.OH_IsConsignee = false;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, true);

			consignee.OH_IsConsignee = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignee.OH_Code, false);
			consignor.OH_IsConsignor = true;
			AssertOrgHeaderIsInError(SADHData.Lookups.ImportersList, consignor.OH_Code, true);
		}

		#region Implementation
		void PortListCheckDefaults(RefUNLOCOCollection list)
		{
			FilterBusinessObjectDefaults filterBODefs = list.FilterBusinessObjectDefaults;
			AssertEquals("filterBODefs.ContainsDefaultFor(\"Code:Property\")", true, filterBODefs.ContainsDefaultFor("Code:Property"));
		}

		void AssertUNLOCOIsInError(RefUNLOCOCollection list, ZString unloco, bool expectedToBeInError)
		{
			list.AdditionalFilter = new ZQuery(RefUNLOCOSchema.RL_Code, unloco);
			AssertEquals("Precondition: list.Count", 1, list.Count);
			RefUNLOCO refUNLOCO = list[0];
			AssertEquals("Precondition: list[0].RL_Code", unloco, refUNLOCO.RL_Code);
			ZQuery rowValidationFilter = list.Relationship.RelationshipFilter;
			bool hasError = !refUNLOCO.MatchesFilter(rowValidationFilter);
			AssertEquals("Is the UNLOCO [" + unloco + "] showing an error?", expectedToBeInError, hasError);
		}

		void AssertOrgHeaderIsInError(OrgHeaderCollection list, ZString orgHeaderCode, bool expectedToBeInError)
		{
			list.Load(new ZQuery(OrgHeaderSchema.OH_Code, orgHeaderCode));
			AssertEquals("Precondition: list.Count", 1, list.Count);
			OrgHeader orgHeader = list[0];
			AssertEquals("Precondition: orgHeader.OH_Code", orgHeaderCode, orgHeader.OH_Code);

			ZQuery rowValidationFilter = ((IBusinessObjectCollectionTestingMembers)list).AdditionalFilter;
			bool hasError = !orgHeader.MatchesFilter(rowValidationFilter);
			AssertEquals("Is the OrgHeader [" + orgHeaderCode + "] showing an error?", expectedToBeInError, hasError);
		}

		SADHFormData SADHData
		{
			get { return fSADHData ?? (fSADHData = new SADHFormData(Factory, Declaration)); }
		}
		SADHFormData fSADHData;

		BaseJobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<BaseJobDeclaration>()); }
		}
		BaseJobDeclaration fDeclaration;
		#endregion

	}
}
