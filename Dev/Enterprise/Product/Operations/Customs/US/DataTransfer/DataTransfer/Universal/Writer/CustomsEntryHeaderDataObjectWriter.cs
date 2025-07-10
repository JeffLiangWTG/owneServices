using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected new UniversalDataObjectWriterHelper helper
		{
			get { return (UniversalDataObjectWriterHelper)base.helper; }
		}

		protected override void PopulateCusAddInfoData(Customs.Business.CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var result = AddInfoGroupCollectionCreator.CreateCollection(helper, entryHeaderBO, writeManager);
			if (result != null)
			{
				entryHeaderData.AddInfoGroupCollection.AddRange(result);
			}
		}

		protected override void PopulateAddInfo(Customs.Business.CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			base.PopulateAddInfo(entryHeaderBO, entryHeaderData);

			var entry = (CusEntryHeader)entryHeaderBO;
			PopulatePSCExplanation(entry, entryHeaderData);
			PopulateLiquidationDetails(entry, entryHeaderData);
		}

		void PopulatePSCExplanation(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var pscExplanation = entryHeaderBO.PSCExplanation;
			if (pscExplanation != null)
			{
				var collection = entryHeaderData.AddInfoGroupCollection ?? new List<UniversalCustoms.AddInfoGroup>();
				collection.Add(new UniversalCustoms.AddInfoGroup()
				{
					Type = new UniversalShipment.CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.PSCReasonCodes, Description = CusCodeDataTypeList.Descriptions.PSCReasonCodes },
					AddInfoCollection = new List<UniversalShipment.AddInfo>(new[] { new UniversalShipment.AddInfo() { Key = AutoUSImportMessageSendingAction.Schema.US_PSCExplanation.Substring(3), Value = pscExplanation.B7_AddInfoData } })
				});
				entryHeaderData.AddInfoGroupCollection = collection;
			}
		}

		void PopulateLiquidationDetails(CusEntryHeader entryHeaderBO, UniversalCustoms.EntryHeader entryHeaderData)
		{
			var liquidationDate = entryHeaderBO.LiquidationDate;
			var liquidationType = entryHeaderBO.LiquidationType;

			if (!liquidationDate.IsEmpty || !liquidationType.IsEmpty)
			{
				var collection = entryHeaderData.AddInfoCollection;
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.LiquidationDate, liquidationDate);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.LiquidationType, liquidationType);
			}
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}
	}
}
