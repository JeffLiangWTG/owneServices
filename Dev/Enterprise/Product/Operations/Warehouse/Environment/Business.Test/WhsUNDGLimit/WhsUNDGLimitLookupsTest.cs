using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsUNDGLimitLookupsTest : WhsBusinessObjectLookupsTestCase
	{
		#region TestWeightUnits

		public void TestWeightUnits()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.WeightUnits);
			AssertEquals(true, lookups.WeightUnits.ContainsCode("KG"));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Weight), lookups.WeightUnits);
		}

		#endregion

		#region TestCubicUnits

		public void TestVolumeUnits()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.VolumeUnits);
			AssertEquals(true, lookups.VolumeUnits.ContainsCode("M3"));
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList(OLookUpEditType.Volume), lookups.VolumeUnits);
		}

		#endregion

		#region TestUNDGCountryReferences

		public void TestUNDGCountryReferences()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var whsAddress = org.MainAddress;
			whsAddress.OA_RN_NKCountryCode = "AU";
			var currentBranch = GlbBranch.CurrentBranch;
			var whs = Helper.CreateWarehouse("TST", whsAddress, currentBranch);

			Factory.Save();

			var limit = whs.UNDGLimits.AddNew();
			var lookups = limit.Lookups.UNDGCountryReferences;
			var countryFilter = lookups.FilterBusinessObjectDefaults["Country:Property"];
			AssertEquals("Property", countryFilter.PropertyName);
			AssertEquals((ZString)"AU", countryFilter.Value);
			AssertEquals(true, countryFilter.IsRemovable);
		}

		#endregion

		#region TestUNDGClass

		public void TestUNDGClass()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var whsAddress = org.MainAddress;
			whsAddress.OA_RN_NKCountryCode = "AU";
			var currentBranch = GlbBranch.CurrentBranch;
			var whs = Helper.CreateWarehouse("TST", whsAddress, currentBranch);

			Factory.Save();

			var limit = whs.UNDGLimits.AddNew();
			var lookups = limit.Lookups.UNDGClass;
			var dgClassList = UNDGDataItemLookups.GetDGClassList(Factory);

			CombineAssertions(() =>
			{
				var codeList = dgClassList.ToList<CodeDescriptionPair>().Select(d => d.Code).ToList();
				var codes = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "Comb" };
				AssertEquals(codes.Length, lookups.Count);

				foreach (var code in codes)
				{
					AssertEquals(true, lookups.ContainsCode(code));
					AssertEquals(true, codeList.Any(c => c.StartsWith(code) || c.Equals(code, System.StringComparison.OrdinalIgnoreCase)));
				}
			});
		}

		#endregion

		#region Implementation

		protected static void AssertFilterDefaults(UNDGCountryReferenceCollection collection, IZType value, string filterName, string propertyName)
		{
			var filterDefault = collection.FilterBusinessObjectDefaults[$"{filterName}:{propertyName}"];
			AssertEquals(propertyName, filterDefault.PropertyName);
			AssertEquals(value, filterDefault.Value);
			AssertEquals(false, filterDefault.IsRemovable);
		}

		protected virtual WhsUNDGLimitLookups GetNewLookups()
		{
			return new WhsUNDGLimitLookups(Factory.New<WhsUNDGLimit>());
		}

		protected WhsUNDGLimitLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		WhsUNDGLimitLookups lookups;

		#endregion
	}
}
