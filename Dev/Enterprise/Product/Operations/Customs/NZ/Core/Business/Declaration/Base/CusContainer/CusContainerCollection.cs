using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	public class CusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public CusContainerCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public CusContainerCollection(JobDeclaration jobDeclaration, ZQuery filter)
			: base(jobDeclaration, filter)
		{
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (!IsValidationSuspended)
			{
				Declaration.Validation.ValidateJE_SendMCDContainerQuarantineDeclaration();
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			if (!IsValidationSuspended && Declaration.IsECIWriteoff)
			{
				Declaration.Validation.ValidatePackagesActualPackageCount();
			}
		}

		internal string GetFirstValidContainerMode()
		{
			var containerModeList = Factory.GetCachedValue<ContainerModeList>();
			return this.Cast<CusContainer>().FirstOrDefault(c => containerModeList.ContainsCode(c.CO_FCL_LCL_AIR))?.CO_FCL_LCL_AIR;
		}

		public new JobDeclaration Declaration => (JobDeclaration)Master;
	}
}
