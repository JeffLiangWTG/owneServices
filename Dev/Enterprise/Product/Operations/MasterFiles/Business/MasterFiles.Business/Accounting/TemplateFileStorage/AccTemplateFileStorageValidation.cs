//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccTemplateFileStorageValidation
//
//    This class should be used for overriding validation in AutoAccTemplateFileStorageValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccTemplateFileStorageValidation : AutoAccTemplateFileStorageValidation
	{
		public AccTemplateFileStorageValidation(AutoAccTemplateFileStorage parent) : base(parent)
		{
		}

		protected override void CheckTFS_Code()
		{
			base.CheckTFS_Code();
			MandatoryValidation.CheckEntered(Parent.TFS_CodeInfo);
			CheckItIsNotDuplicate();
		}

		protected override void CheckTFS_FileData()
		{
			base.CheckTFS_FileData();

			if (!CheckIfFileDataOrGUIDEntered())
			{
				Parent.TFS_FileDataInfo.AddError(FileDataOrGUIDMissingErrorString);
			}
		}

		protected override void CheckTFS_Description()
		{
			base.CheckTFS_Description();
			MandatoryValidation.CheckEntered(Parent.TFS_DescriptionInfo);
		}

		protected override void CheckTFS_IsActive()
		{
			base.CheckTFS_IsActive();
			if (Parent.TFS_IsActiveInfo.HasChanges)
			{
				var errorMessage = CheckTemplateNotBeingUsedByAnyOrganizationOrCompany();
				if (!string.IsNullOrEmpty(errorMessage))
				{
					Parent.TFS_IsActiveInfo.AddError(errorMessage);
				}
			}
		}

		protected override void CheckTFS_ExternalReference()
		{
			base.CheckTFS_ExternalReference();
			if (!CheckIfFileDataOrGUIDEntered())
			{
				Parent.TFS_ExternalReferenceInfo.AddError(FileDataOrGUIDMissingErrorString);
			}
		}

		protected override void CheckTFS_ExternalReferenceIsValidZGuid()
		{
			TypeValidation.CheckValidGuid(Parent.TFS_ExternalReferenceInfo, InvalidGuidValueErrorString);
		}

		string CheckTemplateNotBeingUsedByAnyOrganizationOrCompany()
		{
			var errorString = "";
			var tablePrefixList = new List<string>() { OrgHeaderSchema.Constants.Prefix, "" };

			var templateQuery = @"SELECT ETF_PK, ETF_ConfigType, ETF_GC, ETF_Ledger, ETF_ParentTableCode, ETF_ParentID, ETF_JobType, ETF_ServiceDirection, ETF_TransportMode, ETF_TemplateCode, OH_Code, OH_FullName
						FROM dbo.AccEInvoicingTemplateFileView
						LEFT JOIN dbo.OrgHeader ON OH_PK = ETF_ParentId
WHERE ETF_GC = @parentTFS_GC
AND ETF_ParentTableCode IN (SELECT [value] FROM @orgHeaderPrefixes)
AND ETF_TemplateCOde = @parentTFS_Code";

			var templateParameters = new[]
			{
				ZSqlParameter.New("@parentTFS_GC", Parent.TFS_GC, AccEInvoicingTemplateFileViewSchema.ETF_GC),
				ZSqlParameter.New("@orgHeaderPrefixes", tablePrefixList, AccEInvoicingTemplateFileViewSchema.ETF_ParentTableCode, true),
				ZSqlParameter.New("@parentTFS_Code", Parent.TFS_Code, AccEInvoicingTemplateFileViewSchema.ETF_TemplateCode)
			};

			var templateArray = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			templateArray.Load(templateQuery, templateParameters);

			if (templateArray.Any())
			{
				var orgNamesWithActiveTemplate = new List<Tuple<ZString, ZString>>();
				bool isCompanyTemplateStillInUse = false;

				foreach (DynamicBusinessObject usedTemplate in templateArray)
				{
					if ((ZString)usedTemplate[AccEInvoicingTemplateFileViewSchema.ETF_ParentTableCode.Name] == "")
					{
						isCompanyTemplateStillInUse = true;
					}
					else if ((ZString)usedTemplate[AccEInvoicingTemplateFileViewSchema.ETF_ParentTableCode.Name] == OrgHeaderSchema.Constants.Prefix)
					{
						var orgInfo = Tuple.Create((ZString)usedTemplate[OrgHeaderSchema.OH_Code.Name], (ZString)usedTemplate[OrgHeaderSchema.OH_FullName.Name]);
						orgNamesWithActiveTemplate.Add(orgInfo);
					}
				}
				errorString = IsStillBeingUsedByOrganizationErrorString(orgNamesWithActiveTemplate, isCompanyTemplateStillInUse);
			}

			return errorString;
		}

		string IsStillBeingUsedByOrganizationErrorString(List<Tuple<ZString, ZString>> organisationInfoList, bool companyTemplateStillInUse)
		{
			var sbTemplateUsageErrorMessageCore = new ZStringBuilder(IsStillUsedByCompanyOrOrganizationErrorMessageCore);

			if (companyTemplateStillInUse)
			{
				sbTemplateUsageErrorMessageCore.Append(Parent.Company.GC_Code + " - " + Parent.Company.GC_Name);
			}

			if (organisationInfoList.Any())
			{
				organisationInfoList.ForEach(organisation => sbTemplateUsageErrorMessageCore.Append(organisation.Item1 + " - " + organisation.Item2));
			}

			return (sbTemplateUsageErrorMessageCore.ToStringWithNewLineBetweenAppends());
		}

		void CheckItIsNotDuplicate()
		{
			Parent.ClearRowNotificationsContaining(IsDuplicateErrorStringCore);

			var parentCollection = (AccTemplateFileStorageCollection)((IBusinessObjectInternals)Parent).ParentCollections.FirstOrDefault(pc => pc is AccTemplateFileStorageCollection);

			if (parentCollection != null && parentCollection.Cast<AccTemplateFileStorage>().Any(c => c != Parent && ((AccTemplateFileStorage)Parent).IsDuplicateOf(c)))
			{
				Parent.AddRowError(string.Format(IsDuplicateErrorString, Parent.TFS_Code));
			}
		}

		protected override void CheckTFS_FileDataIsValidZBlobSize()
		{
			byte[] data = (byte[])((IZTypeInternals)Parent.TFS_FileDataInfo.Value).GetValueForLogicalDataLayer(false);
			int numberOfBytes = data.Length;

			if (numberOfBytes > TypeValidation.ZBlobMaxBytesBeforeError)
			{
				Parent.TFS_FileDataInfo.AddError(Res.GetString("3B48A7C0-5434-413A-826D-A24F9AB93A72", "This note is too large to store in the database. Reduce its size by removing any large images or files.\r\n\r\n   - Current Size:  {0} KB\r\n   - Maximum Size:  {1} KB",
					(numberOfBytes / 1024).ToString(),
					(TypeValidation.ZBlobMaxBytesBeforeError / 1024).ToString()) + "\r\n\r\n");
			}
		}

		bool CheckIfFileDataOrGUIDEntered() => !(Parent.TFS_ExternalReference.IsEmpty && Parent.TFS_FileName.IsEmpty);

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckItIsNotDuplicate();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		internal static string IsDuplicateErrorString => Res.GetString("AccTemplateFileStorage|DuplicateError", IsDuplicateErrorStringCore + " The duplicate value(s) are: ({0}).");
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Will be translated above line")]
		const string IsDuplicateErrorStringCore = "The value of Code must be unique on Company.";
		internal static string IsStillUsedByCompanyOrOrganizationErrorMessageCore => Res.GetString("AccTemplateFileStorage|TemplateBeingUsedError", "The XSLT template is still being used by at least one organization or company: ");
		internal static string InvalidGuidValueErrorString => Res.GetString("AccTemplateFileStorage|InvalidGuidError", "GUID value");
		internal static string FileDataOrGUIDMissingErrorString => Res.GetString("AccTemplateFileStorage|FileDataOrGuidMissingError", "Please enter either a GUID or add an XSLT file");
	}
}
