using System.Collections.Generic;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class CargoReleaseMessageBuilder : CargoReleaseMessageBuilderBase<ABIInputBlockControlGenerator>
	{
		public CargoReleaseMessageBuilder(ICargoReleaseCusEntryHeader entryHeader, UpdateActionCode action, bool certifyCargoRelease)
			: base(entryHeader, action, certifyCargoRelease)
		{
		}

		protected override string ApplicationIdentifier
		{
			get { return ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions; }
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
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseDelete;
					break;
				case UpdateActionCode.Replace:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseReplace;
					break;
				default:
					message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseAdd;
					break;
			}
		}
	}

	public abstract class CargoReleaseMessageBuilderBase<T> : EntryHeaderMessageBuilder<T> where T : BlockControlGenerator
	{
		public CargoReleaseMessageBuilderBase(ICargoReleaseCusEntryHeader entryHeader, UpdateActionCode action, bool certifyCargoRelease)
			: base(entryHeader, action)
		{
			this.certifyCargoRelease = certifyCargoRelease;
		}

		#region Implementation

		protected override List<UpdateActionCode> GetSupportedUpdateActionCodeList()
		{
			List<UpdateActionCode> supportedList = base.GetSupportedUpdateActionCodeList();

			supportedList.Add(UpdateActionCode.Add);
			supportedList.Add(UpdateActionCode.Delete);
			supportedList.Add(UpdateActionCode.Replace);

			return supportedList;
		}

		protected override void UpdateMessageBlocks(T block)
		{
			block.MessageBlocks.AddRange(new CargoReleaseBlockBuilder(entryHeader, ApplicationIdentifier).Build(action, certifyCargoRelease));
		}

		protected new ICargoReleaseCusEntryHeader entryHeader
		{
			get { return (ICargoReleaseCusEntryHeader)base.entryHeader; }
		}
		protected readonly bool certifyCargoRelease;

		#endregion
	}

	public class CargoReleaseBlockBuilder
	{
		public CargoReleaseBlockBuilder(ICargoReleaseCusEntryHeader entryHeader, string applicationIdentifier)
		{
			this.entryHeader = entryHeader;
			this.applicationIdentifier = applicationIdentifier;
		}

		public IEnumerable<MessageBlock> Build(UpdateActionCode action, bool certifyCargoRelease)
		{
			ClearCachedValues();
			BuildCore(action, certifyCargoRelease);
			return EntryHeaderBlock;
		}

		protected virtual void BuildCore(UpdateActionCode action, bool certifyCargoRelease)
		{
			EntryHeaderBlock.Add(GenerateCRLH1(action, certifyCargoRelease));

			if (action != UpdateActionCode.Delete)
			{
				EntryHeaderBlock.Add(GenerateCRLH2());
				EntryHeaderBlock.AddRange(GenerateCRLHA());

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

		public CRLH1 GenerateCRLH1(UpdateActionCode action, bool certifyCargoRelease)
		{
			CRLH1 crlH1 = new CRLH1();
			crlH1.UpdateActionCode = UpdateActionCodeConverter.ConvertToString(action);

			crlH1.DistrictPortOfEntry = entryHeader.DistrictPortOfEntry;
			crlH1.EntryFilerCode = entryHeader.EntryFilerCode;
			crlH1.EntryNumber = MQEDIMessage.USEntryNumberPlaceHolder;
			crlH1.ImporterNumber = entryHeader.ImporterOfRecordNumber;

			if (entryHeader.EntryType != EntryTypeList.Codes.ConsumptionFTZ)
			{
				crlH1.ModeOfTransportationMOTCode = entryHeader.ModeOfTransportationCode;
			}
			crlH1.EstimatedDateOfArrival = entryHeader.EstimatedDateOfArrival;
			crlH1.BondTypeCode = entryHeader.BondType;
			crlH1.ReleaseCertificationCode = certifyCargoRelease ? 1 : 0;
			crlH1.PresentationDate = entryHeader.PresentationDate;

			if (entryHeader.EntryType != EntryTypeList.Codes.ConsumptionFTZ)
			{
				crlH1.CarrierCode = entryHeader.CarrierCode;
			}

			crlH1.DistrictPortOfUnlading = entryHeader.DistrictPortOfUnlading;

			crlH1.EntryType = entryHeader.EntryType;
			crlH1.SuretyCode = entryHeader.SuretyCode;

			if (entryHeader.UltimateConsigneeNumber.IsEmpty && entryHeader.IsConsigneeNameAddressUsed)
			{
				crlH1.ConsigneeNameAndAddress = "1";
			}
			return crlH1;
		}

		public CRLH2 GenerateCRLH2()
		{
			CRLH2 crlH2 = new CRLH2();
			crlH2.LocationOfGoods = entryHeader.LocationOfGoods;

			crlH2.UltimateConsigneeNumber = entryHeader.UltimateConsigneeNumber;

			crlH2.EntryDateElectionCode = entryHeader.EntryDateElectionCode;
			crlH2.VoyageFlightTripManifestNumber = entryHeader.VoyageNumber.Left(5);
			crlH2.TotalEntryValue = entryHeader.TotalValueOfEntrySummary.Round(0);
			crlH2.BrokerReferenceNumber = entryHeader.JobReferenceNumber.Right(9);

			if (entryHeader.EntryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				crlH2.VesselNameForeignTradeZoneNumber = entryHeader.ImportFTZNumber;
			}
			else
			{
				crlH2.VesselNameForeignTradeZoneNumber = entryHeader.ImportingVesselName.Left(20);
			}

			return crlH2;
		}

		public IEnumerable<MessageBlock> GenerateCRLHA()
		{
			foreach (IBillDetails billDetails in entryHeader.LowestBillDetails)
			{
				CRLHA crlHA = new CRLHA();
				crlHA.InbondNumber = billDetails.ITNumber;
				crlHA.MasterBillNumber = billDetails.MasterBillNumber;
				crlHA.HouseBillNumber = billDetails.HouseBillNumber;
				crlHA.SubHouseBillNumber = billDetails.SubHouseBillNumber;
				crlHA.Quantity = billDetails.PackageQuantity;
				crlHA.Unit = billDetails.PackageType;
				crlHA.ImmediateTransportationITDate = billDetails.ITDate;
				crlHA.IssuerCodeOfMasterBillNumber = billDetails.IssuerCodeOfMasterBillNumber;
				crlHA.IssuerCodeOfHouseBillNumber = billDetails.IssuerCodeOfHouseBillNumber;
				yield return crlHA;
			}
		}

		public CRLH5 GenerateCRLH5(ICargoReleaseCusEntryLine entryLine)
		{
			CRLH5 crlH5 = new CRLH5();

			crlH5.RecordControlNumber = entryLine.CL_LineNumber;
			crlH5.CountryOfOrigin = entryLine.CountryOfOrigin;
			crlH5.TariffNumber = entryLine.Tariff;
			crlH5.ManufacturerShipper = entryLine.ManufacturerSupplierCode;
			crlH5.LineItemUltimateConsignee = entryLine.UltimateConsigneeNumber;
			crlH5.LineItemValue = entryLine.CL_CustomsValue;

			return crlH5;
		}

		public IEnumerable<MessageBlock> GenerateEntryLine(ICargoReleaseCusEntryLine entryLine)
		{
			yield return GenerateCRLH5(entryLine);

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

		#region OGA

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

		#endregion
	}
}
