using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	internal class TradeGroupsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.TradeGroups;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlobalTariffs;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => true;

		public override bool AllowDelete => true;

		public override bool AllowEdit => true;

		public override bool AllowView => true;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.TradeGroups);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TradeGroupsFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TradeGroupsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusRefTradeGroupCollection(Factory);

		protected override IZForm ShowEditForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			var tradeGroup = selectedBusinessObject as CusRefTradeGroup;
			if (tradeGroup != null && tradeGroup.CR9_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				Globals.Message.ShowError(Res.GetString("d10541fd-dd77-4ef8-8a85-9fe0ed04a27a", "Trade Groups that do not belong to your country cannot be edited."));
			}
			else
			{
				form = base.ShowEditForm(selectedBusinessObject);
			}

			return form;
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IZForm form = null;
			var tradeGroup = selectedBusinessObject as CusRefTradeGroup;
			if (tradeGroup != null && tradeGroup.CR9_RN_NKCountryCode != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				Globals.Message.ShowError(Res.GetString("36bb785d-f1d5-41d8-bd71-25a128cc005a", "Trade Groups that do not belong to your country cannot be deleted."));
			}
			else
			{
				form = base.ShowDeleteForm(selectedBusinessObject);
			}

			return form;
		}

		public ZString FilterBusinessObjectDefaultCountryCode
		{
			get
			{
				var result = ZString.Empty;
				if (FilterBusinessObject is TradeGroupsFilterStripBusinessObject filterObj
					&& filterObj.ContainsDefaults
					&& filterObj[CusRefTradeGroupCollection.FilterConstants.CountryCode] is ModuleNkFilter countryCodeFilter)
				{
					result = countryCodeFilter.Property;
				}
				return result;
			}
		}
	}
}
