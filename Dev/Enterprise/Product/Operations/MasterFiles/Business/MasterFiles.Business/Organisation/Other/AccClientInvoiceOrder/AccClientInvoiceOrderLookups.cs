//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccClientInvoiceOrderLookups
//
//    This class should be used for overriding collections in AutoAccClientInvoiceOrderLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccClientInvoiceOrderLookups : AutoAccClientInvoiceOrderLookups
	{
		public AccClientInvoiceOrderLookups(AutoAccClientInvoiceOrder parent)
			: base(parent)
		{
		}

		#region ChargeCodes

		AccChargeCodeCollection fChargeCodes;
		public override AccChargeCodeCollection ChargeCodes
		{
			get
			{
				if (fChargeCodes == null)
				{
					ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MRG");
					filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "DSB");
					filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "REV");
					filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "NON");
					filter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, "MJA");
					filter.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
					fChargeCodes = new AccChargeCodeCollection(Factory, filter);
				}
				return fChargeCodes;
			}
		}

		#endregion

		#region InvoiceTypeList

		public CodeDescriptionPairList InvoiceTypeList
		{
			get
			{
				CodeDescriptionPairList fInvoiceTypesList = new CodeDescriptionPairList();
				fInvoiceTypesList.Add(InvoiceTypes.All);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.FinalInvoice, InvoiceTypesList.Descriptions.FinalInvoice);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.DisbursementInvoice, InvoiceTypesList.Descriptions.DisbursementInvoice);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.DestinationChargesInvoice, InvoiceTypesList.Descriptions.DestinationChargesInvoice);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.ForeignCurrencyInvoice, InvoiceTypesList.Descriptions.ForeignCurrencyInvoice);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.FreightInvoice, InvoiceTypesList.Descriptions.FreightInvoice);
				fInvoiceTypesList.AddPair(InvoiceTypesList.Codes.InvoicePerTaxCode, InvoiceTypesList.Descriptions.InvoicePerTaxCode);

				return fInvoiceTypesList;
			}
		}

		public static class InvoiceTypes
		{
			public static CodeDescriptionPair All
			{
				get { return new CodeDescriptionPair("ALL", Res.GetString("28272abd-94c9-4b71-bef1-c8ba7700e9c4", "All Invoices")); }
			}
		}

		#endregion
	}
}
