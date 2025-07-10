using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationWithAccIntegrationSupport : BaseJobDeclaration
	{
		public BaseJobDeclarationWithAccIntegrationSupport(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
		{
		}

		protected internal override bool IsIntegrationWithAccountingSupported => true;
	}
}
