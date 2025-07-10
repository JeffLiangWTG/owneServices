using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Packing.Business
{
	public class PackageTemplatePackTypeWrapper : IPackageTemplate
	{
		public PackageTemplatePackTypeWrapper(RefPackType refPackType)
		{
			PackType = refPackType;
		}

		readonly RefPackType PackType;

		#region Length

		ZDecimal IPackageTemplate.Length
		{
			get { return PackType.F3_Length; }
		}

		#endregion

		#region Width

		ZDecimal IPackageTemplate.Width
		{
			get { return PackType.F3_Width; }
		}

		#endregion

		#region Height

		ZDecimal IPackageTemplate.Height
		{
			get { return PackType.F3_Height; }
		}

		#endregion

		#region Weight

		ZDecimal IPackageTemplate.TareWeight
		{
			get { return PackType.F3_Weight; }
		}

		#endregion

		#region Volume

		ZDecimal? IPackageTemplate.Volume
		{
			get { return null; }
		}

		#endregion

		#region WeightUQ

		ZString IPackageTemplate.WeightUQ
		{
			get { return PackType.F3_UnitOfWeight; }
		}

		#endregion

		#region DimensionUQ

		ZString IPackageTemplate.DimensionUQ
		{
			get { return PackType.F3_UnitOfDimension; }
		}

		#endregion

		#region VolumeUQ

		ZString IPackageTemplate.VolumeUQ
		{
			get { return ZString.Empty; }
		}

		#endregion
	}
}

