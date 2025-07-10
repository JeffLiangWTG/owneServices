using System;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Workflow.Business
{
	class UniversalConstantConverter
	{
		public static Type GetType(EDIMessageContentFilterSchemaType type)
		{
			switch (type)
			{
				case EDIMessageContentFilterSchemaType.Shipment:
					return typeof(UniversalDataBuss.DataObjects.Universal.Shipment);

				case EDIMessageContentFilterSchemaType.Event:
					return typeof(UniversalDataBuss.DataObjects.Universal.Event);

				default:
					return null;
			}
		}

		public static CodeDescriptionPair GetCodeDescriptionPair(EDIMessageContentFilterSchemaType type)
		{
			switch (type)
			{
				case EDIMessageContentFilterSchemaType.Shipment:
					return new CodeDescriptionPair(EDIMessageContentFilterLineSchemas.Codes.UniversalShipment,
					EDIMessageContentFilterLineSchemas.Descriptions.UniversalShipment);

				case EDIMessageContentFilterSchemaType.Event:
					return new CodeDescriptionPair(EDIMessageContentFilterLineSchemas.Codes.UniversalEvent,
					EDIMessageContentFilterLineSchemas.Descriptions.UniversalEvent);

				default:
					return null;
			}
		}

		public static EDIMessageContentFilterSchemaType GetSchemaTypeFromActionType(string code)
		{
			switch (code)
			{
				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML:
					return EDIMessageContentFilterSchemaType.Shipment;

				case WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML:
					return EDIMessageContentFilterSchemaType.Event;

				default:
					throw new ArgumentException(FormattableString.Invariant($"Unrecognised action type {code}"), nameof(code));
			}
		}

		public static EDIMessageContentFilterSpec GetSpec(EDIMessageContentFilter filter, EDIMessageContentFilterSchemaType type)
		{
			switch (type)
			{
				case EDIMessageContentFilterSchemaType.Shipment:
					return filter.UniversalShipment;
				case EDIMessageContentFilterSchemaType.Event:
					return filter.UniversalEvent;

				default:
					return null;
			}
		}
	}
}
