using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentDocManagerInfo : DocManagerInfo
	{
		public ShipmentDocManagerInfo(CommonShipment parent, string docManagerCode = Constants.DocManagerCodes.Shipment)
			: base(parent, docManagerCode)
		{
		}

		CommonShipment Shipment
		{
			get { return (CommonShipment)BusinessEntity; }
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var list = new List<BusinessObject>();

			if (Shipment != null)
			{
				list.AddRange(Shipment.Consols);
				list.AddRange(Shipment.Containers);
				LoadRelatedDeclarationsAndCommInvoices(list);
				if (Shipment.Consignee != null)
				{
					list.Add(Shipment.Consignee);
				}

				if (Shipment.Consignor != null)
				{
					list.Add(Shipment.Consignor);
				}

				list.AddRange(Transactions);

				var quote = QuotedBookingQuote;
				if (quote != null)
				{
					list.Add(quote);
				}

				if (Shipment is ICartageParent) // tested in ICartageParentTestCase.TestRelatedLocalTransportObjectsWithEDocs
				{
					list.AddRange((BusinessObject[])Shipment.Factory.Load<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, Shipment.PK)));
				}
			}

			return list.ToArray();
		}

		void LoadRelatedDeclarationsAndCommInvoices(List<BusinessObject> list)
		{
			var declarations = Shipment.Declarations;
			if (declarations != null && declarations.Length > 0)
			{
				foreach (BusinessObject declaration in declarations)
				{
					GlbBranch branch = Shipment.Factory.Load<GlbBranch>((ZGuid)declaration[JobDeclarationSchema.JE_GB]);

					if (branch != null && branch.Company != null)
					{
						if (branch.Company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Singapore && string.IsNullOrEmpty(declaration[JobDeclarationSchema.JE_ApplicationCode].ToString()))
						{
							continue; // for old SG3 declarations, do not load them, because they are not based on basejobdeclaration....and dont have eDocs
						}

						if (branch.Company.Country != null && branch.Company.Country.PK == GlbCompany.CurrentCompany.Country.PK)
						{
							list.Add(declaration);
							IHaveEDocsChildren childGetter = ((IDocManagerSupport)declaration).DocManagerInfo as IHaveEDocsChildren;
							if (childGetter != null)
							{
								list.AddRange(childGetter.GetEDocsChildrenForAFreightJobToDisplay());
							}
						}
					}
				}
			}
		}

		public interface IHaveEDocsChildren
		{
			BusinessObject[] GetEDocsChildrenForAFreightJobToDisplay();
		}

		AccTransactionHeaderCollection Transactions
		{
			get { return new InvoiceLoader(BusinessEntity.Factory).GetInvoicesForUniqueRef(Shipment.JS_UniqueConsignRef); }
		}

		BusinessObject QuotedBookingQuote
		{
			get
			{
				BusinessObject quotedBookingQuote = null;

				if (!Shipment.JS_TH_OneTimeQuote.IsEmpty)
				{
					var quoteQuery = new ZQuery(RatingHeaderSchema.PK, Shipment.JS_TH_OneTimeQuote);
					quoteQuery.AddToFilter(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
					quotedBookingQuote = Shipment.Factory.LoadTop1(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType, quoteQuery);
				}

				if (quotedBookingQuote == null && Shipment.Job != null && !Shipment.Job.JH_TH_NKQuoteNumber.IsEmpty)
				{
					var query = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, Shipment.Job.JH_TH_NKQuoteNumber);
					quotedBookingQuote = Shipment.Factory.LoadTop1(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType, query);
				}

				return quotedBookingQuote;
			}
		}
	}
}
