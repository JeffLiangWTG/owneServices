namespace Enterprise.Customs.NZ.GUI.MAFeBACCa
{
	using System;
	using System.Windows.Forms;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet;
	using Enterprise.Customs.NZ.Business.MAFeBACCa;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces;
	using Enterprise.Customs.NZ.GUI.Documents;
	using Enterprise.DocumentEngineCore.DocumentSupport;
	using Enterprise.Environment;
	using Enterprise.Licensing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Data.Mutex;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.PlugIn;

	public sealed class MAFeBACCaPlugIn : ZMutexedPlugIn
	{
		public MAFeBACCaPlugIn(IMAFPlugInSupport data)
			: base(data.Master)
		{
			Argument.NotNull(data, "data");
			this.data = data;
			mafMessaging = new MAFMessagingBO(data);
			data.PlugInVisibilityDataChanged += ChangeVisibility;
			ChangeVisibility(null, null);
			docEvents = data.DocumentEvents;
			if (docEvents != null)
			{
				docEvents.DocumentPrintRequested += DocumentEvents_DocumentPrintRequested;
			}
		}

		void ChangeVisibility(object sender, EventArgs e)
		{
			Enabled = data.PlugInVisible;
		}

		void DocumentEvents_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			if (e.MenuItem.SU_MenuName == JobDeclarationDocumentSupporter.DocNames.MAFCoverSheet)
			{
				var mafCoverSheet = new NZDocsMAFCoverSheet(mafMessaging);
				using (var mafCoverSheetForm = new MAFCoverSheetForm(mafCoverSheet))
				{
					e.Cancel = ZFormModaliser.ShowDialogWithoutDispose(mafCoverSheetForm) != DialogResult.Yes;
				}

				if (!e.Cancel)
				{
					var factory = ((BusinessObject)sender).Factory;
					factory.RemoveValue<NZDocsMAFCoverSheet>();
					factory.SetValue(() => mafCoverSheet);
				}
			}
		}

		#region Overrides of ZMutexedPlugIn

		protected override ZBool HasUserControl
		{
			get { return true; }
		}

		protected override LicenceCheckpoint LicenceCheckPoint
		{
			get { return Env.Licence.ImportQuarantineMessaging; }
		}

		protected override Control GetNewUserControl()
		{
			return new MAFeBACCaUserControl(mafMessaging);
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			return new MAFeBACCaMenu(mafMessaging);
		}

		public override string Name
		{
			get { return "eBACCa/IPI"; }
		}

		protected override string TextOverride
		{
			get { return Name; }
		}

		protected override bool ShouldPlugInGUIAndBusinessEntityBeCreatedCore()
		{
			return !mafMessaging.PlugInSupport.IsPlugInNew || Mutex.Lock();
		}

		public override ZString PlugInNotDisplayedMessage
		{
			get
			{
				var info = Mutex.GetLockInfo();
				string result;

				if (info != null)
				{
					result = string.Format(@"{0} ({1}) is already in the process of creating eBACCa for this job since {2}.
You cannot process with it until data is saved. Please try later.",
						info.GetUserWithLock(),
						info.HostName,
						EnvProxy.Instance.Time.GetLocalTimeFromUtc(info.LockStartTime.ToDateTime()));
				}
				else
				{
					result = "Someone else is already in the process of creating eBACCa for this job. You cannot process with it until data is saved.Please try later.";
				}
				return result;
			}
		}

		public override ZGlobalMutex Mutex
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, Name + mafMessaging.PlugInSupport.Master.PK)); }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (data != null)
				{
					data.PlugInVisibilityDataChanged -= ChangeVisibility;
				}

				if (docEvents != null)
				{
					docEvents.DocumentPrintRequested -= DocumentEvents_DocumentPrintRequested;
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		ZGlobalMutex mutex;
		readonly MAFMessagingBO mafMessaging;
		readonly IMAFPlugInSupport data;
		readonly IDocumentEvents docEvents;
	}
}
