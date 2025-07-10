using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ManyToManyOrgMerger
	{
		public ManyToManyOrgMerger(FilteredBusinessObjectReader reader)
		{
			Argument.NotNull(reader, "FilteredBusinessObjectReader");
			Reader = reader;
		}

		readonly FilteredBusinessObjectReader Reader;

		public event ManyToManyEventHandler BatchCompleted;

		public void Cancel()
		{
			Cancelled = true;
		}

		public bool Cancelled { get; private set; }
		public int OrganisationsProcessed { get; private set; }
		public int OrganisationsDeleted { get; private set; }
		public int ApproximateCount { get; private set; }
		public ZDateTime StartDateTime { get; private set; }
		public ZDateTime EndDateTime { get; private set; }

		readonly List<string> disallowedMergeMessages = new List<string>();
		public ReadOnlyCollection<string> DisallowedMergeMessages => new ReadOnlyCollection<string>(disallowedMergeMessages);

		public List<string> Merge()
		{
			StartDateTime = ZDateTime.Now;
			var errors = new List<string>();
			var alreadyMerged = new List<ZGuid>();
			try
			{
				int i = 0;
				int batch = 0;
				int batchsize = 10;
				ApproximateCount = Reader.ApproximateCount;
				if (batchsize * 4 > ApproximateCount)
				{
					if (ApproximateCount / 4 > 1)
					{
						batchsize = ApproximateCount / 4;
					}
					else
					{
						batchsize = 1;
					}
				}
				int totalNumberOfBatches = Reader.ApproximateCount / batchsize;

				var orgFilter = new ZDBOnlyQuery(typeof(OrgPatternMatch));
				var subOrgFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgPatternMatchSchema.OS_OH);
				subOrgFilter.AddToFilter(Reader.ObjectFilter);
				orgFilter.AddSubQuery(subOrgFilter, JoinCondition.And);
				var orgList = Reader.OfType<OrgHeader>().ToList();
				foreach (var org in orgList)
				{
					if (Cancelled)
					{
						break;
					}

					if (!alreadyMerged.Contains(org.PK))
					{
						OrganisationsProcessed++;
						org.PatternMatchesForThisOrg.MatchThresholdOverride = OrgMatchThresholds.Codes.High;

						org.SimilarOrgFinder.OverrideMaximumResultsToInfinity = true;
						org.SimilarOrgMatches.MaximumResultsToShow = 500;
						org.SimilarOrgFinder.FindSimilarOrganisations(orgFilter, false);

						var mergedOrgHeadersToBeSaved = GetOrderHeadersToBeSaved(org, alreadyMerged);

						foreach (var merge in mergedOrgHeadersToBeSaved)
						{
							try
							{
								foreach (MergeOrgAddress tmp in merge.OldOrgAddressesCollection)
								{
									foreach (MergeOrgAddress adr in merge.CurrentOrgAddressCollection)
									{
										if (tmp.OldAddressPK == adr.OldAddressPK)
										{
											tmp.Action = adr.Action;
											if (adr.Action == MergeOrgAddress.ActionMerge)
											{
												tmp.NewObjectPK = adr.NewAddressPK;
											}
											break;
										}
									}
								}
								foreach (MergeOrgContact cnt in merge.CurrentOrgContactCollection)
								{
									foreach (MergeOrgContact tmp in merge.OldOrgContactCollection)
									{
										if (tmp.OldContactPK == cnt.OldContactPK)
										{
											tmp.Action = cnt.Action;
											if (cnt.Action == MergeOrgContact.ActionMerge)
											{
												tmp.NewObjectPK = cnt.NewContactPK;
											}
										}
									}
								}
								BusinessObjectFactory.SaveTogether(merge.SaveFactories);
								alreadyMerged.Add(merge.OldOrganisation.PK);
								int count = org.SimilarOrgMatches.Count;
								for (int m = 0; m < count; m++)
								{
									if (org.SimilarOrgMatches.Count <= m)
									{
										break;
									}

									if (org.SimilarOrgMatches[m].OS_OH == merge.OldOrganisation.PK)
									{
										org.SimilarOrgMatches.Remove(org.SimilarOrgMatches[m]);
									}
								}
								OrganisationsProcessed++;
								OrganisationsDeleted++;
								i++;
								if (i >= batchsize)
								{
									i = 0;
									if (BatchCompleted != null)
									{
										BatchCompleted(this, new ManyToManyBatchPrcessedEvent(++batch * 100 / totalNumberOfBatches));
									}
								}
							}
							catch (Exception ex)
							{
								if (ex.IsCriticalException())
								{
									throw;
								}
								errors.Add(!string.IsNullOrEmpty(merge.DeleteError) ? merge.DeleteError : ex.Message);
							}
						}
					}
				}
				if (BatchCompleted != null)
				{
					BatchCompleted(this, new ManyToManyBatchPrcessedEvent(100));
				}
			}
			finally
			{
				EndDateTime = ZDateTime.Now;
			}
			return errors;
		}

		List<MergeOrgHeader> GetOrderHeadersToBeSaved(OrgHeader org, List<ZGuid> alreadyMerged)
		{
			var mergedOrgs = new List<ZGuid>();
			var mergedOrgHeadersToBeSaved = new List<MergeOrgHeader>();
			foreach (OrgPatternMatch match in org.SimilarOrgMatches)
			{
				if (!mergedOrgs.Contains(match.Header.PK))
				{
					mergedOrgs.Add(match.Header.PK);

					var merge = new MergeOrgHeader(org.Factory, match.Header, org);
					merge.RunPreSaveValidation();
					if (!merge.HasErrors)
					{
						if (OrgHeaderMergingChecker.IsAllowedToMergeOrgs(org.PK, match.Header.PK, out string reasons))
						{
							merge.CurrentMode = CurrentQueryMode.Pattern;
							merge.CurrentMatchThresholdCode = OrgMatchThresholds.Codes.High;
							merge.HasToLoadSimilarOrgs = true;
							merge.MaxResults = 500;
							mergedOrgHeadersToBeSaved.Add(merge);
						}
						else
						{
							disallowedMergeMessages.Add(Res.GetString("76253d06-bac3-46a5-adaf-a7a3166eef12",
								"Merge {0} into {1}: {2}", merge.OldOrganisation.OH_FullName, merge.NewOrganisation.OH_FullName, reasons));
							alreadyMerged.Add(merge.OldOrganisation.PK);
						}
					}
				}
			}

			return mergedOrgHeadersToBeSaved;
		}
	}

	public class ManyToManyBatchPrcessedEvent : EventArgs
	{
		public ManyToManyBatchPrcessedEvent(int percentCompleted)
		{
			PercentCompleted = percentCompleted;
		}

		public int PercentCompleted { get; private set; }
	}

	public delegate void ManyToManyEventHandler(object sender, ManyToManyBatchPrcessedEvent e);
}
