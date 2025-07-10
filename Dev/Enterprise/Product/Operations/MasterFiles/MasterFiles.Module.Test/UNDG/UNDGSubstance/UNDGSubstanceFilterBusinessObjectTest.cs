using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGSubstanceFilterBusinessObject))]
	sealed class UNDGSubstanceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestTranslatedNames()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_PSN = "I am a prawn";
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "123";
			subs2.DG_Variant = "b";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs2.DG_PSN = "I am a prawn";

			var att1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First().Names.AddNew();
			att1.DA_Language = Core.SharedConstants.Languages.Russian;
			att1.DA_Descriptor = "ЙА КРЕВЕДКО";

			var att2 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First().Names.AddNew();
			att2.DA_Language = Core.SharedConstants.Languages.Russian;
			att2.DA_Descriptor = "ПЫЩЬ ПЫЩЬ";

			Factory.Save();

			var filter = new UNDGSubstanceFilterBusinessObject();
			((ModuleTextFilter)filter["Proper Shipping Name [Translations]"]).Property = "ЙА КРЕВЕДКО";
			((ModuleTextFilter)filter["Proper Shipping Name [Translations]"]).IsActive = true;

			var substanceCollection = new UNDGSubstanceCollection(Factory, filter.Filter);

			Assert("Filtered for System substances, Collection should contain Substance1", substanceCollection.Contains(att1.UNDGSubstance));
			Assert("Filtered for System substances, Collection should not contain Substance2", !substanceCollection.Contains(att2.UNDGSubstance));
		}

		public void TestIsSystem()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.UNDGSubstance))
			{
				var substance1 = Factory.New<UNDGSubstance>();
				substance1.DG_UNNO = "1111";
				substance1.DG_Variant = "a";
				substance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				substance1.DG_IsSystem = true;

				var substance2 = Factory.New<UNDGSubstance>();
				substance2.DG_UNNO = "1111";
				substance2.DG_Variant = "b";
				substance2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				substance2.DG_IsSystem = false;

				var systemFilter = module.FilterBusinessObject;
				((ModuleTextFilter)systemFilter["Is System Defined"]).Property = "System";
				((ModuleTextFilter)systemFilter["UN"]).Property = "1111";
				((ModuleTextFilter)systemFilter["Is System Defined"]).IsActive = true;
				((ModuleTextFilter)systemFilter["UN"]).IsActive = true;

				var substanceCollection = new UNDGSubstanceCollection(Factory, systemFilter.Filter);

				Assert("Filtered for System substances, Collection should contain Substance1",
					substanceCollection.Contains(substance1));
				Assert("Filtered for System substances, Collection should not contain Substance2",
					!substanceCollection.Contains(substance2));

				((ModuleTextFilter)systemFilter["Is System Defined"]).Property = "Not System";
				((ModuleTextFilter)systemFilter["UN"]).Property = "1111";
				((ModuleTextFilter)systemFilter["Is System Defined"]).IsActive = true;
				((ModuleTextFilter)systemFilter["UN"]).IsActive = true;

				substanceCollection = new UNDGSubstanceCollection(Factory, systemFilter.Filter);

				Assert("Filtered for User substances, Collection should contain Substance2",
					substanceCollection.Contains(substance2));
				Assert("Filtered for User substances, Collection should not contain Substance1",
					!substanceCollection.Contains(substance1));

				((ModuleTextFilter)systemFilter["Is System Defined"]).Property = "All";
				((ModuleTextFilter)systemFilter["UN"]).Property = "1111";
				((ModuleTextFilter)systemFilter["Is System Defined"]).IsActive = true;
				((ModuleTextFilter)systemFilter["UN"]).IsActive = true;

				substanceCollection = new UNDGSubstanceCollection(Factory, systemFilter.Filter);

				Assert("Filtered for all substances, Collection should contain Substance1",
					substanceCollection.Contains(substance1));
				Assert("Filtered for all substances, Collection should contain Substance2",
					substanceCollection.Contains(substance2));

				((ModuleTextFilter)systemFilter["Is System Defined"]).Property = "";
				((ModuleTextFilter)systemFilter["UN"]).Property = "1111";
				((ModuleTextFilter)systemFilter["Is System Defined"]).IsActive = true;
				((ModuleTextFilter)systemFilter["UN"]).IsActive = true;

				substanceCollection = new UNDGSubstanceCollection(Factory, systemFilter.Filter);

				Assert("Not filtered, Collection should contain Substance1", substanceCollection.Contains(substance1));
				Assert("Not filtered, Collection should contain Substance2", substanceCollection.Contains(substance2));
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UNDGSubstanceFilterBusinessObject();
		}

		#endregion
	}
}
