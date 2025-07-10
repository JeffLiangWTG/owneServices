using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolTemplate : AutoConsolTemplate
	{
		public ConsolTemplate(MultiDaysSelection multiDaysSelection)
			: base(multiDaysSelection.Factory)
		{
			this.multiDaysSelection = multiDaysSelection;
			this.multiDaysSelection.OnFlightNumberChanged += MultiDaysSelection_OnFlightNumberChanged;

			ConsolsPerFlight = 1;
			FlightNumber = this.multiDaysSelection.GetFlightNumber();
		}

		#region MultiDaysSelection

		public MultiDaysSelection MultiDaysSelection => multiDaysSelection;
		readonly MultiDaysSelection multiDaysSelection;

		#endregion

		#region FlightNumber

		int FlightNumber { get; set; }

		void MultiDaysSelection_OnFlightNumberChanged(object sender, FlightNumberEventArgs e)
		{
			FlightNumber = e.FlightNumber;
			TotalConsolsInfo.RefreshBinding();
		}

		#endregion

		#region TotalConsols

		public override ZInt TotalConsols => FlightNumber * ConsolsPerFlight;

		#endregion

		#region ConsolTemplateNameExists

		public StmTemplateRecord LoadConsolTemplateByName(ZString consolTemplateName)
		{
			var query = new ZQuery(StmTemplateRecordSchema.STR_TemplateName, consolTemplateName);
			query.AddToFilter(StmTemplateRecordSchema.STR_ModuleID, ModuleIDs.JobConsol.Name);

			return Factory.LoadTop1<StmTemplateRecord>(query);
		}

		public StmTemplateRecord LoadConsolTemplateByReferenceId(ZString consolTemplateReferenceId)
		{
			var query = new ZQuery(StmTemplateRecordSchema.STR_ReferenceId, consolTemplateReferenceId);
			query.AddToFilter(StmTemplateRecordSchema.STR_ModuleID, ModuleIDs.JobConsol.Name);

			return Factory.LoadTop1<StmTemplateRecord>(query);
		}

		#endregion
	}
}
