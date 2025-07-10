using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RefZonePivot : AutoRefZonePivot, IRefZonePivot
	{
		public RefZonePivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Location

		public ILocation Location
		{
			get
			{
				switch (F2_ParentTableCode)
				{
					case RefUNLOCOSchema.Constants.Prefix:
						return Factory.Load<RefUNLOCO>(F2_ParentID);

					case RefCountrySchema.Constants.Prefix:
						return Factory.Load<RefCountry>(F2_ParentID);
				}

				return null;
			}
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind,
			PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			if (F2_ParentTableCode.IsEmpty)
			{
				F2_ParentTableCode = "RN";
			}
		}
#endif
	}
}
