using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENJ.FingerPrint.Entity.ViewObject;

namespace ENJ.FingerPrint.Repository.Interfaces
{
    public interface ILocalCheckInOutRepository
    {
        bool CheckLocalConnection();
        void UpdateLocalDataFingerPrint(LocalCheckInOutViewObject model);
        bool CompareMDBLocalToFPCENTRAL();
        bool InjectCheckInOutToSQL();
        bool CompareUserInfoLocalToFPCENTRAL();
    }
}
