using System;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class CarrierHouseBillMessageInstructions : IMessageInstructions
	{
		public CarrierHouseBillMessageInstructions(IDocumentPivot pivot, IHouseBillTemplate template)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
			this.template = Argument.NotNull(template, nameof(template));
			lazyEmptyCodeDescriptionPairList = new Lazy<ICodeDescriptionPairList>(CreateEmptyCodeDescriptionPairList);
		}

		readonly IDocumentPivot pivot;
		readonly IHouseBillTemplate template;

		public string DataContext => template.DataContext;
		public string DocumentName => pivot.MenuName;
		public string TranslatedDocumentName => string.Empty;
		public string Recipient => (NoResString)"Booking Party";  // Programatic Recipient
		public string EHubClientID => "SHIPPING_INSTRUCTION";
		public string DirectXTClientID => ElectronicBOLConstants.DirectXTClientID;
		public string XmlNamespace => DocDataConstants.XmlNamespaces.BLData;

		public bool AllowSendMessage => true;

		public bool AllowSendMessageAmendment => false;
		public bool AllowSendMessageWithdrawal => false;
		public bool AllowResetToOriginal => false;
		public bool OrderLogsByLocalTime => false;
		public bool RequireMessageAmendmentReason => false;

		public ICodeDescriptionPairList AmendmentOptions => lazyEmptyCodeDescriptionPairList.Value;
		public ICodeDescriptionPairList WidthdrawalOptions => lazyEmptyCodeDescriptionPairList.Value;

		readonly Lazy<ICodeDescriptionPairList> lazyEmptyCodeDescriptionPairList;

		ICodeDescriptionPairList CreateEmptyCodeDescriptionPairList() => new CodeDescriptionPairList();
	}
}
