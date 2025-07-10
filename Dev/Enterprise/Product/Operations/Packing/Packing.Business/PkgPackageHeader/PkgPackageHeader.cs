using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	[DebuggerDisplay("PackageID={KPH_PackageID}")]
	[DeferTriggerAndRunBeforeCommit("TG_PkgPackageHeaderUniqueIDPerJob", "PkgPackageHeaderUniqueIDPerJob", PkgPackageHeaderSchema.Constants.PK, typeof(IPkgPackageHeaderUniqueIDPerJobStrategy_PkgPackageHeaderInsertUpdate))]
	public class PkgPackageHeader : AutoPkgPackageHeader, ISupportPackageIDGenerationInternals, IDocumentSupportable
	{
		public PkgPackageHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZString KPH_PackageID
		{
			get { return base.KPH_PackageID; }
			set
			{
				base.KPH_PackageID = value;
				SetSystemCreateTimeAndUserIfRequired();
			}
		}

		#endregion

		#region CurrentPackageJob

		public PkgPackageJob CurrentPackageJob
		{
			get
			{
				if (currentPackageJob == null)
				{
					ErrorReporter.ReportOnce("PkgPackageHeader|CurrentPackageJob", "CurrentPackageJob should always be set if it needs to be used. ie PackageHeader > Document requires CurrentPackageJob. When delivering Documents, CurrentPackageJob should be set via the UI."); // This is a Developer Error
				}

				return currentPackageJob;
			}
			set { currentPackageJob = value; }
		}
		PkgPackageJob currentPackageJob;

		#endregion

		#region SetSystemCreateTimeAndUserIfRequired

		void SetSystemCreateTimeAndUserIfRequired()
		{
			// SplitPackingAfterOnSavingBOProcessingService creates PkgPackageHeaders after OnSaving inside the Save Transaction. ZArchitecture does not end up setting the SystemCreate fields.
			if (Factory.IsInTransaction && !IsInDatabase)
			{
				var currentUser = new Lazy<ZString>(() => GlbStaff.CurrentUser.GS_Code);

				if (KPH_SystemCreateUser.IsEmpty)
				{
					KPH_SystemCreateUser = currentUser.Value;
				}

				if (KPH_SystemCreateTimeUtc.IsEmpty)
				{
					KPH_SystemCreateTimeUtc = Factory.TransactionStartedTimeUtc;
				}

				if (KPH_SystemLastEditUser.IsEmpty)
				{
					KPH_SystemLastEditUser = currentUser.Value;
				}

				if (KPH_SystemLastEditTimeUtc.IsEmpty)
				{
					KPH_SystemLastEditTimeUtc = Factory.TransactionStartedTimeUtc;
				}
			}
		}

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new PkgPackageHeaderDocumentSupporter(this); }
		}

		#endregion

		#region ISupportPackageIDGeneration Members

		ZString ISupportPackageIDGeneration.KP_PackageID
		{
			get { return KPH_PackageID; }
			set { KPH_PackageID = value; }
		}

		bool ISupportPackageIDGeneration.ShouldGenerateIDOnSaving { get; set; }
		ZInt ISupportPackageIDGeneration.KP_PackageQty => 1;
		bool ISupportPackageIDGeneration.IsContainer => false;
		bool ISupportPackageIDGeneration.IsBookedViaCarrier => false;
		IEnumerable<ISupportPackageIDGeneration> ISupportPackageIDGeneration.Packages => Array.Empty<ISupportPackageIDGeneration>();
		PkgPackageJob ISupportPackageIDGeneration.PackageJob => CurrentPackageJob;
		ZGuid ISupportPackageIDGeneration.PackageJobPK => CurrentPackageJob?.PK ?? ZGuid.Empty;

		void ISupportPackageIDGenerationInternals.CallAfterIDGenerated() => AfterIDGenerated?.Invoke(this, EventArgs.Empty);

		public event EventHandler AfterIDGenerated;

		#endregion

		#region Save

		protected sealed override void OnFactorySaving()
		{
			base.OnFactorySaving();
			// tested in PackageIDGenerationHelperTest.TestOnFactorySave_ShouldGenerateIDOnSaving_PackageHeader()
			PackageIDGenerationHelper.AddPackageForIDGenerationIfMarked(Factory, this);
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (!saveSucceeded)
			{
				// tested in PackageIDGenerationHelperTest.TestShouldGenerateIDOnSaving_ShouldClearOnSaveFailure_PackageHeader()
				PackageIDGenerationHelper.Clear(Factory);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var query = new ZQuery();
			query.AddToFilter(PkgPackageJobPackageHeaderPivotSchema.KPJ_KPH_PackageHeader, PK);
			Factory.Load<PkgPackageJobPackageHeaderPivot>(query).ForEach(p => p.Delete());

			base.Delete();
		}

		#endregion
	}
}
