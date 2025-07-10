using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(PhaseSecurity))]
	public class PhaseSecurityTest : RegistryBusinessObjectTemplateTestCase<PhaseSecurity>
	{
		public void TestPhases()
		{
			PhaseSecurity security = new PhaseSecurity();
			AssertEquals(security, security.Phases.Parent);
		}

		public void TestIPhaseSecurity()
		{
			PhaseSecurity security = new PhaseSecurity();
			security.IsEnabled = true;
			security.Phases.AddNew().Code = "AAA";
			security.Phases.AddNew().Code = "BBB";

			IPhaseSecurity iSecurity = security;
			AssertEquals(true, iSecurity.IsEnabled);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BBB" }, iSecurity.Phases.Select(x => x.Code));
		}

		public void TestClone_Phases()
		{
			PhaseSecurity security = new PhaseSecurity();
			security.Phases.AddNew().Code = "AAA";
			security.Phases.AddNew().Code = "BBB";

			PhaseSecurity clonedSecurity = (PhaseSecurity)security.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), security.Factory);
			AssertContainsExactElementsInAnyOrder("Phases colleciton cloned", new ZString[] { "AAA", "BBB" }, clonedSecurity.Phases.Cast<Phase>().Select(x => x.Code));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PhaseSecurity(new CodeDescriptionPairList());
		}

		protected override PhaseSecurity GetBusinessObjectToClone()
		{
			return (PhaseSecurity)GetNewBusinessObject();
		}

		protected override PhaseSecurity GetBusinessObjectToSerialise()
		{
			return (PhaseSecurity)GetNewBusinessObject();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
