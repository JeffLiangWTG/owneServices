using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer.ForwardAir;
using Enterprise.Rating.DataTransfer.TACT;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Rating.DataTransfer.TACT.TACTImporter;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Rating.DataTransfer
{
	public class RateDataImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public static string ImportFailedPhrase => Res.GetString("a55fe4ab-595e-4984-bb4a-f48c724a5b11", "Import failed");

		#region Constructor

		public static RateDataImporter New(RatingHeader ratingHeader)
		{
			return (RateDataImporter)Activator.CreateInstance(CreateType, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new object[] { ratingHeader }, null);
		}

		public static void RegisterType(Type type)
		{
			CreateType = type;
		}

		public static void UnRegisterType()
		{
			CreateType = typeof(RateDataImporter);
		}

		static Type CreateType
		{
			get { return createType.Value; }
			set { createType.Value = value; }
		}

		static readonly Overridable<Type> createType = new Overridable<Type>(typeof(RateDataImporter));

		protected RateDataImporter(RatingHeader ratingHeader)
		{
			this.RatingHeader = ratingHeader;
		}

		#endregion

		#region Rating Header

		public RatingHeader RatingHeader { get; }

		public RatingHeader.ProgressChangedEventHandler ProgressHandler { get; set; }
		public Action ImportCompletedHandler { get; set; }

		void ReloadRates()
		{
			RatingHeader.ReloadCollectionsWithNoFilter();
		}

		#endregion

		#region Properties

		public ZBool ShouldClearOldDuplicatesOnly
		{
			get { return shouldClearOldDuplicatesOnly; }
			set { SetNonPersistentPropertyValue(ClearDuplicatesOnlyInfo, ref shouldClearOldDuplicatesOnly, value); }
		}
		ZBool shouldClearOldDuplicatesOnly;
		public ZPropertyInfo ClearDuplicatesOnlyInfo => GetZPropertyInfo(nameof(ShouldClearOldDuplicatesOnly));

		public ZBool ShouldClearOldTACTRates
		{
			get { return shouldClearOldTACTRates; }
			set { SetNonPersistentPropertyValue(ShouldClearOldTACTRatesInfo, ref shouldClearOldTACTRates, value); }
		}
		ZBool shouldClearOldTACTRates;
		public ZPropertyInfo ShouldClearOldTACTRatesInfo => GetZPropertyInfo(nameof(ShouldClearOldTACTRates));

		public ZBool ShouldExcludeFromAutoRating
		{
			get { return shouldExcludeFromAutoRating; }
			set { SetNonPersistentPropertyValue(ShouldExcludeFromAutoRatingInfo, ref shouldExcludeFromAutoRating, value); }
		}
		ZBool shouldExcludeFromAutoRating;
		public ZPropertyInfo ShouldExcludeFromAutoRatingInfo => GetZPropertyInfo(nameof(ShouldExcludeFromAutoRating));

		public ZBool IsJobLevelCharge
		{
			get { return isJobLevelCharge; }
			set { SetNonPersistentPropertyValue(IsJobLevelChargeInfo, ref isJobLevelCharge, value); }
		}
		ZBool isJobLevelCharge;
		public ZPropertyInfo IsJobLevelChargeInfo => GetZPropertyInfo(nameof(IsJobLevelCharge));

		[List("RatingRoundingTypeList")]
		[MaxLength(3)]
		public ZString Rounding
		{
			get { return rounding; }
			set
			{
				SetNonPersistentPropertyValue(RoundingInfo, ref rounding, value);
				RoundingInfo.ClearAllNotifications();

				if (!IsValidationSuspended)
				{
					ListValidation.ErrorIfInvalidCode(RoundingInfo);
				}
			}
		}
		ZString rounding;
		public ZPropertyInfo RoundingInfo => GetZPropertyInfo(nameof(Rounding));

		public CodeDescriptionPairList RatingRoundingTypeList
		{
			get
			{
				if (ratingRoundingTypeList == null)
				{
					ratingRoundingTypeList = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair item in new RatingRoundingTypeList())
					{
						ratingRoundingTypeList.AddPair(item.Code, item.MultilingualDescription);
					}
				}
				return ratingRoundingTypeList;
			}
		}
		CodeDescriptionPairList ratingRoundingTypeList;

		public ZBool ShouldClearOldStandardRates
		{
			get { return shouldClearOldStandardRates; }
			set { SetNonPersistentPropertyValue(ShouldClearOldStandardRatesInfo, ref shouldClearOldStandardRates, value); }
		}
		ZBool shouldClearOldStandardRates;
		public ZPropertyInfo ShouldClearOldStandardRatesInfo => GetZPropertyInfo(nameof(ShouldClearOldStandardRates));

		public bool ShowClearRatesOptions { get; set; }

		public virtual bool IsStandardTACTImport
		{
			get { return true; }
		}
		protected virtual bool EnableIATARateImportCore
		{
			get { return RatingHeader.IsStandardCostRate(); }
		}
		public bool EnableIATARateImport
		{
			get { return EnableIATARateImportCore; }
		}

		public string ImportReport { get; protected set; }

		public bool IsDefaultFreightChargeCodeValid
		{
			get
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				var chargeCode = factory.Load<AccChargeCode>(Env.Registry.FreightChargeCode);
				if (chargeCode == null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
					query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);
					chargeCode = factory.LoadTop1<AccChargeCode>(query);
				}
				return chargeCode != null;
			}
		}

		#endregion

		#region Sort Entries With Error

		public void SortEntriesWithError()
		{
			var collection = RatingHeader.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;

			using (collection.SuspendListChanged())
			{
				collection.Sort(new EntryWithErrorComparer());
			}
		}

		class EntryWithErrorComparer : IComparer<RateEntry>
		{
			public int Compare(RateEntry x, RateEntry y)
			{
				if (x.HasErrors && !y.HasErrors)
				{
					return -1;
				}
				else if (!x.HasErrors && y.HasErrors)
				{
					return 1;
				}
				else if (!x.HasErrors && !y.HasErrors && x.HasWarnings && !y.HasWarnings)
				{
					return -1;
				}
				else if (!x.HasErrors && !y.HasErrors && !x.HasWarnings && y.HasWarnings)
				{
					return 1;
				}
				else
				{
					if (x.TI_OriginLRC != y.TI_OriginLRC)
					{
						return x.TI_OriginLRC.CompareTo(y.TI_OriginLRC);
					}
					else
					{
						return x.TI_DestinationLRC.CompareTo(y.TI_DestinationLRC);
					}
				}
			}
		}

		#endregion

		#region TACT

		ExistingRatesHandlingStrategy ExistingRatesHandlingStrategy
		{
			get
			{
				var strategy = ExistingRatesHandlingStrategy.None;

				if (ShouldClearOldTACTRates)
				{
					strategy |= ExistingRatesHandlingStrategy.ClearTACTRates;
				}

				if (ShouldClearOldStandardRates)
				{
					strategy |= ExistingRatesHandlingStrategy.ClearStandardRates;
				}

				return strategy;
			}
		}

		static FixedWidthFlatFileFormat GetFormatter(string fileName, Stream stream)
		{
			var length = GetTACTLinesLength(stream);
			if (length == null)
			{
				Globals.Message.Show(
					ResString.GetMultilingualString("c71fbecd-0163-451d-8d55-9a3f2bfa2935", @"The selected file is NOT a valid TACT file. Please select a valid TACT file format for importing."),
					ResString.GetMultilingualString("d573ce98-53a4-46e1-bea8-f00da3a746bf", "Invalid file"),
					ZMessageBoxButtons.OK,
					ZMessageBoxIcon.Error,
					ZDialogResult.OK);

				return null;
			}

			var fileExtention = Path.GetExtension(fileName);

			if (fileExtention.EndsWith(TACTData150FixedWidthDataFormat.Constants.FileFormat, StringComparison.OrdinalIgnoreCase) ||
				length == TACTData150FixedWidthDataFormat.Constants.RowLength)
			{
				return new TACTData150FixedWidthDataFormat();
			}

			if (fileExtention.EndsWith(TACTData80FixedWidthDataFormat.Constants.FileFormat, StringComparison.OrdinalIgnoreCase) ||
				length == TACTData80FixedWidthDataFormat.Constants.RowLength)
			{
				return new TACTData80FixedWidthDataFormat();
			}

			var caption = ResString.GetMultilingualString("5e85f57f-4fc7-11e7-8154-fcaa14295823", "Unknown file extension");
			var msg = ResString.GetMultilingualString("1a1ad5c9-4fc9-11e7-b8d5-fcaa14295823", @"'{0}' is not supported for IATA TACT Rate Importing. It will be processed as a 80 character formatted TACT file but there may be irregularities. {1} currently supports files with the following extensions:
 '.146' for 80 Character formatted TACT files
 '.054' for 150 Character formatted TACT files", fileExtention, BrandingFactory.Instance.ProductName);

			var dialogResult = Globals.Message.Show(msg, caption, ZMessageBoxButtons.OKCancel, ZMessageBoxIcon.Question, ZDialogResult.Cancel);
			if (dialogResult == ZDialogResult.OK)
			{
				return new TACTData80FixedWidthDataFormat();
			}

			return null;
		}

		static int? GetTACTLinesLength(Stream stream)
		{
			int? length = null;

			stream.Seek(0, SeekOrigin.Begin);

			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (string.IsNullOrEmpty(line))
					{
						// We just ingore empty lines
						continue;
					}

					if (length == null)
					{
						length = line.Length;
						continue;
					}

					if (length != line.Length)
					{
						return null;
					}
				}
			}

			return length;
		}

		public virtual void ImportIATA_TACT(string fileName, Stream stream)
		{
			if (!RatingHeader.IsStandardCostRate())
			{
				ErrorReporter.ReportOnce("IATA are global rates should only be imported into a standard (global) cost");
			}

			var formatter = GetFormatter(fileName, stream);
			if (formatter == null)
			{
				ImportReport = Res.GetString("966c405b-4fd3-11e7-b6d1-fcaa14295823", "Canceled");
				ImportCompletedHandler?.Invoke();
				return;
			}

			var notifications = new NotificationBuffer();

			stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();

			var company = RatingHeader.TH_GC.IsEmpty ? Guid.Empty : RatingHeader.TH_GC.ToGuid();
			var importOptions = new TACTImportOptions()
			{
				CompanyPK = company,
				RatingHeaderPK = RatingHeader.PK.ToGuid(),
				IsJobLevelCharge = IsJobLevelCharge,
				Rounding = Rounding,
				ShouldExcludeFromAutoRating = ShouldExcludeFromAutoRating
			};

			stream.Seek(0, SeekOrigin.Begin);

			var importer = new TACTImporter(stream, formatter, notifications, importOptions);
			importer.ProgressChanged += TACTImportProgressChanged;

			importer.ImportAsync(ExistingRatesHandlingStrategy).ContinueWith(TACTImportCompleted, notifications);
		}

		System.Diagnostics.Stopwatch stopWatch;

		void TACTImportProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			ThreadPool.QueueUserWorkItem(_ => ProgressHandler?.Invoke(e.ProgressPercentage, (string)e.UserState));
		}

		async Task TACTImportCompleted(Task<TACTImportResult> task, object state)
		{
			try
			{
				var res = await task;
				var notifications = (NotificationBuffer)state;

				stopWatch.Stop();
				ImportReport = BuildReport(res, notifications, stopWatch.Elapsed);
			}
			catch (Exception ex)
			{
				var sb = new StringBuilder();
				sb.AppendLine(ImportFailedPhrase);
				sb.AppendLine(ex.ToString());

				ImportReport = sb.ToString();
			}
			finally
			{
				ImportCompletedHandler?.Invoke();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Math.Round is fine")]
		static string BuildReport(TACTImportResult result, INotifications notifications, TimeSpan elapsed)
		{
			var sb = new StringBuilder();
			var elapsedRounded = TimeSpan.FromSeconds(Math.Round(elapsed.TotalSeconds, 1)); // Math.Round is fine
			sb.AppendLine(Res.GetString("aeb4349b-b2d1-4c6f-aa9a-3f4b7dffaf73", "Import completed in time {0}", elapsedRounded.ToString("g", CultureInfo.CurrentCulture)));
			sb.AppendLine();

			sb.AppendLine(Res.GetString("62fc56d8-4bfc-11e7-b734-fcaa14295823", "{0} rates have been successfully imported", result.ImportedRates));
			sb.AppendLine();

			if (result.InvalidRates > 0)
			{
				sb.AppendLine(Res.GetString("5f9f91b0-4b3a-11e7-b631-fcaa14295823", "{0} rates have been skipped due to invalid format on TACT file", result.InvalidRates));
				sb.AppendLine();
			}

			if (result.ExpiredRates > 0)
			{
				sb.AppendLine(Res.GetString("629f917a-4b3a-11e7-933a-fcaa14295823", "{0} rates have been skipped due to their expiration", result.ExpiredRates));
				sb.AppendLine();
			}

			if (result.DeleteActionRates > 0)
			{
				sb.AppendLine(Res.GetString("67C5B6FA-A1E4-46EE-B64A-2737D29A26CF", "{0} rates have been skipped as no longer valid - file is a difference file with records from the previous month", result.DeleteActionRates));
				sb.AppendLine();
			}

			if (result.UnknownCarriers.Any())
			{
				sb.AppendLine(Res.GetString("7b281eb6-4b3a-11e7-b7bd-fcaa14295823", "{0} rates were skipped due Carrier code(s) on TACT file not matched against any airline codes in the system: {1}", result.UnknownCarriers.Sum(r => r.Value), string.Join(", ", result.UnknownCarriers.Keys.Distinct())));
				sb.AppendLine();
			}

			if (result.UnknownCategories.Any())
			{
				sb.AppendLine(Res.GetString("77443254-4b3a-11e7-bf3c-fcaa14295823", "{0} rates were skipped due to unknown Category(s) on TACT file: {1}", result.UnknownCategories.Sum(r => r.Value), string.Join(", ", result.UnknownCategories.Keys.Distinct())));
				sb.AppendLine();
			}

			if (result.UnknownCarrierServiceLevel.Any())
			{
				sb.AppendLine(Res.GetString("69d5e9e8-4b3a-11e7-992f-fcaa14295823", "{0} rates were skipped due to unknown Carrier Service Level(s) on TACT file: {1}", result.UnknownCarrierServiceLevel.Sum(r => r.Value), string.Join(", ", result.UnknownCarrierServiceLevel.Keys.Distinct())));
				sb.AppendLine();
			}

			var logs = (NotificationBuffer)notifications;

			if (logs.HasErrors)
			{
				sb.AppendLine();
				sb.AppendLine(Res.GetString("56da3b49-4fe6-11e7-901f-fcaa14295823", "Errors:"));

				foreach (var evnt in logs.GetEventsByType(NotificationType.Error))
				{
					sb.AppendLine(evnt.Message);
				}
			}

			if (logs.HasWarnings)
			{
				sb.AppendLine();
				sb.AppendLine(Res.GetString("6bcc54f6-4fe6-11e7-be60-fcaa14295823", "Warnings:"));

				foreach (var evnt in logs.GetEventsByType(NotificationType.Warning))
				{
					sb.AppendLine(evnt.Message);
				}
			}

			return sb.ToString();
		}

		#endregion

		#region Forward Air

		public void ImportForwardAirRates(string fileName, Stream stream)
		{
			ReloadRates();

			var reader = new StreamReader(stream);
			var importer = new ForwardAirRatesFlatFileDataImporter(RatingHeader);
			var notificationContext = new NotificationBuffer();

			try
			{
				ITransactionParticipant[] additionalTransactionActions;
				var sourceInfo = new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.RateImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileName);
				importer.ImportDataToFactory(reader, fileName, notificationContext, sourceInfo, out additionalTransactionActions);

				if (!notificationContext.HasErrors)
				{
					ImportReport = "\r\n" + Res.GetString("75dbb391-db27-4c3f-bda5-c6f66bec619d", "Import completed");
				}
				else
				{
					ImportReport = notificationContext.AsString;
				}
			}
			finally
			{
				reader.Close();
				reader.Dispose();

				if (ImportCompletedHandler != null)
				{
					ImportCompletedHandler();
				}
			}
		}

		#endregion

		#region Active

		public bool IsActive
		{
			get { return IsActiveCore; }
		}

		protected virtual bool IsActiveCore
		{
			get { return RatingHeader != null && RatingHeader.IsCosting(); }
		}

		#endregion
	}
}

