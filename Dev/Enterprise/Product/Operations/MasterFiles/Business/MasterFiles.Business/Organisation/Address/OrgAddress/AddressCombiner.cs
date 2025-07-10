using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AddressCombiner : NonPersistentBusinessObject
	{
		public OrgAddress Address1 { get; private set; }
		public OrgAddress Address2 { get; private set; }

		public enum CombineOptions
		{
			TranslateAddress1,
			TranslateAddress2
		}

		public enum CombineResult
		{
			FailedSourceAddressHasExistingTranslatedRecords,
			FailedTargetAddressHasSameLanguageTranslatedRecords,
			Success,
			Fail
		}

		public AddressCombiner(BusinessObjectFactory factory, OrgAddress fromAddress, OrgAddress toAddress) : base(factory)
		{
			Address1 = fromAddress;
			Address2 = toAddress;
		}

		public bool CanCombine()
		{
			return Address1.IsInDatabase && Address2.IsInDatabase
			&& !(Address1.HasChanges && Address2.HasChanges)
			&& Address1.Language != Address2.Language;
		}

		public CombineResult CombineAsTranslatedAddressInBulkTransaction(CombineOptions addressToTranslate)
		{
			OrgAddress fromAddress = addressToTranslate == CombineOptions.TranslateAddress1 ? Address1 : Address2;
			OrgAddress toAddress = addressToTranslate == CombineOptions.TranslateAddress1 ? Address2 : Address1;

			if (fromAddress.TranslatedAddresses.Any())
			{
				return CombineResult.FailedSourceAddressHasExistingTranslatedRecords;
			}

			if (toAddress.TranslatedAddresses.Any(x => x.Language == fromAddress.Language))
			{
				return CombineResult.FailedTargetAddressHasSameLanguageTranslatedRecords;
			}

			var connection = ((IDbConnected)Factory).Connection;
			using (var manager = connection.BeginTransactionWithManager())
			{
				var cmd = connection.Command("XT_MoveFkReferences");// OrganisationMover uses complex SQL scripts that can't be accomplished by using Business Objects
				cmd.AddParameter("@NewParentPk", SqlDbType.UniqueIdentifier, toAddress.PK.ToGuid());
				cmd.AddParameter("@OldParentPk", SqlDbType.UniqueIdentifier, fromAddress.PK.ToGuid());
				cmd.AddParameter("@ParentTableCode", SqlDbType.Char, OrgAddressSchema.Constants.Prefix);
				cmd.AddParameter("@FkSystemLastEditUser", SqlDbType.VarChar, GlbStaff.CurrentUser?.GS_Code.ToString() ?? User.UnKnownUserCode);
				cmd.CommandType = CommandType.StoredProcedure;
				try
				{
					cmd.ExecuteNonQuery();
				}
				catch (SqlException ex)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					fromAddress.Logs.AddNew(AutoEvents.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Failed to update all foreign keys from {0} to point to {1}, rolled back", fromAddress.PK, toAddress.PK));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					manager.RollbackTransaction();
					throw new ZDataException(ex, null, connection);
				}
				manager.CommitTransaction();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				fromAddress.Logs.AddNew(AutoEvents.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Updated all foreign keys from {0} to point to {1}", fromAddress.PK, toAddress.PK));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				SetCapabilities(fromAddress, toAddress);
				CreateTranslatedAddress(fromAddress, toAddress);
				fromAddress.Delete();
			}
			return CombineResult.Success;
		}

		internal void SetCapabilities(OrgAddress fromAddress, OrgAddress toAddress)
		{
			foreach (var fromAddressCapability in fromAddress.CapabilitiesCollection)
			{
				var matchingCapability = toAddress.CapabilitiesCollection.SingleOrDefault(x => x.PZ_AddressType == fromAddressCapability.PZ_AddressType);

				if (matchingCapability != null)
				{
					if (fromAddressCapability.PZ_IsMainAddress)
					{
						fromAddress.AddressCapability.SetIsNotMainAddress(OrgAddressType.Office.Code);
						toAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
					}
				}
				else
				{
					var newCapability = Factory.New<OrgAddressCapability>();
					newCapability.PZ_OA = toAddress.PK;
					newCapability.PZ_AddressType = fromAddressCapability.PZ_AddressType;
					newCapability.PZ_IsMainAddress = fromAddressCapability.PZ_IsMainAddress;
					toAddress.CapabilitiesCollection.Add(newCapability);
				}
			}
		}

		internal OrgTranslatedAddress CreateTranslatedAddress(OrgAddress fromAddress, OrgAddress toAddress)
		{
			var translatedAddress = fromAddress.AddNewTranslatedAddress();
			translatedAddress.OTA_OA = toAddress.PK;
			translatedAddress.Language = fromAddress.Language;
			translatedAddress.Address1 = fromAddress.Address1;
			translatedAddress.Address2 = fromAddress.Address2;
			translatedAddress.City = fromAddress.City;
			translatedAddress.CompanyName = fromAddress.CompanyName;
			translatedAddress.State = fromAddress.State;
			translatedAddress.AddressMap = fromAddress.AddressMap;
			toAddress.AddressLanguagePack.Add(translatedAddress);
			return translatedAddress;
		}
	}
}
