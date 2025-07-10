using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	internal abstract class CertificateCusCodeDataAbstractTest<T> : Customs.Business.Testing.CusCodeDataTest<T>
		where T : CertificateCusCodeData
	{
		protected override IEnumerable<T> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var result = factory.New<T>();
			result.Parent = entryInstruction;
			yield return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();

			var helper = new PermitTestDataHelper(Factory);
			var permitHeader = helper.CreatePermitHeader(importer.PK, "TEST", ZDate.Today, ZDate.Today.AddDays(1), PermitQtyValIndicatorList.Codes.VAL, PermitTypeTypeForTesting, ZString.Empty, 0m, 1000m);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var bizO = Factory.New<T>();
			bizO.Parent = entryInstruction;
			bizO.CY_Code = permitHeader.CPH_Number;

			return bizO;
		}

		protected virtual ZString PermitTypeTypeForTesting => "XXX";
	}
}
