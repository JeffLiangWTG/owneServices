using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public class AddRelatedManagementOrganisationModuleDecisionProvider : ZNodeModuleDecisionProvider<OrgHeader>
	{
		public AddRelatedManagementOrganisationModuleDecisionProvider(ZNode<OrgHeader> parentNode) : base(parentNode)
		{
		}

		public static AddRelatedManagementOrganisationModuleDecisionProvider New(ZNode<OrgHeader> parentOrganisationNode)
		{
			var type = TypeDecider.GetTypeForBinding(typeof(AddRelatedManagementOrganisationModuleDecisionProvider));
			var constructor = type.GetConstructor(new[] { typeof(ZNode<OrgHeader>) });
			return (AddRelatedManagementOrganisationModuleDecisionProvider)constructor.Invoke(new object[] { parentOrganisationNode });
		}

		protected override bool TryAddChildren(IEnumerable<OrgHeader> children)
		{
			var added = false;
			var factory = parentNode.BizObj.Factory;
			foreach (var child in children)
			{
				var reloadedChild = factory.Load<OrgHeader>(child.PK);
				if (reloadedChild != null)
				{
					var newChildNode = parentNode.AddNewChild(reloadedChild);
					if (newChildNode != null)
					{
						added = true;
					}
				}
			}

			return added;
		}

		protected override IBusinessObjectCollection GetList()
		{
			var query = new ZQuery();

			var rootNode = FindRootNode(parentNode);
			query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, rootNode.BizObj.PK);
			AddNodeToQuery(query, rootNode);
			return new OrgHeaderCollection(new BusinessObjectFactory(), query);
		}

		protected ZNode<OrgHeader> FindRootNode(ZNode<OrgHeader> node)
		{
			while (node.ParentNode != null)
			{
				node = node.ParentNode;
			}
			return node;
		}

		protected void AddNodeToQuery(ZQuery query, ZNode<OrgHeader> node)
		{
			if (node?.ChildNodes != null)
			{
				foreach (var child in node?.ChildNodes)
				{
					query.AddToFilter(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, child.BizObj.PK);
					AddNodeToQuery(query, child);
				}
			}
		}

		public override bool ShouldIgnoreAdditionalFilter
		{
			get { return true; }
		}

		public override bool ShouldDisplayNotifications
		{
			get { return true; }
		}
	}
}
