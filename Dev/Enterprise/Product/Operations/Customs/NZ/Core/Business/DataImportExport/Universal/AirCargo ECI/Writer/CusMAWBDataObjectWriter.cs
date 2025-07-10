using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class CusMAWBDataObjectWriter : DataTransfer.Universal.AirManifest.CusMAWBDataObjectWriter<CusMAWB, CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusMAWBDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AirManifestDataObjectWriterHelper CreateNewHVLVAirDataObjectWriterHelper(CusMAWB mawbBO)
		{
			return new AirManifestDataObjectWriterHelper(mawbBO);
		}

		protected override DataTransfer.Universal.AirManifest.CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper mawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, mawbHelper);
		}

		protected override string ResponsiblePartyAddressType
		{
			get { return AddressTypes.ShippingLine; }
		}

		protected override void PopulateCountrySpecificData(CusMAWB mawbBO, Shipment mawbData, AirManifestDataObjectWriterHelper mawbHelper, bool keepExistingData)
		{
			base.PopulateCountrySpecificData(mawbBO, mawbData, mawbHelper, keepExistingData);
			mawbData.MessageStatus = PopulateValue(mawbData.ConsolidatedCargoStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(mawbBO.CM_CustomsStatus, mawbBO.Lookups.CustomsStatus_List)); // TODO: Check with Ben whether this is a Message Status or not
			mawbData.HasProhibitedPackaging = PopulateValue(mawbData.HasProhibitedPackaging, keepExistingData, () => mawbBO.CM_HasProhibitedPackaging);
			mawbData.IsFinalManifest = PopulateValue(mawbData.IsFinalManifest, keepExistingData, () => mawbBO.CM_IsFinalManifest);

			if (mawbBO.IsTSWWriteOff)
			{
				mawbData.SetOrganizationAddressCollection(() =>
				{
					var goodsLocation = new OrganizationDataObjectWriter(writeManager, nameof(DocAddressType.GoodsLocation)).GetDataObject(mawbBO.GoodsLocation);
					return mawbData.OrganizationAddressCollection.MergeCollection(new[] { goodsLocation }, keepExistingData, UniversalDataObjectWriterHelper.IsOrganizationAddressTypeMatched);
				});
			}
		}

		protected override bool SupportFirstArrival
		{
			get { return false; }
		}
	}
}
