using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public class ACSTariffResponseProcessor : TariffResponseProcessor<APLA, APLB, APLY>
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public class ACETariffResponseProcessor : TariffResponseProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	[TopLevel(typeof(ERFF110))]
	public abstract class TariffResponseProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
			OriginalMessageLinker.Link(Message);

			HtmlTableCreator htmlTable = null;
			string emailDesc = "";
			ZStringBuilder emailBody = new ZStringBuilder();
			GlbStaff orginatingUser = null;

			var isHTSRequested = Message.OriginalMessage != null;
			if (isHTSRequested)
			{
				emailDesc = "This Harmonized Tariff Schedule update is sent in response to your HTS query on " + Message.OriginalMessage.EM_SystemCreateTimeUtc.ToLongTimeString() + "\r\n";
			}

			foreach (ERFF110 block in messageBlocks)
			{
				var lastHTSAttemptObj = USCDataVersion.GetLastHTSAttempt(Factory);
				var lastHTSAttempt = lastHTSAttemptObj.UZ_Version;
				var updateNumberInBlock = block.HarmonizedUpdateNumber;

				if (lastHTSAttempt == updateNumberInBlock)
				{
					var sender = new TariffRequestAttemptSender(lastHTSAttempt, Factory);

					bool sendMessage = false;
					ZInt nextAttempt = 0;

					if (failureMessages.Contains(block.NarrativeMessage.Trim().ToString()))
					{
						lastHTSAttemptObj.SetNoteWithDatabaseDetailAndUpdateTime(ExtractReferenceFilesResult.Failure);

						if (sender.CurrentYearNotEqualAttemptYear)
						{
							nextAttempt = sender.NextAttemptForYearCutover;
							sendMessage = isHTSRequested;
						}
					}
					else
					{
						lastHTSAttemptObj.SetNoteWithDatabaseDetailAndUpdateTime(ExtractReferenceFilesResult.Success);

						nextAttempt = sender.NextAttempt;
						sendMessage = isHTSRequested;
					}

					sender.SendMessageIfAllowed(sendMessage, nextAttempt, lastHTSAttemptObj);
					if (block.ToHarmonizedNumber.IsEmpty)
					{
						emailBody.Append(ZString.Format("<br />HTS Queried: <b>{0}</b>", TariffFormatter.DisplayFormat(block.FromHarmonizedNumber)));
					}
					else
					{
						emailBody.Append(ZString.Format("<br />Query Range: From <b>{0}</b> To <b>{1}</b><br />", TariffFormatter.DisplayFormat(block.FromHarmonizedNumber), TariffFormatter.DisplayFormat(block.ToHarmonizedNumber)));
					}

					WriteTable(ref htmlTable, block.NarrativeMessage);
					emailBody.Append(htmlTable.ToHtml());
					htmlTable = null;
				}

				if (Message.OriginalMessage != null && Message.OriginalMessage.EM_SystemCreateUser != User.ServiceUserCode)
				{
					EmailDef email;
					GlbBranch branch = orginatingUser != null ? Factory.Load<GlbBranch>(orginatingUser.GS_GB_HomeBranch) : null;
					GenerateHtmlEmail(
								"HTS Query Request",
								"Updated details for the following Harmonized Tariff Schedule (HTS) data have been received:",
								emailDesc,
								emailBody.ToString(),
								ZString.Empty,
								out email, branch);

					SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branch, false);
				}
			}
		}

		readonly string[] failureMessages = { "NO UPDATES FOUND IN THIS RANGE", "NOT ON FILE OR EXPIRED", "NONE IN FILE OR EXPIRED" };

		void WriteTable(ref HtmlTableCreator htmlTable, ZString narrativeMessage)
		{
			htmlTable = new HtmlTableCreator(new string[] { "Column", "Value" });
			WriteToHtmlTable(htmlTable, "Details ", narrativeMessage);
		}

		void WriteToHtmlTable(HtmlTableCreator table, string columnHeader, IZType data)
		{
			if (!data.IsEmpty)
			{
				table.WriteRow(columnHeader, data);
			}
		}

		public override int MaximumTopLevelMessageBlocksPerProcessor
		{
			get { return 1; }
		}

		#region TariffFormatter

		protected TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = new TariffFormatter();
				}
				return fTariffFormatter;
			}
		}
		TariffFormatter fTariffFormatter;

		#endregion

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Tariff; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	public class ACSTariffHeaderResponseProcessor : TariffHeaderResponseProcessor<APLA, APLB, APLY>
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public class ACETariffHeaderResponseProcessor : TariffHeaderResponseProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	[TopLevel(typeof(HTSW0))]
	public abstract class TariffHeaderResponseProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
			OriginalMessageLinker.Link(Message);

			HtmlTableCreator htmlTable = null;
			string emailDesc = "";
			ZStringBuilder emailBody = new ZStringBuilder();
			GlbStaff orginatingUser = null;

			if (Message.OriginalMessage != null)
			{
				emailDesc = "This tariff update is sent in response to your tariff query on " + Message.OriginalMessage.EM_SystemCreateTimeUtc.ToLongTimeString() + "\r\n";
			}

			foreach (HTSW0 block in messageBlocks)
			{
				if (block.NarrativeMessage == "NOT ON FILE OR EXPIRED" ||
					block.NarrativeMessage == "NONE IN FILE OR EXPIRED")
				{
					ZDateTime effectiveDate = block.AsOfDate.IsEmpty ? ZDateTime.Today : block.AsOfDate;
					USCTariff[] tariffs = new USCTariff.Loader(Factory).LoadCurrentlyValidTariffs(block.FromTariffNumber, effectiveDate);
					foreach (USCTariff tariff in tariffs)
					{
						tariff.UE_DateTo = effectiveDate;
					}
				}

				if (block.ToTariffNumber.IsEmpty)
				{
					emailBody.Append(ZString.Format("<br />Tariff Queried: <b>{0}</b>", TariffFormatter.DisplayFormat(block.FromTariffNumber)));
				}
				else
				{
					emailBody.Append(ZString.Format("<br />Query Range: From <b>{0}</b> To <b>{1}</b><br />", TariffFormatter.DisplayFormat(block.FromTariffNumber), TariffFormatter.DisplayFormat(block.ToTariffNumber)));
				}

				WriteWOTable(ref htmlTable, block.NarrativeMessage);
				emailBody.Append(htmlTable.ToHtml());
				htmlTable = null;
			}

			if (Message.OriginalMessage != null && Message.OriginalMessage.EM_SystemCreateUser != User.ServiceUserCode)
			{
				EmailDef email;
				GlbBranch branch = orginatingUser != null ? Factory.Load<GlbBranch>(orginatingUser.GS_GB_HomeBranch) : null;
				GenerateHtmlEmail(
							"Tariff Query Request",
							"Updated details for the following tariff number(s) have been received:",
							emailDesc,
							emailBody.ToString(),
							ZString.Empty,
							out email, branch);

				SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branch, false);
			}
		}

		void WriteWOTable(ref HtmlTableCreator htmlTable, ZString narrativeMessage)
		{
			htmlTable = new HtmlTableCreator(new string[] { "Column", "Value" });
			WriteToHtmlTable(htmlTable, "Details ", narrativeMessage);
		}

		void WriteToHtmlTable(HtmlTableCreator table, string columnHeader, IZType data)
		{
			if (!data.IsEmpty)
			{
				table.WriteRow(columnHeader, data);
			}
		}

		public override int MaximumTopLevelMessageBlocksPerProcessor
		{
			get { return 1000; }
		}

		#region TariffFormatter

		protected TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = new TariffFormatter();
				}
				return fTariffFormatter;
			}
		}
		TariffFormatter fTariffFormatter;

		#endregion

		protected override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Tariff; }
		}
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem)]
	public class ACSSingleTariffProcessor : SingleTariffProcessor<APLA, APLB, APLY>
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public class ACESingleTariffProcessor : SingleTariffProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	[TopLevel(typeof(HTSW1), typeof(HTSW2), typeof(HTSW3), typeof(HTSW4), typeof(HTSW56789ABCEFGHIJK), typeof(HTSWD), typeof(HTSW0), typeof(HTSWL))]
	public abstract class SingleTariffProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : TariffProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		ZStringBuilder emailBody;
		ZString currentTariffNo;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public override void Process()
		{
			base.Process();

			HtmlTableCreator htmlTable = null;
			string emailDesc = "";
			emailBody = new ZStringBuilder();
			string emailFooter = "";
			JobDeclaration orginatingDec = null;

			OriginalMessageLinker.Link(Message);
			if (Message.OriginalMessage != null)
			{
				if (Message.OriginalMessage.EM_LinkTable == JobDeclarationSchema.Constants.TableName)
				{
					orginatingDec = Factory.Load<JobDeclaration>(Message.OriginalMessage.EM_LinkUniqueID);
					if (orginatingDec != null)
					{
						emailDesc = "This tariff update request was made on Job " + orginatingDec.JobNumber;
						emailFooter = "Note: You can refresh these Tariff details within your Job by choosing the menu option: 'Brokerage>Refresh Tariff Details'";
					}
				}
				else
				{
					emailDesc = "This tariff update is sent in response to your tariff query on " + Message.OriginalMessage.EM_SystemCreateTimeUtc.ToLongTimeString() + "\r\n";
					emailFooter = "";
				}

				int iSODutyOccurrance = 1;

				var firstW1Block = messageBlocks.OfType<IHTS1>().FirstOrDefault();
				var lastW1Block = messageBlocks.OfType<IHTS1>().LastOrDefault();
				var w0Block = messageBlocks.OfType<HTSW0>().FirstOrDefault();

				if (w0Block != null && w0Block.NarrativeMessage == "RANGE EXCEEDS 100 RECORDS")
				{
					emailBody.Append(ZString.Format("<br />Query Range: From <b>{0}</b> To <b>{1}</b><br />", w0Block.FromTariffNumber, w0Block.ToTariffNumber));
					emailBody.Append(ZString.Format("Response Tariff Range: From <b>{0}</b> To <b>{1}</b><br /><br />", firstW1Block.TariffNumber, lastW1Block.TariffNumber));
					WriteWOTable(ref htmlTable, w0Block.NarrativeMessage);
					emailBody.Append(htmlTable.ToHtml());
					htmlTable = null;
				}

				foreach (MessageBlock messageBlock in messageBlocks)
				{
					IHTS1 w1 = messageBlock as IHTS1;
					IHTS2 w2 = messageBlock as IHTS2;
					IHTS3 w3 = messageBlock as IHTS3;
					IHTS4 w4 = messageBlock as IHTS4;
					IHTS56789ABCEFGHIJK w5 = messageBlock as IHTS56789ABCEFGHIJK;
					IHTSL wL = messageBlock as IHTSL;

					if (w1 != null)
					{
						if (!currentTariffNo.IsEmpty)
						{
							AddDetailsToTariffBody(ref htmlTable);
						}

						currentTariffNo = TariffFormatter.DisplayFormat(w1.TariffNumber);
						htmlTable = new HtmlTableCreator(new string[] { "Column", "Value" });

						iSODutyOccurrance = 1;
						ZString units = w1.Unit1.IsEmpty ? "" : w1.NumberOfReportingUnits + " - Unit 1: " + w1.Unit1;
						if (w1.Unit2 != "")
						{
							units = units + "; Unit 2: " + w1.Unit2;
						}

						if (w1.Unit3 != "")
						{
							units = units + "; Unit 3: " + w1.Unit3;
						}

						string baseRateInd = w1.BaseRateIndicator.IsEmpty ? "" : "    Base Rate Ind.: " + w1.BaseRateIndicator;
						ZString effDateFrom = w1.RecordBeginEffectiveDate.ToString("MM/dd/yyyy");
						ZString effDateTo = w1.RecordEndEffectiveDate.ToString("MM/dd/yyyy");
						ZString dutyRate = w1.Column1SpecificRate.ToString(8, false);

						WriteToHtmlTable(htmlTable, "Short Description: ", w1.CommodityDescription);
						WriteToHtmlTable(htmlTable, "Effective Date From: ", effDateFrom);
						WriteToHtmlTable(htmlTable, "Effective Date To: ", effDateTo);
						WriteToHtmlTable(htmlTable, "Reporting Units: ", units);
						WriteToHtmlTable(htmlTable, "Duty Computation Code: ", w1.DutyComputationCode);
						WriteToHtmlTable(htmlTable, "Column 1 Specific Rate " + baseRateInd + ": ", dutyRate);
					}
					else if (w2 != null)
					{
						ZString adValoremRate1 = w2.Column1RateAdValorem.ToString(8, false);
						ZString otherRate1 = w2.Column1RateOther.ToString(8, false);
						ZString specificRate2 = w2.Column2RateSpecific.ToString(8, false);
						ZString adValoremRate2 = w2.Column2RateAdValorem.ToString(8, false);
						ZString otherRate2 = w2.Column2RateOther.ToString(8, false);

						WriteToHtmlTable(htmlTable, "Column 1 Ad Valorem Rate: ", adValoremRate1);
						WriteToHtmlTable(htmlTable, "Column 1 Other Rate: ", otherRate1);
						WriteToHtmlTable(htmlTable, "Column 2 Specific Rate: ", specificRate2);
						WriteToHtmlTable(htmlTable, "Column 2 Ad Valorem Rate: ", adValoremRate2);
						WriteToHtmlTable(htmlTable, "Column 2 Other Rate: ", otherRate2);
						WriteToHtmlTable(htmlTable, "Countervailing Duty Ind.: ", w2.CountervailingDutyFlag);
						WriteToHtmlTable(htmlTable, "Additional Tariff Number Ind.: ", w2.AdditionalTariffNumberIndicator);
						WriteToHtmlTable(htmlTable, "Miscellaneous Permit License Ind.: ", w2.MiscellaneousPermitLicenseIndicator);
					}
					else if (w3 != null)
					{
						WriteToHtmlTable(htmlTable, "Generalized System of Preferences (GSP) Excluded Countries: ", w3.GeneralizedSystemOfPreferencesGSPExcludedCountries);
						WriteToHtmlTable(htmlTable, "Antidumping Ind.: ", w3.AntidumpingDutyFlag);
						WriteToHtmlTable(htmlTable, "Quota Ind.: ", w3.QuotaIndicator);
						WriteToHtmlTable(htmlTable, "Category No.: ", w3.CategoryNumber);
						WriteToHtmlTable(htmlTable, "Special Program Indicator (SPI) Code: ", ZString.Join(", ", w3.SpecialProgramsIndicatorSPICode.Split(2)));
					}
					else if (wL != null)
					{
						WriteToHtmlTable(htmlTable, "Participating Government Agencies (PGA) Codes: ", ZString.Join(", ", wL.ParticipatingGovernmentAgencies.Split(3)));
					}
					else if (w4 != null)
					{
						WriteToHtmlTable(htmlTable, "Value Edit Code: ", w4.ValueEditCode);
						WriteToHtmlTable(htmlTable, "Value Low Bounds: ", w4.ValueLowBounds);
						WriteToHtmlTable(htmlTable, "Value High Bounds: ", w4.ValueHighBounds);
						WriteToHtmlTable(htmlTable, "Entry Date Restriction Code 1: ", w4.EntryDateRestrictionCode1);
						WriteToHtmlTable(htmlTable, "Begin Restriction Date 1: ", w4.BeginRestrictionDate1);
						WriteToHtmlTable(htmlTable, "End Restriction Date 1: ", w4.EndRestrictionDate1);
						WriteToHtmlTable(htmlTable, "Entry Date Restriction Code 2: ", w4.EntryDateRestrictionCode2);
						WriteToHtmlTable(htmlTable, "Begin Restriction Date 2: ", w4.BeginRestrictionDate2);
						WriteToHtmlTable(htmlTable, "End Restriction Date 2: ", w4.EndRestrictionDate2);
						WriteToHtmlTable(htmlTable, "ISO Country of Origin Edit Code: ", w4.ISOCountryOfOriginEditCode);
						WriteToHtmlTable(htmlTable, "Quantity Edit Code: ", w4.QuantityEditCode);
						WriteToHtmlTable(htmlTable, "Quantity Edit Low Bounds: ", w4.QuantityEditLowerBound);
						WriteToHtmlTable(htmlTable, "Quantity Edit High Bounds: ", w4.QuantityEditUpperBound);
					}
					else if (w5 != null)
					{
						ZString specificSpecialRate = w5.SpecificSpecialRate.ToString(8, false);
						ZString adValoremSpecialRate = w5.AdValoremSpecialRate.ToString(8, false);
						ZString otherSpecialRate = w5.OtherSpecialRate.ToString(8, false);
						ZString taxFeeSpecificRate = w5.TaxFeeSpecificRate.ToString(8, false);
						ZString taxFeeAdValorem = w5.TaxFeeAdValorem.ToString(8, false);

						WriteToHtmlTable(htmlTable, "ISO Country Code " + iSODutyOccurrance.ToString() + ": ", w5.InternationalOrganizationForStandardizationISOCountryCode);
						WriteToHtmlTable(htmlTable, "Specific Special Rate " + iSODutyOccurrance.ToString() + ": ", specificSpecialRate);
						WriteToHtmlTable(htmlTable, "Ad Valorem Special Rate " + iSODutyOccurrance.ToString() + ": ", adValoremSpecialRate);
						WriteToHtmlTable(htmlTable, "Other Special Rate " + iSODutyOccurrance.ToString() + ": ", otherSpecialRate);
						WriteToHtmlTable(htmlTable, "Tax/Fee Class Code " + iSODutyOccurrance.ToString() + ": ", w5.TaxFeeClassCode);
						WriteToHtmlTable(htmlTable, "Tax/Fee Computation Code " + iSODutyOccurrance.ToString() + ": ", w5.TaxFeeComputationCode);
						WriteToHtmlTable(htmlTable, "Tax/Fee Flag " + iSODutyOccurrance.ToString() + ": ", w5.TaxFeeFlag);
						WriteToHtmlTable(htmlTable, "Tax/Fee Specific Rate " + iSODutyOccurrance.ToString() + ": ", taxFeeSpecificRate);
						WriteToHtmlTable(htmlTable, "Tax/Fee Ad Valorem Rate " + iSODutyOccurrance.ToString() + ": ", taxFeeAdValorem);
						iSODutyOccurrance++;
					}
					else
					{
						HTSW0 w0 = messageBlock as HTSW0;
						if (w0 != null && (w0.NarrativeMessage == "NOT ON FILE OR EXPIRED" || w0.NarrativeMessage == "NONE IN FILE OR EXPIRED"))
						{
							if (!currentTariffNo.IsEmpty)
							{
								AddDetailsToTariffBody(ref htmlTable);
							}

							currentTariffNo = TariffFormatter.DisplayFormat(w0.FromTariffNumber);
							WriteWOTable(ref htmlTable, w0.NarrativeMessage);
						}
					}
				}

				AddDetailsToTariffBody(ref htmlTable);
			}

			EmailDef email;
			GlbBranch branch = orginatingDec != null ? orginatingDec.Branch : null;
			GenerateHtmlEmail(
						"Tariff Query Request",
						"Updated details for the following tariff number(s) have been received:",
						emailDesc,
						emailBody.ToString(),
						emailFooter,
						out email, branch);

			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branch, false);
		}

		void WriteWOTable(ref HtmlTableCreator htmlTable, ZString narrativeMessage)
		{
			htmlTable = new HtmlTableCreator(new string[] { "Column", "Value" });
			WriteToHtmlTable(htmlTable, "Details: ", narrativeMessage);
		}

		void WriteToHtmlTable(HtmlTableCreator table, string columnHeader, IZType data)
		{
			if (!data.IsEmpty)
			{
				table.WriteRow(columnHeader, data);
			}
		}

		void AddDetailsToTariffBody(ref HtmlTableCreator table)
		{
			if (table != null)
			{
				emailBody.Append(" ");
				emailBody.Append(ZString.Format("<br /><br />Tariff Number: <b>{0}</b><br /><br />", currentTariffNo));
				emailBody.Append(table.ToHtml());
				table = null;
			}
		}

		#region TariffFormatter

		protected TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = new TariffFormatter();
				}
				return fTariffFormatter;
			}
		}
		TariffFormatter fTariffFormatter;

		#endregion
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse)]
	public class ACSSolicitedTariffProcessor : SolicitedTariffProcessor<APLA, APLB, APLY>
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse)]
	public class ACESolicitedTariffProcessor : SolicitedTariffProcessor<AABIOutputA, AABIOutputB, AABIOutputY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class SolicitedTariffProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : TariffProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate)]
	public class UnsolicitedTariffProcessor : TariffProcessor<APLA, APLB, APLY>
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	[TopLevel(typeof(HTSV1), typeof(HTSV2), typeof(HTSV3), typeof(HTSV4), typeof(HTSV56789ABCEFGHIJK), typeof(HTSVD), typeof(HTSVL))]
	public abstract class TariffProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIWithDatabaseLockProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		protected TariffProcessor()
		{
		}

		USCTariff tariff;

		public override void Process()
		{
			try
			{
				SeedFactory();
				MessageBlock lastMessageBlock = null;
				var hasV2 = false;
				var hasV3 = false;
				var hasV4 = false;
				var hasOther = false;
				var hasVL = false;
				foreach (MessageBlock messageBlock in messageBlocks)
				{
					IHTS1 v1 = messageBlock as IHTS1;
					if (v1 != null)
					{
						ProcessHTS1(v1);
					}
					else
					{
						IHTS2 v2 = messageBlock as IHTS2;
						if (v2 != null)
						{
							hasV2 = true;
							ProcessHTS2(v2);
							CheckNot(typeof(IHTS2), lastMessageBlock);
						}
						else
						{
							IHTS3 v3 = messageBlock as IHTS3;
							if (v3 != null)
							{
								hasV3 = true;
								ProcessHTS3(v3);
								CheckNot(typeof(IHTS3), lastMessageBlock);
							}
							else
							{
								IHTS4 v4 = messageBlock as IHTS4;
								if (v4 != null)
								{
									hasV4 = true;
									ProcessHTS4(v4);
									CheckNot(typeof(IHTS4), lastMessageBlock);
								}
								else
								{
									IHTS56789ABCEFGHIJK vDuty = messageBlock as IHTS56789ABCEFGHIJK;
									if (vDuty != null)
									{
										hasOther = true;
										ProcessHTSDuty(vDuty);
									}
									else
									{
										IHTSD vD = messageBlock as IHTSD;
										if (vD != null)
										{
											ProcessHTSDuty(vD);
											CheckNot(typeof(IHTSD), lastMessageBlock);
										}
										else
										{
											IHTS0 w0 = messageBlock as IHTS0;
											if (w0 != null)
											{
												ProcessHTW0(w0);
											}
											else
											{
												IHTSL vL = messageBlock as IHTSL;
												if (vL != null)
												{
													hasVL = true;
													ProcessHTVL(vL);
												}
												else
												{
													CheckUnknownType(messageBlock);
												}
											}
										}
									}
								}
							}
						}
					}
					lastMessageBlock = messageBlock;
				}
				var messageType = Message.EM_MessageType;
				if (messageType == ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse ||
					messageType == ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem ||
					messageType == ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse ||
					messageType == ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse)
				{
					if (!hasV2)
					{
						ProcessEmptyHTS2();
					}
					if (!hasV3)
					{
						ProcessEmptyHTS3();
					}
					if (!hasV4)
					{
						ProcessEmptyHTS4();
					}
					if (!hasOther)
					{
						ProcessEmptyHTSDuty();
					}
					if (!hasVL)
					{
						ProcessEmptyHTVL();
					}
				}
			}
			finally
			{
				Factory.ClearQueryCache();
			}
		}

		protected virtual void CheckUnknownType(MessageBlock messageBlock)
		{
			ErrorReporter.ReportOnce("Unknown block", "An unknown block was detected in tariff processing : " + messageBlock.GetType().FullName);
		}

		void SeedFactory()
		{
			Dictionary<ZString, bool> tariffs = new Dictionary<ZString, bool>();
			foreach (MessageBlock messageBlock in messageBlocks)
			{
				HTSV1 v1 = messageBlock as HTSV1;
				if (v1 != null)
				{
					tariffs[v1.TariffNumber] = true;
				}
			}

			if (tariffs.Count > 0)
			{
				var tariffQuery = new ZQuery(USCTariffSchema.UE_Tariff, tariffs.Keys);
				Factory.Load<USCTariff>(tariffQuery);
				Factory.SeedQueryCache(USCTariff.Schema.TableName, new ZQuery());

				SeedTariffDateRestriction(tariffQuery);
				SeedTariffDutyRate(tariffQuery);
				SeedTariffQuantity(tariffQuery);
				SeedTariffValue(tariffQuery);
			}
		}

		void SeedTariffDutyRate(ZQuery tariffQuery)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(USCTariffDutyRate));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(USCTariff), USCTariffDutyRateSchema.UD_UE);
			subQuery.AddToFilter(tariffQuery);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			Factory.Load<USCTariffDutyRate>(dbOnlyQuery);
			Factory.SeedQueryCache(USCTariffDutyRate.Schema.TableName, new ZQuery());
		}

		void SeedTariffQuantity(ZQuery tariffQuery)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(USCTariffQuantity));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(USCTariff), USCTariffQuantitySchema.UQ_UE);
			subQuery.AddToFilter(tariffQuery);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			Factory.Load<USCTariffQuantity>(dbOnlyQuery);
			Factory.SeedQueryCache(USCTariffQuantity.Schema.TableName, new ZQuery());
		}

		void SeedTariffValue(ZQuery tariffQuery)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(USCTariffValue));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(USCTariff), USCTariffValueSchema.UA_UE);
			subQuery.AddToFilter(tariffQuery);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			Factory.Load<USCTariffValue>(dbOnlyQuery);
			Factory.SeedQueryCache(USCTariffValue.Schema.TableName, new ZQuery());
		}

		void SeedTariffDateRestriction(ZQuery tariffQuery)
		{
			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(USCTariffDateRestriction));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(USCTariff), USCTariffDateRestrictionSchema.UF_UE);
			subQuery.AddToFilter(tariffQuery);
			dbOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
			Factory.Load<USCTariffDateRestriction>(dbOnlyQuery);
			Factory.SeedQueryCache(USCTariffDateRestriction.Schema.TableName, new ZQuery());
		}

		void ProcessHTW0(IHTS0 w0)
		{
			OriginalMessageLinker.Link(Message);

			if (w0.NarrativeMessage == "NOT ON FILE OR EXPIRED" || w0.NarrativeMessage == "NONE IN FILE OR EXPIRED")
			{
				ZDateTime effectiveDate = w0.AsOfDate.IsEmpty ? ZDateTime.Today : w0.AsOfDate;
				USCTariff[] tariffs = new USCTariff.Loader(Factory).LoadCurrentlyValidTariffs(w0.FromTariffNumber, effectiveDate);
				foreach (USCTariff tar in tariffs)
				{
					tar.UE_DateTo = effectiveDate;
				}
			}
		}

		void ProcessHTS1(IHTS1 v1)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(USCTariffSchema.UE_Tariff, v1.TariffNumber);
			query.AddToFilter(USCTariffSchema.UE_DateFrom, v1.RecordBeginEffectiveDate);
			query.AddToFilter(USCTariffSchema.UE_DateTo, v1.RecordEndEffectiveDate);
			tariff = Factory.LoadTop1<USCTariff>(query);
			if (v1.TransactionCode == "D")
			{
				if (tariff != null)
				{
					tariff.Delete();
				}
			}
			else
			{
				CleanupTariffs(v1);

				if (tariff == null)
				{
					tariff = Factory.New<USCTariff>();
					tariff.UE_Tariff = v1.TariffNumber;
					tariff.UE_DateFrom = v1.RecordBeginEffectiveDate;
					tariff.UE_DateTo = v1.RecordEndEffectiveDate;
				}
				tariff.UE_ShortDescription = v1.CommodityDescription;
				tariff.UE_Unit1 = v1.Unit1;
				tariff.UE_Unit2 = v1.Unit2;
				tariff.UE_Unit3 = v1.Unit3;
				tariff.UE_IsBaseRate = v1.BaseRateIndicator == "B";
				tariff.UE_DutyComputationCode = v1.DutyComputationCode;
				tariff.UE_NumberOfReportingUnits = Convert.ToByte(v1.NumberOfReportingUnits);
				tariff.UE_Column1RateSpecific = v1.Column1SpecificRate;

				tariff.DutyRates.MarkForDelete();
			}

			if (tariff != null && !tariff.IsDeleted)
			{
				tariff.UE_OGACodes = ZString.Empty;
				tariff.UE_PGACodes = ZString.Empty;
			}
		}

		void CleanupTariffs(IHTS1 v1)
		{
			var query = new ZQuery(USCTariffSchema.UE_Tariff, v1.TariffNumber);
			if (tariff != null)
			{
				query.AddToFilter(USCTariffSchema.PK, SQLComparisonOperator.NotEqual, tariff.PK);
			}

			var matchingTariffs = Factory.Load<USCTariff>(query);

			foreach (USCTariff matchingTariff in matchingTariffs)
			{
				if (!matchingTariff.IsDeleted)
				{
					if (v1.RecordBeginEffectiveDate <= matchingTariff.UE_DateFrom && matchingTariff.UE_DateFrom <= v1.RecordEndEffectiveDate)
					{
						matchingTariff.Delete();
					}
					else if (matchingTariff.UE_DateFrom < v1.RecordBeginEffectiveDate && matchingTariff.UE_DateTo >= v1.RecordBeginEffectiveDate)
					{
						matchingTariff.UE_DateTo = v1.RecordBeginEffectiveDate.AddDays(-1);
						DeleteTariffIfOverlapingTariffExists(matchingTariff);
					}
				}
			}
		}

		void DeleteTariffIfOverlapingTariffExists(USCTariff tariffToCheck)
		{
			var query = GetTariffQueryWithOverlapingDateRange(tariffToCheck.UE_Tariff, tariffToCheck.UE_DateFrom, tariffToCheck.UE_DateTo, tariff == null ? new[] { tariffToCheck.PK } : new[] { tariffToCheck.PK, tariff.PK });
			var overlapingTariff = Factory.LoadTop1<USCTariff>(query);
			if (overlapingTariff != null) // If there are other tariff then this one is not valid
			{
				tariffToCheck.Delete();
			}
		}

		ZQuery GetTariffQueryWithOverlapingDateRange(ZString tariffNumber, ZDateTime fromDate, ZDateTime toDate, ZGuid[] existingPKs)
		{
			var query = new ZQuery(USCTariffSchema.UE_Tariff, tariffNumber);
			if (existingPKs != null)
			{
				query.AddToFilter(USCTariffSchema.PK, SQLComparisonOperator.NotEqual, existingPKs);
			}
			var dateFromQuery = new ZQuery(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate);
			dateFromQuery.AddToFilter(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualTo, toDate);
			var dateToQuery = new ZQuery(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualTo, fromDate);
			dateToQuery.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.LessThanOrEqualTo, toDate);
			var overlapDateQuery = new ZQuery(USCTariffSchema.UE_DateFrom, SQLComparisonOperator.LessThanOrEqualTo, fromDate);
			overlapDateQuery.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualTo, toDate);
			var dateQuery = new ZQuery(dateFromQuery);
			dateQuery.AddToFilter(dateToQuery, JoinCondition.Or);
			dateQuery.AddToFilter(overlapDateQuery, JoinCondition.Or);
			query.AddToFilter(dateQuery);
			return query;
		}

		void ProcessHTS2(IHTS2 v2)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_Column1RateAdValorem = v2.Column1RateAdValorem;
			tariff.UE_Column1RateOther = v2.Column1RateOther;

			tariff.UE_Column2RateSpecific = v2.Column2RateSpecific;
			tariff.UE_Column2RateAdValorem = v2.Column2RateAdValorem;
			tariff.UE_Column2RateOther = v2.Column2RateOther;

			tariff.UE_CountervailingDutyFlag = (v2.CountervailingDutyFlag == "1");
			tariff.UE_AdditionalTariffNumberIndicator = v2.AdditionalTariffNumberIndicator == "R";
			tariff.UE_PermitLicenseIndicator = v2.MiscellaneousPermitLicenseIndicator;
		}

		void ProcessEmptyHTS2()
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_Column1RateAdValorem = 0;
			tariff.UE_Column1RateOther = 0;

			tariff.UE_Column2RateSpecific = 0;
			tariff.UE_Column2RateAdValorem = 0;
			tariff.UE_Column2RateOther = 0;

			tariff.UE_CountervailingDutyFlag = false;
			tariff.UE_AdditionalTariffNumberIndicator = false;
			tariff.UE_PermitLicenseIndicator = ZString.Empty;
		}

		void ProcessHTS3(IHTS3 v3)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_GSPExcludedCountries = v3.GeneralizedSystemOfPreferencesGSPExcludedCountries;
			tariff.UE_AntiDumping = v3.AntidumpingDutyFlag == "1";
			tariff.UE_QuotaIndicator = v3.QuotaIndicator == "1";
			tariff.UE_TextileCategoryNumber = v3.CategoryNumber;
			tariff.UE_SPICode = v3.SpecialProgramsIndicatorSPICode;
		}

		void ProcessEmptyHTS3()
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_GSPExcludedCountries = ZString.Empty;
			tariff.UE_AntiDumping = false;
			tariff.UE_QuotaIndicator = false;
			tariff.UE_TextileCategoryNumber = ZString.Empty;
			tariff.UE_SPICode = ZString.Empty;
		}

		void ProcessHTS4(IHTS4 v4)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			var query = new ZQuery(USCTariffValueSchema.UA_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffValues = Factory.Load<USCTariffValue>(query).ToList();
			USCTariffValue tariffValue = null;
			if (tariffValues.Count > 0)
			{
				tariffValue = tariffValues[0];
				tariffValues.Remove(tariffValue);
				tariffValues.ForEach(x => x.Delete());
			}
			else
			{
				tariffValue = Factory.New<USCTariffValue>();
				tariffValue.UA_UE = tariff.PK;
			}
			tariff.UE_ISOCountryofOriginEditCode = v4.ISOCountryOfOriginEditCode;

			tariffValue.UA_ValueEditCode = v4.ValueEditCode;
			tariffValue.UA_ValueLowBounds = v4.ValueLowBounds;
			tariffValue.UA_ValueHighBounds = v4.ValueHighBounds;

			if (!v4.EntryDateRestrictionCode1.IsEmpty)
			{
				tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist(v4.EntryDateRestrictionCode1, v4.BeginRestrictionDate1, v4.EndRestrictionDate1);
			}

			if (!v4.EntryDateRestrictionCode2.IsEmpty)
			{
				tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist(v4.EntryDateRestrictionCode2, v4.BeginRestrictionDate2, v4.EndRestrictionDate2);
			}

			query = new ZQuery(USCTariffQuantitySchema.UQ_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffQuantities = Factory.Load<USCTariffQuantity>(query).ToList();
			USCTariffQuantity tariffQuantity = null;
			if (tariffQuantities.Count > 0)
			{
				tariffQuantity = tariffQuantities[0];
				tariffQuantities.Remove(tariffQuantity);
				tariffQuantities.ForEach(x => x.Delete());
			}
			else
			{
				tariffQuantity = Factory.New<USCTariffQuantity>();
				tariffQuantity.UQ_UE = tariff.PK;
			}
			tariffQuantity.UQ_QuantityEditCode = v4.QuantityEditCode;
			tariffQuantity.UQ_LowerBound = v4.QuantityEditLowerBound;
			tariffQuantity.UQ_UpperBound = v4.QuantityEditUpperBound;
		}

		void ProcessEmptyHTS4()
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_ISOCountryofOriginEditCode = ZString.Empty;

			var query = new ZQuery(USCTariffValueSchema.UA_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffValues = Factory.Load<USCTariffValue>(query).ToList();
			if (tariffValues.Count > 0)
			{
				tariffValues.ForEach(x => x.Delete());
			}

			query = new ZQuery(USCTariffDateRestrictionSchema.UF_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffDateRestrictions = Factory.Load<USCTariffDateRestriction>(query).ToList();
			if (tariffDateRestrictions.Count > 0)
			{
				tariffDateRestrictions.ForEach(x => x.Delete());
			}

			query = new ZQuery(USCTariffQuantitySchema.UQ_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffQuantities = Factory.Load<USCTariffQuantity>(query).ToList();
			if (tariffQuantities.Count > 0)
			{
				tariffQuantities.ForEach(x => x.Delete());
			}
		}

		void ProcessHTSDuty(IHTS56789ABCEFGHIJK vDuty)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			USCTariffDutyRate dutyRate = tariff.DutyRates.GetRateForElement(vDuty.DutyElement);
			if (dutyRate == null)
			{
				dutyRate = tariff.DutyRates.AddNew();
				dutyRate.UD_DutyElement = vDuty.DutyElement;
			}
			dutyRate.UD_AdValoremSpecialRate = vDuty.AdValoremSpecialRate;
			dutyRate.UD_ISOCountryCode = vDuty.InternationalOrganizationForStandardizationISOCountryCode;
			dutyRate.UD_OtherSpecialRate = vDuty.OtherSpecialRate;
			dutyRate.UD_SpecificSpecialRate = vDuty.SpecificSpecialRate;
			dutyRate.UD_TaxFeeAdvalorem = vDuty.TaxFeeAdValorem;
			dutyRate.UD_TaxFeeClassCode = vDuty.TaxFeeClassCode;
			dutyRate.UD_TaxFeeComputationCode = vDuty.TaxFeeComputationCode;
			dutyRate.UD_TaxFeeFlag = vDuty.TaxFeeFlag;
			dutyRate.UD_TaxFeeSpecificRate = vDuty.TaxFeeSpecificRate;
			dutyRate.RemoveOnFactorySaving = false;
		}

		void ProcessEmptyHTSDuty()
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.DutyRates.RemoveAndDeleteAll();
		}

		void ProcessHTSDuty(IHTSD vD)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_SPICode = tariff.UE_SPICode.PadRight(28) + vD.SpecialProgramsIndicatorSPICode;
		}

		void ProcessHTVL(IHTSL vL)
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_PGACodes = vL.ParticipatingGovernmentAgencies;
		}

		void ProcessEmptyHTVL()
		{
			if (tariff == null || tariff.IsDeleted)
			{
				return;
			}

			tariff.UE_PGACodes = ZString.Empty;
		}

		protected sealed override CargoWise.Definitions.Customs.US.ReferenceLockType ReferenceLockType
		{
			get { return CargoWise.Definitions.Customs.US.ReferenceLockType.Tariff; }
		}
	}
}
