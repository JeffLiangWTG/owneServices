using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterData.Common;

namespace Enterprise.MasterFiles.Business
{
	public class PatternMatchingAddress : AutoPatternMatchingAddress, IPatternMatchingBusinessObjects
	{
		public PatternMatchingAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZGuid OrganisationPK
		{
			get { return PMA_OH; }
			set { PMA_OH = value; }
		}

		public ZGuid PersonPK
		{
			get { return PMA_PER; }
			set { PMA_PER = value; }
		}

		public ZInt HashedValue
		{
			get { return PMA_HashedValue; }
			set { PMA_HashedValue = value; }
		}

		public ZString ParentTableCode
		{
			get { return PMA_ParentTableCode; }
			set { PMA_ParentTableCode = value; }
		}

		public ZGuid ParentId
		{
			get { return PMA_ParentId; }
			set { PMA_ParentId = value; }
		}

		public ZString PatternMatchingCountryCode
		{
			get { return PMA_RN_NKCountryCode; }
			set { PMA_RN_NKCountryCode = value; }
		}

		public ZBool IsActive
		{
			get { return PMA_IsActive; }
			set { PMA_IsActive = value; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			this.PMA_ParentTableCode = "GS";
		}
#endif
	}
}
