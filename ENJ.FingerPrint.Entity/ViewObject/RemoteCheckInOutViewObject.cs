using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENJ.FingerPrint.Entity.ViewObject
{
    public class RemoteCheckInOutViewObject
    {
        public int UserId { get; set; }
        public DateTime CheckTime { get; set; }
        public string CheckType { get; set; }
        public int VerifyCode { get; set; }
        public string SensorId { get; set; }
        public string MemoInfo { get; set; }
        public int WorkCode { get; set; }
        public string Sn { get; set; }
        public byte UserExtFmt { get; set; }
        public string StaffNo { get; set; }
        public string TrDate { get; set; }
        public string TrTime { get; set; }
    }
}
