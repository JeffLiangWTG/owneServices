using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	class ReconDeclarationRatingAdapter : RatingAdapter<ReconDeclaration>, IAutoRatingCustomsInfo
	{
		public ReconDeclarationRatingAdapter(ReconDeclaration parent)
			: base(parent)
		{
		}

		#region RatingAdapter

		public override AdapterType AdapterType => AdapterType.ReconDeclaration;

		public override IJobDatesProvider JobDatesProvider
		{
			get { return new JobDatesProvider<ReconDeclaration>(Parent); }
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

		public override IJobInvoicingSupporter InvoicingSupporter
		{
			get { return Parent.InvoicingSupporter; }
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.Importer?.MainAddress; }
		}

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var result = base.DebtorOrgs;
				result[RatingDebtorOrgTypes.CNE] = Parent.Importer;

				return result;
			}
		}

		public override FreightMode FreightMode
		{
			get { return FreightMode.UKN; }
		}

		public override Directions JobDirection
		{
			get { return Parent.JobDirection; }
		}

		public override MoneyType MonetaryValues
		{
			get { return Parent.ReconWrappedJobDeclaration.RatingAdapter.MonetaryValues; }
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var result = new List<ServiceLevelInfo>();
				result.Add(new ServiceLevelInfo(Parent.ReconWrappedJobDeclaration.JE_RS_NKServiceLevel, ServiceLevelType.Client));
				return new ServiceLevelRatingInformation(result.ToArray());
			}
		}

		EntryInfoCollection IAutoRatingCustomsInfo.Entries
		{
			get { return Parent.Entries; }
		}

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices
		{
			get { return ((IAutoRatingCustomsInfo)Parent.ReconWrappedJobDeclaration.RatingAdapter).Invoices; }
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
			get { return "09"; }
		}

		ZString IAutoRatingCustomsInfo.MessageType
		{
			get { return Parent.ReconWrappedJobDeclaration.JE_MessageType; }
		}

		ZInt IAutoRatingCustomsInfo.SubHeaderCount
		{
			get { return 0; }
		}

		public override ILocation Destination
		{
			get { return Parent.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates); }
		}

		#endregion
	}
}
