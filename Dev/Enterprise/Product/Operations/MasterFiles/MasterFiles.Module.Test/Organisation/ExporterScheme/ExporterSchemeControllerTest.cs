using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExporterSchemeController))]
	internal class ExporterSchemeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ExporterScheme;
		}

		protected override string CountryCode
		{
			get { return Enterprise.Core.Constants.CountryCodes.Zimbabwe; }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			return header;
		}
	}
}
