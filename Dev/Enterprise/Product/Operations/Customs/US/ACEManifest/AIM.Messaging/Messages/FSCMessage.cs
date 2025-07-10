using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSCMessage : FSNMessage
	{
		public FSCMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZString MessageTypeCode => Constants.AIMMessageSubTypes.FSC;
		public override ZString MessageTypeDescription => "Freight Status Condition";

		#region Freight Status Condition

		public ZString StatusAnswerCode => FreightStatusConditions.StatusAnswerCode.Trim();

		AIMFreightStatusConditions FreightStatusConditions => freightStatusConditions ?? (freightStatusConditions = PopulateMessageBlock(new AIMFreightStatusConditions()));
		AIMFreightStatusConditions freightStatusConditions;

		#endregion

		#region WayBills relatings

		public AIMWaybill WayBill => wayBill ?? (wayBill = PopulateMessageBlock(new AIMWaybill()));
		AIMWaybill wayBill;

		public AIMArrivalWrapper AirWayBillArrival
		{
			get
			{
				if (airWayBillArrival == null)
				{
					var wayBillFound = false;
					var wayBillLineIdentifier = (new AIMWaybill().GetFieldInfos().FirstOrDefault(inf => inf.Position == 1) as AWBMessageBlock.SpecialFieldInfo).Value;
					var arrivalBlock = new AIMArrival();
					var arrivalLineIdentifier = (arrivalBlock.GetFieldInfos().FirstOrDefault(inf => inf.Position == 1) as AWBMessageBlock.SpecialFieldInfo).Value;

					var lineEnumerator = Lines.GetEnumerator();
					while (lineEnumerator.MoveNext())
					{
						var currentLine = lineEnumerator.Current;
						if (!wayBillFound && currentLine.StartsWith(wayBillLineIdentifier + "/"))
						{
							wayBillFound = true;
						}
						else if (wayBillFound && currentLine.StartsWith(arrivalLineIdentifier + "/"))
						{
							PopulateMessageBlock(arrivalBlock, currentLine);
							airWayBillArrival = new AIMArrivalWrapper(arrivalBlock);
						}
					}
				}

				return airWayBillArrival;
			}
		}
		AIMArrivalWrapper airWayBillArrival;

		public AIMAirWaybill_FSQ_FSC WayBillFSC
		{
			get
			{
				if (wayBillFSC == null)
				{
					wayBillFSC = new AIMAirWaybill_FSQ_FSC();
					try
					{
						PopulateMessageBlock(wayBillFSC, lineIndex: 2);
					}
					catch (US.Messaging.Business.InvalidMessageFormatException)
					{
						PopulateMessageBlock(wayBillFSC, lineIndex: 1);
					}
				}
				return wayBillFSC;
			}
		}
		AIMAirWaybill_FSQ_FSC wayBillFSC;

		protected override ZString GetHawbNumber()
		{
			return WayBillFSC.HAWBNumber.Trim();
		}

		#endregion

		#region Text

		public ZString Information => Text.Information.Trim();

		AIMText Text => text ?? (text = PopulateMessageBlock(new AIMText()));
		AIMText text;

		public IEnumerable<ZString> TextContinuation
		{
			get
			{
				if (textContinuation == null)
				{
					textContinuation = new List<ZString>();
					var textRowFound = false;
					foreach (var line in Lines)
					{
						if (textRowFound && line.StartsWith("/"))
						{
							textContinuation.Add(line);
						}

						textRowFound = textRowFound || line.StartsWith("TXT/");
					}
				}

				return textContinuation;
			}
		}
		List<ZString> textContinuation;

		public IEnumerable<ZString> Conditions
		{
			get
			{
				if (conditions == null)
				{
					conditions = new List<ZString>();
					foreach (var line in Lines)
					{
						if (line.StartsWith("TXT/"))
						{
							conditions.Add(line.RemoveSafe(0, 4));
						}
					}
				}

				return conditions;
			}
		}
		List<ZString> conditions;

		#endregion

		#region SplitBillArrivals

		public IEnumerable<AIMArrivalWrapper> SplitBillArrivals => splitBillArrivals ?? SetSplitBillArrivals();
		List<AIMArrivalWrapper> splitBillArrivals;

		List<AIMArrivalWrapper> SetSplitBillArrivals()
		{
			splitBillArrivals = new List<AIMArrivalWrapper>();
			if (StatusAnswerCode == AIMFreightStatusCodes.Codes.BillIsSplit)
			{
				foreach (var line in TextContinuation)
				{
					var splitBillArrival = new AIMText_Continuation_FSC_SplitBillArrival();
					splitBillArrival.Deserialise(line);
					splitBillArrivals.Add(new AIMArrivalWrapper(splitBillArrival));
				}
			}

			return splitBillArrivals;
		}

		#endregion

		#region RoutingInformation

		public AIMText_FSC_RoutingInformation RoutingInformation => routingInformation ?? SetRoutingInformation();
		AIMText_FSC_RoutingInformation routingInformation;

		AIMText_FSC_RoutingInformation SetRoutingInformation()
		{
			routingInformation = new AIMText_FSC_RoutingInformation();
			if (StatusAnswerCode == AIMFreightStatusCodes.Codes.RoutingInformationFollows)
			{
				PopulateMessageBlock(routingInformation);
			}

			return routingInformation;
		}

		#endregion

		#region Message Text Interpretation

		protected override ZString GetMessageInterpretationHeader()
		{
			return ZString.Empty;
		}

		protected override ZString GetMessageInterpretationBody()
		{
			var builder = new ZStringBuilder();
			builder.Append($"Masterbill: {AirWaybillPrefix}-{AirWaybillSerialNumber}");
			if (!HAWBNumber.IsEmpty)
			{
				builder.Append($"Housebill: {HAWBNumber}");
			}

			builder.Append(ZString.Empty);
			var statusAnswerCodeList = Factory.GetCachedValue<AIMFreightStatusCodes>();
			var statusAnswerCode = StatusAnswerCode;
			builder.Append($"{statusAnswerCode} - {statusAnswerCodeList.GetDescriptionFromCode(statusAnswerCode)}");
			foreach (var txtLine in Lines.Where(line => line.StartsWith("TXT/") || line.StartsWith("/")))
			{
				builder.Append($"{Regex.Replace(txtLine, @"^(TXT)?/", string.Empty).TrimEnd()}");
			}

			AppendLinesToInterpretation("WBL", builder);
			AppendLinesToInterpretation("ARR", builder);
			AppendLinesToInterpretation("TRN", builder);

			builder.Append(ZString.Empty);
			builder.Append(ZString.Empty);
			builder.Append(EM_MessageText);

			return builder.ToStringWithNewLineBetweenAppends();
		}

		#endregion
	}
}
