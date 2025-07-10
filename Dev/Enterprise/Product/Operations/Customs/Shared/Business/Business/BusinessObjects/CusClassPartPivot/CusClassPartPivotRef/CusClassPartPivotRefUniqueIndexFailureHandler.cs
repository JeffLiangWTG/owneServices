using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusClassPartPivotRefUniqueIndexFailureHandler : IUniqueIndexFailureHandler
	{
		readonly CusClassPartPivotRef pivotRef;

		public CusClassPartPivotRefUniqueIndexFailureHandler(CusClassPartPivotRef pivotRef)
		{
			this.pivotRef = Argument.NotNull(pivotRef, nameof(pivotRef));
		}

		public IEnumerable<string> HandledUniqueIndexNames
		{
			get
			{
				yield return CusClassPartPivotRefSchema.Constants.Indexes.FK_UC__CIR_CI_CIR_ReferenceType_CIR_ReferenceNumber;
			}
		}

		public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
		{
			var existingItem = GetExistingItem();
			if (existingItem != null)
			{
				var message = GenerateMessage(existingItem);
				var pivot = pivotRef.CusClassPartPivot;
				var referenceType = pivotRef.CIR_ReferenceType;
				pivotRef.Delete();
				Reload(pivot, referenceType);
				notifier.ReportInformation(message, Res.GetString("7693e205-9d6d-4084-89ed-e4a75fd6c714", "Save Error"));
			}
		}

		static void Reload(BaseCusClassPartPivot pivot, ZString referenceType)
		{
			if (pivot != null)
			{
				if (pivot.IsCusClassPartPivotRefsLoaded)
				{
					pivot.CusClassPartPivotRefs.Reload(true);
				}

				if (pivot is ICusClassPartPivotRefTypeSupporter p)
				{
					p.ReloadCollection(referenceType);
				}
			}
		}

		string GenerateMessage(CusClassPartPivotRef existingItem)
		{
			return Res.GetString("c413794b-aa27-45b4-a4ce-bc3c0938d5c1",
				"{0} ({1}) has already been entered on Product ({2}) by another user ({3}). Your changes have been merged, please review your changes and save again.",
				pivotRef.HumanReadableName,
				pivotRef.CIR_ReferenceNumber,
				pivotRef.CusClassPartPivot?.Part?.OP_PartNum ?? ZString.Empty,
				GetLastEditUserAndTime(existingItem));
		}

		static string GetLastEditUserAndTime(CusClassPartPivotRef existingItem)
		{
			var part = existingItem?.CusClassPartPivot?.Part;
			return part?.OP_SystemLastEditUser + " @ " + part?.OP_SystemLastEditTimeUtc;
		}

		CusClassPartPivotRef GetExistingItem()
		{
			var query = new ZDBOnlyQuery(typeof(CusClassPartPivotRef));
			query.AddToFilter(CusClassPartPivotRefSchema.PK, SQLComparisonOperator.NotEqual, pivotRef.PK);
			query.AddToFilter(CusClassPartPivotRefSchema.CIR_CI, pivotRef.CIR_CI);
			query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceType, pivotRef.CIR_ReferenceType);
			query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceNumber, pivotRef.CIR_ReferenceNumber);

			return pivotRef.Factory.LoadTop1<CusClassPartPivotRef>(query);
		}
	}
}
