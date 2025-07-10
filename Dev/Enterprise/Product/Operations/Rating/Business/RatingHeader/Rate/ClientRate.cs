using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Rating.Business
{
	[UniversalDataContext(DataContextType.ClientRate)]
	[CodeProperty(RatingHeader.Schema.TH_ClientCode), DescriptionProperty(RatingHeader.Schema.TH_ClientCode)]
	[BusinessContext(BusinessContext.Rating)]
	public class ClientRate : RatingHeader, IRelatableActivity, IImportParentRelatedActivityInfoOnNew, ITemplateCopyable, IWorkflowProvider
	{
		public ClientRate(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		#region Update New Rate End Date

		/// <summary>
		/// Sets end date on the ClientRate Header and it's ClientRate entries.
		/// Used when changing a Quote --> ClientRate.
		/// </summary>
		public ZDate TH_NewRateEndDate
		{
			get { return fTH_NewRateEndDate; }
			set
			{
				fTH_NewRateEndDate = value;
				Validation.ValidateTH_NewRateEndDate();
				TH_NewRateEndDateInfo.RefreshBinding();

				if (!TH_NewRateEndDateInfo.HasErrors())
				{
					foreach (var entry in AllEntries)
					{
						if (entry.HasChanges)
						{
							entry.TI_RateEndDate = fTH_NewRateEndDate;
						}
					}
				}
			}
		}

		ZDate fTH_NewRateEndDate;

		public ZPropertyInfo TH_NewRateEndDateInfo
		{
			get { return GetZPropertyInfo(nameof(TH_NewRateEndDate)); }
		}

		#endregion

		#region Copy Rate

		protected override void SetNewValuesInHeader(RatingHeader newHeader)
		{
			base.SetNewValuesInHeader(newHeader);
			foreach (RateEntryCollection entryCollection in ((ClientRate)newHeader).EntryCollectionsExcludingSummary.Values)
			{
				SetNewValuesInEntryCollection(entryCollection);
			}
		}

		protected override void SetNewValuesInEntry(RateEntry newEntry)
		{
			newEntry.TI_RateStartDate = ZDate.Today;
			newEntry.TI_RateEndDate = newEntry.DefaultRateEndDate;
		}

		public void CopyPersistentValuesFrom(Quote quote)
		{
			base.CopyPersistentValuesFrom(quote);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Properties

		protected override string RatingHeaderTypeDescriptionCore => TH_GC.IsEmpty
			? Res.GetString("6c5fb67b-fdc3-49be-917d-76bd2ea56ceb", "Global Client Rate")
			: Res.GetString("67882cb9-5e97-460d-a7b2-c0f280e40e39", "Client Rate");

		public override ZString DisplayInfo()
			=> DisplayInfoWithOrgInfo(RatingHeaderTypeDescription, Header);

		[BusinessObjectTestExclude()]   // Getter and Setter aren't sycnhronised
		public override ZDecimal TH_AirCFX => GetCfxUplift(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Air);

		[BusinessObjectTestExclude()]   // Getter and Setter aren't sycnhronised
		public override ZDecimal TH_SeaCFX => GetCfxUplift(OrgConstants.ServiceDirection.Code.Import, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);

		[BusinessObjectTestExclude()]   // Getter and Setter aren't sycnhronised
		public override ZDecimal TH_ExportAirCFX => GetCfxUplift(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Air);

		[BusinessObjectTestExclude()]   // Getter and Setter aren't sycnhronised
		public override ZDecimal TH_ExportSeaCFX => GetCfxUplift(OrgConstants.ServiceDirection.Code.Export, OrgConstants.ModesForGroupOrSubTotal.Codes.Sea);

		ZDecimal GetCfxUplift(ZString serviceDirection, ZString transportMode)
		{
			if (Header == null)
			{
				return ZDecimal.Zero;
			}
			return AccCFXConfigurations.GetRecord("ALL", serviceDirection, transportMode)?.JCF_CFXPercentage ?? 0m;
		}

		#endregion

		#region Validation

		public new ClientRateValidation Validation
		{
			get { return (ClientRateValidation)base.Validation; }
		}

		protected override RatingHeaderValidation GetNewValidation()
		{
			return new ClientRateValidation(this);
		}

		#endregion

		#region Unaccepted Quotes

		UnacceptedQuotesCollection fUnacceptedQuotesCollection;

		public UnacceptedQuotesCollection UnacceptedQuotesCollection
		{
			get
			{
				if (fUnacceptedQuotesCollection == null)
				{
					fUnacceptedQuotesCollection = new UnacceptedQuotesCollection(new BusinessObjectFactory(), Company, TH_OH);
					fUnacceptedQuotesCollection.Load();
				}
				return fUnacceptedQuotesCollection;
			}
		}

		public override ZGuid TH_OH
		{
			get { return base.TH_OH; }
			set
			{
				base.TH_OH = value;
				fUnacceptedQuotesCollection = null;
			}
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.ClientRates; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return Header; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return TH_OHInfo.HasChanges; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return Res.GetString("459deaf3-3ba0-45c8-8334-19c1cd5b1b1d", "Client Rate; {0}", Header.OH_Code); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region IImportParentRelatedActivityInfoOnNew

		bool IImportParentRelatedActivityInfoOnNew.ImportParentInfo(IRelatableActivity parentActivity, IImportRelatedActivityDeciderFactory deciderFactory)
		{
			if (parentActivity.Client != null)
			{
				TH_OH = parentActivity.Client.PK;
			}

			return true;
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var newRate = CopyIncludingChildren();
			newRate.TH_OH = ZGuid.Empty;

			return newRate;
		}

		#endregion

		#region Save Client Rate

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ViewRelatedActivityPivot.DeleteAllPivots(this);

			base.Delete();
			WorkflowItems.RemoveAndDeleteAll();
		}

		#endregion

		#region IWorkflowProvider Members

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ClientRateProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.ClientRateWorkflowDescriptorCode; }
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
		}
#endif
	}
}

