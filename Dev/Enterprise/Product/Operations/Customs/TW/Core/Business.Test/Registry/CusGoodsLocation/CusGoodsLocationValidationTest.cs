using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusGoodsLocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			currentElement.Validation.ValidateAll();
			AssertHasErrorContaining(currentElement.CustomsOfficeInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(currentElement.GoodsLocationInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(currentElement.MessageTypeInfo, MandatoryValidation.MustBeEntered);
		}

		[ExpectNoExceptions]
		public void TestValidateCustomsOffice()
		{
			var targetInfo = currentElement.CustomsOfficeInfo;
			var cannotBeDuplicated = ValidationConstants.CusGoodsLocation.CannotBeDuplicated;
			currentElement.Validation.ValidateCustomsOffice();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.MessageType = "IMP";
			currentElement.CustomsOffice = "66";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			NUnit.Framework.Assert.That(!currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			var item2 = collection.AddNew();
			item2.MessageType = "IMP";
			item2.CustomsOffice = "66";
			currentElement.Validation.ValidateCustomsOffice();
			NUnit.Framework.Assert.That(currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			item2.CustomsOffice = "77";
			currentElement.Validation.ValidateCustomsOffice();
			NUnit.Framework.Assert.That(!currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			currentElement.GoodsLocation = "ANP0055D";
			currentElement.CustomsOffice = "AA";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
			currentElement.GoodsLocation = "ANP0060D";
			currentElement.CustomsOffice = "DD";
			currentElement.Validation.ValidateCustomsOffice();
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
			currentElement.CustomsOffice = "CC";
			currentElement.Validation.ValidateCustomsOffice();
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
		}

		[ExpectNoExceptions]
		public void TestValidateMessageType()
		{
			var targetInfo = currentElement.MessageTypeInfo;
			var cannotBeDuplicated = ValidationConstants.CusGoodsLocation.CannotBeDuplicated;
			currentElement.Validation.ValidateMessageType();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.CustomsOffice = "66";
			currentElement.MessageType = "IMP";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			NUnit.Framework.Assert.That(!currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			var item2 = collection.AddNew();
			item2.MessageType = "IMP";
			item2.CustomsOffice = "66";
			currentElement.Validation.ValidateMessageType();
			NUnit.Framework.Assert.That(currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			item2.MessageType = "EXP";
			currentElement.Validation.ValidateMessageType();
			NUnit.Framework.Assert.That(!currentElement.RowErrors.Contains(cannotBeDuplicated), NUnit.Framework.Is.True);
			currentElement.MessageType = "TD";
			AssertHasErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.MessageType = "IMP";
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
			currentElement.MessageType = "EXP";
			AssertNoErrorContaining(targetInfo, ListValidation.InvalidCodeError);
		}

		public void TestValidateGoodsLocation()
		{
			var targetInfo = currentElement.GoodsLocationInfo;
			currentElement.Validation.ValidateGoodsLocation();
			AssertHasErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.GoodsLocation = "66";
			AssertNoErrorContaining(targetInfo, MandatoryValidation.MustBeEntered);
			currentElement.GoodsLocation = "AA";
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
			currentElement.GoodsLocation = "ANP0060D";
			currentElement.CustomsOffice = "DD";
			currentElement.Validation.ValidateGoodsLocation();
			AssertHasErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
			currentElement.CustomsOffice = "CC";
			currentElement.Validation.ValidateGoodsLocation();
			AssertNoErrorContaining(targetInfo, ValidationConstants.CusGoodsLocation.InvalidGoodsLocation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			new TestTWCreator(Factory).CreateRegistryItemCusGoodsLocation();
			collection = new CusGoodsLocationCollection(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			currentElement = collection.AddNew();
		}

		CusGoodsLocation currentElement;
		CusGoodsLocationCollection collection;
	}
}
