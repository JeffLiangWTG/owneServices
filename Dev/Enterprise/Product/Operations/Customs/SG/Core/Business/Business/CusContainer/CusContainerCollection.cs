
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class CusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public CusContainerCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
			MaxCountValidationEnable(100);
		}

		public CusContainerCollection(JobDeclaration jobDeclaration, ZQuery filter)
			: base(jobDeclaration, filter)
		{
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			((JobDeclaration)Master).JE_ContainerCountInfo.RefreshBinding();
		}
	}
}
