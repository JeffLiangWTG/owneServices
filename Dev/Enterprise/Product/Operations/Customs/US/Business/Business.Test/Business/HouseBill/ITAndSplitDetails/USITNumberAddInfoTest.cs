using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USITNumberAddInfo))]
	sealed class USITNumberAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new USITNumberAddInfo(Factory.New<ITAndSplitDetails>().B7_AddInfoDataInfo);
	}
}
