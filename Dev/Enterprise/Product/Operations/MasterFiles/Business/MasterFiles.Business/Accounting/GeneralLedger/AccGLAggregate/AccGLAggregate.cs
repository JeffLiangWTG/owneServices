using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLAggregate : AutoAccGLAggregate
	{
		public AccGLAggregate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public sealed override bool CanDelete
		{
			get { return false; }
		}
	}
}
