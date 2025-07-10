using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.eTail.Integration.HVLVConstants;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.eTail.GUI
{
	public class CreateUSISFMenuItem : BaseHVLVMenuItem
	{
		public CreateUSISFMenuItem(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("a0afa84d-0b79-4ef5-b87e-06f119fc2eb1", "Create US Importer Security Filing"), shipment)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "app lock key")]
		public const string USISFAppLockKey = "HVLV US ISF";

		protected override Action MenuAction => () =>
		{
			if (UserPromptCheckingHelper.CheckUSSecurityFilingsEnabled()
				&& UserPromptCheckingHelper.CheckBusinessObjectHasNoChangesOrNotify(shipment, SaveFormBeforeCreateISF)
				&& UserPromptCheckingHelper.CheckHasAnyActiveConsignmentOrNotify(Header.ConsignmentsForBinding)
				&& PreValidateData())
			{
				if (!shipment.PK.TryAcquireApplicationLock<ForwardingShipment>(USISFAppLockKey,
				() => CreateISFCore(),
				out var errorMessage))
				{
					Globals.Message.ShowError(errorMessage);
				}
			}
		};

		void CreateISFCore()
		{
			using (var progressForm = new ProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = true;
				progressForm.CaptionResourceString = Res.GetData("78bbd06d-99a2-4401-b22a-94e0707150c4", "Creating US Importer Security Filing...");
				progressForm.ShowModalTo(Form);

				var newFactory = new BusinessObjectFactory();
				newFactory.Saving += NewFactoryOnSaving_SetBulkCopy;
				var shipmentInNewFactory = newFactory.Load<ForwardingShipment>(shipment.PK);

				using (shipmentInNewFactory.SuspendDeclarationForDocuments())
				{
					var creator = ObjectFactory.Get<IUSImporterSecurityFilingCreator>(nameof(IUSImporterSecurityFilingCreator), shipmentInNewFactory, (Action<string, int>)((message, progress) =>
					{
						if (!string.IsNullOrEmpty(message))
						{
							progressForm.SetStatusAndPercentComplete(message, progress);
						}
					}));

					var newISFHeaders = creator.CreateHeaders();

					progressForm.Close();
					if (newISFHeaders != null)
					{
						shipmentInNewFactory.SetSecurityFilingFirstUsageTimeForAllItems();
						shipmentInNewFactory.SetLastUsageCodeForAllItems(UsageCodes.USImporterSecurityFiling);

						if (newISFHeaders.Count == 1)
						{
							var isfHeader = newISFHeaders[0] as BusinessObject;
							var controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(isfHeader.GetType());
							controller.SetFormsModalTo(Form);
							controller.ShowFormOfGivenDisplayType(isfHeader, isfHeader.IsInDatabase ? ODisplayMode.Edit : ODisplayMode.New);
						}
						else
						{
							var from = ObjectFactory.Get<IHVLVISFMetaHeaderForm>(nameof(IHVLVISFMetaHeaderForm), newISFHeaders) as ZForm;
							ZFormModaliser.ShowDialogAndDispose(from, this.Form);
						}
					}
				}
			}
		}

		void NewFactoryOnSaving_SetBulkCopy(BusinessObjectFactory factory)
		{
			factory.SetBulkCopyOnTables(BulkCopyTableNames, batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value, fireTriggers: true, checkConstraints: true);
		}

		public override void UpdateVisibilityAndCaption()
		{
			Visible = HVLVMenuItemHelper.IsSeaShipmentWithUSDestination(shipment)
				&& !Header.GenPivotCollection.CustomsJobs.Any(job => typeof(ICusISFHeader).IsAssignableFrom(job.GetType()))
				&& !shipment.HasTransferredLog(CustomsModuleCodes.Codes.ImporterSecurityFiling);
		}

		protected override bool PreValidateData()
		{
			return UserPromptCheckingHelper.US.CheckWayBill(Header.Consignments.OfType<HVLVConsignment>(), Res.GetString("cc7f2827-50d7-40c1-9a6d-12dd3de3ea3c", "US Importer Security Filing"));
		}

		string SaveFormBeforeCreateISF => Res.GetString("f8ea8b3a-71cf-4740-9180-26de8c3a570b", "Please save the form before create US Importer Security Filing.");

		IList<string> BulkCopyTableNames => new List<string> {
			JobDocAddressSchema.Constants.TableName,
			JobConsolTransportSchema.Constants.TableName,
			CusISFHeaderSchema.Constants.TableName,
			CusISFBillSchema.Constants.TableName,
			CusISFLineSchema.Constants.TableName,
			GenPivotSchema.Constants.TableName,
			StmALogSchema.Constants.TableName,
			JobRequiredDocumentSchema.Constants.TableName
		};
	}
}
