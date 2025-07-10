using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterData.Business
{
	[DescriptionProperty(Schema.SHG_Name)]
	public class GenShapeGeography : AutoGenShapeGeography, IGenShapeGeography
	{
		public GenShapeGeography(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => ResString.GetMultilingualString("27627B72-0039-4A3A-819A-AF7064E5F5C9", "Shape Geography");

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected bool SHG_Name_ReadOnly => SHG_IsSystem;

		protected bool SHG_Description_ReadOnly => SHG_IsSystem;

		[List(nameof(Lookups) + "." + nameof(GenShapeGeographyLookups.SHG_ParentTableCode_List))]
		public override ZString SHG_ParentTableCode
		{
			get => base.SHG_ParentTableCode;
			set
			{
				var valueIsSame = value == SHG_ParentTableCode;

				base.SHG_ParentTableCode = value;

				if (value.IsEmpty || !valueIsSame)
				{
					SHG_ParentID = ZGuid.Empty;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(GenShapeGeographyLookups.SHG_ParentID_List))]
		public override ZGuid SHG_ParentID
		{
			get => base.SHG_ParentID;
			set => base.SHG_ParentID = value;
		}

		protected bool SHG_ParentID_ReadOnly => SHG_ParentTableCode.IsEmpty || !Lookups.SHG_ParentTableCode_List.ContainsCode(SHG_ParentTableCode);

		public ZString ShapeInformation
		{
			get
			{
				if (shapeInformation.IsEmpty)
				{
					if (SHG_Shape.IsEmpty || !SHG_Shape.IsValid)
					{
						shapeInformation = Res.GetString("F82D8CEA-E297-48CE-A2EB-DA39F3250EB3", "Invalid");
					}
					else
					{
						shapeInformation = Res.GetString("F0443668-27B8-4829-B4F8-DFE4A2F63C00", "{0}, {1} point(s)", SHG_Shape.SpatialTypeName, SHG_Shape.STNumPoints());
					}
				}

				return shapeInformation;
			}
		}

		ZString shapeInformation;

		public override ZGeography SHG_Shape
		{
			get { return base.SHG_Shape; }
			set
			{
				shapeInformation = ZString.Empty;
				base.SHG_Shape = value;
			}
		}

		[List(nameof(Lookups) + "." + nameof(GenShapeGeographyLookups.GeographyTypeList))]
		public override ZString SHG_Type => base.SHG_Type;

		protected bool SHG_Type_ReadOnly => SHG_IsSystem;
	}
}
