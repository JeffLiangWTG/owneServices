using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	public class TrackingInvoiceLoader
	{
		public TrackingInvoiceLoader(ITransactionSupport parent)
		{
			this.Parent = parent;
		}

		readonly ITransactionSupport Parent;

		public TrackingTransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new TrackingTransactionHeaderCollection(Parent.Factory);
					if (RelatedOrganisation != null)
					{
						InvoiceLoader invLoader = new InvoiceLoader(Parent.Factory);
						AccTransactionHeaderCollection result;
						invLoader.IncludeReversedTransaction = false;
						invLoader.LimitToCurrentCompany = false;

						if (Parent.SiteUser != null && Parent.SiteUser.IsShipmentQuickViewUser)
						{
							result = invLoader.GetInvoicesForUniqueRef(Parent.Reference);
						}
						else
						{
							List<ZGuid> parties = new List<ZGuid>();
							parties.Add(RelatedOrganisation.PK);
							parties.AddRange(RelatedOrganisation.AllParentParties.Where(x => x.PR_PartyType == RelatedPartyTypeList.Codes.ControllingCustomer).Select(x => x.PR_OH_Parent));
							result = invLoader.GetInvoicesForUniqueRef(parties, Parent.Reference);
						}

						if (result.Count > 0)
						{
							ZQuery transactionLineFilter = new ZQuery(AccTransactionHeaderSchema.PK, result.GetPKs());
							fTransactions.Load(transactionLineFilter);
						}
					}
				}
				return fTransactions;
			}
		}
		TrackingTransactionHeaderCollection fTransactions;

		public InvoicingLineBaseCollection LocalChargesDetails
		{
			get
			{
				if (fLocalChargesDetails == null && Parent.SiteUser != null && Parent.SiteUser.IsShipmentQuickViewUser)
				{
					InvoicingBase localChargeInvoice = null;
					Transactions.Sort(AccTransactionHeaderSchema.AH_InvoiceDate.Name, ListSortDirection.Ascending);
					foreach (InvoicingBase invoice in Transactions)
					{
						if (invoice.AH_FullyPaidDate.IsEmpty &&
							invoice.Job != null && invoice.AH_OH == invoice.Job.LocalChargesPK)
						{
							localChargeInvoice = invoice;
							break;
						}
					}
					if (localChargeInvoice != null)
					{
						fLocalChargesDetails = localChargeInvoice.Lines;
					}
				}
				return fLocalChargesDetails;
			}
		}
		InvoicingLineBaseCollection fLocalChargesDetails;

		protected OrgHeader RelatedOrganisation
		{
			get
			{
				return Parent.LoggedInOrganisation ?? (WebEnv.CurrentUser != null ? ((OrgContact)WebEnv.CurrentUser).ParentOrg : null);
			}
		}

		public ZString ChargesTotalsAsString
		{
			get
			{
				StringBuilder result = new StringBuilder();
				Dictionary<RefCurrency, ZDecimal> totals = new Dictionary<RefCurrency, ZDecimal>();

				foreach (InvoicingBase invoice in this.Transactions)
				{
					RefCurrency currency = invoice.TransactionCurrency;
					ZDecimal total;

					if (totals.TryGetValue(currency, out total))
					{
						total += invoice.AH_OSTotal;
						totals[currency] = total;
					}
					else
					{
						totals.Add(currency, invoice.AH_OSTotal);
					}
				}

				foreach (RefCurrency currency in totals.Keys)
				{
					result.Append(string.Format("{0} {1}, ", currency.RX_Code, totals[currency].ToString("N" + currency.Decimals.ToString())));
				}

				return result.ToString().TrimEnd(',', ' ');
			}
		}
	}
}
