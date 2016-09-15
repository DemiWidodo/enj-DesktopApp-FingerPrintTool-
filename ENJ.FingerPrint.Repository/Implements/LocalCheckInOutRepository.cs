using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Repository.Interfaces;
using ENJ.FingerPrint.Core.Repository;

namespace ENJ.FingerPrint.Repository.Implements
{
    public class LocalCheckInOutRepository : ILocalCheckInOutRepository
    {
        private LocalCheckInOutViewRepository localCheckInOutViewRepository = new LocalCheckInOutViewRepository();

        public bool CheckLocalConnection()
        {
            return localCheckInOutViewRepository.CheckLocalConnection();
        }
    }
}
