using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ReferenceNumberFilterHelperTest : TestCaseWithFactory
	{
		public void TestGetReferenceNumberFilter()
		{
			NewReferenceNumber(Dummy1, "AU", "COC", "MUNDANE");
			NewReferenceNumber(Dummy2, "US", "COC", "MAGIC");
			NewReferenceNumber(Dummy3, "AU", "COC", "MAGIC");
			NewReferenceNumber(Dummy4, "AU", "COC", "INSANE");

			NewReferenceNumber(Dummy1, "AU", "ASL", "MAGIC");
			NewReferenceNumber(Dummy2, "AU", "ASL", "");

			int iCount = Dummy5.Numbers.Count;
			if (dummy5 != null && dummy5.Numbers != null)
			{
				foreach (ICusEntryNumber number in dummy5.Numbers)
				{
					dummy5.Numbers.RemoveAndDelete(number);
				}
			}

			Factory.Save();

			Asserter.AddFieldOfInterest("AU:COC", (s) => GetValue(s, "AU", "COC"));
			Asserter.AddFieldOfInterest("US:COC", (s) => GetValue(s, "US", "COC"));
			Asserter.AddFieldOfInterest("AU:ASL", (s) => GetValue(s, "AU", "ASL"));

			ZQuery filter;

			filter = Provider.GetReferenceNumberFilter(SQLComparisonOperator.StartsWith, "AU", "COC", "MAGIC");
			Asserter.AssertMatches("AU:COC:MAGIC*", filter, Dummy3);

			filter = Provider.GetReferenceNumberFilter(SQLComparisonOperator.StartsWith, "", "COC", "MAGIC");
			Asserter.AssertMatches("COC:MAGIC*", filter, Dummy2, Dummy3);

			filter = Provider.GetReferenceNumberFilter(SpecialComparisonOperator.IsBlank, "", "ASL", "");
			Asserter.AssertMatches("Without ASL", filter, Dummy2, Dummy3, Dummy4, Dummy5);

			filter = Provider.GetReferenceNumberFilter(SpecialComparisonOperator.IsNotBlank, "", "ASL", "");
			Asserter.AssertMatches("With ASL", filter, Dummy1);

			filter = Provider.GetReferenceNumberFilter(SQLComparisonOperator.NotEqual, "", "COC", "MAGIC");
			Asserter.AssertMatches("Without COC:MAGIC", filter, Dummy1, Dummy4, Dummy5);

			filter = Provider.GetReferenceNumberFilter(SQLComparisonOperator.NotContains, "", "COC", "ANE");
			Asserter.AssertMatches("Without COC:*ANE", filter, Dummy2, Dummy3, Dummy5);

			filter = Provider.GetReferenceNumberFilter(SQLComparisonOperator.DoesNotStartWith, "", "COC", "M");
			Asserter.AssertMatches("Without COC:M*", filter, Dummy4, Dummy5);
		}

		public void TestGetReferenceNumberFilter_FilterIsEqualReferenceNumberIsEmpty_RerurnObjectsWithEmptyReferenceNumber()
		{
			NewReferenceNumber(Dummy1, "AU", "COC", "MCLAREN");
			NewReferenceNumber(Dummy2, "AU", "COC", "");
			NewReferenceNumber(Dummy3, "AU", "COC", "");
			NewReferenceNumber(Dummy4, "AU", "COC", "MANUTD");

			Factory.Save();

			Asserter.AddFieldOfInterest("AU:COC", (s) => GetValue(s, "AU", "COC"));

			var filterToTest = Provider.GetReferenceNumberFilter(SQLComparisonOperator.Equal, "", "COC", "");
			Asserter.AssertMatches("With COC:{empty}", filterToTest, Dummy2, Dummy3);
		}

		#region Implementation

		static string GetValue(DummyBusinessObjectWithNumbers parent, string country, string type)
		{
			foreach (ICusEntryNumber number in parent.Numbers)
			{
				if (number.CE_RN_NKCountryCode == country && number.CE_EntryType == type)
				{
					return number.CE_EntryNum;
				}
			}

			return null;
		}

		static ICusEntryNumber NewReferenceNumber(DummyBusinessObjectWithNumbers parent, string countryCode, string type, string number)
		{
			ICusEntryNumber result = parent.Numbers.AddNew();
			result.CE_RN_NKCountryCode = countryCode;
			result.CE_EntryType = type;
			result.CE_EntryNum = number;
			return result;
		}

		DummyBusinessObjectWithNumbers Dummy1
		{
			get
			{
				if (dummy1 == null)
				{
					dummy1 = Factory.New<DummyBusinessObjectWithNumbers>();
					dummy1.Z0_Description = "Dummy1";
					Asserter.AddToScope(dummy1);
				}

				return dummy1;
			}
		}
		DummyBusinessObjectWithNumbers dummy1;

		DummyBusinessObjectWithNumbers Dummy2
		{
			get
			{
				if (dummy2 == null)
				{
					dummy2 = Factory.New<DummyBusinessObjectWithNumbers>();
					dummy2.Z0_Description = "Dummy2";
					Asserter.AddToScope(dummy2);
				}

				return dummy2;
			}
		}
		DummyBusinessObjectWithNumbers dummy2;

		DummyBusinessObjectWithNumbers Dummy3
		{
			get
			{
				if (dummy3 == null)
				{
					dummy3 = Factory.New<DummyBusinessObjectWithNumbers>();
					dummy3.Z0_Description = "Dummy3";
					Asserter.AddToScope(dummy3);
				}

				return dummy3;
			}
		}
		DummyBusinessObjectWithNumbers dummy3;

		DummyBusinessObjectWithNumbers Dummy4
		{
			get
			{
				if (dummy4 == null)
				{
					dummy4 = Factory.New<DummyBusinessObjectWithNumbers>();
					dummy4.Z0_Description = "Dummy4";
					Asserter.AddToScope(dummy4);
				}

				return dummy4;
			}
		}
		DummyBusinessObjectWithNumbers dummy4;

		DummyBusinessObjectWithNumbers Dummy5
		{
			get
			{
				if (dummy5 == null)
				{
					dummy5 = Factory.New<DummyBusinessObjectWithNumbers>();
					dummy5.Z0_Description = "Dummy5";
					Asserter.AddToScope(dummy5);
				}

				return dummy5;
			}
		}
		DummyBusinessObjectWithNumbers dummy5;

		FilterStripAsserter<DummyBusinessObjectWithNumbers> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<DummyBusinessObjectWithNumbers>(Factory, (d) => d.Z0_Description)); }
		}
		FilterStripAsserter<DummyBusinessObjectWithNumbers> asserter;

		ReferenceNumberFilterHelper<DummyBusinessObjectWithNumbers> Provider
		{
			get { return provider ?? (provider = new ReferenceNumberFilterHelper<DummyBusinessObjectWithNumbers>()); }
		}
		ReferenceNumberFilterHelper<DummyBusinessObjectWithNumbers> provider;

		public class DummyBusinessObjectWithNumbers : DummyBusinessObject
		{
			public DummyBusinessObjectWithNumbers(BusinessObjectFactory factory, DataRow row)
				: base(factory, row) { }

			public ICusEntryNumAdditionalReferenceCollection Numbers
			{
				get { return numbers ?? (numbers = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollection>(this)); }
			}

			ICusEntryNumAdditionalReferenceCollection numbers;

			public override string TableName => "CusEntryHeader";
		}

		#endregion
	}
}
