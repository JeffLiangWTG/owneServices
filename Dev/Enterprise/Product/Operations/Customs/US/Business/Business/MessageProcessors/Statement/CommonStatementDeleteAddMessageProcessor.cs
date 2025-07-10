using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class CommonStatementDeleteAddMessageProcessor<THBlock, TH1Block, TH2Block, TI7501StatusBlock, ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : ABIProcessor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where THBlock : MessageBlock, IStatementUpdateInputHBlock
		where TH1Block : MessageBlock, IStatementUpdateOutputH1Block
		where TH2Block : MessageBlock, IStatementUpdateAdditionalOutput
		where TI7501StatusBlock : MessageBlock, I7501Status
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		const string StatementNumberPlaceHolder = "Entry {0} in Statement {1}";
		const string EntryJobNumberPlaceHolder = "Job {0}/ Entry {1}";
		const string EntryNumberPlaceHolder = "Entry {0}";

		public override void Process()
		{
			CusStatementLine statementLine = null;
			var messageAttachee = OriginalMessageLinker.Link(Message);

			IStatementLineDeclaration declaration = messageAttachee as JobDeclaration;

			if (declaration == null)
			{
				var entry = messageAttachee as CusEntryHeader;
				if (entry != null && entry.IsReconEntry)
				{
					declaration = ReconDeclaration.Get(entry.Declaration);
				}
			}

			if (declaration != null)
			{
				statementLine = GetStatementLineFromEntry(declaration, Message.Factory);
			}
			else
			{
				var ensH1 = GetFirstMessageBlock<TH1Block>();
				if (ensH1 != null)
				{
					statementLine = new CusStatementLine.Loader(Factory).LoadTop1NotDeleted(ensH1.EntryNumber, ensH1.EntryFilerCode, GlbCompany.CurrentCompany.PK);
					if (statementLine != null && statementLine.Declaration != null)
					{
						declaration = statementLine.Declaration;
					}
				}
			}

			if (IsCustomsNotOnFileFailureAndNeedToResend())
			{
				ResubmitIfRequired(statementLine, declaration as JobDeclaration);
			}
			else
			{
				ProcessResponse(statementLine, declaration, false);
			}
		}

		#region Implementation

		internal void ProcessResponse(CusStatementLine statementLine, IStatementLineDeclaration declaration, bool hasBeenResubmittedUnsuccessfully)
		{
			var html = new HtmlTableCreator(new string[] { "Column", "Data/Description" });
			bool isFailure = !Message.RelatedMessage.IsSTUMessageAndAccepted();

			var i7501Status = GetFirstMessageBlock<TI7501StatusBlock>();

			if (i7501Status != null && !i7501Status.Code.IsEmpty)
			{
				html.WriteRow(i7501Status.Code, !isFailure ? i7501Status.NarrativeMessage : GetLongDescription(i7501Status.Code, i7501Status.NarrativeMessage));
			}

			var ensH1 = GetFirstMessageBlock<TH1Block>();
			if (ensH1 != null)
			{
				var ensH = GetSentENSH();
				var monthlyStatementMonth = ensH != null ? ensH.PeriodicStatementMonth : ZString.Empty;
				if (declaration != null && !isFailure)
				{
					declaration.US_PaymentType = ensH1.PaymentTypeIndicator.ToString();
					if (declaration.US_PaymentType == PaymentTypeList.Codes.IndividualBasis)
					{
						declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
						declaration.US_PeriodicStatementMM = "";
					}
					else
					{
						declaration.US_PreliminaryStatementPrintDate = ensH1.PreliminaryStatementPrintDate;
						declaration.US_PeriodicStatementMM = monthlyStatementMonth;
					}
					declaration.US_ClientBranchDesignation = ensH1.ClientBranchDesignation;

					var preliminaryStatementPD = ensH?.PreliminaryStatementPrintDate ?? ZDate.Empty;
					if (preliminaryStatementPD.IsValid)
					{
						declaration.US_PSDAccepted = ensH.PreliminaryStatementPrintDate;
					}
					else
					{
						declaration.US_PSDAccepted = declaration.US_PreliminaryStatementPrintDate;
					}
				}

				if (statementLine != null)
				{
					if (!isFailure && IsEntryMovedToAnotherStatement(ensH1, statementLine))
					{
						statementLine.B3_Status = StatementLineStatusList.Codes.Deleted;

						CusStatementHeader statementHeader = statementLine.StatementHeader;
						statementHeader.ActiveLines.Rebuild();

						if (statementHeader.StatementLines.AreAllLinesDeleted)
						{
							statementHeader.B2_Status = StatementHeaderStatusList.Codes.Deleted;
						}
					}
					else
					{
						statementLine.B3_Status = StatementLineStatusList.Codes.Active;
					}
				}

				html.WriteRow("Payment Type", ensH1.PaymentTypeIndicator);
				html.WriteRow("Preliminary Statement Print Date", ensH1.PreliminaryStatementPrintDate.IsValid ? ensH1.PreliminaryStatementPrintDate.ToShortDateString() : "N/R");
				html.WriteRow("Client Branch Designation", !ensH1.ClientBranchDesignation.IsEmpty ? ensH1.ClientBranchDesignation.ToString() : "N/R");
				html.WriteRow("Periodic Monthly Statement Month", !monthlyStatementMonth.IsEmpty ? monthlyStatementMonth.ToString() : "N/R");

				if (!isFailure)
				{
					html.WriteRow(" ", "The entry, " + ensH1.EntryNumber + " has been updated with the data above where applicable.");
				}
				else
				{
					html.WriteRow(" ", "The entry, " + ensH1.EntryNumber + " has NOT been updated with the data reported as the message is rejected.");
				}
			}

			var ensH2 = GetFirstMessageBlock<TH2Block>();
			if (ensH2 != null)
			{
				ZString notRelated = "N/R";
				html.WriteRow("Statement Number", !ensH2.StatementNumber.IsEmpty ? ensH2.StatementNumber : notRelated);
				html.WriteRow("Total Amount Due", ensH2.TotalAmountDue > 0 ? ensH2.TotalAmountDue.ToString(0) : "N/R");
				html.WriteRow("Periodic Monthly Statement Number", !ensH2.PeriodicMonthlyStatementNumber.IsEmpty ? ensH2.PeriodicMonthlyStatementNumber : notRelated);
				html.WriteRow("Periodic Monthly Statement Total Amount Due", ensH2.PeriodicMonthlyStatementTotalAmountDue > 0 ? ensH2.PeriodicMonthlyStatementTotalAmountDue.ToString(0) : "N/R");
			}

			if (hasBeenResubmittedUnsuccessfully)
			{
				html.WriteRow(" ", "This message has been resubmitted to Customs 5 times and has been rejected each time. This is a temporary problem that might occur due to transition from ACS to ACE. Please send the Statement Delete/Update transaction after 30 minutes.");
			}

			GenerateEmail(html.ToHtml(), declaration, statementLine, isFailure);

			if (declaration != null)
			{
				LogMessageStatusChangeEventAgainstTopLevelBusinessObject((IMessageAttachee)declaration);
			}
		}

		protected virtual bool IsCustomsNotOnFileFailureAndNeedToResend()
		{
			return false;
		}

		protected virtual void ResubmitIfRequired(CusStatementLine statementLine, JobDeclaration declaration)
		{
		}

		protected virtual ZString GetLongDescription(ZString errorCode, ZString initialDescription)
		{
			return MessageCalculator.GetLongDescription(errorCode, initialDescription);
		}

		void GenerateEmail(string body, IStatementLineDeclaration declaration, CusStatementLine statementLine, bool isFailure)
		{
			var ensH = GetSentENSH();

			string jobNumber = "Unknown";
			string url = "";

			string formattedEntryNumber = "";

			if (ensH != null)
			{
				formattedEntryNumber = CusEntryHeader.GetFormmattedFilerCodeAndEntryNumber(ensH.EntryFilerCode, ensH.EntryNumber);
			}

			if (declaration != null)
			{
				jobNumber = string.Format(CultureInfo.CurrentCulture, EntryJobNumberPlaceHolder, declaration.JE_DeclarationReference, declaration.FormattedEntryNumber);
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration);
			}
			else if (statementLine != null)
			{
				jobNumber = string.Format(CultureInfo.CurrentCulture, StatementNumberPlaceHolder, formattedEntryNumber, statementLine.StatementHeader.B2_StatementNumber);
				url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(statementLine);
			}
			else if (ensH != null)
			{
				jobNumber = string.Format(CultureInfo.CurrentCulture, EntryNumberPlaceHolder, formattedEntryNumber);
			}
			var branch = Message.OriginalMessage != null ? Message.OriginalMessage.Branch : GlbBranch.CurrentBranch;
			GenerateHtmlEmailAndSendToOriginalOrGroup(url, jobNumber, "Statement Delete/Add", body, isFailure, branch, declaration as BusinessObject);
		}

		bool IsEntryMovedToAnotherStatement(TH1Block ensH1, CusStatementLine statementLine)
		{
			CusStatementHeader statementHeader = statementLine.StatementHeader;

			return ensH1 != null &&
				ensH1.PaymentTypeIndicator.ToString() != statementHeader.B2_PaymentType ||
				ensH1.PreliminaryStatementPrintDate != statementHeader.B2_PrintDate;
		}

		THBlock GetSentENSH()
		{
			var originalMessage = Message.OriginalMessage;
			return originalMessage != null ? originalMessage.MessageBlock.MessageBlocks.OfType<THBlock>().FirstOrDefault() : null;
		}

		CusStatementLine GetStatementLineFromEntry(IStatementLineDeclaration declaration, BusinessObjectFactory factory)
		{
			return new CusStatementLine.Loader(factory).LoadTop1NotDeleted(declaration.EntryNumber, declaration.EntryFilerCode, declaration.RegistryCompanyPK);
		}

		#endregion
	}
}
