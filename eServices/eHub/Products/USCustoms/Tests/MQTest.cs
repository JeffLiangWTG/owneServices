using System;
using IBM.WMQ;
using IBM.WMQ.PCF;

namespace CargoWise.eServices.USCustoms.Tests
{
	public class MQTest
	{
		public MQTest(string hostname, int port, string channel)
		{
			MQEnvironment.Hostname = hostname;
			MQEnvironment.Port = port;
			MQEnvironment.Channel = channel;
		}

		public MQQueueManager QueueManager
		{
			get { return queueManager; }
		}
		MQQueueManager queueManager;

		public string ConnectMQ()
		{
			String strReturn = "";

			try
			{
				queueManager = new MQQueueManager();
				strReturn = "Connected Successfully";
			}
			catch (MQException exp)
			{
				strReturn = "Exception: " + exp.Message;
			}

			return strReturn;
		}

		public string CommitAndDisconnectMQ()
		{
			String strReturn = "";

			try
			{
				queueManager.Commit();
				queueManager = null;
				strReturn = "Connected Successfully";
			}
			catch (MQException exp)
			{
				strReturn = "Exception: " + exp.Message;
			}

			return strReturn;
		}

		public string BackoutAndDisconnectMQ()
		{
			String strReturn = "";

			try
			{
				queueManager.Backout();
				queueManager = null;
				strReturn = "Connected Successfully";
			}
			catch (MQException exp)
			{
				strReturn = "Exception: " + exp.Message;
			}

			return strReturn;
		}

		public string WriteMsg(string queueName, string strInputMsg)
		{
			string strReturn = "";

			try
			{
				var queue = queueManager.AccessQueue(queueName, MQC.MQOO_OUTPUT + MQC.MQOO_FAIL_IF_QUIESCING);

				var queueMessage = new MQMessage();
				queueMessage.WriteString(strInputMsg);
				queueMessage.Format = MQC.MQFMT_STRING;
				var queuePutMessageOptions = new MQPutMessageOptions();
				queue.Put(queueMessage, queuePutMessageOptions);
				queue.Close();
				strReturn = "Message sent to the queue successfully";
			}
			catch (MQException MQexp)
			{
				strReturn = "Exception: " + MQexp.Message;
			}
			catch (Exception exp)
			{
				strReturn = "Exception: " + exp.Message;
			}

			return strReturn;
		}

		public bool ClearQueue(string queueName)
		{
			bool result = true;

			try
			{
				var agent = new PCFMessageAgent(QueueManager);
				var request = new PCFMessage(CMQCFC.MQCMD_CLEAR_Q);
				request.AddParameter(MQC.MQCA_Q_NAME, queueName);
				var responses = agent.Send(request);
			}
			catch (Exception)
			{
				result = false;
			}

			return result;
		}

		public string ReadMsg(string queueName, bool waitInterval)
		{
			String strReturn = "";

			try
			{
				var queue = queueManager.AccessQueue(queueName, MQC.MQOO_INPUT_SHARED + MQC.MQOO_FAIL_IF_QUIESCING + MQC.MQOO_INQUIRE + MQC.MQGMO_CONVERT);
				var queueMessage = new MQMessage();
				queueMessage.Format = MQC.MQFMT_STRING;

				if (queue.CurrentDepth > 0)
				{
					var queueGetMessageOptions = new MQGetMessageOptions();
					queueGetMessageOptions.Options = MQC.MQGMO_SYNCPOINT + MQC.MQGMO_WAIT + MQC.MQGMO_FAIL_IF_QUIESCING;
					if (waitInterval) queueGetMessageOptions.WaitInterval = MQC.MQWI_UNLIMITED; //wait
					queue.Get(queueMessage, queueGetMessageOptions);
					strReturn = queueMessage.ReadString(queueMessage.MessageLength);
				}
				queue.Close();
			}
			catch (MQException MQexp)
			{
				strReturn = "Exception : " + MQexp.Message;
			}
			catch (Exception exp)
			{
				strReturn = "Exception: " + exp.Message;
			}

			return strReturn;
		}
	}
}