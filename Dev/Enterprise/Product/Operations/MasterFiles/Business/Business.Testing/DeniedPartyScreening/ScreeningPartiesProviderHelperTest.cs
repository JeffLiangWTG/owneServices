using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ScreeningPartiesProviderHelperTest : TestCaseWithFactory
	{
		public void TestAssertAndEnsureSingletonForIScreeningPartyProviderHelper()
		{
			var helper = ObjectFactory.Get<IScreeningPartiesProviderHelper>();
			AssertEquals(typeof(ScreeningPartiesProviderHelper), helper.GetType());
			var helperFromSecondCall = ObjectFactory.Get<IScreeningPartiesProviderHelper>();
			AssertEquals(helper, helperFromSecondCall);
			var helperType = typeof(ScreeningPartiesProviderHelper);

			var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
			Assert("Singleton implementation should be stateless", helperType.GetFields(bindingFlags).Length == 0);
			Assert("Singleton implementation should be stateless", helperType.GetProperties(bindingFlags).Length == 0);
		}

		public void TestGetJobInvoicingScreeningParties_ChargesNullOrEmpty()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var helper = new ScreeningPartiesProviderHelper();
			var charges = Array.Empty<JobCharge>();
			var screenParties = helper.GetJobInvoicingScreeningParties((IJobHeaderParent)shipment, charges);
			Assert(charges.IsNullOrEmpty());
			Assert(screenParties != null && screenParties.Length == 0);
		}

		public void TestGetJobInvoicingScreeningParties_ParentNotBizo()
		{
			var parent = new DummyIJobHeaderParent();
			var helper = new ScreeningPartiesProviderHelper();
			var charges = new[] { Factory.NewWithValidTestData<JobCharge>() };
			var screenParties = helper.GetJobInvoicingScreeningParties(parent, charges);

			Assert(!charges.IsNullOrEmpty());
			Assert(screenParties != null && screenParties.Length == 0);
		}

		public void TestGetJobInvoicingScreeningParties_ParentNotShipment()
		{
			var parent = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingConsol)));
			var helper = new ScreeningPartiesProviderHelper();
			var charges = new[] { Factory.NewWithValidTestData<JobCharge>() };
			var screenParties = helper.GetJobInvoicingScreeningParties((IJobHeaderParent)parent, charges);

			Assert(!charges.IsNullOrEmpty());
			Assert(parent is BusinessObject);
			Assert(screenParties != null && screenParties.Length == 0);
		}

		public void TestGetJobInvoicingScreeningParties_ParentNotIShouldUpdateScreeningStatus()
		{
			var rowShipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment))) as INeedRow;
			var parent = new CommonShipmentForTest(Factory, rowShipment.Row);
			var helper = new ScreeningPartiesProviderHelper();
			var charges = new[] { Factory.NewWithValidTestData<JobCharge>() };
			var screenParties = helper.GetJobInvoicingScreeningParties(parent, charges);

			Assert(!charges.IsNullOrEmpty());
			Assert(parent is BusinessObject);
			Assert(parent is IForwardingShipment);
			Assert(screenParties != null && screenParties.Length == 0);
		}

		public void TestGetJobInvoicingScreeningParties()
		{
			var descriptionCreditor = "Creditor";
			var descriptionDebtor = "Debtor";
			var descriptionCreditorAndDebtor = "Creditor And Debtor";

			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IForwardingShipment)));
			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			var charge4 = Factory.NewWithValidTestData<JobCharge>();
			var charge5 = Factory.NewWithValidTestData<JobCharge>();
			var charge6 = Factory.NewWithValidTestData<JobCharge>();
			var charge7 = Factory.NewWithValidTestData<JobCharge>();

			var credit1 = Factory.NewWithValidTestData<OrgHeader>();
			var credit2 = Factory.NewWithValidTestData<OrgHeader>();
			var credit3 = Factory.NewWithValidTestData<OrgHeader>();
			var credit4 = Factory.NewWithValidTestData<OrgHeader>();

			var debtor1 = Factory.NewWithValidTestData<OrgHeader>();
			var debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			var debtor3 = Factory.NewWithValidTestData<OrgHeader>();
			var debtor4 = Factory.NewWithValidTestData<OrgHeader>();

			charge1.JR_OH_CostAccount = credit1.PK;
			charge1.JR_OH_SellAccount = debtor1.PK;

			charge2.JR_OH_CostAccount = credit2.PK;
			charge2.JR_OH_SellAccount = debtor2.PK;

			charge3.JR_OH_CostAccount = credit3.PK;
			charge3.JR_OH_SellAccount = debtor3.PK;

			charge4.JR_OH_CostAccount = credit4.PK;
			charge4.JR_OH_SellAccount = debtor4.PK;

			charge5.JR_OH_CostAccount = credit4.PK;
			charge5.JR_OH_SellAccount = debtor4.PK;

			charge6.JR_OH_CostAccount = credit3.PK;
			charge6.JR_OH_SellAccount = credit3.PK;

			charge7.JR_OH_CostAccount = debtor2.PK;
			charge7.JR_OH_SellAccount = credit3.PK;

			Factory.Save();

			var helper = new ScreeningPartiesProviderHelper();
			var screenParties = helper.GetJobInvoicingScreeningParties((IJobHeaderParent)shipment,
				new[] { charge1, charge2, charge3, charge4, charge5, charge6, charge7 }).ToList();

			CombineAssertions(() =>
			{
				AssertNotNull(screenParties);
				AssertEquals(8, screenParties.Count);
				Assert(screenParties.Exists(sc => sc.Key == credit1.PK && sc.Description == descriptionCreditor));
				Assert(screenParties.Exists(sc => sc.Key == debtor1.PK && sc.Description == descriptionDebtor));
				Assert(screenParties.Exists(sc => sc.Key == credit2.PK && sc.Description == descriptionCreditor));
				Assert(screenParties.Exists(sc => sc.Key == debtor2.PK && sc.Description == descriptionCreditorAndDebtor));
				Assert(screenParties.Exists(sc => sc.Key == credit3.PK && sc.Description == descriptionCreditorAndDebtor));
				Assert(screenParties.Exists(sc => sc.Key == debtor3.PK && sc.Description == descriptionDebtor));
				Assert(screenParties.Exists(sc => sc.Key == credit4.PK && sc.Description == descriptionCreditor));
				Assert(screenParties.Exists(sc => sc.Key == debtor4.PK && sc.Description == descriptionDebtor));
				Assert(screenParties.Select(x => x.Parent.PK).All(x => x == shipment.PK));
			});
		}

		#region Implementation

		class DummyIJobHeaderParent : IJobHeaderParent
		{
			public bool AllowInvoiceDeletion => throw new NotImplementedException();

			public bool IsDeleted => throw new NotImplementedException();

			public ZGuid PK => throw new NotImplementedException();

			public string TableName => throw new NotImplementedException();

			public bool IsInDatabase => throw new NotImplementedException();

			public BusinessObjectFactory Factory => throw new NotImplementedException();

			public string JobNumber => throw new NotImplementedException();

			public void OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleted(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleting(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}
		}

		class CommonShipmentForTest : BusinessObject, IForwardingShipment, IJobHeaderParent
		{
			public CommonShipmentForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
			public ZString JS_UniqueConsignRef { get; set; }
			public ZDateTime JS_A_BKD { get; set; }
			public ZDateTime JS_A_RCV { get; set; }
			public ZDecimal JS_ActualChargeable { get; set; }
			public ZDecimal JS_ActualVolume { get; set; }
			public ZDecimal JS_ActualWeight { get; set; }
			public ZString JS_AdditionalTerms { get; set; }
			public ZString JS_AWBServiceLevel { get; set; }
			public ZString JS_BookingReference { get; set; }
			public ZString JS_CartageWaybill { get; set; }
			public ZString JS_CFSReference { get; set; }
			public ZDateTime JS_ClientRequestedETA { get; set; }
			public ZString JS_ConsolReference { get; set; }
			public ZDecimal JS_DocumentedChargeable { get; set; }
			public ZDecimal JS_DocumentedLoadingMeters { get; set; }
			public ZDecimal JS_DocumentedVolume { get; set; }
			public ZDecimal JS_DocumentedWeight { get; set; }
			public ZDateTime JS_E_ARV { get; set; }
			public ZDateTime JS_E_DEP { get; set; }
			public ZString JS_InspectionTypeCode { get; set; }
			public ZString JS_F3_NKPackType { get; set; }
			public ZString JS_F3_NKTotalCountPackType { get; set; }
			public ZString JS_GoodsDescription { get; set; }
			public ZDecimal JS_GoodsValue { get; set; }
			public ZString JS_HBLAWBChargesDisplay { get; set; }
			public ZString JS_HBLContainerPackModeOverride { get; set; }
			public ZString JS_HouseBill { get; set; }
			public ZDateTime JS_HouseBillIssueDate { get; set; }
			public ZString JS_HouseBillOfLadingType { get; set; }
			public ZString JS_INCO { get; set; }
			public ZDecimal JS_InsuranceValue { get; set; }
			public ZString JS_InterimReceipt { get; set; }
			public ZString JS_InvisibleTabsXML { get; set; }
			public ZBool JS_IsBooking { get; set; }
			public ZBool JS_IsCancelled { get; set; }
			public ZBool JS_IsCFSRegistered { get; set; }
			public ZBool JS_IsDirectBooking { get; set; }
			public ZBool JS_IsForwardRegistered { get; set; }
			public ZBool JS_IsNeutralMaster { get; set; }
			public ZBool JS_IsShipping { get; set; }
			public ZBool JS_IsSplitShipment { get; set; }
			public ZGuid JS_JS_ColoadMasterShipment { get; set; }
			public ZGuid JS_JS_SplitSwitchShipment { get; set; }
			public ZGuid JS_JX { get; set; }
			public ZDecimal JS_LoadingMeters { get; set; }
			public ZDecimal JS_ManifestedChargeable { get; set; }
			public ZDecimal JS_ManifestedLoadingMeters { get; set; }
			public ZDecimal JS_ManifestedVolume { get; set; }
			public ZDecimal JS_ManifestedWeight { get; set; }
			public ZByte JS_NoCopyBills { get; set; }
			public ZByte JS_NoOriginalBills { get; set; }
			public ZGuid JS_OA_ExportReceivingDepot { get; set; }
			public ZGuid JS_OA_ImportReleaseDepot { get; set; }
			public ZGuid JS_OA_BookedShippingLineAddress { get; set; }
			public ZGuid JS_OH_DeliveryAgent { get; set; }
			public ZGuid JS_OH_ExportBroker { get; set; }
			public ZGuid JS_OH_HandledOnBehalfOfForwarder { get; set; }
			public ZGuid JS_OH_ImportBroker { get; set; }
			public ZGuid JS_OH_TranshipAgent { get; set; }
			public ZInt JS_OuterPacks { get; set; }
			public ZBool JS_OverrideWaybillDefaults { get; set; }
			public ZString JS_PackingMode { get; set; }
			public ZInt JS_PackingOrder { get; set; }
			public ZString JS_Phase { get; set; }
			public ZString JS_ReleaseType { get; set; }
			public ZString JS_RL_NKDestination { get; set; }
			public ZString JS_RL_NKOrigin { get; set; }
			public ZString JS_RS_NKServiceLevel { get; set; }
			public ZString JS_RX_NKFrtRateCurrency { get; set; }
			public ZString JS_RX_NKGoodsValueCurr { get; set; }
			public ZString JS_RX_NKInsuranceCurrency { get; set; }
			public ZString JS_ScreeningStatus { get; set; }
			public ZString JS_ShipmentStatus { get; set; }
			public ZString JS_ShipmentType { get; set; }
			public ZString JS_ShippedOnBoard { get; set; }
			public ZDateTime JS_ShippedOnBoardDate { get; set; }
			public ZDecimal JS_ShipperCODAmount { get; set; }
			public ZString JS_ShipperCODPayMethod { get; set; }
			public ZDateTime JS_SystemCreateTimeUtc { get; set; }
			public ZString JS_SystemCreateUser { get; set; }
			public ZDateTime JS_SystemLastEditTimeUtc { get; set; }
			public ZString JS_SystemLastEditUser { get; set; }
			public ZInt JS_TotalPackageCount { get; set; }
			public ZBool JS_TranshipToOtherCFS { get; set; }
			public ZString JS_TransportMode { get; set; }
			public ZDecimal JS_UnitFreightRate { get; set; }
			public ZString JS_UnitOfVolume { get; set; }
			public ZString JS_UnitOfWeight { get; set; }
			public ZInt JS_VisibleTabs { get; set; }
			public ZString JS_WarehouseLocation { get; set; }
			public ZGuid JS_WL { get; set; }
			public IEnumerable<IForwardingShipment> CoLoadShipments { get; }
			public ZBool IsMasterShipmentRepresentingAllChildShipments { get; }
			public IJobDocAddress ConsignorDocumentaryAddress { get; }
			public IJobDocAddress ConsigneeDocumentaryAddress { get; }
			public Enterprise.Integration.Customs.IBaseJobDeclaration[] Declarations { get; }
			public IJobDocsAndCartage DocsAndCartage { get; }
			public override SchemaGuidColumn PKSchemaColumn => JobShipmentSchema.PK;
			public bool AllowInvoiceDeletion => throw new NotImplementedException();
			public string JobNumber => throw new NotImplementedException();
			public ITransport Transports_AddNew()
			{
				throw new NotImplementedException();
			}

			public BusinessObject GetDeclarationFor(ZGuid companyPK)
			{
				throw new NotImplementedException();
			}

			public void SetJobNumberFieldOnSaving()
			{
				throw new NotImplementedException();
			}

			public void OnJobCreating(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobCreated(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleting(JobHeader job)
			{
				throw new NotImplementedException();
			}

			public void OnJobDeleted(JobHeader job)
			{
				throw new NotImplementedException();
			}
		}

		#endregion
	}
}
