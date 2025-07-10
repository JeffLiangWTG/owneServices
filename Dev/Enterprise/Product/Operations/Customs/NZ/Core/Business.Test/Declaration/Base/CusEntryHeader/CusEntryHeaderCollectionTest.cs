using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Manifesting = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(CusEntryHeaderCollection))]
	public class CusEntryHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRemoveAndDeleteUnNecessaryCusEntryHeaders()
		{
			AssertEquals("Declaration.CusEntryHeader.IsCurrentEntryHeaderOnDeclaration", true, Declaration.CusEntryHeader.IsEntryHeaderCurrentDeclarationType(Declaration));
			CusEntryHeader activeEntryHeader = Declaration.CusEntryHeader;
			CusEntryHeader inactiveEntryHeader = Declaration.CustomsEntryHeaders.AddNew();
			inactiveEntryHeader.CH_IsActive = false;
			Declaration.CustomsEntryHeaders.RemoveAndDeleteUnNecessaryCusEntryHeaders();
			AssertEquals("activeEntryHeader.IsDeleted()", false, activeEntryHeader.IsDeleted);
			AssertEquals("inactiveEntryHeader.IsDeleted()", true, inactiveEntryHeader.IsDeleted);
		}

		#region TestCollectionCanHandleBusinessObjectsThatDontConformToTheRules
		public void TestCollectionCanHandleBusinessObjectsThatDontConformToTheRules()
		{
			CusEntryHeader entryHeader = Declaration.CusEntryHeader;
			JobDeclaration declaration2 = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader2 = declaration2.CusEntryHeader;

			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_JE = Declaration.PK;

			Factory.Save();

			Declaration.CustomsEntryHeaders.Add(manifestEntryHeader);
			declaration2.CustomsEntryHeaders.Add(manifestEntryHeader);

			AssertEquals("Declaration.CustomsEntryHeaders.Count", 2, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Declaration.CustomsEntryHeaders.Contains(ManifestEntryHeader)", true, Declaration.CustomsEntryHeaders.Contains(manifestEntryHeader));

			AssertEquals("Declaration2.CustomsEntryHeaders.Count", 2, declaration2.CustomsEntryHeaders.Count);
			AssertEquals("Declaration2.CustomsEntryHeaders.Contains(ManifestEntryHeader)", true, declaration2.CustomsEntryHeaders.Contains(manifestEntryHeader));

			AssertEquals("ManifestEntryHeader.CH_JE == Declaration.PK", Declaration.PK, manifestEntryHeader.CH_JE);
			AssertEquals("ManifestEntryHeader.HasChanges", false, manifestEntryHeader.HasChanges);
		}
		#endregion

		#region TestCustomsEntryHeadersIncludesManifestEntryHeaders
		public void TestCustomsEntryHeadersIncludesNewManifestEntryHeaders()
		{
			ZString manifestReference = NumberFountains.ECIManifestReferencePrefix + "01010101";
			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_BGMReference = manifestReference;
			Declaration.JE_DeclarationReference = manifestReference + "-1";
			Declaration.CustomsEntryHeaders.Load();
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Declaration.CustomsEntryHeaders[0].PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, Declaration.CustomsEntryHeaders[0].PK);
		}

		public void TestCustomsEntryHeadersIncludesOldManifestEntryHeaders()
		{
			ZString manifestReference = NumberFountains.OldECIManifestReferencePrefix + "01010101";
			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_BGMReference = manifestReference;
			Declaration.JE_DeclarationReference = manifestReference + "-1";
			Declaration.CustomsEntryHeaders.Load();
			AssertEquals("Declaration.CustomsEntryHeaders.Count", 1, Declaration.CustomsEntryHeaders.Count);
			AssertEquals("Declaration.CustomsEntryHeaders[0].PK == ManifestEntryHeader.PK", manifestEntryHeader.PK, Declaration.CustomsEntryHeaders[0].PK);
		}
		#endregion

		public void TestAddNewReturnsTheRightTypeOfCusEntryHeaders()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			AssertEquals("Declaration.CustomsEntryHeaders.AddNew()", typeof(FormalEntry.CusEntryHeader), Declaration.CustomsEntryHeaders.AddNew().GetType());

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			AssertEquals("Declaration.CustomsEntryHeaders.AddNew()", typeof(ECIWriteOff.CusEntryHeader), Declaration.CustomsEntryHeaders.AddNew().GetType());

			Declaration.JE_DeclarationReference = NumberFountains.OldECIManifestReferencePrefix + "00001000-1";
			AssertEquals("Declaration.CustomsEntryHeaders.AddNew()", typeof(Manifesting.CusEntryHeader), Declaration.CustomsEntryHeaders.AddNew().GetType());

			Declaration.JE_DeclarationReference = NumberFountains.ECIManifestReferencePrefix + "00001000-1";
			AssertEquals("Declaration.CustomsEntryHeaders.AddNew()", typeof(Manifesting.CusEntryHeader), Declaration.CustomsEntryHeaders.AddNew().GetType());
		}

		#region GetCollectionToTest
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntryHeaderCollection(Declaration, Factory);
		}
		#endregion

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region GetNewJobDeclaration
		protected virtual JobDeclaration GetNewJobDeclaration()
		{
			return JobDeclaration.New(Factory);
		}
		#endregion
	}
}
