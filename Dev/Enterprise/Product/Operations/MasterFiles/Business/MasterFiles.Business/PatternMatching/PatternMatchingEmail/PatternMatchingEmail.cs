using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingEmail : AutoPatternMatchingEmail, IPatternMatchingBusinessObjects
	{
		public PatternMatchingEmail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid OrganisationPK
		{
			get { return PME_OH; }
			set { PME_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PME_PER; }
			set { PME_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PME_HashedValue; }
			set { PME_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PME_ParentTableCode; }
			set { PME_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PME_ParentId; }
			set { PME_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PME_RN_NKCountryCode; }
			set { PME_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PME_IsActive; }
			set { PME_IsActive = value; }
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PME_ParentTableCode = "GS";
		}
#endif
	}
}
