using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class FIATAHouseBillMessageInstructions : IMessageInstructions
	{
		public FIATAHouseBillMessageInstructions(IDocumentPivot pivot, IHouseBillTemplate template)
		{
			this.pivot = Argument.NotNull(pivot, nameof(pivot));
			this.template = Argument.NotNull(template, nameof(template));
		}

		readonly IDocumentPivot pivot;
		readonly IHouseBillTemplate template;

		public string DataContext => template.DataContext;
		public string DocumentName => pivot.MenuName;
		public string TranslatedDocumentName => string.Empty;
		public string Recipient => "HOUSEBILLOFLADING";
		public string EHubClientID => "HOUSEBILLOFLADING";
		public string DirectXTClientID => string.Empty;
		public string XmlNamespace => "/HouseBillOfLading/1";

		public bool AllowSendMessage => FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures.Value;

		public bool AllowSendMessageAmendment => true;
		public bool AllowSendMessageWithdrawal => false;
		public bool AllowResetToOriginal => FreightDataRegistry.Instance.EnableFIATAHouseBillsFeatures.Value;
		public bool OrderLogsByLocalTime => false;
		public bool RequireMessageAmendmentReason => true;

		public ICodeDescriptionPairList AmendmentOptions => null;
		public ICodeDescriptionPairList WidthdrawalOptions => null;
	}
}
