using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingDomain : AutoPatternMatchingDomain, IPatternMatchingBusinessObjects
	{
		public PatternMatchingDomain(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public ZGuid OrganisationPK
		{
			get { return PMD_OH; }
			set { PMD_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PMD_PER; }
			set { PMD_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PMD_HashedValue; }
			set { PMD_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PMD_ParentTableCode; }
			set { PMD_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PMD_ParentId; }
			set { PMD_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PMD_RN_NKCountryCode; }
			set { PMD_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PMD_IsActive; }
			set { PMD_IsActive = value; }
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PMD_ParentTableCode = "HA";
		}
#endif
	}
}
