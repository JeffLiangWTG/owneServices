using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Customs.US.LVS.GUI
{
	public static class ConvertToStandAloneDeclarationHelper
	{
		public static ZMenuItem CreateConvertToStandAloneDeclarationMenuItem(EventHandler convertToStandAloneDeclaration)
		{
			return new ZMenuItem(ConvertToStandAloneDeclarationCaption, convertToStandAloneDeclaration) { Name = "menuItemConvertToStandAloneDeclaration" };
		}

		public static ZMenuItem ManualSelection(EventHandler convertToStandAloneDeclaration)
		{
			return new ZMenuItem(ManualSelectionCaption, convertToStandAloneDeclaration) { Name = "manualSelection" };
		}

		public static ZMenuItem WithMessageErrors(EventHandler convertToStandAloneDeclaration)
		{
			return new ZMenuItem(WithMessageErrorsCaption, convertToStandAloneDeclaration) { Name = "withMessageErrors" };
		}

		public static ZMenuItem WithPGARequirements(EventHandler convertToStandAloneDeclaration)
		{
			return new ZMenuItem(WithPGARequirementsCaption, convertToStandAloneDeclaration) { Name = "withPGARequirements" };
		}

		public static ZMenuItem NavigateToConvertToStandAloneDeclarationMenuItem()
		{
			return new ZMenuItem(ConvertToStandAloneDeclarationCaption);
		}

		public static bool SetMenuItemVisible(ZGrid grid)
		{
			return grid.SelectedElements.Length > 0 &&
				grid.SelectedElements.Any(row =>
					(row is CusUSLVConsignment consignment && consignment.CanBeConvertedToStandaloneDeclaration) ||
					(row is USConsignmentCombined view && view.IsConsignment && view.Consignment.CanBeConvertedToStandaloneDeclaration));
		}

		#region Convert to Stand Alone Declaration

		public static void ConvertToStandAloneDeclarationCombined(BusinessObjectFactory factory, BusinessObject[] selectedElements, Logs clearanceLog)
		{
			ConvertToStandAloneDeclarationCore(factory, selectedElements.Cast<CusUSLVConsignment>(), clearanceLog, true, AddCombinedLogAndSetConvertAction);
		}

		public static void ConvertToStandAloneDeclarationIndividual(BusinessObjectFactory factory, BusinessObject[] selectedElements, Logs clearanceLog, bool displayConvertMessage)
		{
			var selectedConsignmentRows = selectedElements
				.Where(row => row is USConsignmentCombined view && view.IsConsignment).Select(row => ((USConsignmentCombined)row).Consignment)
				.Union(selectedElements.OfType<CusUSLVConsignment>());

			ConvertToStandAloneDeclarationCore(factory, selectedConsignmentRows, clearanceLog, displayConvertMessage, AddIndividualLog);
		}

		static void ConvertToStandAloneDeclarationCore(BusinessObjectFactory factory, IEnumerable<CusUSLVConsignment> selectedConsignmentRows, Logs clearanceLog, bool displayConvertMessage, Action<Logs, CusUSLVConsignment> addLogAction)
		{
			var numberOfConsignmentsQueued = ZInt.Zero;
			if (CheckIfConvertToFormalDeclarationAllowed())
			{
				var totalSelectedCount = selectedConsignmentRows.Count();
				if (totalSelectedCount > 0)
				{
					var convertableConsignments = selectedConsignmentRows.Where(c => c.CanBeConvertedToStandaloneDeclaration);
					var isOkToContinue = true;
					if (displayConvertMessage)
					{
						var convertableCount = convertableConsignments.Count();
						var alreadyConvertedCount = totalSelectedCount - convertableCount;

						var messageContent = Res.GetString("2fa5fcb0-2699-463e-86ff-b380362841d7",
	@"{0} Consignment(s) have been selected
{1} Consignment(s) have previously been converted and will be ignored
{2} Consignment(s) will be converted to a Stand Alone Declaration",
						totalSelectedCount,
						alreadyConvertedCount,
						convertableCount);

						isOkToContinue = Globals.Message.Show(messageContent, ConvertToStandAloneDeclarationCaption, MessageBoxButtons.OKCancel, DialogResult.OK) == DialogResult.OK;
					}

					if (isOkToContinue)
					{
						foreach (var consignment in convertableConsignments)
						{
							var shipmentLog = clearanceLog ?? consignment.Shipment.Logs;
							addLogAction.Invoke(shipmentLog, consignment);
							consignment.SetReadOnlyIncludingChildren(true);
							consignment.ULB_IsActive = false;
							numberOfConsignmentsQueued++;
						}
					}

					if (numberOfConsignmentsQueued > 0)
					{
						try
						{
							factory.Save();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							numberOfConsignmentsQueued = ZInt.Zero;
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}

			if (numberOfConsignmentsQueued > 0)
			{
				Globals.Message.ShowInformation(Res.GetString("6b5bd9ad-4ef0-49c7-b468-cbb9e1475653", "{0} Stand Alone Declarations queued for processing, check the individual consignment for status", numberOfConsignmentsQueued));
			}
		}

		static void AddIndividualLog(Logs shipmentLog, CusUSLVConsignment consignment)
		{
			shipmentLog.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.ReferenceNumber, consignment.PK.ToString()));
		}

		static void AddCombinedLogAndSetConvertAction(Logs shipmentLog, CusUSLVConsignment consignment)
		{
			var combinedLogs = shipmentLog.Find(l => l.SL_SE_NKEvent == AutoEvents.TransferToCustomsImportsDecCode);
			var hasCombinedLog = combinedLogs.Any(l => l.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.Reason, out var reason) && reason == LVSConstants.ConvertToDeclarationReason.CombineConsignments);
			if (!hasCombinedLog)
			{
				shipmentLog.AddNew(AutoEvents.TransferToCustomsImportsDec, new KeyValuePair<string, string>(EventConstants.EventReferenceParameters.Codes.Reason, LVSConstants.ConvertToDeclarationReason.CombineConsignments));
			}

			consignment.ULB_ConvertAction = ULBConvertActionList.Codes.Combined;
		}

		static bool CheckIfConvertToFormalDeclarationAllowed()
		{
			bool result = true;
			if (!Env.Security.USLVClearanceConvertToFormalDeclaration.IsAllowed)
			{
				result = false;
				Env.Security.USLVClearanceConvertToFormalDeclaration.ShowError();
			}

			return result;
		}

		#endregion

		static string ConvertToStandAloneDeclarationCaption => Res.GetString("5f8b7e8f-3381-447c-b182-46cdb2a25db1", "Convert to Stand Alone Declaration");
		static string ManualSelectionCaption => Res.GetString("81df2b76-afa2-4fa7-8c45-7f585b1c9706", "Manual Selection");
		static string WithMessageErrorsCaption => Res.GetString("e8066cf2-b255-4cc6-98fa-cd8d873ab360", "With Message Errors");
		static string WithPGARequirementsCaption => Res.GetString("3ceeec53-a4a9-4a22-a43f-e04dccb1e2f8", "With PGA Requirements");
		public static string SaveFormFirstPrompt => Res.GetString("c2b7ec59-ac83-468c-9594-66ccaf3f774f", "Please save the form before converting.");
	}
}
