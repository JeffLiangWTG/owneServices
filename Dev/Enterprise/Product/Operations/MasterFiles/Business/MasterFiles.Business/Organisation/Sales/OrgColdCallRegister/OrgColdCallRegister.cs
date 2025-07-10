using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Obsoleted by SalesEnquiry. Do not use.
	/// Exists only because the auto-generated code assumes there is a class with the same name as the table.
	/// </summary>
	[CodeProperty(OrgColdCallRegisterSchema.Constants.O1_LeadUniqueReference)]
	public abstract class OrgColdCallRegister : AutoOrgColdCallRegister
	{
		protected OrgColdCallRegister(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new OrgColdCallRegisterDecider();

		class OrgColdCallRegisterDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(SalesEnquiry);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				return typeof(SalesEnquiry);
			}

			public override Type GetTypeForNew()
			{
				return typeof(SalesEnquiry);
			}
		}
	}
}
