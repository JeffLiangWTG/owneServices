using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	/// <summary>
	/// Custom IModuleDecisionProvider.
	/// - The default decision provider will open the record on double click, instead of selecting it.
	/// - The PopupModuleDecisionProvider has the correct behaviour, but requires an IFindBox.
	/// </summary>
	public class SingleSelectModuleDecisionProvider : IModuleDecisionProvider
	{
		public SingleSelectModuleDecisionProvider(IBusinessObjectCollection list = null)
		{
			List = list;
		}

		public Form Popup { get; set; }
		public BusinessObject SelectedBusinessObject { get; private set; }

		public bool ShouldDisplayNotifications => true;
		public bool ShouldLoadFilterBizObj => false;
		public bool ShouldSaveFilterBizObj => false;
		public bool ShouldIgnoreAdditionalFilter => true;
		public bool AllowExcelExport => false;
		public bool EnablePreviousNextSupport => false;
		public IBusinessObjectCollection List { get; private set; }

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
		{
			if (selectedBusinessObjects.Length == 1)
			{
				var bizo = selectedBusinessObjects[0];
				if (!bizo.HasRowErrors)
				{
					SelectedBusinessObject = selectedBusinessObjects[0];
					Popup?.Close();
				}
				else
				{
					Globals.Message.ShowError(bizo.RowErrors.First().Message);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("8A6808AE-0A74-4784-A3DE-0649B130D011", "Please select one item."));
			}
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObjects)
		{
			HandleDefaultAction(selectedBusinessObjects);
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters) { }
		public void InitialiseFindBoxControllerLink(ZController controller) { }
		public void SetFindBoxCodeDescription(BusinessObject bizo) { }
	}
}
