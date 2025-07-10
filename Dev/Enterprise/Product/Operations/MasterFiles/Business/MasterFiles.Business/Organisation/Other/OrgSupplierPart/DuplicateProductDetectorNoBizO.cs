using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.Data;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class DuplicateProductDetectorNoBizO
	{
		public DuplicateProductDetectorNoBizO(ZGuid opPk, ZString op_partNum, bool isActive, IEnumerable<RelatedPartyWithCode> partiesOnCurrentPart, IEnumerable<PartAndFriends> otherPartsToConsiderNotYetInDb)
		{
			currentPartPK = opPk;
			isCurrentPartActive = isActive;
			Initialise(op_partNum, partiesOnCurrentPart, otherPartsToConsiderNotYetInDb);
		}

		void Initialise(string op_partNum, IEnumerable<RelatedPartyWithCode> partiesOnCurrentPart, IEnumerable<PartAndFriends> otherPartsToConsiderNotYetInDb)
		{
			partsAndFriends = new List<PartAndFriends>();
			var sqlToLoadEverything = @"	select OP_PK, OU_Relationship, OH_Code, OP_IsActive from dbo.OrgSupplierPart
											inner join dbo.OrgPartRelation   on OU_OP = OP_PK
											inner join dbo.OrgHeader   on OU_OH = OH_PK
											where OP_PartNum = @partNum and OP_PK != @opPk";

			var partsInFactory = new HashSet<ZGuid>();
			if (otherPartsToConsiderNotYetInDb != null)
			{
				foreach (var rel in otherPartsToConsiderNotYetInDb)
				{
					partsInFactory.Add(rel.OPPK);
				}
			}

			using (var cmd = Db.Connection.Command(sqlToLoadEverything))
			{
				cmd.AddParameter("partNum", System.Data.SqlDbType.VarChar, op_partNum);
				cmd.AddParameter("opPk", System.Data.SqlDbType.UniqueIdentifier, currentPartPK.IsEmpty ? Guid.Empty : currentPartPK.ToGuid());
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var relatedPartPK = new ZGuid(reader[0]);
						if (!partsInFactory.Contains(relatedPartPK))
						{
							partsAndFriends.Add(new PartAndFriends(relatedPartPK, (string)reader[1], (string)reader[2], (bool)reader[3]));
						}
					}
				}
			}

			foreach (var p in partiesOnCurrentPart)
			{
				partsAndFriends.Add(new PartAndFriends(currentPartPK, p.RelationshipCode, p.RelatedOrganisationCode, isCurrentPartActive));
			}

			if (otherPartsToConsiderNotYetInDb != null)
			{
				partsAndFriends.AddRange(otherPartsToConsiderNotYetInDb);
			}
		}

		readonly ZGuid currentPartPK;

		readonly bool isCurrentPartActive;

		HashSet<string> ownerSupplierKeysAlreadyAdded;

		HashSet<string> classificationOrganizationKeysAlreadyAdded;

		public bool HasError { get; protected set; }

		public bool HasWarning
		{
			get { return GetHasWarning(); }
		}

		public ZString OwnerCodeFromLastErrorOrWarning { get; set; }

		public ZString SupplierCodeFromLastErrorOrWarning { get; set; }

		public ZString ClassificationOrganizationCodeFromLastErrorOrWarning { get; set; }

		List<RelatedPartyWithCode> GetRelatedParties(ZGuid opPk)
		{
			return (from PartAndFriends p in partsAndFriends where p.OPPK == opPk select new RelatedPartyWithCode(p.RelationshipType, p.OrgCode)).ToList();
		}

		List<PartAndFriends> partsAndFriends; // OrgPartRalations of the current part being checked and all other parts with the same part number.

		public bool CheckInactiveProducts { get; set; } = true;

		public void Validate()
		{
			HasError = false;
			hasWarning = false;
			hasDeactiveWarning = false;
			ownerSupplierKeysAlreadyAdded = new HashSet<string>();
			classificationOrganizationKeysAlreadyAdded = new HashSet<string>();

			if (isCurrentPartActive)
			{
				foreach (ZGuid otherPartPK in GetAllOtherPartsWithSamePartNumber(true))
				{
					CheckDuplicates(GetRelatedParties(otherPartPK), false, true);
				}

				CheckDuplicates(GetRelatedParties(currentPartPK), true, true);
			}

			if (CheckInactiveProducts && !HasError)
			{
				ownerSupplierKeysAlreadyAdded.Clear();
				foreach (ZGuid otherPartPK in GetAllOtherPartsWithSamePartNumber(false))
				{
					CheckDuplicates(GetRelatedParties(otherPartPK), false, false);
				}

				CheckDuplicates(GetRelatedParties(currentPartPK), true, false);
			}
		}

		static readonly string BlankOrgCode = new string(' ', OrgHeader.Schema.OH_CodeMaxLength);

		void CheckDuplicates(List<RelatedPartyWithCode> relatedParties, ZBool isCurrentPart, bool isActiveCheck)
		{
			var ownerCodes = GetOrganisationCodes(relatedParties, OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both);
			var supplierCodes = GetOrganisationCodes(relatedParties, OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both);
			var classificationOrganizationCodes = GetOrganisationCodes(relatedParties, OrgPartRelation.RelationshipTypes.ClassificationOrganization);

			foreach (var classificationOrganizationCode in classificationOrganizationCodes)
			{
				if (HasDuplicateClassificationOrganization(classificationOrganizationCode, isCurrentPart, isActiveCheck))
				{
					break;
				}
			}

			var firstSupplierCode = supplierCodes.FirstOrDefault() ?? BlankOrgCode;

			// Products with same code should not have same owners.
			foreach (var ownerCode in ownerCodes)
			{
				if (HasDuplicateOwnerSupplier(ownerCode + BlankOrgCode, ownerCode, firstSupplierCode, isCurrentPart, isActiveCheck))
				{
					return;
				}
			}

			// Products with same code should not have same suppliers, if there is no owner that would allow to resolve ambiguity.
			var standaloneSupplierCodes = supplierCodes.Where(s => !ownerCodes.Any(o => o != s)).ToList();
			foreach (var supplierCode in standaloneSupplierCodes)
			{
				if (HasDuplicateOwnerSupplier(BlankOrgCode + supplierCode, BlankOrgCode, supplierCode, isCurrentPart, isActiveCheck))
				{
					return;
				}
			}
		}

		bool HasDuplicateClassificationOrganization(string classificationOrganizationCode, ZBool isCurrentPart, bool isActiveCheck)
		{
			if (classificationOrganizationKeysAlreadyAdded.Contains(classificationOrganizationCode))
			{
				ClassificationOrganizationCodeFromLastErrorOrWarning = classificationOrganizationCode;
				return DuplicateKeyHasError(isCurrentPart, isActiveCheck);
			}
			else if (!isCurrentPart)
			{
				classificationOrganizationKeysAlreadyAdded.Add(classificationOrganizationCode);
			}

			return false;
		}

		bool HasDuplicateOwnerSupplier(string ownerSupplierKey, string ownerCode, string supplierCode, ZBool isCurrentPart, bool isActiveCheck)
		{
			if (ownerSupplierKeysAlreadyAdded.Contains(ownerSupplierKey))
			{
				OwnerCodeFromLastErrorOrWarning = ownerCode;
				SupplierCodeFromLastErrorOrWarning = supplierCode;
				return DuplicateKeyHasError(isCurrentPart, isActiveCheck);
			}
			else if (!isCurrentPart)
			{
				ownerSupplierKeysAlreadyAdded.Add(ownerSupplierKey);
			}

			return false;
		}

		bool DuplicateKeyHasError(bool isCurrentPart, bool isActiveCheck)
		{
			if (isActiveCheck)
			{
				if (isCurrentPart)
				{
					HasError = true;
					return true;
				}
				else
				{
					hasWarning = true;
				}
			}
			else if (isCurrentPart)
			{
				hasDeactiveWarning = true;
				return true;
			}

			return false;
		}

		bool hasWarning;
		bool hasDeactiveWarning;

		protected bool GetHasWarning()
		{
			return hasWarning || hasDeactiveWarning;
		}

		List<string> GetOrganisationCodes(IEnumerable<RelatedPartyWithCode> relatedParties, params string[] relationshipTypesToMatch)
		{
			var result = new List<string>();

			foreach (var relatedOrganisation in relatedParties)
			{
				if (string.IsNullOrEmpty(relatedOrganisation.RelatedOrganisationCode))
				{
					continue;
				}

				if (relationshipTypesToMatch.Contains(relatedOrganisation.RelationshipCode))
				{
					result.Add(relatedOrganisation.RelatedOrganisationCode.PadRight(OrgHeader.Schema.OH_CodeMaxLength));
				}
			}

			return result;
		}

		IEnumerable<ZGuid> GetAllOtherPartsWithSamePartNumber(bool isActive)
		{
			return (from PartAndFriends p in partsAndFriends.DistinctBy(x => x.OPPK) where p.OPPK != currentPartPK && isActive == p.IsActive select p.OPPK);
		}

		public string ErrorMessage
		{
			get { return Res.GetString("4870035e-68f3-45e7-ae9a-10a55563c241", "Duplicate Product detected: {0}", GetOwnerSupplierMessage()); }
		}

		public string WarningMessage
		{
			get
			{
				var result = new ZStringBuilder();
				if (hasDeactiveWarning)
				{
					result.Append(Res.GetString("F873E2DF-A98C-41EF-A1A4-5B0E08285655", "Deactivated Duplicate Product detected"));
				}

				if (hasWarning)
				{
					result.Append(Res.GetString("206a9167-84b3-4b8b-a12d-f6cc8a3fa8a7", "Other Duplicates of this Product detected"));
				}

				if (!result.IsEmpty)
				{
					result = new ZStringBuilder(result.ToStringWithDelimiterBetweenAppends(" " + Res.GetString("A4CE6236-C181-4291-8D2C-0C9ACBCBF745", "and") + " "));
					result.Append(GetOwnerSupplierMessage());
				}

				return result.ToStringWithDelimiterBetweenAppends(" : ");
			}
		}

		string GetOwnerSupplierMessage()
		{
			var result = new ZStringBuilder();

			if (!OwnerCodeFromLastErrorOrWarning.IsEmpty)
			{
				result.Append(Res.GetString("96B62D19-7429-4CAD-93D3-24ACD137514C", "Owner = {0}", OwnerCodeFromLastErrorOrWarning.Trim()));
			}

			if (!SupplierCodeFromLastErrorOrWarning.IsEmpty)
			{
				result.Append(Res.GetString("415ABC3D-CCCC-426C-867B-D85B95D4BB65", "Supplier = {0}", SupplierCodeFromLastErrorOrWarning.Trim()));
			}

			if (!ClassificationOrganizationCodeFromLastErrorOrWarning.IsEmpty)
			{
				result.Append(Res.GetString("3D8BF6AD-DCAF-4328-871C-6EA2CAC5E3E8", "Classification Organization = {0}", ClassificationOrganizationCodeFromLastErrorOrWarning.Trim()));
			}

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}
	}
}
