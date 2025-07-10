using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgOpportunityDocumentSupporter : DocumentSupporter
	{
		public OrgOpportunityDocumentSupporter(BusinessObject parent)
			: base(parent)
		{
		}
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.OrgOpportunity; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, base.BusinessObject);
			return genericWrappers;
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new[]
				{
					Core.Constants.DataContext.BusinessObject,
					Core.Constants.DataContext.GenericFreightJob
				};
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Quotation, BusinessContext.Organisation }; }
		}

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menu, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			var resultList = new List<IDocumentSupportable> { };
			var opportunity = (OrgOpportunity)base.BusinessObject;
			switch (businessContext)
			{
				case BusinessContext.Quotation:

					AddQuotationToCollection(resultList, opportunity.RelatedChildActivityPivotCollection);
					resultList.Sort((x, y) => GetRatingHeader(x).TH_QuoteNumber.CompareTo(GetRatingHeader(y).TH_QuoteNumber));
					break;
				case BusinessContext.Organisation:
					IDocumentSupportable bizO = Factory.Load<OrgHeader>(opportunity.P8_OH);
					if (bizO != null)
					{
						resultList.Add(bizO);
					}
					break;
			}
			return resultList.ToArray();
		}

		void AddQuotationToCollection(List<IDocumentSupportable> collection, IRelatedChildActivityPivotCollection pivots)
		{
			foreach (var pivot in pivots)
			{
				if (pivot.RAP_ChildActivityTableCode.Equals(RatingHeaderSchema.Constants.Prefix) || pivot.RAP_ChildActivityTableCode.Equals(ViewQuotedBookingSchema.Constants.Prefix))
				{
					var bizO = (IDocumentSupportable)Factory.Load(pivot.RAP_ChildActivityTableCode, pivot.RAP_ChildActivityID);
					if (bizO != null)
					{
						var quotationCancelled = bizO is ICancellable cancellable && cancellable.IsCancelled;
						if (!quotationCancelled && CheckLoginCompanyMatches(GetRatingHeader(bizO)))
						{
							collection.Add(bizO);
						}
					}
				}
				if (pivot.ChildActivity != null)
				{
					AddQuotationToCollection(collection, pivot.ChildActivity.RelatedChildActivityPivotCollection);
				}
			}
		}

		static IRatingHeader GetRatingHeader(IDocumentSupportable businessObject)
		{
			if (businessObject is IRatingHeader ratingHeader)
			{
				return ratingHeader;
			}
			else if (businessObject is IQuotedBooking quotedBooking && quotedBooking.Quote is IRatingHeader quotedBookingRatingHeader)
			{
				return quotedBookingRatingHeader;
			}
			else
			{
				return null;
			}
		}

		static bool CheckLoginCompanyMatches(IRatingHeader rating)
		{
			if (rating != null)
			{
				if (rating.TH_GC != Env.CurrentCompany.PK)
				{
					return false;
				}
			}
			return true;
		}

		public new OrgOpportunity BusinessObject
		{
			get { return (OrgOpportunity)base.BusinessObject; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			if (BusinessObject == null)
			{
				return base.GetContactOrganisation(menuName, contactType, direction);
			}

			if (contactType == ContactType.ControllingAgent)
			{
				return BusinessObject.AssignedOrg != null
					? new OrgHeaderContact(BusinessObject.AssignedOrg, null)
					: base.GetContactOrganisation(menuName, contactType, direction);
			}

			return BusinessObject.ClientContact != null && BusinessObject.ClientContact.ParentOrg != null
				? new OrgHeaderContact(BusinessObject.ClientContact.ParentOrg, null)
				: base.GetContactOrganisation(menuName, contactType, direction);
		}
	}
}
