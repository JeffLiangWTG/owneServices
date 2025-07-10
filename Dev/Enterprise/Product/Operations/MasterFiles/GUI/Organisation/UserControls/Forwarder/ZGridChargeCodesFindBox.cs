using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ZGridChargeCodesFindBox : ZGridFindBox, ICustomizableFindBoxPopup
	{
		public ZGridChargeCodesFindBox()
		{
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			IFindBoxPopup popup = null;
			ZFilterModule module = NewModuleFromModuleID();
			if (module != null)
			{
				module.OverrideModuleDecisionProvider(new PopupModuleDecisionProviderWithMultipleSelect(this));
				popup = CreateEmbeddedPopup(module);
			}
			else
			{
				return base.GetNewPopupForm();
			}

			return popup;
		}

		public string CodeForPopup
		{
			get { return "*"; }   //Ugly hack to prevent loading filters for the popup
		}

		public string PropertyNameForPopup
		{
			get { return "*"; }
		}

		protected override void OnPopupSelected(IFindBoxPopup popup, BusinessObject[] selectedBusinessObjects)
		{
			base.OnPopupSelected(popup, selectedBusinessObjects);

			if (selectedBusinessObjects.Any())
			{
				Code = OrgProfitShareDetails.JoinChargeCodesList(selectedBusinessObjects.Cast<ICodeDescription>().Select(x => new ZString(x.Code)));
			}
		}

		protected override IEnumerable<BusinessObject> GetBizObjsToEditOrView()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			foreach (var code in ParsedCodes)
			{
				result.AddRange(IFindBox.ListProvider.GetBusinessObjectsFromCodeWithoutFilter(code));
			}

			return result;
		}

		protected List<ZString> ParsedCodes
		{
			get
			{
				if (string.IsNullOrEmpty(Code))
				{
					return new List<ZString>();
				}
				else
				{
					return OrgProfitShareDetails.SplitChargeCodesString(Code);
				}
			}
		}
	}
}
