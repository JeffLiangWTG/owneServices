using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Global.GlbPerson
{
	public partial class GlbPersonNewForm : ZTemplateForm
	{
		public GlbPersonNewForm(Business.GlbPerson person)
			: base(person)
		{
			Person = person;
			InitializeComponent();
			LogsTabPage.TabVisible = false;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		Business.GlbPerson Person { get; }

		protected override void SaveToRecentItems()
		{
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			FullName.Focus();
		}

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			base.OnFormClosed(e);
			if (Person.IsInDatabase && !Globals.IsTest)
			{
				var justMoved = Person.IsMovingFromAnotherPerson;
				Person.IsMovingFromAnotherPerson = false;

				var controller = ZControllerFactory.Create(ControllerIDs.GlbPerson);
				var form = controller.ShowEditForm(Person);

				if (justMoved)
				{
					var person = (form.BusinessEntityForPersistingForm as Business.GlbPerson);
					if (person != null)
					{
						person.Validation.ValidatePrimaryRelationship();
						if (person.HasErrors)
						{
							person.HasChanges = true;
						}
					}
				}
			}
		}

		public void SetEditMode()
		{
			HomeAddressGroupBox.Visible = false;
			ContactDetailsGroupBox.Visible = false;
			ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 423, true);
			MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 423, true);
			MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 423, true);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			var result = base.ValidateAndSave();
			Person.MobilePhoneNumber.FormattedForBindingInfo.RefreshBinding();
			return result;
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;
	}
}
