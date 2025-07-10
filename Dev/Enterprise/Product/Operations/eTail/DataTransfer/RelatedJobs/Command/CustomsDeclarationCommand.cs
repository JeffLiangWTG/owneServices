using System;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;

namespace Enterprise.eTail.DataTransfer
{
	[CodeAlive("Used by reflection code in HVLVCustomsMenuGroup")]
	[ApplicableLoginCountry(new[] { CountryCodes.Canada })]
	[ShipmentDestinationCountry(CountryCodes.Canada)]
	[ShipmentDirection(new[] { Directions.Import })]
	public class CustomsDeclarationCommand : BaseHVLVRelatedJobCommand
	{
		public CustomsDeclarationCommand(ForwardingShipment shipment) : base(shipment)
		{
		}

		public override MultilingualString RelatedJobName => ResString.GetMultilingualString("9B3085A0-C659-4B52-8C85-6D2927781B20", "Stand Alone Declaration");
		public override CustomsRelatedBusinessObjectConverter Converter => new CustomsDeclarationConverter(this);
		protected override Type RelatedCustomsJobType => typeof(BaseJobDeclaration);
		public override string UsageCode => UsageCodes.ImportStandAloneDeclaration;
	}
}
