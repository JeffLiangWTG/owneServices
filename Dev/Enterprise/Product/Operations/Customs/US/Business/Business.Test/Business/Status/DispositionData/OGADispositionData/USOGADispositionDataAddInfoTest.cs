using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOGADispositionDataAddInfo))]
	sealed class USOGADispositionDataAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USOGADispositionDataAddInfo(Declaration.OGADispositionCodes.AddNew().B7_AddInfoDataInfo);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}

		JobDeclaration declaration;

		#endregion
	}
}
