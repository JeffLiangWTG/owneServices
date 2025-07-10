using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	internal class ForwardingPkgPackageOutturnProvider : IOutturnProvider
	{
		readonly PkgPackage package;

		internal ForwardingPkgPackageOutturnProvider(PkgPackage pkgPackage)
		{
			package = pkgPackage;
		}

		public int OutturnQty => 0;
		public ZDateTime? UnloadDate => null;
		public ZDateTime? LoadDate => null;
		public int OutturnDamagedQty => package.KP_IsDamaged ? package.KP_PackageQty : ZInt.Zero;
		public int OutturnPillagedQty => 0;
		public decimal OutturnedHeight => 0;
		public decimal OutturnedLength => 0;
		public decimal OutturnedVolume => 0;
		public decimal OutturnedWeight => 0;
		public decimal OutturnedWidth => 0;
		public string ActualTransportJobID => "";
		public string ActualTransportJobTypeCode => "";
		public string ActualTransportJobTypeDescription => "";
		public string ExpectedTransportJobID => "";
		public string ExpectedTransportJobTypeCode => "";
		public string ExpectedTransportJobTypeDescription => "";
		public bool IsHighRisk => false;
		public ZString OverriddenAviationSecurityInspectionType => "";
	}
}
