using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.ZArchitecture.Business.FilterStripBusinessObject;

namespace Enterprise.Freight.Forwarding.GUI
{
	class ConsolCreationTemplateColumnStyle : ZCodeFindBoxColumnStyle
	{
		public ConsolCreationTemplateColumnStyle(ConsolCreationTemplateColumnStyleInfo columnInfo)
			: this(() => new ConsolCreationTemplateUserControl(), columnInfo)
		{
		}

		ConsolCreationTemplateColumnStyle(Func<ConsolCreationTemplateUserControl> control, ConsolCreationTemplateColumnStyleInfo columnInfo)
			: base(control, columnInfo)
		{
		}
	}

	[SuppressCheckControlModuleId]
	[SuppressCheckControlLookupList]
	public class ConsolCreationTemplateColumnStyleInfo : ZCodeFindBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(ConsolCreationTemplateColumnStyle); }
		}
	}

	class ConsolCreationTemplateUserControl : ZGridFindBox
	{
		public ConsolCreationTemplateUserControl()
		{
		}

		protected override ZFilterModule NewModuleFromModuleID()
		{
			var module = base.NewModuleFromModuleID();

			if (module is ZFilterGridModule jobConsolModule)
			{
				jobConsolModule.AllowLoadTemplateRecords = true;

				if (module.GridCollection is IFilterBusinessObjectDefaultsProvider filterBODefaultsProvider)
				{
					filterBODefaultsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Template Records", "Property", (ZString)TemplateRecordsFilterCodes.TemplatesOnly));
					filterBODefaultsProvider.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Template Active", "Property", (ZString)StatusActive));
				}
			}

			return module;
		}

		protected override EmbeddedModulePopup CreateEmbeddedPopup(ZFilterModule module)
		{
			var popup = base.CreateEmbeddedPopup(module);

			if (module.FilterBusinessObject is ITemplateRecordFilterProvider templateRecordFilterProvider)
			{
				templateRecordFilterProvider.ShouldApplyTemplateRecordFiltersLayout = true;
			}

			return popup;
		}

		protected override System.Collections.IList GetList(object dataSource, string listMember, string dataMemberForErrorReporting)
		{
			return null;
		}

		public override void SelectFromPopupForm(bool autoSelect = false)
		{
			var oldCode = Code;
			Code = string.Empty;

			base.SelectFromPopupForm(autoSelect);

			if (string.IsNullOrEmpty(Code) && !string.IsNullOrEmpty(oldCode))
			{
				Code = oldCode;
			}
		}

		protected override void OnPopupSelected(IFindBoxPopup popup, BusinessObject[] selectedBusinessObjects)
		{
			base.OnPopupSelected(popup, selectedBusinessObjects);

			var consol = selectedBusinessObjects.FirstOrDefault() as ForwardingConsol;
			if (consol != null)
			{
				Code = consol.TemplateRecord?.STR_TemplateName ?? string.Empty;
			}
		}
	}
}
