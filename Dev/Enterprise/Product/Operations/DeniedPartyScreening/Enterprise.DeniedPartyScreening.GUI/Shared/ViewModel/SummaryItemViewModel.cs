using System;
using System.Windows.Input;
using CargoWise.EntityFramework;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class SummaryItemViewModel
	{
		readonly BusinessObject screeningEntity;

		public SummaryItemViewModel(BusinessObject screeningEntity, string name, string screeningStatus)
		{
			this.screeningEntity = screeningEntity;

			Name = name;
			ScreeningStatus = screeningStatus;
			OpenPartyFormCommand = new DelegateCommand(OpenPartyForm);
		}

		public string Name { get; }

		public string ScreeningStatus { get; }

		public ICommand OpenPartyFormCommand { get; }

		void OpenPartyForm()
		{
			if (screeningEntity != null)
			{
				if (screeningEntity is JobDocAddress || screeningEntity is ITransport)
				{
					Globals.Message.ShowWarning(Res.GetString("EB17BD30-FEFC-4870-907F-7D478B938E63", "Not able to show form for this entity."));
					return;
				}

				var controller = ZControllerFactory.Instance.GetControllerForBizo(screeningEntity) ?? ZControllerFactory.Instance.GetControllerForType(GetBaseType(screeningEntity));

				if (controller != null)
				{
					controller.ShowViewForm(screeningEntity);
				}
				else
				{
					var message = Res.GetString("286372A3-AD74-487E-940F-A60C125EA622", "Can't load appropriate Controller for type: {0}, PK: {1}", GetBaseType(screeningEntity), screeningEntity.PK);
					Globals.Message.ShowError(message);
					ExceptionReporter.Instance.ReportDeveloperException("SummaryForm|CannotOpenLink", message, new ArgumentNullException());
				}
			}
		}

		Type GetBaseType(BusinessObject bizO)
		{
			if (bizO is RefVessel)
			{
				return typeof(RefVessel);
			}

			if (bizO is OrgHeader)
			{
				return typeof(OrgHeader);
			}

			return bizO.GetType();
		}
	}
}
