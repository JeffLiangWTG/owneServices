using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Testing
{
	public class RatingXSDTestHelper : RatingTestCase
	{
		#region XSD

		public Xsd.Organisation ConsigneeXSD
		{
			get
			{
				Xsd.Organisation consigneeXSD = new Xsd.Organisation();
				consigneeXSD.EDICode = Consignee.OH_Code;
				consigneeXSD.OrganisationDetails.Addresses.Add(ConsigneeAddress1XSD);
				consigneeXSD.OrganisationDetails.Addresses.Add(ConsigneeAddress2XSD);
				consigneeXSD.OrganisationDetails.Name = Consignee.OH_FullName;
				consigneeXSD.OrganisationDetails.Location.Value = Consignee.OH_RL_NKClosestPort;

				return consigneeXSD;
			}
		}

		Xsd.OrgAddress ConsigneeAddress1XSD
		{
			get { return SetupOrgAddressXSDForOrganisation(ConsigneeAddress1, Xsd.AddressCapabilityAddressType.PAD, 1); }
		}

		Xsd.OrgAddress ConsigneeAddress2XSD
		{
			get { return SetupOrgAddressXSDForOrganisation(ConsigneeAddress2, Xsd.AddressCapabilityAddressType.PAD, 2); }
		}

		Xsd.OrgAddress SetupOrgAddressXSDForOrganisation(OrgAddress orgAddress, Xsd.AddressCapabilityAddressType type, int sequence)
		{
			Xsd.OrgAddress orgAddressXSD = new Xsd.OrgAddress();
			orgAddressXSD.AddressCode = orgAddress.OA_Code;
			orgAddressXSD.AddressLine1 = orgAddress.OA_Address1;
			orgAddressXSD.Sequence = sequence;
			Xsd.AddressCapability capability = orgAddressXSD.AddressCapabilities.AddNew();
			capability.AddressType = type;

			return orgAddressXSD;
		}

		public Xsd.RateEntry RateEntryXSD
		{
			get
			{
				Xsd.RateEntry rateEntry = new Xsd.RateEntry();
				rateEntry.Category = "AIR";
				rateEntry.Mode = "LSE";
				rateEntry.Origin = "AUSYD";
				rateEntry.Destination = "NZAKL";
				rateEntry.Frequency = 2;
				rateEntry.CommodityCode = "GEN";
				rateEntry.StartDate = new ZDate(2006, 1, 1);
				rateEntry.Consignee = ConsigneeXSD;
				rateEntry.Currency = Core.Constants.CurrencyCodes.Australia;
				rateEntry.FrequencyUnit = "Daily";
				rateEntry.IsCrossTrade = Xsd.TrueFalse.@true;

				return rateEntry;
			}
		}

		OrgAddress fConsigneeAddress1;
		OrgAddress ConsigneeAddress1
		{
			get
			{
				if (fConsigneeAddress1 == null)
				{
					fConsigneeAddress1 = Consignee.Addresses.AddNew();
					fConsigneeAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
					fConsigneeAddress1.OA_Address1 = "1123 Bourke Street";
					fConsigneeAddress1.OA_Code = "ConsigneeAddress1";
				}
				return fConsigneeAddress1;
			}
		}

		OrgAddress fConsigneeAddress2;
		OrgAddress ConsigneeAddress2
		{
			get
			{
				if (fConsigneeAddress2 == null)
				{
					fConsigneeAddress2 = Consignee.Addresses.AddNew();
					fConsigneeAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.PickupAndDelivery);
					fConsigneeAddress2.OA_Address1 = "1456 Bourke Street";
					fConsigneeAddress2.OA_Code = "ConsigneeAddress2";
				}
				return fConsigneeAddress2;
			}
		}

		#endregion
	}

	abstract class FullRatingValueObjectDataAdapterTest<TBusinessObject> : ValueObjectDataAdapterTest<TBusinessObject, Xsd.Rate>
		where TBusinessObject : RatingHeader
	{
		public void TestImportFromValueObject()
		{
			TestImportCore();
		}
		protected abstract void TestImportCore();

		public void TestExpectedRateType()
		{
			TestExpectedRateTypeCore();
		}
		protected abstract void TestExpectedRateTypeCore();

		public void TestShouldCheckOwnerIsSpecified()
		{
			TestShouldCheckOwnerIsSpecifiedCore();
		}
		protected abstract void TestShouldCheckOwnerIsSpecifiedCore();

		public virtual void TestImportRateInWrongModule()
		{
			Xsd.Rate rateXSD = new Xsd.Rate();
			rateXSD.RateType = OtherRateTypeForErrorTesitng;
			rateXSD.Owner = TestHelper.ConsigneeXSD;

			NotificationBuffer buffer = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, buffer);

			GetDataAdapter.CreateOrUpdateFromValueObject(rateXSD, context);

			AssertEquals("notification has errors", true, buffer.HasErrors);

			AssertEquals("buffer should have this error message", true, TestHelper.AssertContainsErrorMesg(ExpectedInvalidModuleMesg, buffer));
		}

		#region Implementation

		protected abstract ZString ExpectedRateType { get; }
		protected abstract bool RateShouldCheckOwnerIsSpecified { get; }
		protected abstract ZString OtherRateTypeForErrorTesitng { get; }
		protected abstract FullRatingValueObjectDataAdapter<TBusinessObject> GetDataAdapter { get; }
		protected abstract ZString ExpectedInvalidModuleMesg { get; }

		protected override ValueObjectDataAdapter<TBusinessObject, Xsd.Rate> GetNewBizObjXmlDataAdapter()
		{
			return GetDataAdapter;
		}

		protected override bool IsExportToValueObjectSupported
		{
			get { return true; }
		}

		protected override bool IsExportToCollectionSupported
		{
			get { return true; }
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Rates"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Rate"; }
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return null;
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			return null;
		}

		#endregion

		#region Containers

		protected ContainerFactory Containers
		{
			get { return fContainers ?? (fContainers = new ContainerFactory(Factory)); }
		}

		ContainerFactory fContainers;

		protected class ContainerFactory
		{
			public ContainerFactory(BusinessObjectFactory factory)
			{
				this.Factory = factory;
			}

			public RefContainer this[string container]
			{
				get { return Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container); }
			}

			readonly BusinessObjectFactory Factory;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper = new RatingXSDTestHelper();
			TestHelper.Consignee.Factory.Save();
		}

		protected RatingXSDTestHelper TestHelper;
	}
}
