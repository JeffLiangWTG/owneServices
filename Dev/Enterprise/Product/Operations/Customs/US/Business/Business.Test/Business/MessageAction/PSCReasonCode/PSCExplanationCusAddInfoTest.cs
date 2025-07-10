using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PSCExplanationCusAddInfo))]
	sealed class PSCExplanationCusAddInfoTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<PSCExplanationCusAddInfo>
	{
		public void TestLoader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var pscExplanation = Factory.New<PSCExplanationCusAddInfo>();
			pscExplanation.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			pscExplanation.B7_ParentID = entry.PK;
			pscExplanation.B7_AddInfoData = "Some explanation";
			Factory.Save();
			var loadedPSCExplanation = new PSCExplanationCusAddInfo.Loader(Factory).Load(entry);
			AssertNotNull("Found data", loadedPSCExplanation);
			AssertEquals("Explanation", "Some explanation", loadedPSCExplanation.B7_AddInfoData);
		}

		protected override IEnumerable<PSCExplanationCusAddInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var pscExplanation = factory.New<PSCExplanationCusAddInfo>();
			pscExplanation.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			pscExplanation.B7_ParentID = entry.PK;
			pscExplanation.B7_AddInfoData = "Some explanation";
			yield return pscExplanation;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<PSCExplanationCusAddInfo>();
	}
}
