using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class GlbStaffPopupModuleHelper
	{
		public GlbStaff GetStaffFromPopupModule(BusinessObjectFactory factory, Form parentForm)
		{
			ReplacementStaffCode = string.Empty;
			var staffModule = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff);
			var staffPopup = new EmbeddedModulePopup(staffModule);

			IFindBox findBox = new FindBox(
				new GlbStaffCollection(factory),
				staffPopup,
				delegate(string code)
				{
					ReplacementStaffCode = code;
				}
			);

			var provider = staffModule.GetModuleDecisionProviderForFindBoxPopup(findBox);
			staffModule.OverrideModuleDecisionProvider(provider);
			staffPopup.EmbeddedModulePopupOKButtonStrategy = provider;
			ZFormModaliser.ShowDialogAndDispose(staffPopup, parentForm);

			return factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, ReplacementStaffCode);
		}

		protected virtual ZString ReplacementStaffCode
		{
			get;
			set;
		}

		#region FindBox Class

		internal delegate void SetCodeHandler(string code);

		internal class FindBox : IFindBox
		{
			readonly IFindBoxListProvider listProvider;
			readonly IFindBoxPopup popupForm;
			readonly SetCodeHandler setCodeHandler;

			internal FindBox(IFindBoxListProvider listProvider, IFindBoxPopup popupForm, SetCodeHandler setCodeHandler)
			{
				this.listProvider = listProvider;
				this.popupForm = popupForm;
				this.setCodeHandler = setCodeHandler;
			}

			#region IFindBox Members

			string IFindBox.Code
			{
				get => string.Empty;
				set => setCodeHandler(value);
			}

			string IFindBox.Description
			{
				get => string.Empty;
				set { }
			}

			IFindBoxListProvider IFindBox.ListProvider => listProvider;
			IFindBoxPopup IFindBox.PopupForm => popupForm;

			#endregion
		}

		#endregion
	}
}
