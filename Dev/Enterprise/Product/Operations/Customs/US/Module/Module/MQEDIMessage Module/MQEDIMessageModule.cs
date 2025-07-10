using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public abstract class MQEDIMessageModule : EDIMessageModule
	{
		protected override bool ShowRequeuingMenu => false;

		protected virtual bool ShowSetToCompleteMenu => true;

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			if (ShowSetToCompleteMenu)
			{
				result.Add(new ZMenuItem("-"));
				setToCompleteMenuItem = new ZMenuItem(SetToCompleteMenuName, new EventHandler(OnSetToComplete_Click));
				result.Add(setToCompleteMenuItem);
			}
			return result.ToArray();
		}
		protected MenuItem setToCompleteMenuItem;

		void OnSetToComplete_Click(object sender, EventArgs e)
		{
			var selectedMessages = Grid.SelectedElements;

			if (selectedMessages.Length == 0)
			{
				Globals.Message.ShowWarning(SelectAtLeastOneMessage);
			}
			else
			{
				foreach (var selectedObject in selectedMessages)
				{
					SetToComplete(selectedObject.PK);
				}
			}
		}

		void SetToComplete(ZGuid messagePK)
		{
			var factory = new BusinessObjectFactory();
			var message = factory.Load<MQEDIMessage>(messagePK);
			if (message != null)
			{
				message.SetToComplete();
				factory.Save();
			}
		}

		internal const string SetToCompleteMenuName = "Set to Complete";

#if DEBUG
		/// <summary>
		/// should only be use for testing
		/// </summary>
		public MenuItem[] ContextMenuExposedForTesting
		{
			get { return ContextMenu; }
		}

		/// <summary>
		/// should only be use for testing
		/// </summary>
		public ZDisplayGrid GridExposedForTesting
		{
			get { return Grid; }
		}
#endif
	}
}
