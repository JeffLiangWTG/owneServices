using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USITDocAddInfo))]
	sealed class USITDocAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new USITDocAddInfo(Factory.New<ITDoc>().B7_AddInfoDataInfo);
	}
}
