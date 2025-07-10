using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Packing.Business
{
	public class PackageIDNumberGeneratorTarget : NumberGeneratorTarget
	{
		#region NumberCustomisationLocation

		public override string NumberCustomisationLocation
		{
			get { return Res.GetString("7cde432f-d4de-44c1-8e37-50f592b4511a", "Packing -> Package ID Customization"); }
		}

		#endregion

		#region GetNumberCustomisationCore

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(PackingRegistry.Instance.PackageIDCustomisation);
		}

		#endregion

		#region GetMaxLengthCore

		protected override int GetMaxLengthCore()
		{
			return PkgPackageHeader.Schema.KPH_PackageIDMaxLength;
		}

		#endregion

		#region GetNameCore

		protected override ZString GetNameCore()
		{
			return Res.GetString("163a2f4f-6157-4b53-9acd-241ffc30b395", "Package ID");
		}
		#endregion
	}
}

