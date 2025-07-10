using System;

using Enterprise.MasterFiles.Business;

using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Freight.Business
{
	public interface IShipmentWithDocsAndCartage : IHaveInternalCartage, IShipmentProvider, IDocAddresses, IDocsAndCartageParent
	{
		[UniversalCopyRelatedEntity(CommaSeparatedSkipPropertiesNames = "JP_ParentID,JP_ParentTableCode", DisableCopyMethodLink = true)]
		JobDocsAndCartage DocsAndCartage { get; }

		event EventHandler TransportModeChanged;
		bool ShouldValidateDeliveryAndPickupCartageCoBeingSame();
		bool ShouldValidateDeliveryCoPK();

		bool RequiresOrderNumbersOnDocs();
		bool RequiresOrderTrackLink();

		JobDocAddress ConsignorDocumentaryAddress { get; }
		JobDocAddress ConsigneeDocumentaryAddress { get; }

		JobDocsAndCartageValidation PiggyBackedValidation { get; }

		string TableName { get; }

		string UniqueConsignRef { get; }
		string MasterBillNumber { get; }
		string HouseBillNumber { get; }
		OrgHeader ExportBroker { get; }
	}
}
