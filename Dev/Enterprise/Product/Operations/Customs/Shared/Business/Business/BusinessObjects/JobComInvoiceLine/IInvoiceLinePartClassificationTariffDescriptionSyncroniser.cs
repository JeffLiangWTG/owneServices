using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IInvoiceLinePartClassificationTariffDescriptionSyncroniser
	{
		bool IsExtendedCommercialDescriptionEnabled { get; }

		ZString ExtraInfoForClassification { get; set; }

		ZString ClassificationDescription { get; }

		ZString TariffDescription { get; }

		ZString PartDescription { get; }

		ZString PartExtendedCommercialDescription { get; }

		BaseJobDeclaration Declaration { get; }

		OrgSupplierPart Part { get; }

		BaseCusClassification Classification { get; }

		ZString Description { get; set; }

		ZPropertyInfo DescriptionInfo { get; }
	}
}
