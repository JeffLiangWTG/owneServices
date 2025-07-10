using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
			var brokerage = UserControl as CustomsBrokerageUserControl;
			if (brokerage != null)
			{
				brokerage.AddPlugins(true);
			}
		}

		public override string Name
		{
			get { return "Brokerage"; }
		}

		protected override MenuItem GetNewTopLevelMenuCore()
		{
			return new NZEDIMenu();
		}

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl()
		{
			BaseCustomsBrokerageUserControl result = null;
			JobDeclaration declaration = JobDeclaration as JobDeclaration;
			if (declaration != null)
			{
				result = new CustomsBrokerageUserControl();
			}
			return result;
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore()
		{
			return new CreateDeclarationHelper();
		}

		protected override bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated()
		{
			ZString errorMessage = ZString.Empty;
			bool result;

			if (!UniversalTariffHelper.UseRefDatabaseData)
			{
				var loader = new NZCTariffVersionLoader(Shipment.Factory);
				errorMessage = loader.ErrorMessage;
			}

			if (!errorMessage.IsEmpty)
			{
				coveringLabelText = errorMessage;
				result = false;
			}
			else
			{
				result = base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
			}

			return result;
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var continueSave = base.ShowPreSaveDialogsCore();

			if (continueSave == ContinueWithSave.Yes)
			{
				var declaration = JobDeclaration as JobDeclaration;

				if (declaration != null)
				{
					var unsentMessageChangeSupport = new UnsentMessageChangeSupport();
					continueSave = unsentMessageChangeSupport.CheckForHeldMessageChangesAndPerformUserAction(declaration, this.Form);
				}
			}

			if (continueSave == ContinueWithSave.Yes)
			{
				var declaration = JobDeclaration as JobDeclaration;
				if (declaration != null && declaration.HasDuplicatedActiveEntryHeader())
				{
					Globals.Message.ShowWarning(
					Res.GetString("07752239-665A-4410-B27F-A852A9CCBE97", "Another user has already created the entry header for the declaration.\r\nThe system will now try to combine your changes with those of the other user.\r\nPlease review the entry header after saved the form."),
					"Duplicated Entry Header");
				}
			}

			return continueSave;
		}
	}
}
