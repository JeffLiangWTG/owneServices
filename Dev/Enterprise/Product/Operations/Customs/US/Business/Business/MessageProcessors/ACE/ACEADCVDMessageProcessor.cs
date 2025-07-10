using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse)]
	class ACEADCVDMessageProcessor : ACEABIWithDatabaseLockProcessor
	{
		const string MsgSubject = "AntiDumping/Countervailing Query";
		const int LargeDataCheckLimit = 350000;

		public override void Process()
		{
			var bizObj = OriginalMessageLinker.Link(Message);
			var originalMessage = Message.OriginalMessage;
			var transmissionDate = ZDate.Empty;
			var dataVersion = originalMessage == null ? GetDataVersionIfApplicable(out transmissionDate) : null;
			if (ShouldProcess(dataVersion, transmissionDate))
			{
				List<USCACCase> acCases = ProcessMessageBlocks();
				Message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACE_AD_CVDQueryResponse;
				if (dataVersion != null && transmissionDate.IsValid)
				{
					dataVersion.SetNoteWithDatabaseDetail("Updated");
					dataVersion.UZ_UpdateTime = transmissionDate;
				}

				if (originalMessage == null || originalMessage.EM_SystemCreateUser != Enterprise.ZArchitecture.Environment.User.ServiceUserCode)
				{
					var firstRXBlock = GetFirstMessageBlock<AADQRX>();
					bool isFailure = firstRXBlock != null && !IsQueryCompleteCode(firstRXBlock.ConditionCode);

					string emailBody = GetEmailBody(originalMessage, acCases);
					GenerateHtmlEmailAndSendToOriginalOrGroup("", "", MsgSubject, emailBody, isFailure, GlbBranch.CurrentBranch, bizObj);
				}
			}
			else
			{
				throw new Enterprise.Messaging.Business.MessageProcessDiscardedException("Similar unsolicited message has already been proccessed.");
			}
		}

		bool ShouldProcess(USCDataVersion dataVersion, ZDate transmissionDate)
		{
			return dataVersion == null || !dataVersion.IsInDatabase || !transmissionDate.IsValid ||
				dataVersion.UZ_UpdateTime < transmissionDate ||
				(dataVersion.UZ_UpdateTime == transmissionDate && dataVersion.GetDatabaseNameFromNote() == Db.DatabaseName);
		}

		USCDataVersion GetDataVersionIfApplicable(out ZDate transmissionDate)
		{
			transmissionDate = ZDate.Empty;
			USCDataVersion result = null;
			var interchange = Message.Interchange;
			if (interchange != null && interchange.EI_BodyText.Length > LargeDataCheckLimit)
			{
				var aabioutputa = new AABIOutputA();
				if (!interchange.EI_HeaderText.IsEmpty)
				{
					aabioutputa.Deserialise(interchange.EI_HeaderText.PadRight(80));
				}
				transmissionDate = aabioutputa.TransmissionDate;
				var aadqra = GetFirstMessageBlock<AADQRA>();
				if (aadqra != null)
				{
					result = USCDataVersion.GetOrCreate(Message.Factory, GetDataVersionName(aadqra.CountryCode));
				}
			}
			return result;
		}

		public static string GetDataVersionName(ZString code)
		{
			return "ACEAntiDumping_" + code;
		}

		string GetEmailBody(MQEDIMessage originalMessage, List<USCACCase> acCases)
		{
			string result = string.Empty;

			var q2Block = originalMessage?.MessageBlock.MessageBlocks.OfType<AADQQ2>().FirstOrDefault();
			if (q2Block != null)
			{
				var queryDetailsTable = new HtmlTableCreator();
				if (!q2Block.CompanyCaseStatus.IsEmpty)
				{
					queryDetailsTable.WriteRow("Case Status", q2Block.CompanyCaseStatus);
				}

				if (!q2Block.CountryCode.IsEmpty)
				{
					queryDetailsTable.WriteRow("Country", q2Block.CountryCode);
				}

				if (!q2Block.HTSNumber.IsEmpty)
				{
					queryDetailsTable.WriteRow("HTS", q2Block.HTSNumber);
				}

				if (!q2Block.ManufacturerIdentificationCode.IsEmpty)
				{
					queryDetailsTable.WriteRow("Manufacturer MID", q2Block.ManufacturerIdentificationCode);
				}

				if (!q2Block.ForeignExporterIdentificationCode.IsEmpty)
				{
					queryDetailsTable.WriteRow("Foreign Shipper MID", q2Block.ForeignExporterIdentificationCode);
				}

				if (!q2Block.DateSinceLastUpdate.IsEmpty)
				{
					queryDetailsTable.WriteRow("Date Since Last Update", q2Block.DateSinceLastUpdate);
				}

				if (!q2Block.TSUSANumber.IsEmpty)
				{
					queryDetailsTable.WriteRow("TSUSA", q2Block.TSUSANumber);
				}

				result = queryDetailsTable.ToHtml();

				if (!string.IsNullOrWhiteSpace(result))
				{
					result += "<br><br>";
				}
			}

			var uris = new ZStringBuilder();
			foreach (var acCase in acCases)
			{
				var uri = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(acCase);
				uris.Append("<a href=\"" + uri + "\">" + acCase.U5_CaseNumber + "</a>");
			}

			if (!uris.IsEmpty)
			{
				result += uris.ToStringWithDelimiterBetweenAppends(", ");
			}

			AADQRX[] rxBlocks = GetMessageBlocks<AADQRX>(999);
			if (rxBlocks.Length > 0)
			{
				HtmlTableCreator table = new HtmlTableCreator(new string[] { "Case Number", "Condition Code", "Message" });

				foreach (var block in rxBlocks)
				{
					table.WriteRow((block.ReferenceDataTypeCode == Q1ConditionDataTypeCode ? block.ReferenceDataText : ZString.Empty),
								block.ConditionCode, block.NarrativeText);
				}

				if (!string.IsNullOrWhiteSpace(result))
				{
					result += "<br><br>";
				}

				result += table.ToHtml();
			}

			return result;
		}

		List<USCACCase> ProcessMessageBlocks()
		{
			List<USCACCase> result = new List<USCACCase>();
			USCACCase acCase = null;

			foreach (var block in messageBlocks.OfType<AADQRA>())
			{
				Factory.AddFetchHint(typeof(USCACCase), new ZQuery(USCACCaseSchema.U5_CaseNumber, block.CaseNumber));
				Factory.AddFetchHint(typeof(USCACCaseRate), new ZQuery(USCACCaseRateSchema.U6_CaseNumber, block.CaseNumber));
				Factory.AddFetchHint(typeof(USCACCaseEvent), new ZQuery(USCACCaseEventSchema.U7_CaseNumber, block.CaseNumber));
				Factory.AddFetchHint(typeof(USCACCaseBondCash), new ZQuery(USCACCaseBondCashSchema.U8_CaseNumber, block.CaseNumber));
				Factory.AddFetchHint(typeof(USCACCaseTariff), new ZQuery(USCACCaseTariffSchema.U9_CaseNumber, block.CaseNumber));
				Factory.AddFetchHint(typeof(USCACCaseLiqSuspension), new ZQuery(USCACCaseLiqSuspensionSchema.UN_CaseNumber, block.CaseNumber));
			}

			foreach (MessageBlock block in messageBlocks)
			{
				AADQRA ra = block as AADQRA;
				if (ra != null)
				{
					acCase = ProcessRA(ra);
					if (!result.Contains(acCase))
					{
						result.Add(acCase);
					}
				}
				else
				{
					AADQRB rb = block as AADQRB;
					if (rb != null)
					{
						ProcessRB(rb, acCase);
					}
					else
					{
						AADQRC rc = block as AADQRC;
						if (rc != null)
						{
							ProcessRC(rc, acCase);
						}
						else
						{
							AADQRD rd = block as AADQRD;
							if (rd != null)
							{
								ProcessRD(rd, acCase);
							}
							else
							{
								AADQRE re = block as AADQRE;
								if (re != null)
								{
									ProcessRE(re, acCase);
								}
								else
								{
									AADQRF rf = block as AADQRF;
									if (rf != null)
									{
										ProcessRF(rf, acCase);
									}
									else
									{
										AADQRG rg = block as AADQRG;
										if (rg != null)
										{
											ProcessRG(rg, acCase);
										}
										else
										{
											AADQRH rh = block as AADQRH;
											if (rh != null)
											{
												ProcessRH(rh, acCase);
											}
											else
											{
												AADQRI ri = block as AADQRI;
												if (ri != null)
												{
													ProcessRI(ri, acCase);
												}
												else
												{
													AADQRJ rj = block as AADQRJ;
													if (rj != null)
													{
														ProcessRJ(rj, acCase);
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
			return result;
		}

		USCACCase ProcessRA(AADQRA ra)
		{
			USCACCase result = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, ra.CaseNumber);
			if (result == null)
			{
				result = Factory.New<USCACCase>();
				result.U5_CaseNumber = ra.CaseNumber;
			}
			else
			{
				result.BondCashIndicators.MarkForDelete();
				result.CaseEvents.MarkForDelete();
				result.CaseRates.MarkForDelete();

				var originalQuery = Message.OriginalMessage;
				if (originalQuery != null)
				{
					//if it queried for a specific tariff number, response does not contain all the tariff information
					var q2 = originalQuery.MessageBlock.MessageBlocks.OfType<AADQQ2>().FirstOrDefault();
					if (q2 == null || q2.HTSNumber.IsEmpty)
					{
						result.CaseTariffs.MarkForDelete();
					}
				}

				result.LiqSuspensions.MarkForDelete();
			}

			result.U5_CaseStatus = ra.CompanyCaseStatus;
			result.U5_CaseStatusDate = ra.CompanyCaseStatusEffectiveDate;
			result.U5_RelatedCaseNumber = ra.RelatedCaseNumber;
			result.U5_ShortDescription = ra.ShortDescription;
			result.U5_ISOCountryCode = ra.CountryCode;

			result.U5_ManufacturerName = ZString.Empty;
			result.U5_ForeignExporterName = ZString.Empty;
			result.U5_OfficialName = ZString.Empty;

			return result;
		}

		void ProcessRB(AADQRB rb, USCACCase acCase)
		{
			acCase.U5_OfficialName += rb.OfficialCaseName;
		}

		void ProcessRC(AADQRC rc, USCACCase acCase)
		{
			if (rc.RecordSequence == "1")
			{
				acCase.U5_ManufacturerMID = rc.ManufacturerIdentificationCode;
			}

			acCase.U5_ManufacturerName += rc.ManufacturerName;
		}

		void ProcessRD(AADQRD rd, USCACCase acCase)
		{
			if (rd.RecordSequence == "1")
			{
				acCase.U5_ForeignExporterMID = rd.ForeignExporterIdentificationCode;
			}

			acCase.U5_ForeignExporterName += rd.ForeignExporterName;
		}

		void ProcessRE(AADQRE re, USCACCase acCase)
		{
			acCase.U5_ContactOffice = re.ContactOffice;
			acCase.U5_ContactName = re.ContactName;
			acCase.U5_Phone1 = re.ContactTelephoneNumber1 + (!re.ContactTelephoneNumber1Extension.IsEmpty ? "|" + re.ContactTelephoneNumber1Extension : "");
			acCase.U5_Phone2 = re.ContactTelephoneNumber2 + (!re.ContactTelephoneNumber2Extension.IsEmpty ? "|" + re.ContactTelephoneNumber2Extension : "");
		}

		void ProcessRF(AADQRF rf, USCACCase acCase)
		{
			USCACCaseRate rate = null;
			foreach (var possible in acCase.CaseRates)
			{
				if (possible.U6_EffectiveDate == rf.DepositRateEffectiveDate
					&& possible.U6_AddedDate == rf.RateAddedDate && possible.U6_InactivatedDate == rf.RateInactivatedDate)
				{
					rate = possible;
					rate.RemoveOnFactorySaving = false;
					break;
				}
			}

			if (rate == null)
			{
				rate = acCase.CaseRates.AddNew();
				rate.U6_CaseNumber = rf.CaseNumber;
				rate.U6_EffectiveDate = rf.DepositRateEffectiveDate;
				rate.U6_AddedDate = rf.RateAddedDate;
				rate.U6_InactivatedDate = rf.RateInactivatedDate;
			}

			rate.U6_AdValoremRate = rf.AdValoremDepositRate / 100;
			rate.U6_SpecificRate = rf.SpecificDepositRate;
			rate.U6_Unit = rf.UnitOfMeasure;
			rate.U6_UnitDesc = rf.OtherUnitOfMeasure;
		}

		void ProcessRG(AADQRG rg, USCACCase acCase)
		{
			USCACCaseEvent caseEvent = null;
			foreach (var possible in acCase.CaseEvents)
			{
				if (possible.U7_EffectiveDate == rg.EventEffectiveDate)
				{
					caseEvent = possible;
					caseEvent.RemoveOnFactorySaving = false;
					break;
				}
			}

			if (caseEvent == null)
			{
				caseEvent = acCase.CaseEvents.AddNew();
				caseEvent.U7_CaseNumber = rg.CaseNumber;
				caseEvent.U7_EffectiveDate = rg.EventEffectiveDate;
			}

			caseEvent.U7_Event = rg.Event;
			caseEvent.U7_Determination = rg.Determination;
			caseEvent.U7_FedRegCitation = rg.FederalRegisterCitation;
			caseEvent.U7_AddedDate = rg.EventAddedDate;
			caseEvent.U7_InactivatedDate = rg.EventInactivatedDate;
		}

		void ProcessRH(AADQRH rh, USCACCase acCase)
		{
			USCACCaseBondCash bondCash = null;
			foreach (var possible in acCase.BondCashIndicators)
			{
				if (possible.U8_EffectiveDate == rh.BondCashEffectiveDate)
				{
					bondCash = possible;
					bondCash.RemoveOnFactorySaving = false;
					break;
				}
			}

			if (bondCash == null)
			{
				bondCash = acCase.BondCashIndicators.AddNew();
				bondCash.U8_EffectiveDate = rh.BondCashEffectiveDate;
				bondCash.U8_CaseNumber = rh.CaseNumber;
			}

			string indicator = "";

			switch (rh.BondCashIndicator.ToUpper())
			{
				case BondCashIndicators.CustomsCodes.BondOrCash:
					indicator = BondCashIndicatorList.Codes.BOC;
					break;

				case BondCashIndicators.CustomsCodes.CashOnly:
					indicator = BondCashIndicatorList.Codes.Cash;
					break;

				case BondCashIndicators.CustomsCodes.NotApplicable:
					indicator = BondCashIndicatorList.Codes.NA;
					break;
			}

			bondCash.U8_Indicator = indicator;
			bondCash.U8_AddedDate = rh.BondCashIndicatorAddedDate;
			bondCash.U8_InactivatedDate = rh.BondCashIndicatorInactivatedDate;
		}

		static class BondCashIndicators
		{
			public static class CustomsCodes
			{
				public const string BondOrCash = "BOND OR CA";
				public const string CashOnly = "CASH ONLY";
				public const string NotApplicable = "N/A";
			}
		}

		void ProcessRI(AADQRI ri, USCACCase acCase)
		{
			USCACCaseTariff tariff1 = null, tariff2 = null, tariff3 = null;

			foreach (var possible in acCase.CaseTariffs)
			{
				if (possible.U9_TariffNumber == ri.TariffNumber1
					&& possible.U9_AddedDate == ri.AddedDate)
				{
					tariff1 = possible;
					tariff1.RemoveOnFactorySaving = false;
				}
				else if (possible.U9_TariffNumber == ri.TariffNumber2
					&& possible.U9_AddedDate == ri.AddedDate1)
				{
					tariff2 = possible;
					tariff2.RemoveOnFactorySaving = false;
				}
				else if (possible.U9_TariffNumber == ri.TariffNumber3
					&& possible.U9_AddedDate == ri.AddedDate2)
				{
					tariff3 = possible;
					tariff3.RemoveOnFactorySaving = false;
				}
			}

			CreateOrUpdateTariff(tariff1, acCase, ri.TariffNumber1, ri.AddedDate, ri.InactivatedDate);
			CreateOrUpdateTariff(tariff2, acCase, ri.TariffNumber2, ri.AddedDate1, ri.InactivatedDate1);
			CreateOrUpdateTariff(tariff3, acCase, ri.TariffNumber3, ri.AddedDate2, ri.InactivatedDate2);
		}

		void CreateOrUpdateTariff(USCACCaseTariff caseTariff, USCACCase acCase, ZString tariffNumber, ZDate addedDate, ZDate inactivatedDate)
		{
			if (caseTariff == null && !tariffNumber.IsEmpty)
			{
				caseTariff = acCase.CaseTariffs.AddNew();
				caseTariff.U9_CaseNumber = acCase.U5_CaseNumber;
				caseTariff.U9_TariffNumber = tariffNumber;
				caseTariff.U9_AddedDate = addedDate;
			}

			if (caseTariff != null)
			{
				caseTariff.U9_InactivatedDate = inactivatedDate;
			}
		}

		void ProcessRJ(AADQRJ rj, USCACCase acCase)
		{
			var suspension = acCase.LiqSuspensions.AddNew();
			suspension.UN_CaseNumber = rj.CaseNumber;
			suspension.UN_Action = rj.SuspensionAction;
			suspension.UN_EffectiveDate = rj.SuspensionActionEffectiveDate;
			suspension.UN_AddedDate = rj.SuspensionActionAddedDate;
			suspension.UN_InactivatedDate = rj.SuspensionActionInactivatedDate;
		}

		bool IsQueryCompleteCode(ZString code)
		{
			return code == "014" || code == "003";
		}

		const string Q1ConditionDataTypeCode = "Q1E";

		protected sealed override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.ACEAntiDumping; }
		}
	}
}
