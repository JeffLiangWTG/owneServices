using System;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eTail.Business
{
	public abstract class ConvertToStandAloneDeclarationResponse : IConvertToStandAloneDeclarationResponse
	{
		protected ConvertToStandAloneDeclarationResponse(Guid convertedStandAloneDeclaration)
		{
			ConvertedStandAloneDeclaration = convertedStandAloneDeclaration;
		}

		protected ConvertToStandAloneDeclarationResponse(string conversionFailureReason)
		{
			ConversionFailureReason = conversionFailureReason;
		}

		public bool ConversionSucceeded => string.IsNullOrEmpty(ConversionFailureReason);

		public string ConversionFailureReason { get; }

		public Guid ConvertedStandAloneDeclaration { get; }

		public static class ErrorMessages
		{
			public static MultilingualString ConsignmentNotAttachedToShipment => ResString.GetMultilingualString("76f81b81-76fc-4eb7-b6e1-584a833b7221", "A Stand Alone Declaration cannot be created for a consignment without an attached shipment");
			public static MultilingualString ConsignmentMissingTransportDetails => ResString.GetMultilingualString("93b9bfb3-d316-4a5a-a6e5-39e7ea415fd9", "A Stand Alone Declaration cannot be created for a Consignment without transport details. Attach a Shipment to a Consolidation to create the Stand Alone Declaration.");
			public static MultilingualString ConsignmentHasClearedImportCustomsStatus => ResString.GetMultilingualString("1ce613c3-6b6f-4a78-a105-1c6b229e8046", "A Stand Alone Declaration cannot be created for a Consignment with import clearance status as CLEAR");
			public static MultilingualString ConsignmentHasClearedExportCustomsStatus => ResString.GetMultilingualString("687135cc-50cc-4b4c-8023-71fdc0c24264", "A Stand Alone Declaration cannot be created for a Consignment with export clearance status as CLEAR");
			public static MultilingualString ShipmentDestinationNotSupported => ResString.GetMultilingualString("372aa616-f164-4615-bb48-52fcdf53caf1", "Stand Alone Declaration creation is not currently supported for this shipment destination.");
			public static MultilingualString ConsignmentHasExistingDeclaration => ResString.GetMultilingualString("45a6a562-5164-478a-a9ec-0212b490216c", "A Stand Alone Declaration cannot be created for a Consignment that already has an existing Declaration");
			public static MultilingualString ConsignmentNotFound => ResString.GetMultilingualString("a0cc8b54-f5a6-472d-a7af-8a5028e21e87", "A Stand Alone Declaration cannot be created as a matching Consignment was not found. Please make sure the Consignment is saved to the database.");
			public static MultilingualString UnexpectedError => ResString.GetMultilingualString("402b5926-2bb4-4efd-8b68-0219ab847f69", "A Stand Alone Declaration cannot be created for this consignment due to an unexpected error. Please reload the shipment form and try again. If this issue persists, please contact support");
		}
	}

	public class ConvertToStandAloneDeclarationSuccessfulResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationSuccessfulResponse(Guid convertedStandAloneDeclaration)
			: base(convertedStandAloneDeclaration)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentNotAttachedToShipmentResponse()
			: base(ErrorMessages.ConsignmentNotAttachedToShipment)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentMissingTransportDetailsResponse()
			: base(ErrorMessages.ConsignmentMissingTransportDetails)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentNotFoundResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentNotFoundResponse()
			: base(ErrorMessages.ConsignmentNotFound)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentHasClearedImportCustomsStatusResponse()
			: base(ErrorMessages.ConsignmentHasClearedImportCustomsStatus)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentHasClearedExportCustomsStatusResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentHasClearedExportCustomsStatusResponse()
			: base(ErrorMessages.ConsignmentHasClearedExportCustomsStatus)
		{
		}
	}

	public class ConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationShipmentDestinationNotSupportedResponse()
			: base(ErrorMessages.ShipmentDestinationNotSupported)
		{
		}
	}

	public class ConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationConsignmentHasExistingDeclarationResponse()
			: base(ErrorMessages.ConsignmentHasExistingDeclaration)
		{
		}
	}

	public class ConvertToStandAloneDeclarationUnexpectedErrorResponse : ConvertToStandAloneDeclarationResponse
	{
		public ConvertToStandAloneDeclarationUnexpectedErrorResponse()
			: base(ErrorMessages.UnexpectedError)
		{
		}

		public ConvertToStandAloneDeclarationUnexpectedErrorResponse(string errorMessage)
			: base(errorMessage)
		{
		}
	}
}
