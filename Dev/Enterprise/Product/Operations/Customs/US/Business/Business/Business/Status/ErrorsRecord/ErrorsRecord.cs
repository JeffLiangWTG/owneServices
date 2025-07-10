using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using static Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.Business
{
	public class ErrorsRecord : Messaging.Business.ErrorsRecord, IErrorsRecord
	{
		public ErrorsRecord(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ErrorsRecord()
			: base()
		{
		}

		public ErrorsRecord(MQEDIMessage message)
			: base(message.Factory)
		{
			this.message = message;
		}

		public readonly new MQEDIMessage message;

		#region Entry Summary Status Notification

		public void UpdateEntrySummaryStatusNotificationDetails()
		{
			if (message != null)
			{
				var e1 = message.MessageBlock.MessageBlocks.OfType<AESSE1>().FirstOrDefault();
				var e2 = message.MessageBlock.MessageBlocks.OfType<AESSE2>().FirstOrDefault();

				if (e1 != null)
				{
					DispositionCode = e1.DispositionTypeCode;
					SourceOfActionRequest = e1.SourceOfActionRequest;
					ImportSpecialistTeam = e1.ImportSpecialistTeam;
					StatusDate = e1.DateOfAction;
					LineNumber = e1.EntrySummaryLineItemIdentifier;
				}

				if (e2 != null)
				{
					CBPStaffContactDetails = e2.CBPUser;

					if (!e2.TelephoneNumber.IsEmpty)
					{
						CBPStaffContactDetails += " (Ph:" + e2.TelephoneNumber + ")";
					}

					ActionIDNumber = e2.ActionIdentificationNumber;
				}

				foreach (var e3 in message.MessageBlock.MessageBlocks.OfType<AESSE3>())
				{
					if (!BlockText.IsEmpty && !e3.Remarks.IsEmpty)
					{
						this.BlockText += "\r\n";
					}

					this.BlockText += e3.Remarks;
				}
			}
			else
			{
				ErrorReporter.ReportOnce("UpdateEntrySummaryStatusNotificationDetails", "This method should be called only when MQEDIMessage is passed to the contructor");
			}
		}

		public ZString DispositionDescription
		{
			get
			{
				if (!dispositionDescription.HasValue)
				{
					dispositionDescription = ENSStatusDispositionCodeListLoader.GetENSStatusDispositionCodeList(Factory).GetDescriptionFromCode(DispositionCode);

					if (!dispositionDescription.HasValue)
					{
						dispositionDescription = ZString.Empty;
					}

					if (!dispositionDescription.Value.IsEmpty)
					{
						ZStringBuilder result = new ZStringBuilder();
						int oneRowLength = 0;

						foreach (ZString word in dispositionDescription.Value.Split(' '))
						{
							if (oneRowLength > 40)
							{
								result.Append("\r\n" + word);
								oneRowLength = 0;
							}
							else
							{
								if (!result.IsEmpty)
								{
									result.Append(" ");
								}

								result.Append(word);
							}

							oneRowLength += word.Length + 1;
						}

						dispositionDescription = result.ToString();
					}
				}

				return dispositionDescription.Value;
			}
		}
		ZString? dispositionDescription;

		public ZString SourceOfActionRequestDesc
		{
			get
			{
				ZString result = ZString.Empty;

				if (SourceOfActionRequest == "1")
				{
					result = "Manual Request";
				}
				else if (SourceOfActionRequest == "2")
				{
					result = "Automated Request";
				}

				return result;
			}
		}

		public bool IsFurtherActionRequiredOnAcknowledged
		{
			get { return message != null && !message.ActionAuthorised; }
		}

		public bool IsFurtherActionRequiredButNotActionedYet
		{
			get { return message != null && ENSStatusDispositionCodeListLoader.IsFurtherActionRequired(Factory, DispositionCode) && !message.ActionAuthorised; }
		}

		public ZDateTime ActionLogEventTime
		{
			get { return message != null ? message.ActionLogEventTime : ZDateTime.Empty; }
		}

		public ZString ActionLogNKUser
		{
			get { return message != null ? message.ActionLogNKUser : ZString.Empty; }
		}

		public ZString ReferenceOnAction
		{
			get { return message != null ? message.EM_ActionStatus : ZString.Empty; }
		}

		public QuotaInformationCollection QuotaInformations
		{
			get
			{
				if (quotaInformations == null)
				{
					quotaInformations = new QuotaInformationCollection();
					if (message != null)
					{
						quotaInformations.AddRange(from e4Block in (message.MessageBlock.MessageBlocks.OfType<AESSE4>()) select new QuotaInformation(e4Block));
					}
				}
				return quotaInformations;
			}
		}

		QuotaInformationCollection quotaInformations;

		public ZString ErrorMessageIdentifierDesc
		{
			get { return Factory.GetCachedValue<ReferenceIdentifierQualifierCodeList>().GetDescriptionFromCode(ErrorMessageIdentifier); }
		}

		#endregion
	}
}
