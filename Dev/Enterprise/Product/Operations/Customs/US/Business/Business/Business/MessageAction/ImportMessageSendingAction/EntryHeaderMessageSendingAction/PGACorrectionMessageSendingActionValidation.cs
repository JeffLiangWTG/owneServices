using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class PGACorrectionMessageSendingActionValidation : ZValidation
	{
		public PGACorrectionMessageSendingActionValidation(PGACorrectionMessageSendingAction parent) : base(parent)
		{
			this.parent = parent;
		}
		readonly PGACorrectionMessageSendingAction parent;

		public void CheckUS_SendMessage()
		{
			var declaration = parent.Entry?.Declaration;
			if (declaration != null)
			{
				var releaseDate = declaration.JE_EntryAuthorisationDate;
				if (parent.US_SendMessage && !releaseDate.IsEmpty)
				{
					var dataCorrections = parent.Entry.PGADataCorrections;
					if (dataCorrections.Count > 0)
					{
						var validReleaseDate = ZDateTime.Today.AddDays(-10);
						var workingDays = CustomsWorkingDays.GetInstance(new BusinessObjectFactory { NameForDebugging = "WorkingDays" });
						if (workingDays != null)
						{
							validReleaseDate = workingDays.GetAnotherStandardWorkingDay(ZDate.Today.ToDateTime(), -10);
						}

						if (declaration.IsEntryClear && declaration.ReleaseStatus == CRLReleaseStatusList.Codes.REL)
						{
							var checkCorrectionForPGA = new Action<ZString>((agencyCode) =>
							{
								if (dataCorrections.HasSpecificPGALines(agencyCode) && declaration.GetSinglePGAEntryStatus(agencyCode) == PGADispositionCodeList.MarkAsClosedCode)
								{
									parent.US_SendMessageInfo.AddMessageError(GetPGAMessageError(agencyCode));
								}
							});

							checkCorrectionForPGA(ACEGovernmentAgenciesCodeList.Codes.FDA);
							checkCorrectionForPGA(ACEGovernmentAgenciesCodeList.Codes.FWS);
						}
						else if (releaseDate < validReleaseDate)
						{
							parent.US_SendMessageInfo.AddMessageError(ReleseDateMessageError);
						}
					}
				}
			}
		}

		public static string GetPGAMessageError(ZString agencyCode)
		{
			return Res.GetString("A2C164FA-E9D2-4C55-86AB-39E6F323A9D5", "PGA correction contains {0} data. Since there is a release date on file, {0} data may not be sent in the PGA data correction", agencyCode);
		}
		public static string ReleseDateMessageError => Res.GetString("55A64695-FAAB-49DB-BE52-31A0B0B245C3", "PGA Correction message cannot be sent now. Entry was released more than 10 days ago");

		public override Type AutoValidationType => typeof(PGACorrectionMessageSendingActionValidation);

		public override void ValidateAll() => CheckUS_SendMessage();
	}
}
