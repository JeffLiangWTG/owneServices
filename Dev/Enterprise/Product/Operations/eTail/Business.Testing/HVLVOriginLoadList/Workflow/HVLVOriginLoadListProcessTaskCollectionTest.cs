using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.eTail.Business.Testing
{
	[TestedType(typeof(HVLVOriginLoadListProcessTaskCollection))]
	public class HVLVOriginLoadListProcessTaskCollectionTest : ProcessTaskCollectionTest<HVLVOriginLoadListProcessTaskCollection>
	{
		#region Implementation

		protected override HVLVOriginLoadListProcessTaskCollection GetCollectionToTestCore()
		{
			return new HVLVOriginLoadListProcessTaskCollection(OriginLoadList);
		}

		HVLVOriginLoadList OriginLoadList
		{
			get
			{
				if (originLoadList == null)
				{
					originLoadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
				}

				return originLoadList;
			}
		}

		HVLVOriginLoadList originLoadList;

		#endregion
	}
}
