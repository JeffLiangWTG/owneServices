using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationSupportsPackingListForTesting : BaseJobDeclaration
	{
		public JobDeclarationSupportsPackingListForTesting(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override bool SupportsCusPackingListCore => true;
	}
}
