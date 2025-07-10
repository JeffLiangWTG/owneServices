using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class OneOffQuoteController : QuotedBookingController
	{
		#region Standard Controller Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OneOffQuotes; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.OneOffQuotes; }
		}

		#endregion

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			ZGuid quoteOnlyPK = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.NotApprovedAndNotAccepted).PK;
			return QuotedBooking.New(quoteOnlyPK, ZGuid.Empty, Factory);
		}

		#endregion

		#region Edit

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var viewQuotedBooking = sourceEntity as ViewQuotedBooking;
			if (viewQuotedBooking?.QuotedBooking?.Booking != null && !viewQuotedBooking.QuotedBooking.IsForwardRegistered)
			{
				return ShowViewForm(sourceEntity);
			}

			return base.ShowEditForm(sourceEntity);
		}

		#endregion

		#region Copy

		protected override IBusiness GetTemplateCopy(IBusiness loadedSourceEntity)
		{
			if (loadedSourceEntity is QuotedBooking quotedBooking)
			{
				return quotedBooking.TemplateCopyOnlyQuote();
			}

			return base.GetTemplateCopy(loadedSourceEntity);
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForView
		{
			get
			{
				return Env.Security.OneOffQuote;
			}
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.OneOffQuoteNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.OneOffQuoteEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.OneOffQuoteDelete; }
		}

		#endregion

		#region CRM Security

		protected override CRMSecurityProvider<ViewQuotedBooking> CRMSecurityProvider
		{
			get
			{
				return OneOffQuoteSecurityProvider;
			}
		}

		readonly OneOffQuoteCRMSecurityProvider OneOffQuoteSecurityProvider = new OneOffQuoteCRMSecurityProvider();

		public override SecurityCheckpoint GetCheckPointForView(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.View, base.GetCheckPointForView(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.Edit, base.GetCheckPointForEdit(bizObject));
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return CRMSecurityProvider.GetSecurityCheckpoint(bizObject as ViewQuotedBooking, FormAction.Delete, base.GetCheckPointForDelete(bizObject));
		}

		#endregion
	}
}
