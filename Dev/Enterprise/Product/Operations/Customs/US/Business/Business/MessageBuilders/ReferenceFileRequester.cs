using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class TariffDate
	{
		public TariffDate(ZString fromTariff, ZString toTariff, ZDate date)
		{
			FromTariff = fromTariff;
			ToTariff = toTariff;
			Date = date;
		}
		public readonly ZString FromTariff;
		public readonly ZString ToTariff;
		public readonly ZDate Date;

		public override bool Equals(object obj)
		{
			return ((TariffDate)obj).Date == Date && ((TariffDate)obj).FromTariff == FromTariff && ((TariffDate)obj).ToTariff == ToTariff;
		}

		public override int GetHashCode()
		{
			return Date.GetHashCode() ^ FromTariff.GetHashCode() ^ ToTariff.GetHashCode();
		}
	}

	public class ADDCVDQueryRequest
	{
		public ADDCVDQueryRequest(ZString caseNumber, ZString tariffNumber, ZString country)
		{
			this.caseNumber = caseNumber;
			this.country = country;
			this.tariffNumber = tariffNumber;
		}
		public readonly ZString caseNumber;
		public readonly ZString country;
		public readonly ZString tariffNumber;

		public override bool Equals(object obj)
		{
			ADDCVDQueryRequest request = (ADDCVDQueryRequest)obj;
			return request.country == country &&
				request.caseNumber == caseNumber &&
				request.tariffNumber == tariffNumber;
		}

		public override int GetHashCode()
		{
			int result = caseNumber.GetHashCode() ^ country.GetHashCode();
			result ^= tariffNumber.GetHashCode();
			return result;
		}
	}

	public class ReferenceFileRequester
	{
		public ReferenceFileRequester(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public ReferenceFileRequester()
			: this(new BusinessObjectFactory())
		{
			autoSave = true;
		}

		public ReferenceFileRequester(BusinessObjectFactory factory, bool autoSave)
			: this(factory)
		{
			this.autoSave = autoSave;
		}

		readonly BusinessObjectFactory factory;
		readonly bool autoSave;

		#region Antidumping and Countervailing request

		public bool RequestADDCVDs(JobDeclaration declaration)
		{
			bool result = false;
			if (declaration.IsACE)
			{
				result = RequestADDCVDsForACE(declaration);
			}

			return result;
		}

		bool RequestADDCVDsForACE(JobDeclaration declaration)
		{
			bool result = false;
			Dictionary<ZString, bool> caseNumbers = new Dictionary<ZString, bool>();
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				if (!invoiceLine.US_ADDCaseNo.IsEmpty)
				{
					if (!caseNumbers.ContainsKey(invoiceLine.US_ADDCaseNo))
					{
						caseNumbers.Add(invoiceLine.US_ADDCaseNo, true);
					}
				}
				if (!invoiceLine.US_CVDCaseNo.IsEmpty)
				{
					if (!caseNumbers.ContainsKey(invoiceLine.US_CVDCaseNo))
					{
						caseNumbers.Add(invoiceLine.US_CVDCaseNo, true);
					}
				}
			}

			if (caseNumbers.Count > 0)
			{
				var message = new ACEACQueryMessageBuilder().Generate(factory, caseNumbers.Keys);
				message.EM_LinkedObject = declaration;
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ADDCVDDutyQuery;
				declaration.Messages.Add(message);
				result = true;
			}
			return result;
		}

		public bool RequestADDCVDsByTariff(VisaQuotaADDCVDQuerySendingActionCollection actions)
		{
			bool result = false;
			if (actions.declaration.IsACE)
			{
				result = RequestADDCVDsByTariffForACE(actions);
			}
			else
			{
				result = RequestADDCVDsByTariffForACS(actions);
			}
			return result;
		}

		bool RequestADDCVDsByTariffForACE(VisaQuotaADDCVDQuerySendingActionCollection actions)
		{
			var caseRequests = new List<ACEACCaseQueryInput>();

			foreach (VisaQuotaADDCVDQuerySendingAction action in actions)
			{
				if (action.US_SendMessage)
				{
					var queryInput = new ACEACCaseQueryInput();
					queryInput.CaseStatus = "B";
					queryInput.HTSNumber = action.US_TariffNumber;
					queryInput.CountryCode = action.US_UC_NKOrigin;

					if (!caseRequests.Contains(queryInput))
					{
						caseRequests.Add(queryInput);
					}
				}
			}

			foreach (var request in caseRequests)
			{
				var message = new ACEACQueryMessageBuilder().Generate(factory, request);
				actions.declaration.Messages.Add(message);
				message.EM_LinkedObject = actions.declaration;
			}

			return caseRequests.Count > 0;
		}

		bool RequestADDCVDsByTariffForACS(VisaQuotaADDCVDQuerySendingActionCollection actions)
		{
			List<ADDCVDQueryRequest> caseRequests = new List<ADDCVDQueryRequest>();

			foreach (VisaQuotaADDCVDQuerySendingAction action in actions)
			{
				if (action.US_SendMessage)
				{
					CreateADDCVDQueryRequest(caseRequests, ZString.Empty, action.US_TariffNumber, action.US_UC_NKOrigin);
				}
			}

			SendADDCVDQueryRequests(caseRequests, actions.declaration);

			return caseRequests.Count > 0;
		}

		public void RequestADDCVDsByCaseNumber(ZString caseNo)
		{
			List<ADDCVDQueryRequest> caseRequests = new List<ADDCVDQueryRequest>();
			CreateADDCVDQueryRequest(caseRequests, caseNo, "", "");
			SendADDCVDQueryRequests(caseRequests, null);
			SaveIfAutoSave();
		}

		void CreateADDCVDQueryRequest(List<ADDCVDQueryRequest> caseRequests, ZString caseNo, ZString tariff, ZString countryOfOrigin)
		{
			ADDCVDQueryRequest addRequest = new ADDCVDQueryRequest(caseNo, tariff, countryOfOrigin);
			if (!caseRequests.Contains(addRequest))
			{
				caseRequests.Add(addRequest);
			}
		}

		void SendADDCVDQueryRequests(List<ADDCVDQueryRequest> caseRequests, JobDeclaration declaration)
		{
			if (caseRequests.Count > 0)
			{
				BlockControlGenerator blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
				blockControlGenerator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery;

				foreach (ADDCVDQueryRequest caseRequest in caseRequests)
				{
					ACDC request = new ACDC();
					request.CaseNumber = caseRequest.caseNumber.Left(7);
					request.CaseNumberSuffix = caseRequest.caseNumber.Right(3);
					request.TariffNumber = caseRequest.tariffNumber;
					request.ISOCountryCode = caseRequest.country;

					blockControlGenerator.AddMessageBlock(request);
				}
				MQEDIMessage message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ADDCVDDutyQuery;
				if (declaration != null)
				{
					declaration.Messages.Add(message);
					message.EM_LinkedObject = declaration;
				}
			}
		}

		#endregion

		public void RequestImportSpecialistTeamAssignmentFile()
		{
			RequestRefFileSimple(new ERFF112());
		}

		public ICollection<EDIMessage> RequestTariffs(IEnumerable<TariffDate> tariffs, IMessageAttachee messageAttachee)
		{
			var messages = new List<EDIMessage>();
			TariffFormatter formatter = new TariffFormatter();
			BlockControlGenerator blockControlGenerator = null;
			int cnt = 0;
			foreach (TariffDate tariffDate in tariffs)
			{
				if (blockControlGenerator == null)
				{
					GlbBranch branch = messageAttachee != null ? messageAttachee.Branch : GlbBranch.CurrentBranch;
					blockControlGenerator = IsACEHTSReferenceFileQueryEnabled ? new ACEInputBlockControlGenerator(branch) : new ABIInputBlockControlGenerator(branch);
					blockControlGenerator.B.ApplicationIdentifier = IsACEHTSReferenceFileQueryEnabled ? ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQuery : ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
				}

				HTSW request = new HTSW();
				blockControlGenerator.AddMessageBlock(request);
				request.FromTariffNumber = formatter.Format(tariffDate.FromTariff);
				request.ToTariffNumber = formatter.Format(tariffDate.ToTariff);
				request.AsOfDate = tariffDate.Date;
				cnt++;
				if (cnt == 50)
				{
					messages.Add(blockControlGenerator.CreateMessage<MQEDIMessage>(factory));
					cnt = 0;
					blockControlGenerator = null;
				}
			}

			if (blockControlGenerator != null)
			{
				MQEDIMessage message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
				messages.Add(message);
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.RequestTariffUpdate;
				if (messageAttachee != null)
				{
					messageAttachee.Messages.Add(message);
				}
			}

			SaveIfAutoSave();
			return messages;
		}

		bool IsACEHTSReferenceFileQueryEnabled => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.ACEHTS, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today);

		public ICollection<EDIMessage> RequestTariffs(JobDeclaration declaration, bool allLines)
		{
			if (declaration != null && (declaration.IsImport || declaration.IsRecon || declaration.IsExport))
			{
				var tariffDates = GetTariffDatesToSendRequestFor(declaration, allLines);

				if (tariffDates.Count > 0)
				{
					return RequestTariffs(tariffDates.Keys, declaration);
				}
			}
			return Array.Empty<EDIMessage>();
		}

		Dictionary<TariffDate, bool> GetTariffDatesToSendRequestFor(JobDeclaration declaration, bool allLines)
		{
			bool isRecon = declaration.IsRecon;

			int countOfElementsInDictionary = isRecon ? declaration.InvoiceLines.Count * 2 : declaration.InvoiceLines.Count;

			var result = new Dictionary<TariffDate, bool>(countOfElementsInDictionary);
			declaration.AddTariffFetchHintsIfNeeded();
			var lines = declaration.IsExport ? declaration.InvoiceLines.GetInvoiceLinesWithTariffType(TariffTypeList.Codes.HTS).ToList() : declaration.InvoiceLines.Cast<JobComInvoiceLine>().ToList();

			foreach (var invoiceLine in lines)
			{
				if (!invoiceLine.JI_Tariff.IsEmpty)
				{
					if (allLines || (invoiceLine.TariffMarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportTariff)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.JI_Tariff, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.TariffMarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.HasEmptySupTariff)
				{
					if (allLines || (invoiceLine.SupTariffMarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupTariff)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupTariff, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupTariffMarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.US_SupAdditionalTariff1.IsEmpty)
				{
					if (allLines || (invoiceLine.SupAdditionalTariff1MarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupAdditionalTariff1)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupAdditionalTariff1, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupAdditionalTariff1MarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.US_SupAdditionalTariff2.IsEmpty)
				{
					if (allLines || (invoiceLine.SupAdditionalTariff2MarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupAdditionalTariff2)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupAdditionalTariff2, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupAdditionalTariff2MarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.US_SupAdditionalTariff3.IsEmpty)
				{
					if (allLines || (invoiceLine.SupAdditionalTariff3MarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupAdditionalTariff3)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupAdditionalTariff3, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupAdditionalTariff3MarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.US_SupAdditionalTariff4.IsEmpty)
				{
					if (allLines || (invoiceLine.SupAdditionalTariff4MarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupAdditionalTariff4)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupAdditionalTariff4, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupAdditionalTariff4MarkedForReferenceFileRequest = false;
					}
				}

				if (!invoiceLine.US_SupAdditionalTariff5.IsEmpty)
				{
					if (allLines || (invoiceLine.SupAdditionalTariff5MarkedForReferenceFileRequest && IsTariffCandidateForUpdateRequest(invoiceLine.EffectiveDateForDutyRate, invoiceLine.ImportSupAdditionalTariff5)))
					{
						TariffDate tariffDate = new TariffDate(invoiceLine.US_SupAdditionalTariff5, "", invoiceLine.EffectiveDateForDutyRate);
						result[tariffDate] = true;
						invoiceLine.SupAdditionalTariff5MarkedForReferenceFileRequest = false;
					}
				}

				if (isRecon)
				{
					if (!invoiceLine.US_R_OrigTariff.IsEmpty)
					{
						if (allLines || (invoiceLine.ReconOrigTariffMarkedForReferenceFileRequest && invoiceLine.ReconOrigTariffIsCandidateForUpdateRequest))
						{
							TariffDate tariffDate = new TariffDate(invoiceLine.US_R_OrigTariff, "", invoiceLine.EffectiveDateForDutyRate);
							result[tariffDate] = true;
							invoiceLine.ReconOrigTariffMarkedForReferenceFileRequest = false;
						}
					}

					if (!invoiceLine.US_R_OrigSupTariff.IsEmpty)
					{
						if (allLines || (invoiceLine.ReconOrigSupTariffMarkedForReferenceFileRequest && invoiceLine.ReconOrigSupTariffIsCandidateForUpdateRequest))
						{
							TariffDate tariffDate = new TariffDate(invoiceLine.US_R_OrigSupTariff, "", invoiceLine.EffectiveDateForDutyRate);
							result[tariffDate] = true;
							invoiceLine.ReconOrigSupTariffMarkedForReferenceFileRequest = false;
						}
					}
				}
			}

			return result;
		}

		bool IsTariffCandidateForUpdateRequest(ZDate effectiveDateForDutyRate, USCTariff tariff)
		{
			return effectiveDateForDutyRate.IsValid && tariff == null;
		}

		public ICollection<EDIMessage> RequestTariffs(ReconDeclaration reconDeclaration, bool allLines)
		{
			if (reconDeclaration != null)
			{
				var tariffDates = GetTariffDatesToSendRequestFor(reconDeclaration.ReconWrappedJobDeclaration, allLines);

				if (tariffDates.Count > 0)
				{
					return RequestTariffs(tariffDates.Keys, reconDeclaration);
				}
			}
			return Array.Empty<EDIMessage>();
		}

		public MQEDIMessage RequestTariffByUpdate(int updateNumber)
		{
			List<MQEDIMessage> result = RequestByUpdateNumber(new int[] { updateNumber });
			return result[0];
		}

		List<MQEDIMessage> RequestByUpdateNumber(IEnumerable<int> updateNumbers)
		{
			List<MQEDIMessage> messages = new List<MQEDIMessage>();

			BlockControlGenerator blockControlGenerator;
			if (IsACEHTSReferenceFileQueryEnabled)
			{
				blockControlGenerator = new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch);
				blockControlGenerator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQuery;
			}
			else
			{
				blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
				blockControlGenerator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			}

			ERFF110 request = new ERFF110();
			blockControlGenerator.AddMessageBlock(request);
			foreach (int updateNumber in updateNumbers)
			{
				request.HarmonizedUpdateNumber = updateNumber;
				messages.Add(blockControlGenerator.CreateMessage<MQEDIMessage>(factory));
			}
			SaveIfAutoSave();
			return messages;
		}

		public void RequestExchangeRates()
		{
			BlockControlGenerator blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			blockControlGenerator.B.ApplicationIdentifier = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			ERFF108 request = new ERFF108();
			request.ExchangeRateDateBeginning = ZDate.Today.AddDays(-1);
			request.ExchangeRateDateEnding = ZDate.Today;
			blockControlGenerator.AddMessageBlock(request);
			blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
			SaveIfAutoSave();
		}

		public void RequestBulkVisaQuery()
		{
			USCCountry[] allCountries = factory.Load<USCCountry>(new ZQuery());

			QTAU1 u1 = new QTAU1();

			foreach (USCCountry country in allCountries)
			{
				u1.CountryOfOrigin = country.UC_Code;
				u1.VisaQueryIndicator = QueryTypeList.Codes.AllVisaRecordsForACountry;
				RequestSimple(ApplicationIdentifierCodeList.Codes.QueryQuota, u1, null, EM_MessageSubTypeList.Codes.QuotaVisaQuery);
			}
		}

		public void RequestCarrierCodesForBill(JobDeclaration declaration)
		{
			if (declaration != null && declaration.IsImport)
			{
				ERFF106 erff106 = null;

				var blockControlGenerator = (BlockControlGenerator)new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch);
				blockControlGenerator.B.ApplicationIdentifier = ACEApplicationIdentifierCodeList.Codes.ExtractReference;

				var listOfSCACs = new List<ZString>();
				declaration.AddCarrierFetchHintsIfNeeded();
				foreach (Bill bill in declaration.Bills)
				{
					if ((bill.IssuerCarrierCodeMarkedForRequest && bill.IsIssuerCarrierCodeCandidateForUpdate))
					{
						var scac = bill.US_UI_NKBillIssuerSCAC;
						if (!listOfSCACs.Contains(scac))
						{
							erff106 = new ERFF106();
							erff106.CarrierCode = scac;

							blockControlGenerator.AddMessageBlock(erff106);
							listOfSCACs.Add(scac);
						}
						bill.IssuerCarrierCodeMarkedForRequest = false;
					}
				}

				if (declaration.CarrierCodeMarkedForRequest && declaration.IsCarrierCodeCandidateForUpdate)
				{
					var scac = declaration.US_UI_NKCarrierSCAC;
					if (!listOfSCACs.Contains(scac))
					{
						erff106 = new ERFF106();
						erff106.CarrierCode = declaration.US_UI_NKCarrierSCAC;

						blockControlGenerator.AddMessageBlock(erff106);
					}
					declaration.CarrierCodeMarkedForRequest = false;
				}

				if (blockControlGenerator.MessageBlocks.Count > 0)
				{
					MQEDIMessage message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
					message.EM_LinkedObject = declaration;
					declaration.Messages.Add(message);

					SaveIfAutoSave();
				}
			}
		}

		public void RequestCountry()
		{
			RequestRefFileSimple(new ERFF102());
		}

		public void RequestForeignPortCodes()
		{
			RequestRefFileSimple(new ERFF104());
		}

		public void RequestCarrierCode()
		{
			RequestRefFileSimple(new ERFF106());
		}

		public void RequestFIRMSCode()
		{
			ERFF111 f111 = new ERFF111();
			f111.BeginDate = ReferenceFileRequester.FIRMSBeginDate;
			RequestRefFileSimple(f111);
		}

		[ThreadStatic]
		static ZDate firmsBeginDate;

		public static ZDate FIRMSBeginDate
		{
			get { return firmsBeginDate.IsEmpty ? firmsBeginDate = new ZDate(2000, 1, 1) : firmsBeginDate; }
		}

		internal MQEDIMessage RequestSimple(string applicationIdentifier, MessageBlock request, BusinessObject parent, string messageSubType)
		{
			var blockControlGenerator = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			blockControlGenerator.B.ApplicationIdentifier = applicationIdentifier;
			blockControlGenerator.AddMessageBlock(request);
			var message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
			message.EM_LinkedObject = parent;
			message.EM_MessageSubType = messageSubType;
			SaveIfAutoSave();
			return message;
		}

		MQEDIMessage RequestSimpleACE(string applicationIdentifier, MessageBlock request, BusinessObject parent, string messageSubType)
		{
			var blockControlGenerator = new ACEInputBlockControlGenerator(GlbBranch.CurrentBranch);
			blockControlGenerator.B.ApplicationIdentifierCode = applicationIdentifier;
			blockControlGenerator.AddMessageBlock(request);
			var message = blockControlGenerator.CreateMessage<MQEDIMessage>(factory);
			message.EM_LinkedObject = parent;
			message.EM_MessageSubType = messageSubType;
			SaveIfAutoSave();
			return message;
		}

		public MQEDIMessage RequestRefFileSimple(MessageBlock request)
		{
			return RequestSimpleACE(ACEApplicationIdentifierCodeList.Codes.ExtractReference, request, null, "");
		}

		void SaveIfAutoSave()
		{
			if (autoSave)
			{
				factory.Save();
			}
		}
	}
}
