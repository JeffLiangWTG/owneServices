using System;
using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GovernmentInvoiceValidation : AccTransactionHeaderValidation
	{
		public GovernmentInvoiceValidation(GovernmentInvoice parent)
			: base(parent)
		{ }

		protected new GovernmentInvoice Parent => (GovernmentInvoice)base.Parent;

		ZDate DateForComplianceAllocation => Parent.ComplianceNumberAllocationDate.Date;

		string DateForComplianceAllocationLabel =>
			Parent.ComplianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code
				? AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Description
				: AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Description;

		SchemaDateTimeColumn DateForComplianceAllocationColumn => Parent.ComplianceNumberAllocationDateColumn;

		bool IsEmptyComplianceSubtypeDisallowed => GetEmptyComplianceSubtypeDisallowedRegistryInstance().Value;

		public static BooleanRegistryItem GetEmptyComplianceSubtypeDisallowedRegistryInstance()
		{
			var locator = new RegistryItemSetLocator();
			var accRegistryItemSet = (RegistryItemSet)locator.GetRegistryItemSet("AccountingConfigurationRegistry");
			return (BooleanRegistryItem)accRegistryItemSet.FindByName("DISALLOWPOSTINGTRANSACTIONWITHEMPTYCOMPLIANCESUBTYPE");
		}

		bool ShouldAllocateComplianceNumberOnPosting()
		{
			string regVal = null;
			switch (Parent.AH_Ledger)
			{
				case LedgerTypes.AccountsPayable:
					regVal = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.Value;
					break;
				case LedgerTypes.AccountsReceivable:
					regVal = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.Value;
					break;
			}
			return regVal == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
		}

		protected override void CheckAH_TransactionReference()
		{
			base.CheckAH_TransactionReference();
			var country = Parent.CompanySafe.Country;
			var transRefInfo = Parent.AH_TransactionReferenceInfo;

			if (country.SupportComplianceSubType && country.Code != Core.Constants.CountryCodes.China)
			{
				if (!Parent.AH_ComplianceSubTypeInfo.HasChanges && !transRefInfo.HasChanges)
				{
					return;
				}
				try
				{
					var isComplianceNumberAllocationMandatory = Parent.IsComplianceNumberAllocationMandatory;
					if (isComplianceNumberAllocationMandatory && IsEmptyComplianceSubtypeDisallowed && ShouldAllocateComplianceNumberOnPosting())
					{
						MandatoryValidation.CheckEntered(transRefInfo);
					}

					var transRef = Parent.AH_TransactionReference;
					if (!transRef.IsEmpty)
					{
						AccComplianceSequence sequence = isComplianceNumberAllocationMandatory ? Parent.ComplianceSequenceFromSubType : Parent.ComplianceSequenceFromTransactionReference;
						if (sequence != null)
						{
							if (isComplianceNumberAllocationMandatory)
							{
								bool mustUseAllocate = false;

								string errorMsg;

								if (Parent.IsComplianceNumberAllocationDateEarlierThanLastDateUsedInBook(sequence))
								{
									errorMsg = ValidateTransactionReference(sequence, transRef);
								}
								else
								{
									errorMsg = Res.GetString("1d6db22d-cfba-4b31-bea4-f17f0be22975", "{0} ({1}) is later than or equal to Last Date used ({2}) for this Compliance Book. Please use 'Allocate' action", DateForComplianceAllocationLabel, DateForComplianceAllocation.ToShortDateString(), Parent.GetLastDateUsedInComplianceBook(sequence).ToShortDateString());
									mustUseAllocate = true;
								}

								if (!string.IsNullOrWhiteSpace(errorMsg))
								{
									transRefInfo.AddError(errorMsg);

									if (!mustUseAllocate)
									{
										string availComplNr = GetAvailableTransRef(sequence);
										//using AddWarning would be more user-friendly, but it wouldn't appear in error box on Save
										if (string.IsNullOrWhiteSpace(availComplNr) || availComplNr == transRef ||
											availComplNr != transRefInfo.OriginalValue.ToString() && !string.IsNullOrWhiteSpace(ValidateTransactionReference(sequence, availComplNr)))
										{
											transRefInfo.AddError(Res.GetString("60399590-24da-429e-b5ef-441c9eb5d84a", "No suggestions have been found for this {0}", DateForComplianceAllocationLabel)); //case 10.3
										}
										else
										{
											transRefInfo.AddError(Res.GetString("4979A700-21FA-4D58-AF86-496B1F14AF1A", @"You can assign the following number: {0}", availComplNr));
										}
									}
								}
							}
							else
							{
								transRefInfo.AddError(Res.GetString("55a933bf-dba5-42ba-beed-f98bc099136e", "The number overlaps with existing compliance sequence book"));
							}
						}
					}
				}
				catch (ComplianceSequenceRelatedException ex)
				{
					transRefInfo.AddError(ex.UserFriendlyMessage);
				}
			}

			if (Parent.IsInDatabase && transRefInfo.HasChanges
				&& country.SupportDocumentSigning)
			{
				transRefInfo.AddError(Res.GetString("112005f2-372b-4ed4-8102-f977e068927a", "This number cannot be changed from it's original value '{0}' as it is used for Digital Signature.", transRefInfo.OriginalValue));
			}

			if (HasEInvoicingStatusValidationError(transRefInfo))
			{
				transRefInfo.AddError(Res.GetString("e0df9a1e-ad4b-473a-a9d4-86a41f32aff3", "Compliance Number cannot be changed from '{0}' as it has already been submitted for E-Invoicing.", transRefInfo.OriginalValue));
			}
		}

		protected override void CheckAH_ComplianceSubType()
		{
			var country = Parent.CompanySafe.Country;
			var subTypeInfo = Parent.AH_ComplianceSubTypeInfo;

			if (country.SupportComplianceSubType)
			{
				if (!Parent.AH_TransactionReference.IsEmpty || IsEmptyComplianceSubtypeDisallowed)
				{
					MandatoryValidation.CheckEntered(subTypeInfo);
				}
				if (!Parent.AH_ComplianceSubType.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(subTypeInfo);
				}
			}

			if (Parent.IsInDatabase && subTypeInfo.HasChanges && !subTypeInfo.OriginalValue.IsEmpty
				 && country.SupportDocumentSigning)
			{
				subTypeInfo.AddError(Res.GetString("67444b05-4a4c-4d2a-9322-415a3bacdc25", "Compliance Sub Type cannot be changed from it's original value '{0}' as allocated for it number is used for Digital Signature.", Parent.AH_ComplianceSubTypeInfo.OriginalValue));
			}

			if (!CheckIsComplianceSubTypeValueProtectedAndNotChanged()
				&& HasEInvoicingStatusValidationError(subTypeInfo))
			{
				subTypeInfo.AddError(Res.GetString("8cd9f01d-37f7-4edc-ab56-6e076202b2ec", "Compliance Sub Type cannot be changed from '{0}' as it has already been submitted for E-Invoicing.", subTypeInfo.OriginalValue));
			}

			var complianceSubTypeValidation = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeValidation(Parent.Company?.GC_RN_NKCountryCode ?? ZString.Empty);
			if (complianceSubTypeValidation != null)
			{
				var errorMessageForComplianceSubTypeValidation = complianceSubTypeValidation.ErrorMessageForComplianceSubTypeValidation(Parent);
				if (!errorMessageForComplianceSubTypeValidation.IsEmpty)
				{
					Parent.AH_ComplianceSubTypeInfo.AddError(errorMessageForComplianceSubTypeValidation);
				}

				var warningMessageForComplianceSubTypeValidation = complianceSubTypeValidation.WarningMessageForComplianceSubTypeValidation(Parent);
				if (!warningMessageForComplianceSubTypeValidation.IsEmpty)
				{
					Parent.AH_ComplianceSubTypeInfo.AddWarning(warningMessageForComplianceSubTypeValidation);
				}
			}
		}

		string ValidateTransactionReference(AccComplianceSequence sequence, ZString complNr)
		{
			string errorMsg = ValidateTransactionReferenceIsNotInUse(sequence, complNr);
			if (string.IsNullOrWhiteSpace(errorMsg))
			{
				errorMsg = ValidateTransactionReferenceIsInRange(sequence, complNr);
				if (string.IsNullOrWhiteSpace(errorMsg))
				{
					errorMsg = ValidateTransactionReferenceToNextNumberAndDate(sequence, complNr);
				}
			}
			return errorMsg;
		}

		string ValidateTransactionReferenceIsNotInUse(AccComplianceSequence sequence, ZString complNr)
		{
			var pkGC = GlbCompany.CurrentCompany.PK.ToGuid();

			var subType = sequence.XD_SequenceClass;
			var startDate = sequence.XD_StartDate;
			var expiryDate = sequence.XD_ExpiryDate;

			var factory = new BusinessObjectFactory();

			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, pkGC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, subType);
			if (startDate.IsValid)
			{
				query.AddToFilter(DateForComplianceAllocationColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
			}
			if (expiryDate.IsValid)
			{
				query.AddToFilter(DateForComplianceAllocationColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, expiryDate);
			}

			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, complNr);

			if (factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, query))
			{
				return Res.GetString("3BBFC863-C157-41B1-9DE3-5CB096BC6A16", "This Compliance Number is already used for the specified Compliance Sub Type");
			}

			return null;
		}

		string ValidateTransactionReferenceIsInRange(AccComplianceSequence sequence, ZString complNr)
		{
			var prefix = sequence.XD_Prefix;
			var startNr = sequence.XD_StartNumber;
			var endNr = sequence.XD_EndNumber;

			if (complNr.StartsWith(prefix))
			{
				var onlyNumberValue = complNr.Replace(prefix, "").KeepNumericCharacters();
				int fixedNumber;
				if (int.TryParse(onlyNumberValue, out fixedNumber) &&
					fixedNumber >= startNr.ToZInt() && fixedNumber <= endNr.ToZInt())
				{
					return null;
				}
			}

			var maxNumDigits = sequence.XD_MaximumNumberDigits;
			var startNrStr = startNr.ToString().PadLeft(maxNumDigits, '0');
			var endNrStr = endNr.ToString().PadLeft(maxNumDigits, '0');
			return Res.GetString("58201553-C95B-48F1-98CF-6CA15F09EF73", "This Compliance Number is not between Start ({0}{1}) and End number ({0}{2}) for the specified Compliance Sub Type", prefix, startNrStr, endNrStr);
		}

		string ValidateTransactionReferenceToNextNumberAndDate(AccComplianceSequence sequence, ZString complNr)
		{
			var prefix = sequence.XD_Prefix;
			var nextNr = sequence.XD_NextNumber;
			var onlyNumberValue = complNr.Replace(prefix, "").KeepNumericCharacters();
			int fixedNumber;
			if (int.TryParse(onlyNumberValue, out fixedNumber) && fixedNumber > nextNr.ToZInt())
			{
				var nextNrStr = nextNr.ToString().PadLeft(sequence.XD_MaximumNumberDigits, '0');
				return Res.GetString("98BD933E-5A75-411A-97C0-6810556F2A76", "This Compliance Number is higher than the next number ({0}{1}) for the specified Compliance Sub Type", prefix, nextNrStr);
			}

			var pkGC = GlbCompany.CurrentCompany.PK.ToGuid();
			var subType = sequence.XD_SequenceClass;
			var startDate = sequence.XD_StartDate;
			var expiryDate = sequence.XD_ExpiryDate;

			var factory = new BusinessObjectFactory();

			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, pkGC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, subType);
			if (startDate.IsValid)
			{
				query.AddToFilter(DateForComplianceAllocationColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, startDate);
			}
			if (expiryDate.IsValid)
			{
				query.AddToFilter(DateForComplianceAllocationColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, expiryDate);
			}

			var conflictsQuery = new ZQuery();
			var queryParams = new ZSqlParameterCollection();
			queryParams.Add("@complianceAssignNumbersOrderDate", DateForComplianceAllocation, DateForComplianceAllocationColumn);

			var subQueryPrev = new ZQuery(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.GreaterThan, complNr);
			subQueryPrev.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.IsNotBlank, string.Empty);
			subQueryPrev.AddFilterAndZSQLParameterCollection($"CONVERT(DATE, {DateForComplianceAllocationColumn.Name}) < @complianceAssignNumbersOrderDate", queryParams);

			var subQueryNext = new ZQuery(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.LessThan, complNr);
			subQueryNext.AddToFilter(AccTransactionHeaderSchema.AH_TransactionReference, SQLComparisonOperator.IsNotBlank, string.Empty);
			subQueryNext.AddFilterAndZSQLParameterCollection($"CONVERT(DATE, {DateForComplianceAllocationColumn.Name}) > @complianceAssignNumbersOrderDate", queryParams);

			conflictsQuery.AddToFilter(subQueryPrev, JoinCondition.Or);
			conflictsQuery.AddToFilter(subQueryNext, JoinCondition.Or);

			query.AddToFilter(conflictsQuery);

			if (factory.ExistsInDatabase(AccTransactionHeaderSchema.Constants.TableName, query))
			{
				return Res.GetString("acb3cbcf-81cf-4096-95c1-b1e65d5f3e9e", "This Compliance Number is not allowed for this {0} ({1})",
					DateForComplianceAllocationLabel, DateForComplianceAllocation.ToShortDateString());
			}

			return null;
		}

		string GetAvailableTransRef(AccComplianceSequence sequence)
		{
			var startDate = sequence.XD_StartDate;
			var lastDate = Parent.GetLastDateUsedInComplianceBook(sequence);

			string selectSql = $@"DECLARE @tempTrans TABLE(ComplNr varchar(20), ComplNrInt bigint, ComplianceAssignNumbersOrderDate date)

				INSERT INTO @tempTrans
				SELECT  AH_TransactionReference, null, CONVERT(date, {DateForComplianceAllocationColumn.Name})
				FROM dbo.AccTransactionHeader
				WHERE   AH_GC = @currComp and AH_ComplianceSubType = @subType
				and CONVERT(date, {DateForComplianceAllocationColumn.Name}) between @startDate and @lastDate
				and AH_TransactionReference LIKE @numPrefix +'%';

				DELETE FROM @tempTrans
				WHERE ComplNr = @currComplNr;

				UPDATE @tempTrans
				SET ComplNrInt = CONVERT(bigint, RIGHT(ComplNr, @maxNumDigits))
				WHERE ISNUMERIC(RIGHT(ComplNr, @maxNumDigits))=1 and RIGHT(ComplNr, @maxNumDigits) NOT LIKE '%[^0-9]%';

				SELECT ISNULL((SELECT ISNULL(

				(SELECT @numPrefix + REPLACE(STR(RIGHT(MIN(ComplNr), @maxNumDigits) - 1, @maxNumDigits), ' ', '0')
				FROM @tempTrans mo
				WHERE ComplianceAssignNumbersOrderDate = @complianceAssignNumbersOrderDate and ComplNrInt between @startNr + 1 and @nextNr - 1
				and NOT EXISTS
				(
				 SELECT NULL FROM @tempTrans mi
				 WHERE   mi.ComplNrInt = mo.ComplNrInt - 1
				)),

				(SELECT @numPrefix + REPLACE(STR(RIGHT(MIN(ComplNr), @maxNumDigits) + 1, @maxNumDigits), ' ', '0')
				FROM @tempTrans mo
				WHERE ComplianceAssignNumbersOrderDate = @complianceAssignNumbersOrderDate and ComplNrInt between @startNr and @nextNr - 2
				and NOT EXISTS
				(
				 SELECT NULL FROM @tempTrans mi
				 WHERE   mi.ComplNrInt = mo.ComplNrInt + 1
				)) )),

				(SELECT case ISNUMERIC(RIGHT(MaxRef, 1))
					when 1 then MaxRef + '/A'
					else case RIGHT(MaxRef, 1)
						when 'Z' then ''
						else LEFT(MaxRef, LEN(MaxRef) - 1) + CHAR(ASCII(RIGHT(MaxRef, 1)) + 1) end
					end
				FROM
				(SELECT MAX(ComplNr) as MaxRef FROM @tempTrans WHERE ComplianceAssignNumbersOrderDate <= @complianceAssignNumbersOrderDate) x)
				)";

			var dbConn = ((IDbConnected)new BusinessObjectFactory()).Connection;

			var command = dbConn.Command(selectSql);
			command.AddParameterBasedOnDbColumn("@currComp", GlbCompany.CurrentCompany.PK.ToGuid(), AccTransactionHeaderSchema.AH_GC);
			command.AddParameterBasedOnDbColumn("@subType", sequence.XD_SequenceClass.ToString(), AccTransactionHeaderSchema.AH_ComplianceSubType);

			command.AddParameterBasedOnDbColumn("@numPrefix", sequence.XD_Prefix.ToString(), AccComplianceSequenceSchema.XD_Prefix);
			command.AddParameterBasedOnDbColumn("@maxNumDigits", (byte)sequence.XD_MaximumNumberDigits, AccComplianceSequenceSchema.XD_MaximumNumberDigits);
			command.AddParameter("@startDate", SqlDbType.Date, startDate.IsValid ? startDate.ToDateTime() : DateTime.MinValue);
			command.AddParameter("@lastDate", SqlDbType.Date, lastDate.IsValid ? lastDate.ToDateTime() : DateTime.MaxValue);
			command.AddParameterBasedOnDbColumn("@startNr", (decimal)sequence.XD_StartNumber, AccComplianceSequenceSchema.XD_StartNumber);
			command.AddParameterBasedOnDbColumn("@nextNr", (decimal)sequence.XD_NextNumber, AccComplianceSequenceSchema.XD_NextNumber);

			command.AddParameterBasedOnDbColumn("@currComplNr", Parent.AH_TransactionReferenceInfo.OriginalValue.ToString(), AccTransactionHeaderSchema.AH_TransactionReference);
			command.AddParameter("@complianceAssignNumbersOrderDate", SqlDbType.Date, DateForComplianceAllocation.ToDateTime());

			object resultObject = command.ExecuteScalar();
			return resultObject?.ToString();
		}

		protected bool HasEInvoicingStatusValidationError(ZPropertyInfo fieldInfo)
		{
			var gc = Parent.AH_GC.IsValid ? Parent.AH_GC.ToGuid() : Guid.Empty;
			var eInvoicingIsEnabled = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(gc, Guid.Empty, Guid.Empty);
			return Parent.IsInDatabase
				&& eInvoicingIsEnabled
				&& Parent.EInvoicingTransaction.IsInvalidStatusToReQueue()
				&& fieldInfo.HasChanges;
		}
	}
}
