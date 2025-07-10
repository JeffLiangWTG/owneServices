using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging
{
	public interface IBCDConsignment : IConsignment
	{
		ZString AssociatedTransportDocumentId { get; }

		ZDecimal BoardedQuantity { get; }

		ZString TransportContractDocumentId { get; }

		ZInt TotalPackageQuantity { get; }

		IEnumerable<IGovernmentProcedure> GovernmentProcedures { get; }

		IPackaging Packaging { get; }

		ZDecimal InvoiceAmount { get; }
	}
}
