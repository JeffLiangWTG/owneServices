using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public abstract partial class BasePricingPageRateLineListComparer
		: IComparer<PricingPageRateLineList>
		, IComparer
		, IEquatable<BasePricingPageRateLineListComparer>
	{
		public static BasePricingPageRateLineListComparer FromPage(PricingPage pricingPage)
		{
			OrgHeader client;
			var originFlags = PortFlags.None;
			var destinationFlags = PortFlags.None;
			ZString mode;
			ZString jobType;

			using (var enumerator = pricingPage.RateEntries.GetEnumerator())
			{
				if (!enumerator.MoveNext())
				{
					return FromCode(null, "", "");
				}

				var header = enumerator.Current.Parent;
				if (header == null || header.Header == null || header.Company == null)
				{
					return FromCode(null, "", "");
				}

				ILocation local = header.Company.Country;
				if (local == null)
				{
					return FromCode(null, "", "");
				}

				client = header.Header;
				mode = enumerator.Current.TI_Mode;
				jobType = CalculateJobType(enumerator.Current.RateType());

				do
				{
					var origin = enumerator.Current.Origin();
					var destination = enumerator.Current.Destination();

					originFlags |= (origin == null || local.CompletelyCovers(origin)) ? PortFlags.HasLocal : PortFlags.HasNonLocal;
					destinationFlags |= (destination == null || local.CompletelyCovers(destination)) ? PortFlags.HasLocal : PortFlags.HasNonLocal;
				}
				while (enumerator.MoveNext());
			}

			string direction = CalculateDirection(originFlags, destinationFlags);

			var invoiceRollUpOrGroup = new OrgInvoiceRollupOrGroup.Loader(client, GlbBranch.CurrentBranch, null).GetGroupOrSubTotal(direction, mode, mode, jobType);

			return FromCode(client, jobType, invoiceRollUpOrGroup);
		}

		public static BasePricingPageRateLineListComparer Alphabetical => new AlphabeticalPricingPageRateLineListComparer();

		public static BasePricingPageRateLineListComparer PrintSequence(OrgHeader client, ZString jobType) => new PrintSequencePricingPageRateLineListComparer(client, jobType);

		public static BasePricingPageRateLineListComparer LineSequence => new LineSequencePricingPageRateLineListComparer();

		public abstract int Compare(PricingPageRateLineList x, PricingPageRateLineList y);

		public virtual bool Equals(BasePricingPageRateLineListComparer other) => other != null && other.GetType() == GetType();

		public sealed override bool Equals(object obj) => Equals(obj as BasePricingPageRateLineListComparer);

		public override int GetHashCode() => GetType().GetHashCode();

		static ZString CalculateDirection(PortFlags originFlags, PortFlags destinationFlags)
		{
			if (originFlags == PortFlags.HasBoth || destinationFlags == PortFlags.HasBoth)
			{
				return OrgConstants.ServiceDirection.Code.All;
			}
			else if (originFlags == PortFlags.HasLocal)
			{
				if (destinationFlags == PortFlags.HasLocal)
				{
					return OrgConstants.ServiceDirection.Code.Domestic;
				}
				else
				{
					return OrgConstants.ServiceDirection.Code.Export;
				}
			}
			else
			{
				if (destinationFlags == PortFlags.HasLocal)
				{
					return OrgConstants.ServiceDirection.Code.Import;
				}
				else
				{
					return OrgConstants.ServiceDirection.Code.CrossTrade;
				}
			}
		}

		static ZString CalculateJobType(RateType rateType)
		{
			if ((rateType & RateType.Forwarding) != 0)
			{
				return JobInvoicingConsumerTypes.Shipment.Code;
			}
			else if ((rateType & RateType.CFS) != 0)
			{
				return JobInvoicingConsumerTypes.CFSShipment.Code;
			}
			else if ((rateType & RateType.Warehouse) != 0)
			{
				return JobInvoicingConsumerTypes.WarehouseStorage.Code;
			}
			else if ((rateType & RateType.TransportBookings) != 0)
			{
				return JobInvoicingConsumerTypes.TransportBooking.Code;
			}
			else if ((rateType & RateType.LocalTransport) != 0)
			{
				return JobInvoicingConsumerTypes.LocalCartage.Code;
			}
			else if ((rateType & RateType.Shipping) != 0)
			{
				return JobInvoicingConsumerTypes.AgencyBillOfLading.Code;
			}
			else if ((rateType & RateType.ShippingImportDetention) != 0)
			{
				return JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code;
			}
			else if ((rateType & RateType.ShippingExportDetention) != 0)
			{
				return JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code;
			}
			else
			{
				return ZString.Empty;
			}
		}

		static BasePricingPageRateLineListComparer FromCode(OrgHeader client, ZString jobType, ZString invoiceRollUpOrGroup)
		{
			switch (invoiceRollUpOrGroup)
			{
				case OrgConstants.GroupOrSubTotalCharges.Code.Alphabetical:
					return Alphabetical;

				case OrgConstants.GroupOrSubTotalCharges.Code.Sequence:
				case OrgConstants.GroupOrSubTotalCharges.Code.SubTotalAndSequence:
				case OrgConstants.GroupOrSubTotalCharges.Code.RollupAndSequence:
					return PrintSequence(client, jobType);

				case OrgConstants.GroupOrSubTotalCharges.Code.User:
				case OrgConstants.GroupOrSubTotalCharges.Code.RollUp:
				case OrgConstants.GroupOrSubTotalCharges.Code.RollUpEntireConsol:
				case OrgConstants.GroupOrSubTotalCharges.Code.SubTotal:
					return LineSequence;

				default: // used for printing Company Tariff
					return LineSequence;
			}
		}

		#region Nested Classes

		[Flags]
		enum PortFlags
		{
			None = 0x00,
			HasLocal = 0x01,
			HasNonLocal = 0x02,
			HasBoth = HasLocal | HasNonLocal
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y) => Compare((PricingPageRateLineList)x, (PricingPageRateLineList)y);

		#endregion
	}
}

