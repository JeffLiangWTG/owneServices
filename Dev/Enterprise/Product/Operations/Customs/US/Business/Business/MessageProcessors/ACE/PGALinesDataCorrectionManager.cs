using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	class PGALinesDataCorrectionManager
	{
		internal PGALinesDataCorrectionManager(MQEDIMessage message, JobDeclaration declaration)
		{
			this.message = Argument.NotNull(message, "message");
			this.declaration = Argument.NotNull(declaration, "declaration");
		}
		readonly MQEDIMessage message;
		readonly JobDeclaration declaration;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void UpdatePGALines(bool isFailure, bool isPGADataCorrectionMessage = false)
		{
			declaration.AddInvoiceLineCusAddInfoFetchHintsIfNeeded();
			if (isFailure)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.PGADataCorrections.ForEach(x => x.SetTrackStatusAfterMessageFailure());
				}
			}
			else
			{
				if (IsDeleteMessage)
				{
					UpdatePGALinesWhenDeletationAccepted();
				}
				else
				{
					var pgaLineToBeAmended = false;

					//TODO Dong UNIT TEST
					//06 Weekly estimate - users send cargo release and got FDA accepted. Then users change description consequently FDA status is changed to 'To Be Updated'. They send entry summary without FDA. Status should still be 'PGA Replacement/Update required'
					foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
					{
						foreach (var pgaDataCorrection in invoiceLine.PGADataCorrections)
						{
							pgaDataCorrection.SetTrackStatusAfterMessageIsLodged();

							var status = (ZString)pgaDataCorrection.TrackingStatusInfo.Value;
							if (status == PGATrackingStatusList.Codes.ToBeDeleted || status == PGATrackingStatusList.Codes.ToBeUpdated)
							{
								pgaLineToBeAmended = true;
							}
						}

						if (!isPGADataCorrectionMessage)
						{
							invoiceLine.SetTrackingID();
						}
					}

					if (!pgaLineToBeAmended)
					{
						declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;
					}
				}
			}
		}

		void UpdatePGALinesWhenDeletationAccepted()
		{
			if (message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ACECargoReleaseDelete || declaration.US_PGAExpeditedRelease)
			{
				declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;

				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					invoiceLine.PGADataCorrections.ForEach(x => x.SetTrackStatusAfterDeletionMessageIsAccepted());
					invoiceLine.RemoveTrackingID();
				}
			}
		}

		bool IsDeleteMessage
		{
			get { return message.EM_MessageSubType == EM_MessageSubTypeList.Codes.ACECargoReleaseDelete || message.EM_MessageSubType == EM_MessageSubTypeList.Codes.EntrySummaryDelete; }
		}
	}
}
