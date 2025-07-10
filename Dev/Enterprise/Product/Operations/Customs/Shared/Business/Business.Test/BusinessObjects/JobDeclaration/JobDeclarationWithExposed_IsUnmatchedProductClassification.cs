using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationWithExposed_IsUnmatchedProductClassification : BaseJobDeclaration
	{
		public JobDeclarationWithExposed_IsUnmatchedProductClassification(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsUnmatchedProductClassification_Exposed(BaseJobComInvoiceLine line) => IsUnmatchedProductClassification(line);
	}
}
