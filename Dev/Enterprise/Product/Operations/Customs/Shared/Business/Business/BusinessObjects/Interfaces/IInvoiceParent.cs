
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IInvoiceParent : IDeclarationProvider
	{
		SchemaGuidColumn ForeignKeyInInvoiceToParent { get; }

		ZGuid PK { get; }
	}
}
