using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.Test;

public class DummyAutoRating : DummyEnterpriseBusinessObject, IAutoRating
{
	public DummyAutoRating(BusinessObjectFactory factory, System.Data.DataRow row)
		: base(factory, row)
	{
	}

	public IRateableMeasureSet RateableMeasures
	{
		get
		{
			var result = new RateableMeasureSet();
			result.SetQuantity(MeasureType.Chargeable, 10m, "KG");
			result.SetQuantity(MeasureType.Weight, 10m, "LB");
			result.SetQuantity(MeasureType.Volume, 13m, "M3");

			var gp20 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var gp40 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");

			result.AddContainerGroup(MeasureInfo.ContainerInfo.LCL, "", "CAR", "CONT00001", 110m, 1.1m,
				new[]
				{
					new MeasureInfo.ContainerInfo(110m, Constants.Weight.Kilograms, 1.1m, Constants.Volume.CubicMetres, 11, 1, "CONT00001", containerCount: 0)
				});
			result.AddContainerGroup(gp20.PK, "", "CAR", "CONT00002", 120m, 1.2m,
				new[]
				{
					new MeasureInfo.ContainerInfo(120m, Constants.Weight.Kilograms, 1.2m, Constants.Volume.CubicMetres, 12, 1, "CONT00002")
				});
			result.AddContainerGroup(gp40.PK, "", "CAR", "CONT00003", 130m, 130m,
				new[]
				{
					new MeasureInfo.ContainerInfo(130m, Constants.Weight.Kilograms, 130m, Constants.Volume.CubicMetres, 13, 2, "CONT00003")
				});
			result.AddContainerGroup(gp40.PK, "", "CAR", "CONT00004", 140m, 1.4m,
				new[]
				{
					new MeasureInfo.ContainerInfo(140m, Constants.Weight.Kilograms, 1.4m, Constants.Volume.CubicMetres, 14, 1, "CONT00004")
				});
			result.AddContainerGroup(ZGuid.BrettsGuid, "", "CAR", "CONT00005", 150m, 1.5m,
				new[]
				{
					new MeasureInfo.ContainerInfo(150m, Constants.Weight.Kilograms, 1.5m, Constants.Volume.CubicMetres, 15, 1, "CONT00005")
				});
			result.AddContainerGroup(ZGuid.Empty, "", "CAR", "CONT00006", 160m, 1.6m,
				new[]
				{
					new MeasureInfo.ContainerInfo(160m, Constants.Weight.Kilograms, 1.6m, Constants.Volume.CubicMetres, 16, 1, "CONT00006")
				});

			result.SetQuantity(MeasureType.Package, 0m, "");

			result.SetQuantity(MeasureType.Unidentified, 1m, "SV");
			return result;
		}
	}

	public OrgHeader CarrierToUseForTest;

	#region Unused

	#region IAutoRating Members

	public IJobExRateCurrencyConverter CurrencyConverter => throw new NotImplementedException();

	public AdapterType AdapterType => default(AdapterType);

	public ZBool IsServicesOnly => false;

	public bool ShouldRemoveChargeWhenMissingServiceOrChargeableUnit(AccChargeCode chargeCode)
	{
		throw new NotImplementedException();
	}

	public void OnAutoRated(IEnumerable<IAutoRatedCharge> charges)
	{
	}

	public IJobInvoicingSupporter InvoicingSupporter
	{
		get { throw new NotImplementedException(); }
	}

	public IJobDatesProvider JobDatesProvider
	{
		get { throw new NotImplementedException(); }
	}

	public ChargeCodeGroupCollection ChargeCodeGroups
	{
		get { throw new NotImplementedException(); }
	}

	public JobInvoicingConsumerType ConsumerType
	{
		get { throw new NotImplementedException(); }
	}

	public MergeChargeOptions MergeCharges
	{
		get { throw new NotImplementedException(); }
	}

	public RateType RateTypeToUse
	{
		get { throw new NotImplementedException(); }
	}

	public JobServicesCollection JobServices
	{
		get { throw new NotImplementedException(); }
	}

	public IEnumerable<ZString> CarrierContractNumbers => throw new NotImplementedException();

	public IEnumerable<ZString> ClientContractNumbers => throw new NotImplementedException();

	public AutoRatingStatusInfo StatusInformation
	{
		get { throw new NotImplementedException(); }
	}

	public Collection<IBusiness> AutoRatedFor
	{
		get { return null; }
	}

	public IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell)
	{
		throw new NotImplementedException();
	}

	public ZString ExtraReference(AccChargeCode chargeCode)
	{
		return ZString.Empty;
	}

	public ZString OperationalJobCode => "QuickCalculator";

	public ZString JobID => "QuickCalculator";

	ZInt IAutoRating.RouteSetNumber
	{
		get { return 0; }
	}

	public ZString NamedAccount => throw new NotImplementedException();

	public ZString PaymentTermOverride { get => ZString.Empty; }

	public ZBool ShouldDiscardPerJobCharges => false;

	public IEnumerable<ZString> ExcludedAttributesWhenMergingRateInfos => throw new NotImplementedException();

	public ZString HBLDeliveryMode { get => ZString.Empty; }

	#endregion

	#region IAutoRatingLocations Members

	public ILocation Destination
	{
		get { throw new NotImplementedException(); }
	}

	public ILocation Origin
	{
		get { throw new NotImplementedException(); }
	}

	public ILocation GetVia(CostSell costOrSell) => throw new NotImplementedException();

	public ILocation PlannedLoad(CostSell costOrSell) => throw new NotImplementedException();

	public ILocation PlannedDischarge(CostSell costOrSell) => throw new NotImplementedException();

	public ILocation RateOrigin
	{
		get { throw new NotImplementedException(); }
	}

	public ILocation RateDestination
	{
		get { throw new NotImplementedException(); }
	}

	public ILocation GetFirstLoad(CostSell costOrSell) => throw new NotImplementedException();
	public ILocation GetLastDischarge(CostSell costOrSell) => throw new NotImplementedException();
	public ILocation GetFirstRouteSetLoad(CostSell costOrSell) => throw new NotImplementedException();
	public ILocation GetLastRouteSetDischarge(CostSell costOrSell) => throw new NotImplementedException();

	#endregion

	#region IAutoRatingOrganisations Members

	public OrgHeader ImportBroker
	{
		get { return null; }
	}

	public OrgHeader ExportBroker
	{
		get { return null; }
	}

	public DebtorOrgCollection DebtorOrgs
	{
		get { return new DebtorOrgCollection(); }
	}

	public OrgHeader OverrideDebtor(AccChargeCode chargeCode)
	{
		return null;
	}

	public OrgHeader Carrier
	{
		get { return CarrierToUseForTest; }
	}

	public ZString DeliveryCartageEquipment
	{
		get { throw new NotImplementedException(); }
	}

	public IDocAddress DeliveryAddress
	{
		get { throw new NotImplementedException(); }
	}

	public IDocAddress ConsigneeDocumentaryAddress
	{
		get { throw new NotImplementedException(); }
	}

	public ZString PickupCartageEquipment
	{
		get { throw new NotImplementedException(); }
	}

	public IDocAddress PickupAddress
	{
		get { throw new NotImplementedException(); }
	}

	public IDocAddress ConsignorDocumentaryAddress
	{
		get { throw new NotImplementedException(); }
	}

	public Creditors Creditors
	{
		get { throw new NotImplementedException(); }
	}

	public IEnumerable<OrgHeader> PossibleServiceProviders
	{
		get { throw new NotImplementedException(); }
	}

	public IEnumerable<OrgHeader> PossibleCarriers => throw new NotImplementedException();

	#endregion

	#region IAutoRatingFreightInfo Members

	public FreightMode FreightMode
	{
		get { throw new NotImplementedException(); }
	}

	public ZString HousebillReleaseType
	{
		get { throw new NotImplementedException(); }
	}

	public PaymentTermInfos PaymentTerm { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

	public ZString AircraftType
	{
		get { throw new NotImplementedException(); }
	}

	public ZString FMCTariffID => throw new NotImplementedException();
	public IEnumerable<RefCommodityCode> OverriddenCommodity => throw new NotImplementedException();

	public bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
	{
		throw new NotImplementedException();
	}

	public Directions JobDirection
	{
		get { throw new NotImplementedException(); }
	}

	public MoneyType MonetaryValues
	{
		get { throw new NotImplementedException(); }
	}

	public ServiceLevelRatingInformation ServiceLevel
	{
		get { throw new NotImplementedException(); }
	}

	public OrgAddress WharfCTOAddress
	{
		get { throw new NotImplementedException(); }
	}

	public ZString ContainerMode
	{
		get { throw new NotImplementedException(); }
	}

	public bool SkipFreightCharge => throw new NotImplementedException();

	#endregion

	#endregion
}
