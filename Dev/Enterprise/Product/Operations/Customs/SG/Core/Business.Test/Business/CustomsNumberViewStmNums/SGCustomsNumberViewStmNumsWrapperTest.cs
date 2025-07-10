using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGCustomsNumberViewStmNumsWrapper))]
	public class SGCustomsNumberViewStmNumsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			var stmNumsWrapper = new SGCustomsNumberViewStmNumsWrapper(stmNum);
			AssertEquals("Number Range", stmNumsWrapper.HumanReadableName);
		}

		public void TestGetSingaporeMessageNumber()
		{
			var customsNumber1 = Company.CustomsNumberProvider.CustomsNumbers.AddNew();
			customsNumber1.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber1.SN_MinimumValue = 1000;
			customsNumber1.SN_MaximumValue = 2000;
			customsNumber1.SN_Value = 1000;
			customsNumber1.SN_FountainName = "SA0001";
			customsNumber1.Factory.Save();
			var customsNumber2 = Company.CustomsNumberProvider.CustomsNumbers.AddNew();
			customsNumber2.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber2.SN_MinimumValue = 3000;
			customsNumber2.SN_MaximumValue = 4000;
			customsNumber2.SN_Value = 3000;
			customsNumber2.SN_FountainName = "SA0001";
			customsNumber1.Factory.Save();
			CombineAssertions(() =>
			{
				AssertNull("GC_CustomsRegistrationNo empty", SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(Company));
				Company.GC_CustomsRegistrationNo = "SA0001";
				AssertEquals("GC_CustomsRegistrationNo filled", 1000L, SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(Company).SN_Value);
			}

			);
		}

		public void TestGetSingaporeMessageNumberAfterSwitchingCompanies()
		{
			var sg1 = Factory.New<GlbCompany>();
			sg1.GC_Code = "SC1";
			sg1.GC_RN_NKCountryCode = "SG";
			sg1.GC_CustomsRegistrationNo = "SA0001";
			var sg1Branch = sg1.Branches.AddNew();
			sg1Branch.GB_Code = "SB1";
			var sg2 = Factory.New<GlbCompany>();
			sg2.GC_Code = "SC2";
			sg2.GC_RN_NKCountryCode = "SG";
			sg2.GC_CustomsRegistrationNo = "SA0002";
			var sg2Branch = sg2.Branches.AddNew();
			sg2Branch.GB_Code = "SB2";
			Factory.Save();
			var sg1Provider = sg1.CustomsNumberProvider;
			var sg1CustomsNumbers = sg1Provider.CustomsNumbers;
			AssertEquals(0, sg1CustomsNumbers.Count);
			var customsNumber1 = sg1CustomsNumbers.AddNew();
			customsNumber1.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber1.SN_MinimumValue = 1000;
			customsNumber1.SN_MaximumValue = 2000;
			customsNumber1.SN_Value = 1000;
			customsNumber1.SN_FountainName = "SA0001";
			customsNumber1.SN_Owner = sg1.PK;
			Factory.Save();
			AssertEquals(1, sg1Provider.CustomsNumbers.Count);
			var sg2Provider = sg2.CustomsNumberProvider;
			Assert(sg1Provider != sg2Provider);
			var sg2CustomsNumbers = sg2Provider.CustomsNumbers;
			Assert(sg1CustomsNumbers != sg2CustomsNumbers);
			var customsNumber2 = sg2CustomsNumbers.AddNew();
			customsNumber2.SN_Type = NumberRangeTypeList.Codes.SingaporeMessageNumber;
			customsNumber2.SN_MinimumValue = 3000;
			customsNumber2.SN_MaximumValue = 4000;
			customsNumber2.SN_Value = 3000;
			customsNumber2.SN_FountainName = "SA0002";
			customsNumber2.SN_Owner = sg2.PK;
			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var nums = factory2.Load<CustomsNumberViewStmNums>(new ZQuery(ViewStmNumsSchema.SN_Name, SQLComparisonOperator.StartsWith, "C#SG"));
			AssertEquals(2, nums.Length);
			var num1 = nums.FirstOrDefault(num => num.SN_Owner == sg1.PK);
			AssertEquals("customsNumber1 sg1 SN_Type", NumberRangeTypeList.Codes.SingaporeMessageNumber, num1.SN_Type);
			AssertEquals("customsNumber1 sg1 SN_Prefix", "SA0001|1", num1.SN_Prefix);
			AssertEquals("customsNumber1 sg1 SN_Name", "C#SG-SMN _SA0001|1", num1.SN_Name);
			var num2 = nums.FirstOrDefault(num => num.SN_Owner == sg2.PK);
			AssertEquals("customsNumber2 sg2 SN_Type", NumberRangeTypeList.Codes.SingaporeMessageNumber, num2.SN_Type);
			AssertEquals("customsNumber2 sg2 SN_Prefix", "SA0002|1", num2.SN_Prefix);
			AssertEquals("customsNumber2 sg2 SN_Name", "C#SG-SMN _SA0002|1", num2.SN_Name);
			//-------------------
			var user = GlbStaff.CurrentUser;
			var department = GlbDepartment.CurrentDepartment;
			// this type of context switch simulates changing company in the UI.  The factory is taken from the User so becomes the same in both envoronments.
			using (Env.SetTemporaryUserContext(new UserContext(user, sg1Branch.PK.ToGuid(), department.PK.ToGuid())))
			{
				var smn1 = SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(GlbCompany.CurrentCompany);
				AssertEquals("GC_CustomsRegistrationNo filled", 1000L, smn1.SN_Value);
				var smn1NumberFountain = smn1.TryGetNumberFountain();
				var urnSeqNo1 = smn1NumberFountain.GetNext(Db.Connection);
				AssertEquals("customsNumber1 sg1 urnSeqNo1", 1000L, urnSeqNo1);
			}

			using (Env.SetTemporaryUserContext(new UserContext(user, sg2Branch.PK.ToGuid(), department.PK.ToGuid())))
			{
				var smn2 = SGCustomsNumberViewStmNumsWrapper.GetSingaporeMessageNumber(GlbCompany.CurrentCompany);
				AssertEquals("GC_CustomsRegistrationNo filled", 3000L, smn2.SN_Value);
				var smn2NumberFountain = smn2.TryGetNumberFountain();
				var urnSeqNo2 = smn2NumberFountain.GetNext(Db.Connection);
				AssertEquals("customsNumber2 sg2 urnSeqNo2", 3000L, urnSeqNo2);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var stmNum = Factory.New<CustomsNumberViewStmNums>();
			return new SGCustomsNumberViewStmNumsWrapper(stmNum);
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
