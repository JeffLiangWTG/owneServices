using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class PatternMatchingDomainRegenerator<TBizo> : PatternMatchingRegenerator<TBizo> where TBizo : BusinessObject, IDeduplicatable, IPatternMatchingRegenerationEntities<TBizo>
	{
		#region Calculate Fields

		OrgAddress[] allOrgAddressArray;
		OrgContact[] allOrgContactArray;
		OrgWebURL[] allOrgWebURLArray;
		PatternMatchingDomain[] patternMatchingDomainArray;

		OrgAddress[] addressNeedAdd;
		OrgContact[] contactNeedAdd;
		OrgWebURL[] webURLNeedAdd;
		PatternMatchingDomain[] needDelete;
		PatternMatchingDomain[] needUpdate;

		#endregion

		public PatternMatchingDomainRegenerator(PatternMatchingRecalculator<TBizo> recalculator)
			: base(recalculator)
		{
		}

		protected override List<string> TablesPrefixList
		{
			get
			{
				return new List<string>()
				{
					OrgAddressSchema.Constants.Prefix,
					OrgContactSchema.Constants.Prefix,
					OrgWebURLSchema.Constants.Prefix,
				};
			}
		}

		protected override int GetActualAddCount()
		{
			return addressNeedAdd.Length + contactNeedAdd.Length + webURLNeedAdd.Length;
		}

		public override int InitializeDataCount(TBizo bizo, BusinessObjectFactory factory)
		{
			if (bizo is OrgHeader header)
			{
				var query = new ZQuery(OrgAddressSchema.OA_OH, header.PK);
				allOrgAddressArray = factory.Load<OrgAddress>(query);

				query = new ZQuery(OrgContactSchema.OC_OH, header.PK);
				allOrgContactArray = factory.Load<OrgContact>(query);

				query = new ZQuery(OrgWebURLSchema.PU_OH, header.PK);
				allOrgWebURLArray = factory.Load<OrgWebURL>(query);

				query = new ZQuery(PatternMatchingDomainSchema.PMD_OH, header.PK);
				query.AddToFilter(PatternMatchingDomainSchema.PMD_ParentTableCode, TablesPrefixList);
				patternMatchingDomainArray = factory.Load<PatternMatchingDomain>(query);

				var matchDomainparentIds = patternMatchingDomainArray.Select(a => a.ParentId).ToList();
				addressNeedAdd = allOrgAddressArray.Where(s => !s.OA_Email.IsEmpty && !matchDomainparentIds.Contains(s.PK)).ToArray();
				contactNeedAdd = allOrgContactArray.Where(s => !s.OC_Email.IsEmpty && !matchDomainparentIds.Contains(s.PK)).ToArray();
				webURLNeedAdd = allOrgWebURLArray.Where(s => !s.PU_URL.IsEmpty && !matchDomainparentIds.Contains(s.PK)).ToArray();

				var orgAddressIds = allOrgAddressArray.Select(a => a.PK).ToList();
				var orgContactIds = allOrgContactArray.Select(a => a.PK).ToList();
				var orgWebURLIds = allOrgWebURLArray.Select(a => a.PK).ToList();

				needDelete = patternMatchingDomainArray.Where(s => !orgAddressIds.Contains(s.ParentId) && !orgContactIds.Contains(s.ParentId) && !orgWebURLIds.Contains(s.ParentId)).ToArray();
				needUpdate = patternMatchingDomainArray.Where(s => orgAddressIds.Contains(s.ParentId) || orgContactIds.Contains(s.ParentId) || orgWebURLIds.Contains(s.ParentId)).ToArray();

				return GetActualAddCount() + needDelete.Length + needUpdate.Length;
			}
			else if (bizo is GlbPerson)
			{
				needDelete = Array.Empty<PatternMatchingDomain>();
				return 0;
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Bizo [{0}] is not supported", bizo.GetType().FullName));
			}
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

		protected override int Add(TBizo bizo, BusinessObjectFactory factory)
		{
			if (bizo is OrgHeader header)
			{
				addressNeedAdd.ForEach((item) =>
				{
					if (!item.OA_Email.IsEmpty)
					{
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(item.OA_Email);

						if (!TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							var matchingDomain = factory.New<PatternMatchingDomain>();

							matchingDomain.PMD_OH = item.OA_OH;
							matchingDomain.PMD_ParentTableCode = item.TablePrefix;
							matchingDomain.PMD_ParentId = item.PK;
							matchingDomain.PMD_IsActive = ZBool.True;
							matchingDomain.PMD_RN_NKCountryCode = item.OA_RN_NKCountryCode;
							matchingDomain.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(item.OA_Email),
								Hash = matchingDomain.HashedValue,
								OriginalValue = item.OA_Email,
								PK = item.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = matchingDomain.TablePrefix
							});
						}

						ReportProgress();
					}
				});

				contactNeedAdd.ForEach((item) =>
				{
					if (!item.OC_Email.IsEmpty)
					{
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(item.OC_Email);

						if (!TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							var matchingDomain = factory.New<PatternMatchingDomain>();

							matchingDomain.PMD_OH = item.OC_OH;
							matchingDomain.PMD_ParentTableCode = item.TablePrefix;
							matchingDomain.PMD_ParentId = item.PK;
							matchingDomain.PMD_IsActive = ZBool.True;
							matchingDomain.PMD_RN_NKCountryCode = header.CountryCode;
							matchingDomain.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(item.OC_Email),
								Hash = matchingDomain.HashedValue,
								OriginalValue = item.OC_Email,
								PK = item.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = matchingDomain.TablePrefix
							});
						}

						ReportProgress();
					}
				});

				webURLNeedAdd.ForEach((item) =>
				{
					if (!item.PU_URL.IsEmpty)
					{
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(item.PU_URL);

						if (!TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							var matchingDomain = factory.New<PatternMatchingDomain>();

							matchingDomain.PMD_OH = item.PU_OH;
							matchingDomain.PMD_ParentTableCode = item.TablePrefix;
							matchingDomain.PMD_ParentId = item.PK;
							matchingDomain.PMD_IsActive = ZBool.True;
							matchingDomain.PMD_RN_NKCountryCode = header.CountryCode;
							matchingDomain.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(item.PU_URL),
								Hash = matchingDomain.HashedValue,
								OriginalValue = item.PU_URL,
								PK = item.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = matchingDomain.TablePrefix
							});
						}

						ReportProgress();
					}
				});

				return GetActualAddCount();
			}
			else if (bizo is GlbPerson)
			{
				return 0;
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Bizo [{0}] is not supported", bizo.GetType().FullName));
			}
		}

		protected override int Update(TBizo bizo, BusinessObjectFactory factory)
		{
			if (bizo is OrgHeader header)
			{
				needUpdate.ForEach((item) =>
				{
					if (item.ParentTableCode.Equals(OrgAddressSchema.Constants.Prefix))
					{
						var orgAddress = allOrgAddressArray.Where(oa => oa.PK.Equals(item.ParentId)).FirstOrDefault();
						var needHashValue = orgAddress.OA_Email;
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(orgAddress.OA_Email);

						item.PMD_IsActive = orgAddress.OA_IsActive;

						if (!needHashValue.IsEmpty && !TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							item.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);
							item.PMD_RN_NKCountryCode = orgAddress.OA_RN_NKCountryCode;

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(orgAddress.OA_Email),
								Hash = item.HashedValue,
								OriginalValue = orgAddress.OA_Email,
								PK = orgAddress.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = item.TablePrefix
							});
						}
						else
						{
							item.Delete();
						}
					}
					else if (item.ParentTableCode.Equals(OrgContactSchema.Constants.Prefix))
					{
						var orgContact = allOrgContactArray.Where(oa => oa.PK.Equals(item.ParentId)).FirstOrDefault();
						var needHashValue = orgContact.OC_Email;
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(orgContact.OC_Email);

						item.PMD_IsActive = orgContact.OC_IsActive;

						if (!needHashValue.IsEmpty && !TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							item.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);
							item.PMD_RN_NKCountryCode = header.CountryCode;

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(orgContact.OC_Email),
								Hash = item.HashedValue,
								OriginalValue = orgContact.OC_Email,
								PK = orgContact.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = item.TablePrefix
							});
						}
						else
						{
							item.Delete();
						}
					}
					else if (item.ParentTableCode.Equals(OrgWebURLSchema.Constants.Prefix))
					{
						var orgWebURL = allOrgWebURLArray.Where(oa => oa.PK.Equals(item.ParentId)).FirstOrDefault();
						var needHashValue = orgWebURL.PU_URL;
						var extractedDomain = TextStandardizerHelper.ExtractEmailDomain(orgWebURL.PU_URL);

						item.PMD_IsActive = ZBool.True;

						if (!needHashValue.IsEmpty && !TextStandardizerHelper.IsGenericDomain(extractedDomain))
						{
							item.HashedValue = TextStandardizerHelper.ComputeStringHashFast(extractedDomain);
							item.PMD_RN_NKCountryCode = header.CountryCode;

							patternMatchingRecalculator.DebuggerMessages.Add(new DeduplicationDebuggerMaster
							{
								ColumnName = nameof(orgWebURL.PU_URL),
								Hash = item.HashedValue,
								OriginalValue = orgWebURL.PU_URL,
								PK = orgWebURL.PK.ToGuid(),
								StandardizedValue = extractedDomain,
								TableName = item.TablePrefix
							});
						}
						else
						{
							item.Delete();
						}
					}

					ReportProgress();
				});
				return needUpdate.Length;
			}
			else if (bizo is GlbPerson)
			{
				return 0;
			}
			else
			{
				throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Bizo [{0}] is not supported", bizo.GetType().FullName));
			}
		}
	}
}
