using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class GlbGroupLocation : AutoGlbGroupLocation
	{
		protected GlbGroupLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region TypeDecider

		public static readonly TypeDecider TypeDecider = new GlbGroupLocationTypeDecider();

		#endregion

		#region Properties

		public abstract ILocation Location { get; }

		#endregion
	}
}
