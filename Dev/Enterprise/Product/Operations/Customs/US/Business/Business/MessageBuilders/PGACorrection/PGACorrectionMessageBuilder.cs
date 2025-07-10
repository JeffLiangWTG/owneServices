using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;

namespace Enterprise.Customs.US.Business
{
	public class PGACorrectionMessageBuilder
	{
		public PGACorrectionMessageBuilder(CusEntryHeader entryHeader, ISEAdditionalData additionalData, bool enableTracking)
		{
			this.entryHeader = entryHeader;
			this.additionalData = additionalData;
			this.enableTracking = enableTracking;

			var processingPortCode = ((IFDACorrectionHeader)(entryHeader.Declaration)).ProcessingDistrictPort;
			var processingOfficeCode = entryHeader.Declaration.ProcessingOfficeCode;
			this.block = new ACEInputBlockControlGenerator(entryHeader.Branch.GB_GC.ToGuid(), processingPortCode, processingOfficeCode);
			block.B.ApplicationIdentifierCode = ACEApplicationIdentifierCodeList.Codes.PGADataCorrection;
			GenerateBlocks();
		}
		readonly ACEInputBlockControlGenerator block;
		readonly IACECusEntryHeader entryHeader;
		readonly ISEAdditionalData additionalData;
		readonly bool enableTracking;

		public void GenerateMessage()
		{
			var message = block.CreateMessage<MQEDIMessage>(entryHeader.Factory);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.PGADataCorrection;
			entryHeader.Messages.Add(message);
		}

		public ZString GetSerialiseMessageContents()
		{
			return block.Serialise(true);
		}

		void GenerateBlocks()
		{
			var ca10 = new APDCCA10();
			ca10.EntryFilerCode = entryHeader.EntryFilerCode;
			ca10.EntryNumber = ((ICusEntryHeader)entryHeader).EntryNumber;
			ca10.ActionCode = "R";
			block.AddMessageBlock(ca10);

			var entryLines = entryHeader.GetEntryLinesWithPGAToSend();
			foreach (IACECusEntryLine entryLine in entryLines)
			{
				block.AddMessageBlock(new APDCCA40() { LineItemIdentifier = entryLine.CL_LineNumber });
				block.AddMessageBlock(new APDCCA60() { TariffNumber = entryLine.Tariff });

				entryLine.ClearPGALineNumbers();

				foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
				{
					block.AddMessageBlock(new APDCCA60() { TariffNumber = secondaryLine.Tariff });
				}

				var lineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, (IPGAGovernmentAgenciesCommon)entryLine);
				var shouldBuildOIBlock = true;
				if (lineToSend.HasLinesToSend())
				{
					var blocks = BuildPGABlocks(lineToSend, additionalData, buildOI: shouldBuildOIBlock).ToArray();
					block.AddMessageBlocks(blocks);
					shouldBuildOIBlock = blocks.Length == 0;
				}

				foreach (ISecondaryTariffLine secondaryLine in entryLine.SecondaryTariffLines)
				{
					lineToSend = new PGAGovernmentAgenciesWithPGACorrection(entryLine, secondaryLine);
					if (lineToSend.HasLinesToSend())
					{
						var blocks = BuildPGABlocks(lineToSend, additionalData, buildOI: shouldBuildOIBlock).ToArray();
						block.AddMessageBlocks(blocks);
						shouldBuildOIBlock = blocks.Length == 0;
					}
				}
			}
		}

		IEnumerable<MessageBlock> BuildPGABlocks(IGovernmentAgenciesCommon lineToSend, IAcknowledgeAndSign signed, bool buildOI)
		{
			foreach (MessageBlock block in PGABlocksCreator.BuildPGABlocks(lineToSend, signed, enablePGATracking: enableTracking, isPGACorrection: true, buildOI: buildOI))
			{
				yield return block;
			}
		}
	}
}
