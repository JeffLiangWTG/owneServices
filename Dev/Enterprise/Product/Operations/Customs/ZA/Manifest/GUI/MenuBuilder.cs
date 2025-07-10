using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.Manifest.Business;
using Enterprise.Customs.ZA.Manifest.Business.EDIFACT;
using Enterprise.Customs.ZA.Manifest.GUI.MessagingProcess;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Manifest.GUI
{
	public class MenuBuilder : ASYCUDA.GUI.MenuBuilder
	{
		public MenuBuilder(ASYCUDA.Business.AsycudaManifestHeader header, ZForm mainForm) : base(header, mainForm)
		{
		}

		public override ResourceString MenuCaption => ResString.GetMultilingualString("18341333-FBDE-4773-BB2B-0C7EC81269DC", "ZA Manifest");

		public override ZMenuItem[] BuildMenu()
		{
			var menuItems = new List<ZMenuItem>();

			if (IsValidForMessage())
			{
				var messageLabel = Res.GetString("FFB49FA9-9F21-4372-A54F-CCC574899F83", "Manifest");
				var messageLabelPOC = Res.GetString("23F3A0CC-6123-40DD-8EEB-E94AB6FFB52E", "Manifest POC");
				if (Header.IsBillLevelManifestType)
				{
					MenuBuilderHelper.AddBillLevelMenuItem(mainForm, menuItems, Header, CreateBillLevelMessage, messageLabel);
					if (MessagingPOCHelper.IsPOCActive)
					{
						MenuBuilderHelper.AddBillLevelMenuItem(mainForm, menuItems, Header, CreateBillLevelMessagePOC, messageLabelPOC, validateManifest: false);
					}
				}
				else if (!Header.IsPackedItemLevelManifestType)
				{
					MenuBuilderHelper.AddSendManifestMenuItem(mainForm, menuItems, Header, CreateManifestLevelMessage, messageLabel);
					if (MessagingPOCHelper.IsPOCActive)
					{
						MenuBuilderHelper.AddSendManifestMenuItem(mainForm, menuItems, Header, CreateManifestLevelMessagePOC, messageLabelPOC, validateManifest: false);
					}
				}
				else
				{
					throw new NotSupportedException("ZA does not support Pack level manifests.");
				}

				var header = Header as AsycudaManifestHeader;

				if (ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Customs.Universal.Constants.FunctionalityTypes.ZAManifestCaseNumbers
					, Core.Constants.CountryCodes.SouthAfrica
					, ZDateTime.Today))
				{
					AddSendSupportingDocumentsMenuItem(mainForm, menuItems, header);
				}

				if (!header.IsStandAlone)
				{
					AddRefreshUcrAndLrnMenuItem(mainForm, menuItems, header);
				}
			}
			else
			{
				var menuItem = GetInvalidMessageMenuItem();
				menuItems.Add(menuItem);
			}

			return menuItems.ToArray();
		}

		void AddRefreshUcrAndLrnMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var caption = ResString.GetMultilingualString("D373A70E-C2E7-402E-9FF7-0181514107CF", "Refresh UCR and LRN data");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, caption, header, () =>
				{
					var message = header.RefreshMainfestBillUcrAndLrns();
					Globals.Message.Show(message);
				}
				, false);
		}

		public void AddSendSupportingDocumentsMenuItem(ZForm mainForm, List<ZMenuItem> menuItems, AsycudaManifestHeader header)
		{
			var captionSupportingDoc = ResString.GetMultilingualString("AsycudaMenu|SendSupportingDocs", "Send Supporting &Documents");
			MenuBuilderHelper.AddMenuItem(mainForm, menuItems, captionSupportingDoc, header, () => SendSupportingDocuments(header, mainForm), false);
		}

		void SendSupportingDocuments(AsycudaManifestHeader header, ZForm mainForm)
		{
			if (SaveDataFirst.Confirm(header, mainForm))
			{
				var manifestWrapper = new ManifestSupportingDocSendingObjectParent(header);
				using (var form = new SupportingDocSendingForm(manifestWrapper))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						new SupportingDocSendingManager(manifestWrapper, new MessageNotificationCollector()).SendMessages();
					}
				}
			}
		}

		void CreateBillLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<ASYCUDA.Business.IMessageParent> messageParents, ASYCUDA.Business.MessageChooser chooser)
		{
			Globals.Message.Show(CusCarMessagingHelper.CreateCusCars((AsycudaManifestHeader)header, messageParents.OfType<AsycudaBill>(), messageSubType));
		}

		void CreateManifestLevelMessage(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			var message = CusCarMessagingHelper.CreateCusCars((AsycudaManifestHeader)header, messageSubType);

			if (message.Length > 0)
			{
				Globals.Message.Show(message);
			}
		}

		void CreateManifestLevelMessagePOC(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType)
		{
			CustomsMessagingGui.SendMessages((AsycudaManifestHeader)header, messageSubType, null, mainForm);
		}
		void CreateBillLevelMessagePOC(ASYCUDA.Business.AsycudaManifestHeader header, string messageSubType, IList<ASYCUDA.Business.IMessageParent> messageParents, ASYCUDA.Business.MessageChooser chooser)
		{
			CustomsMessagingGui.SendMessages((AsycudaManifestHeader)header, messageSubType, messageParents.OfType<AsycudaBill>().ToList(), mainForm);
		}
	}
}
