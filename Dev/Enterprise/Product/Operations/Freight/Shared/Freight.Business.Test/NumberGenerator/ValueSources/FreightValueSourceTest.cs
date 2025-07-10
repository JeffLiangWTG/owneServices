using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Keys = Enterprise.Registry.Business.BillOfLadingNumberCustomisationElement.Keys;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightValueSourceTest : TestCaseWithFactory
	{
		public void TestTranshipmentIndicator()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			NumberGeneratorValueProviderCollection valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support.Setup(m => m.TranshipmentIndicator).Returns("Q");
			AssertEquals("TranshipmentIndicator", "Q", valueProviders[Keys.ContainerTranshipmentIndicator].GetValue(Generator, ""));
		}

		public void TestOriginPortIATA()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			RefUNLOCO brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			NumberGeneratorValueProviderCollection valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support.Setup(m => m.Origin).Returns(brisbane);
			AssertEquals("IATA", "BNE", valueProviders[Keys.OriginIATA].GetValue(Generator, ""));
		}

		public void TestOriginPortUNLOCO()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.PatternMatchOverrides_ForBinding.RemoveAndDeleteAll();

			var match = carrier.CreatePatternMatchOverrideForTest();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match.OO_ForeignCode = "BN";
			match.OO_LocalGuid = brisbane.PK;

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support
				.SetupSequence(m => m.Origin)
				.Returns(brisbane)
				.Returns(brisbane);
			support.SetupSequence(m => m.CarrierPrincipal)
				.Returns((OrgHeader)null)
				.Returns(carrier);

			AssertEquals("UNLOCA", "AUBNE", valueProviders[Keys.OriginUNLOCO].GetValue(Generator, ""));
			AssertEquals("UNLOCO", "BN", valueProviders[Keys.OriginUNLOCO].GetValue(Generator, ""));
		}

		public void TestDestinationPortIATA()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support.Setup(m => m.Destination).Returns(brisbane);
			AssertEquals("IATA", "BNE", valueProviders[Keys.DestinationIATA].GetValue(Generator, ""));
		}

		public void TestDestinationPortUNLOCO()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var match = carrier.CreatePatternMatchOverrideForTest();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match.OO_ForeignCode = "BN";
			match.OO_LocalGuid = brisbane.PK;

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support
				.SetupSequence(m => m.Destination)
				.Returns(brisbane)
				.Returns(brisbane);
			support
				.SetupSequence(m => m.CarrierPrincipal)
				.Returns((OrgHeader)null)
				.Returns(carrier);

			AssertEquals("UNLOCA", "AUBNE", valueProviders[Keys.DestinationUNLOCO].GetValue(Generator, ""));
			AssertEquals("UNLOCO", "BN", valueProviders[Keys.DestinationUNLOCO].GetValue(Generator, ""));
		}

		public void TestLoadPortIATA()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support.Setup(m => m.Load).Returns(brisbane);
			AssertEquals("IATA", "BNE", valueProviders[Keys.LoadIATA].GetValue(Generator, ""));
		}

		public void TestLoadPortUNLOCO()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var match = carrier.CreatePatternMatchOverrideForTest();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match.OO_ForeignCode = "BN";
			match.OO_LocalGuid = brisbane.PK;

			NumberGeneratorValueProviderCollection valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support
				.SetupSequence(m => m.Load)
				.Returns(brisbane)
				.Returns(brisbane);
			support
				.SetupSequence(m => m.CarrierPrincipal)
				.Returns((OrgHeader)null)
				.Returns(carrier);

			AssertEquals("UNLOCA", "AUBNE", valueProviders[Keys.LoadUNLOCO].GetValue(Generator, ""));
			AssertEquals("UNLOCO", "BN", valueProviders[Keys.LoadUNLOCO].GetValue(Generator, ""));
		}

		public void TestDischargePortIATA()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support.Setup(m => m.Discharge).Returns(brisbane);
			AssertEquals("IATA", "BNE", valueProviders[Keys.DischargeIATA].GetValue(Generator, ""));
		}

		public void TestDischargePortUNLOCO()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var brisbane = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");

			var carrier = Factory.NewWithValidTestData<OrgHeader>();

			var match = carrier.CreatePatternMatchOverrideForTest();
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			match.OO_ForeignCode = "BN";
			match.OO_LocalGuid = brisbane.PK;

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support
				.SetupSequence(m => m.Discharge)
				.Returns(brisbane)
				.Returns(brisbane);
			support
				.SetupSequence(m => m.CarrierPrincipal)
				.Returns((OrgHeader)null)
				.Returns(carrier);

			AssertEquals("UNLOCA", "AUBNE", valueProviders[Keys.DischargeUNLOCO].GetValue(Generator, ""));
			AssertEquals("UNLOCO", "BN", valueProviders[Keys.DischargeUNLOCO].GetValue(Generator, ""));
		}

		public void TestDirection()
		{
			var support = new Mock<IBillGenerationSupport>(MockBehavior.Strict);

			var homePort1 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			var homePort2 = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var overseasPort1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, homePort1.RL_RN_NKCountryCode));
			var overseasPort2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new ZString[] { homePort1.RL_RN_NKCountryCode, overseasPort1.RL_RN_NKCountryCode }));

			var valueProviders = new NumberGeneratorValueProviderCollection();
			valueProviders.AddRange(new FreightValueSource(support.Object));

			support
				.SetupSequence(m => m.Origin)
				.Returns(homePort1)
				.Returns(homePort1)
				.Returns(overseasPort1)
				.Returns(overseasPort1);
			support
				.SetupSequence(m => m.Destination)
				.Returns(overseasPort1)
				.Returns(homePort2)
				.Returns(homePort2)
				.Returns(overseasPort2);

			AssertEquals("E", valueProviders[Keys.Direction].GetValue(Generator, ""));
			AssertEquals("D", valueProviders[Keys.Direction].GetValue(Generator, ""));
			AssertEquals("I", valueProviders[Keys.Direction].GetValue(Generator, ""));
			AssertEquals("O", valueProviders[Keys.Direction].GetValue(Generator, ""));
		}

		#region Implementation

		NumberGenerator Generator
		{
			get
			{
				if (generator == null)
				{
					generator = new NumberGenerator();
					generator.Factory = Factory;
					generator.Context = new NumberGeneratorContext();
				}
				return generator;
			}
		}
		NumberGenerator generator;

		#endregion
	}
}
