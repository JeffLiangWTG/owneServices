using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Document.Common;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transit.Document
{
	public abstract class CIN750NotificationBuilder<TSource, TDocDataObject> where TSource : EnterpriseBusinessObject, IConsignment where TDocDataObject : CIN750Notification
	{
		public CIN750NotificationBuilder(TSource sourceBO)
		{
			this.sourceBO = Argument.NotNull(sourceBO, nameof(sourceBO));
			this.context = new TransitCommonContext(sourceBO.Factory.GetCachedReadOnlyFactory());
		}

		protected readonly TSource sourceBO;
		protected readonly IContext context;

		public TDocDataObject Build()
		{
			var cinNotification = GetDocDataObject();

			cinNotification.SourceBusinessObject = sourceBO;
			cinNotification.EnterpriseAndServerCode = TransitDocumentHelper.GetEnterpiseAndServerCode();
			cinNotification.JobID = sourceBO.JobID;

			var (refType, refCode) = GetRefTypeAndRefCode();

			cinNotification.RefType = new CodeDescription(new CIN750RefTypes())
			{
				Code = refType
			};
			cinNotification.RefCode = refCode;

			cinNotification.MovementTime = GetMovementTime();

			var declaredInWarehouseAddress = GetDeclaredInWarehouse();
			cinNotification.DeclaredInWarehouse = AddressBuilder.Create(context, declaredInWarehouseAddress);
			cinNotification.DeclaredInWarehouseCINNumber = declaredInWarehouseAddress?.GetRegistrationNumberWithFallback(context, OrgCusCode.FranceCodeTypes.CIN);
			cinNotification.DeclaredInWarehouseCIN = cinNotification.DeclaredInWarehouseCINNumber?.Value ?? ZString.Empty;
			if (cinNotification.DeclaredInWarehouseCIN.IsEmpty)
			{
				cinNotification.DeclaredInWarehouseCIN = TransitDocDataConstants.NOTCIN;
			}

			PopulateProperties(cinNotification);

			PopulatePackingLines(cinNotification);

			AddValidations(cinNotification);

			cinNotification.ValidateAllIncludingChildren();

			return cinNotification;
		}

		protected abstract (ZString RefType, ZString RefCode) GetRefTypeAndRefCode();

		protected abstract TDocDataObject GetDocDataObject();

		protected virtual ZDateTime GetMovementTime() => ZDateTime.UtcNow;

		protected abstract OrgAddress GetDeclaredInWarehouse();

		protected virtual void PopulateProperties(TDocDataObject cinNotification) { }

		protected virtual void PopulatePackingLines(TDocDataObject cinNotification)
		{
			cinNotification.Goods = new List<DocPackingLine>();
		}

		void AddValidations(TDocDataObject cinNotification)
		{
			cinNotification.RefCodeInfo.AddMessageErrorIfEmpty(Res.GetString("8e5ea630-a1a5-4e32-b5bb-0bbb474c18b0", "Ref Code is required."));
			cinNotification.MovementTimeInfo.AddMessageErrorIfEmpty(Res.GetString("8ddba1ae-2407-4fe3-8e02-4bb60a69703d", "Movement Time is required."));
			cinNotification.DeclaredInWarehouseCINInfo.AddMessageErrorIfEmpty(Res.GetString("12f3bc03-f109-47f0-89ce-78596afd5265", "Declared In Warehouse CIN is required."));

			AddExtraValidations(cinNotification);
		}

		protected virtual void AddExtraValidations(TDocDataObject cinNotification)
		{
		}
	}
}
