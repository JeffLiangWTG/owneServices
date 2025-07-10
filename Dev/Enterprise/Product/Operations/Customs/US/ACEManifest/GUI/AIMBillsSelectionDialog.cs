using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.ACEManifest.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.GUI
{
	public partial class AIMBillsSelectionDialog : ASYCUDA.GUI.AsycudaItemSelectionDialog
	{
		[Obsolete("This constructor is just for the designer")]
		public AIMBillsSelectionDialog()
		{
			InitializeComponent();
		}

		public AIMBillsSelectionDialog(AIMMessageChooser messageChooser, string itemsType)
			: base(messageChooser, itemsType)
		{
			chooser = Argument.NotNull(messageChooser, nameof(messageChooser));

			InitializeComponent();
			ReasonDropEdit.Visible = messageChooser.IsChangeOrCancellation;
		}

		readonly AIMMessageChooser chooser;

		protected override bool IsValidToSend()
		{
			var result = base.IsValidToSend();

			if (result)
			{
				chooser.ChooserItems.OfType<AIMMessageChooserItem>().ForEach(x => x.Validation.ValidateAll());
				chooser.Validation.ValidateAll();
				var messages = chooser.NotificationsIncludingChildren.GetMessageErrors().Select(c => c.Message).Distinct();

				if (messages.Any())
				{
					var message = Res.GetString("23ea2d1f-868f-4851-be21-02dfb61e848e",
						"It is likely that your message(s) will be rejected by Customs. Would you like to send despite these errors?{0}{0}{1}",
						System.Environment.NewLine, string.Join(System.Environment.NewLine, messages));
					result = Globals.Message.Show(message, "Error", ZMessageBoxButtons.YesNo, ZMessageBoxIcon.Warning, ZDialogResult.No) == ZDialogResult.Yes;
				}
			}

			return result;
		}
	}
}
