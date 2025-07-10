using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	class PriorNoticeLine : IPriorNoticeLine
	{
		public PriorNoticeLine(FDA fda, bool reportValue)
		{
			this.fda = fda;
			this.reportValue = reportValue;
		}

		readonly IPriorNoticeLine fda;
		readonly bool reportValue;

		#region IPriorNoticeLine Members

		IPriorNoticeHeader IPriorNoticeLine.PriorNoticeHeader
		{
			get { return fda.PriorNoticeHeader; }
		}

		ZBool IPriorNoticeLine.RequiresPriorNotice
		{
			get { return fda.RequiresPriorNotice; }
		}

		ZBool IPriorNoticeLine.IsDisclaimed
		{
			get { return fda.IsDisclaimed; }
		}

		ZString IPriorNoticeLine.ConfirmationNumber
		{
			get { return fda.ConfirmationNumber; }
		}

		ZString IPriorNoticeLine.CountryOfShipping
		{
			get { return fda.CountryOfShipping; }
		}

		ZString IPriorNoticeLine.FoodFacilityRegistrationExemption
		{
			get { return fda.FoodFacilityRegistrationExemption; }
		}

		ZString IPriorNoticeLine.FoodFacilityRegistrationNumber
		{
			get { return fda.FoodFacilityRegistrationNumber; }
		}

		ZString IPriorNoticeLine.OwnerFirmType
		{
			get { return fda.OwnerFirmType; }
		}

		ZString IPriorNoticeLine.ProducerFirmType
		{
			get { return fda.ProducerFirmType; }
		}

		ZString IPriorNoticeLine.ShipperRegistrationNumber
		{
			get { return fda.ShipperRegistrationNumber; }
		}

		IEnumerable<IMasterHouse> IPriorNoticeLine.Bills
		{
			get { return fda.Bills; }
		}

		IEnumerable<ZString> IPriorNoticeLine.RailCarNumbers
		{
			get { return fda.RailCarNumbers; }
		}

		IEnumerable<ZString> IPriorNoticeLine.ContainerNumbers
		{
			get { return fda.ContainerNumbers; }
		}

		ZString IPriorNoticeLine.HarmonizedTariffNumber
		{
			get { return fda.HarmonizedTariffNumber; }
		}

		OrgHeader IPriorNoticeLine.Importer
		{
			get { return fda.Importer; }
		}

		OrgHeader IPriorNoticeLine.Consignee
		{
			get { return fda.Consignee; }
		}

		#endregion

		#region IFDALine Members

		ZString IFDALine.CommercialDescription
		{
			get { return fda.CommercialDescription; }
		}

		ZInt IFDALine.FDALineNumber
		{
			get { return fda.FDALineNumber; }
			set { fda.FDALineNumber = value; }
		}

		ZString IFDALine.FDAProductCode
		{
			get { return fda.FDAProductCode; }
		}

		ZString IFDALine.CargoStorageStatus
		{
			get { return fda.CargoStorageStatus; }
		}

		ZString IFDALine.CountryOfProduction
		{
			get { return fda.CountryOfProduction; }
		}

		IReadOnlyList<AffirmationCode> IFDALine.AffirmationCodes
		{
			get { return fda.AffirmationCodes; }
		}

		ZString IFDALine.ManufacturerNumber
		{
			get { return fda.ManufacturerNumber; }
		}

		ZString IFDALine.SupplierOrShipperNumber
		{
			get { return fda.SupplierOrShipperNumber; }
		}

		List<FDAQtyUQPair> IFDALine.OrderedQtyUQs
		{
			get { return fda.OrderedQtyUQs; }
		}

		ZDecimal IFDALine.ValueInWholeDollars
		{
			get
			{
				return reportValue ? fda.ValueInWholeDollars : ZDecimal.Zero;
			}
			set
			{
				fda.ValueInWholeDollars = value;
			}
		}

		ZString IFDALine.ConsigneeFEI
		{
			get { return fda.ConsigneeFEI; }
		}

		ZString IFDALine.TradeOrBrandName
		{
			get { return fda.TradeOrBrandName; }
		}

		ZDecimal IFDALine.FirstDimension
		{
			get { return fda.FirstDimension; }
		}

		ZDecimal IFDALine.SecondDimension
		{
			get { return fda.SecondDimension; }
		}

		ZDecimal IFDALine.ThirdDimension
		{
			get { return fda.ThirdDimension; }
		}

		ZString IFDALine.DimensionUQ
		{
			get { return fda.DimensionUQ; }
		}

		ZString IFDALine.ContactName
		{
			get { return fda.ContactName; }
		}

		ZString IFDALine.ContactPhone
		{
			get { return fda.ContactPhone; }
		}

		ZString IFDALine.ContactEmail
		{
			get { return fda.ContactEmail; }
		}

		#endregion

		#region IOGALine Members

		ZString IOGALine.CommercialDesc
		{
			get { return fda.CommercialDesc; }
			set { fda.CommercialDesc = value; }
		}

		#endregion
	}
}
