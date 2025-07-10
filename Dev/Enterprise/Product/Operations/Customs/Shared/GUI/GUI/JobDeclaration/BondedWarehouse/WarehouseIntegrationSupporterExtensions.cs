using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IWarehouseIntegrationSupporter = Enterprise.Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter;

namespace Enterprise.Customs.GUI.WarehouseExtensions
{
	public static class WarehouseIntegrationSupporterExtensions
	{
		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This definitely should not be an event")]
		public static ContinueWithSave FireSaveButton(this Control control)
		{
			return GetParentForm(control)?.FireSaveButton() ?? ContinueWithSave.No;
		}

		static ZForm GetParentForm(Control control)
		{
			ZForm result = null;
			while (control != null)
			{
				result = control as ZForm;
				if (result != null)
				{
					break;
				}
				control = control.Parent;
			}
			return result;
		}

		public static void CancelBondedWarehouseChangeOfOwnership(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishCancelEventForWHSChangeOfOwnershipAndSaveIfNeeded(true, true);
				if (result != null)
				{
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Globals.Message.ShowError(result.ErrorMessage, Res.GetString("{A20FCE7E-1C3A-41DF-82BF-40CE681CD7BE}", "Cannot Cancel Change Of Ownership"));
					}
					else
					{
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						Globals.Message.ShowInformation(Res.GetString("{AC2437CE-9124-4DE2-AA49-1ADBFE2E473E}", "Change Of Ownership has been canceled.{0}", warehouseJob == null ? "" : Res.GetString("f9b3f12f-8a24-4511-bb95-8f4a6aee3d75", " (WHS Change Of Ownership:{0})", warehouseJob.JobNumber)));
					}
				}
			}
		}

		public static void CancelInventoryChangeOfRegime(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishCancelEventForWHSChangeOfRegimeAndSaveIfNeeded(true, true);
				if (result != null)
				{
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Globals.Message.ShowError(result.ErrorMessage, Res.GetString("{A6BC50D5-5AA8-4DE1-A3F5-60EEA0F6838A}", "Cannot Cancel Inventory Change Of Regime"));
					}
					else
					{
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						Globals.Message.ShowInformation(Res.GetString("{0E84898A-A773-40E6-A57D-04DB26A63478}", "Inventory Change Of Regime has been canceled.{0}", warehouseJob == null ? "" : Res.GetString("674f56a3-3315-4e82-80f4-c4a51d6e3913", " (WHS Change Of Regime:{0})", warehouseJob.JobNumber)));
					}
				}
			}
		}

		public static void CancelBondedWarehouseOutward(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishCancelEventForWHSOutwardAndSaveIfNeeded(true, true);
				if (result != null)
				{
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Globals.Message.ShowError(result.ErrorMessage, Res.GetString("AE56A2F5-DD04-4753-AD53-C3DB9D41F362", "Cannot Cancel Stock Release"));
					}
					else
					{
						supporter.PostCancelBondedWarehouseOutwardAction();
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						Globals.Message.ShowInformation(Res.GetString("0BD2143B-3A83-42F1-82AF-503769A66876", "Stock Release has been canceled.{0}", warehouseJob == null ? "" : Res.GetString("64702759-00ed-4cf1-9895-e347ede710b3", " (WHS Order:{0})", warehouseJob.JobNumber)));
					}
				}
			}
		}

		public static void CancelBondedWarehouseInward(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, isManualWhsUpdate: true);
				if (result != null)
				{
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Globals.Message.ShowError(result.ErrorMessage, Res.GetString("74E52FDC-F8EC-494A-9DDE-38834318A7FD", "Cannot Cancel Stock Levels"));
					}
					else
					{
						supporter.PostCancelBondedWarehouseInwardAction();
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						Globals.Message.ShowInformation(Res.GetString("31971f57-bf04-402a-a43b-3bce2d1e95d5", "Stock Levels have been canceled.{0}", warehouseJob == null ? "" : Res.GetString("8d5dfdca-e6c5-4509-b331-2683f0441c68", " (WHS Receipt:{0})", warehouseJob.JobNumber)));
					}
				}
			}
		}

		public static void UpdateBondedWarehouseChangeOfOwnership(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishShipmentForWHSChangeOfOwnership(false, true);
				if (result != null && supporter.MessageInitiator.IsPublishToUniversalTransactionOK(result))
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					Globals.Message.Show(Res.GetString("{2C455C4E-84EE-4598-9117-64BD4C3CE412}", "Change Of Ownership has been updated.{0}", warehouseJob == null ? "" : Res.GetString("f9b3f12f-8a24-4511-bb95-8f4a6aee3d75", " (WHS Change Of Ownership:{0})", warehouseJob.JobNumber)));
				}
			}
		}

		public static void UpdateInventoryChangeOfRegime(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishShipmentForWHSChangeOfRegime(false, true);
				if (result != null && supporter.MessageInitiator.IsPublishToUniversalTransactionOK(result))
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					Globals.Message.Show(Res.GetString("{FB5FC4A2-5D13-48D2-987F-43662D6117A2}", "Inventory Change Of Regime has been updated.{0}", warehouseJob == null ? "" : Res.GetString("674f56a3-3315-4e82-80f4-c4a51d6e3913", " (WHS Change Of Regime:{0})", warehouseJob.JobNumber)));
				}
			}
		}

		public static void UpdateBondedWarehouseOutward(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishShipmentForWHSOutward(true);
				if (result != null && supporter.MessageInitiator.IsPublishToUniversalTransactionOK(result))
				{
					var warehouseJob = result.FindJobIfExists() as IRelatedJob;
					supporter.PublishAcceptEventForWHSOutwardAndSaveIfNeeded(true, true);
					supporter.PostUpdateBondedWarehouseOutwardAction();
					Globals.Message.Show(Res.GetString("6ddd9e71-53a7-4886-9592-659e9c49d8a8", "Stock Release has been updated.{0}", warehouseJob == null ? "" : Res.GetString("64702759-00ed-4cf1-9895-e347ede710b3", " (WHS Order:{0})", warehouseJob.JobNumber)));
				}
			}
		}

		public static void UpdateBondedWarehouseInward(this IWarehouseIntegrationSupporter supporter)
		{
			if (supporter != null)
			{
				var result = supporter.PublishShipmentForWHSInward(false);
				if (result != null)
				{
					if (result.ResultType == UniversalResult.HadErrors)
					{
						Globals.Message.ShowError(result.ErrorMessage, Res.GetString("FED9BE68-AFD1-4A72-BE29-B80B318D3A80", "Cannot Update Stock Levels."));
					}
					else
					{
						var warehouseJob = result.FindJobIfExists() as IRelatedJob;
						supporter.PublishAcceptEventForWHSInwardAndSaveIfNeeded(true, true);
						supporter.PostUpdateBondedWarehouseInwardAction();
						Globals.Message.ShowInformation(Res.GetString("2e92935a-c3ed-49a9-8ee3-08e49f0f025e", "Stock Levels have been updated.{0}", warehouseJob == null ? "" : Res.GetString("8d5dfdca-e6c5-4509-b331-2683f0441c68", " (WHS Receipt:{0})", warehouseJob.JobNumber)));
					}
				}
			}
		}
	}
}
