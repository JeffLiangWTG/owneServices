using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileAttributeCollection : ActiveBusinessObjectCollection<RefCusProfileAttribute>
	{
		public RefCusProfileAttributeCollection(RefCusProfile refCusProfile)
			: base(refCusProfile.Factory, refCusProfile, new ZQuery(), RefCusProfileAttributeSchema.XXY_XX0_Profile)
		{
		}
	}
}
