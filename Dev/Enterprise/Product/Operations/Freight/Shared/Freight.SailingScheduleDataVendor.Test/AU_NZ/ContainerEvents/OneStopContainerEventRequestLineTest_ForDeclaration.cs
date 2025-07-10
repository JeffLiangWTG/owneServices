using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class OneStopContainerEventRequestLineTest_ForDeclaration : OneStopContainerEventRequestLineTest
	{
		protected override CommonContainer NewContainer()
		{
			BusinessObjectCollection containers = (BusinessObjectCollection)Declaration["CusContainers"];
			BusinessObject customsContainer = containers.AddNew();
			customsContainer[CusContainerSchema.CO_FCL_LCL_AIR] = Core.Constants.ContainerModes.FCL;
			return (CommonContainer)customsContainer["JobContainer"];
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
				}
				return declaration;
			}
		}
		BusinessObject declaration;
	}
}
