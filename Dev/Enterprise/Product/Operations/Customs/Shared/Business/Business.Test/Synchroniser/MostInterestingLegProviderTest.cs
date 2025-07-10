using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MostInterestingLegProviderTest : TestCaseWithFactory
	{
		public void TestInboundAndOutboundLegConsideringCustomsJurisdiction()
		{
			var consol = Factory.New<ForwardingConsol>();
			var leg1 = consol.Transports.AddNew("DEFRA", "USEWR");
			leg1.JW_VoyageFlight = "UA961";
			var leg2 = consol.Transports.AddNew("USEWR", "PRSJU");
			leg2.JW_VoyageFlight = "UA1508";

			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			var provider = new MostInterestingLegProvider(declaration);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var inbondLeg = provider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(inbondLeg);
				AssertEquals("UA961", ((Transport)inbondLeg).JW_VoyageFlight);
				AssertEquals("DEFRA", inbondLeg.Load);
				AssertEquals("USEWR", inbondLeg.Discharge);

				var outbondLeg = provider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(outbondLeg);
				AssertEquals("UA961", ((Transport)outbondLeg).JW_VoyageFlight);
				AssertEquals("DEFRA", outbondLeg.Load);
				AssertEquals("USEWR", outbondLeg.Discharge);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var inbondLeg = provider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(inbondLeg);
				AssertEquals("UA961", ((Transport)inbondLeg).JW_VoyageFlight);
				AssertEquals("DEFRA", inbondLeg.Load);
				AssertEquals("USEWR", inbondLeg.Discharge);

				var outbondLeg = provider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(outbondLeg);
				AssertEquals("UA961", ((Transport)outbondLeg).JW_VoyageFlight);
				AssertEquals("DEFRA", outbondLeg.Load);
				AssertEquals("USEWR", outbondLeg.Discharge);
			}
		}

		public void TestInboundAndOutboundLegConsideringCustomsJurisdictionOrEU()
		{
			var consol = Factory.New<ForwardingConsol>();
			var leg1 = consol.Transports.AddNew("USEWR", "NLRTM");
			leg1.JW_Vessel = "BIGSHIP";
			var leg2 = consol.Transports.AddNew("USEWR", "PRSJU");
			leg2.JW_VoyageFlight = "UA1508";

			var shipment = consol.Shipments.AddNew();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;

			var provider = new MostInterestingLegProvider(declaration);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var inbondLeg = provider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(inbondLeg);
				AssertEquals("BIGSHIP", ((Transport)inbondLeg).JW_Vessel);
				AssertEquals("USEWR", inbondLeg.Load);
				AssertEquals("NLRTM", inbondLeg.Discharge);

				var outbondLeg = provider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(outbondLeg);
				AssertEquals("BIGSHIP", ((Transport)outbondLeg).JW_Vessel);
				AssertEquals("USEWR", outbondLeg.Load);
				AssertEquals("NLRTM", outbondLeg.Discharge);
			}
		}

		[TestDate(2016, 2, 2)]
		public void TestInboundAndOutboundLegBetweenUSAndPR()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				consol.JK_RL_NKLoadPort = "USCHI";
				consol.JK_RL_NKDischargePort = "PRSJU";

				var transport = consol.Transports[0];
				transport.JW_VoyageFlight = "A01";

				var shipment = consol.Shipments.AddNew();
				declaration.JE_JS = shipment.PK;

				var provider = new MostInterestingLegProvider(declaration);
				var inbondLeg = provider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(inbondLeg);
				AssertEquals("A01", ((Transport)inbondLeg).JW_VoyageFlight);
				AssertEquals("USCHI", inbondLeg.Load);
				AssertEquals("PRSJU", inbondLeg.Discharge);

				var outbondLeg = provider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(outbondLeg);
				AssertEquals("A01", ((Transport)outbondLeg).JW_VoyageFlight);
				AssertEquals("USCHI", outbondLeg.Load);
				AssertEquals("PRSJU", outbondLeg.Discharge);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				consol.JK_RL_NKLoadPort = "PRSJU";
				consol.JK_RL_NKDischargePort = "USCHI";

				var transport = consol.Transports[0];
				transport.JW_VoyageFlight = "B01";

				var shipment = consol.Shipments.AddNew();
				declaration.JE_JS = shipment.PK;

				var provider = new MostInterestingLegProvider(declaration);
				var inbondLeg = provider.GetInboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(inbondLeg);
				AssertEquals("B01", ((Transport)inbondLeg).JW_VoyageFlight);
				AssertEquals("PRSJU", inbondLeg.Load);
				AssertEquals("USCHI", inbondLeg.Discharge);

				var outbondLeg = provider.GetOutboundLeg(new TypedEnumerable<IMovementLeg>(consol.Transports));
				AssertNotNull(outbondLeg);
				AssertEquals("B01", ((Transport)outbondLeg).JW_VoyageFlight);
				AssertEquals("PRSJU", outbondLeg.Load);
				AssertEquals("USCHI", outbondLeg.Discharge);
			}
		}
	}
}
