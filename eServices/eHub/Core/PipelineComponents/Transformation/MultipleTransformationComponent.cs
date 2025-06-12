using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace CargoWise.eHub.Core.PipelineComponents
{
	/// <summary>
	/// Map a document multiple times.  If the output of one of the maps is zero length an exception is thrown and the
	/// pipeline will terminate.  The map and target document type are resolved from the database by using the sender,
	/// recipient and source document type.  There are two types of transformations; predicated and non-predicated.
	/// Predictaed transform sets take precedence over non-predicated sets.  A 'predicate' is an xpath that is applied
	/// to the source document to find a node.  The first xpath matched, out of a collection of xaths (transform sets),
	/// will indicate that the associated transform set should be applied.
	/// </summary>
	[ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
	[ComponentCategory(CategoryTypes.CATID_Any)]
	[Guid("94B0793D-3BBE-42df-B13B-0C44A16E4E15")]
	public class MultipleTransformationComponent : IBaseComponent, IComponentUI, IComponent, IPersistPropertyBag
	{
		#region IBaseComponent Members

		public string Description
		{
			get { return "Perform multiple transformations to target format."; }
		}

		public string Name
		{
			get { return "Multiple Transformation"; }
		}

		public string Version
		{
			get { return "1.0"; }
		}

		#endregion

		#region IComponentUI Members

		public IntPtr Icon
		{
			get { return IntPtr.Zero; }
		}

		public IEnumerator Validate(object projectSystem)
		{
			return null;
		}

		#endregion

		#region IComponent Members

		[ThreadStatic]
		public static IBaseMessageContext context;

		public IBaseMessage Execute(IPipelineContext pipelineContext, IBaseMessage message)
		{
			if (!this.Enabled) return message;

			Tracer.TraceStart(pipelineContext, message);
			try
			{
				string lastTargetMessageType = string.Empty;
				var transformSetID = string.Empty;
				var cts = new CancellationTokenSource();
				var transformTask = new Task<IBaseMessage>(() => PerformTransformation(message, pipelineContext, cts, out lastTargetMessageType, ref transformSetID));
				transformTask.Start();
				if (!transformTask.Wait(this.Timeout))
				{
					cts.Cancel();
					throw new TimeoutException("Timeout performing transformation.");
				}
				var result = transformTask.Result;

				// Promote the final schema strong name and message type instead of disassembling the message (which would unpack the message if it's an envelope)

				if (!String.IsNullOrEmpty(lastTargetMessageType))
				{
					var docSpec = pipelineContext.GetDocumentSpecByType(lastTargetMessageType);
					result.Context.WriteProperty<BTS.SchemaStrongName>(docSpec.DocSpecStrongName);
					result.Context.PromoteProperty<BTS.MessageType>(docSpec.DocType);
					Tracer.TraceInfo("Final message type: {0} ({1})", docSpec.DocType, docSpec.DocSpecStrongName);
				}

				if (!string.IsNullOrEmpty(transformSetID))
				{
					result.Context.WriteProperty<TransformSetID>(transformSetID);
				}

				Tracer.TraceEnd();
				return result;
			}
			catch (Exception ex)
			{
				Tracer.TraceError(ex);
				if (ex is AggregateException)
					throw ex.InnerException;
				else
					throw;
			}
		}

		IBaseMessage PerformTransformation(IBaseMessage message, IPipelineContext pipelineContext, CancellationTokenSource cts, out string lastTargetMessageType, ref string transformSetID)
		{
			try
			{
				context = message.Context;

				// try set the static context property in order for net45 interface to invoke net40 mapping
				TrySetNet40PipelineComponentContextIfNull();
				var transformSets = FindTransformationTypes(message);
				var transformSetsCount = transformSets.Count;
				transformSetID = transformSetsCount == 0 ? string.Empty : transformSets[0].Transforms[0].TransformSetID.ToString().ToUpper();

				if (transformSetsCount == 1 
					&& string.IsNullOrEmpty(transformSets[0].XpathPredicate)
					&& transformSets[0].Transforms.Count == 1
					&& string.IsNullOrEmpty(transformSets[0].Transforms[0].MapTypeName)
					&& string.IsNullOrEmpty(transformSets[0].Transforms[0].TargetMessageType))
				{
					transformSets.Clear(); //The only transformation set in the collection is for passing TransformSetID.
				}

				var result = GetTransformationPerformer().PerformTransformation(transformSets, pipelineContext, message, out lastTargetMessageType);
				result.Context = PipelineUtil.CloneMessageContext(context);
				return result;
			}
			catch (Exception)
			{
				if (cts.IsCancellationRequested)
				{
					lastTargetMessageType = null;
					return null;
				}
				else
					throw;
			}
		}

		void TrySetNet40PipelineComponentContextIfNull()
		{
			try
			{
				var net40assembly = Assembly.Load("CargoWise.eHub.Core.PipelineComponents, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
				if (net40assembly != null)
				{
					var net40Type = net40assembly.GetType("CargoWise.eHub.Core.PipelineComponents.MultipleTransformationComponent");
					var net40context = net40Type.GetField("context", BindingFlags.Static | BindingFlags.Public);
					net40context.SetValue(null, context);
				}
			}
			catch
			{
			}
		}

		internal virtual ITransformationPerformer GetTransformationPerformer()
		{
			return new TransformationPerformer();
		}

		internal virtual ITransformAccessor GetTransformationAccessor()
		{
			return DataAccessFactories.NewTransformAccessorInstance();
		}

		internal List<TransformSet> FindTransformationTypes(IBaseMessage message)
		{
			var transformSetsToApply = new List<TransformSet>();

			string senderID = message.Context.ReadPropertyString<BTS.SourceParty>();
			string recipientID = message.Context.ReadPropertyString<BTS.DestinationParty>();
			string sourceMessageType = message.Context.ReadPropertyString<BTS.MessageType>();

			if (string.IsNullOrWhiteSpace(sourceMessageType))
			{
				return transformSetsToApply;
			}

			var transformSetsFound = GetTransformationAccessor().SelectTransformsByPartiesMessage(senderID, recipientID, sourceMessageType, null);

			// Check if there are any predicated transform sets and return the one that is found first while parsing the document
			var predicatedTransformSets = transformSetsFound.FindAll(delegate(TransformSet set) { return !String.IsNullOrEmpty(set.XpathPredicate); });
			if (predicatedTransformSets.Count > 0)
			{
				var settings = new XmlReaderSettings();
				settings.CloseInput = false;
				settings.IgnoreWhitespace = true;
				using (var xmlReader = XmlReader.Create(message.BodyPart.GetOriginalDataStream(), settings))
				{
					var xPaths = new List<string>();
					predicatedTransformSets.ForEach(delegate(TransformSet set) { xPaths.Add(set.XpathPredicate); });
					var xDoc = new XPathDocument(xmlReader);
					var xNavigator = xDoc.CreateNavigator();
					for (int i = 0; i < xPaths.Count; i++)
					{
						var xNode = xNavigator.SelectSingleNode(xPaths[i]);
						if (xNode != null)
						{
							transformSetsToApply.Add(predicatedTransformSets[i]);
							break;
						}
					}
				}
				message.BodyPart.GetOriginalDataStream().Seek(0, SeekOrigin.Begin);
			}
			else
			{
				// Determine if there are any non-predicated transform sets and if so return the first one in the list
				var nonPredicatedTransformSets = transformSetsFound.FindAll(delegate(TransformSet set) { return String.IsNullOrEmpty(set.XpathPredicate); });
				if (nonPredicatedTransformSets.Count > 0)
				{
					transformSetsToApply.Add(nonPredicatedTransformSets[0]);
				}
			}

			return transformSetsToApply;
		}

		#endregion

		#region IPersistPropertyBag Members

		public void GetClassID(out Guid classID)
		{
			classID = new Guid("94B0793D-3BBE-42df-B13B-0C44A16E4E15");
		}

		public void InitNew()
		{
		}

		public void Load(IPropertyBag propertyBag, int errorLog)
		{
			object val = null;
			try { propertyBag.Read("Enabled", out val, errorLog); } catch { }
			if (val != null) Enabled = Convert.ToBoolean(val);
			try { propertyBag.Read("Timeout", out val, errorLog); } catch { }
			if (val != null) Timeout = (int)val;
		}

		public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
		{
			object val = Enabled;
			propertyBag.Write("Enabled", ref val);
			val = Timeout;
			propertyBag.Write("Timeout", ref val);
		}

		#endregion

		#region Properties

		public bool Enabled { get; set; }

		int timeout = 0;
		public int Timeout 
		{
			get { return timeout <= 0 ? System.Threading.Timeout.Infinite : timeout; }
			set { timeout = value; }
		}

		#endregion
	}
}
