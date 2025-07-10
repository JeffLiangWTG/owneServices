using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(Schema.STR_ReferenceId), DescriptionProperty(nameof(Description))]
	public class StmTemplateRecord : AutoStmTemplateRecord, ITemplateRecord
	{
		public StmTemplateRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public ZString Description => string.Join(" ", STR_ModuleID, STR_ReferenceId);

		public void PopulateIdIfNeeded()
		{
			if (STR_ReferenceId.IsEmpty)
			{
				STR_ReferenceId = Env.NumberFountains.TemplateRecordID.GetNextFormatted(Factory);
			}
		}

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (fUniqueIndexFailureHandler == null)
				{
					fUniqueIndexFailureHandler = new TemplateNumberFountainUniqueIndexFailureHandler(this);
				}

				yield return fUniqueIndexFailureHandler;
			}
		}

		IUniqueIndexFailureHandler fUniqueIndexFailureHandler;

		protected class TemplateNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public TemplateNumberFountainUniqueIndexFailureHandler(StmTemplateRecord record)
				: base(StmTemplateRecordSchema.Constants.Indexes.NR_UX__STR_ModuleID_STR_ReferenceId, record)
			{
				this.record = record;
			}
			readonly StmTemplateRecord record;

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.TemplateRecordID; }
			}

			protected override DbCommand CommandToFindMaxValueInDatabase(DbConnection connection)
			{
				//also void out STR_ReferenceId here so it can regenerate - hacky I guess but it works
				record.STR_ReferenceId = ZString.Empty;

				return connection.Command(@"SELECT max(substring(STR_ReferenceId, 3, 10)) FROM dbo.StmTemplateRecord
WHERE isnumeric(substring(STR_ReferenceId, 3, 10)) = 1");
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			PopulateIdIfNeeded();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			using (SuspendSettingHasChanges())
			{
				if (IsCancelled && !IsDeleted)
				{
					SetReadOnlyIncludingChildren(IsCancelled);
				}
			}
		}

		public override string CanReactivate()
		{
			var canReactivate = base.CanReactivate();

			if (string.IsNullOrEmpty(canReactivate) && Validation.TemplateNameIsNotUnique())
			{
				canReactivate = Res.GetString(
					"c8a5c039-e711-4c24-bb71-8900a06eae2a",
					"An active template already exists with this template name. You must deactivate that template before activating this template"
				);
			}

			return canReactivate;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public bool IsForTemplateSearch { get; set; }

		public DummyLogger Logger { get; set; } = new DummyLogger();

		protected override IBusinessObjectStrategy[] GetStrategies()
		{
			var baseStrategies = base.GetStrategies();
			var strategies = new List<IBusinessObjectStrategy>(baseStrategies);
			strategies.Add(new StmTemplateRecordBizoStrategy());
			return strategies.ToArray();
		}

		static readonly Regex SingleLineFeed = new Regex("(?<!\\r)\\n", RegexOptions.Compiled);

		[MaxLength(20)]
		public override ZString STR_TemplateName { get => base.STR_TemplateName; set => base.STR_TemplateName = value; }

		public override ZString STR_Data
		{
			get => SingleLineFeed.Replace(base.STR_Data, System.Environment.NewLine);
			set => base.STR_Data = value;
		}
	}
}
