using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Freight;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Business
{
	[CodeAlive("Used in later workflows WI00338820")]
	public class JobPackLinePackage : AutoJobPackLinePackage, IJobPackLinePackage
	{
		public JobPackLinePackage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("PackLine")]
		public override ZGuid JPP_JL_PackLine
		{
			get { return base.JPP_JL_PackLine; }
			set { base.JPP_JL_PackLine = value; }
		}
		public PackLine PackLine => Factory.Load<PackLine>(JPP_JL_PackLine);

		[RelatedBusinessObject("PkgPackage")]
		public override ZGuid JPP_KP_Packge
		{
			get { return base.JPP_KP_Packge; }
			set { base.JPP_KP_Packge = value; }
		}

		public PkgPackage PkgPackage => Factory.Load<PkgPackage>(JPP_KP_Packge);

		public override void OnSaving()
		{
			var duplicate = LoadDuplicatePackage();
			if (duplicate?.PkgPackage != null)
			{
				var errorHeader = Res.GetString("fc7eb4b4-c109-4575-a136-c8520dfacb5c", "Unable to Save Duplicate Package");
				var message = Res.GetString("e1fb972f-21db-4661-82b3-d008f4586f52",
					@"A package can only be associated with one packline.
The following package:
Package ID: {0}
Package PK: {1}
Packline ID: {2}
External Reference: {3}
Previous Packline ID: {4}
is already associated with another packline:
Packline ID: {5},
Packline PK: {6}",
					PkgPackage.KP_PackageID,
					JPP_KP_Packge,
					PackLine.JL_PackLineId,
					PkgPackage.KP_ExternalReference,
					PkgPackage.KP_PreviousPackLineID,
					duplicate.PackLine.JL_PackLineId,
					duplicate.JPP_JL_PackLine
					);
				throw new ZCannotSaveException(message, errorHeader);
			}

			base.OnSaving();
		}

		JobPackLinePackage LoadDuplicatePackage()
		{
			var query = new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, JPP_KP_Packge);
			query.AddToFilter(JobPackLinePackageSchema.PK, SQLComparisonOperator.NotEqual, PK);
			var duplicate = Factory.LoadTop1<JobPackLinePackage>(query);
			return duplicate;
		}

		#endregion
	}
}
