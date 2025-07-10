using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BaseTariffBulkChangeTest : NonPersistentBusinessObjectTestCase
	{
		protected virtual string NoChangeTariffNum => "0104.00.00";

		protected virtual string NoChangeTariffNumForDB => NoChangeTariffNum;

		protected virtual string One2OneOldTariffNum => "0105.92.00";

		protected virtual string One2OneOldTariffNumForDB => One2OneOldTariffNum;

		protected virtual string One2OneNewTariffNum => "0105.94.00";

		protected virtual string One2ManyOldInvalidTariffNum => "2005.90.00";

		protected virtual string One2OneOldTariffNumWithDifferentSuffix => One2OneOldTariffNum;

		protected virtual string One2OneOldTariffNumForDBWithDifferentSuffix => One2OneOldTariffNum;

		protected virtual string One2OneNewTariffNumWithDifferentSuffix => One2OneNewTariffNum;

		protected virtual string One2ManyOldInvalidTariffNumForDB => One2ManyOldInvalidTariffNum;

		protected virtual string One2ManyOldInvalidNew1TariffNum => "2005.91.00";

		protected virtual string One2ManyOldInvalidNew2TariffNum => "2005.99.00";

		protected virtual string One2ManyOldValidTariffNum => "2827.39.00";

		protected virtual string One2ManyOldValidTariffNumForDB => One2ManyOldValidTariffNum;

		protected virtual string One2ManyOldValidNew1TariffNum => "2827.39.00";

		protected virtual string One2ManyOldValidNew2TariffNum => "2852.00.90";

		protected virtual string One2ManyOldValidTariffNumWithDifferentSuffix => One2ManyOldValidTariffNum;

		protected virtual string One2ManyOldValidTariffNumForDBWithDifferentSuffix => One2ManyOldValidTariffNum;

		protected virtual string One2ManyOldValidNew1TariffNumWithDifferentSuffix => One2ManyOldValidNew1TariffNum;

		protected virtual string One2ManyOldValidNew2TariffNumWithDifferentSuffix => One2ManyOldValidNew2TariffNum;

		protected virtual string One2ManyOldValidNew3TariffNumWithDifferentSuffix => One2ManyOldValidNew2TariffNum;

		protected virtual string One2OneClass2OldTariffNum => "9030.39.00";

		protected virtual string One2OneClass2NewTariffNum => "9030.33.00";

		protected virtual string One2MannyClass2OldTariffNum => "9030.83.00";

		protected virtual string One2MannyClass2New1TariffNum => "9030.20.00";

		protected virtual string One2MannyClass2New2TariffNum => "9030.32.00";

		protected virtual string One2MannyClass2New3TariffNum => "9030.39.00";

		protected virtual string One2MannyClass2New4TariffNum => "9030.84.00";

		protected virtual string One2MannyNoDotsOldTariffNum => "9017.20.90";

		protected virtual string One2MannyNoDotsOldTariffNumForDB => One2MannyNoDotsOldTariffNum;

		protected virtual string One2MannyNoDotsNew1TariffNum => "8486.40.10";

		protected virtual string One2MannyNoDotsNew2TariffNum => "9017.20.90";

		protected virtual string One2MannyNoDotsNew3TariffNum => "9017.20.91";

		protected const string NoChangeLookupCode = "L1";
		protected const string NoChangeFreeStandingLookupCode = "L2";
		protected const string One2OneLookupCode = "L3";
		protected const string One2OneFreeStandingLookupCode = "L4";
		protected const string One2OneLookupCodeWithDifferentSuffix = "L5";
		protected const string One2ManyOldInvalidLookupCode = "L6";
		protected const string One2ManyOldInvalidFreeStandingLookupCode = "L7";
		protected const string One2ManyOldInvalidLookupCode2 = "L8";
		protected const string One2ManyOldValidLookupCode = "L9";
		protected const string One2ManyOldValidFreeStandingLookupCode = "L10";
		protected const string One2ManyOldValidLookupCodeWithDifferentSuffix = "L11";
		protected const string One2OneLookupCode2 = "L12";
		protected const string One2ManyLookupCode2 = "L13";
		protected const string One2ManynoDotsLookupCode = "L14";
		protected const string NewLookupCode = "L15";
		protected const string AnotherNewLookupCode = "L16";

		protected const string NoChangePartNum = "P1";
		protected const string One2OnePartNum = "P2";
		protected const string One2OnePartNumWithDifferentSuffix = "P3";
		protected const string One2ManyOldInvalidPartNum = "P4";
		protected const string One2ManyOldInvalidPartNum2 = "P5";
		protected const string One2ManyOldValidPartNum = "P6";
		protected const string One2ManyOldValidPartNumWithDifferentSuffix = "P7";
		protected const string One2OneWithTwoClassesPartNum = "P8";
		protected const string AnotherOne2OneWithTwoClassesPartNum = "P9";
		protected const string One2OneClass2OnlyPartNum = "P10";
		protected const string One2ManyWithTwoClassesPartNum = "P11";
		protected const string AnotherOne2ManyWithTwoClassesPartNum = "P12";
		protected const string One2ManyWithOnlyClass2PartNum = "P13";
		protected const string One2ManyNoDotsPartNum = "P14";

		protected OrgSupplierPart one2ManyValidPart;
		protected BaseCusClassification one2OneClassification;

		public void CreateSomePartsAndClassifications(ZString classType1, ZString classType2)
		{
			var noChangePart = Factory.New<OrgSupplierPart>();
			noChangePart.OP_PartNum = NoChangePartNum;
			var noChangeClassification = noChangePart.ClassificationsForBinding.AddNew();
			noChangeClassification.CC_TariffNum = NoChangeTariffNumForDB;
			noChangeClassification.CC_ClassificationType = classType1;
			noChangeClassification.CC_LookupCode = NoChangeLookupCode;
			var noChangeFreeStandingClassification = Factory.New<BaseCusClassification>();
			noChangeFreeStandingClassification.CC_TariffNum = NoChangeTariffNumForDB;
			noChangeFreeStandingClassification.CC_ClassificationType = classType1;
			noChangeFreeStandingClassification.CC_LookupCode = NoChangeFreeStandingLookupCode;

			var one2OnePart = Factory.New<OrgSupplierPart>();
			one2OnePart.OP_PartNum = One2OnePartNum;
			one2OneClassification = one2OnePart.ClassificationsForBinding.AddNew();
			one2OneClassification.CC_TariffNum = One2OneOldTariffNumForDB;
			one2OneClassification.CC_ClassificationType = classType1;
			one2OneClassification.CC_LookupCode = One2OneLookupCode;
			var one2OneFreeStandingClassification = Factory.New<BaseCusClassification>();
			one2OneFreeStandingClassification.CC_TariffNum = One2OneOldTariffNumForDB;
			one2OneFreeStandingClassification.CC_ClassificationType = classType1;
			one2OneFreeStandingClassification.CC_LookupCode = One2OneFreeStandingLookupCode;

			if (One2OneOldTariffNum != One2OneOldTariffNumWithDifferentSuffix)
			{
				var one2OnePartWithDifferentSuffix = Factory.New<OrgSupplierPart>();
				one2OnePartWithDifferentSuffix.OP_PartNum = One2OnePartNumWithDifferentSuffix;
				var one2OneClassificationWithDifferentSuffix = one2OnePartWithDifferentSuffix.ClassificationsForBinding.AddNew();
				one2OneClassificationWithDifferentSuffix.CC_TariffNum = One2OneOldTariffNumForDBWithDifferentSuffix;
				one2OneClassificationWithDifferentSuffix.CC_ClassificationType = classType1;
				one2OneClassificationWithDifferentSuffix.CC_LookupCode = One2OneLookupCodeWithDifferentSuffix;
			}

			var one2ManyInvalidPart = Factory.New<OrgSupplierPart>();
			one2ManyInvalidPart.OP_PartNum = One2ManyOldInvalidPartNum;
			var one2ManyInvalidClassification = one2ManyInvalidPart.ClassificationsForBinding.AddNew();
			one2ManyInvalidClassification.CC_TariffNum = One2ManyOldInvalidTariffNumForDB;
			one2ManyInvalidClassification.CC_ClassificationType = classType1;
			one2ManyInvalidClassification.CC_LookupCode = One2ManyOldInvalidLookupCode;
			var one2ManyInvalidFreeStandingClassification = Factory.New<BaseCusClassification>();
			one2ManyInvalidFreeStandingClassification.CC_TariffNum = One2ManyOldInvalidTariffNumForDB;
			one2ManyInvalidFreeStandingClassification.CC_ClassificationType = classType1;
			one2ManyInvalidFreeStandingClassification.CC_LookupCode = One2ManyOldInvalidFreeStandingLookupCode;

			var one2ManyInvalidPart2 = Factory.New<OrgSupplierPart>();
			one2ManyInvalidPart2.OP_PartNum = One2ManyOldInvalidPartNum2;
			var one2ManyInvalidClassification2 = one2ManyInvalidPart2.ClassificationsForBinding.AddNew();
			one2ManyInvalidClassification2.CC_TariffNum = One2ManyOldInvalidTariffNumForDB;
			one2ManyInvalidClassification2.CC_ClassificationType = classType1;
			one2ManyInvalidClassification2.CC_LookupCode = One2ManyOldInvalidLookupCode2;

			one2ManyValidPart = Factory.New<OrgSupplierPart>();
			one2ManyValidPart.OP_PartNum = One2ManyOldValidPartNum;
			var one2ManyValidClassification = one2ManyValidPart.ClassificationsForBinding.AddNew();
			one2ManyValidClassification.CC_TariffNum = One2ManyOldValidTariffNumForDB;
			one2ManyValidClassification.CC_ClassificationType = classType1;
			one2ManyValidClassification.CC_LookupCode = One2ManyOldValidLookupCode;
			var one2ManyValidFreeStandingClassification = Factory.New<BaseCusClassification>();
			one2ManyValidFreeStandingClassification.CC_TariffNum = One2ManyOldValidTariffNumForDB;
			one2ManyValidFreeStandingClassification.CC_ClassificationType = classType1;
			one2ManyValidFreeStandingClassification.CC_LookupCode = One2ManyOldValidFreeStandingLookupCode;

			if (One2ManyOldValidTariffNum != One2ManyOldValidTariffNumWithDifferentSuffix)
			{
				var one2ManyValidPartWithDifferentSuffix = Factory.New<OrgSupplierPart>();
				one2ManyValidPartWithDifferentSuffix.OP_PartNum = One2ManyOldValidPartNumWithDifferentSuffix;
				var one2ManyValidClassificationWithDifferentSuffix = one2ManyValidPartWithDifferentSuffix.ClassificationsForBinding.AddNew();
				one2ManyValidClassificationWithDifferentSuffix.CC_TariffNum = One2ManyOldValidTariffNumForDBWithDifferentSuffix;
				one2ManyValidClassificationWithDifferentSuffix.CC_ClassificationType = classType1;
				one2ManyValidClassificationWithDifferentSuffix.CC_LookupCode = One2ManyOldValidLookupCodeWithDifferentSuffix;
			}

			var one2ManyNoDotsPart = Factory.New<OrgSupplierPart>();
			one2ManyNoDotsPart.OP_PartNum = One2ManyNoDotsPartNum;
			var one2OneNoDotsClassification = one2ManyNoDotsPart.ClassificationsForBinding.AddNew();
			one2OneNoDotsClassification.CC_TariffNum = One2MannyNoDotsOldTariffNumForDB;
			one2OneNoDotsClassification.CC_ClassificationType = classType1;
			one2OneNoDotsClassification.CC_LookupCode = One2ManynoDotsLookupCode;

			if (!classType2.IsEmpty)
			{
				var one2OneLookupCode2Classification = Factory.New<BaseCusClassification>();
				one2OneLookupCode2Classification.CC_TariffNum = One2OneClass2OldTariffNum;
				one2OneLookupCode2Classification.CC_ClassificationType = classType2;
				one2OneLookupCode2Classification.CC_LookupCode = One2OneLookupCode2;

				var one2OneWithTwoClassesPart = Factory.New<OrgSupplierPart>();
				one2OneWithTwoClassesPart.OP_PartNum = One2OneWithTwoClassesPartNum;
				one2OneWithTwoClassesPart.ClassificationsForBinding.Add(one2OneClassification);
				one2OneWithTwoClassesPart.ClassificationsForBinding.Add(one2OneLookupCode2Classification);

				var anotherOne2OneWithTwoClassesPart = Factory.New<OrgSupplierPart>();
				anotherOne2OneWithTwoClassesPart.OP_PartNum = AnotherOne2OneWithTwoClassesPartNum;
				anotherOne2OneWithTwoClassesPart.ClassificationsForBinding.Add(one2OneLookupCode2Classification);
				anotherOne2OneWithTwoClassesPart.ClassificationsForBinding.Add(one2OneClassification);

				var one2ManyLookupCode2Classification = Factory.New<BaseCusClassification>();
				one2ManyLookupCode2Classification.CC_TariffNum = One2MannyClass2OldTariffNum;
				one2ManyLookupCode2Classification.CC_ClassificationType = classType2;
				one2ManyLookupCode2Classification.CC_LookupCode = One2ManyLookupCode2;

				var one2ManyWithTwoClassesPart = Factory.New<OrgSupplierPart>();
				one2ManyWithTwoClassesPart.OP_PartNum = One2ManyWithTwoClassesPartNum;
				one2ManyWithTwoClassesPart.ClassificationsForBinding.Add(one2ManyInvalidClassification);
				one2ManyWithTwoClassesPart.ClassificationsForBinding.Add(one2ManyLookupCode2Classification);

				var anotherOne2ManyWithTwoClassesPart = Factory.New<OrgSupplierPart>();
				anotherOne2ManyWithTwoClassesPart.OP_PartNum = AnotherOne2ManyWithTwoClassesPartNum;
				anotherOne2ManyWithTwoClassesPart.ClassificationsForBinding.Add(one2ManyLookupCode2Classification);
				anotherOne2ManyWithTwoClassesPart.ClassificationsForBinding.Add(one2ManyInvalidClassification);

				var one2ManyWithOnlyClass2Part = Factory.New<OrgSupplierPart>();
				one2ManyWithOnlyClass2Part.OP_PartNum = One2ManyWithOnlyClass2PartNum;
				one2ManyWithOnlyClass2Part.ClassificationsForBinding.Add(one2ManyLookupCode2Classification);
			}
			Factory.Save();
		}
	}
}
