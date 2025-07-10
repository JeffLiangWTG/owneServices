using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolTemplateGenerator : AutoConsolTemplateGenerator
	{
		public ConsolTemplateGenerator(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection.Factory)
		{
			this.multiDaysSelection = multiDaysSelection;
		}
		readonly MultiDaysSelection multiDaysSelection;

		#region ConsolTemplates

		[ChildEditable(true)]
		public ConsolTemplateCollection ConsolTemplates
		{
			get
			{
				if (consolTemplates == null)
				{
					consolTemplates = new ConsolTemplateCollection(multiDaysSelection);
					RegisterEditableChildObject(consolTemplates);
				}

				return consolTemplates;
			}
		}
		ConsolTemplateCollection consolTemplates;

		#endregion

		#region ServiceLevel

		[CargoWise.ComponentModel.List("NeutralAirWaybillServiceLevelList")]
		public override ZString ServiceLevel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return base.ServiceLevel;
			}
			[System.Diagnostics.DebuggerStepThrough]
			set
			{
				base.ServiceLevel = value;
			}
		}

		public OrgCarrierServiceLevelCollection NeutralAirWaybillServiceLevelList
		{
			get
			{
				OrgCarrierServiceLevelCollection lCarrierServiceLevels;
				if (fCarrierServiceLevels == null)
				{
					lCarrierServiceLevels = JobMawb.GetNeutralAirWaybillServiceLevelsFrom2LetterCode(Factory, GlbBranch.CurrentBranch, multiDaysSelection.FirstCarrier);
				}
				else
				{
					lCarrierServiceLevels = fCarrierServiceLevels;
				}

				lCarrierServiceLevels.Load();
				fCarrierServiceLevels = lCarrierServiceLevels;
				return fCarrierServiceLevels;
			}
		}

		OrgCarrierServiceLevelCollection fCarrierServiceLevels;

		#endregion

		#region GenerateConsols

		public void GenerateConsols(IReadOnlyList<JobSailingCollection> sailingList)
		{
			if (sailingList == null || ConsolTemplates.Count == 0)
			{
				return;
			}

			var templates = LoadTemplates();
			if (templates.Length == 0)
			{
				return;
			}

			foreach (JobSailingCollection sailings in sailingList)
			{
				if (sailings == null)
				{
					continue;
				}

				foreach (var templateRecord in templates)
				{
					var consolsPerFlight = consolTemplates.OfType<ConsolTemplate>().First(c => c.ConsolTemplateReferenceId.EqualsIgnoringCase(templateRecord.STR_ReferenceId)).ConsolsPerFlight;
					for (ZShort i = 0; i < consolsPerFlight; i++)
					{
						var consol = GetNewConsolWithTransports(sailings, templateRecord);
						using (consol.GetValidationSuspender())
						{
							consol.JK_IsNeutralMaster = AllocateNeutralMaster;
							consol.JK_TransportMode = Core.Constants.TransportModes.Air;
							consol.JK_AWBServiceLevel = ServiceLevel;
						}

						consol.Validation.ValidateJK_MasterBillNum();
						consol.Validation.ValidateJK_RL_NKLoadPort();
						consol.Validation.ValidateJK_RL_NKDischargePort();
						consol.Validation.ValidateJK_JX_JA_E_DEP();
						consol.Validation.ValidateJK_JX_JB_E_LastARV();
						consol.Validation.ValidateJK_OA_ShippingLineAddress();
					}
				}
			}
		}

		#endregion

		#region Implementation

		StmTemplateRecord[] LoadTemplates()
		{
			var templateReferenceIds = ConsolTemplates.Cast<ConsolTemplate>().Select(c => c.ConsolTemplateReferenceId).ToArray();
			var query = new ZQuery(StmTemplateRecordSchema.STR_ReferenceId, templateReferenceIds);
			query.AddToFilter(StmTemplateRecordSchema.STR_ModuleID, ModuleIDs.JobConsol.Name);

			return Factory.Load<StmTemplateRecord>(query);
		}

		ForwardingConsol GetNewConsolWithTransports(JobSailingCollection sailings, StmTemplateRecord templateRecord)
		{
			var consol = GetNewConsol(templateRecord);

			for (int index = 0; index < sailings.Count; index++)
			{
				var sailing = sailings[index];
				if ((consol.Transports.Count == 1) && (index == 0))
				{
					CopySailingToTransport(consol.Transports[0], sailing);
					continue;
				}

				var newTransport = consol.Transports.AddNew();
				CopySailingToTransport(newTransport, sailing);
			}

			return consol;
		}

		void CopySailingToTransport(Transport transport, JobSailing sailing)
		{
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
		}

		ForwardingConsol GetNewConsol(StmTemplateRecord templateRecord)
		{
			var result = CreateNewConsol(templateRecord);
			multiDaysSelection.CreatedConsols.Add(result);

			return result;
		}

		ForwardingConsol CreateNewConsol(StmTemplateRecord templateRecord)
		{
			ForwardingConsol consol;
			Factory.SuspendValidation();

			try
			{
				var templateRecordFactory = new TemplateRecordBusinessObjectFactory();
				var consolProvider = templateRecordFactory.New<ForwardingConsol>();
				(consolProvider as ITemplateRecordProvider).LoadFromTemplateRecord(templateRecord);

				consol = (ForwardingConsol)consolProvider.TemplateCopy(true, true, true, Factory);
				consol.TemplateRecord = templateRecord;
				consol.IsTemplateRecord = false;

				CopyCustomFields(consolProvider, consol);
			}
			finally
			{
				Factory.ResumeValidation();
			}

			return consol;
		}

		void CopyCustomFields(ForwardingConsol source, ForwardingConsol dest)
		{
			foreach (var customField in source.GetUserDefinedValues())
			{
				dest.SetUserDefinedValue(customField.PropertyName, customField.Value);
			}
		}

		#endregion
	}
}
