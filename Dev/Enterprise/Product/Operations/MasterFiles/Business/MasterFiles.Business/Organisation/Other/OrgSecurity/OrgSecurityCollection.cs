using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using WTG.WebSecurityRight;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityCollection : DependentBusinessObjectCollection<OrgSecurity, OrgHeader>
	{
		public OrgSecurityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSecurityCollection(OrgHeader organisation)
			: base(organisation)
		{
			Factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		public void Refresh()
		{
			Load();
			FireListResetEvent();
		}

		#region Granted Rights

		/// <summary>
		/// Returns whether the given security right is granted for this organisation.
		/// If you want to know status for a particular contact, call IsRightGranted() on the OrgContact.
		/// </summary>
		/// <param name="securityRight"></param>
		/// <returns></returns>
		public bool IsRightGranted(WebSecurityRight securityRight)
		{
			bool result = true;
			foreach (OrgSecurity security in this)
			{
				if (security.SecurityKey == securityRight.Code)
				{
					result = security.OX_Granted;
					break;
				}
			}

			return result;
		}

		#endregion

		#region Rights Management

		public override void Load()
		{
			if (Master == null || !Master.IsDeleted)
			{
				base.Load();

				foreach (var right in AllWebSecurityRights.New(Factory))
				{
					OrgSecurity security = GetSecurityBySecurityRight(right);
					if (security != null)
					{
						using (security.SuspendSettingHasChanges())
						{
							security.SecurityItemNameForDisplay = right.Description;
						}
					}
					else
					{
						OrgSecurity newSecurity = AddNew();
						using (newSecurity.SuspendSettingHasChanges())
						{
							newSecurity.OX_SecurityItemName = right.SecurityItemName;
							newSecurity.OX_SU = right.SecurityGuid;
							newSecurity.OX_Granted = right.IsGrantedByDefault;
							newSecurity.SecurityItemNameForDisplay = right.Description;
						}
					}
				}

				foreach (OrgSecurity security in this.ToArray())
				{
					security.InvalidateContactSecurityRights();
				}
			}
		}

		internal OrgSecurity GetSecurityBySecurityRight(WebSecurityRight securityRight)
		{
			foreach (OrgSecurity security in this)
			{
				if (security.SecurityKey == securityRight.Code)
				{
					return security;
				}
			}

			return null;
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (Master == null || !Master.IsDeleted)
			{
				var securityReplacementList = new List<OrgSecurity>(1);

				for (int i = Count - 1; i >= 0; i--)
				{
					var securityRight = this[i];
					if (securityRight.IsInDatabase && !securityRight.IsDeleted && !securityRight.IsDifferentToDefaultRight && securityRight.NoChildrenExistWithDifferentSecurity)
					{
						var securityReplacement = GetReplacementForDeletedSecurityRight(securityRight);
						securityReplacementList.Add(securityReplacement);

						securityRight.Delete();
					}
				}

				foreach (var securityReplacement in securityReplacementList)
				{
					Add(securityReplacement);
				}
			}
		}

		protected OrgSecurity GetReplacementForDeletedSecurityRight(OrgSecurity securityToDelete)
		{
			var newSecurity = Factory.New<OrgSecurity>();
			using (newSecurity.SuspendSettingHasChanges())
			{
				newSecurity.OX_OH = securityToDelete.OX_OH;
				newSecurity.OX_SecurityItemName = securityToDelete.OX_SecurityItemName;
				newSecurity.OX_SU = securityToDelete.OX_SU;
				newSecurity.OX_Granted = securityToDelete.OX_Granted;
				newSecurity.SecurityItemNameForDisplay = securityToDelete.SecurityItemNameForDisplay;
			}

			foreach (OrgSecurityContacts contactSecurity in securityToDelete.ContactSecurityRights.ToArray())
			{
				if (contactSecurity.IsInDatabase)
				{
					var newContactSecurity = Factory.New<OrgSecurityContacts>();
					using (newContactSecurity.SuspendSettingHasChanges())
					using (newContactSecurity.GetValidationSuspender())
					{
						newContactSecurity.OZ_OC = contactSecurity.Contact.PK;
						newContactSecurity.OZ_OX = newSecurity.PK;
						newContactSecurity.OZ_Granted = contactSecurity.OZ_Granted;
						newContactSecurity.ShouldAddLogsOnFactorySaving = contactSecurity.OZ_GrantedInfo.HasChanges;
					}

					foreach (var parentCollection in ((IBusinessObjectInternals)contactSecurity).ParentCollections)
					{
						if (parentCollection is OrgSecurityContactsCollection && ((OrgSecurityContactsCollection)parentCollection).ParentContact != null)
						{
							parentCollection.Add(newContactSecurity);
						}
					}
				}
				else
				{
					using (contactSecurity.GetValidationSuspender())
					{
						securityToDelete.ContactSecurityRights.Remove(contactSecurity);
						contactSecurity.OZ_OX = newSecurity.PK;
					}
				}
			}

			return newSecurity;
		}

		#endregion

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public void SetOrgSecurities(OrgSecurityProfile orgSecurityProfile, bool shouldApplyGranted = true, bool shouldApplyIsCustomerManaged = true,
			bool shouldMaintainExistingContacts = false, bool shouldEraseExistingContacts = false)
		{
			var enableSecurityGroupsForContactsInGLOW = GlowRegistry.Instance.EnableSecurityGroupsForContactsInGLOW.Value;
			if (orgSecurityProfile != null && this.Any() && (shouldApplyGranted || shouldApplyIsCustomerManaged))
			{
				var rights = this.OfType<OrgSecurity>().GroupBy(x => x.SecurityKey).ToDictionary(x => x.Key, y => y);

				if (shouldEraseExistingContacts)
				{
					foreach (OrgContact contact in this.Master.Contacts)
					{
						_ = contact.SecurityRightsView; //so that we can override them later
					}
				}

				foreach (OrgSecurityProfileSetting setting in orgSecurityProfile.OrgSecuritySettings)
				{
					if (rights.TryGetValue(setting.SecurityKey, out var securities))
					{
						foreach (var security in securities)
						{
							var right = security.WebSecurityRight;
							if (right != null && shouldApplyGranted && !(enableSecurityGroupsForContactsInGLOW && WebSecurityApplication.GlowWeb == right.WebApplication))
							{
								if (shouldMaintainExistingContacts)
								{
									if (security.OX_Granted != setting.Granted)
									{
										var dict = new Dictionary<ZGuid, bool>();

										//before we change OX_Granted (which will change all OZ_Granteds in ContactSecurityRights), learn what those values are
										foreach (OrgSecurityContacts contactRight in security.ContactSecurityRights)
										{
											dict.Add(contactRight.OZ_OC, contactRight.OZ_Granted);
										}

										security.OX_Granted = setting.Granted;

										//now set memorized values
										foreach (OrgSecurityContacts contactRight in security.ContactSecurityRights)
										{
											contactRight.OZ_Granted = dict[contactRight.OZ_OC];
										}
									}
								}
								else if (shouldEraseExistingContacts)
								{
									security.OX_Granted = setting.Granted;
									foreach (OrgSecurityContacts securityContact in security.ContactSecurityRights)
									{
										securityContact.OZ_Granted = setting.Granted && securityContact.Contact.OC_WebAccessEnabled;
									}
								}
								else
								{
									if (security.OX_Granted != setting.Granted)
									{
										security.OX_Granted = setting.Granted;
									}
								}
							}

							if (shouldApplyIsCustomerManaged)
							{
								security.OX_IsCustomerManaged = setting.CustomerManaged;
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
