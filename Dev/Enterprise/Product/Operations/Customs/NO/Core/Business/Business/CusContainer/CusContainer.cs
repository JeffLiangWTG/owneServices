using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business
{
	public class CusContainer : Customs.Business.BaseCusContainer
	{
		public CusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusContainer Clone() => (CusContainer)base.Clone();

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

		public new CusContainerValidation Validation => (CusContainerValidation)base.Validation;

		protected override Customs.Business.CusContainerLookups GetNewLookups() => new CusContainerLookups(this);

		protected override Customs.Business.CusContainerValidation GetNewValidation() => new CusContainerValidation(this);

		protected override System.Type GetJobDeclarationType() => typeof(JobDeclaration);
	}
}
