using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.GUI.Testing
{
	abstract class CustomsUserControlBasherAbstractTest : Customs.GUI.Testing.BaseCustomsUserControlBasherTest
	{
		protected override Customs.Business.BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = declaration.TablePrefix;
			header.BH_OverrideFreightDefaults = ZBool.True;
			var moveHeader = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			return declaration;
		}
	}
}
