using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class BorderCargoReleaseMessageBuilder : BorderCargoReleaseMessageBuilderBase<ABIInputBlockControlGenerator>
	{
		public BorderCargoReleaseMessageBuilder(ICargoReleaseCusEntryHeader entryHeader, UpdateActionCode action)
			: base(entryHeader, action)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.BorderCargoRelease; }
		}

		protected override ABIInputBlockControlGenerator GetNewInputBlockControlGenerator()
		{
			return new ABIInputBlockControlGenerator(entryHeader);
		}

		protected override void SetMessageSubType(MQEDIMessage message)
		{
			switch (action)
			{
				case UpdateActionCode.Delete:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseAdd;
					break;
			}
		}
	}

	public abstract class BorderCargoReleaseMessageBuilderBase<T> : EntryHeaderMessageBuilder<T> where T : BlockControlGenerator
	{
		protected BorderCargoReleaseMessageBuilderBase(ICargoReleaseCusEntryHeader entryHeader, UpdateActionCode action)
			: base(entryHeader, action)
		{
		}

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> supportedList = base.GetSupportedUpdateActionCodeList();
			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Replace);
			supportedList.Add(UpdateActionCode.Delete);
			return supportedList;
		}

		protected new ICargoReleaseCusEntryHeader entryHeader
		{
			get { return (ICargoReleaseCusEntryHeader)base.entryHeader; }
		}

		protected override void UpdateMessageBlocks(T block)
		{
			block.MessageBlocks.AddRange(new BorderCargoReleaseBlockBuilder(entryHeader, ApplicationIdentifier).Build(action));
		}
	}

	public class BorderCargoReleaseBlockBuilder
	{
		public BorderCargoReleaseBlockBuilder(ICargoReleaseCusEntryHeader entryHeader, string applicationIdentifier)
		{
			this.entryHeader = entryHeader;
			this.applicationIdentifier = applicationIdentifier;
		}

		public IEnumerable<MessageBlock> Build(UpdateActionCode action)
		{
			ClearCachedValues();
			BuildCore(action);
			return EntryHeaderBlock;
		}

		protected virtual void BuildCore(UpdateActionCode action)
		{
			EntryHeaderBlock.Add(MakeBCR01(action));

			if (action != UpdateActionCode.Delete)
			{
				EntryHeaderBlock.AddRange(MakeBCR0Ms());

				foreach (ICargoReleaseCusEntryLine entryLine in entryHeader.EntryLines)
				{
					EntryHeaderBlock.AddRange(GenerateEntryLine(entryLine));
				}
			}
		}

		void ClearCachedValues()
		{
			entryHeaderBlock = null;
		}

		protected readonly ICargoReleaseCusEntryHeader entryHeader;
		protected readonly string applicationIdentifier;

		protected List<MessageBlock> EntryHeaderBlock
		{
			get { return entryHeaderBlock ?? (entryHeaderBlock = new List<MessageBlock>()); }
		}
		List<MessageBlock> entryHeaderBlock;

		BCR01 MakeBCR01(UpdateActionCode action)
		{
			BCR01 bcr01 = new BCR01();
			bcr01.UpdateActionCode = UpdateActionCodeConverter.ConvertToString(action);
			bcr01.DistrictPortOfEntry = entryHeader.DistrictPortOfEntry;
			bcr01.FilerCode = entryHeader.EntryFilerCode;
			bcr01.EntryNumber = MQEDIMessage.USEntryNumberPlaceHolder;
			bcr01.ModeOfTransportationMOTCode = entryHeader.ModeOfTransportationCode;
			bcr01.ImporterOfRecord = entryHeader.ImporterOfRecordNumber;
			bcr01.BondType = ZInt.ParseEmptyAsZero(entryHeader.BondType);
			bcr01.SuretyCode = entryHeader.SuretyCode;

			if (entryHeader.UltimateConsigneeNumber.IsEmpty && entryHeader.IsConsigneeNameAddressUsed)
			{
				bcr01.ConsigneeNameAndAddress = "1";
			}

			bcr01.UltimateConsignee = entryHeader.UltimateConsigneeNumber;

			bcr01.DateOfArrival = entryHeader.EstimatedDateOfArrival;
			bcr01.EntryType = entryHeader.EntryType;
			bcr01.CarrierCode = entryHeader.CarrierCode;

			return bcr01;
		}

		IEnumerable<MessageBlock> MakeBCR0Ms()
		{
			foreach (IBillDetails billDetails in entryHeader.LowestBillDetails)
			{
				BCR0M bcr0m = new BCR0M();
				if (entryHeader.EntryType != EntryTypeList.Codes.ConsumptionFTZ)
				{
					bcr0m.MasterBillNumber = billDetails.MasterBillNumber;
				}

				bcr0m.HouseBillNumber = billDetails.HouseBillNumber;
				bcr0m.SubHouseBillNumber = billDetails.SubHouseBillNumber;
				bcr0m.Quantity = billDetails.PackageQuantity;
				bcr0m.Unit = billDetails.PackageType;
				bcr0m.IssuerOfMasterBillNumber = billDetails.IssuerCodeOfMasterBillNumber;
				bcr0m.IssuerCodeOfHouseBillNumber = billDetails.IssuerCodeOfHouseBillNumber;
				yield return bcr0m;
			}
		}

		IEnumerable<MessageBlock> GenerateEntryLine(ICargoReleaseCusEntryLine entryLine)
		{
			yield return MakeBCR02(entryLine);

			foreach (MessageBlock block in OGABlocksCreator.GetDisclaimingBlocks(entryLine, true))
			{
				yield return block;
			}

			foreach (MessageBlock block in PGABlocksCreator.GetLaceyActBlocks(entryLine, true))
			{
				yield return block;
			}

			foreach (MessageBlock block in OGABlocksCreator.GetOGABlocks(entryLine, true, applicationIdentifier))
			{
				yield return block;
			}
		}

		BCR02 MakeBCR02(ICargoReleaseCusEntryLine entryLine)
		{
			BCR02 bcr02 = new BCR02();
			bcr02.CountryOfOrigin = entryLine.CountryOfOrigin;
			bcr02.TariffNumber = entryLine.Tariff;
			bcr02.ManufacturerIDCode = entryLine.ManufacturerSupplierCode;
			bcr02.UltimateConsignee = entryLine.UltimateConsigneeNumber;
			bcr02.LineItemValue = ZDecimal.Zero; // WI00025244 DN - Phyllis said we should send blank as it is an optional field
			return bcr02;
		}

		OGABlocksCreator OGABlocksCreator
		{
			get { return ogaBlocksCreator ?? (ogaBlocksCreator = new OGABlocksCreator()); }
		}
		OGABlocksCreator ogaBlocksCreator;

		PGABlocksCreator PGABlocksCreator
		{
			get { return pgaBlocksCreator ?? (pgaBlocksCreator = new PGABlocksCreator()); }
		}
		PGABlocksCreator pgaBlocksCreator;
	}
}
