using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.AutoRating.CarrierShipmentRates;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business;

sealed class CarrierShipmentRatingAdapter(CarrierShipmentRateQueryBusinessObject parent, CostSell costOrSell, BusinessObjectFactory factory) : RatingAdapter<CarrierShipmentRateQueryBusinessObject>(parent)
{
	public override ILocation Origin => LocationHelper.GetCachedLocationFromString(Parent.Origin, factory);

	public override ILocation Destination => LocationHelper.GetCachedLocationFromString(Parent.Destination, factory);

	public override RateType RateTypeToUse => Parent.RateType;

	public override IJobDatesProvider JobDatesProvider => new CarrierShipmentJobDatesProvider(Parent);

	public override Creditors Creditors => Creditors.New(OrgWithSource.New(GetOrgHeaderFromCode(Parent.Carrier), ["TransportProvider"]));

	public override DebtorOrgCollection DebtorOrgs
	{
		get
		{
			var result = base.DebtorOrgs;
			if (!string.IsNullOrEmpty(Parent.Consignor))
			{
				result[Registry.Business.RatingDebtorOrgTypes.CNR] = GetOrgHeaderFromCode(Parent.Consignor);
			}
			if (!string.IsNullOrEmpty(Parent.Consignee))
			{
				result[Registry.Business.RatingDebtorOrgTypes.CNE] = GetOrgHeaderFromCode(Parent.Consignee);
			}
			if (!string.IsNullOrEmpty(Parent.BookingParty))
			{
				result[Registry.Business.RatingDebtorOrgTypes.LCBK] = GetOrgHeaderFromCode(Parent.BookingParty);
			}

			return result;
		}
	}

	public override FreightMode FreightMode => Parent.FreightMode;

	public override IRateableMeasureSet RateableMeasures
	{
		get
		{
			if (measureSet == null)
			{
				var converter = new CarrierShipmentMeasuresAdapter(Parent, costOrSell, factory);
				measureSet = converter.Convert(AdapterType);
			}

			return measureSet;
		}
	}

	IRateableMeasureSet measureSet;

	public override ChargeCodeGroupCollection ChargeCodeGroups
	{
		get
		{
			var result = new ChargeCodeGroupCollection();
			result.AddRange(Env.Registry.Rating.FreightRatedCodes);
			return result;
		}
	}

	OrgHeader GetOrgHeaderFromCode(string code)
	{
		return factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, code);
	}
}
