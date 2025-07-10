using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class InternalCartageManager : CartageManager
	{
		public InternalCartageManager(CartageType cartageType)
		{
			this.cartageType = cartageType;
		}

		protected ZController GetNewController()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Cartage);
			if (LicenceCheckpointForViewOverride != null)
			{
				controller.LicenceCheckpointForViewOverride = LicenceCheckpointForViewOverride;
			}

			return controller;
		}

		public void CreateCartageAndShow(INotifications notify, Form modalForm, MethodInvoker afterFormClosed)
		{
			var buffer = new NotificationBuffer(notify);
			PreCreateCartageCheck(buffer);

			if (!buffer.HasErrors)
			{
				var controller = GetNewController();
				var cartageToBeDeactivated = FindCartage();
				CreateCartageInDifferentFactoryForTest();
				if (cartageToBeDeactivated != null && !DeactivateExisting(buffer, cartageToBeDeactivated, controller.Factory))
				{
					return;
				}

				if (!buffer.HasErrors)
				{
					if (modalForm != null)
					{
						controller.SetFormsModalTo(modalForm);
					}

					Cartage = (CommonCartage)controller.Factory.New(controller.TypeOfTopLevelBusinessObject);
					InternalCartageManagerHelper.PopulateCartage(Cartage, CartageType);
					CartageType.CartageAdvised(Cartage.Factory);

					Cartage.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);

#if DEBUG
					// this is very ugly but it's the only way I can see to get the unit test in CartagePlugin for saving a new cartage to work successfully (TestCartageOrgChangeCreatesNewCartageJobIfNoSaveInProgress())
					if (Globals.IsTest && DoNotShowFormOnCreateCartageTestOnly)
					{
						JustSaveAfterCartageCreated();
					}
					else
#endif
					if (TransportRegistry.Instance.ShowPortTransportJobOnCreation.Value)
					{
						PopPortTranportCreationForm(controller, cartageToBeDeactivated, afterFormClosed);
					}
					else
					{
						var result = Globals.Message.Show(Res.GetString("CC9E4D99-7B60-454D-9AFE-CE150807CA5B", "Port Transport job has been created would you like to view it?"), Res.GetString("E1E01FF1-DD7E-4084-B806-4755601345E4", "Create Port Transport"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
						if (result == DialogResult.Yes)
						{
							PopPortTranportCreationForm(controller, cartageToBeDeactivated, afterFormClosed);
						}
						else
						{
							JustSaveAfterCartageCreated();
						}
					}
				}

				void JustSaveAfterCartageCreated()
				{
					Cartage.Factory.Save();
					buffer.Notify(new InfoNotification(Res.GetString("a6998796-d208-42b6-a420-9dcc61905aa3", "Port Transport Job created Successfully.")));
					afterFormClosed.Invoke();
				}
			}
		}

		internal void PopPortTranportCreationForm(ZController controller, CommonCartage cartageToBeDeactivated, MethodInvoker afterFormClosed)
		{
			var form = (CartageForm)controller.ShowFormForNewEntity(Cartage);
			if (form != null)
			{
				if (cartageToBeDeactivated != null)
				{
					form.CartageToBeDeactivated = cartageToBeDeactivated;
				}
				Cartage.HasChanges = true;
				form.Closed += delegate
				{ afterFormClosed(); };
#if DEBUG
				cartageForm = form;
#endif
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (!cartageHasBeenCreatedAndSaved)
			{
				cartageHasBeenCreatedAndSaved = true;
				CartageType.CartageParent.CartageCreatedAndSaved();
			}
		}
		bool cartageHasBeenCreatedAndSaved;

		public bool DeactivateExisting(INotifications notify, CommonCartage commonCartage, BusinessObjectFactory factoryToDeactivateIn)
		{
			var result = false;

			var cartageController = (ICartageController)GetNewController();
			if (cartageController.IsFormOpen(commonCartage))
			{
				var errorMsg = Res.GetString("InternalCartageManager|DeactivateExisting|JobScreenOpen", "The existing {0} Local Transport Job is open on another screen. Close this screen before attempting to replace it.", CartageType.Description);
				notify.Notify(new ErrorNotification(ErrorType.Error, errorMsg));
			}
			else
			{
				var jobs = (JobHeader[])factoryToDeactivateIn.Load<IJobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, commonCartage.PK));
				var jobDeactivationErrors = new ZStringBuilder();
				foreach (var job in jobs)
				{
					var canCancelReason = job.ReasonForNotAbleToDeactivate;
					if (!canCancelReason.IsEmpty)
					{
						jobDeactivationErrors.Append(canCancelReason);
					}
				}

				if (jobDeactivationErrors.Length > 0)
				{
					var errorMsg = Res.GetString("b225931d-269f-4cd8-8450-3a82408ba1e3", "The existing {0} Port Transport (with parent job number {1}) has been found. It cannot be deactivated because Job(s) attached cannot be deactivated. Reasons being:", CartageType.Description, CartageType.CartageParent.UniqueConsignmentID);
					errorMsg += " \r\n";
					errorMsg += jobDeactivationErrors.ToStringWithNewLineBetweenAppends();
					notify.Notify(new ErrorNotification(ErrorType.Error, errorMsg));
				}
				else if (commonCartage.HasJobAlreadyCommenced && !commonCartage.JJ_IsCancelled)
				{
					var errorMsg = Res.GetString("2e3de724-67b2-4a62-a80a-c041b7b866e5", "The existing {0} Port Transport (with parent job number {1}) has been found. It has already commenced and is not canceled, therefore cannot be overridden.", CartageType.Description, CartageType.CartageParent.UniqueConsignmentID);
					notify.Notify(new ErrorNotification(ErrorType.Error, errorMsg));
				}
				else if (commonCartage.IsCancelled)
				{
					// cartage has already been cancelled, just return true
					result = true;
				}
				else
				{
					var question = Res.GetString("b3ff4f6b-ca21-4f85-96dc-93896ac43e28", "The existing {0} Port Transport (with parent job number {1}) has been found, are you sure you want to override it?", CartageType.Description, CartageType.CartageParent.UniqueConsignmentID);
					var queryUserargs = new QueryUserYesNoEventArgs(Res.GetString("3e821e9a-e76e-4fca-b355-525fefa6ec45", "Existing Port Transport Found"), question, false);
					notify.QueryUser(queryUserargs);

					if (queryUserargs.Response)
					{
						var cartageInControllerFactory = factoryToDeactivateIn.Load<CommonCartage>(commonCartage.PK);
						if (cartageInControllerFactory == null || cartageInControllerFactory.IsCancelled)
						{
							string errorMsg = Res.GetString("InternalCartageManager|AnotherUserOverrideV2",
								"The existing {0} Port Transport (with parent job number {1}) has been overridden by another user. Close and Open this job before attempting to create the Port Transport Job again.", CartageType.Description, CartageType.CartageParent.UniqueConsignmentID);
							notify.Notify(new ErrorNotification(ErrorType.Error, errorMsg));
						}
						else
						{
							if (cartageInControllerFactory.CartageParent == null)
							{
								throw new InvalidOperationException("Port TransportParent doesn't exist in new factory");
							}

							var parent = cartageInControllerFactory.CartageParent;
							cartageInControllerFactory.IsCancelled = true;

							foreach (var job in jobs)
							{
								job.MarkAsInactive();
							}

							InternalCartageManagerHelper.LogCartageDeactivationOnParent(cartageInControllerFactory);

							result = true;
						}
					}
				}
			}

			return result;
		}

		public IZForm ViewCartage(INotifications notify, Form modalForm)
		{
			var commonCartage = FindCartage();
			IZForm form = null;
			if (commonCartage != null)
			{
				var controller = GetNewController();
				controller.SetFormsModalTo(modalForm);
				form = controller.ShowEditForm(commonCartage);
			}
			else
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("f1f816f9-f1e6-4ce9-bb4d-1f9bd4b45c8d", "There are no Port Transport Jobs with parent job number {0}, it has probably been deleted.", CartageType.CartageParent.UniqueConsignmentID)));
			}

			return form;
		}

		void PreCreateCartageCheck(NotificationBuffer buffer, bool cartageCreatedForExport = false)
		{
			if (!cartageCreatedForExport)
			{
				CheckSecurity(buffer);
			}

			if (!buffer.HasErrors)
			{
				CheckSaved(buffer);
			}

			if (!buffer.HasErrors)
			{
				CheckCartageOrganisation(buffer);
			}

			if (!buffer.HasErrors)
			{
				CheckCartageAddressRequirements(buffer);
			}
		}

		void CheckSecurity(NotificationBuffer buffer)
		{
			var controller = GetNewController();
			var checkPoint = controller.GetCheckPointForNew(null);
			if (!checkPoint.IsAllowed)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, checkPoint.ErrorMessageForNotAllowed));
			}
		}

		void CheckSaved(NotificationBuffer buffer)
		{
			if (!IsParentSaved)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("06e3f304-902e-49c7-8434-5c580bbd2288", "Please save this {0} before creating a Port Transport Job.", CartageType.CartageParent.HumanReadableName)));
			}
		}

		void CheckCartageOrganisation(NotificationBuffer buffer)
		{
			ZStringBuilder errors = new ZStringBuilder();
			bool isNull = CartageType.LocalTransportProviderAddress == null;
			bool isTransport = CartageType.LocalTransportProviderAddress != null && CartageType.LocalTransportProviderAddress.Header.OH_IsLocalTransport;

			if (isNull)
			{
				errors.Append(Res.GetString("20943c87-963b-4340-a3f0-72213f2ea1a2", "No Local Transport Organization has been entered for this Port Transport."));
			}

			if (isNull || !isTransport)
			{
				errors.Append(Res.GetString("71168c11-7304-40e6-bf5f-594a7d7712b6", "The Local Transport Organization entered must be set up as a Local Transport Organization (Organization -> Carrier -> Local Transport)."));
			}

			if (!errors.IsEmpty)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, errors.ToStringWithNewLineBetweenAppends()));
			}
		}

		void CheckCartageAddressRequirements(NotificationBuffer buffer)
		{
			ZStringBuilder errors = new ZStringBuilder();
			ZString jobType = CartageType.CartageJobType;
			CommonCartageType cartageJobType = CartageType.CartageParent.Factory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, jobType));

			if (cartageJobType != null)
			{
				foreach (CommonCartageOrg org in cartageJobType.CommonCartageOrganisations)
				{
					if (!NonMandatoryAddressRequirementOrgTypes.Contains(org.E5_OrgType))
					{
						JobDocAddress docAddress = CartageType.GetCartageAddress(org.E5_OrgType);
						if (docAddress == null || docAddress.IsEmpty)
						{
							errors.Append(Res.GetString("a73cac8a-03dc-4866-b61d-a8f915cc93e0", "The Address '{0}' has not been entered. It is mandatory for a Port Transport of type '{1}'.", org.E5_OrgType, cartageJobType.E3_DescriptionMultilingual));
						}
					}
				}
			}
			else
			{
				errors.Append(Res.GetString("B8398349-3B2F-4BE1-81FE-E8F1F0D73660", "Invalid Port Transport Job Type: '{0}'.", jobType));
			}
			if (!errors.IsEmpty)
			{
				buffer.Notify(new ErrorNotification(ErrorType.Error, errors.ToStringWithNewLineBetweenAppends()));
			}
		}

		List<ZString> NonMandatoryAddressRequirementOrgTypes
		{
			get
			{
				if (nonMandatoryAddressRequirementOrgTypes == null)
				{
					nonMandatoryAddressRequirementOrgTypes = new List<ZString>();
					nonMandatoryAddressRequirementOrgTypes.Add(LocalCartageJobOrgTypeList.Codes.MSC);
					nonMandatoryAddressRequirementOrgTypes.Add(LocalCartageJobOrgTypeList.Codes.CYD);
				}
				return nonMandatoryAddressRequirementOrgTypes;
			}
		}
		List<ZString> nonMandatoryAddressRequirementOrgTypes;

		public LicenceCheckpoint LicenceCheckpointForViewOverride;

		public CartageType CartageType
		{
			get { return cartageType; }
		}
		readonly CartageType cartageType;

		public CommonCartage FindCartage()
		{
			return CartageHelper.FindCartage(CartageType);
		}

		protected override OrgHeader SendTo
		{
			get { return CartageType.LocalTransportProviderAddress != null ? CartageType.LocalTransportProviderAddress.Header : null; }
		}

		protected override ZString SendToDescription
		{
			get { return Res.GetString("ad59a0a0-9a5c-4eaf-83e1-5084783b800b", "{0} Local Transport Company", Description); }
		}

		protected override ZString Description
		{
			get { return CartageType.Description; }
		}

		protected override ZString ParentJobNumber
		{
			get { return CartageType.CartageParent.UniqueConsignmentID; }
		}

		protected override CommonCartage GetCartageForExport(NotificationBuffer buffer)
		{
			var commonCartage = FindCartage();
			if (commonCartage == null)
			{
				PreCreateCartageCheck(buffer, true); // bypass security and license as this cartage will not be saved (is temp for export only)

				if (!buffer.HasErrors)
				{
					ZController controller = GetNewController();
					((IBusinessObjectFactoryInternals)controller.Factory).CanSave = false;
					commonCartage = controller.Factory.New<CommonCartage>();
					InternalCartageManagerHelper.PopulateCartage(commonCartage, CartageType);
				}
			}

			return commonCartage;
		}

		protected override BusinessObjectFactory ParentFactory
		{
			get { return CartageType.CartageParent.Factory; }
		}

		protected override Logs ParentLogs
		{
			get { return ((IStmALogParent)CartageType.CartageParent).Logs; }
		}

		protected override Notes ParentNotes
		{
			get { return ((IStmNoteParent)CartageType.CartageParent).Notes; }
		}

		protected override bool IsParentSaved
		{
			get { return CartageType.CartageParent.IsInDatabase && !CartageType.CartageParent.HasChanges; }
		}

		protected override CommonCartageType CartageJobType
		{
			get { return !CartageType.CartageJobType.IsEmpty ? ParentFactory.LoadTop1<CommonCartageType>(new ZQuery(LocalCartageJobTypeSchema.E3_JobType, CartageType.CartageJobType)) : null; }
		}

		protected override void CartageAdvised(BusinessObjectFactory factoryToCartageAdviseIn)
		{
			CartageType.CartageAdvised(factoryToCartageAdviseIn);
		}

#if DEBUG
		internal CartageForm cartageForm;
		public bool DoNotShowFormOnCreateCartageTestOnly { get; set; }
#endif
		internal CommonCartage Cartage;
		partial void CreateCartageInDifferentFactoryForTest();
	}
}

#if DEBUG

namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class InternalCartageManager
	{
		partial void CreateCartageInDifferentFactoryForTest()
		{
			if (CreateCartageForParentAndSave != null)
			{
				CreateCartageForParentAndSave(this, null);
			}
		}
		public EventHandler CreateCartageForParentAndSave;
	}
}

#endif
