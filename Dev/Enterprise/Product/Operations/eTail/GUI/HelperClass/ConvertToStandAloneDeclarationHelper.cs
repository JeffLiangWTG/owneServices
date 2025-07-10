using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer;
using Enterprise.eTail.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ConsignmentAddressType = Enterprise.Integration.Customs.ConsignmentAddressType;

namespace Enterprise.eTail.GUI
{
	public class ConvertToStandAloneDeclarationHelper
	{
		#region Convert a Single Consignment

		public static void ConvertToStandAloneDeclaration(HVLVConsignment consignment)
		{
			if (consignment != null)
			{
				var response = default(IConvertToStandAloneDeclarationResponse);
				var helper = new ConvertToStandAloneDeclarationHelper();
				helper.ShouldShowJobFormWhenConversionCompleted = true;

				var anyChanges = (consignment.BookingHeader?.HasChanges).GetValueOrDefault() ||
					(consignment.ManifestedOnShipment?.HasChanges).GetValueOrDefault() ||
					(consignment.ManifestedOnShipment?.ArrivalConsol?.HasChanges).GetValueOrDefault();

				if (anyChanges)
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("c6cb0486-e469-4528-bbf1-1f69b3cdb2db", "Please save any changes made before creating Stand Alone Declaration"));
				}
				else if (consignment.ReleaseStatusForCurrentDirection == HVLVReleaseStatus.Cleared)
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("cfd0998b-b16f-4726-8b52-00d3cf079805", "A Standalone Declaration cannot be created for a Consignment with Release Status as 'Cleared'"));
				}
				else if (ConsolNotAttached(consignment))
				{
					Globals.Message.ShowError(ConvertToStandAloneDeclarationResponse.ErrorMessages.ConsignmentMissingTransportDetails);
				}
				else if (PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist(consignment))
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("aef8db5e-68dd-4c50-867a-91715e895e88", "New organization(s) have been linked to this consignment. Please save any changes and retry creating a Stand Alone Declaration"));
				}
				else if (GetUnclearedConsignments(consignment).Count > 0)
				{
					response = MergeAndConvertConsignmentsToDeclaration(consignment, helper);
				}
				else
				{
					if (CheckShipmentRequiresWaybillValidation(consignment))
					{
						response = helper.ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(consignment);
					}
				}

				if (response != null && !response.ConversionSucceeded && !string.IsNullOrEmpty(response.ConversionFailureReason))
				{
					Globals.Message.ShowError(response.ConversionFailureReason);
				}
			}
		}

		static IConvertToStandAloneDeclarationResponse MergeAndConvertConsignmentsToDeclaration(HVLVConsignment consignment, ConvertToStandAloneDeclarationHelper helper)
		{
			var response = default(IConvertToStandAloneDeclarationResponse);
			using (var mergeDeclarationForm = new HVLVSelectConsignmentForm(GetUnclearedConsignments(consignment)))
			{
				ZFormModaliser.ShowDialogWithoutDispose(mergeDeclarationForm);
				if (mergeDeclarationForm.DialogResult == DialogResult.OK)
				{
					response = helper.ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(consignment, mergeDeclarationForm.SelectedConsignments);
				}
				else
				{
					var message = Res.GetString("f8229e08-ac1c-4b36-a389-f39986b5c2c9", "Merge has been canceled, do you wish to continue converting Consignment {0} to a stand alone declaration?", consignment.HVC_ConsignmentId);
					var caption = Res.GetString("9802b473-d3cb-4043-9ef3-96cc255921a3", "Ignore Merge Declaration");
					var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (dialogResult == DialogResult.Yes)
					{
						response = helper.ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(consignment);
					}
				}
			}

			return response;
		}

		static bool ConsolNotAttached(HVLVConsignment consignment)
		{
			return consignment.ManifestedOnShipment != null && consignment.ManifestedOnShipment.ArrivalConsol == null;
		}

		static HVLVCommonConsigneeConsignmentCollection GetUnclearedConsignments(HVLVConsignment consignment)
		{
			var filteredConsignments = consignment.ConsignmentsBelongToSameConsigneeExcludingParent;
			var query = new ZQuery();
			if (consignment.IsImport)
			{
				query.AddToFilter(HVLVConsignmentSchema.HVC_ImportReleaseStatus, SQLComparisonOperator.NotEqual, HVLVReleaseStatus.Cleared);
			}
			else if (consignment.IsExport)
			{
				query.AddToFilter(HVLVConsignmentSchema.HVC_ExportReleaseStatus, SQLComparisonOperator.NotEqual, HVLVReleaseStatus.Cleared);
			}

			filteredConsignments.LoadWithMoreFiltering(query);
			return filteredConsignments;
		}

		static bool CheckShipmentRequiresWaybillValidation(HVLVConsignment consignment)
		{
			return !consignment.ManifestedOnShipment.IsShipmentDestinationUS()
				|| UserPromptCheckingHelper.US.CheckWaybill(consignment, Res.GetString("42ef19a6-98de-4d41-9f61-6842149de5e9", "Stand Alone Declaration"));
		}

		static bool PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist(HVLVConsignment consignment)
		{
			var hasCreatedNewOrg = false;
			if (!consignment.ConsigneeIsOrganisation && !consignment.ShipperIsOrganisation)
			{
				var promptResult = Globals.Message.Show(BothOrgsNotFoundPrompt,
					OrgNotFoundCaption,
					MessageBoxButtons.YesNo,
					DialogResult.Yes);
				if (promptResult == DialogResult.Yes)
				{
					var createdConsignee = HVLVSimilarAddressSelectionForm.Convert(consignment, ConsignmentAddressType.Consignee);
					var createdShipper = HVLVSimilarAddressSelectionForm.Convert(consignment, ConsignmentAddressType.Shipper);
					hasCreatedNewOrg = createdConsignee || createdShipper;
				}
			}
			else if (!consignment.ConsigneeIsOrganisation)
			{
				var promptResult = Globals.Message.Show(ConsigneeOrgNotFoundPrompt,
					OrgNotFoundCaption,
					MessageBoxButtons.YesNo,
					DialogResult.Yes);
				if (promptResult == DialogResult.Yes)
				{
					hasCreatedNewOrg = HVLVSimilarAddressSelectionForm.Convert(consignment, ConsignmentAddressType.Consignee);
				}
			}
			else if (!consignment.ShipperIsOrganisation)
			{
				var promptResult = Globals.Message.Show(ShipperOrgNotFoundPrompt,
					OrgNotFoundCaption,
					MessageBoxButtons.YesNo,
					DialogResult.Yes);
				if (promptResult == DialogResult.Yes)
				{
					hasCreatedNewOrg = HVLVSimilarAddressSelectionForm.Convert(consignment, ConsignmentAddressType.Shipper);
				}
			}

			return hasCreatedNewOrg;
		}

		#endregion

		#region Convert Multiple Consignments

		public bool ConvertToStandAloneDeclarations(IEnumerable<HVLVConsignment> consignments, Action<string, string, int> progressUpdateCallback, CancellationTokenSource cancellation)
		{
			if (consignments.IsNullOrEmpty())
			{
				return true;
			}

			var result = true;
			var tracker = new ConvertTracker(consignments.Count(), cancellation.Token, progressUpdateCallback);
			tracker.UpdateCaption(LoadingConsignmentCaption);
			var factory = consignments.First().Factory;

			try
			{
				DataObjectCacheFactoryService.GetOrCreateNewInstance(factory);
				foreach (var consignment in consignments)
				{
					ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(consignment);
					tracker.UpdateLoadingProgress(consignment);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				result = false;
			}
			finally
			{
				DataObjectCacheFactoryService.DisposeInstance(factory);
			}

			return result;
		}

		public void MergeAndConvertToStandAloneDeclarations(IEnumerable<HVLVConsignment> selectedConsignments)
		{
			if (selectedConsignments.IsNullOrEmpty())
			{
				return;
			}

			var factory = selectedConsignments.First().Factory;

			try
			{
				DataObjectCacheFactoryService.GetOrCreateNewInstance(factory);

				var consignmentGroupsByConsignee = selectedConsignments
					.GroupBy(c => new
					{
						c.HVC_ConsigneeName,
						c.HVC_ConsigneeAddress1,
						c.HVC_ConsigneeAddress2,
						c.HVC_ConsigneePostcode,
						c.HVC_ConsigneeCity,
						c.HVC_ConsigneeState,
						c.HVC_RN_NKConsigneeCountryCode
					});

				foreach (var consignments in consignmentGroupsByConsignee)
				{
					var parentConsignment = consignments.First();
					var relatedConsignments = consignments.Skip(1);
					ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(parentConsignment, relatedConsignments);
				}
			}
			finally
			{
				DataObjectCacheFactoryService.DisposeInstance(factory);
			}
		}

		public static bool PromptToMatchOrCreateConsigneeAndShipperOrgsIfNotExist()
		{
			return DialogResult.Yes == Globals.Message.Show(OrgsNotFoundPrompt, OrgsNotFoundCaption, MessageBoxButtons.YesNo, DialogResult.Yes);
		}

		public static bool AllShippersAndConsigneesAreOrgs(IEnumerable<HVLVConsignment> consignments)
		{
			return consignments.All(c =>
				c.ConsigneeIsOrganisation
				&& c.ShipperIsOrganisation);
		}

		#endregion

		static ResourceString LoadingConsignmentCaption => ResString.GetMultilingualString("73dd88fb-8234-43df-afe5-7e671348d7eb", "Loading HVLV Consignment data");
		static ResourceString OrgNotFoundCaption => ResString.GetMultilingualString("dfb5dec7-812e-4692-b75c-fe83bdbfdb28", "Organization not found");
		static ResourceString BothOrgsNotFoundPrompt => ResString.GetMultilingualString("06032821-736e-4a89-9c00-b6bae39b941b", "Consignee and Shipper organizations not found, would you like to convert to organizations with details defaulted from 'Consignee' and 'Shipper' fields?");
		static ResourceString ConsigneeOrgNotFoundPrompt => ResString.GetMultilingualString("bb8a3943-b5f5-4df0-9d63-6e9713a9df16", "Consignee organization not found, would you like to convert to organization with details defaulted from 'Consignee' fields?");
		static ResourceString ShipperOrgNotFoundPrompt => ResString.GetMultilingualString("4931597d-9473-4659-bd00-45c077eb7680", "Shipper organization not found, would you like to convert to organization with details defaulted from 'Shipper' fields?");
		static ResourceString OrgsNotFoundPrompt => ResString.GetMultilingualString("ea410b88-42f0-4a77-8844-1062b3ce6b6e", "Consignee or Shipper organization not found for at least one consignment selected, would you like to convert them into organizations?");
		static ResourceString OrgsNotFoundCaption => ResString.GetMultilingualString("7fde9dfe-b1a8-4530-84f1-0f42f1d6e6ea", "Organizations not found");

		internal static bool ShouldShowConvertToStandAloneDeclarationMenuItem(HVLVConsignment consignment)
		{
			return consignment != null &&
				consignment.ManifestedOnShipment != null &&
				consignment.CanConvertToStandAloneDeclaration;
		}

		ConvertToStandAloneDeclarationService ConvertToStandAloneDeclarationService
		{
			get
			{
				if (convertToStandAloneDeclarationService == null)
				{
					convertToStandAloneDeclarationService = new ConvertToStandAloneDeclarationService();

					if (ShouldShowJobFormWhenConversionCompleted)
					{
						convertToStandAloneDeclarationService.Completed += ConvertToStandAloneDeclarationService_Completed;
					}
				}

				return convertToStandAloneDeclarationService;
			}
		}

		ZController DeclarationController
		{
			get
			{
				if (declarationController == null)
				{
					declarationController = ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
					declarationController.ShowChildrenAsDialog = true;
				}

				return declarationController;
			}
		}

		void ConvertToStandAloneDeclarationService_Completed(object sender, CancelEventArgs e)
		{
			if (sender is BaseJobDeclaration declaration)
			{
				if (declaration.IsInDatabase)
				{
					if (DialogResult.Yes == Globals.Message.Show(Res.GetString("7b460a41-450f-4a2f-bded-bb1881595b10", "An existing Stand Alone Declaration {0} is found containing matching details, proceeding will link this Declaration to the Consignment, would you like to continue?", declaration.JE_DeclarationReference),
						Res.GetString("6ee016c2-8ff3-4add-909f-0147599d8c95", "Existing Stand Alone Declaration Found"),
						MessageBoxButtons.YesNo,
						DialogResult.No))
					{
						declaration.Factory.Save();
						LoadDeclarationChildrenForDisplaying(declaration);
						DeclarationController.ShowFormOfGivenDisplayType(declaration, ODisplayMode.Edit);
					}
					else
					{
						e.Cancel = true;
					}
				}
				else
				{
					LoadDeclarationChildrenForDisplaying(declaration);
					DeclarationController.ShowFormOfGivenDisplayType(declaration, ODisplayMode.New);

					if (!declaration.IsInDatabase)
					{
						e.Cancel = true;
					}
				}
			}
		}

		void LoadDeclarationChildrenForDisplaying(BaseJobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.PackingGroups.Load();
				declaration.Packages.Load();
				declaration.Bills.Load();
				declaration.RunPreSaveValidation();
			}
		}

		bool ShouldShowJobFormWhenConversionCompleted;
		ConvertToStandAloneDeclarationService convertToStandAloneDeclarationService;
		ZController declarationController;
	}
}
