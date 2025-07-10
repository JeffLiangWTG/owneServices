//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGLHeaderSubAccountValidation
//
//    This class should be used for overriding validation in AutoAccGLHeaderSubAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderSubAccountValidation : AutoAccGLHeaderSubAccountValidation
	{
		public AccGLHeaderSubAccountValidation(AutoAccGLHeaderSubAccount parent) : base(parent)
		{
		}

		protected new AccGLHeaderSubAccount Parent
		{
			get { return base.Parent as AccGLHeaderSubAccount; }
		}

		public void ValidateASA_SubClassDisplayName()
		{
			ValidateCalculatedProperty(Parent.ASA_SubClassDisplayNameInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateASA_SubClassDisplayName();
		}

		protected void CheckASA_SubClassDisplayName()
		{
			var glHeader = Parent.GLHeader;

			MandatoryValidation.CheckEntered(Parent.ASA_SubClassDisplayNameInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ASA_SubClassDisplayNameInfo, glHeader.AG_SubAccountTypeList);

			if (glHeader.SubAccountTypes.Count > 0 && !Parent.ASA_SubClassDisplayName.IsEmpty)
			{
				if (glHeader.SubAccountTypes.Cast<AccGLHeaderSubAccount>().Any(x => x.PK != Parent.PK && x.ASA_SubClassDisplayName == Parent.ASA_SubClassDisplayName))
				{
					Parent.ASA_SubClassDisplayNameInfo.AddError(Res.GetString("4e6a2350-7a0e-409b-b677-8800b89b3162", "This Sub Account Type is already defined"));
				}
			}

			if (!AccGLHeader.Constants.AccountTypeListApplicableForSubAccount.ToList().Contains(glHeader.AG_AccountType))
			{
				Parent.ASA_SubClassDisplayNameInfo.AddError(Res.GetString("7938fcf5-de1d-46cc-9f4a-c2dd42b99a18", "Sub Account is only applicable for '{0}' type GL Account.", string.Join((NoResString)"' and '", AccGLHeader.Constants.AccountTypeListApplicableForSubAccount)));
			}

			if (!Parent.ASA_SubClassDisplayNameInfo.HasErrors() && Parent.IsInDatabase &&
				!((ZString)Parent.ASA_SubClassInfo.OriginalValue).IsEmpty && Parent.ASA_SubClassInfo.HasChanges)
			{
				var subClassOriginalValue = (ZString)Parent.ASA_SubClassInfo.OriginalValue;
				var errormessage = IsUsedByTransactionSubAccountType(subClassOriginalValue);

				if (!errormessage.IsNullOrEmpty())
				{
					Parent.ASA_SubClassDisplayNameInfo.AddError((Res.GetString("d1c36431-067a-4ec2-93e8-1fa13a22d7cf", "Sub account type can not be changed from '{0}' to '{1}' because this account is currently in use. It is used by {2}."
						, SubAccountCodeConverter.ConvertSubAccountDBParentTableCodeToSubClassCode(subClassOriginalValue), Parent.ASA_SubClassDisplayNameInfo.Value, errormessage)));
				}
			}
		}

		public string IsUsedByTransactionSubAccountType(ZString subAccountType)
		{
			StringBuilder result = new StringBuilder();
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Parent.Factory);

			string selectSQLForLineSubAccount = @"
SELECT GC_Code, COUNT(*) AS Count, MIN(AL_PostDate) AS Min, MAX(AL_PostDate) AS Max, 'transaction lines' AS Type  
FROM dbo.AccTransactionLineSubAccount 
INNER JOIN dbo.AccTransactionLines ON AL_PK = AL1_AL
INNER JOIN dbo.AccTransactionHeader ON AH_PK = AL_AH
INNER JOIN dbo.GlbCompany ON GC_PK = AH_GC 
WHERE AL_AG = @AGPK and AL1_SubClassParentTableCode = @AL1_SubClassParentTableCode
GROUP BY GC_Code

UNION ALL

SELECT GC_Code, COUNT(*) AS Count, MIN(AH_PostDate) AS Min, MAX(AH_PostDate) AS Max, 'transaction headers' AS Type  
FROM dbo.AccTransactionHeaderSubAccount 
INNER JOIN dbo.AccTransactionHeader ON AH_PK = AHS_AH
INNER JOIN dbo.GlbCompany ON GC_PK = AH_GC 
WHERE AH_AG = @AGPK and AHS_SubClassParentTableCode = @AHS_SubClassParentTableCode
GROUP BY GC_Code";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@AGPK", Parent.GLHeader.PK, AccTransactionHeaderSchema.AH_AG);
			sqlParams.Add("@AL1_SubClassParentTableCode", subAccountType, AccTransactionLineSubAccountSchema.AL1_SubClassParentTableCode);
			sqlParams.Add("@AHS_SubClassParentTableCode", subAccountType, AccTransactionHeaderSubAccountSchema.AHS_SubClassParentTableCode);
			collection.Load(selectSQLForLineSubAccount, sqlParams);
			foreach (DynamicBusinessObject row in collection)
			{
				result.Append(Res.GetString("059633b4-ae49-4f43-a8c3-70d17eddaf7a", "{0} in company {1} posted between {2} and {3}",
					(ZString)row["Type"],
					(ZString)row[GlbCompanySchema.Constants.GC_Code],
					(ZDateTime)row["Min"],
					(ZDateTime)row["Max"]) + ", ");
			}

			return result.ToString().TrimEnd(',', ' ');
		}
	}
}
