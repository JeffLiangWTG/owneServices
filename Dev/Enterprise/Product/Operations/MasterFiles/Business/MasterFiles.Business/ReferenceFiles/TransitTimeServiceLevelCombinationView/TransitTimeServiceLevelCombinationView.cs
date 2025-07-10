using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(TransitTimeServiceLevelCombinationViewSchema.Constants.TSC_Code), DescriptionProperty(nameof(ServiceLevelDescription))]
	public class TransitTimeServiceLevelCombinationView : AutoTransitTimeServiceLevelCombinationView
	{
		public TransitTimeServiceLevelCombinationView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var originZoneName = OriginZone?.SelectedZone != null ? OriginZone?.SelectedZone.ZoneCode : ZString.Empty;
				var destinationZoneName = DestinationZone?.SelectedZone != null ? DestinationZone?.SelectedZone.ZoneCode : ZString.Empty;
				return Res.GetString("3fda3ada-1414-4974-9358-c4ce59fb5f39", "{0} Zone Transit: {1} - {2}", TSC_Code, originZoneName, destinationZoneName);
			}
		}

		public RefTransitTime RefTransitTime
		{
			get => Factory.Load<RefTransitTime>(TSC_RTT);
		}

		public RefServiceLevel ServiceLevel
		{
			get => Factory.Load<RefServiceLevel>(TSC_RS);
		}

		public ZoneSelection OriginZone
		{
			get => RefTransitTime?.OriginZone;
		}

		public ZoneSelection DestinationZone
		{
			get => RefTransitTime?.DestinationZone;
		}

		public ZString OriginZoneCode
		{
			get => OriginZone?.SelectedZone?.ZoneCode ?? ZString.Empty;
		}

		public ZString OriginZoneOwner
		{
			get => OriginZone?.SelectedZoneRelatedOrg?.OH_Code ?? ZString.Empty;
		}

		public ZString DestinationZoneCode
		{
			get => DestinationZone?.SelectedZone?.ZoneCode ?? ZString.Empty;
		}

		public ZString DestinationZoneOwner
		{
			get => DestinationZone?.SelectedZoneRelatedOrg?.OH_Code ?? ZString.Empty;
		}

		[ResourceStringData("TransitTimeServiceLevelCombinationView|TSC_Code", ShortCaption = "Code", Caption = "Code", FullDescription = "The code of this service level.")]
		public override ZString TSC_Code
		{
			get => base.TSC_Code;
			set => base.TSC_Code = value;
		}

		public ZString ServiceLevelDescription
		{
			get => ServiceLevel?.RS_DescriptionMultilingual ?? ZString.Empty;
		}

		public ZString TransitTimeFormatted
		{
			get => RefTransitTime?.TransitTimeFormatted ?? ZString.Empty;
		}
	}
}
