using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public abstract class FullRatingValueObjectDataAdapter<TBusinessObject> : ValueObjectDataAdapter<TBusinessObject, Xsd.Rate>
		where TBusinessObject : RatingHeader
	{
		#region Overrides

		public override XmlSchema Schema
		{
			get { return RatingXmlSchemaDefinitions.Instance.SingleRateSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return RatingXmlSchemaDefinitions.Instance.RatesSchema; }
		}

		public override string RootElementName
		{
			get { return (NoResString)"Rate"; }
		}

		public override string RootCollectionElementName
		{
			get { return (NoResString)"Rates"; }
		}

		protected virtual bool ShouldCheckIfOwnerIsSpecified
		{
			get { return false; }
		}

		#endregion

		#region abstract Properties

		protected abstract ZString ExpectedRateType { get; }

		#endregion

		#region Helpers

		ZString RateFullName(ZString rateType)
		{
			ZString result = ZString.Empty;

			switch (rateType)
			{
				case RatingConstants.RatingHeaderTypes.Quote:
					result = Res.GetString("77786755-2b43-46fd-8924-7c9581be2552", "Quotation");
					break;

				case RatingConstants.RatingHeaderTypes.Costing:
					result = Res.GetString("68674ecd-641c-4dfd-b270-3f6a6062de17", "Costing");
					break;

				case RatingConstants.RatingHeaderTypes.Tariff:
					result = Res.GetString("abdf43ef-6006-47be-bb50-bfcedff7aa11", "Company Tariff");
					break;

				case RatingConstants.RatingHeaderTypes.ClientRate:
					result = Res.GetString("8dcc61e2-14a1-4924-bcff-b3f0444e788e", "Client Rate");
					break;
			}

			return result;
		}

		protected virtual ZQuery QueryForFindingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ZQuery query = new ZQuery(RatingHeaderSchema.TH_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(RatingHeaderSchema.TH_IsCancelled, ZBool.False);
			query.AddToFilter(RatingHeaderSchema.TH_RateType, ExpectedRateType);

			return query;
		}

		protected virtual TBusinessObject[] LoadExistingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			TBusinessObject[] result = Array.Empty<TBusinessObject>();

			ZQuery query = QueryForFindingBusinessObject(rateXSD, context);

			if (ShouldCheckIfOwnerIsSpecified)
			{
				OrgHeader owner = context.Factory.Load<OrgHeader>(new ZGuid(rateXSD.OwnerPK));

				if (owner != null)
				{
					query.AddToFilter(RatingHeaderSchema.TH_OH, owner.PK);
					result = context.Factory.Load<TBusinessObject>(query);
				}
			}
			else
			{
				result = context.Factory.Load<TBusinessObject>(query);
			}

			return result;
		}

		protected override bool ConfirmUpdateOfExistingBusinessObject(TBusinessObject bizObj, INotifications notifications)
		{
			return false;
		}

		protected override void OnUserDeclinedImport(TBusinessObject ratingHeader, Xsd.Rate rate, IValueObjectImportContext context)
		{
			string message = Res.GetString("7f3b151e-7722-49f9-ae0f-1ce04242eeb6", "Import of Rating [({0}) - {1} - {2}] skipped. Reason: Rating for this organization already exists.", ratingHeader.PK.ToString(), ratingHeader.TH_ClientCode, ratingHeader.TH_RateType);
			context.Notify(new InfoNotification(message));
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		protected override TBusinessObject FindBusinessObject(Xsd.Rate value, IValueObjectImportContext context)
		{
			TBusinessObject rateHeader = null;

			TBusinessObject[] candidateRateHeaders = LoadExistingBusinessObject(value, context);

			if (candidateRateHeaders.Length == 1)
			{
				rateHeader = candidateRateHeaders[0];
			}

			return rateHeader;
		}

		protected bool IsOwnerSpecified(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			bool result = true;

			if (rateXSD != null && !rateXSD.Owner.IsSpecified)
			{
				ZString errorMesg = Res.GetString("3382d8bd-0a30-41ee-a21c-025286c0aeae", "Error Importing") + " " + typeof(TBusinessObject).Name + " " + Res.GetString("9687f5e8-5056-4fb0-a9e1-dcc56c2e2cd3", "as Client Info is not provided.");
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, errorMesg));
				result = false;
			}

			return result;
		}

		#endregion

		#region Validation

		protected virtual bool Validate(Xsd.Rate rateXSD, ZString expectedRateType, IValueObjectImportContext context)
		{
			bool result = rateXSD.RateType == ExpectedRateType;

			if (!result)
			{
				var actualRateType = RateFullName(rateXSD.RateType);
				var message = Res.GetString("f73e9fb4-fbed-4032-96c5-f8c92e746eab", "Cannot Import {0} in {1} Module", actualRateType, expectedRateType);
				context.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, message));
			}

			return result;
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			if (ratingHeader != null && rateXSD != null)
			{
				var rateType = RateFullName(ExpectedRateType);
				if (Validate(rateXSD, rateType, context))
				{
					context.Notify(new InfoNotification(Res.GetString("c3d55c9b-d1d4-410f-8db3-579d01c11095", "Importing {0}", rateType)));
					ImportRatingHeaderAndChildObjects(ratingHeader, rateXSD, context);
				}
			}
		}

		public override TBusinessObject CreateOrUpdateFromValueObject(Xsd.Rate value, IValueObjectImportContext context)
		{
			TBusinessObject result = null;

			bool ownerIsSpecified = !ShouldCheckIfOwnerIsSpecified || IsOwnerSpecified(value, context);

			if (ownerIsSpecified)
			{
				result = base.CreateOrUpdateFromValueObject(value, context);
			}

			return result;
		}

		void ImportRatingHeaderAndChildObjects(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ImportRatingHeader(ratingHeader, rateXSD, context);

			foreach (Xsd.RateEntry rateEntryXSD in rateXSD.RateEntries)
			{
				RateEntryValueObjectDataAdapter entryAdapter = new RateEntryValueObjectDataAdapter(ratingHeader);
				entryAdapter.CreateOrUpdateFromValueObject(rateEntryXSD, context);
			}
		}

		protected void ImportClientDetails(TBusinessObject ratingHeader, Xsd.Rate headerXSD, IValueObjectImportContext context)
		{
			if (headerXSD.Owner.IsSpecified)
			{
				OrgHeader orgHeader = ratingHeader.Factory.Load<OrgHeader>(new ZGuid(headerXSD.OwnerPK));

				if (orgHeader == null)
				{
					throw new Exception(string.Format("Could not find this Organisation PK = [{0}]", headerXSD.OwnerPK));
				}
				else
				{
					ratingHeader.TH_OH = orgHeader.PK;
				}
			}
		}

		protected virtual void ImportRatingHeader(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ratingHeader.TH_GC = GlbCompany.CurrentCompany.PK;
			context.SetPropertyInfoValue(ratingHeader.TH_RateTypeInfo, rateXSD.RateType, rateXSD.RateTypeSpecified);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			if (ratingHeader != null && rateXSD != null)
			{
				rateXSD.RateType = ratingHeader.TH_RateType;
				rateXSD.OwnerPK = ratingHeader.TH_OH.ToString();
				ExportRatingHeaderAndChildObjects(ratingHeader, rateXSD, context);
			}
		}

		void ExportRatingHeaderAndChildObjects(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			ExportRatingHeader(ratingHeader, rateXSD, context);

			var entryAdapter = new RateEntryValueObjectDataAdapter(ratingHeader);

			foreach (var entry in ratingHeader.AllEntries)
			{
				var rateEntryXSD = rateXSD.RateEntries.AddNew();
				entryAdapter.ExportToValueObject(entry, rateEntryXSD, context);
			}
		}

		protected void ExportClientDetails(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			if (ratingHeader.Header != null)
			{
				rateXSD.Owner = new OrganisationValueObjectDataAdapter().ExportToValueObject(ratingHeader.Header, context);
				rateXSD.Owner.IsSpecified = true;
			}
		}

		protected virtual void ExportRatingHeader(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			rateXSD.RateType = ratingHeader.TH_RateType;
		}

		#endregion
	}
}
