using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public abstract class ReconFlatFileDataTransferProcessor
	{
		public abstract void ImportReconDataFromCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration);
		public abstract void ExportReconDataToCollection(ReconFlattenedDataLineCollection collection, ReconDeclaration reconDeclaration);

		protected ReconOriginalEntryHeader LocateEntry(ReconFlattenedDataLine dataLine, ReconDeclaration reconDeclaration)
		{
			ReconOriginalEntryHeader result = FindOriginalEntry(reconDeclaration, dataLine.EntryNumber);
			if (result == null)
			{
				result = reconDeclaration.OriginalEntries.AddNew();
				result.CH_OrigEntryReference = dataLine.EntryNumber;
			}

			return result;
		}

		ReconOriginalEntryHeader FindOriginalEntry(ReconDeclaration reconDeclaration, ZString entryNumber)
		{
			if (!entryNumber.IsEmpty)
			{
				foreach (ReconOriginalEntryHeader reconOriginalEntryHeader in reconDeclaration.OriginalEntries)
				{
					if (reconOriginalEntryHeader.CH_OrigEntryReference == entryNumber)
					{
						return reconOriginalEntryHeader;
					}
				}
			}

			return null;
		}

		protected void SetCommonEntryLevelInfo(ReconFlattenedDataLine dataLine, ReconOriginalEntryHeader originalEntry)
		{
			if (!dataLine.EntryNumber.IsEmpty)
			{
				originalEntry.CH_OrigEntryReference = dataLine.EntryNumber.KeepAlphanumericCharacters();
			}

			if (!dataLine.PaymentDate.IsEmpty)
			{
				originalEntry.US_PaymentDate = dataLine.PaymentDate;
			}

			if (!dataLine.EntryDate.IsEmpty)
			{
				originalEntry.US_R_ReleaseDate = dataLine.EntryDate;
			}

			if (!dataLine.EntryPort.IsEmpty)
			{
				originalEntry.US_SchDEntry = dataLine.EntryPort;
			}

			if (!dataLine.ImportationDate.IsEmpty)
			{
				originalEntry.US_ImportDate = dataLine.ImportationDate;
			}

			if (!dataLine.OwnerReferenceNumber.IsEmpty)
			{
				originalEntry.US_R_OwnerRef = dataLine.OwnerReferenceNumber;
			}

			if (!dataLine.HasNoLineDetails.IsEmpty)
			{
				originalEntry.US_R_NoLineDetails = dataLine.HasNoLineDetails == YesNoDefaultList.Codes.Yes;
			}

			if (!originalEntry.US_R_NoLineDetails)
			{
				originalEntry.US_R_IsHMFApplicable = dataLine.OriginalHMF > 0 ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No;
			}

			if (!dataLine.MessageMode.IsEmpty)
			{
				originalEntry.US_R_MsgMode = dataLine.MessageMode.Left(ReconFlattenedDataLine.Schema.MessageModeMaxLength);
			}
		}

		#region Progress

		public delegate bool ProgressChangedEventHandler(int percentComplete, string status);
		public event ProgressChangedEventHandler ProgressChanged;

		public bool OnProgressChanged(int percentComplete, string status)
		{
			if (ProgressChanged != null)
			{
				return ProgressChanged(percentComplete, status);
			}

			return true;
		}

		#endregion
	}
}
