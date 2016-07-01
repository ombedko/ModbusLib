using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ModBus;
using SerialCommLib;
using TcpComm;
using System.Threading;

namespace MBSerialTest {
	class Program {
		static SerialComm _serial;
		static TcpClient _tcp;
		static MBController _mb;

		static MBCommandWriteMultipleRegisters _writeHR;
		static MBCommandReadHoldingRegister _readHR;

		static MBCommandReadInputRegisters _readIR;

		static MBCommandReadCoils _readC;
		static MBCommandWriteMultipleCoils _writeC;

		static MBCommandReadDiscreteInput _readDI;

		private static void DebugOut(string s){
			System.Diagnostics.Debug.WriteLine(s);
		}
		private static void PrintData(byte[] data, string sPref = "") {
			string hex = BitConverter.ToString(data);
			hex = hex.Replace("-", " ");
			DebugOut(sPref + hex);
		}
		enum CommType { Serial, TCP };
		static CommType _type = CommType.Serial;

		static void Main(string[] args) {
			//setup comm
			if(_type == CommType.Serial) {
				//setup serial
				_serial = new SerialComm() {
					PortName = "COM9",
					BaudRate = SerialComm.BaudRates.bps_115200,
					Parity = System.IO.Ports.Parity.None,
					DataBits = 8,
					StopBits = System.IO.Ports.StopBits.One,
					MaxMsgLength = 0,
					MaxMsgTime = 60,
					MsgTimeoutAction = SerialCommMsgTimeoutAction.Send,
					Timeout = 1000
				};
				_serial.DataRecieved += DataRecieved;
				_serial.Connected += Connected;
				_serial.Disconnected += Disconnected;
				_serial.Start();			//start serial port
			}

			if(_type == CommType.TCP) {
				//setup TCP
				_tcp = new TcpClient() {
					Ip = "192.168.2.221",
					Port = 502,
					Timeout = 0
				};
				_tcp.DataRecieved += DataRecieved;
				_tcp.Connected += Connected;
				_tcp.Disconnected += Disconnected;
				_tcp.Start();				//start tcp
			}


			//setup modbus
			string sMBControlName = (_type == CommType.Serial) ? "rtu" : "tcp";
			_mb = new MBController(sMBControlName) { Timeout = 10000, IsRTU = (_type == CommType.Serial) };
			_mb.OnSendData += SendDataEvent;
			_mb.OnError += _mb_OnError;
			_mb.Start();				//start the modbus controller

			_writeHR = new MBCommandWriteMultipleRegisters(sMBControlName) { StartAdr = 0, Quantity = 1 };
			_readHR = new MBCommandReadHoldingRegister(sMBControlName) { StartAdr = 0, Quantity = 10 };
			_readIR = new MBCommandReadInputRegisters(sMBControlName) { StartAdr = 50, Quantity = 10 };
			_readC = new MBCommandReadCoils(sMBControlName) { StartAdr = 0, Quantity = 8 };
			_writeC = new MBCommandWriteMultipleCoils(sMBControlName) { StartAdr = 0, Quantity = 8 };
			_readDI = new MBCommandReadDiscreteInput(sMBControlName) { StartAdr = 0, Quantity = 8 };

			_readC.OnDataUpdated += _readC_OnDataUpdated;
			_readDI.OnDataUpdated += _readDI_OnDataUpdated;
			_readHR.OnDataUpdated += _readHR_OnDataUpdated;
			_readIR.OnDataUpdated += _readIR_OnDataUpdated;

			//send some data
			short[] shData = new short[1] { 0 };
			bool[] bCoils = new bool[8] { true, false, true, false, true, false, true, false };
			while(true) {
				if((_type == CommType.Serial && _serial.IsConnected) || _type == CommType.TCP && _tcp.IsConnected) {				
					shData[0]++;

					_writeHR.Write(shData);
					_readHR.Read();
					_readIR.Read();

					for(int i = 0; i < bCoils.Length; i++)
						bCoils[i] = !bCoils[i];

					_writeC.Write(bCoils);
					_readC.Read();					
					_readDI.Read();

					Thread.Sleep((_type == CommType.Serial) ? 900 : 300);

					DebugOut("Bf: " + _mb.BufferLoad.ToString("000.0") + "%");
					
				}
				else
					Thread.Sleep(100);
			}
		}

		

		static void SendDataEvent(byte[] pData) {
			PrintData(pData, "Send: ");
			if(_type == CommType.Serial)
				_serial.SendMsg(pData);
			if(_type == CommType.TCP)
				_tcp.SendMsg(pData);
		}

		static void DataRecieved(byte[] pData) {
			PrintData(pData, "Recv: ");
			_mb.OnDataRecieved(pData);		
		}

		static void _readIR_OnDataUpdated() { DebugOut("IR: " + String.Join(", ", _readIR.Registers)); }
		static void _readHR_OnDataUpdated() { DebugOut("HR: " + String.Join(", ", _readHR.Registers)); }
		static void _readDI_OnDataUpdated() { DebugOut("DI: " + String.Join(", ", _readDI.DiscInputs)); }
		static void _readC_OnDataUpdated() { DebugOut("C : " + String.Join(", ", _readC.Coils)); }
		static void _mb_OnError(string sError) { Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": Error = " + sError); }
		static void Disconnected() { Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": Disconnected"); }
		static void Connected() { Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": Connected"); }
	}
}
