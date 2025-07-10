using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(OnlineApplicationDocType))]
	sealed class OnlineApplicationDocTypeTest : RegistryBusinessObjectTemplateTestCase<OnlineApplicationDocType>
	{
		#region Properties

		public void TestRT_PK()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			OnlineApplicationDocType oaDocType = NewPopulatedBusinessObject();
			oaDocType.RT_PK = docType.PK;

			AssertEquals(docType.PK, oaDocType.RT_PK);
		}

		public void TestDocTypeDescription()
		{
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();
			docType.RT_Desc = "A test doc type";
			OnlineApplicationDocType oaDocType = NewPopulatedBusinessObject();
			oaDocType.RT_PK = docType.PK;

			AssertEquals("A test doc type", oaDocType.DocTypeDescription);
		}

		public void TestRefDocTypes()
		{
			RefDocType included1 = Factory.New<RefDocType>();
			included1.RT_ReferenceType = Core.Constants.ReferenceTypes.All;
			RefDocType included2 = Factory.New<RefDocType>();
			included2.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			RefDocType notIncluded = Factory.New<RefDocType>();
			notIncluded.RT_ReferenceType = Core.Constants.ReferenceTypes.SupplyChainLogistics;

			OnlineApplicationDocType oaDocType = NewPopulatedBusinessObject();
			AssertCollectionContains(included1, oaDocType.RefDocTypes);
			AssertCollectionContains(included2, oaDocType.RefDocTypes);
			AssertCollectionNotContains(notIncluded, oaDocType.RefDocTypes);
		}

		#endregion

		#region Validation

		public void TestValidation()
		{
			OnlineApplicationDocType oaDocType = NewPopulatedBusinessObject();
			RefDocType docType = Factory.NewWithValidTestData<RefDocType>();

			oaDocType.RunPreSaveValidation();
			AssertHasErrors("Mandatory", oaDocType.RT_PKInfo);

			oaDocType.RT_PK = ZGuid.Invalid;
			AssertHasErrors(oaDocType.RT_PKInfo);

			oaDocType.RT_PK = docType.PK;
			AssertHasErrors("not a HR doc type", oaDocType.RT_PKInfo);

			docType.RT_ReferenceType = Core.Constants.ReferenceTypes.HumanResourcesStaffEmployment;
			oaDocType.ClearAllNotifications();
			oaDocType.ValidateRT_PK();
			AssertNoErrors(oaDocType.RT_PKInfo);
		}

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override OnlineApplicationDocType GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override OnlineApplicationDocType GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		OnlineApplicationDocType NewPopulatedBusinessObject()
		{
			OnlineApplicationDocType result = new OnlineApplicationDocType(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
