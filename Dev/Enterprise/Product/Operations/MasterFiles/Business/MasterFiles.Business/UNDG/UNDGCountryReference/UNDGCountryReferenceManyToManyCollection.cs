using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ZArchitecture.Modules.ModuleId.UNDGCountryReference)]
	public class UNDGCountryReferenceManyToManyCollection<T> : ManyToManyBusinessObjectCollection<UNDGCountryReference, T> where T : BusinessObject, IDGSubstance
	{
		public UNDGCountryReferenceManyToManyCollection(T associatedObject)
				: base(associatedObject)
		{
		}

		#region Collection Functionality

		protected override Type TypeOfRelationshipBusinessObject => typeof(UNDGCountryReferencePivot);

		protected override SchemaGuidColumn PivotTableFKToAssociatedBusinessObject => new SchemaGuidColumn(UNDGCountryReferencePivotSchema.Instance, nameof(UNDGCountryReferencePivot.DCP_DG_Virtual), 0, ZGuid.Empty, false);

		protected override void InitialiseFetchHints()
		{
		}

		protected override bool AllowNewCore => false;

		protected override BusinessObject AddNewCore() => throw new NotSupportedException("Allow new is false so shouldn't get called");

		protected override ZQuery GetAssociatedBusinessObjectZQuery()
		{
			var substance = (IDGSubstance)fAssociatedObject;
			return new ZQuery()
				.AddToFilter(UNDGCountryReferencePivotSchema.DCP_UNNO, substance.UNNO)
				.AddToFilter(UNDGCountryReferencePivotSchema.DCP_Standard, substance.Standard)
				.AddToFilter(UNDGCountryReferencePivotSchema.DCP_Variant, substance.Variant);
		}

		protected override void OnAdded(BusinessObject addedBusinessObject)
		{
			base.OnAdded(addedBusinessObject);

			if (!IsLoading &&
				addedBusinessObject is UNDGCountryReference countryReference &&
				fAssociatedObject is IStmALogParent logParent)
			{
				CreateEventForAttachingOrDetaching(AutoEvents.Attached, logParent, countryReference);
			}
		}

		protected override void OnRemoved(BusinessObject removedBusinessObject)
		{
			base.OnRemoved(removedBusinessObject);

			if (!removedBusinessObject.IsDeleted &&
				!fAssociatedObject.IsDeleted &&
				removedBusinessObject is UNDGCountryReference countryReference &&
				fAssociatedObject is IStmALogParent logParent)
			{
				if (TryFindAndRemoveAnyAttachLogsInTheSameTransaction(logParent, countryReference))
				{
					return;
				}
				else
				{
					CreateEventForAttachingOrDetaching(AutoEvents.Detached, logParent, countryReference);
				}
			}
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);
			var countryReference = (UNDGCountryReference)businessObject;
			countryReference.SubstancePivot = (UNDGCountryReferencePivot)GetRelationshipBusinessObject(countryReference);
		}

		bool TryFindAndRemoveAnyAttachLogsInTheSameTransaction(IStmALogParent logParent, UNDGCountryReference reference)
		{
			var logReference = GenerateLogReference(reference);

			var attachLogQuery = new ZQuery();
			attachLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AttachedCode);
			attachLogQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, logReference);

			var uncommitedAttachLogs = logParent
				.Logs
				.Find(attachLogQuery)?
				.Where(log => !log.IsInDatabase) ?? Enumerable.Empty<StmALog>();

			foreach (var stmALog in uncommitedAttachLogs)
			{
				stmALog.Delete();
			}

			return uncommitedAttachLogs.Any();
		}

		void CreateEventForAttachingOrDetaching(Event eventType, IStmALogParent logParent, UNDGCountryReference reference)
		{
			if (fAssociatedObject is IDGSubstance substance)
			{
				var typeParameter = GenerateTypeParameter(substance);
				var logReference = GenerateLogReference(reference);
				var parameters = new Dictionary<string, string>()
				{
					[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = typeParameter
				};

				logParent.Logs.CreateOrRecreateEventLog(eventType, EstimateActual.Actual, ZDateTimeOffset.UtcNow, logReference, parameters.ToArray());
			}
		}

		string GenerateLogReference(UNDGCountryReference reference)
		{
			var referencePrefix = UNDGCountryReferenceLookups.Types.GetDescriptionFromCode(reference.DCR_Type);
			return string.Concat(referencePrefix, " ", reference.DCR_Code);
		}

		string GenerateTypeParameter(IDGSubstance substance)
		{
			var variant = substance.Variant.IsEmpty
				? ""
				: string.Concat("(", substance.Variant, ")");

			return string.Join(" ",
				substance.Standard,
				substance.UNNO,
				variant);
		}

		#endregion
	}
}
