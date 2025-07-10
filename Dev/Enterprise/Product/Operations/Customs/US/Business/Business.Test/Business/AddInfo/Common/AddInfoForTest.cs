using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AddInfoForTest : AddInfo
	{
		public AddInfoForTest(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public bool IsExportCoreForTesting = true;

		protected override bool IsExportCore => IsExportCoreForTesting;

		protected override ZString GetTransportMode() => Core.Constants.TransportModes.Unknown;

		protected override BusinessObject UseWrappedPropertiesOnly() => null;
	}
}
