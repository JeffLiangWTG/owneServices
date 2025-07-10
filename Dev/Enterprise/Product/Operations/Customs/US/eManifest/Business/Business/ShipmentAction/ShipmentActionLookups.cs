using CargoWise.EntityFramework;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	public class ShipmentActionLookups : ZLookups
	{
		public ShipmentActionLookups(ShipmentAction parent)
			: base(parent) { }

		public ICodeDescriptionPairList ActionCodes
		{
			get
			{
				ICodeDescriptionPairList result;
				if (Parent.LinkingOrDeLinkingShipment)
				{
					result = Parent.Shipment.IsLinked
								? Factory.GetCachedValue(
									"US.eManifest.Business.Shipment.MessageActionCodes.IsLinked",
									() => new CodeDescriptionPairList
									{
										new CodeDescriptionPair(MessageActionCodes.Codes.DeLink, MessageActionCodes.Descriptions.DeLink),
										new CodeDescriptionPair(MessageActionCodes.Codes.DoNotSend, MessageActionCodes.Descriptions.DoNotSend),
									})
								: (Parent.Shipment.B0_IsLodged || Parent.Shipment.IsSplit)
									? Factory.GetCachedValue(
										"US.eManifest.Business.Shipment.MessageActionCodes.IsLodged",
										() => new CodeDescriptionPairList
										{
											new CodeDescriptionPair(MessageActionCodes.Codes.Link, MessageActionCodes.Descriptions.Link),
											new CodeDescriptionPair(MessageActionCodes.Codes.DoNotSend, MessageActionCodes.Descriptions.DoNotSend)
										})
									: Factory.GetCachedValue(
										"US.eManifest.Business.Shipment.MessageActionCodes.IsNotLodged",
										() => new CodeDescriptionPairList
										{
											new CodeDescriptionPair(MessageActionCodes.Codes.DoNotSend, MessageActionCodes.Descriptions.DoNotSend)
										});
				}
				else
				{
					result = Parent.Shipment.B0_IsLodged
								? Factory.GetCachedValue(
									"US.eManifest.Business.Shipment.ActionCodes.IsLodged",
									() => new CodeDescriptionPairList
									{
										new CodeDescriptionPair(MessageActionCodes.Codes.Change, MessageActionCodes.Descriptions.Change),
										new CodeDescriptionPair(MessageActionCodes.Codes.Cancellation, MessageActionCodes.Descriptions.Cancellation),
										new CodeDescriptionPair(MessageActionCodes.Codes.DoNotSend, MessageActionCodes.Descriptions.DoNotSend)
									})
								: Factory.GetCachedValue(
									"US.eManifest.Business.Shipment.ActionCodes.IsNotLodged",
									() => new CodeDescriptionPairList
									{
										new CodeDescriptionPair(MessageActionCodes.Codes.Original, MessageActionCodes.Descriptions.Original),
										new CodeDescriptionPair(MessageActionCodes.Codes.DoNotSend, MessageActionCodes.Descriptions.DoNotSend)
									});
				}
				return result;
			}
		}

		public ICodeDescriptionPairList AmendmentReasonCodes
		{
			get
			{
				return Parent.Shipment.B0_ShipmentType == ShipmentTypes.Codes.Inbond
						? Factory.GetCachedValue(
							"US.eManifest.Business.Shipment.InBondAmendmentReasonCodes",
							() => new ShipmentAmendmentCodes(true))
						: Factory.GetCachedValue(
							"US.eManifest.Business.Shipment.NonInBondAmendmentReasonCodes",
							() => new ShipmentAmendmentCodes(false));
			}
		}

		#region Implementation

		new ShipmentAction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return ((ShipmentAction)base.Parent); }
		}

		#endregion
	}
}
