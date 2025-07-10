using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.US.Business.Protest
{
	using Enterprise.Rating.Rateable;
	using Enterprise.ZArchitecture.Schema;

	class ProtestRatingAdapter : RatingAdapter<Protest>, IAutoRatingCustomsInfo
	{
		public ProtestRatingAdapter(Protest parent)
			: base(parent)
		{
		}

		public override AdapterType AdapterType => AdapterType.Protest;

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return ((IJobInvoicingPlugIn)Parent).InvoicingSupporter; }
		}

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<Protest>(Parent); }
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.BrokerageRatedCodes);
				return result;
			}
		}

		public override JobInvoicingConsumerType ConsumerType
		{
			get { return JobInvoicingConsumerTypes.Brokerage; }
		}

		public override MergeChargeOptions MergeCharges
		{
			get { return MergeChargeOptions.WithinAdapter; }
		}

		public override RateType RateTypeToUse
		{
			get { return RateType.Forwarding; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				if (Parent.Protestant != null)
				{
					result[RatingDebtorOrgTypes.CNE] = Parent.Protestant.Organisation;
				}

				return result;
			}
		}

		public override ILocation Destination
		{
			get { return Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates); }
		}

		public override IDocAddress DeliveryAddress
		{
			get { return (Parent.Protestant == null) ? null : Parent.Protestant.Organisation.MainAddress; }
		}

		public override FreightMode FreightMode
		{
			get { return FreightMode.UKN; }
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>();
				result.Add(new ServiceLevelInfo(Parent.Declaration.JE_RS_NKServiceLevel, ServiceLevelType.Client));
				result.Add(new ServiceLevelInfo(Parent.Declaration.JE_RS_NKServiceLevel, ServiceLevelType.Carrier));
				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get { return Parent.Entries; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get { return new InvoiceInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerInvoice
		{
			get { return new InvoiceInfoCollection(); }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.TariffsPerShipment
		{
			get { return new InvoiceInfoCollection(); }
		}

		ZString IAutoRatingCustomsInfo.MessageSubType
		{
			get { return ZString.Empty; }
		}

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get { return Parent.Declaration.JE_MessageType; }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}
	}
}
