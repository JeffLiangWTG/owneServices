using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestsSubclassesOf(typeof(DetentionStrategy))]
	internal abstract class DetentionStrategyBaseTest<T> : TestCaseWithFactory where T : DetentionStrategy, new()
	{
		public void TestIsRegistered()
		{
			string[] detentionTypes = GetDetentionTypes();
			if (detentionTypes == null || detentionTypes.Length == 0)
			{
				Fail("No detention types specified");
			}
			else
			{
				foreach (string detentionTypeCode in detentionTypes)
				{
					AssertType("Detention Type: " + detentionTypeCode, typeof(T), DetentionStrategy.New(detentionTypeCode));
				}
			}
		}

		#region Implementation
		public JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					voyage = Factory.New<JobVoyage>();
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
					voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
					voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
					voyage.GenerateSailings();
				}

				return voyage;
			}
		}

		JobVoyage voyage;
		public BillOfLading ImportShipment
		{
			get
			{
				if (importShipment == null)
				{
					importShipment = Factory.New<BillOfLading>();
					importShipment.JS_JX = Voyage.Sailings.GetSailingFromLoadAndDischarge("NLAMS", "AUBNE").PK;
				}

				return importShipment;
			}
		}

		BillOfLading importShipment;
		public BillOfLading ExportShipment
		{
			get
			{
				if (exportShipment == null)
				{
					exportShipment = Factory.New<BillOfLading>();
					exportShipment.JS_JX = Voyage.Sailings.GetSailingFromLoadAndDischarge("AUBNE", "NZAKL").PK;
				}

				return exportShipment;
			}
		}

		BillOfLading exportShipment;
		public BillOfLadingContainer ImportContainer
		{
			get
			{
				return importContainer ?? (importContainer = ImportShipment.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer importContainer;
		public BillOfLadingContainer ExportContainer
		{
			get
			{
				return exportContainer ?? (exportContainer = ExportShipment.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer exportContainer;
		public RefContainerStock Stock
		{
			get
			{
				if (stock == null)
				{
					stock = Factory.New<RefContainerStock>();
					stock.R6_ContainerNum = "TEST4100013";
					stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
				}

				return stock;
			}
		}

		RefContainerStock stock;
		public ContainerMovement Movement
		{
			get
			{
				return movement ?? (movement = Stock.Movements.AddNew());
			}
		}

		ContainerMovement movement;
		public ContainerDetention Detention
		{
			get
			{
				return detention ?? (detention = Factory.New<ContainerDetention>());
			}
		}

		ContainerDetention detention;
		public T MovementType
		{
			get
			{
				return movementType ?? (movementType = new T());
			}
		}

		protected T movementType;
		protected abstract string[] GetDetentionTypes();
		#endregion
	}
}
