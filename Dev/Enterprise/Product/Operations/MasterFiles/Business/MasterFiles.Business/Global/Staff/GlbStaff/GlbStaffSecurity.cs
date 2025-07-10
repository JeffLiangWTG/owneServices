using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffSecurity : SecurityStringSplitter
	{
		public GlbStaffSecurity()
			: base()
		{ }

		#region Schema

		public new class Schema : SecurityStringSplitter.Schema
		{
			public const string Explicit = "Explicit";
		}

		#endregion

		public ZString Explicit { get; set; }
	}
}
