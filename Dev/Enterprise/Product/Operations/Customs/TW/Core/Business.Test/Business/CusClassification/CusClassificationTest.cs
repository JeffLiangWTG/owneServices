using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	sealed class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		[ExpectNoExceptions]
		public void TestDefaultValues()
		{
			NUnit.Framework.Assert.That(Classification.CC_RN_NKCountryCode, NUnit.Framework.Is.EqualTo(GlbCompany.CurrentCompany.GC_RN_NKCountryCode), "Country is set");
			NUnit.Framework.Assert.That(Classification.CC_ClassificationType, NUnit.Framework.Is.EqualTo(CusClassification.ClassificationType.Both).Using(CustomComparers.TypeComparison), "Type is set");
		}

		[ExpectNoExceptions]
		public void TestTypeDecider()
		{
			NUnit.Framework.Assert.That(Factory.New(typeof(Customs.Business.BaseCusClassification)).GetType(), NUnit.Framework.Is.EqualTo(GetExpectedBusinessObjectType()), "Update Customs.Business.BaseCusClassification to include a decider for this class");
		}

		#region Implementation
		new CusClassification Classification
		{
			get
			{
				return (CusClassification)base.Classification;
			}
		}
		#endregion
	}
}
