using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingEmailRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		protected PatternMatchingEmail[] patternMatchingEmailArray;

		protected PatternMatchingEmail[] needDelete;
		protected PatternMatchingEmail[] needUpdate;

		#endregion

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgAddressSchema.Constants.Prefix,
					OrgContactSchema.Constants.Prefix,
				};
			}
		}

		protected PatternMatchingEmailRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
			: base(recalculator)
		{
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCountCore(bizo, factory);
		}

		protected void AddCore(BusinessObjectFactory factory, ZString email, ZGuid superParentPk, ZString tableCode, ZGuid parentPk, ZString countryCode, ZString debuggerName)
		{
			if (!TextStandardizerHelper.IsGenericLocalPart(email))
			{
				var matchingEmail = factory.New<PatternMatchingEmail>();

				SetUltimateParentPK(matchingEmail, superParentPk, parentPk);
				matchingEmail.PME_ParentTableCode = tableCode;
				matchingEmail.PME_ParentId = parentPk;
				matchingEmail.PME_RN_NKCountryCode = countryCode;
				matchingEmail.PME_IsActive = ZBool.True;
				matchingEmail.HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = matchingEmail.HashedValue,
					OriginalValue = email,
					PK = parentPk.ToGuid(),
					StandardizedValue = email,
					TableName = tableCode
				});
			}

			ReportProgress();
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			return AddCore(bizo, factory);
		}

		protected override int Delete()
		{
			needDelete.ForEach((item) =>
			{
				item.Delete();

				ReportProgress();
			});

			return needDelete.Length;
		}

		protected void UpdateCore(PatternMatchingEmail patternEmail, ZString email, ZString countryCode, ZGuid parentPk, ZString debuggerName)
		{
			if (!email.IsEmpty && !TextStandardizerHelper.IsGenericLocalPart(email))
			{
				patternEmail.PME_RN_NKCountryCode = countryCode;
				patternEmail.HashedValue = TextStandardizerHelper.ComputeStringHashFast(email);

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = debuggerName,
					Hash = patternEmail.HashedValue,
					OriginalValue = email,
					PK = parentPk.ToGuid(),
					StandardizedValue = email,
					TableName = patternEmail.TablePrefix
				});
			}
			else
			{
				patternEmail.Delete();
			}
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			needUpdate.ForEach((item) =>
			{
				UpdateCore(bizo, item, factory);

				ReportProgress();
			});

			return needUpdate.Length;
		}

		protected abstract int InitializeDataCountCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void SetUltimateParentPK(PatternMatchingEmail patternEmail, ZGuid pk, ZGuid parentPK);

		protected abstract int AddCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void UpdateCore(TBizo bizo, PatternMatchingEmail patternEmail, BusinessObjectFactory factory);
	}
}

