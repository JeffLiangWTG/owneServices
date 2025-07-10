using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(TradersRemark))]
	public class TradersRemarkTest : Customs.Business.Testing.CusSupportingInfoTest<TradersRemark>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<JobDeclaration>().TradersRemarks.AddNew();
		}

		public void TestSetDefaultValues()
		{
			var tradersRemark = Factory.New<TradersRemark>();
			AssertEquals(CusSupportingInfoTypeList.Codes.TradersRemarks, tradersRemark.CSI_Type);
			AssertEquals(JobDeclarationSchema.Constants.Prefix, tradersRemark.CSI_ParentTableCode);
		}

		public void TestCSI_Description_TrimAndDelete()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.COO;
			var tradersRemark = declaration.TradersRemarks.AddNew();
			tradersRemark.CSI_Description = "  12345   ";
			AssertEquals("12345", tradersRemark.CSI_Description);
			tradersRemark.CSI_Description = "  ";
			AssertEquals(true, tradersRemark.IsDeleted);
		}

		protected override IEnumerable<TradersRemark> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var tradersRemarks = factory.New<JobDeclaration>().TradersRemarks.AddNew();
			tradersRemarks.CSI_Description = "Example traders remarks";
			yield return tradersRemarks;
		}
	}
}
