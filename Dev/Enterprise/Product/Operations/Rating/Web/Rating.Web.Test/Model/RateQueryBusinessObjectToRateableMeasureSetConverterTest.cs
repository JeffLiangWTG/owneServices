using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Conversion;

namespace Enterprise.Rating.Web.Test.Model
{
	public class RateQueryBusinessObjectToRateableMeasureSetConverterTest : RatingTestCase
	{
		public void TestConvert_NonJobChargesEndpoints()
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
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertContainsExactElementsInAnyOrder(expectedContainerTypePKs, measureSet.GetContainerTypePKs());

			rateQuery.ContainerTypes = null;
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals(0, measureSet.GetContainerTypePKs().Count());

			rateQuery.ContainerTypes = System.Array.Empty<ContainerType>();
			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.Costing);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals(0, measureSet.GetContainerTypePKs().Count());
		}

		public void TestConvert_Chargeable_Containerized()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP" ,
						Commodity = "HAZ",
						PackLines = new []
							{
								new JobPackLine { PackageType = "PLT", Weight = 20, WeightUnit = "OZ", Volume = 3 , VolumeUnit = "CF" },
								new JobPackLine { PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
								new JobPackLine { PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3
								new JobPackLine { },
								null
							}
					},

					new JobContainer() { ContainerTypeCWCode = "40GP" },
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			Assert(rateQueryBO.IsContainerized);

			AssertEquals(string.Empty, measureSet.GetChargeableCommodity()); // JobContainer Commodities are not the same.
			AssertEquals(2.142M, measureSet.GetActual(MeasureType.Chargeable));
			AssertEquals("M3", measureSet.GetUnit(MeasureType.Chargeable));

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer() { ContainerTypeCWCode = "20GP", Commodity = "HAZ", Unit = 5, Ownership = "" },
					new JobContainer() { ContainerTypeCWCode = "40GP", Commodity = "HAZ" },
				}
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals("HAZ", measureSet.GetChargeableCommodity());
		}

		public void TestConvert_Chargeable_NonContainerized()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "LSE";
			rateQuery.TransportMode = "AIR";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine { Commodity = "HAZ", PackageType = "PLT", Weight = 50, WeightUnit = "T", Volume = 3 , VolumeUnit = "M3" },
							new JobPackLine { Commodity = "GEN", PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3
							new JobPackLine { },
							null
						}
					}
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			Assert(!rateQueryBO.IsContainerized);

			AssertEquals(string.Empty, measureSet.GetChargeableCommodity()); // JobPackLine commodities are not the same
			AssertEquals(50020m, measureSet.GetActual(MeasureType.Chargeable)); // 50T + 10 KG + 10 KG = 50020 KG
			AssertEquals("KG", measureSet.GetUnit(MeasureType.Chargeable));

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						PackLines = new []
						{
							new JobPackLine { Commodity = "HAZ", PackageType = "PLT", Weight = 50, WeightUnit = "T", Volume = 3 , VolumeUnit = "M3" },
							new JobPackLine { Commodity = "HAZ", PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "HAZ", PackageType = "PKG", Weight = 10, Volume = 2 },
						}
					}
				}
			};

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals("HAZ", measureSet.GetChargeableCommodity());
			AssertEquals(50020m, measureSet.GetActual(MeasureType.Chargeable));
			AssertEquals("KG", measureSet.GetUnit(MeasureType.Chargeable));
		}

		public void TestConvert_Chargeable_Override()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP" ,
						Commodity = "HAZ",
						PackLines = new []
						{
							new JobPackLine { PackageType = "PLT", Weight = 20, WeightUnit = "OZ", Volume = 3 , VolumeUnit = "CF" },
							new JobPackLine { PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3
							new JobPackLine { },
							null
						}
					},
				},
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			Assert(rateQueryBO.IsContainerized);

			AssertEquals("HAZ", measureSet.GetChargeableCommodity());
			AssertEquals(2.142M, measureSet.GetActual(MeasureType.Chargeable));
			AssertEquals("M3", measureSet.GetUnit(MeasureType.Chargeable));

			rateQuery.JobInfo.ChargeableOverride = 3.16M;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals("HAZ", measureSet.GetChargeableCommodity());
			AssertEquals(3.16M, measureSet.GetActual(MeasureType.Chargeable));
			AssertEquals("M3", measureSet.GetUnit(MeasureType.Chargeable));

			rateQuery.JobInfo.ChargeableOverride = 0M;

			rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);
			AssertEquals("HAZ", measureSet.GetChargeableCommodity());
			AssertEquals(2.142M, measureSet.GetActual(MeasureType.Chargeable));
			AssertEquals("M3", measureSet.GetUnit(MeasureType.Chargeable));
		}

		public void TestConvert_Packlines_Containerized()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP" ,
						Commodity = "HAZ",
						PackLines = new []
						{
							new JobPackLine { Commodity = "GEN", PackageType = "PLT", Weight = 20, WeightUnit = "OZ", Volume = 3 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "HAZ", PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "XXX", PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3
							new JobPackLine { Commodity = "AAA" },
							null
						}
					},

					new JobContainer() { ContainerTypeCWCode = "40GP" },
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			AssertContainsExactElementsInAnyOrder(
				new decimal[] { 0.56699M, 10M, 10M, 0M, 0M },
				measureSet.GetPacklineWeights_ForTest()
			);

			AssertContainsExactElementsInAnyOrder(
				new string[] { "HAZ", "HAZ", "HAZ", "HAZ", "HAZ" },
				measureSet.GetPacklineCommodities_ForTest()
			);

			AssertContainsExactElementsInAnyOrder(
				new ZGuid[] { GP20.PK },
				measureSet.GetPacklineUniqueContainerTypePKs_ForTest()
			);
		}

		public void TestConvert_Packlines_NonContainerized()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "LCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						Commodity = "HAZ",
						PackLines = new []
						{
							new JobPackLine { Commodity = "GEN", PackageType = "PLT", Weight = 20, WeightUnit = "OZ", Volume = 3 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "HAZ", PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "XXX", PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3
							new JobPackLine { Commodity = "AAA" },
							null
						}
					},
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			AssertContainsExactElementsInAnyOrder(
				new decimal[] { 0.56699M, 10M, 10M, 0M, 0M },
				measureSet.GetPacklineWeights_ForTest()
			);

			AssertContainsExactElementsInAnyOrder(
				new string[] { "GEN", "HAZ", "XXX", "AAA", null },
				measureSet.GetPacklineCommodities_ForTest()
			);

			AssertContainsExactElementsInAnyOrder(
				new ZGuid[] { ZGuid.Empty },
				measureSet.GetPacklineUniqueContainerTypePKs_ForTest()
			);
		}

		public void TestConvert_Containers()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "FCL";
			rateQuery.TransportMode = "SEA";

			rateQuery.JobInfo = new JobInfo()
			{
				Containers = new[]
				{
					new JobContainer()
					{
						ContainerTypeCWCode = "20GP" ,
						Commodity = "HAZ",
						Unit = 3,
						PackLines = new []
						{
							new JobPackLine { Commodity = "GEN", PackageType = "PLT", Weight = 20, WeightUnit = "OZ", Volume = 3 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "HAZ", PackageType = "PKG", Weight = 10, WeightUnit = "KG", Volume = 2 , VolumeUnit = "CF" },
							new JobPackLine { Commodity = "XXX", PackageType = "PKG", Weight = 10, Volume = 2 }, // When unit is not specified, we will consider them as KG/M3. But validation won't allow api consumer to specify weight or volume without their units.
							new JobPackLine { Commodity = "AAA" },
							null
						}
					},

					new JobContainer() { ContainerTypeCWCode = "40GP" , Commodity = "GEN" },
					new JobContainer() { Unit = 0, ContainerTypeCWCode = "40HC" , Commodity = "GEN" },
				}
			};

			var rateQueryBO = new RateQueryBusinessObject(Factory, rateQuery, SourceEndpoint.JobCharges);
			var converter = new RateQueryBusinessObjectToRateableMeasureSetConverter(Factory);
			var measureSet = converter.Convert(rateQueryBO, MasterFiles.Business.AdapterType.Consolidation);

			AssertContainsExactElementsInAnyOrder(
				new[] { GP20.PK, GP40.PK, HC40.PK },
				measureSet.GetContainerTypePKs()
			);

			AssertContainsExactElementsInAnyOrder(
				new[] { "HAZ", "GEN" },
				measureSet.GetContainerUniqueCommodities()
			);

			AssertEquals((decimal)3, measureSet.GetTotalTEUForContainerType(GP20.PK.ToGuid()));
			AssertEquals(2m, GP40.RC_TEU);
			AssertEquals((decimal)2, measureSet.GetTotalTEUForContainerType(GP40.PK.ToGuid()));

			AssertContainsExactElementsInAnyOrder(
				new[] {
					(GP20.PK, "HAZ", "", 1, (decimal)20.56699, (decimal)2.141585),
					(GP20.PK, "HAZ", "", 1, (decimal)20.56699, (decimal)2.141585),
					(GP20.PK, "HAZ", "", 1, (decimal)20.56699, (decimal)2.141585),
					(GP40.PK, "GEN", "", 1, 0, 0),
					(HC40.PK, "GEN", "", 1, 0, 0)
				},
				measureSet.GetContainerListWithCalculatedWeightVolume().ToArray()
			);
		}

		RefContainer HC40
		{
			get
			{
				if (fHC40 == null)
				{
					fHC40 = base.Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "40HC");
					fHC40.RC_Description = "TWENTY FOOT GENERAL PURPOSE";
				}

				return fHC40;
			}
		}
		RefContainer fHC40;
	}
}
