using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WTG.CreditCheck;
using WTG.ROPE.Model;
using static Enterprise.Core.Constants;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.MasterFiles.GUI
{
	public static class CreditReportHelper
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		public const string BuyCreditReportReference = "Buy Credit Report";
		public const string RenewCode = "RCR";
		public const string BuyCode = "BCR";

		public static bool SaveIdentifiers(this OrgHeader header, IEnumerable<Identifier> identifiers, bool silentSave)
		{
			var result = false;

			if (identifiers != null && identifiers.Any())
			{
				if (silentSave)
				{
					if (identifiers.Count() == 1)
					{
						var dunsIdentifier = identifiers.FirstOrDefault(x => x.Type == IdentifierType.DUNS && !string.IsNullOrEmpty(x.ID));
						if (dunsIdentifier != null)
						{
							var factory = new BusinessObjectFactory();
							var query = new ZQuery();
							query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.DataUniversalNumberingSystem);
							query.AddToFilter(OrgCusCodeSchema.OK_OH, header.PK);

							var dunsCode = factory.LoadTop1<OrgCusCode>(query);
							if (dunsCode == null)
							{
								dunsCode = factory.New<OrgCusCode>();
								dunsCode.OK_CodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
								dunsCode.OK_CustomsRegNo = dunsIdentifier.ID;
								dunsCode.OK_OH = header.PK;
								dunsCode.OK_RN_NKCodeCountry = header.CountryCode;
								factory.Save();

								result = true;
							}
						}
					}
				}
				else
				{
					foreach (var identifier in identifiers)
					{
						var dbCodeType = MapCodeType(identifier.Type);
						if (header.CustomsCodes.Cast<OrgCusCode>().All(c => c.OK_CodeType != dbCodeType))
						{
							var cusCode = header.CustomsCodes.AddNew();
							cusCode.OK_CodeType = dbCodeType;
							cusCode.OK_CustomsRegNo = identifier.ID;
							cusCode.OK_OH = header.PK;
							cusCode.OK_RN_NKCodeCountry = header.CountryCode;
						}
					}

					header.CustomsCodes.Cast<OrgCusCode>().ForEach(x => x.Validation.ValidateAll());

					if (header.CustomsCodes.HasErrors())
					{
						Globals.Message.ShowError(Res.GetString("6F3E4F77-E5CE-41A3-AB25-3DEF1BC99B6F", "There are error(s) under Details -> Config -> Registration Numbers / Codes: {0}", header.CustomsCodes.GetErrors().ToMessageListString()));
						return false;
					}

					try
					{
						header.Factory.Save();
						result = true;
					}
					catch (ZSaveException)
					{
						Globals.Message.ShowError(Res.GetString("06351A2B-E19D-4B28-B9A2-D3E5FD39EC89",
							@"Probably before you confirm the organization, another user has already saved same type identifier(s) for this organization.
Please check the existing identifier(s) under: Details -> Config -> Registration Numbers / Codes after refreshing the organization form."));
					}
				}
			}

			return result;
		}

		public static string MapCodeType(IdentifierType type)
		{
			string dbCodeType;
			switch (type)
			{
				case IdentifierType.DUNS:
					dbCodeType = OrgCusCode.CodeTypes.DataUniversalNumberingSystem;
					break;
				case IdentifierType.ACN:
					dbCodeType = OrgCusCode.CodeTypes.CorporationCode;
					break;
				case IdentifierType.NZBN:
					dbCodeType = OrgCusCode.CodeTypes.CompanyNumber;
					break;
				case IdentifierType.NCN:
					dbCodeType = OrgCusCode.CodeTypes.GSTCode;
					break;
				default:
					dbCodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
					break;
			}

			return dbCodeType;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly Dictionary<CreditReportType, string> creditReportTypeNameMapping = new Dictionary<CreditReportType, string>()
		{
			[CreditReportType.ComprehensiveReport] = ReportType.Desc.ComprehensiveReport,
			[CreditReportType.FailureRisk] = ReportType.Desc.FailureRiskReport,
			[CreditReportType.LatePaymentRisk] = ReportType.Desc.LatePaymentRiskReport,
			[CreditReportType.CommercialBureauEnquiry] = ReportType.Desc.CommercialBureauEnquiryReport
		};

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		internal static readonly Dictionary<CreditReportType, string> creditReportTypeCodeMapping = new Dictionary<CreditReportType, string>()
		{
			[CreditReportType.ComprehensiveReport] = ReportType.Code.ComprehensiveReport,
			[CreditReportType.FailureRisk] = ReportType.Code.FailureRiskReport,
			[CreditReportType.LatePaymentRisk] = ReportType.Code.LatePaymentRiskReport,
			[CreditReportType.CommercialBureauEnquiry] = ReportType.Code.CommercialBureauEnquiryReport
		};

		public static IeDoc SaveAndShowReport(this OrgHeader header, CreditReportType reportType, string payload, string payloadExtension, bool isGet, string duns, string dunsRating, string failureRiskScore, string latePaymentScore)
		{
			if (string.IsNullOrEmpty(duns))
			{
				throw new DeveloperNotificationException("No duns number, failed to save and show credit check report.");
			}

			var factoryForSavingReport = new BusinessObjectFactory(); // Use new factory to make sure when failed to save together, nothing added to the org in UI
			var orgHeaderInNewFactory = factoryForSavingReport.Load<OrgHeader>(header.PK)
				?? throw new DeveloperNotificationException("Can't find the organization");

			var creditReportCode = creditReportTypeCodeMapping[reportType];
			var creditReportType = creditReportTypeNameMapping[reportType];
			var fileName = creditReportType + "_" + ZDateTime.UtcToday.Date + payloadExtension;
			var reportBytes = Convert.FromBase64String(payload);
			var docManager = ((IDocManagerSupport)orgHeaderInNewFactory).DocManagerInfo;

			var doc = docManager.AddFileOrDocument(reportBytes, fileName, RefDocTypes.CreditReport);
			orgHeaderInNewFactory.AddBuyCreditReportEvent(creditReportCode, isGet, doc.FileName);
			orgHeaderInNewFactory.AddCreditScores(dunsRating, failureRiskScore, latePaymentScore);

			var dbConnection = ((IDbConnected)factoryForSavingReport).Connection;
			var addBillingAction = new SaveInTransactionDelegateAction(dbConnection, () =>
			{
				var code = new CreditCheckCodeMapping().ReportTypePriceItemCodeMapping[(reportType, true)];

				CreditCheckReportBillingCreator.CreateBillingTransaction(dbConnection, code, Constants.BillingCategory, Constants.ReportingSource, orgHeaderInNewFactory, duns);

				return ChangedTableNames.Empty;
			});

			BusinessObjectFactory.SaveTogether(factoryForSavingReport, docManager.MasterFactory, addBillingAction);

			return doc;
		}

		public static void ShowLatestReport(this OrgHeader header, ZForm parentForm, CreditReportType creditReportType)
		{
			var showReport = false;
			var fileName = header.GetPurchasedReports().FirstOrDefault(x => x.ReportType == creditReportTypeNameMapping[creditReportType]).FileName;
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				var docManager = ((IDocManagerSupport)header).DocManagerInfo;
				var eDoc = docManager.AllEDocs.Cast<IeDoc>().FirstOrDefault(x => x.FileName == fileName);
				if (eDoc != null)
				{
					ViewReport(parentForm, eDoc);
					showReport = true;
				}
			}

			if (!showReport)
			{
				Globals.Message.ShowError(Res.GetString("BA791187-9540-4744-82A7-B40F9B7E994C", "The {0} does not exist.", ResourceStringHelper.GetReportCaption(creditReportType)));
			}
		}

		public static void ViewReport(ZForm parentForm, IeDocBase eDoc)
		{
			var storageDocImageViewer = ObjectFactory.Get<IStorageDocsViewer>();
			storageDocImageViewer.Initialise(parentForm, null);
			storageDocImageViewer.View(eDoc, true);
		}

		public static (CreditReportType CreditReportType, string ReportType, ZDateTime BuyReportDate, string FileName)[] GetPurchasedReports(this OrgHeader orgHeader)
		{
			var logs = orgHeader.GetLogs().GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.CreditCheckEvent).ToList();

			StmALog logComprehensive = null;
			StmALog logFailureRisk = null;
			StmALog logLatePaymentRisk = null;
			StmALog logCommercialBureauEnquiry = null;

			foreach (var log in logs)
			{
				if (log.Parameters.TryGetValue(Params.Codes.Type, out var typeCode) && log.Parameters.TryGetValue(Params.Codes.Reason, out var reasonCode))
				{
					if (typeCode == BuyCode || typeCode == RenewCode)
					{
						switch (reasonCode)
						{
							case ReportType.Code.ComprehensiveReport:

								if (logComprehensive == null)
								{
									logComprehensive = log;
								}

								break;
							case ReportType.Code.FailureRiskReport:

								if (logFailureRisk == null)
								{
									logFailureRisk = log;
								}

								break;
							case ReportType.Code.LatePaymentRiskReport:

								if (logLatePaymentRisk == null)
								{
									logLatePaymentRisk = log;
								}

								break;
							case ReportType.Code.CommercialBureauEnquiryReport:

								if (logCommercialBureauEnquiry == null)
								{
									logCommercialBureauEnquiry = log;
								}

								break;
						}

						if (logComprehensive != null && logFailureRisk != null && logLatePaymentRisk != null && logCommercialBureauEnquiry != null)
						{
							break;
						}
					}
				}
			}

			return new[] { logCommercialBureauEnquiry, logComprehensive, logFailureRisk, logLatePaymentRisk }
				.Where(u => u != null)
				.Select(u => (
					creditReportTypeCodeMapping.Single(x => x.Value == u.Parameters[Params.Codes.Reason]).Key
					, creditReportTypeNameMapping[creditReportTypeCodeMapping.Single(x => x.Value == u.Parameters[Params.Codes.Reason]).Key]
					, u.SL_PostedTimeUtc
					, u.Parameters[Params.Codes.Name])
				).ToArray();
		}

		internal static void AddBuyCreditReportEvent(this OrgHeader header, string reportCode, bool isGet, string fileName)
		{
			var logParameters = new Dictionary<string, string>
			{
				[Params.Codes.Type] = isGet ? BuyCode : RenewCode,
				[Params.Codes.Reason] = reportCode,
				[Params.Codes.Name] = fileName,
			};

			header.GetLogs().AddNew(AutoEvents.CreditCheckEvent, BuyCreditReportReference, logParameters.ToArray());
		}

		internal static void AddCreditScores(this OrgHeader header, string dunsRating, string failureRiskScore, string latePaymentScore)
		{
			if (!string.IsNullOrWhiteSpace(dunsRating))
			{
				header.MiscServ.OM_CCCreditRating = dunsRating;
			}

			if (ZShort.TryParse(failureRiskScore, out ZShort shortFailureRiskScore))
			{
				header.MiscServ.OM_CCFailureRiskScore = shortFailureRiskScore;
			}

			if (ZShort.TryParse(latePaymentScore, out ZShort shortLatePaymentScore))
			{
				header.MiscServ.OM_CCLatePaymentScore = shortLatePaymentScore;
			}
		}

		public static bool CreditReportEnabled =>
			OrganisationsDataRegistry.Instance.EnableCreditReports.Value &&
			OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.Value.Cast<CreditReportItem>().Any(x => x.CountryEnabledForCompany && x.CountryCode == Env.CurrentCompany.Country.Code);
	}

	public static class ReportType
	{
		public static class Code
		{
			public const string ComprehensiveReport = "CRE";
			public const string FailureRiskReport = "FRR";
			public const string LatePaymentRiskReport = "LPR";
			public const string CommercialBureauEnquiryReport = "CBE";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		public static class Desc
		{
			public const string ComprehensiveReport = "Comprehensive Report";
			public const string FailureRiskReport = "Failure Risk Report";
			public const string LatePaymentRiskReport = "Late Payment Risk Report";
			public const string CommercialBureauEnquiryReport = "Commercial Bureau Enquiry Report";
		}
	}
}
