using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Universal
{
	public class RefCusProfileAttribute : AutoRefCusProfileAttribute
	{
		public RefCusProfileAttribute(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(Profile))]
		public override ZGuid XXY_XX0_Profile { get => base.XXY_XX0_Profile; set => base.XXY_XX0_Profile = value; }

		public RefCusProfile Profile => Factory.Load<RefCusProfile>(XXY_XX0_Profile);
	}
}
