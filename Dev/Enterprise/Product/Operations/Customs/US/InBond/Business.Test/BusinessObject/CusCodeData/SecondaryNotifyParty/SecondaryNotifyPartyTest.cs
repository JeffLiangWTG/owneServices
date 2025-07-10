using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(SecondaryNotifyParty))]
	sealed class SecondaryNotifyPartyTest : Customs.Business.Testing.CusCodeDataTest<SecondaryNotifyParty>
	{
		public void TestEmptyDataBizObjIsDeletedOnSaving()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			snp.CY_Data = "HELLO";
			snp.CY_ParentTableCode = "B0";
			snp.CY_ParentID = ZGuid.NewZGuid();
			Factory.Save();
			AssertEquals(false, snp.IsDeleted);
			snp.CY_Data = "";
			Factory.Save();
			AssertEquals(true, snp.IsDeleted);
		}

		public void TestValidation()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals("Validation", typeof(SecondaryNotifyPartyValidation), snp.Validation.GetType());
		}

		public void TestLookups()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals("Validation", typeof(SecondaryNotifyPartyLookups), snp.Lookups.GetType());
		}

		public void TestSetDefaultValues()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			AssertEquals(CusCodeDataTypeList.Codes.SecondaryNotifyParty, snp.CY_Type);
		}

		public void TestParent()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			snp.CY_ParentID = MoveDetail.PK;
			snp.CY_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(CusInBondMoveDetail.Schema.TableName);
			AssertEquals(MoveDetail, snp.Parent);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var snp = factory.NewWithValidTestData<SecondaryNotifyParty>();
			snp.CY_Data = "HELLO";
			return snp;
		}

		protected override IEnumerable<SecondaryNotifyParty> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<SecondaryNotifyParty>();
			result.CY_Data = "HELLO";
			var header = factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.SecondaryNotifyParties.Add(result);
			yield return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SecondaryNotifyParty snp = Factory.New<SecondaryNotifyParty>();
			snp.Parent = MoveDetail;
			return snp;
		}

		CusInBondMoveDetail moveDetail;
		CusInBondMoveDetail MoveDetail
		{
			get
			{
				if (moveDetail == null)
				{
					CusInBondHeader header = Factory.New<CusInBondHeader>();
					CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
					moveDetail = moveHeader.MovementDetails.AddNew();
				}

				return moveDetail;
			}
		}
	}
}
