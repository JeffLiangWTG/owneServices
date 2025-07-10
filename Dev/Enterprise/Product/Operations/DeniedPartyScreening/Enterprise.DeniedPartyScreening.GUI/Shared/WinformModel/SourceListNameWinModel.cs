using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class SourceListNameWinModel
	{
		readonly string code;
		readonly BusinessObjectFactory factory;

		public SourceListNameWinModel(BusinessObjectFactory factory, DpsComplianceListItem item)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNullOrEmpty(item.Code, nameof(item.Code));
			Argument.NotNullOrEmpty(item.Name, nameof(item.Name));
			code = item.Code;
			Name = item.Name;
			Description = item.Description;
			this.factory = factory;
		}

		public string Name { get; }

		public string Description { get; }

		public void OpenComplianceForm()
		{
			if (ScreenedPartyModel.SourceListWatermark == code)
			{
				return;
			}

			var complianceListItem = factory.LoadFromNaturalKey<RefComplianceList>(RefComplianceListSchema.RCL_ListCode, code);

			if (complianceListItem == null)
			{
				Globals.Message.ShowInformation(Res.GetString("4A69B425-A3F4-4729-8482-93F48AE7F50A", "This record is currently unavailable via the Compliance Lists (Party Screening) module. Please raise a customer support incident if this issue persists."));
			}
			else
			{
				var controller = ZControllerFactory.Instance.GetControllerForBizo(complianceListItem);
				if (controller != null)
				{
					controller.ShowViewForm(complianceListItem);
				}
				else
				{
					var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"You do not have appropriate controller for the RefComplianceList: PK[{0}], Code[{1}], Name[{2}] ", complianceListItem.PK, complianceListItem.RCL_ListCode, complianceListItem.RCL_ListName);
					ExceptionReporter.Instance.ReportDeveloperException("7322207A-4889-4854-8FB2-3A8700714858", message, new ArgumentNullException(message));
				}
			}
		}
	}
}
