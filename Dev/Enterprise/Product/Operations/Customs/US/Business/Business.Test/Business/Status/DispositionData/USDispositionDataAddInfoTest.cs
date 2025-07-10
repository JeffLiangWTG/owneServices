using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USDispositionDataAddInfo))]
	sealed class USDispositionDataAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUS_IsInactive()
		{
			USDispositionDataAddInfo dispostionData = (USDispositionDataAddInfo)GetNewBusinessObject();
			AssertEquals(false, dispostionData.US_IsInactive);

			dispostionData.US_IsInactive = true;
			AssertEquals(true, dispostionData.US_IsInactive);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USDispositionDataAddInfo(Declaration.DispositionCodes.AddNew().B7_AddInfoDataInfo);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}

		JobDeclaration declaration;

		#endregion
	}
}
