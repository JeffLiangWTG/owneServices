using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer
{
	public class CostingValueObjectDataAdapter : RatingValueObjectDataAdapter<Costing>
	{
		#region Helpers

		protected override ZQuery QueryForFindingBusinessObject(Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			var owner = context.FindOrganisation(rateXSD.Owner, null, OrganisationTypes.None);
			var query = base.QueryForFindingBusinessObject(rateXSD, context);

			if (owner != null)
			{
				query.AddToFilter(RatingHeaderSchema.TH_OH, owner.PK);
			}
			else
			{
				query.AddToFilter(RatingHeaderSchema.TH_OH, DBNull.Value);
			}

			return query;
		}

		#endregion

		#region Properties

		protected override ZString ExpectedRateType
		{
			get { return RatingConstants.RatingHeaderTypes.Costing; }
		}

		protected override bool ShouldCheckifOwnerIsSpecified
		{
			get { return false; }
		}

		#endregion

		#region Import

		protected override void ImportRatingHeader(Costing ratingHeader, Xsd.Rate rateXSD, IValueObjectImportContext context)
		{
			base.ImportRatingHeader(ratingHeader, rateXSD, context);
			ImportClientDetails(ratingHeader, rateXSD, context);
		}

		protected override RateEntry ImportRateEntry(Costing ratingHeader, Xsd.RateEntry rateEntryXSD, IValueObjectImportContext context)
		{
			RateEntry entry = base.ImportRateEntry(ratingHeader, rateEntryXSD, context);
			entry.TI_OH_Supplier = ZGuid.Empty;
			return entry;
		}

		#endregion

		#region Export

		protected override void ExportRatingHeader(Costing ratingHeader, Xsd.Rate rateXSD, IValueObjectExportContext context)
		{
			base.ExportRatingHeader(ratingHeader, rateXSD, context);
			ExportClientDetails(ratingHeader, rateXSD, context);
		}

		#endregion

		#region Implementation

		protected override string UpdateBusinessObjectMessage(Costing obj)
		{
			if (obj.IsStandardCostRate())
			{
				return Res.GetString("b7654653-51eb-4d6b-8fae-415ee6681170", "Found {0}, Service Provider is empty. Is it OK to update Standard Costing?", obj.HumanReadableName);
			}
			else
			{
				return base.UpdateBusinessObjectMessage(obj);
			}
		}

		#endregion
	}
}

