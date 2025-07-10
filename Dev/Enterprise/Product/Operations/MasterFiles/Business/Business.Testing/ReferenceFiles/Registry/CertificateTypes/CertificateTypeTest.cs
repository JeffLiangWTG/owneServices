using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.ReferenceFiles.Registry.CertificateTypes;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CertificateType))]
	sealed class CertificateTypeTest : RegistryBusinessObjectTemplateTestCase<CertificateType>
	{
		public void TestSetupValues()
		{
			CertificateType type = ParentCollection.AddNew();
			AssertCertificateType(ZString.Empty, ZString.Empty, false, false, false, ZString.Empty, type);

			type.SetupValues("AAA", "A desc", true, true, true, AlertTypeList.Codes.ErrorAlert);
			AssertCertificateType("AAA", "A desc", true, true, true, AlertTypeList.Codes.ErrorAlert, type);
		}

		public void TestDelete_System()
		{
			string errorMessage = "You cannot delete or disable this type.";

			CertificateType type = ParentCollection.AddNew();
			type.IsSystem = true;

			AssertExceptionThrown(typeof(CannotDeleteException), errorMessage, delegate
			{ ParentCollection.RemoveAndDelete(type); });
		}

		[ExpectNoExceptions()]
		public void TestDelete_NonSystem()
		{
			CertificateType type = ParentCollection.AddNew();
			type.IsSystem = false;
			ParentCollection.RemoveAndDelete(type);
			AssertEquals(true, type.IsDeleted);
			AssertEquals(0, ParentCollection.Count);
		}

		public void TestReadOnly()
		{
			CertificateType type = ParentCollection.AddNew();
			type.IsSystem = true;

			AssertEquals("Should be ReadOnly", true, type.ReadOnly);

			type.ReadOnly = false;
			AssertEquals("Should be ReadOnly", true, type.ReadOnly);

			type.IsSystem = false;
			type.ReadOnly = true;
			AssertEquals("Should be NOT ReadOnly", true, type.ReadOnly);

			type.ReadOnly = true;
			AssertEquals("Should be ReadOnly", true, type.ReadOnly);
		}

		#region Validation

		public void TestValidateCode()
		{
			CertificateType type1 = ParentCollection.AddNew();
			type1.Code = "AAA";

			CertificateType type2 = ParentCollection.AddNew();
			type2.Code = "";

			AssertHasError("Should be Error", type2.CodeInfo, "Please enter a value.");

			type2.Code = "AAA";
			AssertHasError("Should be Error", type2.CodeInfo, "Duplicate Code is entered.");

			type2.Code = "BBB";
			AssertNoNotifications("Should be NO Errors", type2.CodeInfo);
		}

		public void TestValidateDescription()
		{
			CertificateType type = ParentCollection.AddNew();
			type.Description = "";

			AssertHasError("Should be Error", type.DescriptionInfo, "Please enter a value.");

			type.Description = "Description";
			AssertNoNotifications("Should be NO Errors", type.DescriptionInfo);
		}

		#endregion

		public void TestICodeDescriptionMembers()
		{
			CertificateType type = ParentCollection.AddNew();
			ICodeDescription typeAsCodeDescription = type;

			type.Code = "AAA";
			type.Description = "AAA Desc";

			AssertEquals("Code", "AAA", typeAsCodeDescription.Code);
			AssertEquals("Description", "AAA Desc", typeAsCodeDescription.Description);
			AssertNull("PK", typeAsCodeDescription.PK);

			type.Code = "BBB";
			type.Description = "BBB Desc";

			AssertEquals("Code", "BBB", typeAsCodeDescription.Code);
			AssertEquals("Description", "BBB Desc", typeAsCodeDescription.Description);
			AssertNull("PK", typeAsCodeDescription.PK);
		}

		public void TestAlertType()
		{
			var bizO = ParentCollection.AddNew();

			bizO.ValidateAlertType();

			AssertEquals(string.Empty, bizO.AlertType);
			AssertHasError(bizO.AlertTypeInfo, "Please enter a value.");

			bizO.AlertType = "NotValidAlertType";
			AssertHasError(bizO.AlertTypeInfo, "Enter a valid selection.");

			bizO.AlertType = AlertTypeList.Codes.ErrorAlert;
			AssertNoErrors(bizO.AlertTypeInfo);
		}

		#region Implementation

		void AssertCertificateType(ZString code, ZString description, ZBool isMandatory, ZBool isUnique, ZBool isSystem, ZString alertType, CertificateType type)
		{
			AssertEquals("Code", code, type.Code);
			AssertEquals("Description", description, type.Description);
			AssertEquals("IsMandatory", isMandatory, type.IsMandatory);
			AssertEquals("IsUnique", isUnique, type.IsUnique);
			AssertEquals("IsSystem", isSystem, type.IsSystem);
			AssertEquals("AlertType", alertType, type.AlertType);
		}

		CertificateType NewPopulatedBusinessObject()
		{
			CertificateType type = ParentCollection.AddNew();
			type.SetupValues("AAA", "A desc", false, false, false, AlertTypeList.Codes.NoAlert);

			return type;
		}

		#region Overrides

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CertificateType GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override CertificateType GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CertificateType(ParentCollection);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ParentCollection = new CertificateTypeCollection();
		}

		#endregion

		CertificateTypeCollection ParentCollection { get; set; }

		#endregion
	}
}
