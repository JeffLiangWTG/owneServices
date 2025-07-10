using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business
{
	public static class ValidationHelper
	{
		public static void ValidateMRNFormat(ZPropertyInfo info)
		{
			ZString mrn = (ZString)info.Value;
			if (!mrn.IsEmpty)
			{
				if (mrn.Length == 18)
				{
					var cusOffice = mrn.Substring(0, 3);
					if (!ZARefCusCodeListTypes.GetCustomsOfficeList(info.BizObj.Factory).ContainsCode(cusOffice))
					{
						info.AddMessageError(ValidationConstants.Shared.InvalidMRNCustomsOffice);
					}

					var testDate = mrn.Substring(3, 8);
					ZDateTime outDate;
					if (ZDateTime.TryParseExact(testDate, out outDate, "yyyyMMdd"))
					{
						if (outDate > ZDateTime.Today)
						{
							info.AddMessageError(ValidationConstants.Shared.InvalidMRNDate);
						}
					}
					else
					{
						info.AddMessageError(ValidationConstants.Shared.InvalidMRNDate);
					}

					if (!mrn.Substring(11).IsNumbersOnlyOrEmpty)
					{
						info.AddMessageError(ValidationConstants.Shared.InvalidMRNNumbersOnly);
					}
				}
				else
				{
					info.AddMessageError(Common.ZA.ZAValidationConstants.InvalidMRNLength);
				}
			}
		}

		public static void ValidateLRNFormat(ZPropertyInfo info)
		{
			ZString lrn = (ZString)info.Value;
			if (!lrn.IsEmpty)
			{
				if (lrn.Length == 25)
				{
					var agendCode = lrn.SubstringSafe(0, 8);
					if (agendCode.KeepNumericCharacters() != agendCode)
					{
						info.AddMessageError(ValidationConstants.Shared.LRNFormatInvalidAgentCode);
					}

					var cusOffice = lrn.SubstringSafe(8, 3);
					if (!ZARefCusCodeListTypes.GetCustomsOfficeList(info.BizObj.Factory).ContainsCode(cusOffice))
					{
						info.AddMessageError(ValidationConstants.Shared.LRNFormatInvalidOfficeCode);
					}

					var testDate = lrn.SubstringSafe(11, 8);
					ZDateTime outDate;
					if (ZDateTime.TryParseExact(testDate, out outDate, "yyyyMMdd"))
					{
						if (outDate > ZDateTime.Today)
						{
							info.AddMessageError(ValidationConstants.Shared.LRNFormatFutureDate);
						}
					}
					else
					{
						info.AddMessageError(ValidationConstants.Shared.LRNFormatInvalidDate);
					}
					var seqNum = lrn.SubstringSafe(19);
					if (seqNum.KeepNumericCharacters() != seqNum)
					{
						info.AddMessageError(ValidationConstants.Shared.LRNFormatInvalidSequenceNumber);
					}
				}
				else
				{
					info.AddMessageError(ValidationConstants.Shared.LRNFormatInvalidSize);
				}
			}
		}

		public static DynamicBusinessObjectCollection GetDuplicatedVINInvoiceLines(JobDeclaration declaration, ZString[] vinNumbers)
		{
			var duplicatedVINNumberSql = System.FormattableString.Invariant($@"
					SELECT JE_DeclarationReference, JZ_InvoiceNumber, JI_LineNo, CVH_VehicleIdentificationNumber
					FROM
						dbo.JobDeclaration
						INNER JOIN dbo.JobComInvoiceHeader ON JZ_ClusterKey = JE_ClusterKey
						INNER JOIN dbo.ZAJobComInvoiceLine ON JE_ClusterKey = JI_ClusterKey AND JI_JZ = JZ_PK
						INNER JOIN dbo.GlbCompany ON JE_GC = GC_PK
						LEFT JOIN dbo.CusVehicle ON JI_PK = CVH_ParentID
					WHERE JE_ApplicationCode = @applicationCode
						AND GC_RN_NKCountryCode = @countryCode
						AND JE_MessageType = @messageType
						AND CVH_VehicleIdentificationNumber IN (SELECT Value FROM @vinNumbers)
						AND JE_PK <> @declarationPK
						AND JE_IsCancelled <> 1
						AND JE_SystemCreateTimeUtc >= @fromDate
						AND CVH_VehicleIdentificationNumber <> ''
					ORDER BY JE_DeclarationReference, JZ_InvoiceNumber, JI_LineNo");

			var duplicateVINInvoiceLines = new DynamicBusinessObjectCollection(declaration.Factory);
			duplicateVINInvoiceLines.Load(duplicatedVINNumberSql, new ZSqlParameter[]
				{
					ZSqlParameter.New("@applicationCode", declaration.JE_ApplicationCode, JobDeclarationSchema.JE_ApplicationCode),
					ZSqlParameter.New("@countryCode", declaration.CountryCode, GlbCompanySchema.GC_RN_NKCountryCode),
					ZSqlParameter.New("@messageType", declaration.JE_MessageType, JobDeclarationSchema.JE_MessageType),
					ZSqlParameter.New("@vinNumbers", vinNumbers, CusVehicleSchema.CVH_VehicleIdentificationNumber, true),
					ZSqlParameter.New("@declarationPK", declaration.PK, JobDeclarationSchema.PK),
					ZSqlParameter.New("@fromDate", ZDateTime.Now.AddMonths(-6), JobDeclarationSchema.JE_SystemCreateTimeUtc)
				});
			return duplicateVINInvoiceLines;
		}

		public static ZString GetduplicateVINWarningMessage(DynamicBusinessObject duplicatedVINinvoiceLineDetail)
		{
			var declarationReference = duplicatedVINinvoiceLineDetail?[JobDeclarationSchema.Constants.JE_DeclarationReference]?.ToString();
			var invoiceNumber = duplicatedVINinvoiceLineDetail?[JobComInvoiceHeaderSchema.Constants.JZ_InvoiceNumber]?.ToString();
			var invoiceLineNo = duplicatedVINinvoiceLineDetail?[JobComInvoiceLineSchema.Constants.JI_LineNo]?.ToString();
			return Res.GetString("bf4caf94-61cf-469c-a374-8379e98fb8a6", "Declaration: '{0}', Invoice Number: '{1}', Invoice Line Number: '{2}'", declarationReference, invoiceNumber, invoiceLineNo);
		}

		public static ZString GetGateInOutMessageSendingNotification(this AsycudaManifestHeader header)
		{
			var stringBuilder = new ZStringBuilder();
			if (header != null)
			{
				if (header.AMA_OA_DeconsolidateAddress.IsEmpty)
				{
					stringBuilder.Append(Res.GetString("87f39021-a870-4f2d-a467-08dbe8383c45", "For Gate In/Out Manifests of Type '{0}' De-consolidation address is Mandatory.", header.GateInOutMessageType));
				}

				if (header.AMA_OA_DischargeTerminalAddress.IsEmpty)
				{
					stringBuilder.Append(Res.GetString("d8f806bd-b2e2-47b2-b14b-39b2b089383a", "For Gate In/Out Manifests of Type '{0}' Terminal address is Mandatory.", header.GateInOutMessageType));
				}
			}

			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public static ZString GetJobVoyageMessageSendingNotification(this JobVoyage jobVoyage)
		{
			var stringBuilder = new ZStringBuilder();
			if (jobVoyage != null)
			{
				foreach (VoyageOrigin origin in jobVoyage.Origins)
				{
					if (origin.JA_A_DEP.IsEmpty && origin.JA_E_DEP.IsEmpty)
					{
						stringBuilder.Append(Res.GetString("AC05CE5F-438D-4ECA-B76E-6CD26F009D25", "For Load Port '{0}', one of ETD or ATD is Mandatory.", origin.JA_RL_NKPortOfLoading));
					}
				}

				foreach (VoyageDestination destination in jobVoyage.Destinations)
				{
					if (destination.JB_A_ARV.IsEmpty && destination.JB_E_ARV.IsEmpty)
					{
						stringBuilder.Append(Res.GetString("77D50AE5-43A4-4CA5-B49C-85E50D386C1C", "For Discharge Port '{0}', one of ETA or ATA is Mandatory.", destination.JB_RL_NKPortOfDischarge));
					}
				}
			}
			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		public static ZBool CheckTariffIsInRange(ZString tariff, IEnumerable<CusPermitHeader> matchingPermits)
		{
			return matchingPermits.Any(x => x.CusPermitRules.Any(rule => rule.IsValueInRange(tariff)));
		}

		public static ZString GetJobNumberOfDuplicateUCR(BusinessObjectFactory factory, ZString ucr, ZGuid cusEntryHeaderGuid)
		{
			var jobNumber = ZString.Empty;

			if (!ucr.IsEmpty && ucr.Right(1).ToUpper() != "M")
			{
				JobDeclaration otherDeclarationThatUsesThisUCR = null;
				var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				var cusEntryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				var cusEntryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
				cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.SouthAfrica);
				cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.UniqueConsignementReference);
				cusEntryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, ucr);
				cusEntryHeaderSubQuery.AddSubQuery(cusEntryNumSubQuery, JoinCondition.And);
				cusEntryHeaderSubQuery.AddToFilter(CusEntryHeaderSchema.PK, SQLComparisonOperator.NotEqual, cusEntryHeaderGuid);
				jobDeclarationQuery.AddSubQuery(cusEntryHeaderSubQuery, JoinCondition.And);
				otherDeclarationThatUsesThisUCR = factory.LoadTop1<JobDeclaration>(jobDeclarationQuery);
				if (otherDeclarationThatUsesThisUCR != null)
				{
					jobNumber = otherDeclarationThatUsesThisUCR.JE_DeclarationReference;
				}
			}
			return jobNumber;
		}

		public static void ValidateBlankLinesForEDIFACT(ZPropertyInfo info, ZString value)
		{
			if (!value.IsEmpty)
			{
				value = value.Replace("\r\n", "\n").Replace("\r", "\n");
				var lines = value.Split(new char[] { '\n' });
				if (lines.Any(x => x == ZString.Empty))
				{
					info.AddMessageError(Res.GetString("8C93A0A5-CD74-4FBF-B660-FC5AC5F7DAC0", "Too many Line Feed and Carriage Return characters in this text field are known to cause EDI Gateway errors"));
				}
			}
		}
	}
}
