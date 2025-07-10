using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business.MessageProcessor;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public static class ICUSDECMessageDataProviderExtensions
	{
		public static ZString GetDeclarationTypeForDocumentWrapper(this ICUSDECMessageDataProvider provider)
		{
			var result = (ZString)DeclarationTypeList.Codes.RegularCompleteDeclarationDefault;
			switch (provider)
			{
				case CUSDECMessageHelper messageHelper:
					result = messageHelper.DeclarationType;
					break;
				case MessageSendingObject sendingObject:
					{
						var lastAcceptedSendDeclarationType = sendingObject.LastAcceptedSendDeclarationType;
						switch (lastAcceptedSendDeclarationType)
						{
							case DeclarationTypeList.Codes.RegularIncompleteDeclaration:
							case DeclarationTypeList.Codes.RegularProvisionalDeclaration:
							case DeclarationTypeList.Codes.RegularSupplementaryDeclaration:
								result = DeclarationTypeList.Codes.RegularSupplementaryDeclaration;
								break;
							default:
								result = DeclarationTypeList.Codes.RegularCompleteDeclarationDefault;
								break;
						}

						break;
					}
			}
			return !result.IsEmpty ? result : (ZString)DeclarationTypeList.Codes.RegularCompleteDeclarationDefault;
		}

		public static ZString GetTransportDocumentNumberInBusinessLogic(this ICUSDECMessageDataProvider provider)
		{
			var result = ZString.Empty;

			if (provider != null)
			{
				result = provider.TransportDocumentNumber;

				if (provider.TransportMode == TransportModeCodeList.Codes.Sea)
				{
					result = provider.MasterCargoCarrier.PadRight(4) + result;
				}
			}

			return result.TrimStart();
		}

		public static ZString GetPartofPackagesValue(this ICUSDECMessageDataProvider provider, JobDeclaration declaration, ZInt currentPartIndex)
		{
			var partClearanceQuantity = provider.PartClearanceQuantity;
			var totalNoOfPacks = provider.TotalNoOfPacks;
			var totalPartClearancePacks = declaration?.ClearanceParts?.Sum(entryHeader => entryHeader.PackagesCount) ?? ZInt.Zero;

			var result = new ZStringBuilder();
			if (partClearanceQuantity > 1 && currentPartIndex > 0)
			{
				result.Append(ZString.Format("Part {0} of {1}", currentPartIndex, partClearanceQuantity));
				if (declaration?.JE_TotalNoOfPacks == 1 && declaration?.Entries.Count > 1)
				{
					result.Append(" - Part of 1 Package");
				}
				else if (declaration?.JE_TotalNoOfPacks > 1)
				{
					if (totalPartClearancePacks > 0)
					{
						result.Append(" - ");
						if (totalPartClearancePacks > 1)
						{
							result.Append(ZString.Format("{0} Packages of {1}", totalNoOfPacks, totalPartClearancePacks));
						}
						else
						{
							result.Append("Part of 1 Package");
						}
					}
				}
			}

			return result.ToString();
		}
	}
}
