using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	class CensusWarningOverrrideMessageBuilder : MessageBuilder<ACEInputBlockControlGenerator>
	{
		public CensusWarningOverrrideMessageBuilder(IACECusEntryHeader entryHeader)
			: base(entryHeader)
		{
			this.entryHeader = entryHeader;
		}

		readonly IACECusEntryHeader entryHeader;

		protected override string ApplicationIdentifier
		{
			get { return ACEApplicationIdentifierCodeList.Codes.CensusWarningOverride; }
		}

		protected override ACEInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new ACEInputBlockControlGenerator(entryHeader);
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CensusWarningOverride;
		}

		protected override void UpdateMessageBlocks(ACEInputBlockControlGenerator block)
		{
			if (entryHeader.IsRemoteLocationFiling)
			{
				block.B.ProcessingDistrictPortCode = entryHeader.PreparerDistrictPort;
			}

			block.MessageBlocks.Add(CreateCW01());

			foreach (IACECusEntryLine entryLine in entryHeader.EntryLines)
			{
				block.MessageBlocks.AddRange(CreateForOneLine(entryLine));
			}
		}

		MessageBlock CreateCW01()
		{
			var result = new ACWOCW01();
			result.EntryNumber = ((ICusEntryHeader)entryHeader).EntryNumber;
			result.EntryFilerCode = entryHeader.EntryFilerCode;
			return result;
		}

		IEnumerable<MessageBlock> CreateForOneLine(IACECusEntryLine entryLine)
		{
			int index = 1;
			ACWOCW02 cw02 = null;

			foreach (ICensusWarningOverride cwo in entryLine.CensusWarningOverrideCodes)
			{
				if (cw02 == null)
				{
					cw02 = new ACWOCW02();
					cw02.EntrySummaryLineItemIdentifier = entryLine.CL_LineNumber.ToString().PadLeft(3, '0');
				}

				UpdateCW02(cw02, index, cwo.ConditionCode, cwo.OverrideCode);

				index++;

				if (index == 8)
				{
					index = 1;
					yield return cw02;
					cw02 = null;
				}
			}

			if (cw02 != null)
			{
				yield return cw02;
			}
		}

		void UpdateCW02(ACWOCW02 cw02, int index, ZString conditionCode, ZString overrideCode)
		{
			switch (index)
			{
				case 1:
					cw02.CensusWarningConditionCode1 = conditionCode;
					cw02.CensusWarningConditionOverrideCode1 = overrideCode;
					break;

				case 2:
					cw02.CensusWarningConditionCode2 = conditionCode;
					cw02.CensusWarningConditionOverrideCode2 = overrideCode;
					break;

				case 3:
					cw02.CensusWarningConditionCode3 = conditionCode;
					cw02.CensusWarningConditionOverrideCode3 = overrideCode;
					break;

				case 4:
					cw02.CensusWarningConditionCode4 = conditionCode;
					cw02.CensusWarningConditionOverrideCode4 = overrideCode;
					break;

				case 5:
					cw02.CensusWarningConditionCode5 = conditionCode;
					cw02.CensusWarningConditionOverrideCode5 = overrideCode;
					break;

				case 6:
					cw02.CensusWarningConditionCode6 = conditionCode;
					cw02.CensusWarningConditionOverrideCode6 = overrideCode;
					break;

				case 7:
					cw02.CensusWarningConditionCode7 = conditionCode;
					cw02.CensusWarningConditionOverrideCode7 = overrideCode;
					break;
			}
		}
	}
}
