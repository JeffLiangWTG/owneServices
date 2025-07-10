using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
	[CodeAlive("This Business Object is used in Glow.")]
	[DeferTriggerAndRunBeforeCommit(
		"TG_WhsItemCycleCountLocation_PreventCreateIfLocationHasOpenVariance",
		"WhsItemCycleCountLocationCheckIfLocationHasOpenVariance",
		WhsItemCycleCountLocationSchema.Constants.PK,
		typeof(IWhsItemCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy)
	)]
	[CodeProperty(WhsItemCycleCountLocationSchema.Constants.WIC_JobID)]
	public class WhsItemCycleCountLocation : AutoWhsItemCycleCountLocation, IWhsItemCycleCountLocation, IStmNoteParent, INumberFountainConsumer
	{
		public WhsItemCycleCountLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventCreateIfLocationHasOpenVarianceTriggerError = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";

		#region Location

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WIC_WL_Location); }
		}

		[RelatedBusinessObject("Location")]
		public override ZGuid WIC_WL_Location { get => base.WIC_WL_Location; set => base.WIC_WL_Location = value; }

		#endregion

		#region NoteTypes

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.CycleCountNotes);
				return types;
			}
		}

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WIC_JobID;
			set => WIC_JobID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.TransitWarehouseCycleCountID;

		#endregion
	}
}
