using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public class PossibleMatchesGrid : ZModuleButtonGrid
	{
		public PossibleMatchesGrid()
		{
			ShowAttachButton = false;
			ShowDetachButton = false;
			ShowEditButton = false;
			ShowNewButton = false;
		}

		protected override bool AllowDoubleClick
		{
			get { return true; }
		}

		protected override bool NeedsSaveToShowEditForm(BusinessObject selected) => false;

		protected override ZController GetNewControllerCore(BusinessObject selected)
		{
			var entry = Argument.NotNull(selected, "selected") as RateEntry;
			Argument.NotNull(entry, "entry");
			Argument.NotNull(entry.Parent, "entry.Parent");

			ControllerID cid;

			if (entry.IsClientRate())
			{
				cid = ControllerIDs.ClientRates;
			}
			else if (entry.IsCompanyTariff())
			{
				cid = ControllerIDs.GlobalRates;
			}
			else if (entry.IsCosting())
			{
				cid = ControllerIDs.Costing;
			}
			else if (entry.IsQuote())
			{
				cid = ControllerIDs.Quotations;
			}
			else if (entry.IsIntercompanyTariff())
			{
				cid = ControllerIDs.IntercompanyTariffs;
			}
			else
			{
				throw new NotSupportedException(entry.Parent.TH_RateType);
			}

			// Since PossibleMatchesForm is MODAL we cannot just bring opened modules forwards.
			// The form is still blocking user interactions with the module form.
			var controller = ZControllerFactory.Create(cid);
			var openedModuleForm = controller.GetOpenedForm(entry.Parent);
			if (openedModuleForm != null)
			{
				var message = Res.GetString(
					"db094d1f-aa45-4e70-8bb4-715ac8e3a53e",
					@"The tariffs and rates form for the selected rate is already opened in background. Please close this form, edit existing rates or add new rates and re-autorate.");
				Globals.Message.ShowInformation(message);
			}

			return controller;
		}

		protected override bool ElementsBelongToDifferentModules
		{
			get { return true; }
		}
	}
}

