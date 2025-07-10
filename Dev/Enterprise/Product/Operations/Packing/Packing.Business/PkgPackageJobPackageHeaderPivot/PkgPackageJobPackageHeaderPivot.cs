using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Packing.Business
{
	public class PkgPackageJobPackageHeaderPivot : AutoPkgPackageJobPackageHeaderPivot, IDocumentSupportable, IPackageSequence
	{
		public PkgPackageJobPackageHeaderPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Objects

		#region PackageJob

		public PkgPackageJob PackageJob
		{
			get { return Factory.Load<PkgPackageJob>(KPJ_KJ_PackageJob); }
		}

		#endregion

		#region PackageHeader

		public PkgPackageHeader PackageHeader
		{
			get
			{
				PkgPackageHeader packageHeader = null;
				if (KPJ_KPH_PackageHeader.IsEmpty)
				{
					packageHeader = Factory.New<PkgPackageHeader>();
					KPJ_KPH_PackageHeader = packageHeader.PK;
				}
				else
				{
					packageHeader = Factory.Load<PkgPackageHeader>(KPJ_KPH_PackageHeader);
				}

				return packageHeader;
			}
		}

		#endregion

		#endregion

		#region Properties

		#region KPJ_KJ_PackageJob

		[RelatedBusinessObject("PackageJob")]
		public override ZGuid KPJ_KJ_PackageJob
		{
			get { return base.KPJ_KJ_PackageJob; }
			set
			{
				var previousValue = KPJ_KJ_PackageJob;
				base.KPJ_KJ_PackageJob = value;

				if (previousValue != value)
				{
					KPJ_Sequence = 0;
					if (!value.IsEmpty)
					{
						PackageJob?.PackageSequenceCalculator.Sequence(this);
					}
				}
			}
		}

		#endregion

		#region KPJ_KPH_PackageHeader

		[RelatedBusinessObject("PackageHeader")]
		public override ZGuid KPJ_KPH_PackageHeader
		{
			get { return base.KPJ_KPH_PackageHeader; }
			set
			{
				var previousValue = KPJ_KPH_PackageHeader;
				base.KPJ_KPH_PackageHeader = value;

				if (value.IsEmpty || (previousValue != value && KPJ_Sequence == 0))
				{
					PackageJob?.PackageSequenceCalculator.Sequence(this);
				}
			}
		}

		#endregion

		#endregion

		public override void Delete()
		{
			var currentSequence = KPJ_Sequence;
			var packageJob = PackageJob;
			base.Delete();

			packageJob?.PackageSequenceCalculator.ShiftSequence(currentSequence);
		}

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new PkgPackageJobPackageHeaderPivotDocumentSupporter(this); }
		}

		#endregion

		#region IPackageIDSequence Members

		ZShort IPackageSequence.Sequence
		{
			get => KPJ_Sequence;
			set => KPJ_Sequence = value;
		}

		ZGuid IPackageSequence.PackageHeaderFK => KPJ_KPH_PackageHeader;

		ZGuid IPackageSequence.PackageJobFK => KPJ_KJ_PackageJob;

		bool IPackageSequence.RequiresSequencing => true;

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new PkgPackageJobPackageHeaderPivotFetchStrategy(this);
		}

		#endregion
	}
}
