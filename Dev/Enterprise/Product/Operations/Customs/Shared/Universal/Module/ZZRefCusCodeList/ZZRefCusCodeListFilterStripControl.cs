using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.Module
{
	partial class ZZRefCusCodeListFilterStripControl : ZFilterStripControl
	{
		public ZZRefCusCodeListFilterStripControl()
		{
			InitializeComponent();
		}

		public ZZRefCusCodeListFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			InitializeMandatoryAttributeColumns();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new ZZRefCusCodeListFilterStrip();
		}

		#region for dynamic fields

		void InitializeMandatoryAttributeColumns()
		{
			if (GridCollection is ZZRefCusCodeListCombinedCollection refCusCodeListCombinedList && refCusCodeListCombinedList.CodeTypes != null && refCusCodeListCombinedList.CodeTypes.Count == 1)
			{
				var customPropertyContainer = new CustomPropertyContainer();
				foreach (var mandatoryAttr in refCusCodeListCombinedList.MandatoryAttributeNames)
				{
					customPropertyContainer.AddCustomProperty(
						mandatoryAttr.ZXE_Name,
						mandatoryAttr.ZXE_ColumnCaption,
						GetValueType(mandatoryAttr.ZXE_ValueDataType),
						bizObj => ((ZZRefCusCodeListCombined)bizObj).GetAttribute(mandatoryAttr),
						(bizObj, value) => ((ZZRefCusCodeListCombined)bizObj).SetAttribute(mandatoryAttr, value)
					);
				}

				var customColumnsInitializer = new ZZRefCusCodeListCustomColumnsInitializer(Grid, refCusCodeListCombinedList, ResourceStringData.Empty, customPropertyContainer);
				customColumnsInitializer.AddCustomColumns();
			}
		}

		Type GetValueType(ZString valueDataType)
		{
			Type result;
			switch (valueDataType.ToUpperInvariant())
			{
				case "":
				case Constants.RefCusCodeListAttributeName.ValueDataTypes.String:
					result = typeof(ZString);
					break;
				case Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean:
					result = typeof(ZBool);
					break;
				case Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer:
					result = typeof(ZInt);
					break;
				case Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal:
					result = typeof(ZDecimal);
					break;
				default:
					throw new DeveloperNotificationException("Unrecognized ZXE_ValueDataType: " + valueDataType);
			}
			return result;
		}

		#endregion
	}
}
