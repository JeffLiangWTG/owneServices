using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	class EntryLineInvoiceLineLinkCalculator
	{
		public EntryLineInvoiceLineLinkCalculator(ZString cH_MessageType)
		{
			this.cH_MessageType = cH_MessageType;
		}

		readonly ZString cH_MessageType;

		internal bool IsEntryLineValidToBeReused(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			return entryLine.Header.CH_MessageType == cH_MessageType;
		}

		public Customs.Business.AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(CusEntryLine entryLine, JobComInvoiceLine invoiceLine)
		{
			Customs.Business.AdditionalInvoiceLineEntryLineLink result = null;
			if (IsCurrentMessageTypeHighestCandidateForJI_CL(invoiceLine.Declaration))
			{
				invoiceLine.JI_CL = entryLine.PK;
			}
			else
			{
				result = invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			}
			return result;
		}

		/// <summary>
		/// if ENS is enabled, an ENS entry should consume JI_CL
		/// otherwise if CRL is enabled without ENS, then an CRL should consume JI_CL
		/// else if INB is the only messaging mode enabled, then JI_CL is consumed by an INB entry
		/// </summary>
		bool IsCurrentMessageTypeHighestCandidateForJI_CL(JobDeclaration declaration)
		{
			bool result = false;

			switch (cH_MessageType)
			{
				case CusEntryHeaderMessageTypeList.Codes.EntrySummary:
					result = declaration.IsENSFormalImport;
					break;

				case CusEntryHeaderMessageTypeList.Codes.CargoRelease:
				case CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease:
					result = declaration.IsCargoReleaseWithoutFormalEntry;
					break;

				case CusEntryHeaderMessageTypeList.Codes.InBond:
					result = declaration.IsInBondOnly;
					break;

				case CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone:
					result = declaration.IsFTZAdmission;
					break;

				case CusEntryHeaderMessageTypeList.Codes.ACECargoRelease:
					result = declaration.IsSimplifiedEntryWithoutFormalEntry;
					break;
			}
			return result;
		}

		internal void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(JobComInvoiceLine invoiceLine)
		{
			CusEntryLine entryLine = invoiceLine.CusEntryLine;
			if (entryLine != null && entryLine.Header.CH_MessageType == cH_MessageType)
			{
				invoiceLine.JI_CL = ZGuid.Empty;
			}
			else
			{
				invoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(cH_MessageType);
			}
		}
	}
}
