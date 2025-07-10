using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public class SalesDashboardController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public SalesDashboardController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.SalesDashboard; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesDashboard; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(SalesDashboardActivity); }
		}

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new NotSupportedException("Can not invoke GetForm() method, use ShowViewForm(), ShowEditForm(), ShowNewForm() or ShowDeleteForm() instead");
		}

		Tuple<ControllerID, BusinessObject> GetControllerAndBusinessObject(BusinessObject sourceEntity)
		{
			var activity = (SalesDashboardActivity)sourceEntity;
			switch (activity.VSA_ActivityType)
			{
				case SalesDashboardActivityTypeCodeList.Codes.Opportunity:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.Opportunity, ((OpportunitySalesDashboardActivity)activity).ParentOpportunity);
				case SalesDashboardActivityTypeCodeList.Codes.Inquiry:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.SalesEnquiry, ((InquirySalesDashboardActivity)activity).ParentInquiry);
				case SalesDashboardActivityTypeCodeList.Codes.Communication:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.Communication, ((CommunicationSalesDashboardActivity)activity).SalesCall);
				case SalesDashboardActivityTypeCodeList.Codes.Campaign:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.GlbCompanyCampaign, ((CampaignSalesDashboardActivity)activity).ParentCampaign);
				case SalesDashboardActivityTypeCodeList.Codes.OneOffQuote:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.OneOffQuotes, ((OneOffQuoteSalesDashboardActivity)activity).ParentQuote);
				case SalesDashboardActivityTypeCodeList.Codes.Quotation:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.Quotations, ((QuotationSalesDashboardActivity)activity).Parent);
				case SalesDashboardActivityTypeCodeList.Codes.Project:
					return new Tuple<ControllerID, BusinessObject>(ControllerIDs.Project, ((ProjectSalesDashboardActivity)activity).Parent);
				default:
					return null;
			}
		}

		public override IZForm ShowViewForm(BusinessObject sourceEntity)
		{
			var tuple = GetControllerAndBusinessObject(sourceEntity);
			
			if (tuple?.Item2 != null)
			{
				return LastShownForm = ZControllerFactory.Create(tuple.Item1).ShowViewForm(tuple.Item2);
			}

			ShowAlreadyDeletedOrIrreversiblyChangedMessage();

			return null;
		}

		public override IZForm ShowEditForm(BusinessObject sourceEntity)
		{
			var tuple = GetControllerAndBusinessObject(sourceEntity);

			if (tuple?.Item2 != null)
			{
				return LastShownForm = ZControllerFactory.Create(tuple.Item1).ShowEditForm(tuple.Item2);
			}

			ShowAlreadyDeletedOrIrreversiblyChangedMessage();

			return null;
		}

		public override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("e1f5742c-775c-41fe-9f79-1a2014dca196", "You are not allowed to add a new sales activity"));
			return null;
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			Globals.Message.Show(Res.GetString("0fad8552-05af-4ab1-b9ac-7a00d32a5ee4", "You are not allowed to delete a sales activity"));
			return null;
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			Globals.Message.Show(Res.GetString("6c9dae0a-4f05-4c8f-b246-d1cef58fddac", "You are not allowed to copy a sales activity"));
			return null;
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.SalesDashboardView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.SalesDashboardEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
