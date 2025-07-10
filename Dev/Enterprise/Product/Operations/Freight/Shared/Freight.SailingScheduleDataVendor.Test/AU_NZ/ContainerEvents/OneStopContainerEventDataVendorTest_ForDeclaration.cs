using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerEventDataVendorTest_ForDeclaration : OneStopContainerEventDataVendorTest
	{
		protected override CommonContainer NewContainer()
		{
			BusinessObjectCollection containers = (BusinessObjectCollection)Declaration["CusContainers"];
			BusinessObject customsContainer = containers.AddNew();
			customsContainer[CusContainerSchema.CO_FCL_LCL_AIR] = Core.Constants.ContainerModes.FCL;

			CommonContainer container = (CommonContainer)customsContainer["JobContainer"];
			return container;
		}

		protected override void SetContainerNumber(CommonContainer container, ZString containerNum)
		{
			container.JC_ContainerNum = containerNum;
			ZQuery query = new ZQuery(CusContainerSchema.CO_JC, container.PK);
			BusinessObject customsContainer = (BusinessObject)container.Factory.LoadTop1<Enterprise.Integration.Customs.Shared.IBaseCusContainer>(query);
			customsContainer[CusContainerSchema.CO_ContainerNumber] = containerNum;
		}

		protected override void SetETD(ZDateTime etd)
		{
			Declaration[JobDeclarationSchema.JE_DateAtOrigin] = etd;
		}

		protected override void SetETA(ZDateTime eta)
		{
			Declaration[JobDeclarationSchema.JE_DateAtFinalDestination] = eta;
		}

		BusinessObject Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
					declaration[JobDeclarationSchema.JE_RL_NKOrigin] = "AUSYD";
					declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = "NZAKL";
				}
				return declaration;
			}
		}
		BusinessObject declaration;
	}
}
