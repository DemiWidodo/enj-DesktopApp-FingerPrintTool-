using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Repository.Interfaces;
using ENJ.FingerPrint.Core.Repository;
using ENJ.FingerPrint.Entity.ViewObject;

namespace ENJ.FingerPrint.Repository.Implements
{
    public class LocalCheckInOutRepository : ILocalCheckInOutRepository
    {
        private LocalCheckInOutViewRepository localCheckInOutViewRepository = new LocalCheckInOutViewRepository();

        public bool CheckLocalConnection()
        {
            return localCheckInOutViewRepository.CheckLocalConnection();
        }

        public void UpdateLocalDataFingerPrint(LocalCheckInOutViewObject model)
        {
            localCheckInOutViewRepository.UpdateLocalDataFingerPrint(model);
        }

        public bool CompareMDBLocalToFPCENTRAL()
        {
           return localCheckInOutViewRepository.CompareMDBLocalToFPCENTRAL();
        }
    }
}
