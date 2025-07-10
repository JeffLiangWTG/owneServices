using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DEAConstituentAddInfo))]
	public class DEAConstituentAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var constituent = Factory.New<DEAConstituent>();
			var addInfo = new DEAConstituentAddInfo(constituent.B7_AddInfoDataInfo);
			return addInfo;
		}

		#endregion
	}
}
