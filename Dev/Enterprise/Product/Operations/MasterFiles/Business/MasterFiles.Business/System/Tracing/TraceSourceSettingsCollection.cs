using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TraceSourceSettingsCollection : NonPersistentBusinessObjectCollection<TraceSourceSettings>
	{
		public static TraceSourceSettingsCollection GetTraceSources()
		{
			var sources = new TraceSourceSettingsCollection();
			sources.AddRange(GetAccountingTraceSources());
			sources.AddRange(GetCoreTraceSources());

			return sources;
		}

		static TraceSourceSettingsCollection GetAccountingTraceSources()
		{
			static TraceSourceSettings AddAccountingSettings(string name, string description) => new()
			{
				TraceSourceCategory = AccountingTraceSourceCodes.CategoryName,
				TraceSourceName = name,
				TraceSourceDescription = description,
				TraceLevel = TraceSourceLevels.Codes.Off,
				TraceFilter_ReadOnly = true
			};

			return new TraceSourceSettingsCollection()
			{
				AddAccountingSettings(AccountingTraceSourceCodes.APA, Res.GetString("TracingList|APA", "AP Automation")),
				AddAccountingSettings(AccountingTraceSourceCodes.CASS, Res.GetString("TracingList|CASS", "CASS file import")),
				AddAccountingSettings(AccountingTraceSourceCodes.ConsolCost, Res.GetString("TracingList|ConsolCost", "Consol Cost")),
				AddAccountingSettings(AccountingTraceSourceCodes.eInvoicing, Res.GetString("TracingList|eInvoicing", "e-Invoicing Eligibility Evaluation")),
				AddAccountingSettings(AccountingTraceSourceCodes.FPOS, Res.GetString("TracingList|FPOS", "Fixed Place Of Supply")),
				AddAccountingSettings(AccountingTraceSourceCodes.Http, Res.GetString("TracingList|HTTP", "HTTP communication")),
				AddAccountingSettings(AccountingTraceSourceCodes.CLC, Res.GetString("TracingList|CLC", "Credit Limit Check Process"))
			};
		}

		static TraceSourceSettingsCollection GetCoreTraceSources()
		{
			var collection = new TraceSourceSettingsCollection();
			collection.Add(new TraceSourceSettings() { TraceSourceCategory = CoreTraceSourceCodes.CategoryName, TraceSourceName = CoreTraceSourceCodes.Registry, TraceSourceDescription = Res.GetString("TracingList|Registry", "Registry Reads"), TraceLevel = TraceSourceLevels.Codes.Off, TraceFilter_ReadOnly = false });
			return collection;
		}

		public ZString[] GetAllCodes() => this.Cast<TraceSourceSettings>().Select(t => t.TraceSourceName).ToArray();

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TraceSourceSettings();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		#endregion
	}
}
