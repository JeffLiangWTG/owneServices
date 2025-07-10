using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbExternalPasswordForATest : GlbExternalPassword
	{
		public GlbExternalPasswordForATest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			CredentialRecipientExpose = CredentialRecipient.eHub;
		}

		public override ZString ConfigurationName => "TEST1";

		public override CredentialRecipient CredentialRecipient => CredentialRecipientExpose;

		public CredentialRecipient CredentialRecipientExpose;
	}

	sealed class GlbExternalPasswordForBTest : GlbExternalPassword
	{
		public GlbExternalPasswordForBTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ConfigurationName => "TEST2";
	}

	sealed class GlbExternalPasswordForCTest : GlbExternalPassword
	{
		public GlbExternalPasswordForCTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString ConfigurationName => "TEST3";
	}
}
