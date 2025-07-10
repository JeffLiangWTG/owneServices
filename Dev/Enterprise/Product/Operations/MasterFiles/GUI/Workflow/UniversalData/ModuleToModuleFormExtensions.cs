using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.FormExtensions
{
	public interface IModuleToModuleForm<TBusinessObject> where TBusinessObject : BusinessObject, IWorkflowProvider, IModuleToModule
	{
		bool HasChanges { get; }
		ZString ErrorMessage { get; }
		ResourceString WasExportedNotificationMessage { get; }

		TBusinessObject BusinessEntity { get; }
		ResourceString TopLevelMenuItemCaption { get; }
		ResourceString MainMenuItemCaption { get; }
		ZString RelatedEntityName { get; }
		ZString RelatedEntityDescription { get; }
		ControllerID ControllerID { get; }

		IModuleToModuleSender GetModuleToModuleSender();
	}

	public static class ModuleToModuleFormExtensions
	{
		public static ZMenuItem CreateRelatedJobsMainMenuItem<TZForm, UBusinessObject>(this TZForm form)
			where TZForm : ZForm, IModuleToModuleForm<UBusinessObject>
			where UBusinessObject : BusinessObject, IWorkflowProvider, IModuleToModule
		{
			var menuItem = new ZMenuItem(form.MainMenuItemCaption);
			var relatedJobsMenuItem = new ZMenuItem(form.TopLevelMenuItemCaption, new[] { menuItem });
			relatedJobsMenuItem.Popup += (sender, e) => SetupCreateMenuItem(form, menuItem);
			form.Menu.MenuItems.Add(relatedJobsMenuItem);

			return relatedJobsMenuItem;
		}

		static void SetupCreateMenuItem<UBusinessObject>(IModuleToModuleForm<UBusinessObject> form, ZMenuItem menuItem)
			where UBusinessObject : BusinessObject, IWorkflowProvider, IModuleToModule
		{
			var businessEntity = form.BusinessEntity;

			foreach (MenuItem item in menuItem.MenuItems)
			{
				item.Dispose();
			}

			menuItem.MenuItems.Clear();
			menuItem.Enabled = businessEntity != null;
			if (menuItem.Enabled)
			{
				ZMenuItem item;
				BusinessObject relatedObject = businessEntity.GetRelatedObject();
				var description = form.RelatedEntityDescription;
				var name = form.RelatedEntityName;
				if (relatedObject != null)
				{
					item = new ZMenuItem(ResString.GetMultilingualString("A2CFC1AB-081E-48B2-B0AD-710C7ED7A581", "{0} View {1}", description, name),
						(sender, e) => ShowForm(relatedObject, form.ControllerID));
				}
				else
				{
					item = new ZMenuItem(ResString.GetMultilingualString("1F242FC6-B764-4880-8718-F52D708A1E57", "{0} Create {1}", description, name),
						(sender, e) => OnCreateObject_Click(form));
				}
				menuItem.MenuItems.Add(item);
			}
		}

		static void OnCreateObject_Click<UBusinessObject>(IModuleToModuleForm<UBusinessObject> form)
			where UBusinessObject : BusinessObject, IWorkflowProvider, IModuleToModule
		{
			if (form.HasChanges)
			{
				var caption = Res.GetString("38796160-3FDF-4DA9-95AE-EC193C398597", "Create {0}", form.RelatedEntityName);
				Globals.Message.ShowError(form.ErrorMessage, caption);
			}
			else
			{
				var businessEntity = form.BusinessEntity;
				var publishResult = form.GetModuleToModuleSender().CreateJob(businessEntity);

				switch (publishResult.ResultType)
				{
					case UniversalResult.Internal:
						var loadedJob = publishResult.FindJobIfExists();
						if (loadedJob != null)
						{
							businessEntity.AddToRelatedJobs(loadedJob);
							ShowForm(loadedJob, form.ControllerID);
						}
						break;
					case UniversalResult.External:
						var caption = Res.GetString("42676134-7634-4EFB-AE19-9BA2659E740F", "Create {0}", form.RelatedEntityName);
						Globals.Message.ShowInformation(form.WasExportedNotificationMessage, caption);
						break;
					case UniversalResult.HadErrors:
						Globals.Message.ShowError(publishResult.ErrorMessage, Res.GetString("7EE07FE7-460D-419F-AA1A-1B08D6C7B99A", "Create {0}", form.RelatedEntityName));
						break;
					default:
						throw new NotSupportedException(string.Format("Publish result type {0} not supported.", publishResult.ResultType.ToString()));
				}
			}
		}

		static void ShowForm(BusinessObject businessObject, ControllerID controllerID)
		{
			var controller = ZControllerFactory.Create(controllerID);

			#region Testing
#if DEBUG
			LastFormShownForTesting =
#endif
			#endregion

			controller.ShowEditForm(businessObject);
		}

		#region LastFormShownForTesting
#if DEBUG
		internal static IZForm LastFormShownForTesting;
#endif
		#endregion

	}
}
