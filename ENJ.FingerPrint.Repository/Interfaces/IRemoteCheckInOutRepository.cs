using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Core;
using ENJ.FingerPrint.Entity;

namespace ENJ.FingerPrint.Repository.Interfaces
{
    public interface IRemoteCheckInOutRepository
    {
        bool CheckRemoteConnection();
        bool ProceedInjectFingerPrintData();
        bool CompareFPCENTRALToMDLocal();
        bool CompareFPCentralToUserInfoLocal();
        bool InjectUserInfoToRemoteSQL();
    }
}
