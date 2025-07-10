using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Web.Test.Model
{
	[TestedType(typeof(RateQueryBusinessObject))]
	public class RateQueryBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public DateTimeOffset DateTimeOffSet { get; private set; }

		protected override BusinessObject GetNewBusinessObject()
		{
			var rateQuery = new RateQuery()
			{
				Origin = new Location() { Type = Location.Types.UNLOCO, Value = "AUSYD" },
				Destination = new Location() { Type = Location.Types.UNLOCO, Value = "USLAX" },
				TransportMode = "SEA",
				ContainerMode = "FCL",
				EffectiveDate = DateTime.Now
			};

			return new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
		}

		public void TestCommodities_NonJobCharges()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAA";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBB";

			var commodity3 = Factory.New<RefCommodityCode>();
			commodity3.RH_Code = "CCC";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "AAA" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "BBB" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "XXX" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			var expectedCommodityPKs = new[] { commodity1.PK, commodity2.PK };
			AssertContainsExactElementsInAnyOrder(expectedCommodityPKs, rateQueryBO.Commodities.Values.Select(c => c.PK));
		}

		public void TestCommodities_JobCharges()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAA";

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBB";

			var commodity3 = Factory.New<RefCommodityCode>();
			commodity3.RH_Code = "CCC";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ContainerMode = "FCL";
			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "AAA" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(0, rateQueryBO.Commodities.Values.Select(c => c.PK).Count());

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer() { Commodity = "AAA" },
					new JobContainer() { Commodity = "BBB" },
					new JobContainer() { Commodity = "XXX" },
					new JobContainer() { Commodity = null },
					new JobContainer() { Commodity = "" },
					null
				}
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			Assert(rateQueryBO.IsContainerized);
			var expectedCommodityPKs = new[] { commodity1.PK, commodity2.PK };
			AssertContainsExactElementsInAnyOrder(expectedCommodityPKs, rateQueryBO.Commodities.Values.Select(c => c.PK));

			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "ULD";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			Assert(rateQueryBO.IsContainerized);
			expectedCommodityPKs = new[] { commodity1.PK, commodity2.PK };
			AssertContainsExactElementsInAnyOrder(expectedCommodityPKs, rateQueryBO.Commodities.Values.Select(c => c.PK));

			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "LCL";
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			Assert(!rateQueryBO.IsContainerized);
			AssertEquals(0, rateQueryBO.Commodities.Values.Select(c => c.PK).Count()); // When Job is not containerized, we don't use Container's commodity info

			rateQuery.ContainerMode = "FCL";
			rateQuery.JobInfo = new JobInfo()
			{
				Containers = null
			};
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			Assert(rateQueryBO.IsContainerized);
			AssertEquals(0, rateQueryBO.Commodities.Values.Select(c => c.PK).Count());

			rateQuery.ContainerMode = "LCL";
			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer() { Commodity = "AAA" },
					new JobContainer()
					{
						Commodity = "CCC",
						PackLines = new []
						{
							new JobPackLine { Commodity = "BBB" },
							new JobPackLine { Commodity = "" },
							new JobPackLine { Commodity = null },
						}
					},
					new JobContainer() { Commodity = "XXX" },
					new JobContainer() { Commodity = null },
					new JobContainer() { Commodity = "" },
					null
				}
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			Assert(!rateQueryBO.IsContainerized);
			expectedCommodityPKs = new[] { commodity2.PK };
			AssertContainsExactElementsInAnyOrder(expectedCommodityPKs, rateQueryBO.Commodities.Values.Select(c => c.PK));
		}

		public void TestUniversalCommodityGroups()
		{
			var commodity1 = Factory.New<RefCommodityCode>();
			commodity1.RH_Code = "AAA";
			commodity1.RH_IsHazardous = true;

			var commodity2 = Factory.New<RefCommodityCode>();
			commodity2.RH_Code = "BBB";
			commodity2.RH_IsPerishable = true;

			var commodity3 = Factory.New<RefCommodityCode>();
			commodity3.RH_Code = "CCC";
			commodity3.RH_IsTimber = true;

			var commodity4 = Factory.New<RefCommodityCode>();
			commodity4.RH_Code = "DDD";
			commodity4.RH_IsFlammable = true;

			var commodity5 = Factory.New<RefCommodityCode>();
			commodity5.RH_Code = "EEE";
			commodity5.RH_ContainerVentRequired = true;

			var commodity6 = Factory.New<RefCommodityCode>();
			commodity6.RH_Code = "FFF";
			commodity6.RH_UniversalCommodityGroup = "UUU";

			var commodity7 = Factory.New<RefCommodityCode>();
			commodity7.RH_Code = "YYY";

			var commodity8 = Factory.New<RefCommodityCode>();
			commodity8.RH_Code = "ZZZ";

			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Commodities = new[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "AAA" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "BBB" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "CCC" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "DDD" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "EEE" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "FFF" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "YYY" },
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "XXX" },
				new CommodityInfo() { Type = CommodityInfo.Types.UniversalCommodityGroup, Value = "UCG" }
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			var expectedUniversalCommodityGroups = new[]
			{
				RefCommodityCode.HAZD,
				RefCommodityCode.PERS ,
				RefCommodityCode.TIMB ,
				RefCommodityCode.FLAM ,
				RefCommodityCode.CNVT ,
				"UUU",
				"UCG"
			};

			AssertContainsExactElementsInAnyOrder(expectedUniversalCommodityGroups, rateQueryBO.UniversalCommodityGroups);
		}

		public void TestUniversalCarrierServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.CarrierServiceLevels = new[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise,  Value = "CWC" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "ABC" },
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "XYZ" },
				null,
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.Universal, Value = "" },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			AssertContainsExactElementsInAnyOrder(
				new[] { "ABC", "XYZ" },
				rateQueryBO.UniversalCarrierServiceLevels
			);

			rateQuery.CarrierServiceLevels = null;
			AssertEquals(0, rateQueryBO.UniversalCarrierServiceLevels.Count());
		}

		public void TestNamedAccounts()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.NamedAccounts = null;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			AssertEquals(0, rateQueryBO.NamedAccounts.Count());

			rateQuery.NamedAccounts = new[]
			{
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "CWC" },
				new NamedAccount() { Type = null, Value = "NAC01" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = null },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC02" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC03" },
			};

			AssertContainsExactElementsInAnyOrder(
				new[] { "NAC02", "NAC03" },
				rateQueryBO.NamedAccounts
			);
		}

		public void TestContainerTypes_NonJobCharges()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			rateQuery.ContainerTypes = new[]
			{
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "42G0" },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "ABCD" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "ABCD" },
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "" },
				new ContainerType() { Type = ContainerType.Types.ISO, Value = "" },
				new ContainerType() { },
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			var expectedContainerTypePKs = new[] { GP20.PK, GP40.PK };
			AssertContainsExactElementsInAnyOrder(
				expectedContainerTypePKs,
				rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK)
			);

			rateQuery.ContainerTypes = null;
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			AssertEquals(0, rateQueryBO.ContainerTypes.Count);

			rateQuery.ContainerTypes = Array.Empty<ContainerType>();
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			AssertEquals(0, rateQueryBO.ContainerTypes.Count);
		}

		public void TestContainerTypes_JobCharges()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.ContainerTypes = new[]
			{
				new ContainerType() { Type = ContainerType.Types.CargoWise, Value = "20GP" },
			};

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "40GP",
					}
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);

			var expectedContainerTypePKs = new[] { GP40.PK };
			AssertContainsExactElementsInAnyOrder(
				expectedContainerTypePKs,
				rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK)
			);

			rateQuery.ContainerTypes = null;
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertContainsExactElementsInAnyOrder(
				expectedContainerTypePKs,
				rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK)
			);

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer() { ContainerTypeCWCode = "40GP" },
					new JobContainer() { ContainerTypeCWCode = "20GP" },
					new JobContainer() { ContainerTypeCWCode = "" }
				}
			};

			expectedContainerTypePKs = new[] { GP20.PK, GP40.PK };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertContainsExactElementsInAnyOrder(
				expectedContainerTypePKs,
				rateQueryBO.ContainerTypes.Values.Select(ct => ct.PK)
			);
		}

		public void TestGoodsValueAndCurrency()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.GoodsValue = 18.9M;
			rateQuery.JobInfo.GoodsValueCurrency = "AUD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(18.9M), rateQueryBO.GoodsValue);
			AssertEquals("AUD", rateQueryBO.GoodsValueCurrency.Code);

			rateQuery.JobInfo.GoodsValue = 0;
			rateQuery.JobInfo.GoodsValueCurrency = "USD";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.GoodsValue);
			AssertEquals("USD", rateQueryBO.GoodsValueCurrency.Code);

			rateQuery.JobInfo = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.GoodsValue);
			AssertNull(rateQueryBO.GoodsValueCurrency);

			rateQuery.JobInfo = new JobInfo()
			{
				GoodsValue = null,
				GoodsValueCurrency = "AUD",
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.GoodsValue);
			AssertEquals("AUD", rateQueryBO.GoodsValueCurrency.Code);
		}

		public void TestInsuranceValueAndCurrency()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.InsuranceValue = 18.9M;
			rateQuery.JobInfo.InsuranceValueCurrency = "AUD";

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(18.9M), rateQueryBO.InsuranceValue);
			AssertEquals("AUD", rateQueryBO.InsuranceValueCurrency.Code);

			rateQuery.JobInfo.InsuranceValue = 0;
			rateQuery.JobInfo.InsuranceValueCurrency = "USD";

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.InsuranceValue);
			AssertEquals("USD", rateQueryBO.InsuranceValueCurrency.Code);

			rateQuery.JobInfo = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.InsuranceValue);
			AssertNull(rateQueryBO.InsuranceValueCurrency);

			rateQuery.JobInfo = new JobInfo()
			{
				InsuranceValue = null,
				InsuranceValueCurrency = "AUD",
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.InsuranceValue);
			AssertEquals("AUD", rateQueryBO.InsuranceValueCurrency.Code);
		}

		public void TestCustomsValue()
		{
			GP20.RC_ISOType = "22G0";
			GP40.RC_ISOType = "42G0";

			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo.CustomsValue = 18.9M;

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(18.9M), rateQueryBO.CustomsValue);

			rateQuery.JobInfo.CustomsValue = 0;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.CustomsValue);

			rateQuery.JobInfo = null;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.CustomsValue);

			rateQuery.JobInfo = new JobInfo()
			{
				CustomsValue = null,
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			AssertEquals(new ZDecimal(0), rateQueryBO.CustomsValue);
		}

		public void TestPickupAddress_Override()
		{
			TestAddressOverride(
				rateQuery => rateQuery.PickupCity = "Sydney",
				rateQueryBizO => rateQueryBizO.PickupAddress,
				expectedCity: "Sydney");

			TestAddressOverride(
				rateQuery => rateQuery.PickupPostcode = "2015",
				rateQueryBizO => rateQueryBizO.PickupAddress,
				expectedPostcode: "2015");

			TestAddressOverride(
				rateQuery =>
				{
					rateQuery.PickupCity = "Melbourne";
					rateQuery.PickupPostcode = "3015";
				},
				rateQueryBizO => rateQueryBizO.PickupAddress,
				expectedCity: "Melbourne",
				expectedPostcode: "3015");
		}

		public void TestDeliveryAddress_Override()
		{
			TestAddressOverride(
				rateQuery => rateQuery.DeliveryCity = "Sydney",
				rateQueryBizO => rateQueryBizO.DeliveryAddress,
				expectedCity: "Sydney");

			TestAddressOverride(
				rateQuery => rateQuery.DeliveryPostcode = "2015",
				rateQueryBizO => rateQueryBizO.DeliveryAddress,
				expectedPostcode: "2015");

			TestAddressOverride(
				rateQuery =>
				{
					rateQuery.DeliveryCity = "Melbourne";
					rateQuery.DeliveryPostcode = "3015";
				},
				rateQueryBizO => rateQueryBizO.DeliveryAddress,
				expectedCity: "Melbourne",
				expectedPostcode: "3015");
		}

		void TestAddressOverride(Action<RateQuery> queryAddressSetter, Func<RateQueryBusinessObject, IDocAddress> addressGetter, string expectedCity = "", string expectedPostcode = "")
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			queryAddressSetter.Invoke(rateQuery);

			var address = addressGetter.Invoke(new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges));

			AssertNotNull(address);
			AssertEquals(expectedCity, address.E2_City);
			AssertEquals(expectedPostcode, address.E2_Postcode);
			Assert("The address is overridden and the org should be the MISC org.", ((OrgHeader)address.Organisation).IsMiscellaneous);
		}

		public void TestCarriers()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "ORG3";
			Factory.Save();

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();

			// Test null carriers
			rateQuery.Carriers = null;
			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			AssertNull(rateQueryBO.Carrier);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);

			// Test empty Carriers
			rateQuery.Carriers = Array.Empty<Organisation>();
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			AssertNull(rateQueryBO.Carrier);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);

			// Test single item Carriers
			rateQuery.Carriers = new[] { new Organisation { CWCode = "ORG1" } };
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			AssertEquals("ORG1", rateQueryBO.Carrier.OH_Code);
			AssertEquals(0, rateQueryBO.PossibleCarriers.Count);

			// Test multi-item Carriers
			rateQuery.Carriers = new[]
			{
				new Organisation { CWCode = "ORG1" },
				new Organisation { CWCode = "ORG2" },
				new Organisation { CWCode = "ORG3" },
			};
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);

			AssertEquals("ORG1", rateQueryBO.Carrier.OH_Code);
			AssertContainsExactElementsInAnyOrder(
				new ZString[] { "ORG2", "ORG3" },
				rateQueryBO.PossibleCarriers.Select(x => x.OH_Code)
			);
		}

		#region Implementation
		RefContainer GP20
		{
			get
			{
				if (fGP20 == null)
				{
					fGP20 = base.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
					fGP20.RC_Description = "TWENTY FOOT GENERAL PURPOSE";
				}

				return fGP20;
			}
		}
		RefContainer fGP20;

		RefContainer GP40
		{
			get
			{
				if (fGP40 == null)
				{
					fGP40 = base.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
				}

				return fGP40;
			}
		}
		RefContainer fGP40;
		#endregion
	}
}
