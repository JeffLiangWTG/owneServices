using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DefaultOSMG))]
	sealed class DefaultOSMGTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			DefaultOSMG result = new DefaultOSMG(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DefaultOSMG();
		}

		#endregion
	}
}
