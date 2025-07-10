using System;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ValidationToolUserControl : ZUserControl
	{
		public ValidationToolUserControl()
		{
			InitializeComponent();
			ValidationRulesFilter.EditableInViewMode = true;
		}

		IValidationToolParent ValidationToolParent => DataSource as IValidationToolParent;

		#region Filter

		public static FilterStripBusinessObject GetValidationRulesFilterBusinessObject()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var filterBizo = new ProcessTemplateValidationFilterStripBusinessObject();
				ObjectFactory.Get<IFilterModuleStrategy>("DeniedPartyScreeningFilterModuleStrategy").RunOnModuleFiltersCreated(filterBizo.ModuleFilters, typeof(ProcessTemplateValidation), null);

				return filterBizo;
			}

			return null;
		}

		void ProcessTemplateValidationFilterStripControl_PerformSearch(object sender, EventArgs e)
		{
			if (ValidationToolParent != null)
			{
				ApplyFilter();
			}
		}

		public void ApplyFilter()
		{
			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name, nameof(ApplyFilter)))
			{
				var collection = ValidationToolParent?.ProcessTemplateValidations;
				if (collection != null)
				{
					var filterBusinessObject = (ProcessTemplateValidationFilterStripBusinessObject)ValidationRulesFilter.FilterBusinessObject;
					filterBusinessObject.FilterInProcessTaskTemplates = collection.ProcessTaskTemplates;
					collection.AdditionalFilter = filterBusinessObject.Filter;
				}
			}
		}

		#endregion
	}
}
