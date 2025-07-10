using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusContainer : EU.Business.Declaration.CusContainer, Integration.Customs.TR.ICusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

		protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);
	}
}
