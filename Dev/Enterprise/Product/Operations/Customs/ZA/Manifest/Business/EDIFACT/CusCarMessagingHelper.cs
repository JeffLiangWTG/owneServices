using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using static System.FormattableString;

namespace Enterprise.Customs.ZA.Manifest.Business.EDIFACT
{
	public static partial class CusCarMessagingHelper
	{
		public static string CreateCusCars(AsycudaManifestHeader asycudaManifestHeader, string messageSubType)
		{
			var carHeader = new CusCarHeader(asycudaManifestHeader);

			var countryName = carHeader.CountryName;
			var bills = asycudaManifestHeader.Bills.Cast<AsycudaBill>().ToArray();
			var sb = new ZStringBuilder();

			if (MessagingProvider.CanSendMessage(asycudaManifestHeader, sb))
			{
				var billIssuerCodes = new List<ZString>();
				foreach (var bill in bills)
				{
					if (bill.IsIssuerCodeMandatory && bill.ABL_BillIssuer.IsEmpty)
					{
						sb.AppendFormat("Bill '{0}' must have a Bill Issuer.", bill.ABL_BillNumber);
					}
					else
					{
						billIssuerCodes.Add(bill.ABL_BillIssuer);
					}
				}

				if (!sb.IsEmpty)
				{
					sb.Prepend(Invariant($"At least one Bill is not eligible for sending to {countryName}."));
					sb.Append("Please fix this before sending again.");
				}
				else
				{
					var distinctBillIssuers = billIssuerCodes.Distinct().ToArray();
					var allMessagesCreated = new List<EDIMessage>();
					if (carHeader.HasBillsAndPacks && !distinctBillIssuers.Any())
					{
						sb.Append("At least one Bill must be created for sending.");
					}
					else
					{
						if (carHeader.HasBillsAndPacks)
						{
							foreach (var issuer in distinctBillIssuers)
							{
								CreateCusCar(MessagingProvider.GetMessageBuilder(carHeader, messageSubType, issuer), issuer, carHeader, allMessagesCreated, sb);
							}
						}
						else
						{
							CreateCusCar(MessagingProvider.GetMessageBuilder(carHeader, messageSubType, ZString.Empty), ZString.Empty, carHeader, allMessagesCreated, sb);
						}

						var oldStatus = "";
						try
						{
							oldStatus = carHeader.GetOldAndSetNewCountryMessagingStatus(messageSubType);
							carHeader.Factory.Save();
							carHeader.Header.Messages.Load();
						}
						catch (ZSaveException ex)
						{
							sb.Append("The following error was encountered while saving the changes:");
							sb.Append(ex.Message);
							// Revert all changes:
							foreach (var m in allMessagesCreated)
							{
								m.Delete();
							}
							carHeader.SetNewCountryMessagingStatus(oldStatus);
						}
					}
				}
			}

			return sb.ToStringWithNewLineBetweenAppends();
		}

		public static string CreateCusCars(AsycudaManifestHeader asycudaManifestHeader, IEnumerable<AsycudaBill> selectedBillCountries, string messageSubType)
		{
			var carHeader = new CusCarHeader(asycudaManifestHeader);

			var countryCode = carHeader.CountryCode;
			var countryName = carHeader.CountryName;
			var sb = new ZStringBuilder();

			if (MessagingProvider.CanSendMessage(asycudaManifestHeader, sb))
			{
				if (!selectedBillCountries.Any())
				{
					sb.Append("At least one Bill must be selected for sending");
				}
				else if (selectedBillCountries.Any(b => b.CountryCode != countryCode))
				{
					sb.Append(Invariant($@"At least one Bill is not eligible for sending to {countryName}.
All Bills must have a 'Location Details' row for {countryCode} and must have a Bill Issuer.
Please fix this before sending again."));  // TODO - changing wording?
				}
				else
				{
					var allMessagesCreated = new List<EDIMessage>();
					var billStatuses = selectedBillCountries.Select(bill =>
					{
						var filteredHelper = new SingleBillCusCarHeader(carHeader, bill);

						var billFunction = GetBillFunction(messageSubType, carHeader, bill);

						var issuer = bill.ABL_BillIssuer;
						CreateCusCar(MessagingProvider.GetMessageBuilder(filteredHelper, billFunction, issuer), issuer, carHeader, allMessagesCreated, sb);
						var oldStatus = bill.ABL_MessageStatus;
						bill.ABL_MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
						return new { bill, oldStatus };
					}).ToArray();

					var oldManifestStatus = "";
					try
					{
						oldManifestStatus = carHeader.GetOldAndSetNewCountryMessagingStatus(messageSubType);
						carHeader.Factory.Save();
						carHeader.Header.Messages.Load();
					}
					catch (ZSaveException ex)
					{
						sb.Append("The following error was encountered while saving the changes:");
						sb.Append(ex.Message);
						// Revert all changes:
						foreach (var m in allMessagesCreated)
						{
							m.Delete();
						}
						carHeader.SetNewCountryMessagingStatus(oldManifestStatus);
						foreach (var billStatus in billStatuses)
						{
							billStatus.bill.ABL_MessageStatus = billStatus.oldStatus;
						}
					}
				}
			}
			return sb.ToStringWithNewLineBetweenAppends();
		}

		public static string GetBillFunction(string messageSubType, CusCarHeader carHeader, AsycudaBill bill) => messageSubType == carHeader.MessageFunctionSubTypeForCancel ? messageSubType
																													: (carHeader.MessageStatusProvider?.AllowModificationMessage(bill) ?? false)
																														? carHeader.MessageFunctionSubTypeForAmend
																														: MessageSubTypeCodes.Codes.Original;

		static void CreateCusCar(IMessageBuilder builder, ZString issuer, CusCarHeader header, List<EDIMessage> allMessagesCreated, ZStringBuilder result)
		{
			var populateResult = builder.PopulateMessages();
			var buildResults = populateResult.GetBuilderResults();

			foreach (var r in buildResults)
			{
				allMessagesCreated.Add(r.Message);
				if (!(header.Header?.IsRoad ?? false))
				{
					var manifestType = ((ICusCarHeader)header).ManifestDocumentType;
					if (manifestType != ManifestDocumentType.COM
						&& manifestType != ManifestDocumentType.BBB
						&& manifestType != ManifestDocumentType.ECL
						&& manifestType != ManifestDocumentType.FFM
						&& manifestType != ManifestDocumentType.AQM
						&& manifestType != ManifestDocumentType.ALM)
					{
						result.Append("Issuer code: " + (issuer.IsEmpty ? "[No Bill Issuer]" : issuer.ToString()));
					}
				}
				if (populateResult.IsSuccess)
				{
					result.Append("	Successfully created message");
				}
				else
				{
					result.Append("	Error creating message");
					result.Append(string.Join("\r\n\t\t", r.Errors));
				}
			}
		}
	}
}
