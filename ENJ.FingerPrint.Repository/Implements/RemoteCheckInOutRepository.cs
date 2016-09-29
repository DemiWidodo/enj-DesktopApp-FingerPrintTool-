using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Core.Repository;
using ENJ.FingerPrint.Repository.Interfaces;

namespace ENJ.FingerPrint.Repository.Implements
{
    public class RemoteCheckInOutRepository : IRemoteCheckInOutRepository
    {
        private RemoteCheckInOutViewRepository remoteCheckInOutRepository = new RemoteCheckInOutViewRepository();

        public bool CheckRemoteConnection()
        {
            return remoteCheckInOutRepository.CheckRemoteConnection();
        }

        public bool ProceedInjectFingerPrintData()
        {
            return remoteCheckInOutRepository.ProceedInjectFingerPrintData();
        }

        public bool CompareFPCENTRALToMDLocal()
        {
            return remoteCheckInOutRepository.CompareFPCENTRALToMDLocal();
        }

        public bool CompareFPCentralToUserInfoLocal()
        {
            return remoteCheckInOutRepository.CompareFPCentralToUserInfoLocal();
        }
    }
}
