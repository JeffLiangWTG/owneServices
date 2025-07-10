using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	[TestedType(typeof(CMDDataValueWrapperCollection))]
	class CMDDataValueWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CMDDataValueWrapperCollection>
	{
		public void TestLoad()
		{
			Assert("Pre-condition", !Collection.IsLoaded);
			AssertEquals("Pre-condition", 0, Collection.Count);
			CusCodeData cusEntry1 = Collection.CMDDataValues.AddNew();
			CusCodeData cusEntry2 = Collection.CMDDataValues.AddNew();
			AssertEquals(0, Collection.Count);
			Collection.Load();
			AssertEquals(2, Collection.Count);
			AssertEquals(cusEntry1, Collection[0].CMDDataValue);
			AssertEquals(cusEntry2, Collection[1].CMDDataValue);
			Assert("Should be loaded now", Collection.IsLoaded);
		}

		public void TestLoad_ResetListsAndFactory()
		{
			BusinessObjectFactory factoryForNewCMDDataValues = Collection.FactoryForNewCMDDataValues;
			CMDDataValueWrapper newCusEntryWrapper = Collection.AddNew();
			Collection.ElementsToDeleteOnCommit.Add(Factory.New<CMDPermitNumber>());
			AssertEquals("Pre-condition", 1, Collection.Count);
			AssertEquals("Pre-condition", 1, Collection.ElementsToDeleteOnCommit.Count);
			Collection.Load();
			AssertNotEquals("Should be re-lazycreated", factoryForNewCMDDataValues, Collection.FactoryForNewCMDDataValues);
			AssertEquals("Elements should be removed", 0, Collection.Count);
			AssertEquals("Elements should be removed", 0, Collection.ElementsToDeleteOnCommit.Count);
		}

		public void TestCopyChangesToShipmentFactory()
		{
			CusCodeData cusEntryNum1 = Collection.CMDDataValues.AddNew();
			cusEntryNum1.CY_Code = CustomsEntryTypeList.Singapore.Permit;
			cusEntryNum1.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cusEntryNum1.CY_Data = "PMT101";
			CusCodeData cusEntryNum2 = Collection.CMDDataValues.AddNew();
			cusEntryNum2.CY_Code = CustomsEntryTypeList.Singapore.Certificate;
			cusEntryNum2.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cusEntryNum2.CY_Data = "CER101";
			Collection.Load();
			AssertEquals("Pre-condition", 2, Collection.Count);
			Shipment.HasChanges = false;
			CMDDataValueWrapper newEntryWrapper = Collection.AddNew();
			newEntryWrapper.PermitOrExemptionType = CustomsEntryTypeList.Singapore.SGExemption.Codes.HT;
			newEntryWrapper.PermitNumberOrExemptionRemarks = "MEH MEH";
			newEntryWrapper.HasChanges = true;
			Collection.RemoveAndDelete(Collection[0]);
			Collection[0].PermitOrExemptionType = CustomsEntryTypeList.Singapore.Permit;
			Collection[0].PermitNumberOrExemptionRemarks = "NEWPMT";
			Collection[0].HasChanges = true;
			Collection.CopyChangesToShipmentFactory();
			Assert("Shipment.HasChanges should be set to true", Shipment.HasChanges);
			AssertEquals(2, Collection.CMDDataValues.Count);
			AssertEquals(CustomsEntryTypeList.Singapore.Permit, Collection.CMDDataValues[0].CY_Code);
			AssertEquals("NEWPMT", Collection.CMDDataValues[0].CY_Data);
			AssertEquals(CustomsEntryTypeList.Singapore.SGExemption.Codes.HT, Collection.CMDDataValues[1].CY_Code);
			AssertEquals("MEH MEH", Collection.CMDDataValues[1].CY_Data);
		}

		public void TestNewShipmentCusEntryNumIsCreatedWhenNewElementIsCreatedInTheCollection()
		{
			CMDDataValueWrapper newEntryWrapper = Collection.AddNew();
			AssertEquals(JobShipmentSchema.Constants.Prefix, newEntryWrapper.CMDDataValue.CY_ParentTableCode);
			AssertEquals(Shipment.PK, newEntryWrapper.CMDDataValue.CY_ParentID);
			AssertEquals(Collection.FactoryForNewCMDDataValues, newEntryWrapper.CMDDataValue.Factory);
		}

		public void TestForceRemoveAll()
		{
			CusCodeData entry1 = Collection.CMDDataValues.AddNew();
			entry1.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CusCodeData entry2 = Collection.CMDDataValues.AddNew();
			entry2.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			CusCodeData entry3 = Collection.CMDDataValues.AddNew();
			entry3.CY_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			Collection.Load();
			AssertEquals(3, Collection.Count);
			Collection.ForceRemoveAll();
			AssertEquals("Should clear all elements regardless of the parent table", 0, Collection.Count);
		}

		protected override CMDDataValueWrapperCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var wrapper = new CMDShipmentWrapper(shipment);
			return new CMDDataValueWrapperCollection(wrapper);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Factory.New<CMDPermitNumber>();
			entry.CY_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			return new CMDDataValueWrapper(entry);
		}

		new CMDDataValueWrapperCollection Collection
		{
			get
			{
				return base.Collection;
			}
		}

		ForwardingShipment Shipment
		{
			get
			{
				return Collection.Shipment;
			}
		}
	}
}
