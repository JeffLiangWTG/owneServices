using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	sealed class ECIBrokeragePlugInTest : BrokeragePlugInTest
	{
		protected override void PrepareShipmentAndMergedDeclaration()
		{
			base.PrepareShipmentAndMergedDeclaration();
			JobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			return new BrokeragePlugIn(Shipment);
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			return declaration;
		}
	}
}
