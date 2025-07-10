using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class UniversalDataObjectWriterHelper : Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper
	{
		public UniversalDataObjectWriterHelper(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.UnitedStates)
		{
		}

		protected override void UpdateOrganizationAddressCollectionCore(UniversalShipment.IOrganizationAddressCollectionParent parent, Customs.Business.MultiLineAddInfos.CusAddInfo cusAddInfo, IDataWritingManager writeManager)
		{
			AddInfoGroupOrganizationAddressCollectionUpdator.Update(parent, cusAddInfo, writeManager);
		}

		protected override Customs.DataTransfer.Universal.IAdditionalAddInfoGroupCollectionDataObjectWriter GetAdditionalAddInfoGroupCollectionSupportForCore(BusinessObject bizObj, IDataWritingManager writeManager)
		{
			var fda = bizObj as FDA;
			if (fda != null)
			{
				return new AdditionalAddInfoGroupCollectionDataObjectWriterForFDARelatedData(this, fda);
			}
			else
			{
				var invoiceLine = bizObj as JobComInvoiceLine;
				if (invoiceLine != null)
				{
					var declaration = invoiceLine.Declaration;
					if (declaration != null && declaration.IsInwardBondedWarehousingEnabled)
					{
						return new AdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine(invoiceLine);
					}
				}
			}
			return null;
		}

		protected override IEnumerable<UniversalCustoms.CustomsReference> GetAdditionalCustomsReferenceDataForCore(BusinessObject bizObj, IDataWritingManager writeManager, string dataContext)
		{
			var cusRefDataCollection = new List<UniversalCustoms.CustomsReference>();
			var entry = bizObj as CusEntryHeader;
			if (entry != null && entry.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.ACECargoRelease)
			{
				var referenceIdentifierQualifierCodeList = entry.Factory.GetCachedValue<ReferenceIdentifierQualifierCodeList>();
				var cusCodeDataTypeList = entry.Factory.GetCachedValue<CusCodeDataTypeList>();
				ErrorsRecordCollection errorRecords = entry.Declaration.EntryStatusesAndErrors.ACECargoRelReferenceData;
				foreach (ErrorsRecord record in errorRecords)
				{
					var data = new UniversalCustoms.CustomsReference();
					data.Type = UniversalDataBuss.DataObjects.Core.ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair>(CusCodeDataTypeList.Codes.ACECargoReleaseReferenceData, cusCodeDataTypeList);
					data.SubType = UniversalDataBuss.DataObjects.Core.ListHelper.GetWithDescription<UniversalShipment.CodeDescriptionPair35Char>(record.ErrorMessageIdentifier, referenceIdentifierQualifierCodeList);
					data.Reference = record.StatusDate.IsValid ? (CargoWise.Types.ZString)(record.StatusDate.ToLongTimeString() + "|" + record.NarrativeMessage) : record.NarrativeMessage;
					data.IsOverridden = false;
					data.Order = 0;

					cusRefDataCollection.Add(data);
				}
			}

			return cusRefDataCollection.Count != 0 ? cusRefDataCollection : null;
		}
	}
}
