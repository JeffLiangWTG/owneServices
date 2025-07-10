using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.PeriodicMonthlyStatementRerouteResponse)]
	public partial class QXCommon : MessageBlock, IStatementRerouteResponse
	{
		#region IRerouteResponse Members

		IStatementRerouteResponse ACEBlock
		{
			get
			{
				if (aceBlock == null && IsACEBlock)
				{
					aceBlock = new APMSQX();
					aceBlock.Deserialise(this.Serialise());
				}
				return aceBlock;
			}
		}
		APMSQX aceBlock;

		IStatementRerouteResponse LegacyBlock
		{
			get
			{
				if (legacyBlock == null && !IsACEBlock)
				{
					legacyBlock = new PMSQX();
					legacyBlock.Deserialise(this.Serialise());
				}
				return legacyBlock;
			}
		}
		PMSQX legacyBlock;

		IStatementRerouteResponse Block
		{
			get { return ACEBlock ?? LegacyBlock; }
		}

		bool IsACEBlock
		{
			get
			{
				var severityCode = Data.SubstringSafe(1, 1);
				return severityCode == "I" || severityCode == "F";
			}
		}

		ZString IStatementRerouteResponse.ErrorCode
		{
			get { return Block != null ? Block.ErrorCode : ZString.Empty; }
		}

		ZString IStatementRerouteResponse.MessageText
		{
			get { return Block != null ? Block.MessageText : ZString.Empty; }
		}

		ZInt IStatementRerouteResponse.TotalNumberOfReroutes
		{
			get { return Block != null ? Block.TotalNumberOfReroutes : ZInt.Zero; }
		}

		#endregion
	}

	[ApplicationIdentifier(MessageBlockDictionary.EmptyApplicationIdentifier, MessageBlockDictionary.ACEApplicationCode)]
	public partial class APMSQX : MessageBlock, IStatementRerouteResponse
	{
		#region IRerouteResponse Members

		ZString IStatementRerouteResponse.ErrorCode
		{
			get { return ConditionCode; }
		}

		ZString IStatementRerouteResponse.MessageText
		{
			get { return NarrativeText; }
		}

		ZInt IStatementRerouteResponse.TotalNumberOfReroutes
		{
			get { return TotalNumberOfReroutes; }
		}

		#endregion
	}
}
