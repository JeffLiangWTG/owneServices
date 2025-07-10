using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USOGADispositionDetailAddInfo))]
	sealed class USOGADispositionDetailAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USOGADispositionDetailAddInfo(OGADispositionCode.OGADispositionDetails.AddNew().B7_AddInfoDataInfo);
		}

		OGADispositionData OGADispositionCode
		{
			get { return fOGADispositionCode ?? (fOGADispositionCode = Factory.New<OGADispositionData>()); }
		}

		OGADispositionData fOGADispositionCode;
		#endregion
	}
}
