using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(SGCusClassPartPivotAddInfo))]
	public class SGCusClassPartPivotAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SGCusClassPartPivotAddInfo(CusClassPartPivot.CI_AddInfoInfo);
		}

		CusClassPartPivot CusClassPartPivot
		{
			get
			{
				return cusClassPartPivot ?? (cusClassPartPivot = Factory.New<CusClassPartPivot>());
			}
		}

		CusClassPartPivot cusClassPartPivot;
		#endregion
	}
}
