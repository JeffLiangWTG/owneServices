using System;
using System.ComponentModel.Design.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.AutoRating
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class AutoRatingExplorerForm : Form // AutoRatingHelperForm form is not part of Enterprise.
	{
		public AutoRatingExplorerForm()
		{
			InitializeComponent();
		}

		public AutoRatingExplorerForm(object businessEntity)
			: this()
		{
			this.businessEntity = businessEntity;
			businessEntityXML = serializer.GetXML(businessEntity);

			xmlContent = new XmlDocument();
			xmlContent.LoadXml(businessEntityXML);

			PopulateTreeViewFromXML(PropertiesTreeView.Nodes);
		}

		readonly object businessEntity;
		XmlDocument xmlContent;
		bool isLoadedFromFile;
		bool hasBeenSent;
		readonly string businessEntityXML;

		protected override void OnLayout(LayoutEventArgs levent)
		{
			this.AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			this.AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		void PopulateTreeViewFromXML(TreeNodeCollection nodes)
		{
			nodes.Clear();
			AddNode(xmlContent.DocumentElement, nodes);
		}

		string AddNode(XmlNode xmlNode, TreeNodeCollection nodes)
		{
			string result = xmlNode.Value;

			if (xmlNode.HasChildNodes)
			{
				foreach (XmlNode childXmlNode in xmlNode.ChildNodes)
				{
					if (childXmlNode.NodeType != XmlNodeType.Text && childXmlNode.Name != RatingObjectSerializer.tagElement)
					{
						TreeNode newTreeNode = new TreeNode(Regex.Unescape(childXmlNode.Name));
						nodes.Add(newTreeNode);
						newTreeNode.Tag = AddNode(childXmlNode, newTreeNode.Nodes);
					}
					else
					{
						result = childXmlNode.Value ?? childXmlNode.FirstChild.Value;
					}
				}
			}

			return result;
		}

		void PropertiesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			DescriptionBox.Text = e.Node.Tag != null ? (string)e.Node.Tag : "";
		}

		void OpenButton_Click(object sender, EventArgs e)
		{
			ZOpenFileDialog dlg = new ZOpenFileDialog();
			dlg.Title = (NoResString)"Open XML Document"; // This is a tool for CargoWise Support only
			dlg.Filter = (NoResString)"XML Files (*.xml)|*.xml"; // This is a tool for CargoWise Support only

			if (dlg.ShowDialog() == DialogResult.OK)
			{
				try
				{
					XmlDocument xDoc = new XmlDocument();
					xDoc.Load(dlg.UnmappedFileName);
					xmlContent = xDoc;
					isLoadedFromFile = true;
					SendButton.Enabled = false;
					PopulateTreeViewFromXML(PropertiesTreeView.Nodes);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.Show(ex.Message);
				}
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			string recipient = RecipientEmailTextBox.Text;

			if (Regex.IsMatch(recipient, emailValidationPattern))
			{
				string fileName = (businessEntity is IBusiness ? (string)((IBusiness)businessEntity).HumanReadableName : "autoRating");

				foreach (char c in System.IO.Path.GetInvalidFileNameChars())
				{
					fileName = fileName.Replace(c.ToString(), "");
				}

				EmailDef email = new EmailDef();
				email.AddRecipientForSystemCommunication(recipient);
				email.Subject = (NoResString)"AutoRating job dump"; // This is a tool for CargoWise Support only
				email.Attachments.Add(new AttachmentDef(fileName + ".xml", Encoding.Unicode.GetBytes(businessEntityXML)));
				Env.OutgoingMailManager.CreateAndSave(email);
				hasBeenSent = true;
				SendButton.Enabled = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a tool for CargoWiseOne Support only")]
		const string emailValidationPattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

		void emailTextBox_TextChanged(object sender, EventArgs e)
		{
			SendButton.Enabled = Regex.IsMatch(RecipientEmailTextBox.Text, emailValidationPattern) && !hasBeenSent && !isLoadedFromFile;
		}

		readonly RatingObjectSerializer serializer = new RatingObjectSerializer();
	}
}
