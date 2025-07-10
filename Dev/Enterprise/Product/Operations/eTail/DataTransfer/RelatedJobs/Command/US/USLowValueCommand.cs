using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.US.LVS;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.UnitedStates })]
	[ShipmentDestinationCountry(CountryCodes.UnitedStates)]
	public class USLowValueCommand : CustomsJobCommand
	{
		public USLowValueCommand(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("7c2085d7-1fe6-494c-9371-729e5002b05d", "Low Value Entries");

		public override bool ShouldValidateWaybill => true;

		protected override Type RelatedCustomsJobType => typeof(ICusUSLVClearance);

		public override CustomsRelatedBusinessObjectConverter Converter => new USLowValueConverter(this);

		public override string UsageCode => UsageCodes.USLowValueEntries;
	}
}
