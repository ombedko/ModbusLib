using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBCommandReadCoils : MBCommand {
		public MBCommandReadCoils(string sName) : base(sName) { }
		private bool[] _bCoils = new bool[0];

		public bool[] Coils {
			get { return _bCoils; }
		}

		public void Read() {

			//create data part of this msg
			byte[] pMsgData = new byte[4];
			pMsgData[0] = BitConverter.GetBytes(_nStartAdr)[1];
			pMsgData[1] = BitConverter.GetBytes(_nStartAdr)[0];
			pMsgData[2] = BitConverter.GetBytes(_nQuantity)[1];
			pMsgData[3] = BitConverter.GetBytes(_nQuantity)[0];

			//build command msg	and add to send que			
			_QueMsg(0, MBFunctionType.enReadCoils, pMsgData);
		}



		public override void HandleMsg(MBMsg msg) {
			byte[] pData = msg.Data;
			
			int nBytes = _nQuantity / 8;
			if(_nQuantity % 8 != 0)
				nBytes++;

			if(pData.Length >= nBytes + 1 && pData[0] == nBytes) {  //recieved right amount of data?		
				if((byte)msg.FuncID < 0x80) {//recieved an exception?
					//all is ok, copy data to buffers in Control
					_bCoils = new bool[_nQuantity];
					int nByteAdr = 0;
					int nBitAdr = 0;
					for(int i = 0; i < _nQuantity; i++) {
						nBitAdr = i % 8;
						nByteAdr = i / 8;
						_bCoils[i] = (pData[1 + nByteAdr] & (0x01 << nBitAdr)) == 0 ? false : true;
					}
				}
			}
		}
	}
}
