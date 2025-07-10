using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using JobDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data.Testing
{
	[TestedType(typeof(NZShipmentDeclarationValueObjectDataAdapter))]
	class NZShipmentDeclarationValueObjectDataAdapterTest : ShipmentDeclarationValueObjectDataAdapterTest
	{
		public void TestGetJobDeclaration()
		{
			NZShipmentDeclarationValueObjectDataAdapterForTest adapter = new NZShipmentDeclarationValueObjectDataAdapterForTest();
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			JobDeclaration declaration = adapter.GetJobDeclaration(context) as JobDeclaration;
			AssertEquals("Declaration.IsFormalEntry", true, declaration.IsFormalEntry);

			interchange.InterchangeInfo.Target.Type = Xsd.InterchangeInfoTargetType.LowValueDeclaration;
			declaration = adapter.GetJobDeclaration(context) as JobDeclaration;
			AssertEquals("Declaration.IsECIWriteoff", true, declaration.IsECIWriteoff);
		}

		protected override void AssertDelegation()
		{
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Default Adapter Type", typeof(NZShipmentDeclarationValueObjectDataAdapter), adapter.GetType());

			TestDeclarationValueObjectDataAdapter.Register();
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Adapter Overriden New Delegate Type", typeof(TestDeclarationValueObjectDataAdapter), adapter.GetType());
		}

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		class NZShipmentDeclarationValueObjectDataAdapterForTest : NZShipmentDeclarationValueObjectDataAdapter
		{
			public new BaseJobDeclaration GetJobDeclaration(IValueObjectImportContext context)
			{
				return base.GetJobDeclaration(context);
			}
		}
	}
}
