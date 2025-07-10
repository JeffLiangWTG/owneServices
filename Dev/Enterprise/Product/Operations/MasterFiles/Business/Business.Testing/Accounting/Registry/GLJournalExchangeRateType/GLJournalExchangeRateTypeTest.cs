using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GLJournalExchangeRateType))]
	class GLJournalExchangeRateTypeTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => true;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone() => new GLJournalExchangeRateType();

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLJournalExchangeRateType();
		}

		protected new GLJournalExchangeRateType BizObj
		{
			get { return (GLJournalExchangeRateType)base.BizObj; }
		}

		#endregion
	}
}
