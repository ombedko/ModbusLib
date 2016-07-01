using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;

namespace ModBus {

	public delegate void SendDataEvent(byte[] pData);
	public delegate void ErrorEvent(string sError);


	class MBMsgCmdItem {
		#region attributes
		private DateTime? _TimeStamp;
		#endregion


		#region properties
		public MBMsg Msg { get;  private set; }
		public MBCommand Cmd { get;  private set; }		
		public bool WasSendt { get { return _TimeStamp != null; } }
		public double WaitedTime {
			get {
				if(_TimeStamp == null)
					return -1;
				else {
					TimeSpan ts = DateTime.Now - (DateTime)_TimeStamp;
					return ts.TotalMilliseconds;
				}
			}
		}
		#endregion

		#region constructor
		public MBMsgCmdItem(MBMsg m, MBCommand c) {
			Msg = m; Cmd = c;
			_TimeStamp = null;
		}
		#endregion

		public byte[] GetArray(){
			_TimeStamp = DateTime.Now;
			return Msg.ToArray();
		}
	}


	public sealed class MBController {
		#region events
		public event SendDataEvent OnSendData;
		public event ErrorEvent OnError;

		private void _OnError(string sMsg) {
			if(OnError != null)
				OnError(sMsg);
		}
		#endregion

		#region static interface
		static Dictionary<string, MBController> _lstControllers = new Dictionary<string, MBController>();

		public static MBController GetController(string sName) {
			if(_lstControllers.Count > 0 && _lstControllers.ContainsKey(sName))
				return _lstControllers[sName];
			else
				throw new Exception("Controller name not found!");
		}
		#endregion

		#region attributes
		Random _rnd = new Random();
		const int MAX_QUE = 100;
		private Object lockQue = new Object();
		private List<MBMsgCmdItem> _que;

		private ushort _nTransactionID;

		private Thread _threadRun;
		private bool _bRun;
		#endregion

		#region properties
		public bool IsRTU { get; set; }
		public int Timeout { get; set; }
		public double BufferLoad { get { return (double)(_que.Count) * 100.0 / (double)MAX_QUE; } }
		#endregion

		#region constructor
		public MBController(string sName) {
			_que = new List<MBMsgCmdItem>();		//create que for msg and cmd 
			_nTransactionID = 1;					//initial transaction ID
			Timeout = 1000;							//default timeout
			IsRTU = false;							//default to TCP mode
			_lstControllers.Add(sName, this);		//add to static list of controllers on creation
		}
		/*
		public MBController()
			: this("default") {			
		}
		*/
		#endregion


		/// <summary>
		/// Que a new message in the buffer
		/// </summary>
		/// <param name="nUnitID"></param>
		/// <param name="nFuncID"></param>
		/// <param name="pMsgData"></param>
		/// <param name="cmd"></param>
		public void QueMsg(byte nUnitID, MBFunctionType nFuncID, byte[] pMsgData, MBCommand cmd) {			
			MBMsg msg;

			lock(lockQue) {

				//create a message
				if(IsRTU) msg = new MBMsgRTU(nUnitID, nFuncID, pMsgData);
				else msg = new MBMsgTCP(nUnitID, nFuncID, pMsgData) { TransactionID = _nTransactionID };
				_nTransactionID++;		//increment transaction id
				
				//add to que 
				_que.Add(new MBMsgCmdItem(msg, cmd));

				if(_que.Count > MAX_QUE) {
					int nRemove = _rnd.Next(MAX_QUE-1)+1;
					_que.RemoveAt(nRemove);	//remove at random to avoid any kind of pattern, not 0, since 0 may be in the process of beeing send. 1 never is (due to lock on buffer)
					_OnError("Buffer full, removing item " + nRemove.ToString());
				}
					
			}
		}
		
		/// <summary>
		/// Handle recieved data. This should be called when data is recieved from TCP or Serial handler
		/// </summary>
		/// <param name="pData"></param>
		public void OnDataRecieved(byte[] pData) {
			lock(lockQue) {
				//create MBMsg object based on RTU setting
				MBMsg msg;
				if(IsRTU) msg = new MBMsgRTU();
				else msg = new MBMsgTCP();

				//make a Message from the buffer
				if(msg.FromArray(pData))			//if message from array success
					_que[0].Cmd.HandleMsg(msg);		//forward msg to first cmd in buffer

				if(!IsRTU && ((MBMsgTCP)msg).TransactionID != ((MBMsgTCP)(_que[0].Msg)).TransactionID)
					_OnError("Lost transaction");

				_que.RemoveAt(0);					//remove from que, either successfully handled or something wrong with incomming data. Either way, this transaction is done
			}
				
		}

		#region thread
		// Worker Thread to send outgoing msg and time out waiting for reply
		private void _Run() {
			bool bSleep;			
			while(_bRun) {
				bSleep = false;

				lock(lockQue) {
					if(_que.Count > 0) {

						//are we waiting for a message?
						if(_que[0].WasSendt) {
							if(_que[0].WaitedTime > Timeout) {				//timeout?
								_que.RemoveAt(0);							//remove
								_OnError("Timeout");
							}
							else
								bSleep = true;								//wait for reply or timeout
						}
						else {												//not waiting for the current first message							
							OnSendData(_que[0].GetArray());					//make byte buffer and send it (also sets the timestamp)							
						}
					}
					else
						bSleep = true;										//wait for new message in buffer
				}

				//Sleep thread if waiting for new message, reply or timeout
				if(bSleep)
					Thread.Sleep(10);				
			}
		}
		#endregion

		#region methods
		/// <summary>
		/// Start worker thread, internal function only. Called from static fuinction Start()
		/// </summary>
		public void Start() {
			if(OnSendData == null)
				throw new Exception("No event handler for OnSendData defined");
			//create thread object			
			_bRun = true;
			_threadRun = new Thread(_Run);
			_threadRun.Start();
		}

		/// <summary>
		/// Stop thread and terminate object. Internal function only. Called from static public function Stop()
		/// </summary>
		public void Stop() {
			_bRun = false;
		}
		#endregion
	}
}
