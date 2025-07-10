using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using NUnit.Framework;
using JobMessageTypeList = Enterprise.Customs.US.Business.JobMessageTypeList;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class JobDeclarationUserControlImportBasherTest : JobDeclarationUserControlBasherAbstractTest
	{
		public override ZString MessageTypeForFormBashing => JobMessageTypeList.Codes.Import;

		protected override BaseJobDeclaration GetPopulatedDeclarationForFormBashing()
		{
			var declaration = base.GetPopulatedDeclarationForFormBashing();
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
