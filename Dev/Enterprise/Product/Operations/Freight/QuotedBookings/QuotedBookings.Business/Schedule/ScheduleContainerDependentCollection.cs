//using Enterprise.ZArchitecture.Business;
//using Enterprise.ZArchitecture.Schema;
//using Enterprise.MasterFiles.Business;
//using Enterprise.Core;

//namespace Enterprise.Freight.QuotedBookings.Business
//{
//    public class ScheduleContainerDependentCollection : BaseDependentContainerCollection
//    {
//        public ScheduleContainerDependentCollection(PackContainerHelper packContainerHelper, BusinessObjectFactory factory)
//            : base(packContainerHelper.Sailing, factory)
//        {
//            this.packContainerHelper = packContainerHelper;
//        }

//        public ScheduleContainerDependentCollection(BusinessObjectFactory factory)
//            : base(factory)
//        {
//        }

//        public ForwardingContainer this[int Index]
//        {
//            get { return (ForwardingContainer)Elements[Index]; }
//        }

//        public new ForwardingContainer AddNew()
//        {
//            return (ForwardingContainer)base.AddNew();
//        }

//        protected override SchemaGuidColumn FKSchemaColumnInDependent
//        {
//            get { return JobContainerSchema.JC_JX; }
//        }

//        #region SetDefaultsForNewChild

//        protected override void SetDefaultsForNewChild(BusinessObject Child)
//        {
//            base.SetDefaultsForNewChild(Child);
//            ForwardingContainer container = (ForwardingContainer)Child;
//            if (Sailing != null && Sailing.JX_TransportMode == Core.Constants.TransportModes.Air)
//            {
//                container.JC_ContainerMode = Constants.ContainerModes.ULD;
//            }
//            else
//            {
//                container.JC_ContainerMode = Constants.ContainerModes.Groupage;
//            }
//        }

//        #endregion

//        #region Implementation

//        PackContainerHelper SailingHelper
//        {
//            get { return packContainerHelper; }
//        }
//        PackContainerHelper packContainerHelper;

//        JobSailing Sailing
//        {
//            get { return SailingHelper.Sailing; }
//        }

//        #endregion
//    }
//}

//#region TestCase
//#if DEBUG

//namespace Enterprise.Freight.QuotedBookings.Business.Testing
//{
//    using NUnit.Framework;
//    using Enterprise.ZArchitecture.Business.Testing;
//    using Enterprise.Core;

//    public class ScheduleContainerDependentCollectionTest : BusinessObjectCollectionTestCase
//    {
//        protected override BusinessObjectCollection GetCollectionToTest()
//        {
//            JobSailing Sailing = Factory.New<JobSailing>();
//            PackContainerHelper SailingHelper = new PackContainerHelper(Sailing);
//            return new ScheduleContainerDependentCollection(SailingHelper, Factory);
//        }

//        protected override BusinessObject GetNewElementToAddToTheCollection()
//        {
//            return Factory.New<ForwardingContainer>();
//        }

//        public void TestAllowNew()
//        {
//            AssertEquals("Allow new should be true.", true, SailingHelper.Containers.AllowNew);
//        }

//        public void TestSetDefaultsForNewChild()
//        {
//            ForwardingContainer Container = SailingHelper.Containers.AddNew();
//            AssertEquals("ContainerMode should be GRP.", Constants.ContainerModes.Groupage, Container.JC_ContainerMode);

//            Sailing.Voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
//            ForwardingContainer Container2 = SailingHelper.Containers.AddNew();
//            AssertEquals("ContainerMode should be ULD.", Constants.ContainerModes.ULD, Container2.JC_ContainerMode);
//        }

//        #region Implementation

//        JobSailing Sailing
//        {
//            get
//            {
//                if (fSailing == null)
//                {
//                    JobVoyage Voyage = Factory.New<JobVoyage>();
//                    Voyage.Origins.AddNew();
//                    Voyage.Origins[0].JA_RL_NKPortOfLoading = "AUSYD";
//                    Voyage.Origins[0].JA_E_DEP = ZDateTime.Today;

//                    Voyage.Destinations.AddNew();
//                    Voyage.Destinations[0].JB_RL_NKPortOfDischarge = "HKHKG";
//                    Voyage.Destinations[0].JB_E_ARV = ZDateTime.Today.AddDays(9);

//                    fSailing = Voyage.Sailings[0];
//                }
//                return fSailing;
//            }
//        }
//        JobSailing fSailing;

//        PackContainerHelper SailingHelper
//        {
//            get
//            {
//                if (fSailingHelper == null)
//                {
//                    fSailingHelper = new PackContainerHelper(Sailing);
//                }
//                return fSailingHelper;
//            }
//        }
//        PackContainerHelper fSailingHelper;

//        #endregion
//    }
//}

//#endif
//#endregion
