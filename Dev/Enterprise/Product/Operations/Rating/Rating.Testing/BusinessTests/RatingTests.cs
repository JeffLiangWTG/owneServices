using System;
using System.Collections.Generic;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.UniversalCopy.GUI;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.RatingTests.Testing.Business
{
	public class RatingTests : TestCaseWithFactory
	{
		public void TestAutoRatingFormCanLoadXMLForCFSShipping()
		{
			var shipment = Factory.New<CFSShipment>();

			var ratingObjectSerializer = new RatingObjectSerializer();
			var xml = ratingObjectSerializer.GetXML(shipment);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			AssertNotNull("XML from ratingObjectSerializer should be parsable", xmlDocument.DocumentElement);
		}

		public void TestRatingAdaptorJobIDisFoundOnValidObjects()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertNotNull(shipment.RatingAdapter);
			AssertNullOrEmpty(shipment.RatingAdapter.OperationalJobCode);
			AssertNullOrEmpty(shipment.RatingAdapter.JobID);

			var customsDeclaration = Factory.New<Customs.US.Business.JobDeclaration>();
			AssertNotNull(customsDeclaration.RatingAdapter);
			AssertNullOrEmpty(customsDeclaration.RatingAdapter.OperationalJobCode);
			AssertNullOrEmpty(customsDeclaration.RatingAdapter.JobID);

			var consol = Factory.New<ForwardingConsol>();
			AssertNotNull(consol.RatingAdapter);
			AssertNotNullOrEmpty(consol.RatingAdapter.OperationalJobCode);
			AssertNullOrEmpty(consol.RatingAdapter.JobID);

			Factory.Save();

			AssertNotNullOrEmpty(shipment.RatingAdapter.OperationalJobCode);
			AssertNotNullOrEmpty(shipment.RatingAdapter.JobID);

			AssertNotNullOrEmpty(customsDeclaration.RatingAdapter.OperationalJobCode);
			AssertNotNullOrEmpty(customsDeclaration.RatingAdapter.JobID);

			AssertNotNullOrEmpty(consol.RatingAdapter.OperationalJobCode);
			AssertNotNullOrEmpty(consol.RatingAdapter.JobID);
		}

		public void TestUniversalCopyRateLine_WhenCallingGetFilter_ShouldNotCreateOrphanRateLine()
		{
			try
			{
				ExceptionReporterTestListener.Instance.Clear();
				var copyTemplateTree = new CopyTemplateTree();

				var copyTemplateNode = new EntityCopyTemplateNode { Name = "RateLine" };
				copyTemplateNode.Nodes.Add(new PropertyCopyTemplateNode { Name = RateLinesSchema.Constants.TL_RX_NKCurrency, PropertyType = "string", CopyMethod = CopyMethod.Copy });

				copyTemplateTree.InnerNode = copyTemplateNode;

				using (var copyManager = new UniversalCopyManagerForTest(typeof(RateLine), ModuleIDs.NotAssigned))
				{
					copyManager.GetFilter(copyTemplateTree, new List<string>());
				}

				AssertEquals("Calling get filter for  universal copy rate line should not create orphan rate line", 0, ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		class UniversalCopyManagerForTest : UniversalCopyManager
		{
			public UniversalCopyManagerForTest(Type elementType, ZModule module) : base(elementType, module)
			{
			}

			public UniversalCopyManagerForTest(Type elementType, ModuleIdentifier moduleId)
				: base(elementType, moduleId)
			{
			}
			protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
			{
				throw new NotImplementedException();
			}

			protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
			{
				throw new NotImplementedException();
			}

			protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
			{
				throw new NotImplementedException();
			}

			protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
			{
				throw new NotImplementedException();
			}
		}
	}
}
