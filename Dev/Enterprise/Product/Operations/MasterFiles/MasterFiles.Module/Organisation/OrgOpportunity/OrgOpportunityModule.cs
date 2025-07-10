using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class OrgOpportunityModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public OrgOpportunityModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.Opportunity;
			}
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.OpportunityWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Opportunity);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrgOpportunityFilterControl(GridCollection, (OrgOpportunityFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgOpportunityCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrgOpportunityFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OpportunityManagement; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.RelationshipOpportunityManager; }
		}

		#endregion

		#region Additional Menu

		const string CopyMenuItemTag = "CopyMenuItemTag";

		protected override MenuItem[] GetNewAdditionalMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewAdditionalMenuItems());
			result.InsertRange(result.Count - 1,
				new[]
				{
					new ZMenuItem(ResString.GetMultilingualString("b92ee584-6d05-4639-bef3-ba31e250c0a5", "Copy"), OnCopyMenuItemClick) { Tag = CopyMenuItemTag }
				});

			return result.ToArray();
		}

		protected override void SetupButtonDetailForItem(MenuItem item, ref IconTypes buttonImage, ref IconTypes buttonImageActive, ref string buttonToolTip)
		{
			base.SetupButtonDetailForItem(item, ref buttonImage, ref buttonImageActive, ref buttonToolTip);

			if (item.Tag == (object)CopyMenuItemTag)
			{
				buttonImage = IconTypes.EditButtonRest;
				buttonImageActive = IconTypes.EditButtonActive;
				buttonToolTip = Res.GetString("68b1ebb7-0135-4974-bfb0-44c6cccd251f", "Copy selected opportunity");
			}
		}

		void OnCopyMenuItemClick(object sender, EventArgs e)
		{
			var selectedRows = Grid.GetSelectedRows().OfType<OrgOpportunity>().ToArray();
			if (selectedRows.Length == 1)
			{
				var factory = new BusinessObjectFactory();
				var sourceOpportunity = factory.Load<OrgOpportunity>(selectedRows[0].PK);
				var targetOpportunity = factory.New<OrgOpportunity>();

				targetOpportunity.P8_OpportunityDescription = sourceOpportunity.P8_OpportunityDescription;
				targetOpportunity.P8_PackageType = sourceOpportunity.P8_PackageType;
				targetOpportunity.P8_OpportunityType = sourceOpportunity.P8_OpportunityType;
				targetOpportunity.P8_DiscountAmount = sourceOpportunity.P8_DiscountAmount;
				targetOpportunity.P8_RentalMultiplier = sourceOpportunity.P8_RentalMultiplier;
				targetOpportunity.P8_OH = sourceOpportunity.P8_OH;
				targetOpportunity.P8_OC = sourceOpportunity.P8_OC;
				targetOpportunity.P8_GS_NKPrimarySalesPerson = sourceOpportunity.P8_GS_NKPrimarySalesPerson;
				targetOpportunity.P8_OA_AssignedOffice = sourceOpportunity.P8_OA_AssignedOffice;
				targetOpportunity.P8_OC_AssignedOfficeContact = sourceOpportunity.P8_OC_AssignedOfficeContact;
				targetOpportunity.P8_Source = sourceOpportunity.P8_Source;
				targetOpportunity.P8_SourceDetails = sourceOpportunity.P8_SourceDetails;
				targetOpportunity.P8_OH_ReferringOrganisation = sourceOpportunity.P8_OH_ReferringOrganisation;
				targetOpportunity.P8_OC_ReferringContact = sourceOpportunity.P8_OC_ReferringContact;
				targetOpportunity.OnCopyOrgOpportunity(sourceOpportunity);

				var targetOppForm = GetNewController(null).ShowFormForNewEntity(targetOpportunity);
				((ZForm)targetOppForm).Shown += (s, ev) =>
				{
					ObjectFactory.Get<ITradeDetailClonerGUIManager>().ShowForm(sourceOpportunity, targetOpportunity);
				};
			}
			else
			{
				Globals.Message.Show(Res.GetString("653c8bc4-3647-48b0-8816-965c51c94d2f", "Please select one opportunity to copy."));
			}
		}

		#endregion

		#region Actions Menu

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (!OrganisationsDataRegistry.Instance.DivideAllOpportunityValuesByTwelveHasRun.Value)
			{
				divideAllLegacyValuesByTwelveMenuItem = new ZMenuItem(ResString.GetMultilingualString("6BCD4E6A-5669-4FBA-8E7C-A7818370788A", "Divide all Legacy Values by 12 (One-off)"), OnDivideAllLegacyEstimatedValuesByTwelveClick);
				result.Add(divideAllLegacyValuesByTwelveMenuItem);
			}
			result.Add(new ZMenuItem(ResString.GetMultilingualString("21CCB066-4E73-49E9-9EC9-6443E85F632F", "Bulk update Exchange Rate Date"), OnBulkUpdateExchangeRateDateClick));

			return result.ToArray();
		}

		#region DivideAllLegacyEstimatedValuesByTwelve

		void OnDivideAllLegacyEstimatedValuesByTwelveClick(object sender, EventArgs args)
		{
			new DivideAllLegacyEstimatedValuesByTwelveController().Show();

			if (OrganisationsDataRegistry.Instance.DivideAllOpportunityValuesByTwelveHasRun.Value)
			{
				ActionsMenuItem.MenuItems.Remove(divideAllLegacyValuesByTwelveMenuItem);
			}
		}

		ZMenuItem divideAllLegacyValuesByTwelveMenuItem;

		#endregion

		#region BulkUpdateExchangeRateDate

		void OnBulkUpdateExchangeRateDateClick(object sender, EventArgs args)
		{
			var updateAction = GetNewUpdateAction();
			if (updateAction != null)
			{
				var form = new BulkUpdateOpportunityDateForExchangeRateForm(updateAction);
				ZFormModaliser.Show(form, ParentModalForm);
			}
		}

		BulkUpdateP8_DateForExchangeRateAction GetNewUpdateAction()
		{
			var selectedRows = Grid.GetSelectedRows().OfType<OrgOpportunity>().ToArray();
			if (selectedRows.Length > 0)
			{
				return new BulkUpdateP8_DateForExchangeRateAction(selectedRows);
			}

			var allElements = GridCollection.OfType<OrgOpportunity>().ToArray();
			if (allElements.Length > 0)
			{
				return new BulkUpdateP8_DateForExchangeRateAction(allElements);
			}

			Globals.Message.ShowError(Res.GetString("C1314AEB-645B-41FB-8856-D237BC2EC57C", "No opportunities have been selected. Please filter for the opportunities that you wish to update exchange rate date for."));
			return null;
		}

		#endregion

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new OrgOpportunityActionSupporter(); }
		}

		#endregion
	}
}
