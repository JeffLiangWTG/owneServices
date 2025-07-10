using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.BIRD
{
	public class BRDDT : Messaging.Business.MessageBuildingBlocks.BIRD.Abstract.BRDDT, IBIRDStatusRecord
	{
		ZDateTime DateTime1
		{
			get
			{
				int hours = 0, minutes = 0, seconds = 0;

				GetTime(Time1, out hours, out minutes, out seconds);

				return Date1.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
			}
		}

		ZDateTime DateTime2
		{
			get
			{
				int hours = 0, minutes = 0, seconds = 0;

				GetTime(Time2, out hours, out minutes, out seconds);

				return Date2.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
			}
		}

		ZDateTime DateTime3
		{
			get
			{
				int hours = 0, minutes = 0, seconds = 0;

				GetTime(Time3, out hours, out minutes, out seconds);

				return Date3.AddHours(hours).AddMinutes(minutes).AddSeconds(seconds);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		void GetTime(ZString timeInHHMMSS, out int hours, out int minutes, out int seconds)
		{
			hours = ZInt.ParseSafe(timeInHHMMSS.SubstringSafe(0, 2), 0);
			minutes = ZInt.ParseSafe(timeInHHMMSS.SubstringSafe(2, 2), 0);
			seconds = ZInt.ParseSafe(timeInHHMMSS.SubstringSafe(4, 2), 0);
		}

		#region IBIRDStatusRecord Members

		void IBIRDStatusRecord.Update(JobDeclaration declaration, INotifications notifications)
		{
			SetDateTime(declaration, notifications, DateTime1, Date1Qualifier);

			SetDateTime(declaration, notifications, DateTime2, Date2Qualifier);

			SetDateTime(declaration, notifications, DateTime3, Date3Qualifier);
		}

		void SetDateTime(JobDeclaration declaration, INotifications notifications, ZDateTime dateTime, ZString qualifier)
		{
			if (!dateTime.IsEmpty)
			{
				switch (qualifier)
				{
					case BIRDDateQualifierList.Codes.ArrivalAtFirstPortUnlading:
						declaration.JE_DateOfArrival = dateTime;
						break;

					case BIRDDateQualifierList.Codes.ArrivalAtPortOfEntry:
						declaration.US_EntryDate = dateTime;
						break;

					case BIRDDateQualifierList.Codes.DutyDueDate:
						declaration.US_PaymentDueDate = dateTime;
						break;

					case BIRDDateQualifierList.Codes.DutyPaid:
						declaration.US_PaymentDate = dateTime;
						break;

					case BIRDDateQualifierList.Codes.CustomsRelease:
						CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry ?? declaration.ActiveEntryHeaders.CargoReleaseEntry;

						if (entry != null)
						{
							entry.Declaration.JE_EntryAuthorisationDate = dateTime;
						}
						else
						{
							ErrorReporter.ReportOnce("Neither ENS nor CRL entry exists for BIRD", "Neither ENS nor CRL entry exists for BIRD and the passed Release date cannot be set");
						}
						break;

					case BIRDDateQualifierList.Codes.Statement:
						declaration.US_PreliminaryStatementPrintDate = dateTime;
						break;
				}
			}
		}

		#endregion
	}
}
