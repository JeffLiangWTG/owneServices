using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public abstract class AIMInboundMessage : AIMEDIMessage
	{
		protected AIMInboundMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = EDIMessageTypeList.Codes.FHL;
			EM_MessageSubType = MessageTypeCode;
		}

		public const string MasterAirWaybillIndicator = "M";

		public abstract ZString MessageTypeCode { get; }
		public abstract ZString MessageTypeDescription { get; }

		public ZString PackageTrackingIdentifier => AirWaybill.PackageTrackingIdentifier.Trim();

		public ZString AirWaybillPrefix => AirWaybill.AirWaybillPrefix.Trim();

		public ZString AirWaybillSerialNumber => AirWaybill.AWBSerialNumber.Trim();

		public ZString HAWBNumber
		{
			get
			{
				if (!hawbNumber.HasValue)
				{
					hawbNumber = GetHawbNumber();
				}

				return hawbNumber.Value;
			}
		}
		ZString? hawbNumber;

		protected virtual ZString GetHawbNumber()
		{
			var hawbNum = AirWaybill.HAWBNumber.Trim();
			return hawbNum != MasterAirWaybillIndicator ? hawbNum : ZString.Empty;
		}

		public ZString MAWBNumber => mawbNumber ?? (mawbNumber = AirWaybillPrefix + AirWaybillSerialNumber).Value;
		ZString? mawbNumber;

		public ZString MAWBNumberFormatted
		{
			get
			{
				if (!mawbNumberFormatted.HasValue)
				{
					var prefix = AirWaybillPrefix;
					if (!prefix.IsEmpty)
					{
						prefix += '-';
					}

					mawbNumberFormatted = prefix + AirWaybillSerialNumber;
				}

				return mawbNumberFormatted.Value;
			}
		}
		ZString? mawbNumberFormatted;

		public AIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = GetAirwayBill());
		AIMAirWaybill airWaybill;

		// AirWaybill is expected in line 2 but can appear in line 1.
		AIMAirWaybill GetAirwayBill()
		{
			var airwayBill = new AIMAirWaybill();

			try
			{
				PopulateMessageBlock(airwayBill, lineIndex: 2);
			}
			catch (US.Messaging.Business.InvalidMessageFormatException)
			{
				PopulateMessageBlock(airwayBill, lineIndex: 1);
			}

			return airwayBill;
		}

		#region PopulateMessageBlock

		protected T PopulateMessageBlock<T>(T messageBlock) where T : AWBMessageBlock
		{
			var fieldInfo = messageBlock.GetFieldInfos().FirstOrDefault(inf => inf.Position == 1) as AWBMessageBlock.SpecialFieldInfo;
			var lineIdentifier = fieldInfo?.Value ?? ZString.Empty;
			if (!lineIdentifier.IsEmpty)
			{
				var lineText = Lines.FirstOrDefault(l => l.StartsWith(lineIdentifier + "/"));
				PopulateMessageBlock(messageBlock, lineText);
			}

			return messageBlock;
		}

		protected T PopulateMessageBlock<T>(T messageBlock, int lineIndex) where T : AWBMessageBlock
		{
			if (Lines.Count > lineIndex)
			{
				PopulateMessageBlock(messageBlock, Lines[lineIndex]);
			}

			return messageBlock;
		}

		protected void PopulateMessageBlock(AWBMessageBlock messageBlock, ZString lineText)
		{
			if (!lineText.IsEmpty)
			{
				messageBlock.Deserialise(lineText);
			}
		}

		#endregion

		protected IList<ZString> Lines
		{
			get
			{
				if (lines == null)
				{
					EnsureMessageIsWellFormatted();
					lines = EM_MessageText.SplitIgnoringEscapedDelimiter('\n', ' ', true);
				}
				return lines;
			}
		}
		ZString[] lines;

		void EnsureMessageIsWellFormatted()
		{
			if (!EM_MessageText.EndsWith("\r\n"))
			{
				EM_MessageText += "\r\n";
			}
		}
	}
}
