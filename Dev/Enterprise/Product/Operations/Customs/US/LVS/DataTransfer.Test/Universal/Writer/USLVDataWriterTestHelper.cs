using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.US.LVS.DataTransfer.Test
{
	public class USLVDataWriterTestHelper : OrganizationAddressTestHelper
	{
		protected void AssertOrganizationAddressContents(IOrganizationAddressCollectionParent shipment, string addressType, OrgAddress expectedAddress)
		{
			var addressData = shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == addressType);
			AssertNotNull(addressData);
			var orgHeader = Factory.Load<OrgHeader>(expectedAddress.OA_OH);
			AssertNotNull(orgHeader);
			AssertEquals(orgHeader.OH_Code, addressData.OrganizationCode.GetValueOrDefault());
		}

		protected void AssertDate(ZString message, Shipment shipment, DateType dataType, ZDateTime? expectedValue)
		{
			var data = shipment.DateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == dataType);
			AssertNotNull(message, data);
			AssertEquals(message, expectedValue, data.Value);
		}

		protected void AssertMasterAdditionalBill(ZString message, CusUSLVClearance clearance, Shipment shipment, ZString billNumber, WayBillType expectedValue)
		{
			var additionalBill = shipment.AdditionalBillCollection.FirstOrDefault(x => x.BillNumber.HasValue && x.BillNumber.Value == billNumber);
			AssertEquals(message, expectedValue.Code, additionalBill.BillType.Code);
			AssertEquals(message, expectedValue.Description, additionalBill.BillType.Description);
			AssertAddInfo(message, additionalBill.AddInfoCollection, LVSConstants.AddInfoConstants.UI_NKBillIssuerSCAC, clearance.ULH_MasterBillIssuerSCAC);
		}

		protected void AssertAddInfo(ZString message, List<UAddInfo> collection, ZString addInfoName, ZString expectedValue)
		{
			var addinfo = collection.FirstOrDefault(x => x.Key.GetValueOrDefault() == addInfoName);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedValue, addinfo.Value);
		}

		protected void AssertNullAddInfo(ZString message, List<UAddInfo> collection, ZString addInfoName)
		{
			var addinfo = collection.FirstOrDefault(x => x.Key.GetValueOrDefault() == addInfoName);
			AssertNull(message, addinfo);
		}
	}
}
