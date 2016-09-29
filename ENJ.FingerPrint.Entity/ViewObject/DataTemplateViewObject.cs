using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ENJ.FingerPrint.Entity.ViewObject
{

    public class LocalTemplateViewObject
    {
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public string FingerId { get; set; }
    }

    public class RemoteTemplateViewObject
    {
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public string FingerId { get; set; }
    }

    public class NewDataTemplateViewObject
    {
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public string FingerId { get; set; }
    }

    public class LocalUserInfoTempViewObject
    {
        public string UserId { get; set; }
        public string badgeNumber { get; set; }
    }

    public class RemoteUserInfoTempViewObject
    {
        public string UserId { get; set; }
        public string badgeNumber { get; set; }
    }

    public class NewUserInfoTempViewObject
    {
        public string UserId { get; set; }
        public string badgeNumber { get; set; }
    }
    
}
