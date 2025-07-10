using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.Business
{
	[CodeAlive("Used in later workflows WI00403150")]
	public class CertificateOfOriginLine : AutoCertificateOfOriginLine
	{
		public CertificateOfOriginLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject("CertificateOfOrigin")]
		public override ZGuid COL_COO
		{
			get { return base.COL_COO; }
			set { base.COL_COO = value; }
		}

		public CertificateOfOrigin CertificateOfOrigin => Factory.Load<CertificateOfOrigin>(COL_COO);

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			COL_WeightUQ = "KG";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
