using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class DuplicateAlertControlHelper
	{
		internal static (ResourceString Text, Color ForeColor) GetTextAndColor(DuplicationConditions duplicationCondition)
		{
			(ResourceString, Color) result;
			switch (duplicationCondition)
			{
				case DuplicationConditions.ShowDeduplicationStatus:
					result = (ResString.GetMultilingualString("ECF03C60-8216-4EC8-BBA9-3CB30F0A8E4A", "Detecting duplicates"), Color.Black);
					break;
				case DuplicationConditions.ShowNoDuplicatesFound:
					result = (ResString.GetMultilingualString("2403c7fd-2a04-4e7b-951b-15fcb5a0c293", "No duplicates found."), Color.DarkSeaGreen);
					break;
				case DuplicationConditions.ShowExcludedDuplicationMessage:
					result = (ResString.GetMultilingualString("82a0ac4b-8c0c-4ebd-9b57-b5905369afbf", "Excluded from De-duplication"), Color.OrangeRed);
					break;
				case DuplicationConditions.ShowNotEnoughInformation:
					result = (ResString.GetMultilingualString("9f0f1321-e4a0-4304-b28b-bd1f7deb2e40", "Not enough information to detect duplicates"), Color.DarkSeaGreen);
					break;
				case DuplicationConditions.ShowDuplicatesFound:
					result = (ResString.GetMultilingualString("4E80A2F8-9A5D-4D17-A959-2C915302D5D7", "Duplicates found"), Color.Blue);
					break;
				case DuplicationConditions.ShowDeduplicationTimeoutMessage:
					result = (ResString.GetMultilingualString("4a123af9-265d-4501-beb2-49872758dbe2", "This process has stopped due to timeout"), Color.OrangeRed);
					break;
				default:
					result = (ResString.GetMultilingualString("f3aa6234-9dc2-4d88-a513-4f3eae13a721", "An error occurred while detecting duplicates"), Color.OrangeRed);
					break;
			}

			return result;
		}

		public void ShowDuplicateAlert<T>(T supportControl, IDuplicationEventArgs e) where T : Control, ISupportDuplicationAlertControl
		{
			if (supportControl != null && !supportControl.IsDisposed)
			{
				SupportControl = supportControl;
				CloseExistingDuplicateAlert();

				if (e.Results.Any(x => x.ConfidenceRating > DeduplicationUtils.GetExcludeConfidenceRatingResult()))
				{
					var duplicateAlertControl = new DuplicateAlertControl((DuplicationEventArgs)e);
					duplicateAlertControl.DisplayDetailsAndFixes += DuplicateAlertControl_DisplayDetailsAndFixes;
					duplicateAlertControl.Name = DuplicateAlertControl;
					if (AnchorLocation != Point.Empty)
					{
						AddOverlayControl(duplicateAlertControl, ParentControl, AnchorLocation);
					}
					else
					{
						AddOverlayControl(duplicateAlertControl, ParentControl, ReferenceControl);
					}
				}
			}
		}

		void AddOverlayControl(Control control, Control parentControl, Control referenceControl, Orientation orientation = Orientation.Horizontal)
		{
			control.AdjustLocation(parentControl.FindForm(), parentControl, referenceControl, orientation);
			AddControl(control, parentControl);
		}

		void AddOverlayControl(Control control, Control parentControl, [DpiState(DpiState.ScaledVariant)] Point location)
		{
			control.Location = location;
			AddControl(control, parentControl);
		}

		void AddControl(Control control, Control parentControl)
		{
			parentControl.Controls.Add(control);
			control.BringToFront();
			parentControl.Disposed += (x, y) => { control.Dispose(); };
		}

		public void CloseExistingDuplicateAlert()
		{
			ExistingAlertControl?.Close();
		}

		internal DuplicateAlertControl ExistingAlertControl => ParentControl?.Controls.Find(DuplicateAlertControl, false).FirstOrDefault() as DuplicateAlertControl;

		void DuplicateAlertControl_DisplayDetailsAndFixes(object sender, DuplicationEventArgs e)
		{
			var isValid = true;

			if (e.Master is GlbPerson)
			{
				var factory = new BusinessObjectFactory();

				var persons = (e.TargetObjects as IEnumerable<object>).OfType<DeduplicationGlbPerson>();

				if (!persons.Any() || persons.Any(per => factory.Load<GlbPerson>(per.PK) == null))
				{
					isValid = false;
				}
			}

			if (isValid)
			{
				ZFormModaliser.Show(GetForm(e), null);
			}
			else
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("c843861f-f2a2-4fdd-9cf9-c7688c9194fa", @"Unable to view matches.

The reason is:
At least one of following matches no longer exists.

You can rectify this by doing the following:
Save and reload the form. If the problem persists, please contact {0} Support.", BrandingFactory.Instance.ProductName));
			}
		}

		protected virtual Form GetForm(DuplicationEventArgs e)
		{
			return ObjectFactory.Get<IMasterDataProviderGUI>().ShowDeduplicationResultsViewerForm(e);
		}

		public const string DuplicateAlertControl = "DuplicateAlertControl";

		#region ISupportDuplicationAlertControl

		ISupportDuplicationAlertControl SupportControl { get; set; }

		Control ParentControl => SupportControl?.DuplicationAlertParentControl;

		Control ReferenceControl => SupportControl?.DuplicationAlertReferenceControl;

		Point AnchorLocation => SupportControl.DuplicationAlertAnchorLocation;

		#endregion
	}

	internal enum DuplicationConditions
	{
		ShowDeduplicationStatus,
		ShowNoDuplicatesFound,
		ShowExcludedDuplicationMessage,
		ShowNotEnoughInformation,
		ShowDeduplicationTimeoutMessage,
		ShowDeduplicationErrorOccurredMessage,
		ShowDuplicatesFound,
	}
}
