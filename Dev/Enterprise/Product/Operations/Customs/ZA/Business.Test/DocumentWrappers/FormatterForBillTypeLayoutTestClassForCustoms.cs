using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	internal class FormatterForBillTypeLayoutTestClassForCustoms : FormatterForBillTypeLayout
	{
		public FormatterForBillTypeLayoutTestClassForCustoms()
			: base()
		{
			base.HaveCalculatedMarksAndNumbers = false;
			base.HaveCalculatedDescription = false;
			base.HaveCalculatedContainerColumns = false;
		}

		public ZInt TestMarksAndNumbersAndDescriptionRowHeight;
		public ZInt TestMarksAndNumbersWidth;
		public ZInt TestDescriptionWidth;
		public ZInt TestContainerRowHeight;

		public override ZString PackageTypeDescription
		{
			get
			{
				var freightPkgUnitList = new RefPackTypeCollection(new BusinessObjectFactory());
				return freightPkgUnitList.GetDescriptionFromCode("PLT");
			}
		}

		public override ZInt PackCount
		{
			get { return 100; }
		}

		public override IDocSimpleContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					BusinessObjectFactory factory = new BusinessObjectFactory();

					fContainers = new IDocSimpleContainerCollection(factory);

					var type20FR = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR"));
					var type40OT = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40OT"));

					var container1 = factory.New<CusContainer>();
					container1.CO_ContainerNumber = "1";
					container1.CO_FCL_LCL_AIR = "FCL";
					container1.CO_RC = type20FR.PK;
					container1.CO_Seal = "seal 1";
					fContainers.Add(DocCusContainer.New(container1, factory));

					var container2 = factory.New<CusContainer>();
					container2.CO_ContainerNumber = "2";
					container2.CO_FCL_LCL_AIR = "FCL";
					container2.CO_RC = type20FR.PK;
					fContainers.Add(DocCusContainer.New(container2, factory));

					var container3 = factory.New<CusContainer>();
					container3.CO_ContainerNumber = "3";
					container3.CO_FCL_LCL_AIR = "FCL";
					container3.CO_RC = type40OT.PK;
					container3.CO_Seal = "seal 3";
					fContainers.Add(DocCusContainer.New(container3, factory));

					var container4 = factory.New<CusContainer>();
					container4.CO_ContainerNumber = "4";
					container4.CO_FCL_LCL_AIR = "LCL";
					container4.CO_RC = type40OT.PK;
					container4.CO_Seal = "seal 4";
					fContainers.Add(DocCusContainer.New(container4, factory));
				}

				return fContainers;
			}
		}

		public override ZString UnformattedGoodsDescription
		{
			get { return "This is the goods description\nto test this formatter with"; }
		}

		public override ZString UnformattedMarksAndNumbers
		{
			get { return "This is the marks and numbers\nto test this formatter with"; }
		}

		public override ZString ContainerSectionHeading
		{
			get { return "CONTAINER        SEAL                     TYPE         "; }
		}

		public override ZBool ShowContainerAdditionalDetails
		{
			get { return ZBool.False; }
		}

		public override ZInt MarksAndNumbersAndDescriptionRowHeight
		{
			get { return TestMarksAndNumbersAndDescriptionRowHeight; }
		}

		public override ZInt MarksAndNumbersWidth
		{
			get { return TestMarksAndNumbersWidth; }
		}

		public override ZInt DescriptionWidth
		{
			get { return TestDescriptionWidth; }
		}

		public override ZInt ContainerRowHeight
		{
			get { return TestContainerRowHeight; }
		}

		protected IDocSimpleContainerCollection fContainers;
	}
}
