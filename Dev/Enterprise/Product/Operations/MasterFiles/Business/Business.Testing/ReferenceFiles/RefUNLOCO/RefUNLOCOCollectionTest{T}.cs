using System;
using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class RefUNLOCOCollectionTest<T> : ActiveBusinessObjectCollectionTestCase<T> where T : RefUNLOCOCollection
	{
		public void TestNearestMatch()
		{
			RefUNLOCO loco1 = Factory.New<RefUNLOCO>();
			loco1.RL_Code = "12345";
			loco1.RL_IATA = "345";

			RefUNLOCO loco2 = Factory.New<RefUNLOCO>();
			loco2.RL_Code = "34567";
			loco2.RL_IATA = "567";

			RefUNLOCO loco3 = Factory.New<RefUNLOCO>();
			loco3.RL_Code = "12121";
			loco3.RL_IATA = "567";

			RefUNLOCOCollection collection = new RefUNLOCOCollection(Factory);
			string result = null;

			result = ((IFindBoxListProvider)collection).NearestMatch("345", true, -1).Item1;
			AssertEquals("AutoCompleted to the IATA when 3 chars", "12345", result);

			result = ((IFindBoxListProvider)collection).NearestMatch("345", false, -1).Item1;
			AssertEquals("AutoCompleted to the IATA when 3 chars - even without explicit call", "12345", result);

			result = ((IFindBoxListProvider)collection).NearestMatch("34", true, -1).Item1;
			AssertEquals("AutoCompleted to the UNLOCO when 2 chars", "34567", result);

			result = ((IFindBoxListProvider)collection).NearestMatch("34", false, -1).Item1;
			AssertEquals("NOT AutoCompleted to the UNLOCO when not explicit autocomplete", "34", result);

			result = ((IFindBoxListProvider)collection).NearestMatch("123", true, -1).Item1;
			AssertEquals("AutoComplete to UNLOCO when 3 characters and IATA not found", "12345", result);

			result = ((IFindBoxListProvider)collection).NearestMatch("999", true, -1).Item1;
			AssertEquals("AutoComplete returns same when not found", "999", result);
		}

		public void TestAutoCompleteOnCommit()
		{
			RefUNLOCOCollection testCollection = new RefUNLOCOCollection(Factory);
			AssertEquals(true, (((IFindBoxListProvider)testCollection).AutoCompleteOnCommit));
		}

		public void TestSetFilterBusinessObjectDefaults()
		{
			RefUNLOCOCollection refUNLOCOCollection = new RefUNLOCOCollection(Factory);

			Assert("Should not contain Servicing Postal Code:Property", !refUNLOCOCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			refUNLOCOCollection.SetPostCodeDefault(false);
			Assert("Should contain Servicing Postal Code:Property", refUNLOCOCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			refUNLOCOCollection.SetPostCodeDefault(true);
			Assert("Should not contain Servicing Postal Code:Property", !refUNLOCOCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			refUNLOCOCollection.SetPostCodeDefault(false);
			Assert("Should not contain Servicing Postal Code:Property", !refUNLOCOCollection.FilterBusinessObjectDefaults.ContainsDefaultFor("Servicing Postal Code:Property"));
		}

		public void TestDescriptionFromRefLocoMap()
		{
			RefUNLOCO loco = Factory.New<RefUNLOCO>();
			loco.RL_Code = "12365";
			loco.RL_PortName = "TESTEST";
			loco.RL_HasAirport = ZBool.True;
			loco.RL_IATA = "345";

			RefUNLOCOCollection collection = new RefUNLOCOCollection(Factory, "AAA");

			AssertEquals(loco.RL_PortName, ((IFindBoxListProvider)collection).DescriptionFromCode("12365"));

			RefLocoMap localMapping = Factory.New<RefLocoMap>();
			localMapping.RY_LocalPortCode = "1111";
			localMapping.RY_RL_NKLocoPort = "12365";
			localMapping.RY_RN = GlbCompany.CurrentCompany.Country.PK;
			localMapping.RY_SystemUsage = "AAA";

			RefLocoMap localMapping2 = Factory.New<RefLocoMap>();
			localMapping2.RY_LocalPortCode = "2222";
			localMapping2.RY_RL_NKLocoPort = "12365";
			localMapping2.RY_RN = GlbCompany.CurrentCompany.Country.PK;
			localMapping2.RY_SystemUsage = "BBB";

			AssertEquals("TESTEST (1111)", ((IFindBoxListProvider)collection).DescriptionFromCode("12365"));
		}

		public void TestUserUNLOCOIdentifierFilterBusinessObjectDefaults()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var codeDescriptionValues = new CodeDescriptionPairList();
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasAirport, "Has Airport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRail, "Has Rail");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasRoad, "Has Road");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasSeaport, "Has Seaport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasTerminal, "Has Terminal");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasOutport, "Has Outport");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasDischarge, "Has Discharge");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasUnload, "Has Unload");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasStore, "Has Store");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasPost, "Has Post");
			codeDescriptionValues.AddPair(RefUNLOCOSchema.Constants.RL_HasCustomsLodge, "Has Customs");

			var collection = new CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection(new ReadOnlyCodeDescriptionPairList(codeDescriptionValues));
			collection[0].Bool = true;
			collection[1].Bool = true;
			collection[2].Bool = false;
			collection[3].Bool = false;
			collection[4].Bool = true;
			collection[5].Bool = true;
			collection[6].Bool = false;
			collection[7].Bool = false;
			collection[8].Bool = true;
			collection[9].Bool = true;
			collection[10].Bool = false;

			using (OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var testCollection = new RefUNLOCOCollection(Factory, org);
					var identifiers = OrganisationsDataRegistry.Instance.UNLOCODefaultingRules.Value.Cast<CodeDescriptionBoolDisallowNewWithDefaultDisabled>();
					var useAnd = OrganisationsDataRegistry.Instance.RequireAllUNLOCOConditionsToBeMet.Value;

					AssertNoDefault("Precondition", testCollection, "UNLOCO Identifiers", "AndJoinCondition");

					(testCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter

					CombineAssertions("Filter defaults should be set based on registry value", () =>
					{
						AssertHasDefault(testCollection, "UNLOCO Identifiers", "AndJoinCondition", (ZBool)useAnd);
						AssertHasDefault(testCollection, "UNLOCO Identifiers", "OrJoinCondition", (ZBool)!useAnd);

						AssertHasDefault("Has Airport: ", testCollection, "UNLOCO Identifiers", "Property0", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasAirport, identifiers));
						AssertHasDefault("Has Rail: ", testCollection, "UNLOCO Identifiers", "Property1", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasRail, identifiers));
						AssertHasDefault("RL_HasRoad: ", testCollection, "UNLOCO Identifiers", "Property2", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasRoad, identifiers));
						AssertHasDefault("RL_HasSeaport: ", testCollection, "UNLOCO Identifiers", "Property3", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasSeaport, identifiers));
						AssertHasDefault("RL_HasTerminal: ", testCollection, "UNLOCO Identifiers", "Property4", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasTerminal, identifiers));
						AssertHasDefault("RL_HasOutport: ", testCollection, "UNLOCO Identifiers", "Property5", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasOutport, identifiers));
						AssertHasDefault("RL_HasDischarge: ", testCollection, "UNLOCO Identifiers", "Property6", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasDischarge, identifiers));
						AssertHasDefault("RL_HasUnload: ", testCollection, "UNLOCO Identifiers", "Property7", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasUnload, identifiers));
						AssertHasDefault("RL_HasStore: ", testCollection, "UNLOCO Identifiers", "Property8", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasStore, identifiers));
						AssertHasDefault("RL_HasPost: ", testCollection, "UNLOCO Identifiers", "Property9", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasPost, identifiers));
						AssertHasDefault("RL_HasCustoms: ", testCollection, "UNLOCO Identifiers", "Property10", HelperMethodGetIdentifier(RefUNLOCOSchema.Constants.RL_HasCustomsLodge, identifiers));
					});
				}

				using (OrganisationsDataRegistry.Instance.IgnoreUNLOCODefaultingRules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var testCollection = new RefUNLOCOCollection(Factory, org);
					AssertNoDefault("Precondition", testCollection, "UNLOCO Identifiers", "AndJoinCondition");

					(testCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter

					CombineAssertions("Filter does not exist", () =>
					{
						AssertNoDefault(testCollection, "UNLOCO Identifiers", "OrJoinCondition");
						AssertNoDefault("Has Airport: ", testCollection, "UNLOCO Identifiers", "Property0");
					});
				}
			}
		}

		public void TestUserCountryStateFilterBusinessObjectDefaults()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.MainAddress.OA_State = "NSW";
			Factory.Save();

			RefUNLOCOCollection testCollectionD = new RefUNLOCOCollection(Factory);

			CombineAssertions("Filter defaults should not be set", () =>
			{
				AssertNoDefault("Should not have country filter", testCollectionD, "CountryState", "Property1");
				AssertNoDefault("Should not have state filter", testCollectionD, "CountryState", "Property2");
			});

			RefUNLOCOCollection testCollection = new RefUNLOCOCollection(Factory, org);

			CombineAssertions("Filter defaults should not be set", () =>
			{
				AssertNoDefault("Should not have country filter", testCollectionD, "CountryState", "Property1");
				AssertNoDefault("Should not have state filter", testCollectionD, "CountryState", "Property2");
			});

			(testCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter

			CombineAssertions("Country/State filters should be set", () =>
			{
				AssertHasDefault(testCollection, "CountryState", "Property1", (ZString)"AU");
				AssertHasDefault(testCollection, "CountryState", "Property2", org.MainAddress.RelatedState.PK);
			});

			org.MainAddress.OA_RN_NKCountryCode = "CN";
			org.MainAddress.OA_State = string.Empty;

			(testCollection.FilterBusinessObjectDefaults as IEnumerable).GetEnumerator(); // Mock Add FilterBusinessObjectDefault to filter

			CombineAssertions("Country/State filters should be update", () =>
			{
				AssertHasDefault(testCollection, "CountryState", "Property1", (ZString)"CN");
				AssertHasDefault(testCollection, "CountryState", "Property2", ZGuid.Empty);
			});
		}

		public void TestFilterBusinessObjectDefaultsWhenRegistrySayYesOrNo()
		{
			using (OrganisationsDataRegistry.Instance.EnablePredefinedUNLOCOFilterInOrganizations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertFilterBusinessObjectDefaultsAreExisted(true);
			}

			using (OrganisationsDataRegistry.Instance.EnablePredefinedUNLOCOFilterInOrganizations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertFilterBusinessObjectDefaultsAreExisted(false);
			}
		}

		void AssertFilterBusinessObjectDefaultsAreExisted(bool existed)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testCollection = new RefUNLOCOCollection(Factory, orgHeader);
			AssertEquals(existed, testCollection.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().Any());
		}

		ZBool HelperMethodGetIdentifier(string code, System.Collections.Generic.IEnumerable<CodeDescriptionBoolDisallowNewWithDefaultDisabled> identifiers)
		{
			var result = false;

			var regsetting = identifiers.SingleOrDefault(d => d.Code == code);
			if (regsetting != null)
			{
				result = regsetting.Bool;
			}
			return result;
		}
	}
}
