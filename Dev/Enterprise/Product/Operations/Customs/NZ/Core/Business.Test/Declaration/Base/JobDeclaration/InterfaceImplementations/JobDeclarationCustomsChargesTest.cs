using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.Business.Declaration.InterfaceImplementations.Testing
{
	using CargoWise.Common;
	using Enterprise.Accounting.Integration;

	class JobDeclarationCustomsChargesTest : TestCaseWithFactory
	{
		public void TestECIAutoRatingIsDisabledForECIWriteoffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			ICustomsCharges decAsICustomsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			AssertEquals("Declaration.IsActive - from ICustomsCharges", false, decAsICustomsCharges.IsActive);
		}

		public void TestIsCustomsChargesActiveCore()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ICustomsCharges customsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("CustomsCharges.IsActive", true, customsCharges.IsActive);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("CustomsCharges.IsActive", true, customsCharges.IsActive);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			AssertEquals("CustomsCharges.IsActive", false, customsCharges.IsActive);

			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Drawback;
			AssertEquals("CustomsCharges.IsActive", false, customsCharges.IsActive);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			AssertEquals("CustomsCharges.IsActive", true, customsCharges.IsActive);
		}
	}
}
