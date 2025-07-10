using System;
using System.Collections.Generic;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public abstract class RatingValueObjectDataAdapter<TBusinessObject> : ValueObjectDataAdapter<TBusinessObject, Xsd.Rate>
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

		protected override bool AllowDifferentImportContextFactory
		{
			get { return true; }
		}

		#endregion

		#region abstract Properties

		protected abstract ZString ExpectedRateType { get; }

		protected abstract bool ShouldCheckifOwnerIsSpecified { get; }

		#endregion

		#region Helpers

		ZString RateFullName(ZString rateType)
		{
			ZString result = ZString.Empty;

			switch (rateType)
			{
				case RatingConstants.RatingHeaderTypes.Quote:
					result = Res.GetString("9fd6977f-de4d-4c59-95ba-649b890a345d", "Quotation");
					break;

				case RatingConstants.RatingHeaderTypes.Costing:
					result = Res.GetString("7fb8992c-a2d1-4a0e-ad93-327b163c4843", "Costing");
					break;

				case RatingConstants.RatingHeaderTypes.Tariff:
					result = Res.GetString("f48cf323-157e-4f79-8659-f554710103bb", "Company Tariff");
					break;

				case RatingConstants.RatingHeaderTypes.ClientRate:
					result = Res.GetString("b27b92f3-3327-42a7-b1f0-0fd9e7e9a4a2", "Client Rate");
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
			ZQuery query = QueryForFindingBusinessObject(rateXSD, context);

			if (ShouldCheckifOwnerIsSpecified)
			{
				var owner = context.FindOrganisation(rateXSD.Owner, null, OrganisationTypes.None);
				query.AddToFilter(RatingHeaderSchema.TH_OH, owner != null ? owner.PK : ZGuid.Empty);
			}

			var result = context.Factory.Load<TBusinessObject>(query);

			return result;
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
				ZString errorMesg = Res.GetString("ff04b550-5590-4d43-849e-f2813134a136", "Error Importing") + " " + typeof(TBusinessObject).Name + " " + Res.GetString("2c7affb0-5903-4eea-9295-f8e6b9652fd5", "as Client Info is not provided.");
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
					context.Notify(new InfoNotification(Res.GetString("ffa76af4-5b7b-423a-aaef-439c83ba93d6", "Importing {0}", rateType)));
					ImportRatingHeaderAndChildObjects(ratingHeader, rateXSD, context);
				}
			}
		}

		public override TBusinessObject CreateOrUpdateFromValueObject(Xsd.Rate value, IValueObjectImportContext context)
		{
			TBusinessObject result = null;

			bool ownerIsSpecified = !ShouldCheckifOwnerIsSpecified || IsOwnerSpecified(value, context);

			if (ownerIsSpecified)
			{
				result = base.CreateOrUpdateFromValueObject(value, context);
			}

			return result;
		}

		List<RateEntry> createdEntries;

		void ImportRatingHeaderAndChildObjects(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			createdEntries = new List<RateEntry>();

			ImportRatingHeader(ratingHeader, rateXSD, context);

			try
			{
				ratingHeader.EntryLineOrderSuspended = true;
				ratingHeader.Factory.SuspendValidation();

				if (rateXSD.RateEntries.Count > 0)
				{
					context.Notify(new InfoNotification(Res.GetString("d861a53a-c8ee-4113-ae94-afd82a4d7e1c", "Importing Rate Entries")));
				}

				int batchSize = BatchSize; // Access virtual property getter only once

				for (int i = 0; i < rateXSD.RateEntries.Count; i++)
				{
					if (!ratingHeader.OnProgressChanged(i * 100 / rateXSD.RateEntries.Count, Res.GetString("b9108374-7f8d-48c0-930a-3371b0b1fe9f", "Importing ({0} of {1})...", i + 1, rateXSD.RateEntries.Count)))
					{
						break;
					}

					RateEntry entry = ImportRateEntry(ratingHeader, rateXSD.RateEntries[i], context);
					createdEntries.Add(entry);

					if (context.NotificationsHasErrors)
					{
						break;
					}

					if ((i + 1) % batchSize == 0)
					{
						context.Notify(new InfoNotification(Res.GetString("3d71fd12-268b-4e6d-a570-b37ddcc1f401", "Saving Batch")));
						context.FactoryProvider.SaveCurrentAndUpdateRecordCounts();
						context.Notify(new InfoNotification(Res.GetString("a06f4e91-8abb-44d0-9290-e3b161abe320", "Batch Saved")));
					}
				}
			}
			finally
			{
				ratingHeader.EntryLineOrderSuspended = false;
				ratingHeader.Factory.ResumeValidation();
				((IXMLImportOrgCache)ratingHeader).CachedOrgsForXMLImport.Clear();

				if (createdEntries.Count > 0)
				{
					if (!context.NotificationsHasErrors)
					{
						context.Notify(new InfoNotification(Res.GetString("d861a53a-c8ee-4113-ae94-afd82a4d7e2d", "Validating Updated Data")));

						foreach (RateEntry createdEntry in createdEntries)
						{
							createdEntry.SuspendSettingRateLineTariff = false;
							foreach (RateLine line in createdEntry.RateLines)
							{
								line.Validation.ValidateAll();
							}
							createdEntry.Validation.ValidateAll();
						}
					}

					createdEntries.Clear();
				}
			}
		}

		protected virtual int BatchSize
		{
			get { return 50; }
		}

		protected void ImportClientDetails(TBusinessObject ratingHeader, Xsd.Rate headerXSD, IValueObjectImportContext context)
		{
			if (headerXSD.Owner.IsSpecified)
			{
				ratingHeader.TH_OH = context.FindOrCreateTempOrganisationPK(headerXSD.Owner, ratingHeader, OrganisationTypes.None);
				AddClientOrganisationToCache(ratingHeader, headerXSD);
			}
		}

		void AddClientOrganisationToCache(TBusinessObject ratingHeader, Xsd.Rate headerXSD)
		{
			IXMLImportOrgCache importOrgCache = ratingHeader;
			ZString key = headerXSD.Owner.EDICode + headerXSD.Owner.OwnerCode;
			if (importOrgCache != null && !key.IsEmpty && ratingHeader.Header != null && !importOrgCache.CachedOrgsForXMLImport.ContainsKey(key))
			{
				importOrgCache.CachedOrgsForXMLImport[key] = ratingHeader.Header;
			}
		}

		protected virtual void ImportRatingHeader(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			ratingHeader.TH_GC = GlbCompany.CurrentCompany.PK;
			context.SetPropertyInfoValue(ratingHeader.TH_RateTypeInfo, rateXSD.RateType, rateXSD.RateTypeSpecified);
		}

		protected virtual RateEntry ImportRateEntry(TBusinessObject ratingHeader, Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			RateEntryValueObjectDataAdapter entryAdapter = new RateEntryValueObjectDataAdapter(ratingHeader);
			return entryAdapter.CreateOrUpdateFromValueObject(rateEntryXSD, context);
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			if (ratingHeader != null && rateXSD != null)
			{
				rateXSD.RateType = ratingHeader.TH_RateType;
				ExportRatingHeaderAndChildObjects(ratingHeader, rateXSD, context);
			}
		}

		void ExportRatingHeaderAndChildObjects(TBusinessObject ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			ExportRatingHeader(ratingHeader, rateXSD, context);

			RateEntryValueObjectDataAdapter entryAdapter = new RateEntryValueObjectDataAdapter(ratingHeader);

			const int MaxRateLinesPerFactory = 5000;

			BusinessObjectFactoryProvider factoryProvider = new BusinessObjectFactoryProvider(ratingHeader.IsInDatabase ? null : ratingHeader.Factory);
			factoryProvider.Current.RefreshEnabled = false;
			int rateLinesInFactory = 0;
			int entriesPerBatch = BatchSize;
			List<ZGuid> pkBatch = new List<ZGuid>(entriesPerBatch);
			foreach (RatingHeader.LazyEntryCollection lazyCollection in ratingHeader.EntryCollectionsExcludingSummary.Values)
			{
				pkBatch.Clear();
				List<Guid> entryPks = lazyCollection.GetEntryPks();
				for (int i = 0; i < entryPks.Count; ++i)
				{
					pkBatch.Add(entryPks[i]);

					if (pkBatch.Count == entriesPerBatch || i == entryPks.Count - 1)
					{
						ZQuery query = new ZQuery(RateEntrySchema.PK, pkBatch);
						var entries = factoryProvider.Current.Load(ratingHeader.RateEntryType, query);
						pkBatch.Clear();

						foreach (RateEntry entry in entries)
						{
							rateLinesInFactory += entry.RateLines.Count;
							Xsd.RateEntry rateEntryXSD = rateXSD.RateEntries.AddNew();
							entryAdapter.ExportToValueObject(entry, rateEntryXSD, context);
							PostProcessExportedRateEntry(ratingHeader, entry, rateEntryXSD, context);
						}

						if (rateLinesInFactory >= MaxRateLinesPerFactory && ratingHeader.IsInDatabase)
						{
							rateLinesInFactory = 0;
							factoryProvider.CreateNewAndReclaimMemoryWithoutSave();
						}
					}
				}
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

		protected virtual void PostProcessExportedRateEntry(TBusinessObject ratingHeader, RateEntry entry, Xsd.RateEntry rateEntryXSD, IValueObjectExportContext context)
		{
		}

		#endregion
	}
}

