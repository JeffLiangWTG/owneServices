using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageSendingObjectParentValidation))]
	sealed class LicensingMessageSendingObjectParentValidationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddErrorAfterValidation()
		{
			var objectForTest = Factory.NewWithValidTestData<ObjectForTest>();
			objectForTest.Validation.ErrorMessageForValidateStringProperty = "Error Message For Validate StringProperty";
			var licensingMessageSendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(objectForTest, ControllingMessageTypeList.Codes.NX101);
			var validationForTest = new ValidationForTest(licensingMessageSendingObjectParent, objectForTest, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var errorCollection = new MessageSendingNotificationCollection();

			validationForTest.AddErrorAfterValidationForTest(errorCollection, objectForTest.Validation.ValidateStringProperty, objectForTest.StringPropertyInfo, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: Error Message For Validate StringProperty").Using(CustomComparers.TypeComparison), "error message is not expected");
		}

		[ExpectNoExceptions]
		public void TestAddErrorIfEmpty()
		{
			var objectForTest = Factory.NewWithValidTestData<ObjectForTest>();
			var licensingMessageSendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(objectForTest, ControllingMessageTypeList.Codes.NX101);
			var validationForTest = new ValidationForTest(licensingMessageSendingObjectParent, objectForTest, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var errorCollection = new MessageSendingNotificationCollection();

			objectForTest.StringProperty = string.Empty;
			validationForTest.AddErrorIfEmptyForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: You have not entered a value.").Using(CustomComparers.TypeComparison), "error message is not expected");

			errorCollection.Clear();
			validationForTest.AddErrorIfEmptyForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, "Test For Validation", columnName: "String Column");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for ColumnName: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: You have not entered a String Column.").Using(CustomComparers.TypeComparison), "Test for ColumnName: error message is not expected");

			errorCollection.Clear();
			validationForTest.AddErrorIfEmptyForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, "Test For Validation", errorMessage: "String Column Error Message.");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for ErrorMessage: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: String Column Error Message.").Using(CustomComparers.TypeComparison), "Test for ErrorMessage: error message is not expected");

			errorCollection.Clear();
			objectForTest.StringProperty = "asdzxc";
			validationForTest.AddErrorIfEmptyForTest(errorCollection, objectForTest.Validation, objectForTest.DecimalPropertyInfo, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(0), "errorCollection should not have any error");
		}

		[ExpectNoExceptions]
		public void TestAddErrorIfAddErrorIfNotGreaterThan0()
		{
			var objectForTest = Factory.NewWithValidTestData<ObjectForTest>();
			var licensingMessageSendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(objectForTest, ControllingMessageTypeList.Codes.NX101);
			var validationForTest = new ValidationForTest(licensingMessageSendingObjectParent, objectForTest, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var errorCollection = new MessageSendingNotificationCollection();

			objectForTest.DecimalProperty = 0;
			validationForTest.AddErrorIfNotGreaterThan0ForTest(errorCollection, objectForTest.Validation, objectForTest.DecimalPropertyInfo, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: Please enter a 'DecimalProperty' greater than 0.").Using(CustomComparers.TypeComparison), "error message is not expected");

			errorCollection.Clear();
			objectForTest.DecimalProperty = -1;
			validationForTest.AddErrorIfNotGreaterThan0ForTest(errorCollection, objectForTest.Validation, objectForTest.DecimalPropertyInfo, "Test For Validation", columnName: "Decimal Column");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for ColumnName: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: Please enter a 'Decimal Column' greater than 0.").Using(CustomComparers.TypeComparison), "Test for ColumnName: error message is not expected");

			errorCollection.Clear();
			objectForTest.DecimalProperty = decimal.MinValue;
			validationForTest.AddErrorIfNotGreaterThan0ForTest(errorCollection, objectForTest.Validation, objectForTest.DecimalPropertyInfo, "Test For Validation", errorMessage: "Decimal Column Error Message.");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for ErrorMessage: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: Decimal Column Error Message.").Using(CustomComparers.TypeComparison), "Test for ErrorMessage: error message is not expected");

			errorCollection.Clear();
			objectForTest.DecimalProperty = 1;
			validationForTest.AddErrorIfNotGreaterThan0ForTest(errorCollection, objectForTest.Validation, objectForTest.DecimalPropertyInfo, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(0), "errorCollection should not have any error");
		}

		[ExpectNoExceptions]
		public void TestAddErrorIfEmptyOrNotInList_CodeDescriptionPairList()
		{
			var codeDescriptionPairList = new CodeDescriptionPairList();
			codeDescriptionPairList.AddPair("C01");
			codeDescriptionPairList.AddPair("C02");

			var objectForTest = Factory.NewWithValidTestData<ObjectForTest>();
			var licensingMessageSendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(objectForTest, ControllingMessageTypeList.Codes.NX101);
			var validationForTest = new ValidationForTest(licensingMessageSendingObjectParent, objectForTest, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var errorCollection = new MessageSendingNotificationCollection();

			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, codeDescriptionPairList, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for empty: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: You have not entered a value.").Using(CustomComparers.TypeComparison), "Test for empty: error message is not expected");

			errorCollection.Clear();
			objectForTest.StringProperty = "UNK";
			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, codeDescriptionPairList, "Test For Validation", columnName: "String Property");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for not in list: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: String Property: The code you have selected is not in the list.").Using(CustomComparers.TypeComparison), "Test for not in list: error message is not expected");

			errorCollection.Clear();
			objectForTest.StringProperty = "C01";
			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, codeDescriptionPairList, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(0), "errorCollection should has no error");
		}

		[ExpectNoExceptions]
		public void TestAddErrorIfEmptyOrNotInList_IEnumerableZString()
		{
			var list = new List<ZString>
			{
				"C01",
				"C02"
			};

			var objectForTest = Factory.NewWithValidTestData<ObjectForTest>();
			var licensingMessageSendingObjectParent = LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(objectForTest, ControllingMessageTypeList.Codes.NX101);
			var validationForTest = new ValidationForTest(licensingMessageSendingObjectParent, objectForTest, Env.Security.CustomsDeclarationSendWithMessageErrors);
			var errorCollection = new MessageSendingNotificationCollection();

			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, list, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for empty: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: You have not entered a value.").Using(CustomComparers.TypeComparison), "Test for empty: error message is not expected");

			errorCollection.Clear();
			objectForTest.StringProperty = "UNK";
			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, list, "Test For Validation", errorMessageForNotInList: "'UNK' is not in the list.");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(1), "Test for not in list: errorCollection should has 1 error");
			NUnit.Framework.Assert.That(errorCollection[0].Message, NUnit.Framework.Is.EqualTo("Test For Validation: 'UNK' is not in the list.").Using(CustomComparers.TypeComparison), "Test for not in list: error message is not expected");

			errorCollection.Clear();
			objectForTest.StringProperty = "C01";
			validationForTest.AddErrorIfEmptyOrNotInListForTest(errorCollection, objectForTest.Validation, objectForTest.StringPropertyInfo, list, "Test For Validation");
			NUnit.Framework.Assert.That(errorCollection.Count, NUnit.Framework.Is.EqualTo(0), "errorCollection should has no error");
		}

		class ValidationForTest : LicensingMessageSendingObjectParentValidation
		{
			public ValidationForTest(LicensingMessageSendingObjectParent parent, JobDeclaration declaration, SecurityCheckpoint sendMessageWithErrorsSecurityCheckpoint, bool refreshValidation = true) : base(parent, declaration, null, sendMessageWithErrorsSecurityCheckpoint, refreshValidation)
			{
			}

			public void AddErrorAfterValidationForTest(MessageSendingNotificationCollection errorColelction, Action validateAction, ZPropertyInfo propertyInfo, string prefix)
			{
				AddErrorAfterValidation(errorColelction, validateAction, propertyInfo, prefix);
			}

			public bool AddErrorIfEmptyForTest(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, string prefix, string errorMessage = null, string columnName = null)
			{
				return AddErrorIfEmpty(errorColelction, validator, propertyInfo, prefix, errorMessage: errorMessage, columnName: columnName);
			}

			public void AddErrorIfNotGreaterThan0ForTest(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, string prefix, string errorMessage = null, string columnName = null)
			{
				AddErrorIfNotGreaterThan0(errorColelction, validator, propertyInfo, prefix, errorMessage: errorMessage, columnName: columnName);
			}

			public void AddErrorIfEmptyOrNotInListForTest(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, CodeDescriptionPairList list, string prefix, string errorMessageForEmpty = null, string errorMessageForNotInList = null, string columnName = null)
			{
				AddErrorIfEmptyOrNotInList(errorColelction, validator, propertyInfo, list, prefix, errorMessageForEmpty: errorMessageForEmpty, errorMessageForNotInList: errorMessageForNotInList, columnName: columnName);
			}

			public void AddErrorIfEmptyOrNotInListForTest(MessageSendingNotificationCollection errorColelction, IValidationInternals validator, ZPropertyInfo propertyInfo, IEnumerable<ZString> list, string prefix, string errorMessageForEmpty = null, string errorMessageForNotInList = null, string columnName = null)
			{
				AddErrorIfEmptyOrNotInList(errorColelction, validator, propertyInfo, list, prefix, errorMessageForEmpty: errorMessageForEmpty, errorMessageForNotInList: errorMessageForNotInList, columnName: columnName);
			}
		}

		class ObjectForTest : JobDeclaration
		{
			public ObjectForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override Customs.Business.JobDeclarationValidation GetNewValidation() => Factory.GetCachedValue("ObjectForTest.GetNewValidation", () => new ObjectForTestValidation(this));

			public new ObjectForTestValidation Validation => (ObjectForTestValidation)base.Validation;

			#region String

			ZString stringProperty;

			public ZString StringProperty
			{
				get => stringProperty;
				set
				{
					SetNonPersistentPropertyValue(StringPropertyInfo, ref stringProperty, value);
				}
			}

			public ZPropertyInfo StringPropertyInfo => GetZPropertyInfo(nameof(StringProperty));

			#endregion

			#region Decimal

			ZDecimal decimalProperty;

			[ResourceStringData("NPBO:Enterprise.Customs.TW.Business.Testing.ObjectForTest|DecimalProperty", Caption = "DecimalProperty")]
			public ZDecimal DecimalProperty
			{
				get => decimalProperty;
				set
				{
					SetNonPersistentPropertyValue(DecimalPropertyInfo, ref decimalProperty, value);
				}
			}

			public ZPropertyInfo DecimalPropertyInfo => GetZPropertyInfo(nameof(DecimalProperty));

			#endregion
		}

		class ObjectForTestValidation : JobDeclarationValidation
		{
			public ObjectForTestValidation(ObjectForTest parent) : base(parent)
			{
				this.parent = parent;
			}

			readonly ObjectForTest parent;

			public override Type AutoValidationType => typeof(ObjectForTest);

			public override void ValidateAll()
			{
				ValidateStringProperty();
			}

			public string ErrorMessageForValidateStringProperty { get; set; }

			public void ValidateStringProperty()
			{
				if (!string.IsNullOrEmpty(ErrorMessageForValidateStringProperty))
				{
					(this as IValidationInternals).Validate(parent.StringPropertyInfo, () => parent.StringPropertyInfo.AddMessageError(ErrorMessageForValidateStringProperty));
				}
			}
		}
	}

	abstract class LicensingMessageSendingObjectParentValidationTest<ValidatorType, SendingObjectType> : TestCaseWithFactory
		where ValidatorType : LicensingMessageSendingObjectParentValidation
		where SendingObjectType : LicensingMessageSendingObject
	{
		protected abstract string MessageType { get; }

		protected virtual LicensingMessageSendingObjectParent GetMessageSendingObjectParent(JobDeclaration declaration, CusTWControllingMessageHeader header)
		{
			return LicensingMessageSendingObjectParent.GetLicensingMessageSendingObjectParent(declaration, MessageType);
		}

		protected virtual SendingObjectType GetMessageSendingObject(JobDeclaration declaration, CusTWControllingMessageHeader header, LicensingMessageSendingObjectParent parent)
		{
			return parent.SendingObjectsCollection[0] as SendingObjectType;
		}

		protected (JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusTWControllingMessageHeader messageHeader, LicensingMessageSendingObjectParent parent, SendingObjectType sendingObject) SetupData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Invoice Units of Measurement");
			helper.CreateCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "Processing Unit");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ControlAgency, "The Control Agency to which the Processing Unit belongs", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "TW");
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "10", "總局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWReceivingUnit, "20", "基隆分局", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, cusTariffType.PK, "10064000004", ZDateTime.BrettsBirthday, ZDateTime.Today);
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.PermitCusSupportingCollection.AddNew();

			var messageHeader = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			messageHeader.TW1_ControllingMessageType = MessageType;
			messageHeader.TW1_ControllingAgency = "TCA";
			var parent = GetMessageSendingObjectParent(declaration, messageHeader);
			return (declaration, invoiceHeader, invoiceLine, messageHeader, parent, GetMessageSendingObject(declaration, messageHeader, parent));
		}
	}
}
