using System;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PartAttributeValidation : ValidationProvider
	{
		#region Attribute Validation for Operational Data Entry

		#region Attributes

		public virtual void CheckAttribute(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
#if DEBUG
			PartAttributeValidationChecker.SetCheckAttributeCall(org, part, info, attributeNumber);
#endif

			if (org != null && part != null)
			{
				if (org.PartAttributeManager.IsPartAttributeUsedByProduct(part, attributeNumber))
				{
					switch (org.PartAttributeManager.PartAttributeType(attributeNumber))
					{
						// non mandatory attributes dont require validation - any input is valid
						case PartAttributeTypeList.Codes.Mandatory:
						case PartAttributeTypeList.Codes.BatchNumber:
						case PartAttributeTypeList.Codes.JulianBatchNumber:
							CheckMandatoryAttribute(org, part, info, attributeNumber);
							break;

						case PartAttributeTypeList.Codes.VIN:
							CheckVIN(org, part, info, attributeNumber);
							break;
					}
				}
				else if (GlbCompany.CurrentCompany != null && org.CompanyData.OB_IMUsedBondedWhs && !string.IsNullOrEmpty(info.Value.ToString()))
				{
					var attributeName = Res.GetString("f827af7d-7212-4e72-9622-7e183b3d2fb5", "Part Attribute {0}", attributeNumber);
					AddNotification(info, Res.GetString("0dd2e932-f23c-4601-a270-241745020bdc", "The part '{0}' is not set up to use {1} with the Importer '{2}'. Please either remove the value '{3}' from the {4} field, or set up the Product and Importer to use {5}.", part.OP_PartNum, attributeName, org.OH_FullNameTruncated, info.Value.ToString(), attributeName, attributeName));
				}
				else if (!string.IsNullOrEmpty(info.Value.ToString()) && CheckAttributeWhenNotEmpty(info))
				{
					AddNotification(info, org.PartAttributeManager.PartAttributeName(attributeNumber) + " " + Res.GetString("d4d2ef40-1994-4910-ad00-6f872fe271da", "is not specified on the Product Master. Please do not enter a value."));
				}

				if (org.PartAttributeManager.PartAttributeType(attributeNumber) == PartAttributeTypeList.Codes.VIN)
				{
					CheckVinCore(org, part, info, attributeNumber);
				}
			}
		}

		#endregion

		#region Expiry Date

		public virtual void CheckExpiryDate(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
#if DEBUG
			PartAttributeValidationChecker.SetCheckAttributeCall(org, part, info, -1);
#endif

			if (org != null && part != null)
			{
				if (org.PartAttributeManager.IsExpiryDateUsedByProduct(part))
				{
					var errorMessage = MandatoryValidation.MustBeEnteredMessage(Res.GetString("ffb59c11-472d-4e17-a70a-ddee768b6bb1", "Expiry date"));
					AddNotificationIfValueIsEmpty(info, errorMessage);
				}
				else if (!info.Value.IsEmpty && CheckAttributeWhenNotEmpty(info))
				{
					AddNotification(info, Res.GetString("e5afd7dd-cb69-4386-a9a3-4f3b0549f546", "Expiry Date is not specified on the Product Master. Please do not enter a value."));
				}
			}
		}

		protected virtual bool CheckAttributeWhenNotEmpty(ZPropertyInfo info)
		{
			return info.Name.StartsWith("W", false, Culture.Invariant);
		}

		#endregion

		#region Packing Date

		public virtual void CheckPackingDate(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
#if DEBUG
			PartAttributeValidationChecker.SetCheckAttributeCall(org, part, info, -2);
#endif

			if (org != null && part != null)
			{
				if (org.PartAttributeManager.IsPackingDateUsedByProduct(part))
				{
					var errorMessage = MandatoryValidation.MustBeEnteredMessage(Res.GetString("2b8e6d32-b205-4ab2-aa9b-edad00869011", "Packing date"));
					AddNotificationIfValueIsEmpty(info, errorMessage);
				}
				else if (!info.Value.IsEmpty && CheckAttributeWhenNotEmpty(info))
				{
					AddNotification(info, Res.GetString("ec39d246-25cf-47a8-ae01-aec59b6c60af", "Packing Date is not specified on the Product Master. Please do not enter a value."));
				}
			}
		}

		#endregion

		#endregion

		#region Attribute Definition Validation for Organisation and Part Setup

		#region Organisation Level

		#region Attribute Names

		public void CheckAttributeNameDefinitionForOrganisation(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo nameInfo, ZPropertyInfo typeInfo, SchemaColumn orgPartRelationUseColumn)
		{
			if (typeInfo != null && !typeInfo.Value.IsEmpty)
			{
				MandatoryValidation.CheckEntered(nameInfo);
			}
			CheckAttributeDefinitionForOrganisationCore(factory, org, nameInfo, orgPartRelationUseColumn);
		}

		public void CheckAttributeNameForDuplication(ZPropertyInfo nameInfo1, ZPropertyInfo nameInfo2)
		{
			if (!nameInfo1.Value.IsEmpty && !nameInfo2.Value.IsEmpty && (ZString)nameInfo1.Value == (ZString)nameInfo2.Value)
			{
				nameInfo1.AddError(Res.GetString("cdefbd2a-b06c-4a6e-bc40-30b681c7f53a", "This attribute name has already been used"));
			}
		}

		#endregion

		#region Attribute Types

		public void CheckAttributeTypeDefinitionForOrganisation(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo typeInfo, ZPropertyInfo nameInfo, SchemaColumn orgPartRelationUseColumn)
		{
			if (!nameInfo.Value.IsEmpty)
			{
				MandatoryValidation.CheckEntered(typeInfo);
			}

			ListValidation.ErrorIfInvalidCode(typeInfo, new PartAttributeTypeList());

			CheckAttributeDefinitionForOrganisationCore(factory, org, typeInfo, orgPartRelationUseColumn);
		}

		// Tested in OrgMiscServValidation.
		public void CheckAttributeTypeIsNotUsedByExistingStock(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo partAttribTypeInfo, SchemaBoolColumn productUsingAttribColumn)
		{
			if (!partAttribTypeInfo.HasErrors())
			{
				if (partAttribTypeInfo.HasChanges)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsDocketLineSchema.WE_WD);
					subQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, org.PK);

					var query = new ZDBOnlyQuery(typeof(IWhsDocketLine));
					query.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
					query.AddSubQuery(subQuery, JoinCondition.And);
					AddSupplierPartSubQuery(org, query, productUsingAttribColumn);

					var stockExistsUsingThisAttribute = factory.ExistsInDatabase(WhsDocketLineSchema.Constants.TableName, query);
					if (stockExistsUsingThisAttribute)
					{
						partAttribTypeInfo.AddError(Res.GetString("8e393cde-78b2-46a2-a4df-b8539002f503",
@"This attribute must remain as '{0}' because there is current inventory for products using this attribute.
You must remove this inventory from the warehouse before this attribute can be changed.", partAttribTypeInfo.OriginalValue));
					}
				}
			}
		}

		#endregion

		#region Expiry Date

		public void CheckExpiryDateDefinitionForOrganisation(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo info)
		{
			CheckAttributeDefinitionForOrganisationCore(factory, org, info, OrgPartRelationSchema.OU_UseExpiryDate);
		}

		#endregion

		#region Packing Date

		public void CheckPackingDateDefinitionForOrganisation(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo info)
		{
			CheckAttributeDefinitionForOrganisationCore(factory, org, info, OrgPartRelationSchema.OU_UsePackingDate);
		}

		#endregion

		#region Serial Number

		public void CheckSerialNumberDefinitionForOrganisation(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo info)
		{
			CheckAttributeDefinitionForOrganisationCore(factory, org, info, OrgPartRelationSchema.OU_UseSerialNumber);
		}

		#endregion

		#endregion

		#region Part Level

		#region Attributes

		public void CheckAttributeDefinitionForPart(BusinessObjectFactory factory, OrgPartRelation relation, int attributeNumber, ZPropertyInfo info, SchemaColumn whsInventoryColumn)
		{
			if (relation != null)
			{
				CheckAttributeDefinitionForPart(factory, relation.Organisation, relation.SupplierPart, info, whsInventoryColumn);
				CheckUsePartAttribDefinitionForPart(relation, attributeNumber, info);
			}
		}

		void CheckUsePartAttribDefinitionForPart(OrgPartRelation relation, int attributeNumber, ZPropertyInfo info)
		{
			var org = relation.Organisation;
			if (org != null)
			{
				var attributeNumbers = new int[] { 1, 2, 3 };
				if (attributeNumbers.Contains(attributeNumber))
				{
					bool on = (info.Value is ZBool) ? (ZBool)info.Value : (ZBool)!((ZString)info.Value).IsEmpty;
					if (on && org.PartAttributeManager.PartAttributeType(attributeNumber) == PartAttributeTypeList.Codes.VIN
						&& attributeNumbers.Any(i => i != attributeNumber
							&& (ZBool)relation["OU_UsePartAttrib" + i.ToString(CultureInfo.InvariantCulture)]
							&& org.PartAttributeManager.PartAttributeType(i) == PartAttributeTypeList.Codes.VIN))
					{
						info.AddError(Res.GetString("5e409f71-22be-418f-a95d-3cbe67e9abb5", "You can only use one VIN attribute for the organization on the product."));
					}
				}
			}
		}

		public void CheckSerialNumberDefinitionForPart(BusinessObjectFactory factory, OrgPartRelation relation, ZPropertyInfo info, SchemaColumn whsInventoryColumn)
		{
			if (relation != null)
			{
				CheckAttributeDefinitionForPart(factory, relation.Organisation, relation.SupplierPart, info, whsInventoryColumn);
			}
		}

		#endregion

		#region Expiry Date

		public void CheckExpiryDateDefinitionForPart(BusinessObjectFactory factory, OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
			CheckAttributeDefinitionForPart(factory, org, part, info, WhsDocketLineSchema.WE_ExpiryDate);
		}

		#endregion

		#region Packing Date

		public void CheckPackingDateDefinitionForPart(BusinessObjectFactory factory, OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
			CheckAttributeDefinitionForPart(factory, org, part, info, WhsDocketLineSchema.WE_PackingDate);
		}

		#endregion

		#endregion

		#endregion

		#region ErrorMessages

		public static string GetThereIsCurrentInventoryUsingJulianBatchNumbersErrorMessage(string fieldName)
		{
			return Res.GetString("83923175-d51c-4333-bfc0-4dd231eee73d", "There is current inventory using Julian Batch numbers. This inventory must be removed from the warehouse before the {0} can be changed.", fieldName);
		}

		#endregion

		#region Implementation

		#region Attribute Validation Implementation

		void CheckMandatoryAttribute(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
			if (info.Value.IsEmpty)
			{
				var messageToAppend = GetMandatoryAttributeErrorMessageToAppend(org, part, info.Value, attributeNumber);
				var mustBeEnteredMessage = MandatoryValidation.MustBeEnteredMessage(org.PartAttributeManager.PartAttributeName(attributeNumber));
				var errorMessage = messageToAppend.IsEmpty
					? mustBeEnteredMessage
					: string.Format(Culture.Current, "{0} {1}", mustBeEnteredMessage, messageToAppend);

				AddNotificationIfValueIsEmpty(info, errorMessage);
			}
		}

		protected virtual ZString GetMandatoryAttributeErrorMessageToAppend(OrgHeader org, OrgSupplierPart part, IZType value, int attributeNumber)
		{
			return ZString.Empty;
		}

		public void CheckSerialNumber(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
			if (org != null && part != null)
			{
				if (org.PartAttributeManager.IsSerialNumberUsedByProduct(part))
				{
					CheckMandatoryAttribute(org, part, info, 4);
					CheckSerialNumberCore(org, part, info);
				}
				else if (GlbCompany.CurrentCompany != null && org.CompanyData.OB_IMUsedBondedWhs && !string.IsNullOrEmpty(info.Value.ToString()))
				{
					AddNotification(info, Res.GetString("9e231bc3-fa47-4026-b73c-e24afe93ecf2", "The part '{0}' is not set up to use Serial Number with the Importer '{1}'. Please either remove the value '{2}' from the Serial Number field, or set up the Product and Importer to use Serial Number.", part.OP_PartNum, org.OH_FullNameTruncated, info.Value.ToString()));
				}
				else if (!string.IsNullOrEmpty(info.Value.ToString()) && CheckAttributeWhenNotEmpty(info))
				{
					AddNotification(info, Res.GetString("188979f1-9fa0-42dc-bb1a-392493942608", "Serial Number") + " " + Res.GetString("d4d2ef40-1994-4910-ad00-6f872fe271da", "is not specified on the Product Master. Please do not enter a value."));
				}
			}
		}

		protected virtual void CheckSerialNumberCore(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info)
		{
		}

		void CheckVIN(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
			CheckMandatoryAttribute(org, part, info, attributeNumber);
		}

		protected virtual void CheckVinCore(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
		}

		#endregion

		#region Attribute Definition Validation Implementation

		void CheckAttributeDefinitionForOrganisationCore(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo info, SchemaColumn column)
		{
			if (org != null)
			{
				bool on = (info.Value is ZBool) ? (ZBool)info.Value : (ZBool)!((ZString)info.Value).IsEmpty;

				if (on)
				{
					if (HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttrbutes(factory, org, info, column))
					{
						info.AddError(Res.GetString("186fd1d7-d2f6-427b-bf02-0f0cb8c47e4f", "You are changing this attribute from non-mandatory to mandatory but there is current inventory for products using this attribute where the attribute is empty. You must remove this inventory from the warehouse before this attribute can be made mandatory"));
					}
				}
				else
				{
					if (IsRelationshipUsingAttribute(factory, org, column))
					{
						info.AddError(Res.GetString("be686f46-0ac8-40de-a8b8-4eac21afd839", "This attribute is being used by at least one product with a relationship to this organization.\r\nThese product relationships must have this attribute disabled before you can disable it for the organization.\r\nSee Product Entry."));
					}
				}
			}
		}

		static void CheckAttributeDefinitionForPart(BusinessObjectFactory factory, OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, SchemaColumn inventoryColumn)
		{
			var on = (info.Value is ZBool) ? (ZBool)info.Value : (ZBool)!((ZString)info.Value).IsEmpty;

			var hasErrorIfChanged = CheckAttributeDefinitionForPartHasErrorIfChanged(factory, org, part, inventoryColumn, on);
			if (hasErrorIfChanged)
			{
				var errorMessage = on
					? Res.GetString("e15dc7f2-94fc-4fb9-8b0c-9350c3c3c1bf", "There is current inventory which is NOT using this attribute. This inventory must be removed from the warehouse before this attribute can be enabled.")
					: Res.GetString("8560a3d4-0248-4de8-aa1a-8bbfc48ffdfa", "There is current inventory using this attribute. This inventory must be removed from the warehouse before this attribute can be disabled.");
				info.AddError(errorMessage);
			}
		}

		public static bool CheckAttributeDefinitionForPartHasErrorIfChanged(BusinessObjectFactory factory, OrgHeader org, OrgSupplierPart part, SchemaColumn inventoryColumn, ZBool enabled)
		{
			var error = false;

			if (org != null && part != null && inventoryColumn != null)
			{
				var relation = GetOwnerRelationship(org, part);

				if (enabled
					? (!CanSkipCurrentInventoryCheck(org, relation, inventoryColumn) && HasCurrentInventory(factory, org, part, null, inventoryColumn, true))
					: HasCurrentInventory(factory, org, part, null, inventoryColumn, GetIsReleaseCaptured(inventoryColumn, relation)))
				{
					error = true;
				}
			}

			return error;
		}

		static OrgPartRelation GetOwnerRelationship(OrgHeader org, OrgSupplierPart part)
		{
			return part.RelatedOrganisations.FindByOrganisationPKAndRelationship(org.PK, OrgPartRelation.RelationshipTypes.Owner);
		}

		static bool GetIsReleaseCaptured(SchemaColumn inventoryColumn, OrgPartRelation relation)
		{
			bool result = false;

			if (relation != null)
			{
				if (inventoryColumn == WhsDocketLineSchema.WE_PartAttrib1)
				{
					result = (ZBool)relation.OU_IsPartAttrib1ReleaseCapturedInfo.Value;
				}
				else if (inventoryColumn == WhsDocketLineSchema.WE_PartAttrib2)
				{
					result = (ZBool)relation.OU_IsPartAttrib2ReleaseCapturedInfo.Value;
				}
				else if (inventoryColumn == WhsDocketLineSchema.WE_PartAttrib3)
				{
					result = (ZBool)relation.OU_IsPartAttrib3ReleaseCapturedInfo.Value;
				}
				else if (inventoryColumn == WhsDocketLineSchema.WE_SerialNumber)
				{
					result = (ZBool)relation.OU_IsSerialNumberReleaseCapturedInfo.Value;
				}
			}

			return result;
		}

		#endregion

		#region Implementation

		void AddNotificationIfValueIsEmpty(ZPropertyInfo info, string errorMessage)
		{
			if (info.Value.IsEmpty)
			{
				AddNotification(info, errorMessage);
			}
		}

		protected virtual void AddNotification(ZPropertyInfo info, string errorMessage) => info.AddError(errorMessage);

		static bool CanSkipCurrentInventoryCheck(OrgHeader org, OrgPartRelation relation, SchemaColumn column)
		{
			var result = GetIsReleaseCaptured(column, relation);

			if (column == WhsDocketLineSchema.WE_PartAttrib1)
			{
				result |= (org.PartAttributeManager.PartAttributeType(1) == PartAttributeTypeList.Codes.NonMandatory);
			}
			else if (column == WhsDocketLineSchema.WE_PartAttrib2)
			{
				result |= (org.PartAttributeManager.PartAttributeType(2) == PartAttributeTypeList.Codes.NonMandatory);
			}
			else if (column == WhsDocketLineSchema.WE_PartAttrib3)
			{
				result |= (org.PartAttributeManager.PartAttributeType(3) == PartAttributeTypeList.Codes.NonMandatory);
			}

			return result;
		}

		bool IsRelationshipUsingAttribute(BusinessObjectFactory factory, OrgHeader org, SchemaColumn column)
		{
			var filter = new ZQuery(OrgPartRelationSchema.OU_OH, org.PK);
			filter.AddToFilter(column, ZBool.True);
			return factory.ExistsInDatabase(OrgPartRelationSchema.Constants.TableName, filter);
		}

		static bool HasCurrentInventory(BusinessObjectFactory factory, OrgHeader org, OrgSupplierPart part, SchemaColumn relationColumn, SchemaColumn inventoryColumn, bool isEmpty)
		{
			return factory.LoadTop1<IWhsDocket>(GetInventoryQuery(org, part, relationColumn, inventoryColumn, isEmpty)) != null;
		}

		static ZDBOnlyQuery GetInventoryQuery(OrgHeader org, OrgSupplierPart part, SchemaColumn relationColumn, SchemaColumn inventoryColumn, bool isEmpty)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(IWhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_CurrentInventoryStatus, new[] { "AVL", "HEL", "INT", "STA" });

			if (part != null)
			{
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_OP, part.PK);
			}
			else
			{
				AddSupplierPartSubQuery(org, docketLineSubQuery, relationColumn);
			}

			if (inventoryColumn is SchemaDateTimeColumn)
			{
				docketLineSubQuery.AddToFilter(inventoryColumn, isEmpty ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else
			{
				docketLineSubQuery.AddToFilter(inventoryColumn, isEmpty ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, ZString.Empty);
			}

			var docketQuery = new ZDBOnlyQuery(typeof(IWhsDocket));
			docketQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, org.PK);
			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			return docketQuery;
		}

		static void AddSupplierPartSubQuery(OrgHeader org, ZDBOnlyQuery filter, SchemaColumn relationColumn)
		{
			if (relationColumn != null) // should not happen when part == null, but just in case.
			{
				var supplierPartSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsDocketLineSchema.WE_OP);
				var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, org.PK);
				relationSubQuery.AddToFilter(relationColumn, true);

				supplierPartSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);
				filter.AddSubQuery(supplierPartSubQuery, JoinCondition.And);
			}
		}

		bool HasCurrentInventoryThatIsFinalizedWithNonMandatoryAttrbutes(BusinessObjectFactory factory, OrgHeader org, ZPropertyInfo info, SchemaColumn relationColumn)
		{
			bool result = false;
			if (info.HasChanges)
			{
				if (info.OriginalValue.ToString() == PartAttributeTypeList.Codes.NonMandatory)
				{
					SchemaColumn inventoryColumn = null;
					if (relationColumn == OrgPartRelationSchema.OU_UsePartAttrib1)
					{
						inventoryColumn = WhsDocketLineSchema.WE_PartAttrib1;
					}
					else if (relationColumn == OrgPartRelationSchema.OU_UsePartAttrib2)
					{
						inventoryColumn = WhsDocketLineSchema.WE_PartAttrib2;
					}
					else if (relationColumn == OrgPartRelationSchema.OU_UsePartAttrib3)
					{
						inventoryColumn = WhsDocketLineSchema.WE_PartAttrib3;
					}
					result = HasCurrentInventory(factory, org, null, relationColumn, inventoryColumn, true);
				}
			}
			return result;
		}

		#endregion

		#endregion
	}
}

#if DEBUG

namespace Enterprise.MasterFiles.Business
{
	using CargoWise.EntityFramework.Testing;

	public class PartAttributeValidationChecker : TestCaseWithFactory
	{
		public static void SetCheckAttributeCall(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
			if (AttributeCallCheckerInstance != null)
			{
				AttributeCallCheckerInstance.Check(org, part, info, attributeNumber);
			}
		}

		public static AttributeCallChecker AttributeCallCheckerInstance;

		public class AttributeCallChecker : IDisposable
		{
			#region fields
			readonly OrgHeader testOrg;
			readonly OrgSupplierPart testPart;
			readonly string testInfoName;
			readonly int testAttribute;

			readonly bool shouldCheck;
			bool isChecked;
			bool isOk;
			string msg;

			#endregion

			#region ctors
			/// <summary>
			/// General ctor.
			/// </summary>
			/// <param name="shouldCheck"></param>
			/// <param name="org"></param>
			/// <param name="part"></param>
			/// <param name="info"></param>
			/// <param name="attributeNumber"></param>
			public AttributeCallChecker(bool shouldCheck, OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
			{
				this.shouldCheck = shouldCheck;
				testOrg = org;
				testPart = part;
				testInfoName = info == null ? "" : info.Name;
				testAttribute = attributeNumber;
				if (AttributeCallCheckerInstance != null)
				{
					Assert(string.Format(@"Attempted to start new attribute checking test with parameters:
Org={0}
Part={1}
Property={2}
AttributeNumber={3}
while another test with parameters:
Org={4}
Part={5}
Property={6}
AttributeNumber={7}
is not completed yet."
								, testOrg
								, testPart
								, testInfoName
								, testAttribute
								, AttributeCallCheckerInstance.testOrg
								, AttributeCallCheckerInstance.testPart
								, AttributeCallCheckerInstance.testInfoName
								, AttributeCallCheckerInstance.testAttribute
							)
							, true
					);
				}
				AttributeCallCheckerInstance = this;
			}

			/// <summary>
			/// Use this ctor to check that no validation is called
			/// </summary>
			public AttributeCallChecker()
				: this(false, null, null, null, 0)
			{
			}

			/// <summary>
			/// Use this ctor to check that validation is called with indicated parameters
			/// </summary>
			/// <param name="org"></param>
			/// <param name="part"></param>
			/// <param name="info"></param>
			/// <param name="attributeNumber"></param>
			public AttributeCallChecker(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
				: this(true, org, part, info, attributeNumber)
			{
			}

			#endregion

			public void Check(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
			{
				string name = info == null ? "" : info.Name;
				Assert(string.Format(@"Unexpected attribute validation call with parameters:
Org={0}
Part={1}
Property={2}
AttributeNumber={3}"
							, org
							, part
							, name
							, attributeNumber
						)
						, shouldCheck
				);

				if (isOk)
				{
					return;
				}

				if (testOrg == org
					&& testPart == part
					&& testInfoName == name
					&& testAttribute == attributeNumber
				)
				{
					isOk = true;
				}
				else
				{
					#region expected and given parameters aren't match: forming error message
					if (!isChecked)
					{
						msg = string.Format(@"Expected:
Org={0}
Part={1}
Property={2}
AttributeNumber={3}"
							, testOrg
							, testPart
							, testInfoName
							, testAttribute
						);
					}
					msg += string.Format(@"

Get:
Org={0}
Part={1}
Property={2}
AttributeNumber={3}"
						, org
						, part
						, name
						, attributeNumber
					);

					#endregion
				}
				isChecked = true;
			}

			#region IDisposable Members
			public void Dispose()
			{
				AttributeCallCheckerInstance = null;
				if (shouldCheck && !isChecked)
				{
					msg = string.Format(@"No checking was done for
Org={0}
Part={1}
Property={2}
AttributeNumber={3}"
						, testOrg
						, testPart
						, testInfoName
						, testAttribute
					);
				}
				Assert(msg, isOk || !shouldCheck);
			}

			#endregion
		}
	}
}

#endif
