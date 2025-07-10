
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for AccGLAccountDescriptorCollection.
	/// </summary>
	[ModuleID(ModuleId.AccGLAccountDescriptor)]
	public class AccGLAccountDescriptorCollection : BusinessObjectCollection<AccGLAccountDescriptor>, IAccGLAccountDescriptorCollection
	{
		public AccGLAccountDescriptorCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public AccGLAccountDescriptorCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}
	}
}
