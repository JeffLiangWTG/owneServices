using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ZA.Module
{
	public class RefCusTariffModule : Universal.Module.RefCusTariffModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new RefCusTariffController();

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new RefCusTariffFilterStripBusinessObject(GridCollection as ChildTariffViewCollection);

		public void SetFindBoxCodeDescription(IFindBox findBox, BusinessObject bizo)
		{
			SetFindBoxCodeDesciption(findBox, bizo);
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox)
		{
			return new PopupModuleDecisionProvider(findbox, this);
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxCore(IFindBox findbox)
		{
			return new ZAModuleDecisionProvider(findbox);
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;

		internal static void SetFindBoxCodeDesciption(IFindBox findBox, BusinessObject bizo)
		{
			var tariffView = ((TariffView)bizo);
			var checkDigit = tariffView.GetAttribute(UniversalReferenceConstants.TariffAttributes.CheckDigit)?.ZZ3_Value.SubstringSafe(0, 2) ?? ZString.Empty;
			var tariffCode = string.Concat(tariffView.ZZ1_TariffCode.PadRight(10, ' '), checkDigit);
			tariffView.ZZ1_TariffCode = tariffCode;
			findBox.Code = tariffCode;
		}

		protected override IEnumerable<BusinessObject> GetBizObjsToEditOrViewCore(ZFilterModule module, ZString codeToFind, Func<IEnumerable<BusinessObject>> getBizoActionDefault)
		{
			var query = module.GridCollection.CompleteFilter;
			query.AddToFilter(RefCusTariffFilterStripBusinessObject.GetTariffCodePlusCheckDigitQuery(SQLComparisonOperator.Equal, codeToFind));
			return Factory.Load<TariffView>(query);
		}
	}

	public class ZAModuleDecisionProvider : ZArchitecture.Modules.Internal.ModuleDecisionProvider
	{
		public ZAModuleDecisionProvider(IFindBox findBox)
			: base(findBox, null)
		{
		}

		public override void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			RefCusTariffModule.SetFindBoxCodeDesciption(FindBox, bizo);
		}
	}

	public class PopupModuleDecisionProvider : ZArchitecture.Modules.Internal.PopupModuleDecisionProvider
	{
		public PopupModuleDecisionProvider(IFindBox findBox, RefCusTariffModule module)
			: base(findBox)
		{
			this.module = module;
		}

		public override void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			module.SetFindBoxCodeDescription(FindBox, bizo);
		}

		readonly RefCusTariffModule module;
	}
}
