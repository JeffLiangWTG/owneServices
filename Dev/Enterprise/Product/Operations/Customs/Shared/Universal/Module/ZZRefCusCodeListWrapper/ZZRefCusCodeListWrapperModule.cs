using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Universal.Module
{
#if DEBUG
	[SuppressFormsLocalizedTest]
#endif
	public abstract class ZZRefCusCodeListWrapperModule : ZFilterGridModule
	{
		public override ZBool HasActions => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowDelete => false;
		public override bool AllowView => true;

		protected override IFilterControl GetNewFilterControl()
		{
			var result = new ZFilterStripControl(GridCollection, FilterBusinessObject);
			var newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = CodeCaption, ColumnName = ZZRefCusCodeListWrapper.Schema.Code };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, CodeCaptionColumnWidth, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			newStyleInfo = new ZTextBoxColumnStyleInfo { Caption = DescriptionCaption, ColumnName = ZZRefCusCodeListWrapper.Schema.Description };
			ControlDpiScalingHelper.SetWidth(newStyleInfo, DescriptionColumnWidth, true);
			result.FilteredGrid.ColumnStyles.Add(newStyleInfo);

			return result;
		}

		protected virtual string CodeCaption => Res.GetString("b3aa0a5b-f52e-4a85-a0b0-7e0eea5d1c08", "Code");
		protected virtual string DescriptionCaption => Res.GetString("80c410bf-15f8-48e2-a25e-7193b5680c2d", "Description");
		protected virtual int CodeCaptionColumnWidth => 80;
		protected virtual int DescriptionColumnWidth => 300;

		protected override BusinessObject[] SelectedBusinessObjects => base.SelectedBusinessObjects.Cast<ZZRefCusCodeListWrapper>().Select(x => x.CusCodeList).ToArray();

		protected override BusinessObject CurrentBusinessObjectInGrid => (base.CurrentBusinessObjectInGrid as ZZRefCusCodeListWrapper)?.CusCodeList;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.Universal.ZZRefCusCodeList);
		}

		protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter)
		{
			var businessObject = selectedBusinessObject;
			if (selectedBusinessObject is ZZRefCusCodeListWrapper wrapper)
			{
				businessObject = wrapper.CusCodeList;
			}
			return base.CanReloadWithFilter(newFactory, businessObject, filter);
		}

		protected override bool IsModuleAllowAsync => false;

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			// I'm pretty sure ZZResCusCodeListWrapperCollection<T> leaks, due to CusCodeListCollection never updating its factory.
			var result = ((IZZRefCusCodeListWrapperCollection)GridCollection).LoadCusCodeList(query);
			return PerformSearchResult.Success(factory, query, result, permitActiveCollectionUpdates: false);
		}
	}
}
