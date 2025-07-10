using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.SG
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.SGCustomsProcedureCode)]
	public class SGCPCAddInfo : AutoSGCPCAddInfo
	{
		public SGCPCAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public new SGCPC Parent
		{
			get { return (SGCPC)base.Parent; }
			protected set { base.Parent = value; }
		}

		public override ZString SG_CPCCode
		{
			get { return base.SG_CPCCode; }
			set
			{
				base.SG_CPCCode = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC1
		{
			get { return base.SG_PC1; }
			set
			{
				base.SG_PC1 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC2
		{
			get { return base.SG_PC2; }
			set
			{
				base.SG_PC2 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC3
		{
			get { return base.SG_PC3; }
			set
			{
				base.SG_PC3 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC4
		{
			get { return base.SG_PC4; }
			set
			{
				base.SG_PC4 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC5
		{
			get { return base.SG_PC5; }
			set
			{
				base.SG_PC5 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		[ResourceStringData("E03A4A01-FAD4-4CC7-8A5F-52522AB12516", Caption = "Customs Procedure Code", ShortCaption = "CPC")]
		public override ZString SG_PC6
		{
			get { return base.SG_PC6; }
			set
			{
				base.SG_PC6 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC7
		{
			get { return base.SG_PC7; }
			set
			{
				base.SG_PC7 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC8
		{
			get { return base.SG_PC8; }
			set
			{
				base.SG_PC8 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC9
		{
			get { return base.SG_PC9; }
			set
			{
				base.SG_PC9 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC10
		{
			get { return base.SG_PC10; }
			set
			{
				base.SG_PC10 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC11
		{
			get { return base.SG_PC11; }
			set
			{
				base.SG_PC11 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC12
		{
			get { return base.SG_PC12; }
			set
			{
				base.SG_PC12 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC13
		{
			get { return base.SG_PC13; }
			set
			{
				base.SG_PC13 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC14
		{
			get { return base.SG_PC14; }
			set
			{
				base.SG_PC14 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		public override ZString SG_PC15
		{
			get { return base.SG_PC15; }
			set
			{
				base.SG_PC15 = value;
				MarkDeclarationAsNeedingValidation();
			}
		}

		void MarkDeclarationAsNeedingValidation()
		{
			if (Parent.Parent != null)
			{
				Parent.Parent.MarkAsNeedingValidation();
			}
		}
	}
}
