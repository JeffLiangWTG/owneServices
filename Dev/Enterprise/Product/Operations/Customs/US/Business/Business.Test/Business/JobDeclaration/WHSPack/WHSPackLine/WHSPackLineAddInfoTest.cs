using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USWHSPackLineAddInfo))]
	public class WHSPackLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			AssertEquals(WHSPackLine, WHSPackLineAddInfo.Parent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return WHSPackLineAddInfo;
		}

		USWHSPackLineAddInfo WHSPackLineAddInfo
		{
			get { return whsPackLineAddInfo ?? (whsPackLineAddInfo = new USWHSPackLineAddInfo(WHSPackLine.B7_AddInfoDataInfo)); }
		}
		USWHSPackLineAddInfo whsPackLineAddInfo;

		WHSPackLine WHSPackLine
		{
			get { return whsPackLine ?? (whsPackLine = Declaration.WHSPackLines.AddNew(WHSPack)); }
		}
		WHSPackLine whsPackLine;

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
