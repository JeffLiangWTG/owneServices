using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGEDIMessage : EDIMessage
	{
		public const string SG4ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;

		public SGEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = SG4ApplicationCode;
		}

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;

		protected ZString GetMonthAbbrev(string month)
		{
			switch (month)
			{
				case "01":
					return "JAN";
				case "02":
					return "FEB";
				case "03":
					return "MAR";
				case "04":
					return "APR";
				case "05":
					return "MAY";
				case "06":
					return "JUN";
				case "07":
					return "JUL";
				case "08":
					return "AUG";
				case "09":
					return "SEP";
				case "10":
					return "OCT";
				case "11":
					return "NOV";
				case "12":
					return "DEC";
				default:
					return "";
			}
		}

		#region TradeNetMessage

		public Edifact.Auto.SegmentGroup TradeNetMessage => tradeNetMessage ?? (tradeNetMessage = GetTradeNetMessage());
		Edifact.Auto.SegmentGroup tradeNetMessage;

		protected Edifact.Auto.SegmentGroup GetTradeNetMessage()
		{
			var characterSet = new UNOASGCharacterSet();
			var isTradeNet4_1 = GetMessageCode(characterSet) == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne;
			var msgFactory = isTradeNet4_1 ? SG.Business.CustomsMessaging.D09B.Sg09bEdifactMessageFactory.SG41MessageFactory
											: SG.V4.Business.CustomsMessaging.Sg05bEdifactMessageFactory.SG4MessageFactory;
			return GetAutoEdifactMessageUsingNamedFactory(msgFactory, characterSet);
		}

		protected string GetMessageCode(UNCharacterSet characterSet)
		{
			try
			{
				var unhSegment = new Edifact.Generic.UNHSegment();
				string unhString = EM_MessageText.Substring(0, EM_MessageText.IndexOf(characterSet.SegmentDelimiterChar));
				unhSegment.Parse(characterSet, unhString);
				return unhSegment.MessageIdentifier.AssociationAssignedCode;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return "";
			}
		}

		protected bool IsTradeNet4_1Message(string code)
		{
			return (code == SGConstants.TradeNetVersion.AssociationAssignedCodes.FourPointOne);
		}

		#endregion
	}
}
