using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	partial class RefCusTariffFilterStripControl : ZFilterStripControl
	{
		public RefCusTariffFilterStripControl()
		{
			InitializeComponent();
		}

		public RefCusTariffFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			InitializeMandatoryAttributeColumns();
		}

		protected override ZFilterStrip NewZFilterStrip() => new RefCusTariffFilterStrip();

		protected override ZFilterGrid GetNewFilteredGrid() => new RefCusTariffGrid();

		#region for dynamic fields

		void InitializeMandatoryAttributeColumns()
		{
			if (GridCollection is ChildTariffViewCollection tariffViewCombinedList && !string.IsNullOrEmpty(tariffViewCombinedList.TariffType))
			{
				var customPropertyContainer = new CustomPropertyContainer();
				foreach (var mandatoryAttr in tariffViewCombinedList.MandatoryAttributeNames)
				{
					customPropertyContainer.AddCustomProperty(
						mandatoryAttr.ZY6_Name,
						mandatoryAttr.ZY6_ColumnCaption,
						typeof(ZString),
						bizObj => ((TariffView)bizObj).GetAttribute(mandatoryAttr.ZY6_Name)?.ZZ3_Value ?? ZString.Empty,
						(bizObj, value) => ((TariffView)bizObj).SetAttribute(mandatoryAttr, value)
					);
				}

				var customColumnsInitializer = new ZZRefCusCodeListCustomColumnsInitializer(Grid, tariffViewCombinedList, ResourceStringData.Empty, customPropertyContainer);
				customColumnsInitializer.AddCustomColumns();
			}
		}

		#endregion
	}
}
