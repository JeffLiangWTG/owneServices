using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Forwarding.Business
{
	public class CertificateOfOrigin : AutoCertificateOfOrigin
	{
		public CertificateOfOrigin(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			COO_ParentTableCode = "JS";
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
	}
}
