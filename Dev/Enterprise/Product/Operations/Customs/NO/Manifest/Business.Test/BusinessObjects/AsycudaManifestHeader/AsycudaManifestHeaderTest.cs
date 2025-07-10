using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeader))]
sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
{
	public void TestZZValidationHelperType() => AssertType<ZZDatabaseValidationHelper>(ManifestHeader.ZZValidationHelper);

	public void TestSetDefaultValuesForRepresentativeWhenNewForwarderManifest()
	{
		var originalOrgPK = ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK;
		var branchProxy = Factory.NewWithValidTestData<OrgHeader>();

		using (GlbCompany.TemporaryLoginInNewCompanyForCountry("AU"))
		{
			CombineAssertions(() =>
			{
				ManifestHeader.AMA_ManifestType = NOManifestTypes.Codes.DMO;
				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("OrgPK should remain unchanged when AMA_ApplicationCode is Consolidator", originalOrgPK, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);

				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.BreakBulk;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				AssertEquals("OrgPK should be set to originalOrgPK variable defined above as Application code is not consolidator", originalOrgPK, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);

				ManifestHeader.MasterBill.ABL_OA_Forwarder = ZGuid.Empty;
				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = branchProxy.PK;
				AssertEquals("OrgPK should be set to Company OrgProxy when branchProxy is empty and companyProxy has a value", GlbCompany.CurrentCompany.GC_OH_OrgProxy, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);

				ManifestHeader.MasterBill.ABL_OA_Forwarder = ZGuid.Empty;
				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				var branchProxy2 = Factory.NewWithValidTestData<OrgHeader>();
				ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK = branchProxy.PK;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy2.PK;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				AssertEquals("OrgPK should be set to branchProxy.PK because the field is not empty", branchProxy.PK, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
				ManifestHeader.MasterBill.ABL_OA_Forwarder = ZGuid.Empty;
				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				var currentOrgPK = ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK;
				AssertEquals("OrgPK should remain unchanged when both branch and company proxies are empty", currentOrgPK, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);

				ManifestHeader.AMA_ManifestType = "GLC";
				ManifestHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				AssertEquals("OrgPK should remain unchanged when AMA_ApplicationCode is Consolidator and ManifestType is not DMO", currentOrgPK, ManifestHeader.MasterBill.ABL_OA_Forwarder_ZAddress.OrgPK);
			});
		}
	}

	public void TestConveyanceNationality_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_RN_NKConveyanceNationality)
			.WithCaption("Nationality")
			.WithFullDescription("Nationality/Country code for Means of transport at Border Crossing."));

	public void TestDateAtCustomsOffice_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_DateAtCustomsOffice)
			.WithCaption("ETA Cust. Office")
			.WithFullDescription("Estimated Time of Arrival at Customs Office/Border Crossing. Update the time as needed when changes occur."));

	public void TestDriverCommunicationId_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_DriverCommunicationId)
			.WithCaption("Phone NO./E-Mail")
			.WithFullDescription("Telephone number (or mail address) of operator/driver of Means of transport at Border Crossing.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestDriverCommunicationId_Persistence() => PersistenceTestHelper.AssertValueIsPersistedInGenAddOnColumn<ZString>(Factory, ManifestHeader.AMA_DriverCommunicationIdInfo, "DMODriverCommunicationId", "42");

	public void TestDriverName_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_DriverName)
			.WithCaption("Operator/Driver")
			.WithFullDescription("Name of operator/driver of Means of transport at Border Crossing.")
			.WithAttribute<MaxLengthAttribute>(x => x.MaxLength == 70));

	public void TestDriverName_Persistence() => PersistenceTestHelper.AssertValueIsPersistedInGenAddOnColumn<ZString>(Factory, ManifestHeader.AMA_DriverNameInfo, "DMODriverName", "Testy Testsson");

	public void TestScheduledDateOfAddCustOff_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_ScheduledDateOfAddCustOff)
			.WithCaption("Sched. Arr Cust.Off")
			.WithFullDescription("Scheduled Time of Arrival at Customs Office/Border Crossing (set at first submission).")
			.WithAttribute<ReadOnlyAttribute>(x => x.IsReadOnly));

	public void TestScheduledDateOfAddCustOff_Persistence() => PersistenceTestHelper.AssertValueIsPersistedInGenAddOnColumn(Factory, ManifestHeader.AMA_ScheduledDateOfAddCustOffInfo, "DMOScheduledDateOfAddCustOff", ZDateTime.BrettsBirthday);

	public void TestTransportMeans_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(x => x.AMA_TransportMeans)
			.WithList($"{nameof(AsycudaManifestHeader.Lookups)}.{nameof(AsycudaManifestHeaderLookups.TransportMeansCodeList)}")
			.WithCaption("Type of Means")
			.WithFullDescription("Type of Means of transport at Border Crossing."));

	public void TestVehicleRegistration_CaptionsWhenRoad()
	{
		ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Road;
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(ManifestHeader.AMA_VehicleRegistrationInfo, TransportTypeList.Codes.Road, new DataBoundBusinessObject(ManifestHeader));
		AssertNotNull("[PRE-CONDITION] resourceStringData should not be null", resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("Transport ID", resourceStringData.Caption);
			AssertEquals("The license plate for the truck/active means of transport at border crossing.", resourceStringData.FullDescription);
		});
	}

	public void TestVehicleRegistration_CaptionsWhenAir()
	{
		ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Air;
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(ManifestHeader.AMA_VehicleRegistrationInfo, TransportTypeList.Codes.Air, new DataBoundBusinessObject(ManifestHeader));
		AssertNotNull("[PRE-CONDITION] resourceStringData should not be null", resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("Aircraft Reg. No.", resourceStringData.Caption);
			AssertEquals("The aircraft registration number - as in tail number.", resourceStringData.FullDescription);
		});
	}

	public void TestVehicleRegistration_CaptionsWhenSea()
	{
		ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(ManifestHeader.AMA_VehicleRegistrationInfo, TransportTypeList.Codes.Sea, new DataBoundBusinessObject(ManifestHeader));
		AssertNotNull("[PRE-CONDITION] resourceStringData should not be null", resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("IMO Ship No.", resourceStringData.Caption);
			AssertEquals("The unique IMO registration number of the vessel.", resourceStringData.FullDescription);
		});
	}

	public void TestVehicleRegistration_CaptionsWhenRail()
	{
		ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Rail;
		var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(ManifestHeader.AMA_VehicleRegistrationInfo, TransportTypeList.Codes.Rail, new DataBoundBusinessObject(ManifestHeader));
		AssertNotNull("[PRE-CONDITION] resourceStringData should not be null", resourceStringData);
		CombineAssertions(() =>
		{
			AssertEquals("Train No.", resourceStringData.Caption);
			AssertEquals("The train number.", resourceStringData.FullDescription);
		});
	}

	public void TestMovementReferenceNumber_Attributes() => CombineAssertions(() =>
		AssertEntity<AsycudaManifestHeader>()
			.HasProperty(h => h.MovementReferenceNumber)
			.WithAttribute<MaxLengthAttribute>(l => l.MaxLength == 35));

	public void TestBills() => AssertType<ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>>(ManifestHeader.Bills);

	public void TestLookups() => AssertType<AsycudaManifestHeaderLookups>(ManifestHeader.Lookups);

	public void TestValidation() => AssertType<AsycudaManifestHeaderValidation>(ManifestHeader.Validation);

	public void TestMovementReferenceNumber_SaveAndUpdate() => CombineAssertions(() =>
	{
		AssertEquals("[Pre-Condition] No Entry", ZString.Empty, ManifestHeader.MovementReferenceNumber);

		ManifestHeader.MovementReferenceNumber = "1234";
		var mrnEntryNumber = GetMrnEntryNumber();
		AssertEquals("MRN Entry Number first time assignment", "1234", mrnEntryNumber.CE_EntryNum);

		ManifestHeader.MovementReferenceNumber = "4567";
		mrnEntryNumber = GetMrnEntryNumber();
		AssertEquals("MRN Entry Number on update", "4567", mrnEntryNumber.CE_EntryNum);

		CusEntryNumber GetMrnEntryNumber()
			=> CusEntryNumber.Load(ManifestHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, ManifestHeader.AMA_RN_NKCountry, true);
	});

	public void TestMovementReferenceNumber_Load()
	{
		var entryNumber = CusEntryNumber.LoadOrCreate(ManifestHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, ManifestHeader.AMA_RN_NKCountry);
		entryNumber.CE_EntryNum = "NEW123";
		AssertEquals("MRN", "NEW123", ManifestHeader.MovementReferenceNumber);
	}

	public void TestIMovementReferenceNumberSupporter_MovementReferenceNumber() => CombineAssertions(() =>
	{
		var movementReferenceNumberSupporter = (IMovementReferenceNumberSupporter)ManifestHeader;

		AssertEquals("[Pre-Condition] No Entry", ZString.Empty, movementReferenceNumberSupporter.MovementReferenceNumber);

		ManifestHeader.MovementReferenceNumber = "123";
		AssertEquals($"{nameof(IMovementReferenceNumberSupporter.MovementReferenceNumber)} Get", "123", movementReferenceNumberSupporter.MovementReferenceNumber);

		movementReferenceNumberSupporter.MovementReferenceNumber = "666";
		AssertEquals($"{nameof(IMovementReferenceNumberSupporter.MovementReferenceNumber)} Value on Update", "666", ManifestHeader.MovementReferenceNumber);
	});

	public void TestTransportMode() => CombineAssertions(() =>
	{
		ITransportModeProvider transportModeProvider = ManifestHeader;
		AssertEquals("[Pre-Condition] When not set", ZString.Empty, transportModeProvider.TransportMode);

		ManifestHeader.AMA_TransportMode = TransportTypeList.Codes.Sea;
		AssertEquals("When set to SEA", TransportTypeList.Codes.Sea, transportModeProvider.TransportMode);
	});

	public void TestSynchroniser()
	{
		var consol = Factory.New<ForwardingConsol>();
		ManifestHeader.SetParent(consol);

		AssertType<AsycudaManifestHeaderSynchroniser>(ManifestHeader.Synchroniser);
	}

	public AsycudaManifestHeader ManifestHeader => manifestHeader ??= CreateBusinessObject(Factory);
	AsycudaManifestHeader manifestHeader;

	protected override BusinessObject GetNewBusinessObject() => CreateBusinessObject(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var header = CreateBusinessObject(factory);
		header.SuspendCheckBusinessObjectType();
		return header;
	}

	static AsycudaManifestHeader CreateBusinessObject(BusinessObjectFactory factory) => factory.New<AsycudaManifestHeader>();
}
