using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbPersonDocManagerInfo : DocManagerInfo
	{
		public GlbPersonDocManagerInfo(BusinessObject parent, string docManagerCode) : base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			List<BusinessObject> result = new List<BusinessObject>();

			var person = (GlbPerson)BusinessEntity;
			result.AddRange(person.StaffCollection.ToArray());
			result.AddRange(person.ApplicantCollection.ToArray());

			return result.ToArray();
		}
	}
}
