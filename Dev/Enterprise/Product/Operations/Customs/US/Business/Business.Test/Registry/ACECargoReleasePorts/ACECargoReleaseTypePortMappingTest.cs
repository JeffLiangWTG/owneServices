using System;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ACECargoReleaseTypePortMapping))]
	sealed class ACECargoReleaseTypePortMappingTest : RegistryBusinessObjectTemplateTestCase<ACECargoReleaseTypePortMapping>
	{
		public void TestCloneTargets()
		{
			var data = new ACECargoReleaseTypePortMapping();
			var portsAndModes = data.PortsAndModes.AddNew();
			portsAndModes.Delete();
			var portsAndModes2 = data.PortsAndModes.AddNew();
			portsAndModes2.CertificationMethod = CertificationOptionsList.Codes.ACE;
			portsAndModes2.Port = "3901";
			portsAndModes2.TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			var cloned = (ACECargoReleaseTypePortMapping)data.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			var count = data.PortsAndModes.Count;
			AssertEquals("Deleted target should not be cloned", count - 1, cloned.PortsAndModes.Count);
			for (int i = 0; i < cloned.PortsAndModes.Count; i++)
			{
				AssertEquals(data.PortsAndModes[i + 1].CertificationMethod, cloned.PortsAndModes[i].CertificationMethod);
				AssertEquals(data.PortsAndModes[i + 1].Port, cloned.PortsAndModes[i].Port);
				AssertEquals(data.PortsAndModes[i + 1].TransportMode, cloned.PortsAndModes[i].TransportMode);
			}
		}

		public void TestValidation()
		{
			var data = new ACECargoReleaseTypePortMapping();
			data.CertificationOption = "!";
			AssertHasErrorContaining(data.CertificationOptionInfo, "Enter a valid selection.");
			data.CertificationOption = CertificationOptionsList.Codes.ACS;
			AssertNoErrorContaining(data.CertificationOptionInfo, "Enter a valid selection.");
		}

		public void TestGetCertificationMethod()
		{
			var data = new ACECargoReleaseTypePortMapping();
			var portsAndModes = data.PortsAndModes.AddNew();
			portsAndModes.CertificationMethod = CertificationOptionsList.Codes.ACE;
			portsAndModes.Port = "3901";
			portsAndModes.TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Sea;
			AssertEquals(CertificationOptionsList.Codes.ACE, data.GetCertificationMethod("3901", Enterprise.Customs.Business.TransportTypeList.Codes.Sea));
			AssertEquals(ZString.Empty, data.GetCertificationMethod("2501", Enterprise.Customs.Business.TransportTypeList.Codes.Sea));
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override ACECargoReleaseTypePortMapping GetBusinessObjectToClone()
		{
			var result = new ACECargoReleaseTypePortMapping();
			result.FillWithValidTestData();
			return result;
		}

		protected override ACECargoReleaseTypePortMapping GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
