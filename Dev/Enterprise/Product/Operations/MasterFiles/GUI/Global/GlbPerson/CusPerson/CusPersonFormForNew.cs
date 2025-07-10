using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CusPersonForm : ZTemplateForm
	{
		public CusPersonForm(GlbPerson person)
			: base(person)
		{
			Person = person;
		}

		GlbPerson person;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "this.Text is used, this.Text must be assigned.")]
		GlbPerson Person
		{
			get { return person; }
			set
			{
				person = value;
				if (person != null)
				{
					this.Text = Res.GetString("GlbPersonForm.Caption", "Person {0}", person.PER_FullName);
				}
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
