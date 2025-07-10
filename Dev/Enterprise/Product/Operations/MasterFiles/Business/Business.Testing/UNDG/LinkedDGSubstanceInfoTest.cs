using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LinkedDGSubstanceInfo))]
	sealed class LinkedDGSubstanceInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLinkedDGSubstanceInfo_InformationMapping()
		{
			var substance = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IATA);
			substance.DG_Class = "2.1";
			substance.DG_PSN = "Bad Subs";
			substance.DG_SubLabel1 = "Subs Label 1";
			substance.DG_LQMaxAmt = 12;
			substance.DG_ExceptedQuantityCode = "ER";
			substance.DG_Mode = "SEA";
			substance.DG_Country = "AU";

			var attr = substance.SpecialProvisionsAttributes.AddNew();
			attr.DA_Descriptor = "Description";

			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;

			var info = new LinkedDGSubstanceInfo(item, substance);
			AssertEquals("SEA", info.Mode);
			AssertEquals("AU", info.Country);
			AssertEquals("Bad Subs", info.ProperShippingName);
			AssertEquals("2.1", info.Class);
			AssertEquals("Subs Label 1", info.SubLabel);
			AssertEquals(12m, info.LimitedQuantity);
			AssertEquals("ER", info.ExceptedQuantity);
			AssertEquals("Description", info.AdditionalInformation);
		}

		public void TestUNDGSubstancePivotSynchronization_Default()
		{
			var substance = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IATA);
			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;

			var info = new LinkedDGSubstanceInfo(item, substance);
			Assert(info.IsLinked);
			Assert(info.IsLinkedInfo.ReadOnly);
			AssertEquals(1, item.UNDGSubstancePivotCollection.Count);
		}

		public void TestUNDGSubstancePivotSynchronization_AdditionalSubstance()
		{
			var substance1 = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IATA);
			var substance2 = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IMO);

			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance1.PK;

			var info = new LinkedDGSubstanceInfo(item, substance2);
			Assert(!info.IsLinked);
			Assert(!info.IsLinkedInfo.ReadOnly);
			AssertEquals(1, item.UNDGSubstancePivotCollection.Count);
		}

		public void TestUNDGSubstancePivotSynchronization_Linked()
		{
			var substance1 = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IATA);
			var substance2 = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IMO);

			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance1.PK;

			var info = new LinkedDGSubstanceInfo(item, substance2);
			info.IsLinked = true;

			Assert(!info.IsLinkedInfo.ReadOnly);
			AssertEquals(2, item.UNDGSubstancePivotCollection.Count);

			info.IsLinked = false;
			AssertEquals(1, item.UNDGSubstancePivotCollection.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var substance = CreateSubstance("8888", "C", UNDGSubstanceStandardTypes.IATA);

			var item = Factory.New<UNDGDataItem>();
			item.DI_DG = substance.PK;

			return new LinkedDGSubstanceInfo(item, substance);
		}

		UNDGSubstance CreateSubstance(ZString unno, ZString variant, ZString standard, Action<UNDGSubstance> additionalInitialisation = null)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = unno;
			substance.DG_Variant = variant;
			substance.DG_Standard = standard;
			additionalInitialisation?.Invoke(substance);
			return substance;
		}
	}
}
