using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerInformationProcessorTest_ForDeclaration : OneStopContainerInformationProcessor_BaseTest
	{
		protected override CommonContainer NewContainer()
		{
			BusinessObjectCollection cusContainers = (BusinessObjectCollection)Declaration["CusContainers"];
			return (CommonContainer)cusContainers.AddNew()["JobContainer"];
		}

		protected override string JobNumber
		{
			get { return Declaration[JobDeclarationSchema.JE_DeclarationReference].ToString(); }
		}

		protected override string PropagatedReference => "Propagated: All Declaration Containers|FAC=CTO|LOC=AUMEL";

		protected override void SetVesselName(ZString vesselName)
		{
			Declaration[JobDeclarationSchema.JE_VesselName] = vesselName;
		}

		protected override void SetVoyage(ZString voyage)
		{
			Declaration[JobDeclarationSchema.JE_VoyageFlightNo] = voyage;
		}

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "AUMEL";
				}
				return declaration;
			}
		}
		BusinessObject declaration;

		protected override void SetContainerDischargePort(string portCode)
		{
			Declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = portCode;
		}

		protected override void SetContainerLoadPort(string portCode)
		{
			Declaration[JobDeclarationSchema.JE_RL_NKOrigin] = portCode;
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

			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_RL_NKDischargePort = (ZString)Declaration[JobDeclarationSchema.JE_RL_NKFinalDestination];
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.ConsigneePK = org1.PK;
			shipment1.ConsignorPK = org2.PK;
			shipment1.OuterPackLines.AddNew().SetContainer(container.PK);

			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.ConsigneePK = org1.PK;
			shipment2.ConsignorPK = org2.PK;
			shipment2.OuterPackLines.AddNew().SetContainer(container.PK);
		}
	}
}
