using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public partial class CusEntryHeader : ISimplifiedMessageLinkedObject, ICusDispositionParent
	{
		#region Properties

		BusinessObject ISimplifiedMessageLinkedObject.LinkedObject => this;

		IEntryHeaderParentBusinessObject ISimplifiedMessageLinkedObject.ParentBusinessObject => Declaration;

		ZBool ISimplifiedMessageLinkedObject.IsCargoReleaseBeingCertified => IsCargoReleaseBeingCertified;

		bool ISimplifiedMessageLinkedObject.IsFormalEntry => this.IsFormalEntry;

		bool ISimplifiedMessageLinkedObject.IsBorderCargoRelease => this.IsBorderCargoRelease;

		bool ISimplifiedMessageLinkedObject.IsCargoRelease => this.IsCargoRelease;

		bool ISimplifiedMessageLinkedObject.IsACECargoRelease => this.IsACECargoRelease;

		bool ISimplifiedMessageLinkedObject.UseCodeIsHVL => false;

		ZString ISimplifiedMessageLinkedObject.CargoReleaseCertifiedStatus { get => US_CRLCertStatus; set => US_CRLCertStatus = value; }

		ZString ISimplifiedMessageLinkedObject.EntryFilerCode => this.EntryFilerCode;

		bool ISimplifiedMessageLinkedObject.IsLVSCargoRelease => false;

		#endregion

		#region Functions

		void ISimplifiedMessageLinkedObject.ClearCRLCertStatusFromAllHeaders()
		{
			Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().ToList().ForEach(x => x.US_CRLCertStatus = ZString.Empty);
		}

		void ISimplifiedMessageLinkedObject.DeactiveStatementLineIfRequired(ZString[] clearDeletedStatus, ZString messageStatus)
		{
			var statement = Declaration.RelatedStatement;

			if (statement != null && clearDeletedStatus.Contains(messageStatus))
			{
				if (statement.CanDeactivateStatementLine)
				{
					statement.DeactivateStatementLine(EntryFilerCode, EntryNumber);
				}
			}
		}

		void ISimplifiedMessageLinkedObject.AutoSendEntrySummaryQueryIfEligible()
		{
			if (IsFormalEntry)
			{
				new AutoEntrySummaryQuerySender().SendIfEligible(this);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void ISimplifiedMessageLinkedObject.UpdateACEFDALineInfoIfRequired(OGADispositionData processingOGADispositionData)
		{
			if (processingOGADispositionData != null && processingOGADispositionData.US_OGAIdentifier == GovernmentAgencyProgramCodeList.Codes.FDA)
			{
				ACEFDA matchedFDALine = null;
				var entryLine = MergedLines.FindByLineNumber(ZInt.ParseSafe(processingOGADispositionData.US_OGADispositionBeginningCBPLine, 0));
				if (entryLine != null)
				{
					var pgaLineNumber = ZInt.ParseSafe(processingOGADispositionData.US_OGADispositionBeginningOGALine, 0);
					foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
					{
						matchedFDALine = invoiceLine.ACE_FDALines.OfType<ACEFDA>().FirstOrDefault(x => x.US_LineNo == pgaLineNumber);
						if (matchedFDALine == null)
						{
							foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
							{
								matchedFDALine = secondaryLine.ACE_FDALines.OfType<ACEFDA>().FirstOrDefault(x => x.US_LineNo == pgaLineNumber);
								if (matchedFDALine != null)
								{
									break;
								}
							}
						}

						if (matchedFDALine != null)
						{
							break;
						}
					}
				}

				if (matchedFDALine != null)
				{
					foreach (OGADispositionDetail data in processingOGADispositionData.OGADispositionDetails)
					{
						if (data.US_ReferenceIDQualifier == OGADispositionReferenceQualifierList.Codes.PriorNoticeConfirmationNum && !data.US_ReferenceID.IsEmpty && matchedFDALine.US_PNC.IsEmpty)
						{
							matchedFDALine.US_PNC = data.US_ReferenceID.Left(AutoUSACEFDAAddInfo.Schema.US_PNCMaxLength);
							matchedFDALine.US_IsPNCFromMsg = true;
							break;
						}
					}

					if (processingOGADispositionData.US_Code == PGADispositionCodeList.DataRejectedPerPGAReview)
					{
						var pgaDataCorrection = matchedFDALine as IPGADataCorrection;
						if (pgaDataCorrection != null)
						{
							pgaDataCorrection.SetTrackStatusAfterSOMessageIsRejected();
							var declaration = Declaration;
							if (declaration != null && declaration.US_PGAReplaceUpdateNeeded.IsEmpty)
							{
								declaration.US_PGAReplaceUpdateNeeded = YesNoDefaultList.Codes.Yes;
							}
						}
					}
				}
			}
		}

		void ISimplifiedMessageLinkedObject.UpdateFWSLineInfoIfRequired(OGADispositionData processingOGADispositionData)
		{
			if (processingOGADispositionData != null && processingOGADispositionData.US_OGAIdentifier == GovernmentAgencyProgramCodeList.Codes.FWS)
			{
				FWSHeader matchedFWSLine = null;
				var entryLine = MergedLines.FindByLineNumber(ZInt.ParseSafe(processingOGADispositionData.US_OGADispositionBeginningCBPLine, 0));
				if (entryLine != null)
				{
					var pgaLineNumber = ZInt.ParseSafe(processingOGADispositionData.US_OGADispositionBeginningOGALine, 0);
					foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
					{
						matchedFWSLine = invoiceLine.FWSHeaders.OfType<FWSHeader>().FirstOrDefault(x => x.US_LineNo == pgaLineNumber);
						if (matchedFWSLine == null)
						{
							foreach (JobComInvoiceLine secondaryLine in invoiceLine.SecondaryTariffLines)
							{
								matchedFWSLine = secondaryLine.FWSHeaders.OfType<FWSHeader>().FirstOrDefault(x => x.US_LineNo == pgaLineNumber);
								if (matchedFWSLine != null)
								{
									break;
								}
							}
						}

						if (matchedFWSLine != null)
						{
							break;
						}
					}
				}

				if (matchedFWSLine != null)
				{
					foreach (OGADispositionDetail data in processingOGADispositionData.OGADispositionDetails)
					{
						if (data.US_ReferenceIDQualifier == OGADispositionReferenceQualifierList.Codes.FWSConfirmationNumber && !data.US_ReferenceID.IsEmpty && matchedFWSLine.US_ConfirmationNum.IsEmpty)
						{
							matchedFWSLine.US_ConfirmationNum = data.US_ReferenceID.Left(AutoUSFWSHeaderAddInfo.Schema.US_ConfirmationNumMaxLength);
							break;
						}
					}
				}
			}
		}

		KeyValuePair<string, string>[] ISimplifiedMessageLinkedObject.GetMSCEventParameters()
		{
			return null;
		}

		ZString ISimplifiedMessageLinkedObject.GetMSCEventReferenceForCargoReleaseResponse(CBPEDIMessage message)
		{
			return message.EM_MessageType;
		}

		#endregion

		#region IAESCusDispositionParent

		ZString ICusDispositionParent.ParentTableCode => CusEntryHeaderSchema.Constants.Prefix;

		BusinessObject ICusDispositionParent.CollectionMaster => this;

		ZString ICusDispositionParent.Type => ZString.Empty;

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return ZString.Empty;
		}

		#endregion

		#region AESCusDispositions

		public AESCusDispositionCollection AESCusDispositions
		{
			get
			{
				if (aesCusDispositionCollection == null)
				{
					aesCusDispositionCollection = new AESCusDispositionCollection(this);
					aesCusDispositionCollection.Load();
				}
				return aesCusDispositionCollection;
			}
		}
		AESCusDispositionCollection aesCusDispositionCollection;

		#endregion
	}
}
