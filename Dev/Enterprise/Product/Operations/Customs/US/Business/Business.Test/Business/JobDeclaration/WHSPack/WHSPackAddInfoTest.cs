using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USWHSPackAddInfo))]
	public class WHSPackAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			AssertEquals(WHSPack, WHSPackAddInfo.Parent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return WHSPackAddInfo;
		}

		USWHSPackAddInfo WHSPackAddInfo
		{
			get { return whsPackAddInfo ?? (whsPackAddInfo = new USWHSPackAddInfo(WHSPack.B7_AddInfoDataInfo)); }
		}
		USWHSPackAddInfo whsPackAddInfo;

		WHSPack WHSPack
		{
			get { return whsPack ?? (whsPack = Declaration.WHSPacks.AddNew()); }
		}
		WHSPack whsPack;

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		#endregion
	}
}
