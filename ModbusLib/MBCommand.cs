using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModBus {
	public class MBCommand {
		public event DataUpdated OnDataUpdated;

		protected int _nStartAdr = 0;
		protected int _nQuantity = 2;
		protected MBController _control;
		protected MBMsg _msgCommand;

		public int StartAdr {
			get { return _nStartAdr; }
			set { _nStartAdr = value; }
		}
		public int Quantity {
			get { return _nQuantity; }
			set { _nQuantity = value; }
		}

		public MBCommand(string sControlName) {
			_control = MBController.GetController(sControlName);		
		}
		/*
		public MBCommand() {
			_control = MBController.GetController("default");
		}
	*/
		public virtual void HandleMsg(MBMsg msg) {			
		}

		protected void _OnDataUpdated() {
			if(OnDataUpdated != null)
				OnDataUpdated();
		}

		protected void _QueMsg(byte nUnitID, MBFunctionType nFuncID, byte[] pMsgData) {
			_control.QueMsg(nUnitID, nFuncID, pMsgData, this);
		}
	}
}
