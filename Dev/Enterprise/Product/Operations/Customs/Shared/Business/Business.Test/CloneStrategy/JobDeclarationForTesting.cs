using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationForTesting : BaseJobDeclaration
	{
		public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override bool SupportsCusPackingListCore => false;
	}
}
