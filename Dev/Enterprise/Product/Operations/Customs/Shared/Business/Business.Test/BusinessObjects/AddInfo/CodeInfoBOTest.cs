using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CodeInfo))]
	public abstract class CodeInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestToString()
		{
			CodeInfo codeInfo = (CodeInfo)GetNewBusinessObject();
			codeInfo.ZO_Code = "A";
			codeInfo.ZO_Data = "B";

			AssertEquals("ToString()", "A" + BaseAddInfo.CodeValueSeparator + "B", codeInfo.ToString());
		}

		public void TestLoadFromString()
		{
			CodeInfo codeInfo = (CodeInfo)GetNewBusinessObject();
			codeInfo.LoadFromString("A=B");
			AssertEquals("Code", "A", codeInfo.ZO_Code);
			AssertEquals("Data", "B", codeInfo.ZO_Data);
		}

		public void TestRemoveReservedCharactersFromDataAndCode()
		{
			CodeInfo codeInfo = (CodeInfo)GetNewBusinessObject();
			codeInfo.ZO_Code = BaseAddInfo.CodeValueSeparator.ToString();
			AssertEquals("CodeValueSeparator is reserved and should be stripped out", ZString.Empty, codeInfo.ZO_Code);

			codeInfo.ZO_Data = BaseAddInfo.CodeInfoSeparator.ToString();
			AssertEquals("CodeInfoSeparator is reserved and should be stripped out", ZString.Empty, codeInfo.ZO_Data);
		}

		public virtual void TestValidateCode()
		{
			CodeInfoCollection collection = GetCodeInfoCollection();
			CodeInfo codeInfo = collection.AddNew();
			codeInfo.ZO_Code = "~";
			AssertHasMessageError(codeInfo.ZO_CodeInfo, ListValidation.InvalidCodeMessageError);

			if (!codeInfo.IsDuplicateCodeAllowed)
			{
				CodeInfo codeInfo2 = collection.AddNew();
				codeInfo2.ZO_Code = "~";
				AssertHasError(codeInfo2.ZO_CodeInfo, "~ already exists.");
			}
		}

		public void TestValidateData()
		{
			CodeInfo codeInfo = (CodeInfo)GetNewBusinessObject();
			codeInfo.ZO_Data = "D";
			AssertHasError(codeInfo.ZO_DataInfo, string.Format("You have entered {0} without {1}.", codeInfo.HumanFriendlyDataColumnName, codeInfo.HumanFriendlyCodeColumnName));

			codeInfo.ZO_Code = CodeRequiringDataIfExists;

			if (codeInfo.DataRequirement == CodeInfo.DataRequirements.DataRequiredForCode)
			{
				codeInfo.ZO_Data = "";
				AssertHasMessageError(codeInfo.ZO_DataInfo, string.Format("Please enter {0} corresponding to the {1}", codeInfo.HumanFriendlyDataColumnName, codeInfo.HumanFriendlyCodeColumnName));

				codeInfo.ZO_Data = "D";
				AssertNoMessageError(codeInfo.ZO_DataInfo, string.Format("Please enter {0} corresponding to the {1}", codeInfo.HumanFriendlyDataColumnName, codeInfo.HumanFriendlyCodeColumnName));
			}
			else if (codeInfo.DataRequirement == CodeInfo.DataRequirements.DataNotRequiredForCode)
			{
				codeInfo.ZO_Data = "D";
				AssertHasMessageError(codeInfo.ZO_DataInfo, string.Format("{0} does not require {1} to be entered", codeInfo.ZO_Code, codeInfo.HumanFriendlyDataColumnName));

				codeInfo.ZO_Data = "";
				AssertNoMessageError(codeInfo.ZO_DataInfo, string.Format("{0} does not require {1} to be entered", codeInfo.ZO_Code, codeInfo.HumanFriendlyDataColumnName));
			}
		}

		public virtual void TestDescription()
		{
			CodeInfo codeInfo = (CodeInfo)GetNewBusinessObject();
			ICodeDescription codeDesc = codeInfo.ZO_CodeList[0];
			codeInfo.ZO_Code = codeDesc.Code;
			AssertEquals("Description", codeDesc.Description, codeInfo.ZO_Description);
		}

		protected abstract CodeInfoCollection GetCodeInfoCollection();
		protected abstract string CodeRequiringDataIfExists { get; }

		public void TestSavingFactoryResetsHasChanges()
		{
			CodeInfo codeInfoBizObj = (CodeInfo)GetNewBusinessObject();
			AssertEquals("Precondition : HasChanges", false, codeInfoBizObj.HasChanges);
			codeInfoBizObj.ZO_Code = "123";
			AssertEquals("Precondition : HasChanges", true, codeInfoBizObj.HasChanges);
			Factory.Save();
			AssertEquals("HasChanges", false, codeInfoBizObj.HasChanges);
		}
		public void TestCloneCodeInfo()
		{
			CodeInfo codeInfoBizObj = (CodeInfo)GetNewBusinessObject();
			codeInfoBizObj.ZO_Code = "CC";
			codeInfoBizObj.ZO_Data = "Data";
			CodeInfo clonedCodeInfo = (CodeInfo)codeInfoBizObj.Clone();
			AssertNotNull("Cloned object is not null", clonedCodeInfo);
			AssertEquals("Same codes", codeInfoBizObj.ZO_Code, clonedCodeInfo.ZO_Code);
			AssertEquals("Same data", codeInfoBizObj.ZO_Data, clonedCodeInfo.ZO_Data);
			AssertEquals("Same descriptions", codeInfoBizObj.ZO_Description, clonedCodeInfo.ZO_Description);
		}
	}
}
