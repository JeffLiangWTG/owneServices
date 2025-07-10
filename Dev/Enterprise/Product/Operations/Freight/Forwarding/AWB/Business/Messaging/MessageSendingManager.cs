using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public class MessageSendingManager : IMessageSendingManager
	{
		public MessageSendingManager(ExportAWBHeader awbHeader, OrgContact contactSending)
			: this(awbHeader)
		{
			this.contactSending = contactSending;
		}

		public MessageSendingManager(ExportAWBHeader awbHeader)
		{
			if (awbHeader == null)
			{
				throw new ArgumentNullException(nameof(awbHeader));
			}
			this.awbHeader = awbHeader;
		}

		readonly ExportAWBHeader awbHeader;
		readonly OrgContact contactSending;

		public bool SendAll(out string resultMessage)
		{
			var awbsToSend = new List<ExportAWBHeader>();
			AddAWBIfRejectedOrNotSent(awbsToSend, awbHeader);
			var hawbNumbers = new Dictionary<string, int>();
			foreach (ExportAWBHeader hawbHeader in awbHeader.ChildBills)
			{
				AddAWBIfRejectedOrNotSent(awbsToSend, hawbHeader);

				int hawbCount;
				hawbNumbers.TryGetValue(hawbHeader.EH_WayBillNumber, out hawbCount);
				hawbNumbers[hawbHeader.EH_WayBillNumber] = hawbCount + 1;
			}

			if (ThereAreDuplicateHAWBs(hawbNumbers, out resultMessage))
			{
				return false;
			}

			return SendAWBs(awbsToSend, out resultMessage);
		}

		bool ThereAreDuplicateHAWBs(Dictionary<string, int> allHawbNumbers, out string resultMessage)
		{
			var duplicateHawbNumbers = new List<KeyValuePair<string, int>>();
			foreach (var hawbNumberAndCount in allHawbNumbers)
			{
				if (hawbNumberAndCount.Value > 1)
				{
					duplicateHawbNumbers.Add(hawbNumberAndCount);
				}
			}

			if (duplicateHawbNumbers.Count > 0)
			{
				duplicateHawbNumbers.Sort(delegate(KeyValuePair<string, int> x, KeyValuePair<string, int> y)
				{ return string.Compare(x.Key, y.Key, StringComparison.Ordinal); });

				var result = new ZStringBuilder();
				ZString thisLine = ZString.Empty;
				foreach (var hawbNumberAndCount in duplicateHawbNumbers)
				{
					if (thisLine.Length > 50)
					{
						result.Append(thisLine + ",");
						thisLine = ZString.Empty;
					}

					thisLine += (thisLine.IsEmpty ? "" : ", ")
						+ string.Format(Culture.Invariant, (NoResString)"{0} (x{1})", hawbNumberAndCount.Key.Trim(), hawbNumberAndCount.Value.ToString(Culture.Invariant)); // String contains only symbols.
				}

				if (!thisLine.IsEmpty)
				{
					result.Append(thisLine + ".");
				}

				resultMessage = Res.GetString("5e46c150-2d17-477c-ba7b-d61b59df41ea", @"ERROR: Cannot send a MAWB with duplicate HAWB numbers on it.

The following HAWBs occurred multiple times on this MAWB:
{0}", result.ToStringWithNewLineBetweenAppends());

				return true;
			}

			resultMessage = "";
			return false;
		}

		public bool ReSendAll(out string resultMessage)
		{
			var awbsToSend = new List<ExportAWBHeader>();
			awbsToSend.Add(awbHeader);
			foreach (ExportAWBHeader hawbHeader in awbHeader.ChildBills)
			{
				awbsToSend.Add(hawbHeader);
			}

			return SendAWBs(awbsToSend, out resultMessage);
		}

		public bool ReSendThisOnly(out string resultMessage)
		{
			return SendAWBs(new[] { awbHeader }, out resultMessage);
		}

		static void AddAWBIfRejectedOrNotSent(List<ExportAWBHeader> awbsToSend, ExportAWBHeader hawbHeader)
		{
			if (hawbHeader.EH_AWBStatus == AWBMessagingStatusList.Codes.NotSent
				|| hawbHeader.EH_AWBStatus == AWBMessagingStatusList.Codes.ErrorRejected)
			{
				awbsToSend.Add(hawbHeader);
			}
		}

		bool SendAWBs(IEnumerable<ExportAWBHeader> awbsToSend, out string resultMessage)
		{
			resultMessage = UnknownFailure_ShouldNeverHappen;
			var resetters = new List<FieldSetterResetter>();
			var sendLogger = new SendLogger(awbHeader.MasterBill);

			foreach (var awbToSend in awbsToSend)
			{
				if (!BuildAndQueueMessageWithInterchange(awbToSend, ref resultMessage, resetters, sendLogger))
				{
					ResetResetters(resetters);
					return false;
				}
			}

			resultMessage = sendLogger.ToString();

			return SaveHandlingZSaveException(ref resultMessage, resetters);
		}

		bool BuildAndQueueMessageWithInterchange(ExportAWBHeader awbHeaderParam, ref string resultMessage, List<FieldSetterResetter> resetters, SendLogger sendLogger)
		{
			var message = awbHeaderParam.Messages.AddNew();
			message.EM_MessageSubType = CIMEDIMessage.MessageSubTypes.Standalone;
			message.EM_ReceiveTransmit = CIMEDIMessage.Direction.Transmit;
			message.EM_ApplicationReference = awbHeaderParam.MasterBill;
			new MessageSenderManager(message).LogSenderIfRequired(contactSending);

			if (awbHeaderParam.AWBType == ExportAWBHeader.TypeOfAWB.AgentMaster)
			{
				message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FWB;
				message.EM_MessageText = new FWB(new FWBMessageDetails(awbHeaderParam), FWB.Version.No16).ToString();

				sendLogger.SentFWB();
			}
			else if (awbHeaderParam.AWBType == ExportAWBHeader.TypeOfAWB.House)
			{
				message.EM_MessageType = CIMEDIMessage.MessageTypes.Sent.FHL;
				message.EM_MessageText = new FHL(new FWBMessageDetails(awbHeaderParam.ParentBill), new FHLMessageDetails(awbHeaderParam), FHL.Version.No4).ToString();

				sendLogger.SentFHL();
			}
			else
			{
				message.Delete();
				resultMessage = FunctionalityNotImplementedYet;
				return false;
			}
			CreateInterchange(message);

			resetters.Add(new FieldSetterResetter(awbHeaderParam.EH_AWBStatusInfo, AWBMessagingStatusList.Codes.SentToAirlines, message));

			return true;
		}

		void CreateInterchange(CIMEDIMessage message)
		{
			var interchange = message.Factory.New<CIMEDIInterchange>();

			interchange.EI_HeaderText = ZString.Empty;  // This will be set in the Sending Processor as it changes when being sent to different places / by different means.
			interchange.EI_BodyText = message.EM_MessageText;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = CIMEDIInterchange.GetERouterFromCode();
			interchange.EI_To = CIMEDIInterchange.GetERouterToCode();

			message.EM_EI = interchange.PK;
		}

		bool SaveHandlingZSaveException(ref string resultMessage, List<FieldSetterResetter> resetters)
		{
			bool result = true;
			try
			{
				awbHeader.Factory.Save();
			}
			catch (ZSaveException saveException)
			{
				ResetResetters(resetters);

				resultMessage = saveException.Message;
				result = false;
				ZExceptionReporting.HandleSaveException(saveException);
			}

			return result;
		}

		void ResetResetters(List<FieldSetterResetter> resetters)
		{
			try
			{
				foreach (var resetter in resetters)
				{
					resetter.Reset();
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}
			}
		}

		class FieldSetterResetter
		{
			internal FieldSetterResetter(ZPropertyInfo info, ZString newValue, CIMEDIMessage message)
			{
				this.info = info;
				this.message = message;
				this.oldValue = (ZString)info.Value;
				info.SetValueFromString(newValue);
			}

			readonly ZString oldValue;
			readonly ZPropertyInfo info;
			readonly CIMEDIMessage message;

			internal void Reset()
			{
				info.SetValueFromString(oldValue);
				if (message != null)
				{
					var interchange = message.Interchange;
					if (interchange != null)
					{
						interchange.Delete();
					}
					message.Delete();
				}
			}
		}

		class SendLogger
		{
			internal SendLogger(ZString mawbNumber)
			{
				this.mawbNumber = mawbNumber;
			}
			readonly ZString mawbNumber;

			bool fwbSent;
			int fhlsSent;

			internal void SentFWB()
			{
				fwbSent = true;
			}

			internal void SentFHL()
			{
				fhlsSent++;
			}

			public override string ToString()
			{
				string whatWasSent = Res.GetString("cff3ea34-dfd4-477b-98cc-f74a6216b5b9", "Nothing");

				if (fwbSent)
				{
					whatWasSent = "FWB";
				}

				if (fhlsSent > 0)
				{
					whatWasSent = fwbSent ? whatWasSent + " + " : "";

					if (fhlsSent == 1)
					{
						whatWasSent += Res.GetString("1caffc22-637a-4a32-8d84-34848ed790ae", "1 FHL");
					}
					else
					{
						whatWasSent += fhlsSent.ToString(Culture.Invariant) + " FHLs";
					}
				}

				return Res.GetString("54a10cac-b2f4-4082-81eb-2c465198acbd", "{0} sent for MAWB: {1}-{2}", whatWasSent, mawbNumber.Left(3), mawbNumber.SubstringSafe(3));
			}
		}

		internal static string FunctionalityNotImplementedYet
		{
			get { return Res.GetString("66b263a3-5433-4911-bb2b-428b005b92bc", "There is no currently no messaging support for this type of AWB. You can only send messages for Agent MAWBs and HAWBs."); }
		}

		internal static string UnknownFailure_ShouldNeverHappen
		{
			get { return Res.GetString("27875f67-fc86-4fde-888c-3cf02c4cd032", "Unknown failure in Message Sending."); }
		}
	}
}
