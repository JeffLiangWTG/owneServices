using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.RefDataRepo.Ent.Client;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefDataForm : ZChildForm
	{
		public RefDataForm()
		{
			InitializeComponent();
			InitializeLogger();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var encryptedKey = DataRegistry.Instance.RawRegistry.EncryptedRegistrationKey.Value;
			if (string.IsNullOrEmpty(encryptedKey))
			{
				TerminateWithInfo();
			}
			else
			{
				LoadDataSetUpdaterWrapper();
			}

			if (dataSetUpdaters != null && dataSetUpdaters.IsCountMoreThan(0))
			{
				dataSetUpdaterComboBox.DataSource = dataSetUpdaters.Select(x => x.Name).ToArray();
			}
		}

		void TerminateWithInfo()
		{
			loadButton.Enabled = false;
			logger.Log(LogType.Warning, "Terminate because the system does not have a registration.");
			logger.Log(LogType.Warning, "Please register your system in order to use Reference Service.");
		}

		void LoadDataSetUpdaterWrapper()
		{
			try
			{
				loadButton.Enabled = false;

				using (Db.DisposableActionForDbConnection())
				{
					Db.Connection.EnsureIsOpen();

					sRDbDataSetUpdaterWrapper = ObjectFactory.Get<ISRDbDataSetUpdaterWrapper>();
					refDataSetUpdaterWrapper = ObjectFactory.Get<IRefDataSetUpdaterWrapper>();
					sRDbDataSetUpdaterWrapper.SetLogger(logger);
					refDataSetUpdaterWrapper.SetLogger(logger);
#if DEBUG
					if (DataUtils.LoadDbExtendedProperty(Db.Connection, RefDbTableNameResolver.SingleRefDatabaseConsumeDATSnapshot, RefDbTableNameResolver.DefaultSingleRefDbName) == "Y")
					{
						logger.Log(LogType.Information, "SRDb is set to use DAT Snapshot, RDU won't run, please go to Testing -> Reset Single Reference Database -> Use Reference Service menu to switch");
						return;
					}
#endif
					logger.Log(LogType.Information, "Upgrading SRDb schema.");
					try
					{
						if (sRDbDataSetUpdaterWrapper.IsSchemaUpgradeSuccessful())
						{
							logger.Log(LogType.Information, "SRDb Schema upgraded successfully.");
						}
						else
						{
							logger.Log(LogType.Information, "Upgrading SRDb schema failed. Please try again later");
						}
					}
					catch (Exception ex)
					{
						if (errorReportingClientWrapper.Value.PostCrashReport(ex))
						{
							logger.Log(LogType.Error, "Error report sent");
						}
#if DEBUG
						logger.Log(LogType.Error, "Please use Help -> Database Administraion -> Reference Data option and try again");
#endif
						logger.Log(LogType.Error, string.Format(CultureInfo.InvariantCulture, "Can not upgrade SRDb because" + ex.ToString()));
					}

					logger.Log(LogType.Information, "Loading SRDb Data Set Updaters and Versions.");

					logger.Log(LogType.Information, "Loading Data Set Updaters and Versions.");
#if DEBUG
					if (Globals.IsTest)
					{
						dataSetUpdaters = Array.Empty<ISharedDataSetUpdater>();
					}
					else
					{
#endif
						dataSetUpdaters = refDataSetUpdaterWrapper.GetAllDataSetUpdater()
											.Concat(sRDbDataSetUpdaterWrapper.GetAllDataSetUpdater())
											.OrderBy(x => x.Name);
#if DEBUG
					}
#endif
				}

				logger.Log(LogType.Information, "Data Set Updaters and Versions loaded successfully.");
				loadButton.Enabled = true;
			}

			catch (AggregateException ex)
			{
				var errorMessages = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} : {1}", ex.Message, ex.StackTrace);

				var inner = ex.InnerException;
				while (inner != null)
				{
					errorMessages += ((NoResString)"Inner Exception: " + inner.Message);
					errorMessages += ((NoResString)"Inner Exception: " + inner.StackTrace);
					inner = inner.InnerException;
				}

				logger.Log(LogType.Error, "Unable to load Data Set Updaters and Versions.\r\n" + errorMessages);
			}
		}

		void loadButton_Click(object sender, EventArgs e)
		{
			try
			{
				loadButton.Enabled = false;

				if (dataSetUpdaterComboBox.SelectedIndex == -1 || string.IsNullOrEmpty(dataSetUpdaterComboBox.SelectedValue.ToString()))
				{
					logger.Log(LogType.Error, "Please select data set updater.");
				}
				else if (dataSetVersionComboBox.SelectedIndex == -1 || string.IsNullOrEmpty(dataSetVersionComboBox.SelectedValue.ToString()))
				{
					logger.Log(LogType.Error, "Please select data set version.");
				}
				else
				{
					RunUpdater();
				}
			}
			finally
			{
				loadButton.Enabled = true;
			}
		}

		void RunUpdater()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Db.Connection.EnsureIsOpen();
				var updaterName = dataSetUpdaterComboBox.SelectedValue.ToString();
				if (DataSetUpdaterHelper.GetUpdaterType(updaterName) == UpdaterType.RDU)
				{
					sRDbDataSetUpdaterWrapper.Update(DataSetUpdaterHelper.GetTableName(updaterName), dataSetVersionComboBox.SelectedValue.ToString());
				}
				else
				{
					refDataSetUpdaterWrapper.Update(DataSetUpdaterHelper.GetTableName(updaterName), dataSetVersionComboBox.SelectedValue.ToString());
				}
			}
		}

		void dataSetUpdaterComboBox_Changed(object sender, EventArgs e)
		{
			if (dataSetUpdaterComboBox.SelectedIndex == -1 || string.IsNullOrEmpty(dataSetUpdaterComboBox.SelectedValue.ToString()))
			{
				return;
			}

			var selectedDataSetUpdater =
				dataSetUpdaters.FirstOrDefault(x => string.Compare(x.Name, dataSetUpdaterComboBox.SelectedValue.ToString(),
														StringComparison.OrdinalIgnoreCase) == 0);
			if (selectedDataSetUpdater != null)
			{
				dataSetVersionComboBox.DataSource = selectedDataSetUpdater.DataSetNames.ToArray();
			}
		}

		void InitializeLogger()
		{
			loggerMessage = new StringBuilder();
			logger = new RefDataFormLogger
			{
				OnLog = (message) =>
				{
					loggerMessage.AppendLine(message);
					outputTextBox.Text = loggerMessage.ToString();
				}
			};
		}

		ILogger logger;
		IRefDataSetUpdaterWrapper refDataSetUpdaterWrapper;
		ISRDbDataSetUpdaterWrapper sRDbDataSetUpdaterWrapper;
		IEnumerable<ISharedDataSetUpdater> dataSetUpdaters;
		StringBuilder loggerMessage;
		readonly Lazy<ErrorReportingClientWrapper> errorReportingClientWrapper = new Lazy<ErrorReportingClientWrapper>();

		protected override void Dispose(bool isNotFinalizing)
		{
			if (refDataSetUpdaterWrapper != null)
			{
				refDataSetUpdaterWrapper.Dispose();
				refDataSetUpdaterWrapper = null;
			}
			if (sRDbDataSetUpdaterWrapper != null)
			{
				sRDbDataSetUpdaterWrapper.Dispose();
				sRDbDataSetUpdaterWrapper = null;
			}

			base.Dispose(isNotFinalizing);
		}
	}
}
