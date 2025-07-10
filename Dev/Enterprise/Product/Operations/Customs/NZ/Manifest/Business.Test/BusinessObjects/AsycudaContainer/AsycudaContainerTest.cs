using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaContainer))]
	sealed class AsycudaContainerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaContainer()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.NZManifest.IAsycudaContainer>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaContainer>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(container.Header);
		}

		public void TestSendMCDInformation()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("SendMCDInformation - default value", false, container.SendMCDInformation);
				AssertEquals("SendMCDInformation is not readonly as no children answers are true", false, container.SendMCDInformationInfo.ReadOnly);
				AssertSendMCDInformationIsSetCorrectly(container, container.HasMPIQDInfo);
				AssertSendMCDInformationIsSetCorrectly(container, container.IsContainerCleanInfo);
				AssertSendMCDInformationIsSetCorrectly(container, container.IsPackingContaminatedInfo);
				AssertSendMCDInformationIsSetCorrectly(container, container.IsWoodPackingUsedInfo);
				AssertSendMCDInformationIsSetCorrectly(container, container.IsWoodPackingTreatedInfo);
				AssertSendMCDInformationIsSetCorrectly(container, container.HasWoodPackingTreatmentCertInfo);
			});
		}

		public void TestDeliveryDestination()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals("DeliveryDestination - default value", ZGuid.Empty, container.DeliveryDestination);
			AssertEquals("DeliveryDestination_ZAddress.OrgPk - default value", ZGuid.Empty, container.DeliveryDestinationPartyDocAddress.OrganisationPK);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			container.DeliveryDestinationOrgPK = orgHeader.PK;
			container.DeliveryDestination = orgHeader.MainAddress.PK;
			AssertEquals("Setting the DeliveryDestinationPrgPk loads the correct OrgHeader", orgHeader, container.DeliveryDestinationPartyDocAddress.Organisation);
			AssertEquals("The DeliveryDestination address PK is returned", orgHeader.MainAddress.PK, container.DeliveryDestination);
			Factory.Save();
			var jobDocAddresses = Factory.Load<JobDocAddress>(new ZQuery(ZArchitecture.Schema.JobDocAddressSchema.E2_ParentID, container.PK));
			AssertEquals(1, jobDocAddresses.Length);
			var jobDocAddress = jobDocAddresses[0];
			AssertEquals("CLD", jobDocAddress.E2_AddressType);
			AssertEquals(orgHeader, jobDocAddress.Organisation);
			AssertEquals(orgHeader.MainAddress, jobDocAddress.Address);
		}

		public void TestStuffingLocation()
		{
			var container = (AsycudaContainer)GetNewBusinessObject();
			AssertEquals("PackLocation - default value", ZGuid.Empty, container.ACN_OA_PackLocation);
			AssertEquals("PackLocation_ZAddress.OrgPk - default value", ZGuid.Empty, container.ACN_OA_PackLocation_ZAddress.OrgPK);
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			container.PackLocationOrgPK = orgHeader.PK;
			container.ACN_OA_PackLocation = orgHeader.MainAddress.PK;
			AssertEquals("Setting the PackLocationOrgPk loads the correct OrgHeader", orgHeader, container.ACN_OA_PackLocation_ZAddress.OrgHeader);
			AssertEquals("The PackLocation address PK is returned", orgHeader.MainAddress.PK, container.ACN_OA_PackLocation);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, NZManifestTypes.Codes.ICR, ApplicationCodeTypeList.Codes.ShippingLine);
			header.SuspendCheckBusinessObjectType();
			return header.Containers.AddNew();
		}

		void AssertSendMCDInformationIsSetCorrectly(AsycudaContainer container, ZPropertyInfo property)
		{
			property.Value = ZBool.True;
			var propertyName = property.Name;
			AssertEquals($"SendMCDInformation should be true as {propertyName} is true", true, container.SendMCDInformation);
			AssertEquals($"SendMCDInformation should be readonly as {propertyName} is true", true, container.SendMCDInformationInfo.ReadOnly);
			property.Value = ZBool.False;
			container.SendMCDInformation = ZBool.False;
		}
	}
}
