using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(PLDefaultCommunicationChannelNCTSP5RegistryItem))]
sealed class PLDefaultCommunicationChannelNCTSP5RegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PLDefaultCommunicationChannelNCTSP5>
{
	protected override StronglyTypedRegistryItem<PLDefaultCommunicationChannelNCTSP5, PLDefaultCommunicationChannelNCTSP5> GetNewRegistryItem()
	{
		var plDefaultCommunicationChannelNCTSP5 = new PLDefaultCommunicationChannelNCTSP5(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
		plDefaultCommunicationChannelNCTSP5.IsEmailChannel = true;
		var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
		var isPhase5 = nctsSettings.IsUsingPhase5(GlbCompany.CurrentCompany.Country.Code);
		return new PLDefaultCommunicationChannelNCTSP5RegistryItem("", (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.System, isPhase5 ? RegistryOptions.Default : RegistryOptions.IsReadOnly, plDefaultCommunicationChannelNCTSP5);
	}
}

[TestedType(typeof(PLDefaultCommunicationChannelNCTSP5DataType))]
sealed class PLDefaultCommunicationChannelNCTSP5DataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<PLDefaultCommunicationChannelNCTSP5DataType>
{
	protected override PLDefaultCommunicationChannelNCTSP5DataType GetNewDataType() => new PLDefaultCommunicationChannelNCTSP5DataType();

	protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
	{
		var factory = new BusinessObjectFactory();
		var sample1 = new PLDefaultCommunicationChannelNCTSP5(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), factory);
		sample1.IsEmailChannel = true;
		sample1.IsSeapID = false;
		var sample2 = new PLDefaultCommunicationChannelNCTSP5 { IsEmailChannel = false, IsSeapID = true };

		return new[]
		{
			new ValidSampleAndBinaryValueInDB(sample1, new PLDefaultCommunicationChannelNCTSP5DataType().Serialise(sample1)),
			new ValidSampleAndBinaryValueInDB(sample2, new PLDefaultCommunicationChannelNCTSP5DataType().Serialise(sample2))
		};
	}

	protected override string ExpectedEditorName => "PLDefaultCommunicationChannelNCTSP5RegistryItemEditor";
}
