using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	[ControllerDoesNotSupportForm]
	class TradeGroupsController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.TradeGroups;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TradeGroups;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusRefTradeGroup);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlobalCodesView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlobalCodesNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlobalCodesEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlobalCodesDelete;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusRefTradeGroupForm((CusRefTradeGroup)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var tradeGroup = (CusRefTradeGroup)base.GetNewBusinessEntityInLocalFactory();
			if (ParentModule is TradeGroupsModule tradeGroupsModule)
			{
				var defaultCountryCode = tradeGroupsModule.FilterBusinessObjectDefaultCountryCode;
				if (!defaultCountryCode.IsEmpty)
				{
					tradeGroup.CR9_RN_NKCountryCode = defaultCountryCode;
				}
			}
			return tradeGroup;
		}
	}
}
