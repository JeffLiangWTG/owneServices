using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
	{
		public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
			: base(factory, companyPkToFilterOn)
		{
		}

		public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

		public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();

		protected override bool AllowNewCore => false;
	}
}
