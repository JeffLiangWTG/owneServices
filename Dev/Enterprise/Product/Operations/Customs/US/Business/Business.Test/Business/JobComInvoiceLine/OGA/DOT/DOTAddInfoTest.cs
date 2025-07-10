using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOTAddInfo))]
	public class DOTAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDOT()
		{
			AssertEquals(DOT, DOTAddInfo.Parent);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return DOTAddInfo;
		}

		DOTAddInfo DOTAddInfo
		{
			get
			{
				if (dotAddInfo == null)
				{
					dotAddInfo = new DOTAddInfo(DOT.B7_AddInfoDataInfo);
				}

				return dotAddInfo;
			}
		}
		DOTAddInfo dotAddInfo;

		DOT DOT
		{
			get { return dot ?? (dot = Factory.New<DOT>()); }
		}
		DOT dot;

		#endregion
	}
}
