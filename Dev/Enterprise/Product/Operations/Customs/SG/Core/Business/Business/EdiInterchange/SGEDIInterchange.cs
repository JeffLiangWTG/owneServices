using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGEDIInterchange : EDIInterchange
	{
		public SGEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		// EDIFACT segment identifier used for format recognition.
		public const string EdifactInterchangeServiceSegment = "UNA:+";
		public const string EdifactInterchangeHeaderSegment = "UNB+";

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = EDIInterchange.ApplicationCodes.SGCustomsTradenetXML;
		}

		public override bool IsTestInterchange
		{
			get
			{
				return EI_ApplicationCode == EDIInterchange.ApplicationCodes.SingaporeTradenet4
					? base.IsTestInterchange
					: EI_From == EDIInterchange.InterchangePartyIDs.TradeNetV4TestSystem;
			}
		}

		public override UNCharacterSet CharacterSet => EI_ApplicationCode == EDIInterchange.ApplicationCodes.SingaporeTradenet4 ? base.CharacterSet : null;

		#region Create New Interchange From xml or Edifact String

		public static EDIInterchange CreateNewInterchangeFromXmlOrEdifactString(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, bool shouldReportWholeErrorMessage = false)
		{
			return CreateNewInterchange(factory, interchangeString, applicationCode, false, false, shouldReportWholeErrorMessage);
		}

		public static EDIInterchange CreateNewInterchange(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode, bool ignoreFunctionalGroup, bool onlyCreateInterchange, bool shouldReportWholeErrorMessage = false)
		{
			EDIInterchange result = null;
			if (factory != null)
			{
				if (IsEDIFactVersion(interchangeString))
				{
					result = CreateNewInterchangeFromString(factory, interchangeString, applicationCode, ignoreFunctionalGroup, onlyCreateInterchange, shouldReportWholeErrorMessage);
				}
				else
				{
					result = CreateNewXmlInterchange(factory, interchangeString, ApplicationCodes.SGCustomsTradenetXML);
				}
			}

			return result;
		}

		static bool IsEDIFactVersion(string messageText)
		{
			return messageText.StartsWith(EdifactInterchangeServiceSegment, StringComparison.OrdinalIgnoreCase) || messageText.StartsWith(EdifactInterchangeHeaderSegment, StringComparison.OrdinalIgnoreCase);
		}

		static EDIInterchange CreateNewXmlInterchange(BusinessObjectFactory factory, ZString interchangeString, ZString applicationCode)
		{
			var result = factory.New<SGEDIInterchange>();

			var shouldDelete = true;

			try
			{
				result.EI_ApplicationCode = applicationCode;
				result.EI_InterchangeType = applicationCode;
				result.EI_BodyText = interchangeString;
				result.EI_From = GetValueFromTag(interchangeString, "SenderID");                      // DCST.DCST401
				result.EI_To = GetValueFromTag(interchangeString, "RecipientID");                     // V13T.V13T001
				result.EI_InterchangeNum = GenerateInterchangeNumberFromURN(interchangeString);
				result.EI_Status = Status.Received;
				result.EI_ReceiveTransmit = Direction.Receive;
				result.EI_Priority = "HGH";
				result.EI_SystemCreateTimeUtc = ZDateTime.UtcNow;
				result.EI_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
				result.EI_SystemLastEditTimeUtc = result.EI_SystemCreateTimeUtc;
				result.EI_SystemLastEditUser = result.EI_SystemCreateUser;
				result.EI_IsActive = true;

				var message = result.ContainedMessages.AddNew(typeof(SGXmlEDIMessage));

				message.EM_MessageText = interchangeString;
				message.EM_IsTestMessage = result.IsTestInterchange;
				message.EM_Status = SGXmlEDIMessage.Status.Queued;
				message.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;

				message.EM_GB = result.EI_GB;
				message.EM_GE = GlbDepartment.CurrentDepartment.PK;

				message.EM_SystemCreateTimeUtc = result.EI_SystemCreateTimeUtc;
				message.EM_SystemCreateUser = result.EI_SystemCreateUser;
				message.EM_SystemLastEditTimeUtc = result.EI_SystemLastEditTimeUtc;
				message.EM_SystemLastEditUser = result.EI_SystemLastEditUser;

				shouldDelete = false;
			}
			finally
			{
				if (shouldDelete)
				{
					result.Delete(); // effectively a memory transaction
				}
			}

			return result;
		}

		static string GetValueFromTag(string interchangeText, string tagToFind)
		{
			var result = ZString.Empty;
			int startPos = interchangeText.IndexOf(tagToFind + ">", StringComparison.OrdinalIgnoreCase);
			if (startPos > 0)
			{
				int valuePos = startPos + (tagToFind + ">").Length;
				int endPos = interchangeText.IndexOf("</cbc:" + tagToFind, StringComparison.OrdinalIgnoreCase);
				int valueLength = endPos - valuePos;
				result = interchangeText.Substring(valuePos, valueLength);
			}

			return result;
		}

		static string GenerateInterchangeNumberFromURN(string interchangeText)
		{
			var interchangeNumber = ZDateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			int startPos = interchangeText.IndexOf("UniqueReferenceNumber><cbc:ID>", StringComparison.OrdinalIgnoreCase);
			if (startPos > 0)
			{
				var urnElements = interchangeText.Substring(startPos, 160);
				var sequenceNumber = GetValueFromTag(urnElements, "SequenceNumeric");
				interchangeNumber += sequenceNumber;
			}

			return interchangeNumber.Trim();
		}

		#endregion

	}
}
