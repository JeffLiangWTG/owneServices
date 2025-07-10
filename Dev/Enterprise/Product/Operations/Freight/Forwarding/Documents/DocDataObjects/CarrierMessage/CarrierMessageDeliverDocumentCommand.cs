using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Macros;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class CarrierMessageDeliverDocumentCommand : ICommand, INotifiableDocumentInfoCreated
	{
		public CarrierMessageDeliverDocumentCommand(ForwardingConsol consol, bool isSendMessageAsPDF = false)
		{
			this.isSendMessageAsPDF = isSendMessageAsPDF;
			this.consol = consol;
			documentSupportable = (IDocumentSupportable)consol ?? throw new ArgumentNullException(nameof(consol));
		}

		readonly IDocumentSupportable documentSupportable;
		readonly ForwardingConsol consol;
		readonly bool isSendMessageAsPDF;

		public bool Invoke() => Invoke(null);

		public bool Invoke(MacroMap parameters)
		{
			var documentInfo = documentInfos.FirstOrDefault();
			var documentName = documentInfo.Descriptor.MessageInstructions.DocumentName;
			var deliverDocumentCommand = new DeliverDocumentCommand(documentSupportable);

			if (isSendMessageAsPDF)
			{
				return DeliverDocument(deliverDocumentCommand, parameters);
			}
			else if (RoutingRuleIsisValid(documentInfo))
			{
				using (var form = ObjectFactory.Get<IDeliverDocumentPopupForm>(nameof(IDeliverDocumentPopupForm), documentName))
				{
					return DeliverDocumentPopupFormAction(parameters, deliverDocumentCommand, documentInfos, form.ShowDialogAndGetResult());
				}
			}
			else
			{
				using (var form = ObjectFactory.Get<IAlertHyperLinkForm>(nameof(IAlertHyperLinkForm)))
				{
					if (form.ShowDialogAndGetResult() == ZDialogResult.OK)
					{
						return DeliverDocument(deliverDocumentCommand, parameters);
					}
				}
				return false;
			}
		}

		public string Id => CommandIds.DeliverDocument;
		public string Caption => Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("98cb48bb-d8da-42aa-83e1-bd975e0e4278", "Deliver Document");
		public object Image { get; }

		public bool IsEnabled => documentInfos.Count > 0;

		readonly HashSet<IDocumentInfo> documentInfos = new HashSet<IDocumentInfo>();

		public bool IsVisible => true;

		void INotifiableDocumentInfoCreated.NotifyDocumentInfoCreated(IDocumentInfo documentInfo)
		{
			if (documentInfo != null)
			{
				documentInfos.Add(documentInfo);
			}
		}

		public bool DeliverDocumentPopupFormAction(MacroMap parameters, DeliverDocumentCommand deliverDocumentCommand, HashSet<IDocumentInfo> documentInfos, DeliverDocumentPopupAction action)
		{
			if (action == DeliverDocumentPopupAction.SendMessage)
			{
				var sendMessageCommand = new SendMessageCommand();
				return sendMessageCommand.Invoke(parameters, documentInfos.FirstOrDefault());
			}
			else if (action == DeliverDocumentPopupAction.DeliverDocument)
			{
				return DeliverDocument(deliverDocumentCommand, parameters);
			}

			return false;
		}

		bool RoutingRuleIsisValid(IDocumentInfo documentInfo)
		{
			var interchange = documentInfo.Document
				.ToUniversalXmlDataObject()
				.PopulateDataContext(documentInfo.Descriptor.MessageInstructions.DocumentName)
				.ToUniversalXml(documentInfo.Descriptor.MessageInstructions.XmlNamespace)
				.WrapInInterchange(documentInfo.Descriptor.MessageInstructions.EHubClientID);

			return ObjectFactory.Get<IRoutingRuleValidator>().IsValid(interchange);
		}

		bool DeliverDocument(DeliverDocumentCommand deliverDocumentCommand, MacroMap parameters)
		{
			foreach (var eachdocumentInfo in documentInfos)
			{
				((INotifiableDocumentInfoCreated)deliverDocumentCommand).NotifyDocumentInfoCreated(eachdocumentInfo);
			}

			return deliverDocumentCommand.Invoke(parameters);
		}
	}
}
