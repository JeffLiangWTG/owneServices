using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business
{
	public abstract class PatternMatchingPhoneRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		protected PatternMatchingPhone[] allPatternMatchingPhoneArray;
		protected PatternMatchingPhone[] needDelete;

		#endregion

		protected PatternMatchingPhoneRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
			: base(recalculator)
		{
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			return InitializeDataCountCore(bizo, factory);
		}

		public ZString GetObjectPropertyValue(BusinessObject bizo, string propertyname)
		{
			PropertyInfo property = bizo.GetType().GetProperty(propertyname);
			object objValue = property?.GetValue(bizo, null);

			if (objValue == null || TextStandardizerHelper.IsPlaceholderPhone(objValue.ToString()))
			{
				return ZString.Empty;
			}

			return objValue.ToString();
		}

		protected int AddCore(List<string> propList, BusinessObject childBizo, BusinessObjectFactory factory, ZGuid superParentPk, ZString countryCode, ZBool isActive)
		{
			var counter = 0;

			foreach (var prop in propList)
			{
				var propValue = GetObjectPropertyValue(childBizo, prop);

				if (!propValue.IsEmpty && !TextStandardizerHelper.IsPlaceholderPhone(propValue))
				{
					var matchingPhone = factory.New<PatternMatchingPhone>();

					SetUltimateParentPK(matchingPhone, superParentPk, childBizo.PK);
					matchingPhone.PMP_ParentTableCode = childBizo.TablePrefix;
					matchingPhone.PMP_ParentId = childBizo.PK;
					matchingPhone.PMP_IsActive = isActive;
					matchingPhone.PMP_RN_NKCountryCode = countryCode;
					matchingPhone.HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePhone(propValue));

					patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
					{
						ColumnName = prop,
						Hash = matchingPhone.HashedValue,
						OriginalValue = propValue,
						PK = childBizo.PK.ToGuid(),
						StandardizedValue = propValue,
						TableName = matchingPhone.TablePrefix
					});

					counter++;
					ReportProgress();
				}
			}

			return counter;
		}

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			return AddCore(bizo, factory);
		}

		protected override int Delete()
		{
			needDelete.ForEach((x) =>
			{
				x.Delete();

				ReportProgress();
			});

			return needDelete.Length;
		}

		protected void UpdateCore(List<string> propList, BusinessObject childBizo, BusinessObjectFactory factory, ZGuid superParentPk, ZString countryCode, ZBool isActive)
		{
			List<PatternMatchingPhone> patternMatchingPhones = allPatternMatchingPhoneArray.Where(a => !a.IsDeleted && a.ParentId.Equals(childBizo.PK)).ToList();
			List<string> notNullPropList = new List<string>();

			foreach (var prop in propList)
			{
				if (!GetObjectPropertyValue(childBizo, prop).IsEmpty)
				{
					notNullPropList.Add(prop);
				}
			}

			while (patternMatchingPhones.Count > 0 && patternMatchingPhones.Count > notNullPropList.Count)
			{
				var patternMatchingPhone = patternMatchingPhones[0];

				patternMatchingPhones.Remove(patternMatchingPhone);
				patternMatchingPhone.Delete();
			}

			while (patternMatchingPhones.Count > 0 && patternMatchingPhones.Count < notNullPropList.Count)
			{
				var matchingPhone = factory.New<PatternMatchingPhone>();

				SetUltimateParentPK(matchingPhone, superParentPk, childBizo.PK);
				matchingPhone.PMP_ParentTableCode = childBizo.TablePrefix;
				matchingPhone.PMP_ParentId = childBizo.PK;
				matchingPhone.PMP_RN_NKCountryCode = countryCode;
				matchingPhone.PMP_IsActive = isActive;
				patternMatchingPhones.Add(matchingPhone);
			}

			for (int i = 0; i < notNullPropList.Count; i++)
			{
				var prop = notNullPropList[i];
				patternMatchingPhones[i].PMP_RN_NKCountryCode = countryCode;
				var needHashValue = GetObjectPropertyValue(childBizo, prop);
				patternMatchingPhones[i].HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizePhone(needHashValue));

				patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
				{
					ColumnName = prop,
					Hash = patternMatchingPhones[i].HashedValue,
					OriginalValue = needHashValue,
					PK = childBizo.PK.ToGuid(),
					StandardizedValue = needHashValue,
					TableName = patternMatchingPhones[i].TablePrefix
				});
			}
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			return UpdateCore(bizo, factory);
		}

		protected abstract int InitializeDataCountCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int AddCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract int UpdateCore(TBizo bizo, BusinessObjectFactory factory);

		protected abstract void SetUltimateParentPK(PatternMatchingPhone patternPhone, ZGuid pk, ZGuid parentPK);
	}
}

