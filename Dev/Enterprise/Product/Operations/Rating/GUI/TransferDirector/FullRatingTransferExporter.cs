using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.DataTransfer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.GUI
{
	public class FullRatingTransferExporter : XmlDataTransferExporter
	{
		public FullRatingTransferExporter()
			: base(new FullClientRatesValueObjectDataAdapter(), false)
		{
		}

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			if (selectedElements.Count < 1)
			{
				return;
			}

			var ratingHeaders = new List<RatingHeader>();

			//Only Export Non_Empty Ratings
			foreach (OrgHeader organisation in selectedElements)
			{
				var thisOrgRatingHeaders = organisation.Factory.Load<RatingHeader>(new ZQuery(RatingHeaderSchema.TH_OH, organisation.PK));

				if (thisOrgRatingHeaders == null)
				{
					continue;
				}
				//only export ClientRate
				foreach (RatingHeader ratingHeader in thisOrgRatingHeaders)
				{
					if (ratingHeader.IsClientRate())
					{
						ratingHeaders.Add(ratingHeader);
					}
				}
			}
			base.PromptUserAndExportCore(ratingHeaders.ToArray());
		}
	}
}
