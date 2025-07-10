using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class PPQForm368Data : AutoPPQForm368Data
	{
		public PPQForm368Data(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public JobDeclaration Declaration
		{
			get { return Parent; }
		}

		public new JobDeclaration Parent
		{
			get
			{
				if (B7_Type != CusAddInfoTypeAttribute.Codes.USPPQForm368Data)
				{
					fParent = null;
				}
				else
				{
					if (fParent == null || fParent.PK != B7_ParentID)
					{
						fParent = Factory.Load<JobDeclaration>(B7_ParentID);
					}
				}
				return fParent;
			}
		}
		JobDeclaration fParent;

		[BusinessObjectTestExclude]
		public override ZString B7_ParentTableCode
		{
			get { return base.B7_ParentTableCode; }
			set
			{
				if (value != JobDeclarationSchema.Constants.Prefix)
				{
					throw new NotSupportedException("Setting PPQForm368Data.B7_ParentTableCode is not supported.");
				}
				base.B7_ParentTableCode = value;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			base.B7_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			B7_Type = CusAddInfoTypeAttribute.Codes.USPPQForm368Data;
		}
	}
}
