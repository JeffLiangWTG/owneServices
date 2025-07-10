using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : DataObjectWriter<CusEntryHeader, UniversalCustoms.EntryHeader>
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = Argument.NotNull(helper, "UniversalDataObjectWriterHelper helper");
		}

		protected readonly UniversalDataObjectWriterHelper helper;

		protected sealed override UniversalCustoms.EntryHeader PopulateDataObject(CusEntryHeader entryHeaderBO)
		{
			var entryHeaderData = new UniversalCustoms.EntryHeader()
			{
				EntryInstructionLink = helper.GetAllocatedEntryInstructionLink(entryHeaderBO.CH_CEI_Instruction),
				Type = ListHelper.GetWithDescription<EntryType>(entryHeaderBO.CH_MessageType, entryHeaderBO.Lookups.CH_MessageTypeList),
				MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(entryHeaderBO.CH_Status, entryHeaderBO.Lookups.MessageStatusList),
				EntryStatus = ListHelper.GetWithDescription<EntryStatus>(entryHeaderBO.CH_EntryStatus, entryHeaderBO.Lookups.CH_EntryStatusList),
				Reference = entryHeaderBO.CH_BGMReference,
				TotalAmountPaid = entryHeaderBO.CH_TotalPaid,
				//TODO - CH_HighestLineNumber
				EntrySubmittedDate = entryHeaderBO.CH_EntrySubmittedDate,
				EntryReleaseDate = entryHeaderBO.CH_EntryReleaseDate,
				BondValidToDate = entryHeaderBO.CH_BondValidToDate,
				EntryLineCollection = ProcessCollection(helper.Load<CusEntryLine>(new ZQuery(CusEntryLineSchema.CL_CH, entryHeaderBO.PK)), GetNewCustomsEntryLineDataObjectWriter())
			};

			PopulateCusEntryHeaderChargeData(entryHeaderBO, entryHeaderData);
			PopulateCusEntryNumberData(entryHeaderBO, entryHeaderData);
			PopulateRelatedCusEntryHeaderData(entryHeaderBO, entryHeaderData);

			PopulateAddInfo(entryHeaderBO, entryHeaderData);
			PopulateCusAddInfoData(entryHeaderBO, entryHeaderData);
			PopulateCusEntrySupportingInfoData(entryHeaderBO, entryHeaderData);
			PopulateCustomsEntryHeaderPaymentInformationData(entryHeaderBO, entryHeaderData);

			entryHeaderData.CustomsReferenceCollection = CustomsReferenceCollectionCreator.CreateCollection(helper, entryHeaderBO, writeManager);

			return entryHeaderData;
		}

		protected virtual void PopulateCusAddInfoData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			entryHeaderData.AddInfoGroupCollection = AddInfoGroupCollectionCreator.CreateCollection(helper, entryHeaderBO, writeManager);
		}

		protected virtual void PopulateAddInfo(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			entryHeaderData.AddInfoCollection = AddInfoCollectionCreator.CreateCollection(entryHeaderBO, CusEntryHeaderSchema.CH_AddInfo);

			if (!entryHeaderBO.CH_CustomsMessageRemarks.IsEmpty)
			{
				entryHeaderData.AddInfoCollection.Add(new AddInfo() { Key = "AmendmentReason", Value = entryHeaderBO.CH_CustomsMessageRemarks });
			}
		}

		protected virtual CustomsEntryLineDataObjectWriter GetNewCustomsEntryLineDataObjectWriter()
		{
			return new CustomsEntryLineDataObjectWriter(writeManager, helper);
		}

		void PopulateRelatedCusEntryHeaderData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var query = new ZQuery(CusEntryHeaderSchema.CH_CH_PrimeEntry, entryHeaderBO.PK);
			query.AddToFilter(CusEntryHeaderSchema.CH_ClusterKey, entryHeaderBO.CH_ClusterKey);
			query.AddToFilter(CusEntryHeaderSchema.CH_JE, entryHeaderBO.CH_JE);
			var relatedEntryHeaders = helper.Load<CusEntryHeader>(query);
			entryHeaderData.RelatedEntryHeaderCollection = ProcessCollection(relatedEntryHeaders, GetNewCustomsEntryHeaderDataObjectWriter(), true);
		}

		protected virtual CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected virtual void PopulateCusEntrySupportingInfoData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
		}

		protected virtual bool ShouldPopulatePaymentInformationData => false;

		void PopulateCustomsEntryHeaderPaymentInformationData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			if (ShouldPopulatePaymentInformationData)
			{
				entryHeaderData.PaymentInformationCollection = ProcessCollection(entryHeaderBO.EntryPayInfos, new CusEntryPayInfoDataObjectWriter(writeManager, helper));
			}
		}

		void PopulateCusEntryNumberData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var entryNumbers = CusEntryNumber.Load(entryHeaderBO);
			entryHeaderData.EntryNumberCollection = ProcessCollection(entryNumbers, GetNewCustomsEntryNumberDataObjectWriter());
		}

		protected virtual CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriter()
		{
			return new CustomsEntryNumberDataObjectWriter(writeManager, helper);
		}

		void PopulateCusEntryHeaderChargeData(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var entryHeaderChargeBOs = entryHeaderBO.Charges;
			if (entryHeaderChargeBOs.Count > 0)
			{
				var entryHeaderChargeCollection = new List<UniversalCustoms.EntryHeaderCharge>(entryHeaderChargeBOs.Count);
				foreach (CusEntryHeaderCharges chargeBO in entryHeaderChargeBOs)
				{
					entryHeaderChargeCollection.Add(new UniversalCustoms.EntryHeaderCharge()
					{
						Amount = chargeBO.C1_ChargeAmount,
						Type = ListHelper.GetWithDescription<CodeDescriptionPair>(chargeBO.C1_ChargeType, entryHeaderBO.EntryChargeTypeList)
					});
				}
				entryHeaderData.EntryHeaderChargeCollection = entryHeaderChargeCollection;
			}
		}
	}
}
