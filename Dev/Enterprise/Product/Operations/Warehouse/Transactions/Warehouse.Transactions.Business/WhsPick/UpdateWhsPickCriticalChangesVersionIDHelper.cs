using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	static class UpdateWhsPickCriticalChangesVersionIDHelper
	{
		#region UpdatePickVersionAndSubscribeToFactory

		public static void UpdatePickVersionAndSubscribeToFactory(BusinessObjectFactory factory, ZGuid pickPK)
		{
			if (!pickPK.IsValid)
			{
				throw new ArgumentException("Pick PK should be valid.");
			}

			GetUpdatePickVersionService(factory).RegisterBusinessObjectPK(pickPK);
		}

		public static void UpdatePickVersionAndSubscribeToFactory(BusinessObjectFactory factory, WhsPickLine pickLine)
		{
			if (pickLine == null)
			{
				throw new ArgumentNullException(nameof(pickLine));
			}

			GetUpdatePickVersionService(factory).ByPickLine(pickLine);
		}

		public static void UpdatePickVersionAndSubscribeToFactory(BusinessObjectFactory factory, WhsPickableDocket pickableDocket)
		{
			if (pickableDocket == null)
			{
				throw new ArgumentNullException(nameof(pickableDocket));
			}

			GetUpdatePickVersionService(factory).ByPickableDocket(pickableDocket);
		}

		public static void UpdatePickVersionAndSubscribeToFactory(BusinessObjectFactory factory, WhsPickableDocketLine pickableDocketLine)
		{
			if (pickableDocketLine == null)
			{
				throw new ArgumentNullException(nameof(pickableDocketLine));
			}

			GetUpdatePickVersionService(factory).ByPickableDocketLine(pickableDocketLine);
		}

		#endregion

		#region GetUpdatePickVersionService

		static UpdateWhsPickCriticalChangesVersionIDServiceProvider GetUpdatePickVersionService(BusinessObjectFactory factory)
		{
			if (factory == null)
			{
				throw new ArgumentNullException(nameof(factory));
			}

			var updatePickVersionService = factory.ServiceContainer.GetAfterOnSavingService<UpdateWhsPickCriticalChangesVersionIDServiceProvider>();
			if (updatePickVersionService == null)
			{
				updatePickVersionService = new UpdateWhsPickCriticalChangesVersionIDServiceProvider(factory);
				factory.ServiceContainer.AddAfterOnSavingService(updatePickVersionService);
			}

			return updatePickVersionService;
		}

		#endregion

		#region UpdateCriticalChangesVersionIDServiceProvider

		public class UpdateWhsPickCriticalChangesVersionIDServiceProvider : UpdateCriticalChangesVersionIDServiceProvider<WhsPick>
		{
			public UpdateWhsPickCriticalChangesVersionIDServiceProvider(BusinessObjectFactory factory)
				: base(factory)
			{
				ProcessedPKs = new HashSet<ZGuid>();
			}

			#region ByPickableDocket

			internal void ByPickableDocket(WhsPickableDocket pickableDocket)
			{
				if (ProcessedPKs.Add(pickableDocket.PK))
				{
					if (!pickableDocket.WD_WP.IsEmpty)
					{
						RegisterBusinessObjectPK(pickableDocket.WD_WP);
					}

					var originalWD_WP = pickableDocket.WD_WPInfo.OriginalValue;
					if (!originalWD_WP.IsEmpty)
					{
						RegisterBusinessObjectPK((ZGuid)originalWD_WP);
					}
				}
			}

			#endregion

			#region ByPickableDocketLine

			internal void ByPickableDocketLine(WhsPickableDocketLine pickableDocketLine)
			{
				if (ProcessedPKs.Add(pickableDocketLine.WE_WD))
				{
					var pickableDocket = pickableDocketLine?.PickableDocket;
					if (pickableDocket != null && !pickableDocket.WD_WP.IsEmpty)
					{
						RegisterBusinessObjectPK(pickableDocket.WD_WP);
					}
				}
			}

			#endregion

			#region ByPickLine

			internal void ByPickLine(WhsPickLine pickLine)
			{
				if (ProcessedPKs.Add(pickLine.PK) && ProcessedPKs.Add(pickLine.WZ_WE_TransactionLine))
				{
					var pickableDocketLine = pickLine?.DocketLine as WhsPickableDocketLine;
					if (pickableDocketLine != null)
					{
						ByPickableDocketLine(pickableDocketLine);
					}
				}
			}

			#endregion

			#region ClearPKsCore

			protected override void ClearPKsCore()
			{
				ProcessedPKs.Clear();
			}

			#endregion

			readonly HashSet<ZGuid> ProcessedPKs;
		}

		#endregion

	}
}
