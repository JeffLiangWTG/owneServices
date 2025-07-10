using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class AccGLHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public AccGLHeaderFetchStrategy(EnterpriseBusinessObject businessObject) : base(businessObject)
		{
			if (!(businessObject is AccGLHeader))
			{
				throw new ArgumentException("Bizo must be AccGLHeader", nameof(businessObject));
			}
		}
	}
}
