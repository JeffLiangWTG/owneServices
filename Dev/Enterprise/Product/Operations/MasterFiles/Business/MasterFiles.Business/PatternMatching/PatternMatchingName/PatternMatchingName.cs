using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingName : AutoPatternMatchingName, IPatternMatchingBusinessObjects
	{
		public PatternMatchingName(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid OrganisationPK
		{
			get { return PMN_OH; }
			set { PMN_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PMN_PER; }
			set { PMN_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PMN_HashedValue; }
			set { PMN_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PMN_ParentTableCode; }
			set { PMN_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PMN_ParentId; }
			set { PMN_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PMN_RN_NKCountryCode; }
			set { PMN_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PMN_IsActive; }
			set { PMN_IsActive = value; }
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PMN_ParentTableCode = "GS";
		}
#endif
	}
}
