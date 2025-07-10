using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPatternMatchOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Parent should be passed in the constructor", Parent, Lookups.Parent);
		}
		
		public void TestOO_Relationship_List()
		{
			AssertEquals(17, Lookups.OO_Relationship_List.Count);
			AssertEquals((NoResString)"Organization", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Organisation));
			AssertEquals((NoResString)"Port", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Port));
			AssertEquals((NoResString)"Currency", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Currency));
			AssertEquals((NoResString)"Country/Region", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Country));
			AssertEquals((NoResString)"Commodity", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Commodities));
			AssertEquals((NoResString)"Drop Mode", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.DropMode));
			AssertEquals((NoResString)"Equipment", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Equipment));
			AssertEquals((NoResString)"Incoterm", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.IncoTerm));
			AssertEquals((NoResString)"Container Type", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.ContainerType));
			AssertEquals((NoResString)"Charge Code", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.ChargeCodes));
			AssertEquals((NoResString)"Package Type", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.PackageType));
			AssertEquals((NoResString)"Event Code", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.EventCode));
			AssertEquals((NoResString)"Warehouse", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.Warehouse));
			AssertEquals((NoResString)"Service Level", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.ServiceLevel));
			AssertEquals((NoResString)"International Zone", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.IntZone));
			AssertEquals((NoResString)"Carrier Service Level", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel));
			AssertEquals((NoResString)"Document Type", Lookups.OO_Relationship_List.GetDescriptionFromCode(Constants.OrgPatternMatchOverrideRelationships.DocumentType));
		}

		public void TestOO_Context_List()
		{
			var list = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", ""),
				new CodeDescriptionPair("DEF", ""),
			};
			using (OrganisationsDataRegistry.Instance.UserDefinedContext.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, list))
			{
				Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DropMode;
				AssertEquals(2, Lookups.OO_Context_List.Count);

				var lookup = Lookups.OO_Context_List.GetAllCodes();
				AssertEquals("ABC", lookup[0]);
				AssertEquals("DEF", lookup[1]);

				Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
				AssertEquals(3, Lookups.OO_Context_List.Count);

				lookup = Lookups.OO_Context_List.GetAllCodes();
				AssertEquals("ABC", lookup[0]);
				AssertEquals("DEF", lookup[1]);
				AssertEquals("OCM", lookup[2]);
			}
		}

		[ExpectNoExceptions]
		public void TestOO_Relationship_ListSortedByDescription()
		{
			string previousDescription = null;

			foreach (CodeDescriptionPair pair in Lookups.OO_Relationship_List)
			{
				if (previousDescription != null)
				{
					if (StringComparer.OrdinalIgnoreCase.Compare(previousDescription, pair.Description) > 0)
					{
						Fail(string.Format("'{0}' should appear after '{1}'", pair.Description, previousDescription));
					}
				}
			}
		}

		public void TestOO_IncoTerm_List()
		{
			CodeDescriptionPairList expected = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncludingDomesticTerms);
			AssertEquals(expected.Count, Lookups.OO_IncoTerm_List.Count);
			AssertEquals(expected.CodesAsString, Lookups.OO_IncoTerm_List.CodesAsString);
		}

		public void TestOrganisations()
		{
			AssertNotNull(Lookups.Organisations);
		}

		public void TestRefUNLOCOs()
		{
			AssertNotNull(Lookups.RefUNLOCOs);
		}

		public void TestRefCurrencies()
		{
			AssertNotNull(Lookups.RefCurrencies);
		}

		public void TestCommodities()
		{
			AssertNotNull(Lookups.Commodities);
			AssertType<RefCommodityCodeCollection>(Lookups.Commodities);
		}

		public void TestDropModes()
		{
			AssertNotNull(Lookups.DropModes);
			AssertType<CombinedEquipmentNeededList>(Lookups.DropModes);
		}

		public void TestEquipment()
		{
			AssertNotNull(Lookups.Equipment);
			AssertType<RefEquipmentCollection>(Lookups.Equipment);
		}

		public void TestRefCountries()
		{
			AssertNotNull(Lookups.RefCountries);
		}

		public void TestRefContainers()
		{
			AssertNotNull(Lookups.RefContainers);
		}

		public void TestChargeCodes()
		{
			AssertNotNull(Lookups.ChargeCodes);
		}

		public void TestPackageTypes()
		{
			AssertNotNull(Lookups.PackageTypes);
		}

		public void TestEventTypes()
		{
			AssertNotNull(Lookups.EventTypes);
		}

		public void TestWhsWarehouses()
		{
			AssertNotNull(Lookups.WhsWarehouses);
		}

		public void TestRefServiceLevel()
		{
			AssertNotNull(Lookups.RefServiceLevels);
		}

		public void TestDocumentType()
		{
			AssertNotNull(Lookups.DocTypes);
		}

		public void TestZones()
		{
			AssertNotNull(Lookups.IntZones);
		}

		public void TestCarrierServiceLevels()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			var svcLvl = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl.PL_Code = "ABC";
			svcLvl.PL_CarrierServiceLevelDescription = "TestLevel";
			Factory.Save();

			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride.OO_OH = org.PK;
			var lookups = new OrgPatternMatchOverrideLookupsForTest(orgPatternMatchOverride);

			AssertNotNull(lookups.CarrierServiceLevels);
			AssertEquals("ABC", lookups.CarrierServiceLevels[0].PL_Code);
		}

		public void TestOrgCoNames()
		{
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ContainerType;
			TestRefContainers();
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Country;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Commodities;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Equipment;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Warehouse;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ServiceLevel;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IntZone;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DocumentType;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.DropMode;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.IncoTerm;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.PackageType;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			AssertNotNull(Lookups.OrgCoNames);
			Parent.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			AssertNotNull(Lookups.OrgCoNames);
		}

		public void TestOO_ForeignCode_List()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.CarrierServiceLevels.RemoveAndDeleteAll();
			var svcLvl = org.MiscServ.CarrierServiceLevels.AddNew();
			svcLvl.PL_Code = "ABC";
			svcLvl.PL_CarrierServiceLevelDescription = "TestLevel";
			Factory.Save();

			var orgPatternMatchOverride = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			orgPatternMatchOverride.OO_OH = org.PK;
			var lookups = new OrgPatternMatchOverrideLookupsForTest(orgPatternMatchOverride);

			orgPatternMatchOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.IsENettOrganisation(It.IsAny<ZGuid>())).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertContainsExactElementsInAnyOrder(orgPatternMatchOverride.eNettGenericChargeCodes, lookups.OO_ForeignCode_List);
			}
		}

		#region Implementation

		OrgPatternMatchOverrideLookupsForTest Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new OrgPatternMatchOverrideLookupsForTest(Parent);
				}
				return fLookups;
			}
		}

		OrgPatternMatchOverride Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.New<OrgPatternMatchOverride>();
				}
				return fParent;
			}
		}

		OrgPatternMatchOverride fParent;
		OrgPatternMatchOverrideLookupsForTest fLookups;

		#region OrgPatternMatchOverrideLookupsForTest

		class OrgPatternMatchOverrideLookupsForTest : OrgPatternMatchOverrideLookups
		{
			public OrgPatternMatchOverrideLookupsForTest(AutoOrgPatternMatchOverride parent)
				: base(parent)
			{
			}

			public new BusinessObject Parent
			{
				get { return base.Parent; }
			}
		}

		#endregion

		#endregion
	}
}
