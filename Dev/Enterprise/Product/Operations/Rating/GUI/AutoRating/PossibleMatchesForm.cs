using System;
using System.Windows.Forms;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Displays similar rate matches during AutoRating.
	/// </summary>
	[SuppressBindingMemberBashingTest] // for UpdateJobDetailsCheckbox
	public partial class PossibleMatchesForm : ZChildForm, _Rating.ISuspendOwner
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		public PossibleMatchesForm(PossibleMatchesWrapper possibleMatchesWrapper)
			: base(possibleMatchesWrapper)
		{
			ratingSuspender = _Rating.Suspend(this);
			InitializeComponent();
			descriptionText.Text = Res.GetString("4bfea087-de63-45db-8afc-f3d3705ea31f", "Autorating was unable to find any matching rates. However similar rates exist in your system.");
			instructionText.Text = Res.GetString("3b0cefdc-73e4-4011-bd2e-cbd1c4bf6529", $"Please create rates with trade lanes matching your job. " +
				$"You can do so by making the above trade lanes more generic or by cloning the above trade lanes and then modifying newly created trade lanes. " +
				$"You can double click on an item in the grid to navigate.\r\n" +
				$"If you find company tariffs above with trade lanes matching your job, you can link the corresponding company tariffs to the client's organization.");
		}

		readonly IDisposable ratingSuspender;

		public void HandleAttemptToCreateNestedSession()
		{
			// Don't want to do too much here, like closing this form, since this isn't a reproducable problem.
			// Try bring the window to the front so the user can see what they need to interact with and close it themselves.
			BeginInvoke(new MethodInvoker(BringToFront));
		}

		void OkayButton_Click(object sender, EventArgs e)
		{
			Close();
			Dispose();
		}

		public override string FormHeading
		{
			get { return FormCaption; }
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			ratingSuspender.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}
