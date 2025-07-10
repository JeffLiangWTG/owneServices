using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccAlternateGLAccountAttributeDependentCollection : DependentBusinessObjectCollection<AccAlternateGLAccountAttribute, AccAlternateGLAccount>
	{
		public AccAlternateGLAccountAttributeDependentCollection(AccAlternateGLAccount alternateGLAccount) : base(alternateGLAccount)
		{
		}

		protected override string FkColumnName => AccAlternateGLAccountAttributeSchema.AAA_AGA_AlternateGLAccount.Name;
	}

	public class AccAlternateGLAccountAttributeCollection : BusinessObjectCollection<AccAlternateGLAccountAttribute>
	{
		public AccAlternateGLAccountAttributeCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
