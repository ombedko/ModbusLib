using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using ModBus;					//BEDKO ModBus library
using TcpComm;					//BEDKO TCP Comm library
using ThreadSafeExtensions;		//BEDKO ThreadSafe cross-thread GUI handling library

namespace MBTest {
	public partial class Form1 : Form {
		MBCommandReadInputRegisters _readIR;
		TcpClient _client;
		Timer _tmrReadData;

		public Form1() {
			InitializeComponent();

			_tmrReadData = new Timer();
			_tmrReadData.Interval = 1000;
			_tmrReadData.Tick += new EventHandler(_tmrReadData_Tick);

			_client = new TcpClient();
			_client.Ip = "192.168.2.221";
			_client.Port = 502;
			_client.DataRecieved += new TCPDataRecieved(_client_DataRecieved);

			_readIR = new MBCommandReadInputRegisters();
			_readIR.StartAdr = 0;
			_readIR.Quantity = 4;
			_readIR.SendDataEvent += new SendData(_readIR_SendDataEvent);


			MBController.Start();		//first start the modbus controller singlton
			_client.Start();			//start tcp client
			_tmrReadData.Start();		//start data reading timer

		}
				
		void _tmrReadData_Tick(object sender, EventArgs e) {
			//call this to make a command. When finished, the "SendDataEvent" is triggered to let the calling object send the data in its own way
			_readIR.MakeCommand();
		}

		void _readIR_SendDataEvent(byte[] pData) {
			//new byte array from read command, ready to send
			_client.SendMsg(pData);
		}

		void _client_DataRecieved(byte[] pData) {
			//got some data, check if it was for this read object
			if(_readIR.CheckRecievedData(pData)) {
				//this message was for this readIR object, so now its registers is updated
				ThreadSafe.Do(this, () => {//update the gui label in a threadsafe way, by invoking the anonymous code on the form's thread, not the modbus event handler
					string sText = "";
					foreach(short reg in _readIR.Registers){
						sText += reg.ToString()+"\r\n";
					}
					lblData.Text = sText;
				});				
			}
		}

		protected override void OnClosed(EventArgs e) {
			base.OnClosed(e);
			MBController.Stop();
			_tmrReadData.Stop();
			_client.Stop();
		}
	}
}
