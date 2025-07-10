using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class LastMileCarrierBookingMenuItem(Func<HVLVConsignment> consignmentGetter)
		: BaseLastMileCarrierBookingMenuItem(ResString.GetMultilingualString("8b84ef27-293d-4b24-b115-a8b09f6b169a", "Last Mile Carrier Booking"), consignmentGetter)
	{
		public override void UpdateVisibilityAndCaption()
		{
			base.UpdateVisibilityAndCaption();
			Visible = Visible && !Consignment.Items.OfType<HVLVItem>().All(i => i.IsLastMileCarrierBooked);
		}

		protected override Action MenuAction => () =>
		{
			using (var selectConsignmentForm = new HVLVBookLastMileCarrierSelectItemForm(Consignment))
			{
				ZFormModaliser.ShowDialogWithoutDispose(selectConsignmentForm);
				if (selectConsignmentForm.DialogResult == DialogResult.OK)
				{
					BookLastMileCarrierCore(selectConsignmentForm.SelectedItems);
				}
			}
		};

		void BookLastMileCarrierCore(IEnumerable<HVLVItem> itemsToBook)
		{
			try
			{
				var hvlvItemRTUSBookingProvider = ObjectFactory.Get<ILastMileCarrierBookingService>(nameof(HVLVItemRTUSBookingProvider), Consignment.Factory);
				var hasAnyItemBooked = false;
				foreach (var item in itemsToBook)
				{
					var lastMileCarrierBookingResponseCollection = (IEnumerable<ILastMileCarrierBookingResponse>)hvlvItemRTUSBookingProvider.BookLastMileCarrier(item.PK.ToGuid());
					foreach (var response in lastMileCarrierBookingResponseCollection)
					{
						if (response.Successful)
						{
							hasAnyItemBooked = true;
						}
						else
						{
							Globals.Message.ShowError(response.ErrorMessage);
						}
					}
				}

				if (hasAnyItemBooked)
				{
					var docManagerInfo = ((IDocManagerSupport)Consignment).DocManagerInfo;
					DeliverDocument(docManagerInfo.AllEDocs);
				}
			}
			catch (ConfigurationErrorsException ex)
			{
				Globals.Message.ShowInformation(ex.Message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}

		#region DeliverDocument

		void DeliverDocument(IStorageDocsBaseCollection storageDocs)
		{
			var pack = CreateDocumentPack(storageDocs);
			var instructions = new DeliveryInstructions(pack);
			SetDeliverySettingsOnInstructions(instructions);
			SetDeliveryMethodOnInstructions(instructions);
			using (var task = new PrintTask())
			{
				task.Add(pack);
				task.RunWithPartialInstructions(instructions.DeliveryOptions, instructions, Env.Security.None);
			}
		}

		void SetDeliverySettingsOnInstructions(DeliveryInstructions instructions)
		{
			instructions.DeliveryOptions = AllowedDeliveryOptions.All;
			instructions.AllowAutoDelivery = false;
			instructions.AttachmentTypeDisabled = true;
		}

		void SetDeliveryMethodOnInstructions(DeliveryInstructions instructions)
		{
			var emailModes = new[] { Core.Constants.ContactNotifyModes.Email, Core.Constants.ContactNotifyModes.EPrint };
			var canOnlyEmail = instructions.DeliverablesToBePrinted
				.SelectMany(d => ((IDeliverable)d).GetSupportedDeliveryMethods())
				.All(m => emailModes.Contains(m));

			foreach (DocDeliveryContact recipient in instructions.Recipients)
			{
				using (recipient.GetValidationSuspender())
				{
					if (canOnlyEmail)
					{
						recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
					}

					recipient.AttachmentTypeDisabled = true;
				}
			}
		}

		DocumentPack CreateDocumentPack(IStorageDocsBaseCollection storageDocs)
		{
			var result = new DocumentPack();
			foreach (StorageDocsBase storageDoc in storageDocs)
			{
				result.Add(storageDoc);
			}

			return result;
		}

		#endregion
	}
}
