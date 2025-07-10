using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusProcedureAttribute : AutoRefCusProcedureAttribute
	{
		public RefCusProcedureAttribute(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Procedure")]
		public override ZGuid ZXB_ZZ6_ProcedureCode
		{
			get => base.ZXB_ZZ6_ProcedureCode;
			set => base.ZXB_ZZ6_ProcedureCode = value;
		}

		public RefCusProcedure Procedure => Factory.Load<RefCusProcedure>(ZXB_ZZ6_ProcedureCode);

		[List("Lookups.AttributeNames")]
		public override ZString ZXB_Name { get => base.ZXB_Name; set => base.ZXB_Name = value; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoRefCusProcedureAttribute.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public static ZString[] LoadAttributesList(BusinessObjectFactory factory, ZString procedureCode, ZString previousProcedureCode, ZString concession, ZString dataGroupingCode, ZString shipmentType, ZString attributeName)
			{
				var result = new List<ZString>();
				var query = new ZQuery(RefCusProcedureSchema.ZZ6_ZZZ_NKDataGrouping, dataGroupingCode);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_ShipmentType, shipmentType);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_ProcedureCode, procedureCode);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_PreviousProcedureCode, previousProcedureCode);
				query.AddToFilter(RefCusProcedureSchema.ZZ6_Concession, concession);
				foreach (var cusProcedure in factory.Load<RefCusProcedure>(query))
				{
					result.AddRange(cusProcedure.Attributes.Where(x => x.ZXB_Name == attributeName).OrderBy(x => x.ZXB_Value).Select(x => x.ZXB_Value));
				}
				return result.ToArray();
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(RefCusProcedureAttribute);
			}
		}
	}
}
