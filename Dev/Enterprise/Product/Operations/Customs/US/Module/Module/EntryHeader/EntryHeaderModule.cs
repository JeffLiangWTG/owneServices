using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public class EntryHeaderModule : Customs.Module.EntryHeaderModule
	{
		protected override IFilterControl GetNewFilterControl() => new EntryHeaderFilterUserControl(GridCollection, (EntryHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ModuleEntryHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EntryHeaderFilterBusinessObject();

		public Form ParentModalFormOwner
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return parentModalFormOwner;
				}
#endif
				return ParentModalForm == null || ParentModalForm.Owner == null ? null : ParentModalForm.Owner;
			}
#if DEBUG
			set
			{
				if (Globals.IsTest)
				{
					parentModalFormOwner = value;
				}
			}
#endif
		}
#if DEBUG
		Form parentModalFormOwner;
#endif
	}
}
