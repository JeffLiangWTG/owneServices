using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class UniversalDataObjectWriterHelperTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestLoadCusEntryNumber()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.Parent = dec;
			entryNumber.CE_EntryType = "SB2";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Antarctica;
			Factory.SaveForTesting();
			var helper = new UniversalDataObjectWriterHelper(new BusinessObjectFactory(), Core.Constants.CountryCodes.Bermuda);
			AssertNull(helper.LoadCusEntryNumber(ZGuid.Invalid, "ZD", "Z#"));
			var entryNumberLoaded = helper.LoadCusEntryNumber(dec.PK, "SB2", Core.Constants.CountryCodes.Antarctica);
			AssertEquals(entryNumber.PK, entryNumberLoaded.PK);
		}

		public void TestLoad()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "DUMMY 234";
			org.MainAddress.OA_Address1 = "STREET 1";
			org.OH_Code = "DZ!@#";

			var helper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates);

			var addInfos = new Dictionary<ZString, ZString>();
			addInfos.Add(DummyBizoSchema.Z0_Money.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZDecimal)34.53m));
			addInfos.Add(DummyBizoSchema.Z0_Guid.Name.Substring(3), BaseAddInfo.GetStringRepresentation(org.MainAddress.PK));

			var loadObject = helper.Load<OrgAddress, DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Guid, addInfos);
			AssertEquals(org.MainAddress, loadObject);
		}

		public void TestGetValue()
		{
			var guid = ZGuid.NewZGuid();
			var helper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates);
			var dummy = Factory.New<DummCusContainerWithAddInfo>();
			var addInfos = new Dictionary<ZString, ZString>();
			addInfos.Add(DummyBizoSchema.Z0_Money.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZDecimal)8635.45m));
			addInfos.Add(DummyBizoSchema.Z0_Bool.Name.Substring(3), BaseAddInfo.GetStringRepresentation(ZBool.True));
			addInfos.Add(DummyBizoSchema.Z0_Code.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZString)"DGD"));
			addInfos.Add(DummyBizoSchema.Z0_Date.Name.Substring(3), BaseAddInfo.GetStringRepresentation(ZDateTime.BrettsBirthday));
			addInfos.Add(DummyBizoSchema.Z0_Decimal.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZDecimal)45.35m));
			addInfos.Add(DummyBizoSchema.Z0_Guid.Name.Substring(3), BaseAddInfo.GetStringRepresentation(guid));
			addInfos.Add(DummyBizoSchema.Z0_Number.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZInt)342));
			addInfos.Add(DummyBizoSchema.Z0_Short.Name.Substring(3), BaseAddInfo.GetStringRepresentation((ZShort)8962));

			AssertEquals(null, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Money, addInfos));
			AssertEquals(ZBool.True, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Bool, addInfos));
			AssertEquals((ZString)"DGD", helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Code, addInfos));
			AssertEquals(ZDateTime.BrettsBirthday, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Date, addInfos));
			AssertEquals((ZDecimal)45.35m, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Decimal, addInfos));
			AssertEquals(guid, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Guid, addInfos));
			AssertEquals((ZInt)342, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Number, addInfos));
			AssertEquals((ZShort)8962, helper.GetValue<DummCusContainerWithAddInfo>(DummyBizoSchema.Z0_Short, addInfos));
		}

		public void TestUpdate()
		{
			var helper = new UniversalDataObjectWriterHelper(Factory.BOFactory, Core.Constants.CountryCodes.UnitedStates);
			var addInfoCollection = new List<AddInfo>();
			helper.Update(addInfoCollection, "HELLO", (ZString)"HI");
			AssertEquals(1, addInfoCollection.Count);
			AssertEquals("HELLO", addInfoCollection[0].Key);
			AssertEquals("HI", addInfoCollection[0].Value);

			helper.Update(addInfoCollection, "HELLO", ZString.Empty);
			AssertEquals(0, addInfoCollection.Count);

			helper.Update(addInfoCollection, "HELLO", ZDateTime.BrettsBirthday);
			AssertEquals(1, addInfoCollection.Count);
			AssertEquals("HELLO", addInfoCollection[0].Key);
			AssertEquals(BaseAddInfo.GetStringRepresentation(ZDateTime.BrettsBirthday), addInfoCollection[0].Value);
		}

		public void TestAllocateAndGetEntryInstructionLink()
		{
			CombineAssertions(() =>
			{
				var factory = new BusinessObjectFactory();
				var helper = new UniversalDataObjectWriterHelper(factory, Core.Constants.CountryCodes.SouthAfrica);

				var testID1 = ZGuid.NewZGuid();
				var testID2 = ZGuid.Empty;
				var testID3 = ZGuid.Invalid;
				var testID4 = ZGuid.NewZGuid();

				AssertNull("Getting From Blank 1", helper.GetAllocatedEntryInstructionLink(testID1));
				AssertNull("Getting From Black 2", helper.GetAllocatedEntryInstructionLink(testID2));
				AssertNull("Getting From Black 3", helper.GetAllocatedEntryInstructionLink(testID3));
				AssertNull("Getting From Black 4", helper.GetAllocatedEntryInstructionLink(testID4));

				AssertEquals("Setting to Blank 1", 1, helper.AllocateEntryInstructionLink(testID1));
				AssertNull("Setting to Black 2", helper.AllocateEntryInstructionLink(testID2));
				AssertNull("Setting to Black 3", helper.AllocateEntryInstructionLink(testID3));
				AssertEquals("Setting to Black 4", 2, helper.AllocateEntryInstructionLink(testID4));

				AssertEquals("Getting from allocated 1", 1, helper.GetAllocatedEntryInstructionLink(testID1));
				AssertNull("Getting from allocated 2", helper.GetAllocatedEntryInstructionLink(testID2));
				AssertNull("Getting from allocated 3", helper.GetAllocatedEntryInstructionLink(testID3));
				AssertEquals("Getting from allocated 4", 2, helper.GetAllocatedEntryInstructionLink(testID4));
			});
		}

		public void TestGetWayBillType()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalDataObjectWriterHelper(factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals("HWB", helper.GetWayBillType("CN").Code.Value);
			AssertEquals("HWB", helper.GetWayBillType("HB").Code.Value);
			AssertEquals("MWB", helper.GetWayBillType("MB").Code.Value);

			helper = new DataObjectWriterHelperForTest(factory);
			AssertEquals("C55", helper.GetWayBillType("CN").Code.Value);
		}

		sealed class DataObjectWriterHelperForTest : UniversalDataObjectWriterHelper
		{
			internal DataObjectWriterHelperForTest(BusinessObjectFactory factory) : base(factory, Core.Constants.CountryCodes.Taiwan)
			{
			}

			public override WayBillType GetWayBillType(ZString billType)
			{
				return billType == "CN" ? new WayBillType() { Code = "C55" } : base.GetWayBillType(billType);
			}
		}
	}
}
