using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerInformationProcessorTest_ForConsol : OneStopContainerInformationProcessor_BaseTest
	{
		protected override CommonContainer NewContainer()
		{
			return Consol.Containers.AddNew();
		}

		protected override string JobNumber
		{
			get { return Consol.JK_UniqueConsignRef; }
		}

		protected override string PropagatedReference => "Propagated: All Consol Containers|FAC=CTO|LOC=AUMEL";

		protected override void SetVesselName(ZString vesselName)
		{
			Consol.Transports.MostInterestingTransport.JW_Vessel = vesselName;
		}

		protected override void SetVoyage(ZString voyage)
		{
			Consol.Transports.MostInterestingTransport.JW_VoyageFlight = voyage;
		}

		CommonConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
					consol.JK_RL_NKDischargePort = "AUMEL";
					consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
				}
				return consol;
			}
		}
		CommonConsol consol;

		protected override void SetContainerDischargePort(string portCode)
		{
			consol.JK_RL_NKDischargePort = portCode;
		}

		protected override void SetContainerLoadPort(string portCode)
		{
			consol.JK_RL_NKLoadPort = portCode;
		}

		protected override void SetConsigneeConsignorsIdentical(CommonContainer container)
		{
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BR1";
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "BR2";
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG123";
			org1.OH_FullName = "Organization 123";
			org1.CompanyData.OB_GB_ControllingBranch = branch1.PK;

			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ZUB987";
			org2.OH_FullName = "Organization 987";
			org2.CompanyData.OB_GB_ControllingBranch = branch2.PK;

			CommonShipment shipment1 = Consol.Shipments.AddNew();
			shipment1.ConsigneePK = org1.PK;
			shipment1.ConsignorPK = org2.PK;
			shipment1.OuterPackLines.AddNew().SetContainer(container.PK);

			CommonShipment shipment2 = Consol.Shipments.AddNew();
			shipment2.ConsigneePK = org1.PK;
			shipment2.ConsignorPK = org2.PK;
			shipment2.OuterPackLines.AddNew().SetContainer(container.PK);
		}
	}
}
