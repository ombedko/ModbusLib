using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBCommandReadInputRegisters : MBCommand {
		public MBCommandReadInputRegisters(string sName) : base(sName) { }
		private short[] _nRegisters=new short[0];

		public short[] Registers {
			get {
				return _nRegisters;
			}
		}
		
		public void Read() {

			//create data part of this msg
			byte[] pMsgData = new byte[4];
			pMsgData[0] = BitConverter.GetBytes(_nStartAdr)[1];
			pMsgData[1] = BitConverter.GetBytes(_nStartAdr)[0];
			pMsgData[2] = BitConverter.GetBytes(_nQuantity)[1];
			pMsgData[3] = BitConverter.GetBytes(_nQuantity)[0];

			//build command msg	and add to send que			
			_QueMsg(0, MBFunctionType.enReadInputRegisters, pMsgData);
		}



		public override void HandleMsg(MBMsg msg) {
			byte[] pData = msg.Data;			
			//add some checking here, verify lengths ok and match the expected
			if(pData.Length >= 2 * _nQuantity + 1 && pData[0] == 2 * _nQuantity) {    //recieved right amount of data?
				if((byte)msg.FuncID < 0x80) {//recieved an exception?					
					//all is ok, copy data to buffers in Control
					_nRegisters = new short[_nQuantity];
					for(int i = 0; i < _nQuantity; i++) {
						_nRegisters[i] = MBMsg.GetShortFromByteSwappedBuffer(pData, 1 + i * 2);
					}
					_OnDataUpdated();
				}
			}
		}
	}
}
