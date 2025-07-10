using System;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents
{
	public sealed class DefaultHouseBillMessageInstructions : IMessageInstructions
	{
		public DefaultHouseBillMessageInstructions(IDocumentPivot pivot, IHouseBillTemplate template)
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
		public string Recipient => string.Empty;
		public string EHubClientID => string.Empty;
		public string DirectXTClientID => string.Empty;
		public string XmlNamespace => string.Empty;
		public bool AllowSendMessage => false;
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
