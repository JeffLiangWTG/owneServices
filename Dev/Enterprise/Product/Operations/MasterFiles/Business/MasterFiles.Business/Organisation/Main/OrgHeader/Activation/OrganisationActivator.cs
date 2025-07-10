using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public static class OrganisationActivator
	{
		public delegate IDisposable ActivatingHeaderAction(OrgHeader org);
		public delegate void FailedToActivateHeaderAction(IDisposable form);
		public delegate void ActivateOrDeactivateNotificationHeaderAction(INotification notification);
		public delegate bool ScreenStatusChangedWarningHeaderAction();

		/// <summary>
		/// Activates or De-activates organizations.
		/// </summary>
		/// <param name="orgPKs">Array of primary keys of organizations to process</param>
		/// <returns>Error message if any.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public static List<OrgHeader> ActivateOrDeactivate(ZGuid[] orgPKs, bool activate, ActivatingHeaderAction activatingHeaderAction = null, FailedToActivateHeaderAction failedToActivateHeaderAction = null, ActivateOrDeactivateNotificationHeaderAction notificationHeaderAction = null, ScreenStatusChangedWarningHeaderAction screenStatusChangedWarningHeaderAction = null)
		{
			OrgHeader[] organisations;
			var factory = new BusinessObjectFactory();
			var organisationsToProceed = new List<OrgHeader>();

			if (Env.Security.OrgDetailsModifyIsActiveOrg.IsAllowed)
			{
				organisations = factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, orgPKs));
				if (orgPKs.Length == 0 || organisations.Length == 0)
				{
					NotifyUserIfRequired(new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("e792c970-e1ff-4f39-bca1-d189223475f7", "You have not selected any Organizations.")), notificationHeaderAction);
				}
				else
				{
					organisationsToProceed = processOrganisations(organisations, activate, activatingHeaderAction, failedToActivateHeaderAction, notificationHeaderAction, screenStatusChangedWarningHeaderAction);
				}
			}
			else
			{
				NotifyUserIfRequired(new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("5189A2C8-610D-402f-A9D8-41F4EB2F6B2C", "You do not have rights to activate/deactivate organizations.")), notificationHeaderAction);
			}

			return organisationsToProceed;
		}

		static void NotifyUserIfRequired(INotification notification, ActivateOrDeactivateNotificationHeaderAction notificationHeaderAction)
		{
			if (notification != null && notificationHeaderAction != null)
			{
				notificationHeaderAction(notification);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		static List<OrgHeader> processOrganisations(OrgHeader[] organisations, bool activate, ActivatingHeaderAction activatingHeaderAction, FailedToActivateHeaderAction failedToActivateHeaderAction, ActivateOrDeactivateNotificationHeaderAction notificationHeaderAction, ScreenStatusChangedWarningHeaderAction screenStatusChangedWarningHeaderAction)
		{
			var organisationsToProceed = new List<OrgHeader>();
			var message = ZString.Empty;
			var proxyComMessage = new ZStringBuilder();
			var deactivatedOrgPKs = new List<Guid>();
			var orgsWithValidationErrors = new List<ZString>();
			var formsWithValidationErrors = new List<IDisposable>();
			var orgsWithActiveTransactions = new List<ZString>();
			var activeTransactionDetailsString = ZString.Empty;
			var hasDeactivatedSystemOrg = false;
			INotificationType type = null;
			var success = false;

			try
			{
				foreach (OrgHeader org in organisations)
				{
					var oldActiveValue = org.OH_IsActive;
					if (!org.IsSystemDefinedOrganisation)
					{
						if (!activate)
						{
							var collection = org.CompanyData.GetActiveTransactionDetails(true);
							if (collection.Count > 0)
							{
								orgsWithActiveTransactions.Add(org.OH_Code);
								activeTransactionDetailsString = org.CompanyData.ConvertActiveTransactionCollectionToString(collection);
								continue;
							}

							if (!org.IsProxyOrgOfAnyCompany())
							{
								deactivatedOrgPKs.Add(org.PK.ToGuid());
								organisationsToProceed.Add(org);
								org.HasDeactivatedEntity = true;
								org.OH_IsActive = activate;
							}
							else
							{
								var companyList = org.CompanyProxies(false).First().GC_Code;
								proxyComMessage.AppendLine((Res.GetString("4DCE5838-A929-415A-BD29-D93B96E1897E", "{0} is an organization proxy for {1} Company.", org.HumanReadableName, companyList)));
							}
						}
						else
						{
							organisationsToProceed.Add(org);
							org.OH_IsActive = activate;
						}
					}
					else if (!activate)
					{
						hasDeactivatedSystemOrg = true;
					}

					if (activate && !oldActiveValue)
					{
						IDisposable form = null;
						if (activatingHeaderAction != null)
						{
							form = activatingHeaderAction(org);
						}
						else
						{
							org.Validation.ValidateAll();
						}

						if (org.HasErrors)
						{
							orgsWithValidationErrors.Add(org.OH_Code);
							if (form != null)
							{
								formsWithValidationErrors.Add(form);
							}
							org.OH_IsActive = false;
							organisationsToProceed.Remove(org);
						}
					}
				}

				try
				{
					if (activate)
					{
						if (orgsWithValidationErrors.Count > 0)
						{
							message = Res.GetString(
								"2cc47907-ae49-474c-ad44-2f503cc82e82", @"Selected Organizations are Activated except Organizations ({0}), as they have validation errors.",
								orgsWithValidationErrors.Aggregate((x, y) => x + ", " + y));
							type = CargoWise.EntityFramework.NotificationType.Information;
							if (failedToActivateHeaderAction != null)
							{
								foreach (var form in formsWithValidationErrors)
								{
									failedToActivateHeaderAction(form);
								}
							}
						}
						else
						{
							message = Res.GetString("c138d61b-93bd-4d04-8b43-56fc92da0069", "Selected Organizations are Activated.");
							type = CargoWise.EntityFramework.NotificationType.Information;
						}
					}
					else
					{
						if (deactivatedOrgPKs.Count > 0)
						{
							if (orgsWithActiveTransactions.Count == 0 && proxyComMessage.IsEmpty)
							{
								message = Res.GetString("8c408e64-2c40-473f-ac2c-08382bfea952", "Selected Organizations are De-activated.");
								type = CargoWise.EntityFramework.NotificationType.Information;
							}
							else if (orgsWithActiveTransactions.Count == 1)
							{
								message = Res.GetString("2adf4607-5231-4e76-864b-9b2fc1f1f0c1", @"Selected Organizations are De-activated except organization {0} as there are active AR and/or AP transactions in the following system companies.
{1}", orgsWithActiveTransactions.First(), activeTransactionDetailsString);
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
							else if (!proxyComMessage.IsEmpty)
							{
								message = proxyComMessage.ToString();
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
							else
							{
								message = Res.GetString("3e0a8ea5-b933-4eac-8a3f-a95913618fc3", @"Selected Organizations are De-activated except these Organizations because active transactions still exist for them: {0}.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided.", string.Join(", ", orgsWithActiveTransactions));
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
						}
						else
						{
							if (orgsWithActiveTransactions.Count == 1)
							{
								message = Res.GetString("d9381999-96b1-4d63-bc73-10e471f347f8", @"You cannot De-activate this organization as there are active AR and/or AP transactions in the following system companies.
{0}", activeTransactionDetailsString);
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
							else if (!proxyComMessage.IsEmpty)
							{
								message = proxyComMessage.ToString();
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
							else
							{
								message = Res.GetString("5337e729-58f2-4a0b-92f2-36c121dca98e", @"These Organizations were not De-activated because active transactions still exist for them: {0}.
For a list of system companies with active transactions, please De-activate each organization separately and the list will be provided.", string.Join(", ", orgsWithActiveTransactions));
								type = CargoWise.EntityFramework.NotificationType.Error;
							}
						}
					}
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				success = true;
			}
			finally
			{
				if (!success)
				{
					//if we don't go here - all forms will be displayed. So they can be disposed when they're closed.
					foreach (IDisposable form in formsWithValidationErrors)
					{
						form.Dispose();
					}
				}
			}

			if (hasDeactivatedSystemOrg)
			{
				message += string.IsNullOrEmpty(message) ? string.Empty : System.Environment.NewLine;
				message += Res.GetString("2cf4cb1e-3519-46b9-98b2-376ac98afe7d", "Some of the selected Organizations were not De-activated because you cannot De-activate System Defined Organization.");
				type = CargoWise.EntityFramework.NotificationType.Error;
			}

			if (type != null && !string.IsNullOrEmpty(message))
			{
				if (organisationsToProceed.Any())
				{
					if (screenStatusChangedWarningHeaderAction == null || screenStatusChangedWarningHeaderAction())
					{
						NotifyUserIfRequired(new Notification(type, message), notificationHeaderAction);

						foreach (var org in organisationsToProceed)
						{
							org.Factory.Save();
						}
					}
					else
					{
						organisationsToProceed.Clear();
					}
				}
				else
				{
					NotifyUserIfRequired(new Notification(type, message), notificationHeaderAction);
				}
			}

			return organisationsToProceed;
		}
	}
}
