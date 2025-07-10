using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	// Please do not add column styles in this class, because one canNOT edit grid columns in ZModuleButtonGrid via VS GUI designer.
	// May add/edit column styles in parent caller control, etc LinkedOrganizationsControl, from which one can edit grid columns via VS GUI designer.
	public partial class ZModuleButtonGridForDesigner : ZModuleButtonGrid
	{
		public event EventHandler<ModuleButtonGridOnAttachEventArgs> Attached;

		public event EventHandler<ModuleButtonGridOnAttachEventArgs> BeforeAttached;

		public ZModuleButtonGridForDesigner() : base()
		{
			BindingSource.SetBindingMember(InnerGrid, ".");
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			=> new SubZRecordAttacher(destinationCollection, findBoxList, moduleID, Attached, BeforeAttached);

		class SubZRecordAttacher : ZRecordAttacher
		{
			EventHandler<ModuleButtonGridOnAttachEventArgs> OnAttachedHandler { get; }

			EventHandler<ModuleButtonGridOnAttachEventArgs> BeforeAttachedHandler { get; }

			public SubZRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, EventHandler<ModuleButtonGridOnAttachEventArgs> onAttachedHandler, EventHandler<ModuleButtonGridOnAttachEventArgs> beforeAttachedHandler)
				: base(destinationCollection, findBoxList, moduleID)
			{
				OnAttachedHandler = onAttachedHandler;
				BeforeAttachedHandler = beforeAttachedHandler;
			}

			protected override void AttachItemsCore(IBusinessObjectCollection destinationCollection, IEnumerable<BusinessObject> list)
			{
				BeforeAttachedHandler?.Invoke(this, new ModuleButtonGridOnAttachEventArgs(list.ToArray()));
				base.AttachItemsCore(destinationCollection, list);
				OnAttachedHandler?.Invoke(this, new ModuleButtonGridOnAttachEventArgs(list.ToArray()));
			}
		}
	}
}
