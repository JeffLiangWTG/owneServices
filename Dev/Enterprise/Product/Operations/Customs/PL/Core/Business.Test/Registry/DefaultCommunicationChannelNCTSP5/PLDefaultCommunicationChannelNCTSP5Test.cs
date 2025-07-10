using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLDefaultCommunicationChannelNCTSP5))]
sealed class PLDefaultCommunicationChannelNCTSP5Test : RegistryBusinessObjectTemplateTestCase<PLDefaultCommunicationChannelNCTSP5>
{
	public void TestPLDefaultCommunicationChannelNCTSP5DefaultValuesNoEmailChannel()
	{
		var plDefaultCommunicationChannelNCTSP5 = new PLDefaultCommunicationChannelNCTSP5();
		AssertEquals(false, plDefaultCommunicationChannelNCTSP5.IsEmailChannel);
		AssertEquals(true, plDefaultCommunicationChannelNCTSP5.IsSeapID);
	}

	public void TestPLDefaultCommunicationChannelNCTSP5DefaultValuesWithEmailChannel()
	{
		using (PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "Odyssey"))
		{
			var plDefaultCommunicationChannelNCTSP5 = new PLDefaultCommunicationChannelNCTSP5();
			AssertEquals(true, plDefaultCommunicationChannelNCTSP5.IsEmailChannel);
			AssertEquals(false, plDefaultCommunicationChannelNCTSP5.IsSeapID);
		}
	}

	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => true;

	protected override PLDefaultCommunicationChannelNCTSP5 GetBusinessObjectToClone() => (PLDefaultCommunicationChannelNCTSP5)GetNewBusinessObject();

	protected override PLDefaultCommunicationChannelNCTSP5 GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

	protected override BusinessObject GetNewBusinessObject() => new PLDefaultCommunicationChannelNCTSP5(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
}
