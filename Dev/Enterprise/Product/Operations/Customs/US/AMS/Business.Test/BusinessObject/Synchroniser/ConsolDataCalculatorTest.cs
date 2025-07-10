using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class ConsolDataCalculatorTest : Customs.Business.Testing.ConsolDataCalculatorTest
	{
		protected override Customs.Business.ConsolDataCalculator CreateNewCalculator(ForwardingConsol consol)
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.SetEnabled(false, false);
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
			return new ConsolDataCalculator(consol, header);
		}

		protected override RefUNLOCO CreatePortInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USLAX");
		}

		protected override RefUNLOCO CreatePortInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USNYC");
		}

		protected override RefUNLOCO CreatePortInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "USCHI");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
		}
	}
}
