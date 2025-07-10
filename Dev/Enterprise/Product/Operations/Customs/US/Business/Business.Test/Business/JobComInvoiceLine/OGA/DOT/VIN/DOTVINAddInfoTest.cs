using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOTVINAddInfo))]
	public class DOTVINAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDOTVIN()
		{
			AssertEquals(DOTVIN, DOTVINAddInfo.Parent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return DOTVINAddInfo;
		}

		DOTVINAddInfo DOTVINAddInfo
		{
			get
			{
				if (dotvinAddInfo == null)
				{
					dotvinAddInfo = new DOTVINAddInfo(DOTVIN.B7_AddInfoDataInfo);
				}

				return dotvinAddInfo;
			}
		}
		DOTVINAddInfo dotvinAddInfo;

		DOTVIN DOTVIN
		{
			get { return dotvin ?? (dotvin = DOT.DOTVINs.AddNew()); }
		}
		DOTVIN dotvin;

		DOT DOT
		{
			get { return dot ?? (dot = Factory.New<DOT>()); }
		}
		DOT dot;

		#endregion
	}
}
