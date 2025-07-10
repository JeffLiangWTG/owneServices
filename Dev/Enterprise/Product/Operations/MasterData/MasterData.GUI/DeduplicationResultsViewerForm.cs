using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DeduplicationResultsViewerForm : ZChildForm
	{
		public DeduplicationResultsViewerForm(object master, IEnumerable<object> targets, IEnumerable<ScoringResult> results, IEnumerable<PatternMatchingResultModel> resultModels, ZGuid selectedItemPK)
		{
			InitializeComponent();
			ExcludeDeletedItems(master, targets, results);
			if (results.Any())
			{
				Results = results;
				Master = master;
				Targets = targets;
				ResultModels = resultModels;
				SelectedItemPK = selectedItemPK;

				if (!DesignModeFinder.IsDesigning)
				{
					BackColor = SystemDataRegistry.Instance.ColorTheme.TabBackgroundColor;
				}
			}
			else
			{
				Globals.Message.ShowInformation(CommonMessage.DuplicateRecordRemovedPressCtrlG);
				Close();
			}
		}

		void ExcludeDeletedItems(object master, IEnumerable<object> targets, IEnumerable<ScoringResult> results)
		{
			var scoringResult = results as List<ScoringResult>;
			if (master is OrgHeader)
			{
				scoringResult?.RemoveAll(o => !IsSelectedItemInDB(typeof(OrgHeader), o.TargetPK));
			}
			else if (master is GlbPerson)
			{
				scoringResult?.RemoveAll(o => !IsSelectedItemInDB(typeof(GlbPerson), o.TargetPK));
			}

			if (targets is List<IGlbPerson> glbPersonList)
			{
				glbPersonList?.RemoveAll(o => !IsSelectedItemInDB(typeof(GlbPerson), (o as DeduplicationGlbPerson).PER_PK));
			}
			else if (targets is List<IOrgHeader> orgHeaderList)
			{
				orgHeaderList?.RemoveAll(o => !IsSelectedItemInDB(typeof(OrgHeader), (o as DeduplicationOrgHeader).OH_PK));
			}
		}

		protected bool IsSelectedItemInDB(Type type, ZGuid targetPk)
		{
			return new BusinessObjectFactory().Load(type, targetPk) != null;
		}

		protected object Master { get; }
		protected IEnumerable<ScoringResult> Results { get; }
		protected IEnumerable<PatternMatchingResultModel> ResultModels { get; }
		protected ZGuid SelectedItemPK { get; }
		protected IEnumerable<object> Targets { get; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();

			if (!DesignModeFinder.IsDesigning && Master != null && ResultModels != null && ResultModels.Any())
			{
				Shown += new EventHandler((s, eArgs) => SetupDataContext());
			}
		}

		void SetupDataContext()
		{
			UpdateStatusBar(ResString.GetMultilingualString("a461485b-c8b5-4697-b75f-e126b09614ff", "Searching for additional duplicates..."), null);

			if (Master is OrgHeader master)
			{
				var resultDetail = new DeduplicationOrganisationResultDetail(new DeduplicationOrgHeader(master), Targets.Cast<DeduplicationOrgHeader>(), Results, ResultModels, Close)
				{
					SelectedCandidatePK = SelectedItemPK
				};

				ResultsViewerUserControl.SetupDataContext(resultDetail, isAdminPanel: false);
			}

			if (Master is GlbPerson person)
			{
				var resultDetail = new DeduplicationPersonResultDetail((DeduplicationGlbPerson)person.CreateIGlbPerson(), Targets.Cast<DeduplicationGlbPerson>(), Results, ResultModels, Dispose)
				{
					SelectedCandidatePK = SelectedItemPK
				};

				ResultsViewerUserControl.SetupDataContext(resultDetail, isAdminPanel: false);
			}

			UpdateStatusBar(string.Empty, null);
		}

		public override string FormVerb => string.Empty;
		protected override bool AllowNew
		{
			get { return false; }
		}

		#region Form Caption

		public override string FormCaption
		{
			get { return ResString.GetMultilingualString("592A5FB6-FEE5-499D-823A-5FAED0F81562", "Potential Duplicate Records"); }
		}

		#endregion
	}
}
