using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ReversiGUI.Games;
using ReversiGUI.Boards;

namespace ReversiGUI.AI
{
    class Com
    {

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int DllSearch(out double value);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void DllConfigureSearch(int aiColor, int mid, int end, int oneMoveTime, [MarshalAs(UnmanagedType.Bool)]bool useTimer, [MarshalAs(UnmanagedType.Bool)] bool useMPC, [MarshalAs(UnmanagedType.Bool)]bool enablePreSearch);

        [DllImport("Resources\\MonoReversi.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void DllShowMsg();

        private bool IsParamSet=false;

        private int color;

        public Com()
        {
        }

        ~Com()
        {
        }

        public void SetParam(int color, int mid, int end, int win , int shallowDepth, int useHash, int time, bool useTimer, bool useMPC, bool enablePreSearch)
        {
            this.color = color;
            DllConfigureSearch(color, mid, end, time, useTimer, useMPC, enablePreSearch);
            IsParamSet = true;
        }

        public int GetNextMoveAsync(int color, out double value)
        {
            if (IsParamSet)
            {
                return DllSearch(out value);
            }
            else
            {
                value = -2;
                return Define.FAIL;
            }
        }
        
        public void ShowMassage()
        {
            DllShowMsg();
        }
    }
}
