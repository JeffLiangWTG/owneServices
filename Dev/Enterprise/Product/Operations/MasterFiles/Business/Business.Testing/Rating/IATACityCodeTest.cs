using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(IATACityCode))]
	sealed class IATACityCodeTest : NonPersistentBusinessObjectTestCase
	{
		class IATACityCodeForTest : IATACityCode
		{
			public IATACityCodeForTest(BusinessObjectFactory factory, ZString regionCode)
				: base(factory, regionCode)
			{
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IATACityCodeForTest(Factory, "_CD");
		}

		#region Setup

		class TestUNLOCO
		{
			TestUNLOCO(string purePortCode, string countryCode, bool isActive = true)
				: this(purePortCode, "", countryCode, isActive)
			{
			}

			TestUNLOCO(string purePortCode, string regionCode, string countryCode, bool isActive = true)
			{
				PortCode = countryCode + purePortCode;
				PortName = purePortCode + "Name";
				CountryCode = countryCode;
				IATARegionCode = regionCode;
				IsActive = isActive;
			}

			public string PortCode { get; }
			public string PortName { get; }
			public string CountryCode { get; }
			public string IATARegionCode { get; }
			public bool IsActive { get; }

			public static TestUNLOCO[] TestUNLOCOs => new[]
			{
				new TestUNLOCO("P01", "QQA", "C1"),
				new TestUNLOCO("P02", "QQB", "C2"),
				new TestUNLOCO("P03", "QQB", "C2", false),
				new TestUNLOCO("P04",        "C3"),
				new TestUNLOCO("P05", "QQC", "C3"),
				new TestUNLOCO("P06", "QQC", "C3"),
				new TestUNLOCO("P07", "QQC", "C3"),
				new TestUNLOCO("P08",        "C3"),
				new TestUNLOCO("P09", "QQD", "C3"),
				new TestUNLOCO("P10", "QQD", "C3"),
				new TestUNLOCO("P11", "QQE", "C4", false),
				new TestUNLOCO("P12",        "C5"),
				new TestUNLOCO("P13",        "C5", false),
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			var countryCodes = TestUNLOCO.TestUNLOCOs
				.Select(x => x.CountryCode)
				.Where(x => x?.Length == 2)
				.Distinct();

			foreach (var countryCode in countryCodes)
			{
				var newCountry = Factory.New<RefCountry>();
				newCountry.RN_Code = countryCode;
			}

			foreach (var testUNLOCO in TestUNLOCO.TestUNLOCOs)
			{
				var newPort = Factory.New<RefUNLOCO>();
				newPort.RL_Code = testUNLOCO.PortCode;
				newPort.RL_PortName = testUNLOCO.PortName;
				newPort.RL_RN_NKCountryCode = testUNLOCO.CountryCode;
				newPort.RL_IATARegionCode = testUNLOCO.IATARegionCode;
				newPort.RL_IsActive = testUNLOCO.IsActive;
			}

			Factory.Save();
		}

		#endregion

		public void TestGetValidOrDefault_ShouldReturnAnInstanceFromCacheIfExistsForSuchCode()
		{
			var code1 = IATACityCode.GetValidOrDefault(Factory, "IEV");
			var code2 = IATACityCode.GetValidOrDefault(Factory, "IEV");
			var code3 = IATACityCode.GetValidOrDefault(Factory, "SYD");

			// Precondition
			AssertNotNull(code1);
			AssertNotNull(code2);
			AssertNotNull(code3);

			// Test
			AssertEquals(true, ReferenceEquals(code1, code2));
			AssertEquals(false, ReferenceEquals(code1, code3));
			AssertEquals(false, ReferenceEquals(code2, code3));
		}

		public void TestIATACityCodeProperties()
		{
			AssertIATACityCode("", false, false);
			AssertIATACityCode("C3", false, false);
			AssertIATACityCode("C2P02", false, false);
			AssertIATACityCode("QQA", true, false, 1, "C1P01");
			AssertIATACityCode("R~~", false, false);
			AssertIATACityCode("R05", false, false);
			AssertIATACityCode("QQA", true, false, 1, "C1P01");
			AssertIATACityCode("QQA", true, false, 1, "C1P01");
			AssertIATACityCode("QQB", true, true, 2);
			AssertIATACityCode("QQC", true, true, 3);
			AssertIATACityCode("QQD", true, true, 2);
		}

		void AssertIATACityCode(ZString regionCode, bool isValid, bool hasMoreThanOnePort, int numberOfPorts = 0, string portCode = "")
		{
			var region = new IATACityCodeForTest(Factory, regionCode);

			AssertNotNull(region.Code);
			AssertNotNull(region.RelatedPorts);

			AssertEquals(isValid, region.IsValid);
			AssertEquals(hasMoreThanOnePort, region.HasMoreThanOnePort);

			AssertEquals(numberOfPorts, region.RelatedPorts.Count());

			if (string.IsNullOrEmpty(portCode))
			{
				AssertNull(region.UNLOCO);
			}
			else
			{
				AssertEquals(portCode, region.UNLOCO?.Code);
			}

			var relatedPorts = IATACityCode.GetRelatedPorts(Factory, regionCode);

			AssertNotNull(relatedPorts);
			AssertEquals(numberOfPorts, relatedPorts.Length);

			var validRegionOrNull = IATACityCode.GetValidOrDefault(Factory, regionCode);

			if (isValid)
			{
				AssertNotNull(validRegionOrNull);
				AssertEquals(numberOfPorts, validRegionOrNull.RelatedPorts.Count());
			}
			else
			{
				AssertNull(validRegionOrNull);
			}
		}
	}
}
