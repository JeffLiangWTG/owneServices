using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.eTail.DataTransfer;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public class CommandJobTypeMenuGroup<TCommandJob> : BaseHVLVMenuGroup where TCommandJob : BaseHVLVRelatedJobCommand
	{
		public CommandJobTypeMenuGroup(ForwardingShipment shipment)
			: base(ResString.GetMultilingualString("012debce-b7d2-417a-a0c9-89f9bbb73bad", "Temporarily Empty"), shipment)
		{
		}

		protected override void BuildChildrenMenuItems()
		{
			command = Activator.CreateInstance(typeof(TCommandJob), shipment) as BaseHVLVRelatedJobCommand;
			Caption = command.RelatedJobName;

			createCommandMenuItem = new ZMenuItem(ResString.GetMultilingualString("f6ec1522-b387-4144-ab9e-afe87770279a", "Create {0}", command.RelatedJobName), (sender, eventArgs) =>
			{
				var form = (ZForm)ParentControl.FindForm();
				CreateCustomsRelatedBusinessObject(form, command);
			});

			OpenCommandMenuItem = new ZMenuItem(ResString.GetMultilingualString("30c63647-6cf5-492c-8b4a-a7def898acac", "Open {0}", command.RelatedJobName), (sender, eventArgs) =>
			{
				if (command.ActiveRelatedCustomsJobs.Any())
				{
					var form = (ZForm)ParentControl.FindForm();
					OpenCustomsRelatedBusinessForms(new ReadOnlyCollection<BusinessObject>(command.ActiveRelatedCustomsJobs.ToList()), form, false);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("0e3b6391-d8d4-45fc-8f65-3bac957eacdf", "No {0} has been created for Shipment {1}", command.RelatedJobName, shipment.JS_UniqueConsignRef));
				}
			});

			SyncCommandMenuItem = new ZMenuItem(ResString.GetMultilingualString("0813b17b-890f-43a2-bfc8-58671ec87b91", "Sync {0}", command.RelatedJobName), (sender, eventArgs) =>
			{
				var form = (ZForm)ParentControl.FindForm();
				CreateCustomsRelatedBusinessObject(form, command);
			});

			MenuItems.Add(createCommandMenuItem);
			MenuItems.Add(OpenCommandMenuItem);
			MenuItems.Add(SyncCommandMenuItem);
		}

		public override void UpdateVisibilityAndCaption()
		{
			createCommandMenuItem.Visible = command.CanCreate;
			OpenCommandMenuItem.Visible = command.CanOpen;
			SyncCommandMenuItem.Visible = command.CanSync;
			Visible = createCommandMenuItem.Visible || OpenCommandMenuItem.Visible || SyncCommandMenuItem.Visible;
		}

		BaseHVLVRelatedJobCommand command;
		ZMenuItem createCommandMenuItem;
		ZMenuItem OpenCommandMenuItem;
		ZMenuItem SyncCommandMenuItem;

		#region Transfer to customs jobs

		static void CreateCustomsRelatedBusinessObject(ZForm form, BaseHVLVRelatedJobCommand command)
		{
			if (UserPromptCheckingHelper.CheckCreateRelatedJob(command))
			{
				CreateCustomsRelatedBusinessObject(form, () => command.Converter);
			}
		}

		static void CreateCustomsRelatedBusinessObject(ZForm form, Func<CustomsRelatedBusinessObjectConverter> converterGetter)
		{
			using (var converter = converterGetter.Invoke())
			{
				if (!converter.TryAcquireApplicationLock(
					() => CreateCustomsRelatedBusinessObjectCore(form, converter),
					out var shipmentLockedErrorMessage))
				{
					Globals.Message.ShowError(shipmentLockedErrorMessage);
				}
			}
		}

		static void CreateCustomsRelatedBusinessObjectCore(ZForm form, CustomsRelatedBusinessObjectConverter converter)
		{
			var progressForm = new HVLVCancelableMinimisableProgressForm(form, converter.Cancel);
			progressForm.ShowModalTo(form);
			converter.SetUpProgressUpdate((string caption, string message, int progress) =>
			{
				if (progressForm.WindowState != FormWindowState.Minimized)
				{
					if (!string.IsNullOrEmpty(caption))
					{
						progressForm.Text = caption;
					}

					if (!string.IsNullOrEmpty(message))
					{
						progressForm.SetStatusAndPercentComplete(message, progress);
					}
				}
			});

			var errorMessage = string.Empty;
			converter.TryConvert(out errorMessage);

			form.BeginInvokeSafe(() =>
			{
				progressForm.Close();
			});

			if (converter.CustomsRelatedBusinessCollection != null && converter.CustomsRelatedBusinessCollection.Any())
			{
				form.BeginInvokeSafe(() =>
				{
					OpenCustomsRelatedBusinessForms(converter.CustomsRelatedBusinessCollection, form);
				});
			}
			else
			{
				Globals.Message.ShowError(errorMessage);
			}
		}

		public static void OpenCustomsRelatedBusinessForms(ReadOnlyCollection<BusinessObject> customsRelatedBusinessCollection, ZForm form, bool isNewEntity = true)
		{
			if (customsRelatedBusinessCollection.Any())
			{
				var relatedBusinessObject = customsRelatedBusinessCollection[0];

				var controller = GetControllerByBizoWithFallBack(relatedBusinessObject, form);

				if (customsRelatedBusinessCollection.Count == 1)
				{
					OpenCustomsRelatedBusinessForm(relatedBusinessObject, controller, isNewEntity);
				}
				else
				{
					var confirmToOpenAll = Res.GetString("40d4202e-e6c5-495d-acdf-826890b93c93", "{0} Cargo Reports have been generated. Do you wish to open all?", customsRelatedBusinessCollection.Count);
					if (DialogResult.Yes == Globals.Message.Show(confirmToOpenAll, ZString.Empty, MessageBoxButtons.YesNo, DialogResult.Yes))
					{
						foreach (var singleCargoReport in customsRelatedBusinessCollection)
						{
							OpenCustomsRelatedBusinessForm(singleCargoReport, controller, isNewEntity);
						}
					}
				}
			}
		}

		static ZController GetControllerByBizoWithFallBack(BusinessObject relatedBusinessObject, ZForm form)
		{
			var type = relatedBusinessObject.GetType();

			ZController controller;
			do
			{
				controller = ZControllerFactory.Instance.GetControllerForType(type);
				type = type.BaseType;
			}
			while (type != null && controller == null);
			controller.SetFormsModalTo(form);

			return controller;
		}

		static void OpenCustomsRelatedBusinessForm(BusinessObject relatedBusinessObject, ZController controller, bool isNewEntity)
		{
			using (PerformanceStatisticsCollector.StartMonitoring(nameof(OpenCustomsRelatedBusinessForm)))
			{
				var openedForm = controller.GetOpenedForm(relatedBusinessObject);
				if (openedForm != null)
				{
					openedForm.Dispose();
				}

				if (isNewEntity)
				{
					controller.ShowFormForNewEntity(relatedBusinessObject);
				}
				else
				{
					controller.ShowEditForm(relatedBusinessObject);
				}
			}
		}

		#endregion
	}
}
