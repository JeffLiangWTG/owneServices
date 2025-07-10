using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusOutturnTestHelper))]
	sealed class CusOutturnTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUnderbondResponsiblePartyID()
		{
			Underbond.C4_DestinationPremiseID = "B123B";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "67094168242";

			AssertEquals("67094168242", Outturn.UnderbondResponsiblePartyID);

			CodeDescriptionPairList codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("B123B", "21003980130");
			FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, codeDescriptionPairList);

			AssertEquals("21003980130", Outturn.UnderbondResponsiblePartyID);
		}

		public void TestSetLinkedObject()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			OutturnableDummy dummy = Factory.New<OutturnableDummy>();
			outturn.Parent = dummy;
			AssertEquals("ParentID", dummy.PK, outturn.C5_ParentID);
			AssertEquals("TableCode", "Z0", outturn.C5_ParentTableCode);
			outturn.Parent = null;
			AssertEquals("ParentID", ZGuid.Empty, outturn.C5_ParentID);
			AssertEquals("TableCode", ZString.Empty, outturn.C5_ParentTableCode);
		}

		public void TestGetLinkedObject()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			OutturnableDummy dummy1 = Factory.New<OutturnableDummy>();
			outturn.Parent = dummy1;
			AssertEquals("LinkedObject", dummy1, outturn.Parent);
			OutturnableDummy dummy2 = Factory.New<OutturnableDummy>();
			outturn.Parent = dummy2;
			AssertEquals("LinkedObject", dummy2, outturn.Parent);
			dummy2.Delete();
			AssertEquals("LinkedObject", null, outturn.Parent);
		}

		public void TestOutturnMessages()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			AssertNotNull(outturn.Messages);
		}

		public void TestCustomsStatus()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			outturn.C5_CustomsStatus = "HLD";
			AssertEquals("HLD", outturn.CustomsStatus.Code);
			outturn.C5_CustomsStatus = "";
			AssertEquals("", outturn.CustomsStatus.Code);
			OutturnableDummy dummy = Factory.New<OutturnableDummy>();
			outturn.Parent = dummy;
			AssertEquals("WTO", outturn.CustomsStatus.Code);
		}

		public void TestMessageStatus()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			AssertEquals("", outturn.MessageStatus.Code);
			outturn.C5_MessageStatus = "EXP";
			AssertEquals("EXP", outturn.MessageStatus.Code);
		}

		public void TestNewIsTypeDecided()
		{
			Assert("Type is Correct", ExpectedBusinessObjectType.IsSubclassOf(Factory.New(typeof(CusOutturn)).GetType()));
		}

		public void TestLoadIsTypeDecided()
		{
			BusinessObject outturn = this.Outturn;
			Factory.Save();
			Assert("Type is Correct", ExpectedBusinessObjectType.IsSubclassOf(new BusinessObjectFactory().Load(typeof(CusOutturn), outturn.PK).GetType()));
		}

		public void TestUnderbond()
		{
			AssertEquals("Underbond", Underbond, Outturn.Underbond);
		}

		public void TestGetParentStringRepresentation()
		{
			CusOutturn outturn = ((CusOutturn)GetNewBusinessObject());
			OutturnableDummy dummy1 = Factory.New<OutturnableDummy>();
			outturn.Parent = dummy1;
			AssertEquals("ParentStringRepresentation", dummy1.UnderbondHumanReadableName, outturn.ParentStringRepresentation);
			outturn.Parent = null;
			AssertEquals("ParentStringRepresentation", ZString.Empty, outturn.ParentStringRepresentation);
		}

		public void TestSetParentStringRepresentation()
		{
			DummyBizoWithUnderbondCollection underbondParent = Factory.New<DummyBizoWithUnderbondCollection>();

			OutturnableDummy dummy1 = Factory.New<OutturnableDummy>();

			CusUnderbond underbond = underbondParent.Underbonds.AddNew(typeof(CusUnderbondThatLinksToDummyBizo));

			CusOutturn outturn = underbond.Outturns.AddNew(typeof(TestHelperCusOutturn));
			AssertEquals("Parent", null, outturn.Parent);
			underbondParent.OutturnableLines = new IOutturnableLine[] { dummy1 };
			outturn.ParentStringRepresentation = dummy1.UnderbondHumanReadableName;
			AssertEquals("Parent", dummy1, outturn.Parent);
			AssertEquals("C5_OuterPacks", 5, outturn.C5_OuterPacks);
			outturn.ParentStringRepresentation = "123123123";
			AssertEquals("Parent", null, outturn.Parent);
			AssertEquals("ParentStringRepresentation", ZString.Empty, outturn.ParentStringRepresentation);
		}

		public void TestParentStringRepresentationCanSetHouseInfo()
		{
			var underbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
			CusOutturn outturn = underbond.Outturns.AddNew();
			outturn.ParentStringRepresentation = "Cuckoo Squeakers";
			AssertEquals("Cuckoo Squeakers", outturn.ParentStringRepresentation);
			outturn.C5_HouseBill = "";
			AssertEquals("", outturn.ParentStringRepresentation);
		}

		public void TestCusUnderbondType()
		{
			var outturn = Factory.New<CusOutturnTestHelper>();
			AssertEquals("CusUnderbondType", typeof(CusUnderbond), outturn.CusUnderbondType);
		}

		public void TestTypeDecider()
		{
			AssertType<CusOutturnTypeDecider>(CusOutturn.TypeDecider);
		}

		#region Implementation

		#region TestHelper

		class CusOutturnTestHelper : CusOutturn
		{
			public CusOutturnTestHelper(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Type CusUnderbondType => base.CusUnderbondType;

			protected override TypeLoaderCollection GetParentLoaders()
			{
				TypeLoaderCollection result = base.GetParentLoaders();
				result.Add(new TypeLoader(typeof(OutturnableDummy)));
				return result;
			}
		}

		#endregion

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Outturn;
		}

		CusOutturn fOutturn;
		CusOutturn Outturn
		{
			get
			{
				if (fOutturn == null)
				{
					fOutturn = Factory.New<CusOutturn>();
					Underbond.Outturns.Add(fOutturn);
				}
				return fOutturn;
			}
		}

		CusUnderbond fUnderbond;
		CusUnderbond Underbond
		{
			get
			{
				if (fUnderbond == null)
				{
					fUnderbond = (CusUnderbond)Factory.New<Integration.Customs.AU.ICusUnderbond>();
				}
				return fUnderbond;
			}
		}

		#endregion
	}
}
