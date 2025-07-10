using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingPhone : AutoPatternMatchingPhone, IPatternMatchingBusinessObjects
	{
		public PatternMatchingPhone(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid OrganisationPK
		{
			get { return PMP_OH; }
			set { PMP_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PMP_PER; }
			set { PMP_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PMP_HashedValue; }
			set { PMP_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PMP_ParentTableCode; }
			set { PMP_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PMP_ParentId; }
			set { PMP_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PMP_RN_NKCountryCode; }
			set { PMP_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PMP_IsActive; }
			set { PMP_IsActive = value; }
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PMP_ParentTableCode = "GS";
		}
#endif
	}
}
