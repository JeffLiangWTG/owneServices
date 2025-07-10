

namespace Enterprise.Customs.TW.Business
{
	public partial class CusContainer
	{
		#region New'd objects for type safety (typeDeciders/concrete classes must take care of instantiation

		public new CusContainer Clone()
		{
			return (CusContainer)base.Clone();
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}

		public new CusContainerLookups Lookups
		{
			get
			{
				return (CusContainerLookups)base.Lookups;
			}
		}

		public new CusContainerValidation Validation
		{
			get { return (CusContainerValidation)base.Validation; }
		}

		#endregion

		#region Implementation

		#region protected override

		protected override Customs.Business.CusContainerLookups GetNewLookups()
		{
			return new CusContainerLookups(this);
		}

		protected override Customs.Business.CusContainerValidation GetNewValidation()
		{
			return new CusContainerValidation(this);
		}

		protected override System.Type GetJobDeclarationType()
		{
			return typeof(JobDeclaration);
		}

		#endregion

		#endregion
	}
}
