using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	[CodeAlive("Required for the creation of pattern table")]
	public class PatternMatchingRegCode : AutoPatternMatchingRegCode, IPatternMatchingBusinessObjects
	{
		public PatternMatchingRegCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid OrganisationPK
		{
			get { return PMR_OH; }
			set { PMR_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PMR_PER; }
			set { PMR_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PMR_HashedValue; }
			set { PMR_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PMR_ParentTableCode; }
			set { PMR_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PMR_ParentId; }
			set { PMR_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PMR_RN_NKCountryCode; }
			set { PMR_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PMR_IsActive; }
			set { PMR_IsActive = value; }
		}
#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PMR_ParentTableCode = "GS";
		}
#endif
	}
}
