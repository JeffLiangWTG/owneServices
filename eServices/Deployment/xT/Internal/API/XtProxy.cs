using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using xtdadmclr;
using Xtrade.Administration;
using Xtrade.Administration.Data.SQL;
using Xtrade.Core;
using Xtrade.Core.Objects;

namespace XT.Internal.API
{
	public class XtProxy
	{
		readonly IXtDataAdapter adapter;
		readonly XtradeServer server;
		readonly ImportOptions ImportConfig = new ImportOptions()
		{
			FailOnShortnameCollision = true,
			KeepIds = true
		};
		readonly ExportOptions ExportConfig = new ExportOptions()
		{
			AllowExternalRelations = true,
			ExportUUIDS = true,
			IncludeSensitiveData = true,
			PrettyFormat = false
		};
		readonly Func<DateTime> now;

		public XtProxy(
			Func<object, XtAdminContext.InterCom> intercomFactory, 
			Func<XtAdminContext.InterCom, IXtDataAdapter> adapterFactory, 
			Func<DateTime> now,
			string serverName)
		{
			this.now = now;
			var intercom = intercomFactory(this);
			adapter = adapterFactory(intercom);
			server = GetServer(serverName);
		}

		public XtProxy(string connectionString, string xTUserName, string xTPassword, string serverName)
			: this((owner) => {
				var interCom = new XtAdminContext().CreateIntercom(owner, "DeploymentProcess");
				interCom.RegisterRequestHandler<IWaitStatus, WaitStatusRequest>((r, i) =>
				{
					return new StatusObserver();
				});
				return interCom;
			}, 
			(intercom) => new XtdApiDataAdapter(connectionString, xTUserName, xTPassword, intercom), 
			() => DateTime.Now,
			serverName)
		{ }

		public void ImportConfiguration(string data, StringBuilder report, out List<DirectedRelation> externalRelations)
		{
			externalRelations = new List<DirectedRelation>();
			var mainStream = GetStream(server.StreamId);
			CreateHierarchy(mainStream, out var root, out var objectsBeforeImport, out var _);
			var tempStream = CloneStream(mainStream, root, report);
			SetServerStream(server, tempStream);

			var objectBeingImported = (JObject)JsonConvert.DeserializeObject(data);
			var idOfXviewBeingImported = Guid.Parse(objectBeingImported["XView"]["uuid"].Value<string>());
			if (objectsBeforeImport.ContainsKey(idOfXviewBeingImported))
			{
				var xviewBeingUpdated = objectsBeforeImport[idOfXviewBeingImported];
				var name = xviewBeingUpdated.Instance.Name;
				report.AppendLine($"XView [{name}] already existis.");
				report.AppendLine($"Checking XView [{name}] for external relations.");
				externalRelations.AddRange(GenerateReportOfExternalRelations(xviewBeingUpdated, report));
				mainStream.DeleteObject(xviewBeingUpdated.Instance, false);
				report.AppendLine($"XView [{name}] was deleted.");
			}

			var counter = 2;
			while (true)
			{
				try
				{
					mainStream.ImportConfiguration(data, mainStream.RootFolder.Id, ImportConfig);
					break;
				}
				catch (ApiException ex) when (Regex.Match(ex.Message, @"Invalid relation (?<ID>.*?)\. Not included in the import file\.") is var match && match.Success)
				{
					var idOfMissingObject = Guid.Parse(match.Groups["ID"].Value);
					var missingObjectNode = objectsBeforeImport[idOfMissingObject];
					report.AppendLine("Object is missing in import file.");
					report.AppendLine($"\tName: [{missingObjectNode.Instance.Name}]");
					var missingObjectParentXview = missingObjectNode.Ancestors().ElementAt(1);
					report.AppendLine($"\tParent XView: [{missingObjectParentXview.Instance.Name}]");
					var missingConfig = mainStream.ExportConfiguration(new[] { missingObjectParentXview.Instance.Id }, ExportConfig);
					report.AppendLine("XView [{missingObjectParentXview.Instance.Name}] was exported from current configuration.");
					var missingObject = (JObject)JsonConvert.DeserializeObject(missingConfig);
					objectBeingImported.Add($"XView@{counter++}", missingObject["XView"]);
					data = JsonConvert.SerializeObject(objectBeingImported);
					report.AppendLine($"XView [{missingObjectParentXview.Instance.Name}] was added to import file.");
					mainStream.DeleteObject(missingObjectParentXview.Instance, false);
					report.AppendLine($"XView [{missingObjectParentXview.Instance.Name}] was deleted from current configuration.");
					report.AppendLine($"Missing object was resolved.");
					continue;
				}
			}
			SetServerStream(server, mainStream);
		}

		public void ImportConfigurationFile(string path, StringBuilder report, out List<DirectedRelation> externalRelations)
		{
			ImportConfiguration(File.ReadAllText(path), report, out externalRelations);
		}

		//public string PreDeploymentCheck()
		//{
		//	return string.Empty;
		//}

		public void PostDeploymentCheck(List<DirectedRelation> externalRelations, StringBuilder report)
		{
			var mainStream = GetStream(server.StreamId);
			CreateHierarchy(mainStream, out var _, out var objects, out var _);

			foreach (var externalRelation in externalRelations)
			{
				if (!objects.ContainsKey(externalRelation.From.Instance.Id))
				{
					report.AppendLine($"Node:{ externalRelation.From.Instance.Name } is missing.");
				}
				if (!objects.ContainsKey(externalRelation.To.Instance.Id))
				{
					report.AppendLine($"Node:{ externalRelation.To.Instance.Name } is missing.");
				}
			}
        }

		IEnumerable<DirectedRelation> GenerateReportOfExternalRelations(Node node, StringBuilder report)
		{
			foreach (var externalRelation in node.RecursiveRelations().Where(x => x.IsExternal()))
			{
				report.AppendLine("Found external relation:");
				report.AppendLine("\tSource:");
				report.AppendLine($"\t\tObject: { externalRelation.From.Instance.Name }");
				report.AppendLine($"\t\tXView: { externalRelation.From.Ancestors().FirstOrDefault(x => x.Instance.Type == ObjectType.XView).Instance.Name }");
				report.AppendLine("\tTarget:");
				report.AppendLine($"\t\tObject: { externalRelation.To.Instance.Name }");
				report.AppendLine($"\t\tXView: { externalRelation.To.Ancestors().FirstOrDefault(x => x.Instance.Type == ObjectType.XView).Instance.Name }");
				yield return externalRelation;
			}
		}

		XtradeServer GetServer(string name)
		{
			return adapter.ListServers().FirstOrDefault(x => x.Name == name);
		}

		IXtDataStream GetStream(int id)
		{
			return adapter.GetStream(id);
		}

		IXtDataStream CreateStream(string name)
		{
			var newStreamId = adapter.CreateStream(-1L, name, "Deployment");
			return adapter.GetStream(newStreamId);
		}

		void SetServerStream(XtradeServer server, IXtDataStream stream)
		{
			server.StreamId = stream.StreamId;
			adapter.SaveServer(server);
		}

		IXtDataStream CloneStream(IXtDataStream source, Node root, StringBuilder report)
		{
			var sourceName = adapter.GetStreamsInfo().Single(x => x.Id == source.StreamId).Name;
			var newStream = CreateStream($"Clone {now()}");
			var newName = adapter.GetStreamsInfo().Single(x => x.Id == newStream.StreamId).Name;
			newStream.ImportConfiguration(
					source.ExportConfiguration(root.Children.Select(x => x.Instance.Id).ToArray(), ExportConfig),
					newStream.RootFolder.Id,
					ImportConfig);
			report.AppendLine($"Stream [{newName}] was cloned from [{sourceName}] stream.");
			return newStream;
		}

		void CreateHierarchy(
			IXtDataStream source,
			out Node root,
			out Dictionary<Guid, Node> objects,
			out Dictionary<Guid, DirectedRelation> relations)
		{
			var _root = new Node() { Instance = source.RootFolder };
			var _objects = source
				.GetObjects(new QSettings(), new LSettings())
				.ToDictionary(
					x => x.Id,
					x => new Node() { Instance = x }
				);
			var _relations = new Dictionary<Guid, DirectedRelation>();
			foreach (var node in _objects.Values)
			{
				if (node.Instance.Parent == _root.Instance.Id)
				{
					_root.AddChild(node);
					node.Parent = _root;
				}
				else
				{
					var parent = _objects[node.Instance.Parent];
					parent.AddChild(node);
					node.Parent = parent;
				}
				node.Instance.Reload();
				foreach (var target in node.Instance.Properties.Values
					.Where(x => x.Value.GetType() == typeof(Guid))
					.Where(x => _objects.ContainsKey((Guid)x.Value))
					.Select(x => _objects[(Guid)x.Value]))
				{
					var relation = new DirectedRelation()
					{
						From = node,
						To = target
					};
					_relations.Add(relation.Id, relation);
					node.AddOutRelation(relation);
					target.AddInRelation(relation);
				}
			}
			root = _root;
			objects = _objects;
			relations = _relations;
		}
	}
}
