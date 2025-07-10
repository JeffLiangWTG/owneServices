using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBSpecialHandlingLookups : Forwarding.AWB.Business.ExportAWBSpecialHandlingLookups
	{
		public ExportAWBSpecialHandlingLookups(AutoExportAWBSpecialHandling parent)
			: base(parent)
		{
		}

		AWBSpecialHandlingCodeDescriptionPairList GetSpecialHandlingCodeDescriptionList(bool justAirline = false)
		{
			var specialHandlingCodeDescriptionPairList = new AWBSpecialHandlingCodeDescriptionPairList();
			
			if (Parent is ExportAWBSpecialHandling consolExportAWBSpecialHandling && consolExportAWBSpecialHandling?.Master is ConsolExportAWBHeader consolAwbHeader && (consolAwbHeader.Consol?.ShouldRemoveSCOSecurityStatus ?? false))
			{
				specialHandlingCodeDescriptionPairList.RemoveCode(AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly);
			}

			if (justAirline)
			{
				specialHandlingCodeDescriptionPairList.Clear();
			}

			if (Parent is ExportAWBSpecialHandling exportAWBSpecialHandling && exportAWBSpecialHandling?.Master is ExportAWBHeader awbHeader && awbHeader != null)
			{
				var carrier = awbHeader.EH_AirlinePrefix;
				var originAirPort = awbHeader.OriginLOCOCode;
				var destAirport = awbHeader.DestinationLOCOCode;
				var airline = RefAirline.LoadFromAirlinePrefix(Factory, carrier);

				if (airline == null || string.IsNullOrEmpty(originAirPort) || string.IsNullOrEmpty(destAirport))
				{
					return specialHandlingCodeDescriptionPairList;
				}

				var definedSpecialHandling = airline.RefAirlineSpecialHandlingCodeCollection
					.Where(x => (x.RHC_OriginPortOrCountry.EqualsIgnoringCase(originAirPort)
					|| x.RHC_OriginPortOrCountry == ZString.Empty
					|| x.RHC_OriginPortOrCountry.EqualsIgnoringCase(awbHeader.OriginCountryCode))
				&& (x.RHC_DestinationPortOrCountry.EqualsIgnoringCase(destAirport)
					|| x.RHC_DestinationPortOrCountry == ZString.Empty
					|| x.RHC_DestinationPortOrCountry.EqualsIgnoringCase(awbHeader.DestinationCountryCode))
				&& x.RHC_RM_Airline == airline?.PK);

				foreach (var code in definedSpecialHandling)
				{
					specialHandlingCodeDescriptionPairList.AddPairIfNotExist(code.RHC_Code, code.RHC_Description);
				}
				specialHandlingCodeDescriptionPairList.Sort();
			}
			return specialHandlingCodeDescriptionPairList;
		}

		public override AWBSpecialHandlingCodeDescriptionPairList SpecialHandlingCodeDescriptionList
		{
			get
			{
				return GetSpecialHandlingCodeDescriptionList();
			}
		}

		public override AWBSpecialHandlingCodeDescriptionPairList SpecialHandlingCodeDescriptionListInAirLine
		{
			get
			{
				return GetSpecialHandlingCodeDescriptionList(true);
			}
		}
	}
}
