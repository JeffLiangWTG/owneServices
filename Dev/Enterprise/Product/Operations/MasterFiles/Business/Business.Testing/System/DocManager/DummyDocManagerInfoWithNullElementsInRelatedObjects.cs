using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyDocManagerInfoWithNullElementsInRelatedObjects : DocManagerInfo
	{
		public DummyDocManagerInfoWithNullElementsInRelatedObjects(BusinessObject parent, string docManagerCode) : base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects())
			{
				null
			};
			return result.ToArray();
		}
	}
}
